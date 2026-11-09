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
using AutoFrame;
namespace Autoframe
{
    /// <summary>
    /// 激光位移检测站
    /// </summary>
    public class StationLaserDisp : StationEx
    {
        string moverid;//动子ID号
        int sgngcount = 0;
        int curcavity;//当前穴位号
        int iRet;//动子当前行

        string curcavitystatus;//当前穴位状态

        string[] frontstadata = new string[Global.slotTotalCount];
        string[] currentstadata = new string[Global.slotTotalCount];
        string[] redisdata = new string[Global.trayTotalCol];

        double[] sgdata = new double[Global.trayTotalCol];

        bool firstinit = true;
        bool dataenable = true;//是否需要获取前站数据

        int currentrow = 0;
        int currentcol = 0;
        double Ypos = 0;
        double Zpos = 0;
        POINT 检测位;
        //TcpCommu_Camera laser;
        SGHelper SG = null;
        int laserNum = 0;
        /// <summary>
        /// 构造函数，需要设置站位当前的IO输入，IO输出，轴方向及轴名称，以显示在手动页面方便操作
        /// </summary>
        /// <param name="strName"></param>
        public StationLaserDisp(string strName) : base(strName)
        {
            HardwareConfig();
            CheckPoint<POINT>();
            LoadContainer<CONTAINER, ProductData>();
        }

        void HardwareConfig()
        {
            //配置站位界面显示输入
            arrayDI = new object[] { };
            //配置站位界面显示输出
            arrayDO = new object[] {  };
            //配置站位界面显示气缸
            arrayCylinder = new object[] { };
            //配置站位轴
            arrayAxis = new object[] { AXIS.位移激光检测Y轴 };
        
        }

        ProductData productData = new ProductData();

        //TcpCommu_Camera camera = new TcpCommu_Camera();
        /// <summary>
        /// 站位内的点位枚举
        /// </summary>
        public enum POINT
        {
            穴位1检测位//面对显示器，右上角

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
            AXIS.位移激光检测Y轴.ServoOn(false);

        }
        public override void OnPause()
        {
            //base.OnPause() 不能删，且需要放在第一行。
            base.OnPause();
        }
        public override void OnResume()
        {
            //base.OnResume() 不能删，且需要放在第一行。
            // return base.OnResume();
        }

        /// <summary>
        /// 初始化之前调用
        /// 1.设置初始化流程和自动流程的初始步
        /// 2.复位寄存器
        /// </summary>
        public override void Preinit()

        /// <summary>
        /// 为不同的流程设置起始步
        /// </summary>

        {
            switch (Process)
            {
                case AutoProcess.正常生产:
                case AutoProcess.空跑:
                case AutoProcess.相机标定:
                    //默认流程入口 是 StepEntry
                    break;
                //case AutoProcess.相机标定:
                //    break;
                default:
                    this.InitEntry = StepExit;
                    this.AutoEntry = StepEmpty;
                    break;
            }
            RegBit.激光位移检测站初始化完成.Set(false);
        }

        protected override string InitRun(string strStep)
        {
            switch (strStep)
            {
                case StepEntry:
                    return "激光连接"; // [Khởi tạo] Bắt đầu kết nối cảm biến đo khoảng cách Laser

                case "激光连接": // [Khởi tạo] Kết nối TCP/IP tới cảm biến khoảng cách Laser SSZN (SGHelper)
                    if (Parameter.GetBit("LaserEnable"))
                    {
                        SG = new SGHelper("192.168.0.10"); // IP cảm biến laser
                        SG.Close();
                        if (SG.Open())
                        {
                            sgngcount = 0;
                            Logger.Process(Name, "激光位移传感器连接成功"); // Log: Kết nối cảm biến đo cao Laser thành công
                            return "设置激光通讯模式"; // Thiết lập chế độ truyền thông
                        }
                        else
                        {
                            return "激光连接失败";
                        }
                    }
                    else
                    {
                        return "轴上使能";
                    }
                case "激光连接失败": // [Khởi tạo] Báo lỗi và đếm số lần kết nối Laser thất bại
                     sgngcount++;
                     if (sgngcount >= 3)
                        {
                           Logger.Process(Name, "激光位移传感器连接失败");
                           ShowMessage("激光位移传感器连接失败", null, new string[] { "再次连接" });
                           sgngcount = 0;
                        return "激光连接";
                        }
                     else
                        {
                        return "激光连接";
                         }
                case "设置激光通讯模式": // [Khởi tạo] Cài đặt mode giao tiếp cho Laser
                        if (SG.SetCommonicateMode(0))
                        {
                            sgngcount = 0;
                            Logger.Process(Name, "激光位移通讯模式设置成功"); // Log: Cấu hình mode truyền thông Laser thành công
                            return "轴上使能"; // Bật servo trục Y
                        }
                        else
                        {
                            return "通讯设置失败";
                        }
                   
                case "通讯设置失败": // [Khởi tạo] Đếm lỗi cấu hình mode truyền thông Laser
                    sgngcount++;
                    if (sgngcount >= 3)
                    {
                        Logger.Process(Name, "激光位移传感器通讯设置失败");
                        ShowMessage("激光位移传感器通讯设置失败", null, new string[] { "再次设置" });
                        sgngcount = 0;
                        return "设置激光通讯模式";
                    }
                    else
                    {
                        return "设置激光通讯模式";
                    }

                case "轴上使能": // [Khởi tạo] Bật servo trục Y di chuyển đầu đo Laser
                    AXIS.位移激光检测Y轴.ServoOn(true); // Bật Servo trục Y quét Laser
                    Delay(1000);
                    return "Y轴回检测位"; // Trục Y về vị trí kiểm tra

                case "Y轴回检测位": // [Khởi tạo] Trục Y về vị trí đo ô khay 1 ban đầu
                    AxisSingleTo(AXIS.位移激光检测Y轴, POINT.穴位1检测位);
                    WaitAxis(AXIS.位移激光检测Y轴); // Chờ trục Y dừng hẳn
                    return "初始化完成"; // Hoàn tất khởi tạo trạm

                case "初始化完成": // [Khởi tạo] Đánh dấu hoàn tất khởi tạo trạm đo cao Laser
                    firstinit = true;
                    dataenable = true;
                    currentrow = 0;
                    currentcol = 0;
                    RegBit.激光位移检测站初始化完成.Set(true); // Bật bit: Trạm đo Laser khởi tạo hoàn tất
                    return StepExit;
                default:
                    return null;
            }
        }

        protected override string ProduceRun(string strStep)
        {
            switch (strStep)
            {
                case StepEntry:
                    return "等待动子到位"; // [Sản xuất] Chờ mover vào vị trí đo Laser

                case "等待动子到位": // [Sản xuất] Đọc tín hiệu PLC kiểm tra mover đã vào trạm đo độ cao
                    iRet = Global.m_plcHelper.ReadData_Int16(0, new int[] { ((int)ReadPLC.高度检测到位信号位) }); // Đọc hàng hiện tại từ PLC (1..N)

                    if (iRet != 0)
                    {
                        return "获取动子号"; // Đọc ID mover
                    }
                    else
                    {
                        return "等待动子到位";
                    }
                case "获取动子号": // [Sản xuất] Lấy mã ID của mover từ PLC và cập nhật Redis
                    if (currentcol == 0 && currentrow == 0)
                    {
                        dataenable = true;
                    }
                  
                    moverid = Global.m_plcHelper.ReadData_Int16(0, new int[] { ((int)ReadPLC.高度检测工位动子号) }).ToString();
                    Global.arr_moveID[1] = moverid;
                    Logger.Process(this.Name, $"动子已到位-动子号:{moverid}"); // Log: Mover đã vào trạm Laser - Mã: moverid

                    string m_data = RedisHelperEx.Instance.HashGet(Global.m_RESPMonitorMover, "Wait1");
                    if (m_data.Split(',')[0] == moverid)
                    {
                        RedisHelperEx.Instance.HashSet(Global.m_RESPMonitorMover, "Wait1", $"Null"); // Xóa hàng chờ Wait1
                        RedisHelperEx.Instance.HashSet(Global.m_RESPMonitorMover, this.Name, $"{moverid},{m_data.Split(',')[1]}"); // Đánh dấu mover đang ở trạm Laser
                    }
                    return "检查数据库当前动子是否存在"; // Kiểm tra mover trên Redis

                case "检查数据库当前动子是否存在": // [Sản xuất] Kiểm tra xem mover có dữ liệu trên Redis không
                    if (RedisHelperEx.Instance.Keycheck("0", moverid.ToString()))
                    {
                        return "获取前站与本站数据"; // Lấy kết quả từ trạm trước (S1)
                    }
                    else
                    {
                        return "通知磁悬浮可离开"; // Không có data -> bỏ qua cho trôi đi
                    }

                case "获取前站与本站数据": // [Sản xuất] Đọc kết quả trạm trước (S1: tiếp nhận/chụp khay) từ Redis
                    if (dataenable)
                    {
                        dataenable = false;
                        for (int i = 0; i < Global.slotTotalCount; i++)
                        {                         
                            string strValue = RedisHelperEx.Instance.HashGet(moverid.ToString(), $"S1_{i + 1}"); // Đọc trạng thái trạm S1 (OK/NA/NG)
                            frontstadata[i] = strValue;
                            currentstadata[i] = RedisHelperEx.Instance.HashGet(moverid.ToString(), $"S2_{i + 1}"); // Đọc trạng thái trạm S2 hiện tại
                        }
                    }

                    return "本站数据判断"; // Phán đoán hàng/cột bắt đầu

                case "本站数据判断": // [Sản xuất] Xác định hàng hiện tại cần đo dựa theo tín hiệu PLC
                    if (firstinit) // Lần đầu tiên mover vào trạm
                    {
                        firstinit = false;
                        currentrow = iRet - 1; // Xác định hàng bắt đầu đo (0..N-1)
                        currentcol = 0;
                    }

                    return "当前动子行数判断";

                case "当前动子行数判断": // [Sản xuất] Đối chiếu hàng PLC yêu cầu với hàng phần mềm đang kiểm tra
                    Logger.Process(this.Name, $"动子当前行:{iRet}-工站检测行{currentrow + 1}");
                    if (iRet == currentrow + 1)
                    {
                        return "当前穴位判断"; // Đúng hàng -> kiểm tra từng ô khay (cột)
                    }
                    else
                    {
                        ShowMessage($"{this.Name}动子行:{iRet}与工站检测行:{currentrow + 1}不匹配,请检查", "行数失败", new string[] { "再次判断" });
                        iRet = Global.m_plcHelper.ReadData_Int16(0, new int[] { ((int)ReadPLC.高度检测到位信号位) });
                    }
                    return "当前动子行数判断";

                case "当前穴位判断": // [Sản xuất] Kiểm tra ô khay hiện tại có sản phẩm không (chỉ đo ô có trạng thái OK từ trạm trước)
                    if (currentcol <= Global.trayTotalCol - 1) // Chưa hết các cột của hàng này
                    {
                        int curcavity = currentcol + currentrow * Global.trayTotalCol; // Tính số thứ tự ô khay trong khay
                        curcavitystatus = frontstadata[curcavity]; // Trạng thái từ trạm trước

                        if (frontstadata[curcavity] == "OK" || IsDryRun) // Chỉ đo ô có sản phẩm OK
                        {
                            return "Y轴到检测位"; // Di chuyển trục Y tới ô này để đo Laser
                        }
                        else
                        {
                            redisdata[currentcol] = frontstadata[curcavity] == "NA" ? "NA" : "NG"; // Ô trống -> gán NA, không cần đo
                            currentcol++; // Bỏ qua, sang cột tiếp theo
                            return "当前穴位判断";
                        }
                    }
                    else
                    {
                        return "HandleCurrentColumn"; // Đã duyệt hết cột của hàng
                    }
               
                    
                case "Y轴到检测位": // [Sản xuất] Trục Y di chuyển tới tọa độ của cột (ô khay) tương ứng
                    Ypos = POINT.穴位1检测位.GetPos(AXIS.位移激光检测Y轴, this) - Math.Abs(Global.trayColGap) * currentcol; // Tọa độ = Gốc - Khoảng cách cột * số cột
                    AXIS.位移激光检测Y轴.AbsMove(Ypos, 100);
                    WaitAxis(AXIS.位移激光检测Y轴); // Chờ trục Y đến vị trí
                    Delay(250); // Nghỉ 250ms cho cơ cấu ổn định độ rung
                    return "LaserGetValue"; // Đọc giá trị đo Laser

                case "LaserGetValue": // [Sản xuất] Đọc giá trị đo khoảng cách/độ cao từ cảm biến Laser
                    if (Parameter.GetBit("LaserEnable"))
                    {
                        if (SG == null)
                        {
                            SG = new SGHelper("192.168.0.10");
                            SG.Close();
                            SG.Open();
                            Logger.Process(Name, "激光位移传感器连接成功"); 
                        }
                        sgdata[currentcol] = SG.GetSingleData(1); // Đọc độ cao đơn điểm
                        redisdata[currentcol] = sgdata[currentcol].ToString("0.000"); // Định dạng 3 chữ số thập phân
                        if (sgdata[currentcol] <= 0) // Nếu lần 1 đọc lỗi/âm -> đọc lại lần 2
                        {
                            sgdata[currentcol] = SG.GetSingleData(1);
                            redisdata[currentcol] = sgdata[currentcol].ToString("0.000");
                        }
                    }
                    else
                    {
                        sgdata[currentcol] = 0;
                        redisdata[currentcol] = "0.000";
                    }

                    return "CompareWithLoadingValue"; // Đối chiếu giá trị đo Laser với ảnh CCD1

                case "CompareWithLoadingValue": // [Sản xuất] So sánh chéo kết quả Laser với ảnh chụp có/không sản phẩm của CCD1
                    if (Parameter.GetBit("Enable_LaserCompareWithCCD1"))
                    {
                        if (Parameter.GetBit("LaserEnable"))
                        {
                            int index = currentcol + Global.trayTotalCol * currentrow;

                            if (frontstadata[index] == "NA") // Camera bảo KHÔNG CÓ sản phẩm
                            {
                                if (sgdata[currentcol] > 0) // Nhưng Laser lại đo thấy CÓ sản phẩm -> lệch dữ liệu
                                {
                                    switch (ShowMessage($"穴位{index + 1}激光数据({sgdata[currentcol].ToString("F3")})和上料数据({frontstadata[index]})不匹配", "数据不匹配".txtTrans(TransType.Trans), new string[] { "重新镭射".txtTrans(TransType.Trans), "用激光数据".txtTrans(TransType.Trans), "用上料数据".txtTrans(TransType.Trans) }))
                                    {
                                        case 0:
                                            return "LaserGetValue"; // Đo lại
                                        case 1:
                                            RedisHelperEx.Instance.HashSet(moverid, $"S1_{index + 1}", "OK"); // Chọn dùng kết quả Laser
                                            break;
                                        case 2:
                                            break; // Giữ nguyên kết quả camera
                                    }
                                }
                            }
                            else // Camera bảo CÓ sản phẩm
                            {
                                if (sgdata[currentcol] <= 0) // Nhưng Laser đo không thấy
                                {
                                    sgdata[currentcol] = Parameter.GetDouble("DownAOI1SGCalibHeight");
                                    redisdata[currentcol] = sgdata[currentcol].ToString("0.000");
                                }
                            }
                        }
                    }
                   
                    return "HandleCurrentColumn"; // Xử lý chuyển cột

                case "HandleCurrentColumn": // [Sản xuất] Kiểm tra xem đã đo hết tất cả các cột trong hàng chưa
                    if (currentcol >= (Global.trayTotalCol - 1))
                    {
                        currentcol = 0; // Reset số cột về 0
                        return "更新数据库"; // Ghi toàn bộ dữ liệu hàng này vào Redis
                    }
                    else
                    {
                        currentcol++; // Sang cột kế tiếp
                        return "当前穴位判断";
                    }

                case "更新数据库": // [Sản xuất] Cập nhật kết quả đo độ cao Laser (S2) của cả hàng lên Redis
                    for (int i = 0; i < Global.trayTotalCol; i++)
                    {
                        RedisHelperEx.Instance.HashSet(moverid, $"S2_{(i + 1) + Global.trayTotalCol * currentrow}", redisdata[i]); // Ghi giá trị S2_{index}
                    }
                    return "当前行数判断"; // Kiểm tra xem đã hết khay chưa

                case "当前行数判断": // [Sản xuất] Kiểm tra xem đã đo hết tất cả các hàng trong khay chưa
                    if (currentrow >= (Global.trayTotalRow - 1))
                    {
                        return "通知磁悬浮可离开"; // Đã đo xong toàn bộ khay -> cho mover đi tiếp
                    }
                    else
                    {
                        return "通知磁悬浮相对移动"; // Chưa hết hàng -> báo ray từ tính dịch sang hàng tiếp
                    }

                case "通知磁悬浮相对移动": // [Sản xuất] Báo PLC điều khiển ray từ tính dịch chuyển mover sang hàng tiếp theo
                    Logger.Process(this.Name, $"动子号{moverid}-PLC发行数{iRet}-相对移动-当前行：{currentrow+1}");
                    Global.m_plcHelper.WriteData_Int16(0, new int[] { ((int)WritePLC.高度检测到位信号位) }, 0); // Reset tín hiệu到位
                    Global.m_plcHelper.WriteData_Int16(0, new int[] { ((int)WritePLC.高度检测完成信号位) }, currentrow + 1); // Ghi PLC: Đã đo xong hàng (currentrow + 1)
                    Logger.Process(this.Name, $"高度检测步长:{Global.trayRowGap}--- 高度检测完成信号位:{currentrow + 1}");
                    currentrow++; // Tăng hàng lên 1
                  
                    return "Y轴到起始检测位"; // Trục Y về lại cột đầu tiên để chuẩn bị đo hàng mới

                case "通知磁悬浮可离开": // [Sản xuất] Đo xong toàn bộ khay, báo PLC cho mover rời khỏi trạm Laser sang trạm AOI dưới 1 (DownAOI1)
                    Global.m_plcHelper.WriteData_Int16(0, new int[] { ((int)WritePLC.高度检测到位信号位) }, 0); // Reset tín hiệu到位
                    Global.m_plcHelper.WriteData_Int16(0, new int[] { ((int)WritePLC.高度检测完成信号位) }, 8); // Ghi PLC: Hoàn tất trạm đo cao Laser = 8 (Mã lệnh cho mover đi tiếp)
                    Logger.Process(this.Name, $"动子号:{moverid} - 通知动子离开：8");

                    string m_dataOut = RedisHelperEx.Instance.HashGet(Global.m_RESPMonitorMover, this.Name);
                    if (m_dataOut.Split(',')[1] == "1")
                    {
                        RedisHelperEx.Instance.HashSet(Global.m_RESPMonitorMover, "Wait2", $"{m_dataOut.Split(',')[0]},{m_dataOut.Split(',')[1]}"); // Đẩy vào hàng chờ Wait2 của trạm DownAOI1
                        RedisHelperEx.Instance.HashSet(Global.m_RESPMonitorMover, this.Name, $"{moverid},0"); // Đánh dấu trạm Laser đã trống
                    }
                    else
                    {
                        RedisHelperEx.Instance.HashSet(Global.m_RESPMonitorMover, "Wait2", $"{m_dataOut.Split(',')[0]},{m_dataOut.Split(',')[1]}");
                        RedisHelperEx.Instance.HashSet(Global.m_RESPMonitorMover, this.Name, $"{moverid},0");
                    }

                    currentrow = 0; // Reset số hàng
                    currentcol = 0; // Reset số cột
                    return "Y轴到起始检测位"; // Trục Y về điểm gốc chuẩn bị cho khay tiếp

                case "Y轴到起始检测位": // [Sản xuất] Đưa trục Y về vị trí kiểm tra gốc
                    AxisSingleTo(AXIS.位移激光检测Y轴, POINT.穴位1检测位);
                    WaitAxis(AXIS.位移激光检测Y轴); // Chờ dừng hẳn
                        
                    return StepEntry; // Hoàn tất chu trình khay, quay về StepEntry đón mover kế tiếp

                default:
                    return null;

            }

        }

        protected override string CalibRun(string strStep)
        {
            switch (strStep)
            {
                case StepEntry:
                    Delay(10);
                    return "标定";
                default:
                    return null;
            }
        }

        protected override string GrrRun(string strStep)
        {
            switch (strStep)
            {
                case StepEntry:
                    return "GRR";
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
    }
    public static partial class HandleEx
    {
        public static double GetPos(this StationLaserDisp.POINT pt, AXIS axis, StationBase station)
        {
            return station.dictPoint[(int)pt][$"a{(int)axis}"];
        }

        public static double GetPos(this StationLaserDisp.POINT pt, AXIS axis, string strStation)
        {
            StationBase station = StationMgr.GetStation(strStation);
            if (station == null)
                return 0;

            return station.dictPoint[(int)pt][$"a{(int)axis}"];
        }
    }
}
