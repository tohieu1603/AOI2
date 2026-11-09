using AF.Card;
using AF.Common;
using AF.Communicate;
using AF.Data;
using AF.Station;
using Autoframe;
using AutoFrame;
using AutoFrame.DBExtend;
using Microsoft.VisualBasic.Logging;
using OxyPlot.Axes;
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
using static lctdevice.ecat_motion;

namespace Autoframe
{
    /// <summary>
    /// 上料取放站
    /// </summary>
    public class StationLoadingPickEx : StationEx
    {
        enum GRRMode
        {
            Static,
            Move

        }
        GRRMode grr;

        int nRow = Global.trayTotalRow;//行
        int nCol = Global.trayTotalCol;//列  
        int nRowDis = 24;//行间距
        int nColDis = 14;//列间距
        int grrruncount = 0;//GRR次数
        int moveCount = 0;//料盘Z轴相对运动次数



        int nCCDNGCount = 0;

        string ccddata="";
        bool ccdresult;
        bool grrmode;
        bool isHaveTray=false;



        string[,] PlateResults = null;


        /// <summary>
        /// 创建 M行N列 字符串二维数组，全部初始值为 "NA"
        /// </summary>
        /// <param name="nRow">M 行数</param>
        /// <param name="nCol">N 列数</param>
        /// <returns></returns>
        public static string[,] CreateMatrix(int nRow, int nCol)
        {
             nRow = Global.trayTotalRow;//行
             nCol = Global.trayTotalCol;//列  
            string[,] matrix = new string[nRow, nCol];
            // 双重循环全部赋值 NA
            for (int i = 0; i < nRow; i++)
            {
                for (int j = 0; j < nCol; j++)
                {
                    matrix[i, j] = "NA";
                }
            }
            return matrix;
        }



        /// <summary>
        /// 构造函数，需要设置站位当前的IO输入，IO输出，轴方向及轴名称，以显示在手动页面方便操作
        /// </summary>
        /// <param name="strName"></param>
        public StationLoadingPickEx(string strName) : base(strName)
        {
             nRow = Global.trayTotalRow;//行
             nCol = Global.trayTotalCol;//列  
            PlateResults = new string[nRow, nCol];
            HardwareConfig();
            CheckPoint<POINT>();
            LoadContainer<CONTAINER, ProductData>();
        }

        void HardwareConfig()
        {
            //配置站位界面显示输入
            arrayDI = new object[] { DI.取料站夹料盘夹紧位, DI.取料站托料盘夹紧位};
            //配置站位界面显示输出
            arrayDO = new object[] { DO.取料站夹料盘夹紧,DO.取料站夹料盘松开,DO.取料站托料盘夹紧,DO.取料站托料盘松开 };
            //配置站位界面显示气缸
            arrayCylinder = new object[] {Cylinder.取料站夹料盘, Cylinder.取料站托料盘  };
            //配置站位轴
            arrayAxis = new object[] { AXIS.上料取放X轴, AXIS.上料取放大Z轴,AXIS.上料取放小Z轴  };
        }

        ProductData productData = new ProductData();

        TcpCommu_Camera_CCD1 camera = new TcpCommu_Camera_CCD1();
        TcpCommu_Camera_CCD1.Command cmdgrr;
        /// <summary>
        /// 站位内的点位枚举
        /// </summary>
        public enum POINT
        {
            安全位,
            夹取料盘位,
            夹取分盘位,
            料盘托盘下位,
            料盘托盘上位,
            接驳放料位,
            接驳拍照位,
            初始化料仓放料位

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
            AXIS.上料取放X轴.ServoOn(false);
            AXIS.上料取放大Z轴.ServoOn(false);
            AXIS.上料取放小Z轴.ServoOn(false);
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
                    grrmode = false;

                    break;
                case AutoProcess.相机标定:
                    break;
                case AutoProcess.CCD1相机静态:
                    grrmode = true;
                    grr = GRRMode.Static;
                    cmdgrr = TcpCommu_Camera_CCD1.Command.CCD1静态;

                    break;
                case AutoProcess.CCD1相机动态:
                    grrmode = true;
                    grr = GRRMode.Move;
                    cmdgrr = TcpCommu_Camera_CCD1.Command.CCD1动态;
                    break;
                default:
                    this.InitEntry = StepExit;
                    this.AutoEntry = StepEmpty;
                    break;
            }
            RegBit.上料取放站初始化完成.Set(false);

            RegBit.上料取放站初始化安全.Set(false);
            RegBit.上料仓站初始化可放料.Set(false);//是否需要两个工站复位

            RegBit.入料接驳站通知可拍照.Set(false);
            RegBit.入料接驳站通知可放料.Set(false);

            RegBit.上料取放站拍照完成.Set(false);
            RegBit.上料取放站放料完成.Set(false);

        }

        protected override string InitRun(string strStep)
        {
            switch (strStep)
            {
                case StepEntry:
                    PlateResults = CreateMatrix(nRow, nCol); // [Khởi tạo] Tạo ma trận kết quả khay với giá trị ban đầu là "NA"
                    return "相机连接"; // Kết nối camera

                case "相机连接": // [Khởi tạo] Kết nối camera CCD1 (Camera định vị / kiểm tra khay trên mover)
                    if (Parameter.GetBit("LoadVisionEnable")) // Nếu bật cờ dùng camera
                    {
                        if (camera.Init(this))
                        {
                            nCCDNGCount = 0;
                            Logger.Process(Name, "相机连接成功"); // Log: Kết nối camera thành công
                            return "轴上使能"; // Bật servo các trục
                        }
                        else
                        {
                            return "相机连接失败"; // Chuyển sang xử lý lỗi kết nối
                        }       
                    }
                    else
                    {
                        nCCDNGCount = 0;
                        Logger.Process(Name, "未启用视觉"); // Log: Không sử dụng thị giác
                        return "轴上使能"; // Bật servo các trục
                    }

                case "相机连接失败": // [Khởi tạo] Đếm lỗi kết nối camera và cho phép retry
                    nCCDNGCount++;
                    if (nCCDNGCount >= 3)
                    {
                        Logger.Process(Name, "相机连接失败");
                        ShowMessage("CCD1相机连接失败", null, new string[] { "再次连接" }); // Báo lỗi kết nối CCD1 quá 3 lần
                        nCCDNGCount = 0;
                        return "相机连接";
                    }
                    else
                    {
                        return "相机连接"; // Thử kết nối lại
                    }

                case "轴上使能": // [Khởi tạo] Bật servo trục X, Z lớn, Z nhỏ cụm gắp khay
                    AXIS.上料取放X轴.ServoOn(true); // Bật Servo trục X di chuyển ngang
                    AXIS.上料取放大Z轴.ServoOn(true); // Bật Servo trục Z lớn (nâng hạ cụm gắp khay)
                    AXIS.上料取放小Z轴.ServoOn(true); // Bật Servo trục Z nhỏ (nâng hạ má đỡ khay)
                    Delay(1000);
                    return "料盘夹爪夹紧"; // Bước đệm trước khi kéo trục Z lên

                case "料盘夹爪夹紧": // [Khởi tạo] Vị trí trung gian
                    return "大Z轴回安全位"; // Đưa trục Z lớn về an toàn trước

                case "大Z轴回安全位": // [Khởi tạo] Trục Z lớn về vị trí an toàn trên cao
                    AxisSingleTo(AXIS.上料取放大Z轴, POINT.安全位);
                    WaitAxis(AXIS.上料取放大Z轴); // Chờ trục Z lớn dừng hẳn

                    return "托盘夹爪气缸松开"; // Mở xi lanh má đỡ khay

                case "托盘夹爪气缸松开": // [Khởi tạo] Thu xi lanh đỡ khay về và kiểm tra cảm biến
                    CylinderBack(Cylinder.取料站托料盘, false); // Mở má đỡ khay
                    Delay(1500);
                    if (DI.取料站托料盘夹紧位.Get()) // Nếu cảm biến vẫn báo kẹp -> kẹt cơ khí
                    {
                        ShowMessage("取料站托料盘夹爪没有松开,请排查", strStep, new string[] { "已经解决" }); // Cảnh báo: Kẹp đỡ khay chưa mở!
                        return "托盘夹爪气缸松开";
                    }
                    return "小Z轴回安全位"; // Trục Z nhỏ về an toàn

                case "小Z轴回安全位": // [Khởi tạo] Trục Z nhỏ về vị trí an toàn
                    AxisSingleTo(AXIS.上料取放小Z轴, POINT.安全位);
                    WaitAxis(AXIS.上料取放小Z轴);
                    return "X轴回安全位"; // Trục X về an toàn

                case "X轴回安全位": // [Khởi tạo] Trục X về vị trí an toàn
                    AxisSingleTo(AXIS.上料取放X轴, POINT.安全位);
                    WaitAxis(AXIS.上料取放X轴);
                    return "上料取放站初始化安全"; // Đánh dấu trạm gắp đã an toàn

                case "上料取放站初始化安全": // [Khởi tạo] Bật cờ an toàn báo cho trạm kho chứa (StationLoading)
                    RegBit.上料取放站初始化安全.Set(true); // Bật bit: Trạm gắp đã an toàn

                    return grrmode ? "提示OP确认夹爪有无料" : "等待上料站通知可放料"; // Kiểm tra chế độ GRR hoặc chờ trạm kho khay sẵn sàng


                case "等待上料站通知可放料": // [Khởi tạo] Chờ trạm kho chứa nâng khay lên vị trí sẵn sàng
                    if (RegBit.上料仓站初始化可放料.Get())
                    {
                        RegBit.上料仓站初始化可放料.Set(false); // Reset cờ
                        return "X轴移动到料仓放料位"; // Trục X chạy sang vị trí kho khay
                    }
                    else
                    {
                        return "等待上料站通知可放料";
                    }
                case "X轴移动到料仓放料位": // [Khởi tạo] Di chuyển trục X tới vị trí đặt khay trả lại kho
                    AxisSingleTo(AXIS.上料取放X轴, POINT.初始化料仓放料位);
                    WaitAxis(AXIS.上料取放X轴);
                    return "大Z轴移动到料仓放料位"; // Trục Z lớn hạ xuống vị trí đặt khay

                case "大Z轴移动到料仓放料位": // [Khởi tạo] Hạ trục Z lớn xuống vị trí đặt khay vào kho
                    AxisSingleTo(AXIS.上料取放大Z轴, POINT.初始化料仓放料位);
                    WaitAxis(AXIS.上料取放大Z轴);
                    return "夹料盘夹爪松开"; // Mở các kẹp khay để nhả khay

                case "提示OP确认夹爪有无料": // [Khởi tạo] Hỏi người vận hành kiểm tra tay kẹp có khay phôi hay không
                    ShowMessage("夹爪是否有无料盘,有料盘请取走", null, new string[] { "已取走料盘" }); // Mời thao tác viên lấy khay ra nếu có
                    return "夹料盘夹爪松开";

                case "夹料盘夹爪松开": // [Khởi tạo] Mở xi lanh kẹp khay và xi lanh đỡ khay
                    CylinderBack(Cylinder.取料站夹料盘, false); // Mở kẹp khay
                    Delay(500);
                    CylinderBack(Cylinder.取料站托料盘, false); // Mở đỡ khay
                    Delay(500);
                    return "大Z轴到安全位"; // Rút Z lớn lên cao

                case "大Z轴到安全位": // [Khởi tạo] Trục Z lớn về vị trí an toàn
                    AxisSingleTo(AXIS.上料取放大Z轴, POINT.安全位);
                    WaitAxis(AXIS.上料取放大Z轴);
                    return "X轴到安全位"; // Trục X về vị trí an toàn

                case "X轴到安全位": // [Khởi tạo] Trục X về an toàn
                    AxisSingleTo(AXIS.上料取放X轴, POINT.安全位);
                    WaitAxis(AXIS.上料取放X轴);
                    return "初始化完成"; // Hoàn tất khởi tạo trạm

                case "初始化完成": // [Khởi tạo] Đánh dấu hoàn tất khởi tạo trạm gắp đặt
                    DO.CCD1上料定位拍照.Set(false); // Tắt DO trigger chụp camera
                    RegBit.上料取放站初始化放料完成.Set(true); // Bật bit: Trạm gắp đã hoàn tất trả khay
                    RegBit.上料取放站初始化完成.Set(true); // Bật bit: Trạm gắp khởi tạo hoàn tất
                    return StepExit; // Kết thúc chu trình khởi tạo
                default:
                    return null;
            }
        }

        protected override string ProduceRun(string strStep)
        {
            switch (strStep)
            {
                case StepEntry:
                    productData = new ProductData(); // Khởi tạo đối tượng dữ liệu khay phôi
                    return "Z轴到安全位"; // Đưa 2 trục Z về vị trí an toàn

                case "Z轴到安全位": // [Sản xuất] Đưa cả trục Z lớn và Z nhỏ lên vị trí an toàn
                    AxisSingleTo(AXIS.上料取放大Z轴, POINT.安全位);
                    AxisSingleTo(AXIS.上料取放小Z轴, POINT.安全位);
                    WaitAxis(AXIS.上料取放大Z轴);
                    WaitAxis(AXIS.上料取放小Z轴);
                    return "MoveToSnapPoint"; // Trục X di chuyển tới vị trí chụp ảnh mover

                case "MoveToSnapPoint": // [Sản xuất] Di chuyển trục X tới vị trí chụp ảnh接驳
                    AxisSingleTo(AXIS.上料取放X轴, POINT.接驳拍照位);
                    WaitAxis(AXIS.上料取放X轴);
                    return "等待入料接驳通知可放料"; // Chờ trạm tiếp nhận mover (StationFeedTrans) sẵn sàng

                case "等待入料接驳通知可放料": // [Sản xuất] Chờ tín hiệu cho phép đặt khay từ trạm tiếp nhận
                    if (RegBit.入料接驳站通知可放料.Get())
                    {
                        RegBit.入料接驳站通知可放料.Set(false); // Reset bit
                        return "视觉拍照确认当前动子有无料盘"; // Chụp ảnh kiểm tra trên mover đã có khay chưa
                    }
                    else
                    {
                        return "等待入料接驳通知可放料";
                    }

                case "视觉拍照确认当前动子有无料盘": // [Sản xuất] Gửi lệnh chụp ảnh kiểm tra có/không có khay trên mover
                    if (Parameter.GetBit("LoadVisionEnable")) // Chế độ dùng thị giác
                    {
                        camera.SendCommand(TcpCommu_Camera_CCD1.Command.CCD1料盘有无); // Gửi command TCP
                        camera.Camtrigger(cmdgrr, true); // Kích tín hiệu trigger
                    }
                    return "获取视觉拍照数据"; // Chờ nhận kết quả

                case "获取视觉拍照数据": // [Sản xuất] Đợi kết quả trả về từ camera
                    if (Parameter.GetBit("LoadVisionEnable"))
                    {
                        if (camera.WaitReply(TcpCommu_Camera_CCD1.Command.CCD1料盘有无, out string camdata))
                        {
                            return "判断视觉拍照结果"; // Đã nhận data, phân tích kết quả
                        }
                        else
                        {
                            ShowMessage("等待视觉拍照确认动子有无料盘1s内没收到结果".txtTrans(TransType.Trans), "提示_CCD1", new string[] { "重新拍照" });
                            return "视觉拍照确认当前动子有无料盘"; // Chụp lại
                        }
                    }
                    else
                    {
                        return "判断视觉拍照结果";
                    }

                case "判断视觉拍照结果": // [Sản xuất] Kiểm tra cờ OK/NG của camera
                    if (Parameter.GetBit("LoadVisionEnable"))
                    {
                        ccdresult = camera.result;
                        if (ccdresult)
                        {
                            nCCDNGCount = 0;
                            return "视觉拍照结果处理"; // Chụp thành công -> xử lý
                        }
                        else
                        {
                            return "视觉拍照失败处理"; // Chụp thất bại
                        }
                    }
                    else
                    {
                        nCCDNGCount = 0;
                        return "视觉拍照结果处理";
                    }

                case "视觉拍照失败处理": // [Sản xuất] Đếm số lần chụp lỗi và cảnh báo
                    nCCDNGCount++;
                    if (nCCDNGCount >= 3)
                    {
                        if (ShowMessage($"{this.Name}相机连续{nCCDNGCount}次拍照失败,请检查", "拍照失败", new string[] { "重新拍照" }) == 0)
                            nCCDNGCount = 0;
                        return "视觉拍照确认当前动子有无料盘";
                    }
                    else
                    {
                        return "视觉拍照确认当前动子有无料盘";
                    }

                case "视觉拍照结果处理": // [Sản xuất] Phán đoán xem trên mover đã có khay chưa
                    if (Parameter.GetBit("LoadVisionEnable"))
                    {
                        if (camera.resultHaveProduct) // Mover ĐÃ CÓ KHAY
                        {
                            return "入料接驳解锁气缸缩回"; // Thu chốt khóa mover và không đặt thêm khay nữa
                        }
                        else // Mover CHƯA CÓ KHAY
                        {
                            return "移动到取料位上方"; // Sang bước gắp khay mới từ kho cấp liệu
                        }
                    }
                    else
                    {
                        return "移动到取料位上方";
                    }

                #region 取料 - Chu trình gắp khay từ kho
                case "移动到取料位上方": // [Sản xuất] Trục X chạy sang phía trên vị trí khay ở kho cấp liệu
                    AxisSingleTo(AXIS.上料取放X轴, POINT.夹取料盘位, bAllowInterrupt: true);
                    WaitAxis(AXIS.上料取放X轴);
                    return "等待料仓可取料"; // Chờ trạm kho khay báo sẵn sàng

                case "等待料仓可取料": // [Sản xuất] Chờ bit cho phép gắp từ StationLoading
                    if (RegBit.上料取放站可取料.Get())
                    {
                        RegBit.上料取放站可取料.Set(false); // Reset bit
                        return "大Z轴到取料位"; // Hạ Z lớn xuống gắp khay
                    }
                    else
                    {
                        return "等待料仓可取料";
                    }

                case "大Z轴到取料位": // [Sản xuất] Hạ trục Z lớn theo 2 giai đoạn (chạy nhanh đến gần điểm, sau đó hạ chậm 5mm cuối)
                    double ZPos = POINT.夹取料盘位.GetPos(AXIS.上料取放大Z轴, this);
                    AXIS.上料取放大Z轴.AbsMove(ZPos - 5, AxisHandle.SpeedPercent); // Hạ nhanh tới cách điểm 5mm
                    WaitAxis(AXIS.上料取放大Z轴);
                    AXIS.上料取放大Z轴.AbsMove(ZPos, 5.0); // Tiếp tục hạ chậm với tốc độ 5%
                    WaitAxis(AXIS.上料取放大Z轴);

                    AxisSingleTo(AXIS.上料取放大Z轴, POINT.夹取料盘位);
                    WaitAxis(AXIS.上料取放大Z轴);
                    return "取料时夹料盘夹紧"; // Kẹp chặt má kẹp khay

                case "取料时夹料盘夹紧": // [Sản xuất] Đẩy xi lanh kẹp khay vào vị trí kẹp
                    if (!IsDryRun)
                    {
                        CylinderOut(Cylinder.取料站夹料盘); // Kẹp má kẹp khay
                        Delay(200);
                    }
                    else
                    {
                        CylinderOut(Cylinder.取料站夹料盘, false);
                        Delay(200);
                    }
                    return "供料Z轴分盘多次运动"; // Điều khiển trục Z kho khay rung nhẹ để tách khay

                case "供料Z轴分盘多次运动": // [Sản xuất] Trục Z cấp liệu di chuyển lên xuống nhẹ để tách rời khay trên cùng khỏi chồng khay
                    if (moveCount <= 2)
                    {
                        moveCount++;
                        HandleEx.RelativeMove(AXIS.料盘供料Z轴, -1, 5.0); // Hạ 1mm
                        WaitAxis(AXIS.料盘供料Z轴);
                        Delay(200);
                        if (moveCount <= 2) // Lần cuối không cần nhấc lên lại
                        {
                            HandleEx.RelativeMove(AXIS.料盘供料Z轴, 1, 5.0); // Nâng 1mm
                            WaitAxis(AXIS.料盘供料Z轴);
                            Delay(200);
                        }

                        return "供料Z轴分盘多次运动";
                    }
                    else
                    {
                        moveCount = 0;
                        return "大Z轴到料盘分离位"; // Kéo trục Z lớn lên điểm tách khay
                    }

                case "大Z轴到料盘分离位": // [Sản xuất] Trục Z lớn nhấc khay lên điểm tách khay
                    AxisSingleTo(AXIS.上料取放大Z轴, POINT.夹取分盘位);
                    WaitAxis(AXIS.上料取放大Z轴);
                    return "小Z轴到料盘托盘位"; // Trục Z nhỏ di chuyển lên đỡ đáy khay

                case "小Z轴到料盘托盘位": // [Sản xuất] Trục Z nhỏ lên vị trí chờ đỡ khay
                    AxisSingleTo(AXIS.上料取放小Z轴, POINT.料盘托盘下位);
                    WaitAxis(AXIS.上料取放小Z轴);
                    return "取料时托料盘夹紧"; // Đẩy ngàm đỡ khay ra

                case "取料时托料盘夹紧": // [Sản xuất] Đẩy ngàm đỡ khay vào đáy khay
                    if (!IsDryRun)
                    {
                        CylinderOut(Cylinder.取料站托料盘); // Đẩy má đỡ khay ra
                        Delay(300);
                    }
                    else
                    {
                        CylinderOut(Cylinder.取料站托料盘, false);
                        Delay(200);
                    }
                    return "小Z轴到料盘托住上位";

                case "小Z轴到料盘托住上位":
                    return "大Z轴到安全高度"; // Đưa trục Z lớn lên cao độ an toàn

                case "大Z轴到安全高度": // [Sản xuất] Trục Z lớn lên vị trí an toàn trên cao
                    AxisSingleTo(AXIS.上料取放大Z轴, POINT.安全位);
                    WaitAxis(AXIS.上料取放大Z轴);
                    return "移动到入料接驳拍照位上方";

                case "移动到入料接驳拍照位上方":
                    return "通知料仓取料完成"; // Báo cho kho khay biết đã lấy xong khay

                case "通知料仓取料完成": // [Sản xuất] Bật cờ thông báo cho StationLoading đã lấy khay thành công
                    RegBit.上料取放站取料完成.Set(true); // Bật bit: Gắp khay hoàn tất
                    return "移动到入料接驳放料位上方"; // Trục X di chuyển sang vị trí đặt khay lên mover
                #endregion


                #region 放料 - Chu trình đặt khay lên mover 
                case "移动到入料接驳放料位上方": // [Sản xuất] Trục X sang vị trí thả khay, đẩy xi lanh nâng và mở khóa mover
                    AxisSingleTo(AXIS.上料取放X轴, POINT.接驳放料位);
                    WaitAxis(AXIS.上料取放X轴);
                    CylinderOut(Cylinder.入料移载站升降); // Đẩy xi lanh nâng bàn tiếp nhận
                    CylinderOut(Cylinder.入料移载站解锁); // Mở chốt khóa định vị mover
                    Delay(500);
                    return "大Z轴下降到放料位"; // Hạ Z lớn xuống đặt khay

                case "大Z轴下降到放料位": // [Sản xuất] Trục Z lớn hạ khay xuống mover (hạ nhanh tới -5mm rồi hạ chậm 5% tốc độ)
                    ZPos = POINT.接驳放料位.GetPos(AXIS.上料取放大Z轴, this);
                    AXIS.上料取放大Z轴.AbsMove(ZPos - 5, AxisHandle.SpeedPercent);
                    WaitAxis(AXIS.上料取放大Z轴);
                    AXIS.上料取放大Z轴.AbsMove(ZPos, 5.0);
                    WaitAxis(AXIS.上料取放大Z轴);
                    return "夹爪气缸松开"; // Nhả kẹp để bàn giao khay cho mover

                case "夹爪气缸松开": // [Sản xuất] Mở xi lanh đỡ khay và xi lanh kẹp khay
                    CylinderBack(Cylinder.取料站托料盘, false); // Thu ngàm đỡ khay về
                    Delay(200);
                    CylinderBack(Cylinder.取料站夹料盘, false); // Mở má kẹp khay
                    Delay(300);
                    return "入料接驳解锁缩回"; // Thu chốt mở khóa bàn tiếp nhận

                case "入料接驳解锁缩回": // [Sản xuất] Thu xi lanh mở khóa mover
                    CylinderBack(Cylinder.入料移载站解锁);
                    return "Z轴回安全位"; // Rút 2 trục Z lên cao

                case "Z轴回安全位": // [Sản xuất] Đưa cả trục Z lớn và Z nhỏ về vị trí an toàn trên cao
                    AxisSingleTo(AXIS.上料取放大Z轴, POINT.安全位);
                    WaitAxis(AXIS.上料取放大Z轴);
                    AxisSingleTo(AXIS.上料取放小Z轴, POINT.安全位);
                    WaitAxis(AXIS.上料取放小Z轴);
                    return "通知入料接驳站放料完成"; // Thông báo trạm tiếp nhận mover đã đặt khay xong
                #endregion

                case "入料接驳解锁气缸缩回": // [Sản xuất - Rẽ nhánh khi mover đã có sẵn khay]
                    isHaveTray = true; // Đánh dấu mover đã có khay
                    CylinderBack(Cylinder.入料移载站解锁);
                    return "通知入料接驳站放料完成";

                case "通知入料接驳站放料完成": // [Sản xuất] Bật bit báo trạm tiếp nhận hoàn tất đặt khay
                    RegBit.上料取放站放料完成.Set(true); // Bật bit: Đặt khay hoàn tất
                    return "移到接驳拍照位"; // Chuyển trục X về vị trí chụp kiểm tra sản phẩm trong ô khay


                #region 拍穴位有无 - Chụp ảnh định vị / kiểm tra tồn sản phẩm trong từng ô khay
                case "移到接驳拍照位": // [Sản xuất] Di chuyển trục X tới vị trí chụp ảnh kiểm tra từng ô khay
                    AxisSingleTo(AXIS.上料取放X轴, POINT.接驳拍照位);
                    WaitAxis(AXIS.上料取放X轴);
                    return "等待入料接驳通知可拍照"; // Chờ trạm tiếp nhận thông báo mover đã vào vị trí chụp

                case "等待入料接驳通知可拍照": // [Sản xuất] Chờ bit cho phép chụp ảnh từ StationFeedTrans
                    if (RegBit.入料接驳站通知可拍照.Get() || false)
                    {
                        RegBit.入料接驳站通知可拍照.Set(false); // Reset bit
                        return "通知视觉拍照"; // Kích hoạt chụp ảnh
                    }
                    else
                    {
                        return "等待入料接驳通知可拍照";
                    }

                case "通知视觉拍照": // [Sản xuất] Gửi lệnh TCP và trigger chụp định vị kiểm tra từng ô khay
                    if (Parameter.GetBit("LoadVisionEnable") && !IsDryRun)
                    {
                        camera.SendCommand(TcpCommu_Camera_CCD1.Command.CCD1上料定位拍照, AXIS.上料取放X轴.GetPos(), 0, 0); // Gửi tọa độ trục X kèm lệnh chụp
                        camera.Camtrigger(cmdgrr, true); // Kích tín hiệu trigger
                        return "获取视觉数据"; // Chờ nhận dữ liệu
                    }
                    else
                    {
                        return "获取视觉数据";
                    }

                case "获取视觉数据": // [Sản xuất] Nhận chuỗi kết quả từ camera CCD1
                    if (Parameter.GetBit("LoadVisionEnable") && !IsDryRun)
                    {
                        camera.WaitReply(TcpCommu_Camera_CCD1.Command.CCD1上料定位拍照, out string camdata);
                        return "判断视觉结果"; // Phân tích kết quả
                    }
                    else
                    {
                        return "判断视觉结果";
                    }

                case "判断视觉结果": // [Sản xuất] Kiểm tra camera báo thành công hay thất bại
                    if (Parameter.GetBit("LoadVisionEnable") && !IsDryRun)
                    {
                        ccdresult = camera.result;
                        if (ccdresult)
                        {
                            nCCDNGCount = 0;
                            return "视觉结果处理"; // Chụp thành công -> ghi Redis
                        }
                        else
                        {
                            return "视觉拍照失败"; // Chụp thất bại -> đếm lỗi retry
                        }
                    }
                    else
                    {
                        nCCDNGCount = 0;
                        return "视觉结果处理";
                    }

                case "视觉拍照失败": // [Sản xuất] Đếm số lần chụp lỗi liên tiếp và hiện thông báo
                    nCCDNGCount++;
                    if (nCCDNGCount >= 3)
                    {
                        if (ShowMessage($"{this.Name}相机连续{nCCDNGCount}次拍照失败,请检查", "拍照失败", new string[] { "重新拍照" }) == 0)
                            nCCDNGCount = 0;
                        return "通知视觉拍照"; // Chụp lại
                    }
                    else
                    {
                        return "通知视觉拍照";
                    }

                case "视觉结果处理": // [Sản xuất] Phân tích trạng thái từng ô khay (0: không có linh kiện = NA, 1: có linh kiện = OK) và ghi vào Redis
                    int inputQty = 0;
                    if (Parameter.GetBit("LoadVisionEnable") && !IsDryRun)
                    {
                        for (int i = 0; i < Global.slotTotalCount; i++)
                        {
                            if (camera.ccdData[i] == "0") // Ô khay trống
                            {
                                RedisHelperEx.Instance.HashSet(Global.arr_moveID[0], $"S1_{i + 1}", "NA"); // Ghi trạng thái NA                                         
                            }
                            else // Ô khay có sản phẩm
                            {
                                RedisHelperEx.Instance.HashSet(Global.arr_moveID[0], $"S1_{i + 1}", "OK"); // Ghi trạng thái OK
                                inputQty++;
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < Global.slotTotalCount; i++)
                        {
                            RedisHelperEx.Instance.HashSet(Global.arr_moveID[0], $"S1_{i + 1}", "OK");
                            inputQty++;
                        }
                    }
                    // Ghi thời gian bắt đầu trạm S1 và tổng số lượng linh kiện nạp vào
                    RedisHelperEx.Instance.HashSet($"{Global.arr_moveID[0]}_Data", $"S1_StartTime", $"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff")}");
                    RedisHelperEx.Instance.HashSet($"{Global.arr_moveID[0]}_Data", $"S1_InputQty", $"{inputQty}");
                    return "通知入料接驳站拍照完成"; // Thông báo trạm tiếp nhận mover đã chụp xong

                case "通知入料接驳站拍照完成": // [Sản xuất] Bật cờ báo trạm StationFeedTrans biết đã chụp xong
                    RegBit.上料取放站拍照完成.Set(true); // Bật bit: Chụp ảnh hoàn tất
                    if (isHaveTray)
                    {
                        isHaveTray = false;
                        return "X轴到入料接驳拍照位上方";
                    }
                    else
                    {
                        return StepEntry; // Chu trình hoàn tất, quay lại StepEntry chờ mover kế tiếp
                    }

                case "X轴到入料接驳拍照位上方": // [Sản xuất] Đưa trục X về vị trí chụp ảnh chờ khay kế tiếp
                    AxisSingleTo(AXIS.上料取放X轴, POINT.接驳拍照位);
                    WaitAxis(AXIS.上料取放X轴);
                    return "等待入料接驳通知可放料";
                default:
                    return null;
                    #endregion
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
                    if (grr==GRRMode.Static)
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
                        return "大Z轴到安全位";

                case "大Z轴到安全位":
                        AxisSingleTo(AXIS.上料取放大Z轴,POINT.安全位 );
                        WaitAxis(AXIS.上料取放大Z轴);
                        return "小Z轴到安全位";

                case "小Z轴到安全位":
                        AxisSingleTo(AXIS.上料取放小Z轴,POINT.安全位);
                        WaitAxis(AXIS.上料取放小Z轴);
                        return "提示OP放GRR测试料";

                 case "提示OP放GRR测试料":
                        ShowMessage("请在载具相机工位放好GRR测试物料");
                        return "移动到GRR测试拍照位";

                 case "移动到GRR测试拍照位":
                        AxisSingleTo(AXIS.上料取放X轴,POINT.接驳拍照位 );
                        WaitAxis(AXIS.上料取放X轴);

                       return "发送拍照指令";
                 case "发送拍照指令":
                       camera.SendCommand(cmdgrr);

                       return "相机拍照";
                 case "相机拍照":
                    camera.Camtrigger(cmdgrr, true);
                    return "等待视觉反馈";

                case "等待视觉反馈":

                        camera.WaitReply(cmdgrr, out string camdata);
                        //camera.Camtrigger(cmdccd, false);
                        return "更新计数";

                 case "更新计数":
                       grrruncount++;
                       return "计数判断";

                 case "计数判断":
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
                       return "取放大Z轴到安全位";

                 case "取放大Z轴到安全位":
                        AxisSingleTo(AXIS.上料取放大Z轴,POINT.安全位);
                        WaitAxis(AXIS.上料取放大Z轴);
                        return "取放小Z轴到安全位";

                 case "取放小Z轴到安全位":
                        AxisSingleTo(AXIS.上料取放小Z轴,POINT.安全位);
                        WaitAxis(AXIS.上料取放小Z轴);
                       return "提示OP放GRR动态测试料";

                 case "提示OP放GRR动态测试料":
                        ShowMessage("请在载具相机工位放好GRR测试物料");
                        return "发送视觉拍照指令";

                 case "发送视觉拍照指令":
                        camera.SendCommand(cmdgrr);

                        return "触发相机拍照";
                 case "触发相机拍照":
                    //DO.CCD1上料定位拍照.Set(false);
                    //Delay(10);
                    //DO.CCD1上料定位拍照.Set(true);
                    //Delay(500);
                    //DO.CCD1上料定位拍照.Set(false);
                    camera.Camtrigger(cmdgrr, true);
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

                       AxisSingleTo(AXIS.上料取放X轴,POINT.安全位);
                       WaitAxis(AXIS.上料取放X轴);
                       return "运动到拍照位";
                 case "运动到拍照位":
                       AxisSingleTo(AXIS.上料取放X轴,POINT.接驳拍照位);
                       WaitAxis(AXIS.上料取放X轴);
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
    }
    public static partial class HandleEx
    {
        public static double GetPos(this StationLoadingPickEx.POINT pt, AXIS axis, StationBase station)
        {
            return station.dictPoint[(int)pt][$"a{(int)axis}"];
        }

        public static double GetPos(this StationLoadingPickEx.POINT pt, AXIS axis, string strStation)
        {
            StationBase station = StationMgr.GetStation(strStation);
            if (station == null)
                return 0;

            return station.dictPoint[(int)pt][$"a{(int)axis}"];
        }
    }
}
