using AF.Card;
using AF.Common;
using AF.Communicate;
using AF.Data;
using AF.Data.DB;
using AF.Station;
using Autoframe;
using AutoFrame.PLCExtend;
using Microsoft.VisualBasic.Logging;
using OxyPlot.Axes;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using AutoFrame.DBExtend;
using AutoFrame.SG;
using System.Data;
using System.Resources;
using System.Threading.Tasks;
using AF.File;

namespace Autoframe
{
    public class StationAOIExDown1 : StationEx
    {
        enum GRRMode
        {
            Static,
            Move
        }
        GRRMode grr;
        /// <summary>
        /// 动子ID号
        /// </summary>
        string moverid;
        /// <summary>
        /// 当前穴位号
        /// </summary>
        int curcavity;
        /// <summary>
        /// ccd视觉反馈NG计数
        /// </summary>
        int ccdngcount;
        /// <summary>
        /// 当前穴位CCD拍照次数
        /// </summary>
        int ccdcurrnum = 1;
        /// <summary>
        /// 相机聚焦次数
        /// </summary>
        int focuscount;
        /// <summary>
        /// 相机连接失败次数
        /// </summary>
        int ccdConnectNgCount = 0;
        /// <summary>
        /// 相机GRR运行次数
        /// </summary>
        int grrruncount = 0;
        /// <summary>
        /// 当前工站
        /// </summary>
        string curstation;
        /// <summary>
        /// 前一工站
        /// </summary>
        string frontstation;

        int movIdx = 0;

        string[] sgstadata = new string[Global.slotTotalCount];//S2站测高数据
        double[] d_sgData = new double[Global.slotTotalCount];//S2站测高数据
        string[] frontstadata = new string[Global.slotTotalCount];
        string[] redisdata = new string[Global.trayTotalCol];
        bool[] arr_columnRet = new bool[Global.trayTotalCol];
        string[] arr_NGReason = new string[Global.trayTotalCol];

        string[] arrSN = new string[Global.trayTotalCol];//SN
        double sgcalibheight;
        bool firstinit = true;//第一次初始化
        bool dataenable = true;//是否需要获取前站数据
        bool sharerow;//共享行检测,下相机AOI1与下相机AOI2
        bool sharecol;//共享列检测,下相机AOI1与下相机AOI2
        bool isShareRow;//当前行是否是共享行

        bool grrmode;
        int currentrow = 0;//当前行数
        int currentcol = 0;//当前列数

        int totalrow;//总行数
        int totalcol;//总列数

        int sharerowindx;//共享行索引
        int sharecolindx;//共享列索引

        int startrow;//开始检测行数,从0开始
        int startcol;//开始检测列数,从0开始

        int endcol;//结束检测列数，从1开始
        int endrow;//结束检测行数，从1开始

        int rediscolindex;//数据库列索引

        int iRet;//当前行数
        int redisStartRowCount;//
        bool bIsSnap = false;

        double Ypos = 0;
        double Zpos = 0;

        POINT POINT检测位;

        RegBit BitAOI工站初始化完成;

        AXIS AXIS检测Y轴, AXIS检测Z轴;

        WritePLC W动子已到位, W动子可离开;
        ReadPLC R动子已到位, R动子号;
        DO DO_CCD硬触发复位;
        TcpCommu_Camera_AOI camera;

        TcpCommu_Camera_AOI.Command cmdccd;
        TcpCommu_Camera_AOI.Command resetCmd;
        TcpCommu_Camera_AOI.Command cmdresult;
        TcpCommu_Camera_AOI.Command cmdgrr;

        enum ResultPriority
        {
            NA,
            OK,
            NG
        }

        //ResultPriority resultPriority = ResultPriority.NA;
        //public static event RefreshDataHandle refreshDataEvent;

        //public static event ResetDataHandle resetDataEvent;

        /// <summary>
        /// 返回工站视觉结果
        /// </summary>
        /// <returns></returns>
        public bool CamResult()
        {
            if (this.Name == "下相机AOI1站")
            {
                return camera.CCD2result;

            }
            if (this.Name == "上相机AOI站")
            {
                return camera.CCD3result;
            }
            if (this.Name == "下相机AOI2站")
            {
                return camera.CCD4result;
            }
            return false;
        }

        /// <summary>
        /// 构造函数，需要设置站位当前的IO输入，IO输出，轴方向及轴名称，以显示在手动页面方便操作
        /// </summary>
        /// <param name="strName"></param>
        public StationAOIExDown1(string strName) : base(strName)
        {
            HardwareConfig();
            CheckPoint<POINT>();
            LoadContainer<CONTAINER, ProductData>();
        }

        void HardwareConfig()
        {
            if (this.Name == "下相机AOI1站")
            {
                curstation = "S3";//redis当前工站号
                frontstation = "S1";//redis前一工站号--S1
                movIdx = 2;
                BitAOI工站初始化完成 = RegBit.下相机AOI1站初始化完成;
                AXIS检测Y轴 = AXIS.下AOI1检测Y轴;
                AXIS检测Z轴 = AXIS.下AOI1检测Z轴;

                camera = new TcpCommu_Camera_AOI() { TcpLink = TcpClientMgr.GetClient(1) };
                cmdccd = TcpCommu_Camera_AOI.Command.CCD2下AOI检测拍照;
                cmdresult = TcpCommu_Camera_AOI.Command.CCD2获取拍照结果;
                resetCmd = TcpCommu_Camera_AOI.Command.CCD2Reset;
                focuscount = Parameter.GetBit("Enable_ReverseSide") ? 4 : 6;//三个聚焦面

                sharerow = Global.trayTotalRow % 2 == 0 ? false : true;//是否奇数行共同检测
                sharecol = Global.trayTotalCol % 2 == 0 ? false : true;//是否奇数列共同检测

                startrow = 0; //开始行索引
                startcol = sharerow ? (Global.trayTotalCol + 1) / 2 : 0;//开始列索引,下标从0开始

                endrow = (Global.trayTotalRow + 1) / 2;//检测行数,从1开始
                endcol = sharerow ? (Global.trayTotalCol + 1) / 2 : Global.trayTotalCol;//检测列数,从1开始

                redisStartRowCount = Global.trayTotalRow / 2;//数据库起始行

                sharerowindx = Global.trayTotalRow / 2;//检测行数,从0开始
                sgcalibheight = Parameter.GetDouble("DownAOI1SGCalibHeight");//深视标定高度

                R动子已到位 = ReadPLC.缺陷检测工位1到位信号位;
                R动子号 = ReadPLC.缺陷检测工位1工位动子号;

                W动子已到位 = WritePLC.缺陷检测工位1到位信号位;

                W动子可离开 = WritePLC.缺陷检测工位1完成信号位;
                //W步距 = WritePLC.缺陷检测工位1步长;
                //W行数 = WritePLC.缺陷检测工位1行数;
                DO_CCD硬触发复位 = DO.CCD2硬触发复位;
            }
            if (this.Name == "上相机AOI站")
            {
                curstation = "S4";//redis当前工站号
                frontstation = "S1";//redis前一工站号--S1
                movIdx = 3;

                BitAOI工站初始化完成 = RegBit.上相机AOI站初始化完成;

                AXIS检测Y轴 = AXIS.上AOI检测Y轴;
                AXIS检测Z轴 = AXIS.上AOI检测Z轴;

                camera = new TcpCommu_Camera_AOI() { TcpLink = TcpClientMgr.GetClient(2) };
                cmdccd = TcpCommu_Camera_AOI.Command.CCD3上AOI检测拍照;
                cmdresult = TcpCommu_Camera_AOI.Command.CCD3获取拍照结果;
                resetCmd = TcpCommu_Camera_AOI.Command.CCD3Reset;
                focuscount = Parameter.GetBit("Enable_ReverseSide") ? 6 : 4;//三个聚焦面

                //startindex = 0;//开始索引,数据库索引
                startrow = 0;//下标从0开始
                startcol = 0;//下标从0开始

                endrow = Global.trayTotalRow;//检测列数,从1开始
                endcol = Global.trayTotalCol;//检测列数,从1开始

                redisStartRowCount = 0;//数据库起始行

                sharerow = false;//是否奇数行共同检测
                sharecol = false;//是否奇数列共同检测

                sharerowindx = 0;//检测行数,从0开始
                sharecolindx = 0;//检测行数,从0开始

                sgcalibheight = Parameter.GetDouble("UpAOISGCalibHeight");//SG标定高度

                R动子已到位 = ReadPLC.缺陷检测工位2到位信号位;
                R动子号 = ReadPLC.缺陷检测工位2工位动子号;

                W动子已到位 = WritePLC.缺陷检测工位2到位信号位;

                W动子可离开 = WritePLC.缺陷检测工位2完成信号位;
                //W步距 = WritePLC.缺陷检测工位2步长;
                //W行数 = WritePLC.缺陷检测工位2行数;
                DO_CCD硬触发复位 = DO.CCD3硬触发复位;

            }
            if (this.Name == "下相机AOI2站")
            {

                curstation = "S5";//redis当前工站号
                frontstation = "S1";//redis前一工站号--S1
                movIdx = 4;

                BitAOI工站初始化完成 = RegBit.下相机AOI2站初始化完成;

                AXIS检测Y轴 = AXIS.下AOI2检测Y轴;
                AXIS检测Z轴 = AXIS.下AOI2检测Z轴;

                camera = new TcpCommu_Camera_AOI() { TcpLink = TcpClientMgr.GetClient(3) };
                cmdccd = TcpCommu_Camera_AOI.Command.CCD4下AOI检测拍照;
                cmdresult = TcpCommu_Camera_AOI.Command.CCD4获取拍照结果;
                resetCmd = TcpCommu_Camera_AOI.Command.CCD4Reset;
                focuscount = Parameter.GetBit("Enable_ReverseSide") ? 4 : 6;//三个聚焦面
                sharerow = Global.trayTotalRow % 2 == 0 ? false : true;//是否奇数行共同检测
                startrow = 0; //开始行索引
                startcol = 0; //开始列索引
                endrow = (Global.trayTotalRow + 1) / 2;//检测行数,从1开始
                endcol = sharerow ? (Global.trayTotalCol + 1) / 2 : Global.trayTotalCol;//检测列数,从1开始
                redisStartRowCount = 0;//数据库起始行
                sharerowindx = Global.trayTotalRow / 2;//检测行数,从0开始
                sharecolindx = (Global.trayTotalCol + 1) / 2 - 1;//检测列数,从0开始
                sgcalibheight = Parameter.GetDouble("DownAOI2SGCalibHeight");//深视标定高度
                R动子已到位 = ReadPLC.缺陷检测工位3到位信号位;
                R动子号 = ReadPLC.缺陷检测工位3工位动子号;

                W动子已到位 = WritePLC.缺陷检测工位3到位信号位;
                W动子可离开 = WritePLC.缺陷检测工位3完成信号位;
                DO_CCD硬触发复位 = DO.CCD4硬触发复位;

            }

            //配置站位界面显示输入
            arrayDI = new object[] { };
            //配置站位界面显示输出
            arrayDO = new object[] { };
            //配置站位界面显示气缸
            arrayCylinder = new object[] { };
            //配置站位轴
            arrayAxis = new object[] { AXIS检测Y轴, AXIS检测Z轴 };

        }

        ProductData productData = new ProductData();


        /// <summary>
        /// 站位内的点位枚举
        /// </summary>
        public enum POINT
        {
            安全位,
            穴位1检测聚焦位1,//面对显示器，右上角
            穴位1检测聚焦位2,//面对显示器，右上角
            穴位1检测聚焦位3,//面对显示器，右上角
            GRR测试拍照位
        }

        /// <summary>
        /// 产品穴位
        /// </summary>
        public enum CONTAINER
        {
            穴位1,
            穴位2,
        }

        public override void OnStop()
        {
            //base.OnStop() 不能删，且需要放在第一行。
            base.OnStop();
            AXIS检测Y轴.ServoOn(false);
            AXIS检测Z轴.ServoOn(false);
            


        }
        public override void OnPause()
        {
            //base.OnPause() 不能删，且需要放在第一行。
            base.OnPause();
        }
        public override void OnResume()
        {
            base.OnResume(); //不能删，且需要放在第一行。
            //return base.OnResume();
        }

        /// <summary>
        /// 初始化时设置站位安全状态，用来设备初始化时各站位之间相互卡站，
        /// 控制各站位先后动作顺序用，比如流水线组装，肯定需要组装的Z轴升
        /// 起后，流水线才能动作这种情况
        /// </summary>
        public override void Preinit()
        {
            BitAOI工站初始化完成.Set(false); // Bit: Reset cờ hoàn tất khởi tạo trạm AOI
            switch (Process)
            {
                case AutoProcess.正常生产:
                case AutoProcess.空跑:
                    grrmode = false;
                    break;
                case AutoProcess.CCD2相机静态:
                    if (this.Name == "下相机AOI1站")
                    {
                        grrmode = true;
                        grr = GRRMode.Static;
                        cmdgrr = TcpCommu_Camera_AOI.Command.CCD2静态;
                    }
                    else
                    {
                        grrmode = false;
                        this.InitEntry = StepExit;
                        this.AutoEntry = StepEmpty;
                    }
                    break;
                case AutoProcess.CCD2相机动态:
                    if (this.Name == "下相机AOI1站")
                    {
                        grrmode = true;
                        grr = GRRMode.Move;
                        cmdgrr = TcpCommu_Camera_AOI.Command.CCD2动态;
                    }
                    else
                    {
                        grrmode = false;
                        this.InitEntry = StepExit;
                        this.AutoEntry = StepEmpty;
                    }
                    break;
                case AutoProcess.CCD3相机静态:
                    if (this.Name == "上相机AOI站")
                    {
                        grrmode = true;
                        grr = GRRMode.Static;
                        cmdgrr = TcpCommu_Camera_AOI.Command.CCD3静态;
                    }
                    else
                    {
                        grrmode = false;
                        this.InitEntry = StepExit;
                        this.AutoEntry = StepEmpty;
                    }
                    break;
                case AutoProcess.CCD3相机动态:
                    if (this.Name == "上相机AOI站")
                    {
                        grrmode = true;
                        grr = GRRMode.Move;
                        cmdgrr = TcpCommu_Camera_AOI.Command.CCD3动态;
                    }
                    else
                    {
                        grrmode = false;
                        this.InitEntry = StepExit;
                        this.AutoEntry = StepEmpty;
                    }
                    break;
                case AutoProcess.CCD4相机静态:
                    if (this.Name == "下相机AOI2站")
                    {
                        grrmode = true;
                        grr = GRRMode.Static;
                        cmdgrr = TcpCommu_Camera_AOI.Command.CCD4静态;
                    }
                    else
                    {
                        grrmode = false;
                        this.InitEntry = StepExit;
                        this.AutoEntry = StepEmpty;
                    }
                    break;
                case AutoProcess.CCD4相机动态:
                    if (this.Name == "下相机AOI2站")
                    {
                        grrmode = true;
                        grr = GRRMode.Move;
                        cmdgrr = TcpCommu_Camera_AOI.Command.CCD4动态;
                    }
                    else
                    {
                        grrmode = false;
                        this.InitEntry = StepExit;
                        this.AutoEntry = StepEmpty;
                    }
                    break;

                default:
                    this.InitEntry = StepExit;
                    this.AutoEntry = StepEmpty;
                    break;
            }
        }

        protected override string InitRun(string strStep)
        {
            switch (strStep)
            {
                case StepEntry: // [Khởi tạo] Bắt đầu khởi tạo trạm AOI (Down AOI 1 / Up AOI / Down AOI 2)
                    return "相机连接"; // Chuyển sang: Kết nối camera AOI
                case "相机连接": // [Khởi tạo] Kết nối camera AOI qua giao thức TCP/IP
                    if (Parameter.GetBit("VisionEnable")) // Nếu bật chức năng thị giác
                    {
                        if (camera.Init(this)) // Khởi tạo kết nối camera thành công
                        {
                            ccdConnectNgCount = 0;
                            Logger.Process(Name, "相机连接成功"); // Log: Kết nối camera thành công
                            return "相机复位"; // Chuyển sang: Reset camera
                        }
                        else
                        {
                            return "相机连接失败"; // Chuyển sang: Xử lý lỗi kết nối
                        }
                    }
                    else
                    {
                        ccdConnectNgCount = 0;
                        Logger.Process(Name, "未启用视觉"); // Log: Chưa bật chức năng thị giác
                        return "相机复位"; // Chuyển sang: Reset camera
                    }
                case "相机连接失败": // [Khởi tạo] Xử lý lỗi kết nối camera quá 3 lần
                    ccdConnectNgCount++;
                    if (ccdConnectNgCount >= 3)
                    {
                        Logger.Process(Name, "相机连接失败"); // Log: Kết nối thất bại
                        ShowMessage($"{this.Name}相机连接失败", null, new string[] { "再次连接" }); // Hiển thị cảnh báo lỗi kết nối camera
                        ccdConnectNgCount = 0;
                        return "相机连接"; // Thử kết nối lại
                    }
                    else
                    {
                        return "相机连接"; // Thử kết nối lại
                    }
                case "相机复位": // [Khởi tạo] Gửi lệnh reset sang phần mềm thị giác AOI
                    if (Parameter.GetBit("VisionEnable"))
                    {
                        string strRef = "";
                        if (!camera.Communicate(resetCmd, ref strRef, null)) // Gửi lệnh reset
                        {
                            ShowMessage($"{this.Name}相机复位失败", null, new string[] { "再次连接" }); // Cảnh báo reset thất bại
                            return "相机复位"; // Thử reset lại
                        }
                        else
                        {
                            return "轴上使能"; // Chuyển sang: Bật servo các trục
                        }
                    }
                    else
                    {
                        return "轴上使能"; // Chuyển sang: Bật servo các trục
                    }
                case "轴上使能": // [Khởi tạo] Bật servo trục Y và Z của cụm camera AOI
                    AXIS检测Y轴.ServoOn(true); // Bật Servo trục Y quét ảnh
                    AXIS检测Z轴.ServoOn(true); // Bật Servo trục Z lấy nét
                    Delay(500); // Trì hoãn 500ms
                    return "Z轴回安全位"; // Chuyển sang: Trục Z về an toàn

                case "Z轴回安全位": // [Khởi tạo] Trục Z nâng về vị trí an toàn tránh va chạm
                    AxisSingleTo(AXIS检测Z轴, POINT.安全位); // Trục Z về an toàn
                    WaitAxis(AXIS检测Z轴); // Chờ trục Z dừng
                    return "ZY轴回初始检测位"; // Chuyển sang: Trục ZY về vị trí kiểm tra ban đầu

                case "ZY轴回初始检测位": // [Khởi tạo] Trục Y và Z chạy về vị trí tiêu cự ban đầu (khoang 1 tiêu cự 1)
                    AxisSingleTo(AXIS检测Y轴, POINT.穴位1检测聚焦位1); // Trục Y về vị trí tiêu cự 1
                    WaitAxis(AXIS检测Y轴);
                    AxisSingleTo(AXIS检测Z轴, POINT.穴位1检测聚焦位1); // Trục Z về vị trí tiêu cự 1
                    WaitAxis(AXIS检测Z轴);
                    return "初始化完成"; // Chuyển sang: Hoàn tất khởi tạo

                case "初始化完成": // [Khởi tạo] Reset cờ đếm, xung trigger cứng 2 lần và bật bit hoàn tất khởi tạo trạm AOI
                    firstinit = true;
                    dataenable = true;
                    ccdngcount = 0;
                    ccdcurrnum = 1;
                    camera.Camtrigger(cmdccd, false); // Tắt tín hiệu trigger camera
                    //S1:入料接驳站，S2:激光位移检测站，S3:下相机AOI1站，S4:上相机AOI站，S5:下相机AOI2站，S6:出料接驳站，
                    DO_CCD硬触发复位.Set(true); // DO: Xung reset trigger cứng lần 1
                    Delay(500);
                    DO_CCD硬触发复位.Set(false);

                    DO_CCD硬触发复位.Set(true); // DO: Xung reset trigger cứng lần 2
                    Delay(500);
                    DO_CCD硬触发复位.Set(false);
                    BitAOI工站初始化完成.Set(true); // Bit: Đánh dấu trạm AOI đã khởi tạo xong
                    return StepExit; // Thoát chu trình khởi tạo
                default:
                    return null;
            }
        }

        Stopwatch swVision = null;

        protected override string ProduceRun(string strStep)
        {
            switch (strStep)
            {
                case StepEntry: // [Sản xuất] Bắt đầu chu trình tự động kiểm tra AOI
                    return "生产流程开始"; // Chuyển sang: Khởi động quy trình sản xuất
                case "生产流程开始": // [Sản xuất] Chuyển tiếp vào bước chờ Mover
                    return "等待动子到位"; // Chuyển sang: Chờ Mover vào vị trí
                case "等待动子到位": // [Sản xuất] Đọc tín hiệu PLC hàng hiện tại của Mover đã vào vị trí kiểm tra
                    iRet = Global.m_plcHelper.ReadData_Int16(0, new int[] { (int)R动子已到位 }); // Đọc số hàng Mover đã đến
                    if (iRet != 0)
                    {
                        Global.m_plcHelper.WriteData_Int16(0, new int[] { ((int)W动子已到位) }, 0); // PLC: Reset cờ到位
                        Logger.Process(this.Name, $"动子当前行:{iRet}"); // Log: Hàng hiện tại của Mover
                        return "获取动子号"; // Chuyển sang: Đọc mã số Mover
                    }
                    else
                    {
                        return "等待动子到位"; // Tiếp tục chờ Mover vào vị trí
                    }
                case "获取动子号": // [Sản xuất] Đọc ID của Mover từ PLC, lưu bộ nhớ và cập nhật giám sát In
                    moverid = Global.m_plcHelper.ReadData_Int16(0, new int[] { (int)R动子号 }).ToString(); // Đọc ID Mover
                    Global.arr_moveID[movIdx] = moverid; // Gán ID Mover vào vị trí mảng toàn cục
                    Logger.Process(this.Name, $"动子已到位-动子号:{moverid}"); // Log: Mover đã vào vị trí

                    MonitorCheckMover("In", moverid); // Giám sát Mover vào trạm

                    return "检查数据库当前动子是否存在"; // Chuyển sang: Kiểm tra Mover trong CSDL
                case "检查数据库当前动子是否存在": // [Sản xuất] Kiểm tra key Mover có tồn tại trong Redis không
                    if (RedisHelperEx.Instance.Keycheck("0", moverid.ToString()))
                    {
                        return "获取前站与本站数据"; // Chuyển sang: Đọc dữ liệu các trạm trước
                    }
                    else
                    {
                        return "通知磁悬浮可离开"; // Mover lỗi/rỗng, chuyển sang: Cho phép Mover rời trạm
                    }
                case "获取前站与本站数据": // [Sản xuất] Đọc dữ liệu độ cao Laser SG S2 và kết quả có phôi trạm trước S1 từ Redis
                    for (int i = 0; i < Global.slotTotalCount; i++)
                    {
                        sgstadata[i] = RedisHelperEx.Instance.HashGet(moverid.ToString(), $"S2_{i + 1}"); // Dữ liệu độ cao Laser S2
                        frontstadata[i] = RedisHelperEx.Instance.HashGet(moverid.ToString(), $"{frontstation}_{i + 1}"); // Kết quả trạm S1
                        if (!double.TryParse(sgstadata[i], out d_sgData[i]))
                        {
                            d_sgData[i] = 0; // Mặc định 0 nếu không parse được
                        }
                    }
                    return "本站数据处理"; // Chuyển sang: Xử lý số hàng
                case "本站数据处理": // [Sản xuất] Lần đầu khởi tạo gán chỉ số hàng xử lý hiện tại bằng số hàng đọc từ PLC - 1
                    if (firstinit)
                    {
                        firstinit = false;
                        currentrow = iRet - 1; // Chỉ số hàng hiện tại (0-indexed)
                    }
                    return "当前动子行数判断"; // Chuyển sang: So khớp hàng Mover

                case "当前动子行数判断": // [Sản xuất] So khớp số hàng báo từ PLC với số hàng trạm đang cần kiểm tra
                    Logger.Process(this.Name, $"动子当前行:{iRet}-工站检测行{currentrow + 1}");
                    if (iRet == currentrow + 1) // Trùng khớp số hàng
                    {
                        return "当前行数是否奇数行共用"; // Chuyển sang: Kiểm tra hàng dùng chung
                    }
                    else // Lệch số hàng
                    {
                        ShowMessage($"{this.Name}动子行:{iRet}与工站检测行:{currentrow + 1}不匹配,请检查", "行数失败", new string[] { "再次判断" });
                        return "等待动子到位";
                    }

                case "当前行数是否奇数行共用": // [Sản xuất] Tính toán giới hạn hàng và cột dùng chung giữa AOI dưới 1 và AOI dưới 2
                    if (sharerow) // Nếu khay có số hàng lẻ
                    {
                        isShareRow = (currentrow + redisStartRowCount) == sharerowindx;
                        if (isShareRow) // Hàng hiện tại là hàng dùng chung
                        {
                            totalrow = endrow; // Cập nhật tổng số hàng cần quét
                            totalcol = endcol; // Cập nhật tổng số cột cần quét
                            if (this.Name == "下相机AOI1站")
                            {
                                if (sharecol)
                                {
                                    totalcol = Global.trayTotalCol;
                                }
                            }
                            rediscolindex = startcol; // Cột bắt đầu ghi Redis
                            currentcol = startcol; // Cột bắt đầu quét
                        }
                        else
                        {
                            totalrow = endrow;
                            totalcol = Global.trayTotalCol;
                            rediscolindex = 0;
                            currentcol = 0;
                        }
                    }
                    else
                    {
                        totalrow = endrow;
                        totalcol = Global.trayTotalCol;
                        rediscolindex = 0;
                        currentcol = 0;
                    }

                    return "当前穴位判断"; // Chuyển sang: Kiểm tra điều kiện ô sản phẩm
                case "当前穴位判断": // [Sản xuất] Kiểm tra ô sản phẩm: kiểm tra độ cao Laser SG, nếu quá giới hạn báo lỗi hoặc gán NG
                    if (currentcol <= (totalcol - 1)) // Chưa hết các cột trong hàng
                    {
                        curcavity = currentcol + (currentrow + redisStartRowCount) * Global.trayTotalCol; // Tính chỉ số ô toàn khay (0-indexed)
                        if (frontstadata[curcavity] == "OK" || frontstadata[curcavity] == "NG") // Khay có phôi (kiểm tra toàn bộ OK/NG)
                        {
                            if (d_sgData[curcavity] > 5 || d_sgData[curcavity] <= 0) // Độ cao đo được từ laser SG bất thường
                            {
                                if (true/*0 == ShowMessage($"穴位{curcavity + 1}激光值超限:{d_sgData[curcavity]}，请检查", "警告", new string[] {"跳过" , "获取系统"})*/)
                                {
                                    redisdata[currentcol] = frontstadata[curcavity] == "NA" ? "NA" : "NG"; // Gán NG cho ô lỗi độ cao
                                    currentcol++; // Chuyển sang cột tiếp
                                    return "当前穴位判断";
                                }
                                else
                                {
                                    d_sgData[curcavity] = sgcalibheight;
                                    return "Y轴到检测位";
                                }
                            }
                            return "Y轴到检测位"; // Chuyển sang: Trục Y di chuyển đến vị trí kiểm tra
                        }
                        else // Ô rỗng không có phôi (NA)
                        {
                            redisdata[currentcol] = frontstadata[curcavity] == "NA" ? "NA" : "NG";
                            currentcol++; // Bỏ qua chụp, chuyển sang cột tiếp
                            return "当前穴位判断";
                        }

                    }
                    else
                    {
                        bIsSnap = false;
                        return "当前列数判断"; // Đã hết các cột trong hàng
                    }

                case "Y轴到检测位": // [Sản xuất] Trục Y di chuyển đến tọa độ cột của ô hiện tại theo khoảng cách cột ColGap

                    Ypos = POINT.穴位1检测聚焦位1.GetPos(AXIS检测Y轴, this) - Math.Abs(Global.trayColGap) * currentcol; // Tọa độ Y = Vị trí gốc - Khoảng cách cột * Cột hiện tại
                    AXIS检测Y轴.AbsMove(Ypos, 100); // Trục Y chạy tuyệt đối tốc độ 100
                    WaitAxis(AXIS检测Y轴); // Chờ trục Y dừng
                    swVision = Stopwatch.StartNew(); // Bắt đầu tính thời gian thị giác
                    return "Z轴到检测位"; // Chuyển sang: Trục Z lấy nét


                case "Z轴到检测位": // [Sản xuất] Trục Z di chuyển đến vị trí lấy nét tương ứng (bù trừ theo độ cao Laser SG)
                    switch (ccdcurrnum)
                    {
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                            POINT检测位 = POINT.穴位1检测聚焦位1; // Điểm lấy nét 1
                            break;
                        case 5:
                            POINT检测位 = POINT.穴位1检测聚焦位2; // Điểm lấy nét 2
                            break;
                        case 6:
                            POINT检测位 = POINT.穴位1检测聚焦位3; // Điểm lấy nét 3
                            break;
                        default:
                            POINT检测位 = POINT.穴位1检测聚焦位1;
                            break;
                    }
                    if (!Parameter.GetBit("Enable_ReverseSide")) // Mặt thuận
                    {                     
                        if (this.Name == "上相机AOI站")
                        {
                            switch (ccdcurrnum)
                            {
                                case 1:
                                case 2:
                                case 3:
                                    POINT检测位 = POINT.穴位1检测聚焦位1;
                                    break;
                                case 4:
                                    POINT检测位 = POINT.穴位1检测聚焦位2;
                                    break;
                                default:
                                    POINT检测位 = POINT.穴位1检测聚焦位1;
                                    break;
                            }
                        }
                    }
                    else // Mặt đảo ngược (ReverseSide)
                    {                       
                        if (this.Name.Contains("下相机AOI站"))
                        {
                            switch (ccdcurrnum)
                            {
                                case 1:
                                case 2:
                                case 3:
                                    POINT检测位 = POINT.穴位1检测聚焦位1;
                                    break;
                                case 4:
                                    POINT检测位 = POINT.穴位1检测聚焦位2;
                                    break;
                                default:
                                    POINT检测位 = POINT.穴位1检测聚焦位1;
                                    break;
                            }
                        }
                    }


                    if (Parameter.GetBit("LaserEnable")) // Nếu bật bù trừ độ cao Laser
                    {
                        if (this.Name == "上相机AOI站")
                        {
                            Zpos = POINT检测位.GetPos(AXIS检测Z轴, this) - d_sgData[curcavity] + sgcalibheight; // Bù trừ Z cho trạm AOI trên
                            Logger.Process(this.Name, $"Snap{ccdcurrnum} Z pos：{POINT检测位.GetPos(AXIS检测Z轴, this)} - {d_sgData[curcavity]} + {sgcalibheight} = {Zpos}");
                        }
                        else
                        {
                            Zpos = POINT检测位.GetPos(AXIS检测Z轴, this) + d_sgData[curcavity] - sgcalibheight; // Bù trừ Z cho trạm AOI dưới
                            Logger.Process(this.Name, $"Snap{ccdcurrnum} Z pos：{POINT检测位.GetPos(AXIS检测Z轴, this)} + {d_sgData[curcavity]} - {sgcalibheight} = {Zpos}");
                        }

                      
                    }
                    else
                    {
                        Zpos = POINT检测位.GetPos(AXIS检测Z轴, this); // Không bù trừ
                    }
                    AXIS检测Z轴.AbsMove(Zpos); // Trục Z chạy đến cao độ lấy nét
                    WaitAxis(AXIS检测Z轴); // Chờ trục Z dừng
                    return "通知视觉拍照"; // Chuyển sang: Báo thị giác chụp ảnh

                case "通知视觉拍照": // [Sản xuất] Gửi lệnh chụp (SN, Slot) và phát xung Trigger phần cứng tới camera
                    if (ccdcurrnum == 1) // Ở lần chụp lấy nét đầu tiên của ô hiện tại
                    {
                        int iTraySeparate = startcol + (Global.trayTotalRow / 2) * Global.trayTotalCol;
                        if (this.Name == "下相机AOI1站")
                        {
                            if (curcavity >= iTraySeparate)
                            {
                                arrSN[currentcol] = $"{curcavity + 1}_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}"; // Tạo mã SN theo thời gian
                            }
                        }
                        else if (this.Name == "上相机AOI站")
                        {
                            if (Global.slotTotalCount % 2 == 0) // Tổng số chẵn
                            {
                                if (curcavity < Global.slotTotalCount / 2)
                                {
                                    arrSN[currentcol] = $"{curcavity + 1}_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}";
                                }
                                else
                                {
                                    arrSN[currentcol] = RedisHelperEx.Instance.HashGet($"{moverid}_Data", $"S3_{curcavity + 1}_SN");
                                }
                            }
                            else // Tổng số lẻ
                            {
                                if (curcavity <= Global.slotTotalCount / 2)
                                {
                                    arrSN[currentcol] = $"{curcavity + 1}_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}";
                                }
                                else
                                {
                                    arrSN[currentcol] = RedisHelperEx.Instance.HashGet($"{moverid}_Data", $"S3_{curcavity + 1}_SN");
                                }
                            }
                          
                        }
                        else if (this.Name == "下相机AOI2站")
                        {
                            arrSN[currentcol] = RedisHelperEx.Instance.HashGet($"{moverid}_Data", $"S4_{curcavity + 1}_SN"); // Lấy lại SN từ trạm S4
                        }
                        if (Parameter.GetBit("VisionEnable")) // Nếu bật thị giác
                        {
                            camera.SendCommand(cmdccd, 0, $"SN={arrSN[currentcol]}", $"Slot={curcavity + 1}"); // Gửi lệnh kèm SN và số Slot
                        }
                           
                    }
                    if (Parameter.GetBit("VisionEnable")) // Phát xung trigger phần cứng camera
                    {                      
                        camera.Camtrigger(cmdccd, true); // Bật xung Trigger
                        Delay(20); // Độ rộng xung 20ms
                        camera.Camtrigger(cmdccd, false); // Tắt xung Trigger
                        Logger.Process(this.Name, $"触发第{ccdcurrnum}次"); // Log: Đã phát xung trigger lần thứ n
                    }
                    return "当前穴位拍照次数判断"; // Chuyển sang: Đánh giá số lần chụp


                case "当前穴位拍照次数判断": // [Sản xuất] Kiểm tra xem đã chụp đủ số tiêu cự (focuscount) của ô sản phẩm chưa
                    if (ccdcurrnum >= focuscount) // Đã chụp đủ các mặt lấy nét
                    {
                        ccdcurrnum = 1; // Reset biến đếm lần chụp
                        bIsSnap = true; // Đánh dấu đã chụp xong ô này
                        return "当前列数判断"; // Chuyển sang: Đọc kết quả và chuyển cột
                    }
                    else // Chưa đủ các mặt lấy nét
                    {
                        ccdcurrnum++; // Tăng bước tiêu cự
                        return "Z轴到检测位"; // Đổi độ cao Z lấy nét mặt tiếp theo
                    }

                case "当前列数判断": // [Sản xuất] Chờ nhận kết quả từ camera (WaitAndAnalysCamData), lưu kết quả và di chuyển trước trục Y
                    string strNextStep;
                    if (currentcol >= (totalcol - 1)) // Đã là cột cuối cùng của hàng
                    {
                        Ypos = POINT.穴位1检测聚焦位1.GetPos(AXIS检测Y轴, this) - Math.Abs(Global.trayColGap) * 0; // Trục Y chuẩn bị quay về cột 0
                        strNextStep = "更新数据库"; // Chuyển sang: Cập nhật CSDL
                    }
                    else // Vẫn còn cột tiếp theo
                    {
                        Ypos = POINT.穴位1检测聚焦位1.GetPos(AXIS检测Y轴, this) - Math.Abs(Global.trayColGap) * (currentcol + 1); // Trục Y đón trước cột kế tiếp
                        strNextStep = "当前穴位判断"; // Chuyển sang: Kiểm tra ô kế tiếp
                    }
                    AXIS检测Y轴.AbsMove(Ypos, 100); // Trục Y dịch chuyển đón trước trong lúc đợi xử lý ảnh
                    if (bIsSnap)
                    {
                        DateTime startTime = DateTime.Now;
                        if (WaitAndAnalysCamData(cmdccd, camera, out bool bRet, out string strNGReason)) // Chờ nhận kết quả từ thị giác
                        {
                            Logger.Process(this.Name, $"Waiting reply time: {DateTime.Now.Subtract(startTime).TotalMilliseconds}ms");
                            ccdngcount = 0;
                            redisdata[currentcol] = bRet ? "OK" : "NG"; // Ghi nhận OK/NG
                            arr_NGReason[currentcol] = strNGReason; // Ghi nhận chuỗi lý do NG
                            if (currentcol < (totalcol - 1))
                                currentcol++;
                        }
                        //else
                        //{
                        //    ccdngcount++;
                        //    if (ShowMessage($"{this.Name}相机连续{ccdngcount}次拍照失败,请检查", "拍照失败", new string[] { "重新拍照" }) == 0)
                        //        ccdngcount = 0;
                        //    strNextStep = "Y轴到检测位";
                        //}

                        swVision.Stop(); // Dừng đo thời gian thị giác
                        if (Parameter.GetBit("VisionEnable"))
                        {
                            Task.Factory.StartNew(() =>
                            {
                                string content = curcavity.ToString() + "," + swVision.ElapsedMilliseconds.ToString();
                                CsvWriter.WriteLine("D:\\exe\\ProductData\\" + DateTime.Now.ToString("yyyy_MM_dd") + "\\" + this.Name + ".csv", content); // Lưu log CT vào file CSV
                            });
                        }
                    }
                    WaitAxis(AXIS检测Y轴); // Chờ trục Y dừng
                    return strNextStep;


                case "更新数据库": // [Sản xuất] Ghi toàn bộ kết quả kiểm tra của hàng hiện tại (S3/S4/S5, NGReason, SN) vào Redis
                    int numrow = (currentrow + redisStartRowCount);
                    for (int i = rediscolindex; i < totalcol; i++) // Vòng lặp cập nhật từng ô trong hàng
                    {
                        //待添加数据库判断
                        int index = i + 1 + Global.trayTotalCol * numrow;
                        //refreshDataEvent?.Invoke(index, redisdata[i]);

                        RedisHelperEx.Instance.HashSet(moverid, $"{curstation}_{index}", redisdata[i]); // Ghi kết quả OK/NG vào Redis
                        RedisHelperEx.Instance.HashSet($"{moverid}_Data", $"{curstation}_{index}_NGReason", arr_NGReason[i]); // Ghi lý do NG
                        RedisHelperEx.Instance.HashSet($"{moverid}_Data", $"{curstation}_{index}_SN", arrSN[i]); // Ghi mã SN


                        //if(this.Name == "下相机AOI1站")
                        //{
                        //    Console.WriteLine($"动子号{moverid}:"+ $"{curstation}_{index}_SN", arrSN[i]);
                        //}
                        //else
                        //if(this.Name == "上相机AOI站")
                        //{
                        //    Console.WriteLine($"动子号{moverid}:" + $"{curstation}_{index}_SN", arrSN[i]);
                        //}
                    }
                    return "是否全部检测完成判断"; // Chuyển sang: Kiểm tra hoàn thành toàn khay
                case "是否全部检测完成判断": // [Sản xuất] Kiểm tra xem đã quét hết tất cả các hàng của khay chưa
                    arr_NGReason = new string[Global.trayTotalCol]; // Xóa mảng lý do lỗi hàng cũ
                    if (currentrow >= (totalrow - 1)) // Đã hoàn thành hàng cuối cùng
                    {
                        return "通知磁悬浮可离开"; // Chuyển sang: Báo Mover rời trạm
                    }
                    else // Vẫn còn hàng tiếp theo
                    {
                        return "通知磁悬浮相对移动"; // Chuyển sang: Báo Mover bước sang hàng kế tiếp
                    }

                case "通知磁悬浮相对移动": // [Sản xuất] Ghi PLC tín hiệu số hàng hoàn thành để Mover từ tính bước sang hàng kế tiếp
                    Logger.Process(this.Name, $"动子号{moverid}-PLC发行数{iRet}-相对移动-当前行：{currentrow + 1}");
                    //Global.m_plcHelper.WriteData_Int16(0, new int[] { ((int)W动子已到位) }, 0);
                    Global.m_plcHelper.WriteData_Int16(0, new int[] { ((int)W动子可离开) }, currentrow + 1); // PLC: Hoàn thành hàng (currentrow + 1)
                    currentrow++; // Tăng chỉ số hàng
                    return "Y轴到起始检测位"; // Chuyển sang: Trục Y về vị trí bắt đầu chờ hàng mới

                case "通知磁悬浮可离开": // [Sản xuất] Ghi PLC giá trị 8 báo Mover từ tính đã hoàn thành toàn bộ và rời trạm đi tiếp
                    dataenable = true;
                    //Global.m_plcHelper.WriteData_Int16(0, new int[] { ((int)W动子已到位) }, 0);
                    Global.m_plcHelper.WriteData_Int16(0, new int[] { ((int)W动子可离开) }, 8); // PLC: Hoàn thành toàn trạm (8 = Mover rời trạm)
                    Logger.Process(this.Name, $"动子号:{moverid} - 通知动子离开：8"); // Log: Cho phép Mover rời trạm

                    MonitorCheckMover("Out", moverid); // Giám sát Mover ra khỏi trạm

                    currentrow = startrow; // Reset chỉ số hàng về ban đầu
                    return "Y轴到起始检测位"; // Chuyển sang: Trục Y về vị trí gốc

                case "Y轴到起始检测位": // [Sản xuất] Trục Y di chuyển về tọa độ tiêu cự ban đầu sẵn sàng đón lượt Mover tiếp theo
                    Ypos = POINT.穴位1检测聚焦位1.GetPos(AXIS检测Y轴, this);
                    AXIS检测Y轴.AbsMove(Ypos, 100);
                    WaitAxis(AXIS检测Y轴);
                    return StepEntry; // Về bước khởi đầu chu trình sản xuất
                default:
                    return null;
            }


        }


        private bool WaitAndAnalysCamData(object cmdIndex, TcpCommu_Camera_AOI camera, out bool bResult, out string NGReason)
        {
            bResult = false;
            string strRcvString = "";
            bool bRet = false;
            NGReason = "";
            if (!Parameter.GetBit("VisionEnable"))
            {
                bResult = true;
                return true;
            }

            for (int i = 0; i < 3; i++)
            {
                if (camera.WaitReply(cmdIndex, out strRcvString))
                {
                    bResult = CamResult();
                    if (!CamResult())
                    {
                        NGReason = camera.NgReason;
                    }
                    bRet = true;
                    break;
                }
                else
                {
                    if (i == 2)
                    {
                        if(ShowMessage($"{this.Name},视觉反馈结果超时3次", "WaitAndAnalysCamData",new string[] { "等待视觉处理", "跳过当前穴位" })==0)
                        {
                            WaitAndAnalysCamData(cmdccd, camera, out bResult, out string strNGReason);
                        }
                        else
                        {
                            bResult = false;
                            NGReason = "0";
                            Logger.Process(this.Name, "视觉无反馈结果，将结果手动设置为模型异常");
                        }
                    }
                    bRet = false;
                }
            }
           
            return bRet;
        }

        protected override string CalibRun(string strStep)
        {
            switch (strStep)
            {
                case StepEntry:
                    Delay(10);
                    return "相机标定开始";
                case "相机标定开始":
                    Delay(10);
                    return "相机标定开始";

                default:
                    return null;
            }
        }

        protected override string GrrRun(string strStep)
        {
            switch (strStep)
            {
                case StepEntry:
                    if (grr == GRRMode.Static)
                    {
                        return "相机静态流程开始";
                    }
                    else
                    {
                        return "相机动态流程开始";
                    }


                case "空步":
                    Delay(1000);
                    return "StepExit";

                //相机静态流程
                case "相机静态流程开始":
                    grrruncount = 0;
                    return "Z轴到安全位";

                case "Z轴到安全位":
                    AxisSingleTo(AXIS检测Z轴, POINT.安全位);
                    WaitAxis(AXIS检测Z轴);
                    return "移动到GRR测试拍照位";
                case "移动到GRR测试拍照位":
                    AxisSingleTo(AXIS检测Y轴, POINT.GRR测试拍照位);
                    WaitAxis(AXIS检测Y轴);
                    AxisSingleTo(AXIS检测Z轴, POINT.GRR测试拍照位);
                    WaitAxis(AXIS检测Z轴);
                    Delay(200);
                    return "提示OP放GRR测试料";
                case "提示OP放GRR测试料":
                    ShowMessage("请在载具相机工位放好GRR测试物料");
                    return "发送拍照指令";
                case "发送拍照指令":
                    camera.SendCommand(cmdgrr);

                    return "相机拍照";
                case "相机拍照":
                    camera.Camtrigger(cmdccd, true);
                    Delay(50);
                    camera.Camtrigger(cmdccd, false);
                    return "更新计数";
                case "更新计数":
                    grrruncount++;
                    return "计数判断";
                case "计数判断":
                    Delay(200);
                    if (grrruncount >= 200)
                    {
                        grrruncount = 0;
                        return "提示静态测试完成";
                    }
                    else
                    {
                        return "发送拍照指令";
                    }
                case "提示静态测试完成":
                    ShowMessage("静态测试完成");
                    return "空步";

                //相机动态流程
                case "相机动态流程开始":
                    grrruncount = 0;
                    return "检测Z轴到安全位";

                case "检测Z轴到安全位":
                    AxisSingleTo(AXIS检测Z轴, POINT.安全位);
                    WaitAxis(AXIS检测Z轴);
                    return "运动到GRR测试拍照位";
                case "运动到GRR测试拍照位":
                    AxisSingleTo(AXIS检测Y轴, POINT.GRR测试拍照位);
                    WaitAxis(AXIS检测Y轴);
                    AxisSingleTo(AXIS检测Z轴, POINT.GRR测试拍照位);
                    WaitAxis(AXIS检测Z轴);
                    return "提示OP放GRR动态测试料";
                case "提示OP放GRR动态测试料":
                    ShowMessage("请在载具相机工位放好GRR测试物料");
                    return "发送视觉拍照指令";
                case "发送视觉拍照指令":
                    camera.SendCommand(cmdgrr);

                    return "触发相机拍照";
                case "触发相机拍照":
                    camera.Camtrigger(cmdccd, false);
                    //Delay(50);
                    camera.Camtrigger(cmdccd, true);
                    //Delay(20);
                    return "计数更新";
                case "计数更新":
                    grrruncount++;
                    return "拍照次数判断";
                case "拍照次数判断":
                    if (grrruncount >= 200)
                    {
                        grrruncount = 0;
                        return "提示动态测试完成";
                    }
                    else
                    {
                        return "运动到安全位";
                    }
                case "提示动态测试完成":
                    ShowMessage("动态测试完成");
                    return "空步";

                case "运动到安全位":
                    AxisSingleTo(AXIS检测Z轴, POINT.安全位);
                    AxisSingleTo(AXIS检测Y轴, POINT.安全位);
                    WaitAxis(AXIS检测Z轴);
                    WaitAxis(AXIS检测Y轴);
                    return "运动到拍照位";
                case "运动到拍照位":
                    AxisSingleTo(AXIS检测Z轴, POINT.GRR测试拍照位);
                    AxisSingleTo(AXIS检测Y轴, POINT.GRR测试拍照位);
                    WaitAxis(AXIS检测Z轴);
                    WaitAxis(AXIS检测Y轴);
                    return "发送视觉拍照指令";


                default:
                    return null;
            }
        }

        protected override string OtherRun(string strStep)
        {
            switch (strStep)
            {
                case StepEntry:
                    return "Other";
                default:
                    return null;
            }
        }

        private void MonitorCheckMover(string process ,string moverid)
        {
            if (process == "In")
            {
                if (this.Name == "下相机AOI1站")
                {
                    string m_data = RedisHelperEx.Instance.HashGet(Global.m_RESPMonitorMover, "Wait2");
                    if (m_data.Split(',')[0] == moverid)
                    {
                        RedisHelperEx.Instance.HashSet(Global.m_RESPMonitorMover, "Wait2", $"Null");
                        RedisHelperEx.Instance.HashSet(Global.m_RESPMonitorMover, this.Name, $"{moverid},{m_data.Split(',')[1]}");
                    }
                }
                else if (this.Name == "上相机AOI站")
                {
                    string m_data = RedisHelperEx.Instance.HashGet(Global.m_RESPMonitorMover, "Wait3");
                    if (m_data.Split(',')[0] == moverid)
                    {
                        RedisHelperEx.Instance.HashSet(Global.m_RESPMonitorMover, "Wait3", $"Null");
                        RedisHelperEx.Instance.HashSet(Global.m_RESPMonitorMover, this.Name, $"{moverid},{m_data.Split(',')[1]}");
                    }
                }
                else if (this.Name == "下相机AOI2站")
                {
                    string m_data = RedisHelperEx.Instance.HashGet(Global.m_RESPMonitorMover, "Wait4");
                    if (m_data.Split(',')[0] == moverid)
                    {
                        RedisHelperEx.Instance.HashSet(Global.m_RESPMonitorMover, "Wait4", $"Null");
                        RedisHelperEx.Instance.HashSet(Global.m_RESPMonitorMover, this.Name, $"{moverid},{m_data.Split(',')[1]}");
                    }
                }
            }
            else if(process == "Out")
            {
                if (this.Name == "下相机AOI1站")
                {
                    string m_dataOut = RedisHelperEx.Instance.HashGet(Global.m_RESPMonitorMover, this.Name);
                    if (m_dataOut.Split(',')[1] == "1")
                    {
                        RedisHelperEx.Instance.HashSet(Global.m_RESPMonitorMover, "Wait3", $"{m_dataOut.Split(',')[0]},{m_dataOut.Split(',')[1]}");
                        RedisHelperEx.Instance.HashSet(Global.m_RESPMonitorMover, this.Name, $"{moverid},0");
                    }
                    else
                    {
                        RedisHelperEx.Instance.HashSet(Global.m_RESPMonitorMover, "Wait3", $"{m_dataOut.Split(',')[0]},{m_dataOut.Split(',')[1]}");
                        RedisHelperEx.Instance.HashSet(Global.m_RESPMonitorMover, this.Name, $"{moverid},0");
                    }
                }
                else if (this.Name == "上相机AOI站")
                {
                    string m_dataOut = RedisHelperEx.Instance.HashGet(Global.m_RESPMonitorMover, this.Name);
                    if (m_dataOut.Split(',')[1] == "1")
                    {
                        RedisHelperEx.Instance.HashSet(Global.m_RESPMonitorMover, "Wait4", $"{m_dataOut.Split(',')[0]},{m_dataOut.Split(',')[1]}");
                        RedisHelperEx.Instance.HashSet(Global.m_RESPMonitorMover, this.Name, $"{moverid},0");
                    }
                    else
                    {
                        RedisHelperEx.Instance.HashSet(Global.m_RESPMonitorMover, "Wait4", $"{m_dataOut.Split(',')[0]},{m_dataOut.Split(',')[1]}");
                        RedisHelperEx.Instance.HashSet(Global.m_RESPMonitorMover, this.Name, $"{moverid},0");
                    }
                }
                else if (this.Name == "下相机AOI2站")
                {
                    string m_dataOut = RedisHelperEx.Instance.HashGet(Global.m_RESPMonitorMover, this.Name);
                    if (m_dataOut.Split(',')[1] == "1")
                    {
                        RedisHelperEx.Instance.HashSet(Global.m_RESPMonitorMover, "Wait5", $"{m_dataOut.Split(',')[0]},{m_dataOut.Split(',')[1]}");
                        RedisHelperEx.Instance.HashSet(Global.m_RESPMonitorMover, this.Name, $"{moverid},0");
                    }
                    else
                    {
                        RedisHelperEx.Instance.HashSet(Global.m_RESPMonitorMover, "Wait5", $"{m_dataOut.Split(',')[0]},{m_dataOut.Split(',')[1]}");
                        RedisHelperEx.Instance.HashSet(Global.m_RESPMonitorMover, this.Name, $"{moverid},0");
                    }
                }
            }
        }
    }
    public static partial class HandleEx
    {
        public static double GetPos(this StationAOIExDown1.POINT pt, AXIS axis, StationBase station)
        {
            return station.dictPoint[(int)pt][$"a{(int)axis}"];
        }

        public static double GetPos(this StationAOIExDown1.POINT pt, AXIS axis, string strStation)
        {
            StationBase station = StationMgr.GetStation(strStation);
            if (station == null)
                return 0;

            return station.dictPoint[(int)pt][$"a{(int)axis}"];
        }
    }
}
