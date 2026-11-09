//m97 - new UnloadGantry Station
using AF.Card;
using AF.Common;
using AF.Communicate;
using AF.Data;
using AF.Station;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Autoframe
{

    public class StationUnloadGantryEx : StationEx
    {
        TcpCommu_Camera CCD5Camera;
        TcpCommu_Camera CCD6Camera;
        TcpCommu_Camera CCD7Camera;    
        SwapStack curStack;

        /// <summary>
        /// 构造函数，需要设置站位当前的IO输入，IO输出，轴方向及轴名称，以显示在手动页面方便操作
        /// </summary>
        /// <param name="strName"></param>
        public StationUnloadGantryEx(string strName) : base(strName)
        {
            HardwareConfig();
            CheckPoint<POINT>();
            LoadContainer<CONTAINER, ProductData>();
        }
        void HardwareConfig()
        {

            //配置站位界面显示输入
            arrayDI = new object[] { DI.下料站吸嘴1真空到位, DI.下料站吸嘴2真空到位, DI.下料站吸嘴3真空到位, DI.下料站吸嘴4真空到位 };
            //配置站位界面显示输出
            arrayDO = new object[] { DO.CCD5下料定位拍照,DO.CCD6下料定位拍照,DO.CCD7料盘定位拍照,DO.下料站吸嘴1吸真空,DO.下料站吸嘴1破真空,
            DO.下料站吸嘴2吸真空,DO.下料站吸嘴2破真空,DO.下料站吸嘴3吸真空,DO.下料站吸嘴3破真空,DO.下料站吸嘴4吸真空,DO.下料站吸嘴4破真空};
            //配置站位界面显示气缸
            arrayCylinder = new object[] { };
            //配置站位轴
            arrayAxis = new object[] { AXIS.下料取放X轴,AXIS.下料取放Y轴,AXIS.下料取放吸嘴1Z轴,AXIS.下料取放吸嘴1U轴,
            AXIS.下料取放吸嘴2Z轴,AXIS.下料取放吸嘴2U轴,AXIS.下料取放吸嘴3Z轴,AXIS.下料取放吸嘴3U轴,AXIS.下料取放吸嘴4Z轴,AXIS.下料取放吸嘴4U轴};
        }
        ProductData productData = new ProductData();
        /// <summary>
        /// 站位内的点位枚举
        /// </summary>
        public enum POINT
        {

            安全位,
            CCD7拍照位A1,
            CCD7拍照位A2,
            CCD7拍照位A3,
            CCD7拍照位A4,
            CCD7拍照位B1,
            CCD7拍照位B2,
            CCD7拍照位B3,
            CCD7拍照位B4,
            CCD7拍照位C1,
            CCD7拍照位C2,
            CCD7拍照位C3,
            CCD7拍照位C4,

          

            A区吸嘴1取料对应的第1个穴位,
            A区吸嘴2取料对应的第1个穴位,
            A区吸嘴3取料对应的第1个穴位,
            A区吸嘴4取料对应的第1个穴位,

            B区吸嘴1取料对应的第1个穴位,
            B区吸嘴2取料对应的第1个穴位,
            B区吸嘴3取料对应的第1个穴位,
            B区吸嘴4取料对应的第1个穴位,

            C区吸嘴1取料对应的第1个穴位,
            C区吸嘴2取料对应的第1个穴位,
            C区吸嘴3取料对应的第1个穴位,
            C区吸嘴4取料对应的第1个穴位,

            CCD5吸嘴1和2拍照位,
            CCD5吸嘴3和4拍照位,
            CCD6吸嘴1和2拍照位,
            CCD6吸嘴3和4拍照位,

            A区吸嘴1放料对应的第1个穴位,
            A区吸嘴2放料对应的第1个穴位,
            A区吸嘴3放料对应的第1个穴位,
            A区吸嘴4放料对应的第1个穴位,

            B区吸嘴1放料对应的第1个穴位,
            B区吸嘴2放料对应的第1个穴位,
            B区吸嘴3放料对应的第1个穴位,
            B区吸嘴4放料对应的第1个穴位,

            C区吸嘴1放料对应的第1个穴位,
            C区吸嘴2放料对应的第1个穴位,
            C区吸嘴3放料对应的第1个穴位,
            C区吸嘴4放料对应的第1个穴位,

            CCD7与CCD5关联拍菲林片,

            吸嘴1取菲林片位置,
            CCD5拍菲林片位置,
            CCD6拍菲林片位置,
            标定安全位,

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
            AXIS.下料取放X轴.ServoOn(false);
            AXIS.下料取放Y轴.ServoOn(false);
            AXIS.下料取放吸嘴1Z轴.ServoOn(false);
            AXIS.下料取放吸嘴1U轴.ServoOn(false);
            AXIS.下料取放吸嘴2Z轴.ServoOn(false);
            AXIS.下料取放吸嘴2U轴.ServoOn(false);
            AXIS.下料取放吸嘴3Z轴.ServoOn(false);
            AXIS.下料取放吸嘴3U轴.ServoOn(false);
            AXIS.下料取放吸嘴4Z轴.ServoOn(false);
            AXIS.下料取放吸嘴4U轴.ServoOn(false);
            SwapController.Stop();
        }
        public override void OnPause()
        {
            //base.OnPause() 不能删，且需要放在第一行。
            base.OnPause();
        }
        public override void OnResume()
        {
            //base.OnResume() 不能删，且需要放在第一行。
            base.OnResume();
        }

        /// <summary>
        /// [Khởi tạo trước - Preinit] Thiết lập trạng thái an toàn của trạm gantry dỡ liệu khi bắt đầu khởi tạo,
        /// phối hợp khoá liên động (interlock) an toàn giữa các trạm để tránh va chạm.
        /// Đồng thời điều hướng bước bắt đầu (AutoEntry) tương ứng theo chế độ chạy được chọn (Sản xuất, Dry-run, Hiệu chuẩn camera, Đăng ký template, Test động/tĩnh).
        /// </summary>
        public override void Preinit()
        {
            switch (Process)
            {
                case AutoProcess.正常生产: // Chế độ sản xuất bình thường
                case AutoProcess.空跑:     // Chế độ chạy không tải (Dry run)
                    this.AutoEntry = StepEntry; // Bắt đầu từ bước đầu tiên StepEntry
                    break;
                case AutoProcess.CCD5第1个吸嘴相机9点标定: // Hiệu chuẩn 9 điểm đầu hút 1 camera CCD5
                    this.AutoEntry = "CCD5第1个吸嘴相机9点标定";
                    break;
                case AutoProcess.CCD6第1个吸嘴相机9点标定: // Hiệu chuẩn 9 điểm đầu hút 1 camera CCD6
                    this.AutoEntry = "CCD6第1个吸嘴相机9点标定";
                   break;

                case AutoProcess.CCD5第1个吸嘴的5点标定: // Hiệu chuẩn xoay 5 điểm đầu hút 1 camera CCD5
                    this.AutoEntry = "CCD5第1个吸嘴的5点标定";
                    break;
                case AutoProcess.CCD5第2个吸嘴的5点标定: // Hiệu chuẩn xoay 5 điểm đầu hút 2 camera CCD5
                    this.AutoEntry = "CCD5第2个吸嘴的5点标定";
                    break;
                case AutoProcess.CCD5第3个吸嘴的5点标定: // Hiệu chuẩn xoay 5 điểm đầu hút 3 camera CCD5
                    this.AutoEntry = "CCD5第3个吸嘴的5点标定";
                    break;
                case AutoProcess.CCD5第4个吸嘴的5点标定: // Hiệu chuẩn xoay 5 điểm đầu hút 4 camera CCD5
                    this.AutoEntry = "CCD5第4个吸嘴的5点标定";
                    break;

                case AutoProcess.CCD6第1个吸嘴的5点标定: // Hiệu chuẩn xoay 5 điểm đầu hút 1 camera CCD6
                    this.AutoEntry = "CCD6第1个吸嘴的5点标定";
                    break;
                case AutoProcess.CCD6第2个吸嘴的5点标定: // Hiệu chuẩn xoay 5 điểm đầu hút 2 camera CCD6
                    this.AutoEntry = "CCD6第2个吸嘴的5点标定";
                    break;
                case AutoProcess.CCD6第3个吸嘴的5点标定: // Hiệu chuẩn xoay 5 điểm đầu hút 3 camera CCD6
                    this.AutoEntry = "CCD6第3个吸嘴的5点标定";
                    break;
                case AutoProcess.CCD6第4个吸嘴的5点标定: // Hiệu chuẩn xoay 5 điểm đầu hút 4 camera CCD6
                    this.AutoEntry = "CCD6第4个吸嘴的5点标定";
                    break;

                case AutoProcess.CCD5注册模板吸嘴1: // Đăng ký template mẫu CCD5 cho đầu hút 1
                    this.AutoEntry = "CCD5注册模板吸嘴1";
                    break;
                case AutoProcess.CCD5注册模板吸嘴2: // Đăng ký template mẫu CCD5 cho đầu hút 2
                    this.AutoEntry = "CCD5注册模板吸嘴2";
                    break;
                case AutoProcess.CCD5注册模板吸嘴3: // Đăng ký template mẫu CCD5 cho đầu hút 3
                    this.AutoEntry = "CCD5注册模板吸嘴3";
                    break;
                case AutoProcess.CCD5注册模板吸嘴4: // Đăng ký template mẫu CCD5 cho đầu hút 4
                    this.AutoEntry = "CCD5注册模板吸嘴4";
                    break;

                case AutoProcess.CCD6注册模板吸嘴1: // Đăng ký template mẫu CCD6 cho đầu hút 1
                    this.AutoEntry = "CCD6注册模板吸嘴1";
                    break;
                case AutoProcess.CCD6注册模板吸嘴2: // Đăng ký template mẫu CCD6 cho đầu hút 2
                    this.AutoEntry = "CCD6注册模板吸嘴2";
                    break;
                case AutoProcess.CCD6注册模板吸嘴3: // Đăng ký template mẫu CCD6 cho đầu hút 3
                    this.AutoEntry = "CCD6注册模板吸嘴3";
                    break;
                case AutoProcess.CCD6注册模板吸嘴4: // Đăng ký template mẫu CCD6 cho đầu hút 4
                    this.AutoEntry = "CCD6注册模板吸嘴4";
                    break;


                case AutoProcess.CCD5吸嘴1静态测试: // Kiểm tra tĩnh (Static Test) CCD5 đầu hút 1
                    this.AutoEntry = "CCD5吸嘴1静态测试";
                    break;
                case AutoProcess.CCD6吸嘴1静态测试: // Kiểm tra tĩnh (Static Test) CCD6 đầu hút 1
                    this.AutoEntry = "CCD6吸嘴1静态测试";
                    break;
                case AutoProcess.CCD5吸嘴1动态测试: // Kiểm tra động (Dynamic Test) CCD5 đầu hút 1
                    this.AutoEntry = "CCD5吸嘴1动态测试";
                    break;
                case AutoProcess.CCD6吸嘴1动态测试: // Kiểm tra động (Dynamic Test) CCD6 đầu hút 1
                    this.AutoEntry = "CCD6吸嘴1动态测试";
                    break;

                case AutoProcess.CCD7静态测试: // Kiểm tra tĩnh (Static Test) camera quét khay CCD7
                    this.AutoEntry = "CCD7静态测试";
                    break;
                case AutoProcess.CCD7动态测试: // Kiểm tra động (Dynamic Test) camera quét khay CCD7
                    this.AutoEntry = "CCD7动态测试";
                    break;

                case AutoProcess.CCD7与CCD5关联: // Căn chỉnh liên kết toạ độ CCD7 và CCD5
                    this.AutoEntry = "CCD7与CCD5关联";
                    break;
                case AutoProcess.CCD7与CCD6关联: // Căn chỉnh liên kết toạ độ CCD7 và CCD6
                    this.AutoEntry = "CCD7与CCD6关联";
                    break;
                default:
                    this.InitEntry = "StepExit";
                    this.AutoEntry = "StepEmpty";
                    break;
            }
            RegBit.UnloadGantryInitFinished.Set(false); // Bit: Đặt cờ hoàn tất khởi tạo Gantry dỡ liệu về false
            RegBit.UnloadInitSnapProcessDone.Set(false); // Bit: Đặt cờ hoàn tất chụp ảnh khay lúc khởi tạo về false
        }

       
        string InitStep = string.Empty;
        bool isCameraCalibOrTest = false;
        List<int> lstVacuumNG = new List<int>();

        /// <summary>
        /// Đưa toàn bộ 4 trục Z của 4 đầu hút gantry về vị trí an toàn (tránh va quẹt khi X/Y di chuyển)
        /// </summary>
        private bool AllAxisZGotoSecurityPosition()
        {
            bool bZ1 = AxisSingleTo(POINT.安全位, AXIS.下料取放吸嘴1Z轴); // Đưa Z1 về vị trí an toàn
            bool bZ2 = AxisSingleTo(POINT.安全位, AXIS.下料取放吸嘴2Z轴); // Đưa Z2 về vị trí an toàn
            bool bZ3 = AxisSingleTo(POINT.安全位, AXIS.下料取放吸嘴3Z轴); // Đưa Z3 về vị trí an toàn
            bool bZ4 = AxisSingleTo(POINT.安全位, AXIS.下料取放吸嘴4Z轴); // Đưa Z4 về vị trí an toàn
            WaitAxis(AXIS.下料取放吸嘴1Z轴); // Chờ trục Z1 dừng
            WaitAxis(AXIS.下料取放吸嘴2Z轴); // Chờ trục Z2 dừng
            WaitAxis(AXIS.下料取放吸嘴3Z轴); // Chờ trục Z3 dừng
            WaitAxis(AXIS.下料取放吸嘴4Z轴); // Chờ trục Z4 dừng
            return (bZ1 && bZ2 && bZ3 && bZ4);
        }

        /// <summary>
        /// Đưa toàn bộ 4 trục Z của 4 đầu hút gantry về vị trí an toàn hiệu chuẩn (Calibration Security Position)
        /// </summary>
        private bool AllAxisZGotoSecurityPositionCaib()
        {
            bool bZ1 = AxisSingleTo(POINT.标定安全位, AXIS.下料取放吸嘴1Z轴); // Đưa Z1 về an toàn hiệu chuẩn
            bool bZ2 = AxisSingleTo(POINT.标定安全位, AXIS.下料取放吸嘴2Z轴); // Đưa Z2 về an toàn hiệu chuẩn
            bool bZ3 = AxisSingleTo(POINT.标定安全位, AXIS.下料取放吸嘴3Z轴); // Đưa Z3 về an toàn hiệu chuẩn
            bool bZ4 = AxisSingleTo(POINT.标定安全位, AXIS.下料取放吸嘴4Z轴); // Đưa Z4 về an toàn hiệu chuẩn
            WaitAxis(AXIS.下料取放吸嘴1Z轴); // Chờ trục Z1 dừng
            WaitAxis(AXIS.下料取放吸嘴2Z轴); // Chờ trục Z2 dừng
            WaitAxis(AXIS.下料取放吸嘴3Z轴); // Chờ trục Z3 dừng
            WaitAxis(AXIS.下料取放吸嘴4Z轴); // Chờ trục Z4 dừng
            return (bZ1 && bZ2 && bZ3 && bZ4);
        }

        /// <summary>
        /// [Khởi tạo - InitRun] Chu trình khởi tạo trạm Gantry dỡ liệu:
        /// 1. Kết nối 3 camera (CCD5, CCD6, CCD7)
        /// 2. Bật Servo toàn bộ hệ trục (X, Y, Z1-4, U1-4)
        /// 3. Tắt thổi khí phá chân không
        /// 4. Về gốc Home trục Z1-4, sau đó Home X, Y và U1-4
        /// 5. Đưa trục về an toàn, phát tín hiệu an toàn cho 3 khoang khay A, B, C
        /// 6. Giao tiếp gửi mã loại khay sang Vision CCD7
        /// 7. Lần lượt quét chụp ảnh kiểm tra khay tồn tại lúc khởi tạo cho Bin A, Bin B, Bin C
        /// 8. Kiểm tra trạng thái chân không 4 đầu hút (xả hàng sót nếu có)
        /// 9. Khởi chạy luồng SwapController và hoàn tất khởi tạo.
        /// </summary>
        protected override string InitRun(string objStep)
        {
            InitStep = (string)objStep;
            switch (InitStep)
            {
                case StepEntry: // [Khởi tạo] Bắt đầu chu trình khởi tạo trạm Gantry dỡ liệu
                    isCameraCalibOrTest = !(Process == AutoProcess.空跑 || Process == AutoProcess.正常生产);
                    lstVacuumNG = new List<int>(); // Xoá danh sách đầu hút dính phôi sót
                    return "CameraConnect"; // Chuyển sang: Kết nối camera CCD5, CCD6, CCD7

                case "CameraConnect": // [Khởi tạo] Khởi tạo kết nối mạng TCP tới 3 camera thị giác
                    CCD5Camera = new TcpCommu_Camera() { TcpLink = TcpClientMgr.GetClient(4) }; // Camera CCD5 (Định vị đầu hút dưới 1-2)
                    CCD6Camera = new TcpCommu_Camera() { TcpLink = TcpClientMgr.GetClient(5) }; // Camera CCD6 (Định vị đầu hút dưới 3-4)
                    CCD7Camera = new TcpCommu_Camera() { TcpLink = TcpClientMgr.GetClient(6) }; // Camera CCD7 (Camera trên gantry quét khay)
                    if (Parameter.GetBit("UnloadVisionEnable") && !IsDryRun)
                    {
                        if (!CCD5Camera.Init(this))
                        {
                            Logger.Process(this.Name, "CCD5 connect failed");
                            return "CameraConnect"; // Lỗi kết nối CCD5, thử lại
                        }
                        if (!CCD6Camera.Init(this))
                        {
                            Logger.Process(this.Name, "CCD6 connect failed");
                            return "CameraConnect"; // Lỗi kết nối CCD6, thử lại
                        }
                        if (!CCD7Camera.Init(this))
                        {
                            Logger.Process(this.Name, "CCD7 connect failed");
                            return "CameraConnect"; // Lỗi kết nối CCD7, thử lại
                        }
                        Logger.Process(this.Name, "CCD567 connect successfully");
                        return "AxisServoOn"; // Chuyển sang: Bật Servo các trục
                    }
                    else
                        Logger.Process(Name, "Disable Unload Vision");
                    return "AxisServoOn"; // Chuyển sang: Bật Servo các trục

                case "AxisServoOn": // [Khởi tạo] Bật Servo toàn bộ hệ trục Gantry dỡ liệu
                    AXIS.下料取放X轴.ServoOn(true);      // Servo On: Trục X gantry
                    AXIS.下料取放Y轴.ServoOn(true);      // Servo On: Trục Y gantry
                    AXIS.下料取放吸嘴1Z轴.ServoOn(true);  // Servo On: Trục nâng hạ Z đầu hút 1
                    AXIS.下料取放吸嘴1U轴.ServoOn(true);  // Servo On: Trục xoay U đầu hút 1
                    AXIS.下料取放吸嘴2Z轴.ServoOn(true);  // Servo On: Trục nâng hạ Z đầu hút 2
                    AXIS.下料取放吸嘴2U轴.ServoOn(true);  // Servo On: Trục xoay U đầu hút 2
                    AXIS.下料取放吸嘴3Z轴.ServoOn(true);  // Servo On: Trục nâng hạ Z đầu hút 3
                    AXIS.下料取放吸嘴3U轴.ServoOn(true);  // Servo On: Trục xoay U đầu hút 3
                    AXIS.下料取放吸嘴4Z轴.ServoOn(true);  // Servo On: Trục nâng hạ Z đầu hút 4
                    AXIS.下料取放吸嘴4U轴.ServoOn(true);  // Servo On: Trục xoay U đầu hút 4
                    Delay(500); // Chờ servo kích hoạt ổn định
                    return "VaccumBlowOff"; // Chuyển sang: Tắt khí phá chân không

                case "VaccumBlowOff": // [Khởi tạo] Tắt van thổi phá chân không 4 đầu hút
                    //DO.下料站吸嘴1吸真空.Set(false);
                    //DO.下料站吸嘴2吸真空.Set(false);
                    //DO.下料站吸嘴3吸真空.Set(false);
                    //DO.下料站吸嘴4吸真空.Set(false);
                    DO.下料站吸嘴1破真空.Set(false); // DO: Tắt thổi phá chân không đầu hút 1
                    DO.下料站吸嘴2破真空.Set(false); // DO: Tắt thổi phá chân không đầu hút 2
                    DO.下料站吸嘴3破真空.Set(false); // DO: Tắt thổi phá chân không đầu hút 3
                    DO.下料站吸嘴4破真空.Set(false); // DO: Tắt thổi phá chân không đầu hút 4
                    Delay(200);
                    return "ZAxisHome"; // Chuyển sang: Về gốc Home 4 trục Z

                case "ZAxisHome": // [Khởi tạo] Về gốc Home 4 trục Z đầu hút lên cao an toàn trước
                    AXIS.下料取放吸嘴1Z轴.Home(); // Home trục Z đầu hút 1
                    AXIS.下料取放吸嘴2Z轴.Home(); // Home trục Z đầu hút 2
                    AXIS.下料取放吸嘴3Z轴.Home(); // Home trục Z đầu hút 3
                    AXIS.下料取放吸嘴4Z轴.Home(); // Home trục Z đầu hút 4
                    WaitAxis(AXIS.下料取放吸嘴1Z轴); // Chờ Z1 hoàn tất Home
                    WaitAxis(AXIS.下料取放吸嘴2Z轴); // Chờ Z2 hoàn tất Home
                    WaitAxis(AXIS.下料取放吸嘴3Z轴); // Chờ Z3 hoàn tất Home
                    WaitAxis(AXIS.下料取放吸嘴4Z轴); // Chờ Z4 hoàn tất Home
                    return "XYUAxisHome"; // Chuyển sang: Về gốc Home trục X, Y và U1-4

                case "XYUAxisHome": // [Khởi tạo] Về gốc Home trục phẳng X, Y và 4 trục xoay góc U
                    AXIS.下料取放X轴.Home();     // Home trục X gantry
                    AXIS.下料取放Y轴.Home();     // Home trục Y gantry
                    AXIS.下料取放吸嘴1U轴.Home(); // Home trục xoay U1
                    AXIS.下料取放吸嘴2U轴.Home(); // Home trục xoay U2
                    AXIS.下料取放吸嘴3U轴.Home(); // Home trục xoay U3
                    AXIS.下料取放吸嘴4U轴.Home(); // Home trục xoay U4
                    WaitAxis(AXIS.下料取放X轴);
                    WaitAxis(AXIS.下料取放Y轴);
                    WaitAxis(AXIS.下料取放吸嘴1U轴);
                    WaitAxis(AXIS.下料取放吸嘴2U轴);
                    WaitAxis(AXIS.下料取放吸嘴3U轴);
                    WaitAxis(AXIS.下料取放吸嘴4U轴);

                    // Xoay 4 trục U về góc mặc định theo vị trí lỗ khay tương ứng
                    AxisSingleTo(AXIS.下料取放吸嘴1U轴, POINT.A区吸嘴1取料对应的第1个穴位);
                    AxisSingleTo(AXIS.下料取放吸嘴2U轴, POINT.A区吸嘴1取料对应的第1个穴位);
                    AxisSingleTo(AXIS.下料取放吸嘴3U轴, POINT.A区吸嘴1取料对应的第1个穴位);
                    AxisSingleTo(AXIS.下料取放吸嘴4U轴, POINT.A区吸嘴1取料对应的第1个穴位);
                    WaitAxis(AXIS.下料取放吸嘴1U轴);
                    WaitAxis(AXIS.下料取放吸嘴2U轴);
                    WaitAxis(AXIS.下料取放吸嘴3U轴);
                    WaitAxis(AXIS.下料取放吸嘴4U轴);
                    if (isCameraCalibOrTest) // Nếu là chế độ Calib hoặc Test thì kết thúc sớm chu trình Init
                    {
                        return "InitializeFinished";
                    }
                    return "CheckVaccumSignal"; // Chuyển sang: Kiểm tra cảm biến chân không và đưa trục về an toàn

                case "CheckVaccumSignal": // [Khởi tạo] Đưa toàn bộ trục Z, X, Y về toạ độ an toàn
                    AllAxisZGotoSecurityPosition(); // Trục Z lên cao an toàn
                    AxisSingleTo(AXIS.下料取放X轴, POINT.安全位, AxisHandle.SpeedPercent); // Trục X về an toàn
                    AxisSingleTo(AXIS.下料取放Y轴, POINT.安全位, AxisHandle.SpeedPercent); // Trục Y về an toàn
                    WaitAxis(AXIS.下料取放X轴);
                    WaitAxis(AXIS.下料取放Y轴);
                    return "SendUnloadBinSafetySignal"; // Chuyển sang: Báo tín hiệu an toàn cho trạm khay

                case "SendUnloadBinSafetySignal": // [Khởi tạo] Bật cờ liên động báo gantry đã ở vị trí an toàn
                    RegBit.UnloadGantryInitSafety.Set(true);     // Bit: Báo Gantry ở vị trí an toàn cho trạm Bin nâng hạ
                    RegBit.UnloadInitSnapProcessDone.Set(false); // Bit: Reset cờ hoàn tất chụp ảnh khay lúc khởi tạo
                    if (IsDryRun)
                    {
                        return "WaitBinASnapRequest"; // Chế độ chạy không tải: Chờ yêu cầu chụp khay A
                    }
                    if (!Parameter.GetBit("UnloadVisionEnable"))
                    {
                        return "WaitBinASnapRequest"; // Tắt thị giác: Chờ yêu cầu chụp khay A
                    }
                    return "SendTrayTypeToVision"; // Chuyển sang: Gửi thông tin mã khay sang hệ thống thị giác

                case "SendTrayTypeToVision": // [Khởi tạo] Gửi cấu hình số hàng, cột của khay (TrayType) sang CCD7
                    string currentTrayType = Global.trayTotalRow.ToString() + "_" + Global.trayTotalCol.ToString();
                    CCD7Camera.SendCommand(TcpCommu_Camera.Command.料盘型号, currentTrayType); // TCP: Gửi lệnh loại khay tới CCD7
                    if (!Parameter.GetBit("UnloadVisionEnable"))
                    {
                        return "WaitBinASnapRequest";
                    }
                    return "WaitVisionResult"; // Chuyển sang: Chờ phản hồi từ thị giác

                case "WaitVisionResult": // [Khởi tạo] Chờ nhận kết quả phản hồi loại khay từ CCD7
                    Delay(300);
                    if (!CCD7Camera.WaitReply(TcpCommu_Camera.Command.料盘型号, out _))
                    {
                        ShowMessage("300ms没有收到视觉反馈料盘型号的结果".txtTrans(TransType.Trans), this.Name, new string[] { "重新获取".txtTrans(TransType.Trans) });
                        return "SendTrayTypeToVision"; // Quá 300ms không nhận được phản hồi, gửi lại
                    }
                    return "JudgeVisionResult"; // Chuyển sang: Phán đoán kết quả thị giác phản hồi

                case "JudgeVisionResult": // [Khởi tạo] Kiểm tra kết quả phản hồi loại khay từ thị giác có trùng khớp không
                    string currentTrayTypeRes = Global.trayTotalRow.ToString() + "_" + Global.trayTotalCol.ToString();
                    if (CCD7Camera.trayTypeFromVision == currentTrayTypeRes)
                    {
                        CCD7Camera.trayTypeFromVision = string.Empty;
                        Logger.Process(this.Name, $"Result from Vision {CCD7Camera.trayTypeFromVision}");
                        return "WaitBinASnapRequest"; // Khớp thông số -> Chuyển sang chờ yêu cầu chụp ảnh khoang A
                    }
                    else
                    {
                        ShowMessage($"视觉反馈料盘信息与发送的不一致,{CCD7Camera.trayTypeFromVision}".txtTrans(TransType.Trans), "重新获取".txtTrans(TransType.Trans));
                        Logger.Process(this.Name, $"Result from Vision error.");
                        return "SendTrayTypeToVision"; // Thông số không khớp, gửi lại
                    }

                case "WaitBinASnapRequest": // [Khởi tạo] Chờ trạm khay A gửi yêu cầu định vị/kiểm tra khay A
                    if (RegBit.BinAInitSnapRequest.Get()) // Bit: Trạm Bin A yêu cầu chụp ảnh lúc khởi tạo
                    {
                        RegBit.BinAInitSnapRequest.Set(false); // Xoá cờ yêu cầu
                        return "BinAInitSnapProcess"; // Chuyển sang: Thực hiện chụp ảnh khay khoang A
                    }
                    return "WaitBinASnapRequest"; // Tiếp tục chờ tín hiệu

                case "BinAInitSnapProcess": // [Khởi tạo] Gantry di chuyển CCD7 chụp ảnh kiểm tra khay tại khoang A
                    bool bRet = true;
                    if (!GantryGoUpCamSnap(SwapController.binA, 1, true)) // CCD7 chụp ảnh khay khoang A (bIsCheckTray = true)
                    {
                        bRet = false;
                    }

                    if (!bRet)
                    {
                        ShowMessage($"CCD7拍照料仓A失败，请检查".txtTrans(TransType.Trans));
                        return "BinAInitSnapProcess"; // Chụp lỗi, thử lại
                    }
                    else
                    {
                        RegBit.BinAInitSnapFinish.Set(true); // Bit: Hoàn tất chụp ảnh khay khoang A lúc khởi tạo
                        if (CCD7Camera.CCD7HaveProduct)
                            RegBit.BinAInitSnapHaveTray.Set(true); // Bit: Xác nhận có khay tại khoang A
                        return "WaitBinBSnapRequest"; // Chuyển sang: Chờ yêu cầu chụp khay khoang B
                    }

                case "WaitBinBSnapRequest": // [Khởi tạo] Chờ trạm khay B gửi yêu cầu định vị/kiểm tra khay B
                    if (RegBit.BinBInitSnapRequest.Get()) // Bit: Trạm Bin B yêu cầu chụp ảnh lúc khởi tạo
                    {
                        RegBit.BinBInitSnapRequest.Set(false); // Xoá cờ yêu cầu
                        return "BinBInitSnapProcess"; // Chuyển sang: Thực hiện chụp ảnh khay khoang B
                    }
                    return "WaitBinBSnapRequest"; // Tiếp tục chờ tín hiệu

                case "BinBInitSnapProcess": // [Khởi tạo] Gantry di chuyển CCD7 chụp ảnh kiểm tra khay tại khoang B
                    bRet = true;
                    if (!GantryGoUpCamSnap(SwapController.binB, 1, true)) // CCD7 chụp ảnh khay khoang B
                    {
                        bRet = false;
                    }

                    if (!bRet)
                    {
                        ShowMessage($"CCD7拍照料仓B失败，请检查".txtTrans(TransType.Trans));
                        return "BinBInitSnapProcess"; // Chụp lỗi, thử lại
                    }
                    else
                    {
                        RegBit.BinBInitSnapFinish.Set(true); // Bit: Hoàn tất chụp ảnh khay khoang B lúc khởi tạo
                        if (CCD7Camera.CCD7HaveProduct)
                            RegBit.BinBInitSnapHaveTray.Set(true); // Bit: Xác nhận có khay tại khoang B
                        return "WaitBinCSnapRequest"; // Chuyển sang: Chờ yêu cầu chụp khay khoang C
                    }

                case "WaitBinCSnapRequest": // [Khởi tạo] Chờ trạm khay C gửi yêu cầu định vị/kiểm tra khay C
                    if (RegBit.BinCInitSnapRequest.Get()) // Bit: Trạm Bin C yêu cầu chụp ảnh lúc khởi tạo
                    {
                        RegBit.BinCInitSnapRequest.Set(false); // Xoá cờ yêu cầu
                        return "BinCInitSnapProcess"; // Chuyển sang: Thực hiện chụp ảnh khay khoang C
                    }
                    return "WaitBinCSnapRequest"; // Tiếp tục chờ tín hiệu

                case "BinCInitSnapProcess": // [Khởi tạo] Gantry di chuyển CCD7 chụp ảnh kiểm tra khay tại khoang C
                    bRet = true;
                    if (!GantryGoUpCamSnap(SwapController.binC, 1, true)) // CCD7 chụp ảnh khay khoang C
                    {
                        bRet = false;
                    }

                    if (!bRet)
                    {
                        ShowMessage($"CCD7拍照料仓C失败，请检查".txtTrans(TransType.Trans));
                        return "BinCInitSnapProcess"; // Chụp lỗi, thử lại
                    }
                    else
                    {
                        RegBit.BinCInitSnapFinish.Set(true); // Bit: Hoàn tất chụp ảnh khay khoang C lúc khởi tạo
                        if (CCD7Camera.CCD7HaveProduct)
                            RegBit.BinCInitSnapHaveTray.Set(true); // Bit: Xác nhận có khay tại khoang C
                        return "SendInitSnapFinishSignal"; // Chuyển sang: Gửi tín hiệu hoàn tất chụp ảnh cả 3 khoang
                    }

                case "SendInitSnapFinishSignal": // [Khởi tạo] Bật cờ hoàn tất chu trình chụp ảnh khởi tạo cả 3 khoang khay
                    RegBit.UnloadInitSnapProcessDone.Set(true); // Bit: Đã chụp ảnh xong toàn bộ khay lúc khởi tạo
                    return "AxisGoToSafePos"; // Chuyển sang: Đưa các trục về vị trí an toàn

                case "AxisGoToSafePos": // [Khởi tạo] Đưa toàn bộ trục Z, X, Y về toạ độ an toàn sau khi chụp xong
                    AllAxisZGotoSecurityPosition(); // Trục Z lên an toàn
                    RegBit.UnloadGantryInitSafety.Set(false); // Tắt cờ an toàn tạm thời
                    AxisSingleTo(AXIS.下料取放X轴, POINT.安全位, AxisHandle.SpeedPercent); // X về an toàn
                    AxisSingleTo(AXIS.下料取放Y轴, POINT.安全位, AxisHandle.SpeedPercent); // Y về an toàn
                    WaitAxis(AXIS.下料取放X轴);
                    WaitAxis(AXIS.下料取放Y轴);
                    return "GetVaccumStatus"; // Chuyển sang: Kiểm tra cảm biến chân không các đầu hút

                case "GetVaccumStatus": // [Khởi tạo] Đọc cảm biến áp suất chân không xem có đầu hút nào còn dính phôi sót không
                    if (DI.下料站吸嘴1真空到位.Get()) // DI: Đầu hút 1 còn phôi
                        lstVacuumNG.Add(1);

                    if (DI.下料站吸嘴2真空到位.Get()) // DI: Đầu hút 2 còn phôi
                        lstVacuumNG.Add(2);

                    if (DI.下料站吸嘴3真空到位.Get()) // DI: Đầu hút 3 còn phôi
                        lstVacuumNG.Add(3);

                    if (DI.下料站吸嘴4真空到位.Get()) // DI: Đầu hút 4 còn phôi
                        lstVacuumNG.Add(4);

                    if (lstVacuumNG.Count > 0)
                    {
                        return "GotoPlaceProductToSlot"; // Có phôi sót -> chuyển sang xả phôi vào lỗ khay trống
                        //ShowMessage($"请把吸嘴{string.Join(",", lstVacuumNG)}上的物料拿走".txtTrans(TransType.Trans));
                        //return "CheckVaccumSignal";
                    }

                    return "StartSwapThread"; // Cả 4 đầu hút đều sạch -> khởi chạy luồng SwapController

                case "GotoPlaceProductToSlot": // [Khởi tạo] Đặt phôi tồn đọng trên đầu hút vào vị trí lỗ khay trống (NA)
                    (BinInfo, int) targetBinAndSlotNG;
                    for (int i = 0; i < lstVacuumNG.Count; i++)
                    {
                        targetBinAndSlotNG = GetSlotNA(lstVacuumNG[i]); // Lấy đối tượng khay và số vị trí lỗ trống NA
                        if (targetBinAndSlotNG.Item1 == null)
                        {
                            ShowMessage($"吸嘴{string.Join(",", lstVacuumNG)}上物料无处可放，请处理", "GotoPlaceProductToSlot", new string[] { "已处理" });
                        }
                        else
                        {
                            // PlaceProductToNASlot(targetBinAndSlotNG.Item2, i, ref targetBinAndSlotNG.Item1);
                        }
                    }

                    return "StartSwapThread"; // Chuyển sang: Khởi chạy luồng SwapController

                case "StartSwapThread": // [Khởi tạo] Kích hoạt tiến trình SwapController quản lý tráo đổi khay
                    SwapController.Start();
                    return "InitializeFinished"; // Chuyển sang: Hoàn tất khởi tạo

                case "InitializeFinished": // [Khởi tạo] Tắt các ngõ ra chụp ảnh và hoàn tất toàn bộ chu trình khởi tạo trạm
                    DO.CCD5下料定位拍照.Set(false); // DO: Tắt ngõ ra chụp định vị CCD5
                    DO.CCD6下料定位拍照.Set(false); // DO: Tắt ngõ ra chụp định vị CCD6
                    DO.CCD7料盘定位拍照.Set(false); // DO: Tắt ngõ ra chụp định vị CCD7
                    RegBit.UnloadGantryInitFinished.Set(true); // Bit: Hoàn tất khởi tạo trạm Gantry dỡ liệu
                    // Kết thúc khởi tạo trạm thành công
                    return "StepExit";
                default:
                    return null;
            }
        }  
        /// <summary>
        /// 相机硬触发触发延时关闭时间
        /// </summary>
        int CameraTriggerTimeClosed = 200;
        /// <summary>
        /// 等待视觉结果延时
        /// </summary>
           
        string ProduceRunStepName = string.Empty;   
        /// <summary>
        /// [Vận hành sản xuất - ProduceRun] Chu trình hoạt động tự động của trạm Gantry dỡ liệu:
        /// 1. Đưa toàn bộ trục Z lên vị trí an toàn, chờ các lệnh điều phối:
        ///    - Yêu cầu chụp ảnh định vị khay tại khoang A, B hoặc C (CCD7 quét 4 điểm góc khay).
        ///    - Yêu cầu thực hiện nhiệm vụ đổi/sắp xếp hàng (SwapStack task) từ SwapController:
        ///      + Dùng đầu hút 3, 4 gắp phôi OK từ khay nguồn (khay lỗi có lẫn hàng OK).
        ///      + Qua camera CCD5/CCD6 chụp bù tâm góc xoay cho đầu hút 3, 4.
        ///      + Dùng đầu hút 1, 2 gắp phôi NG từ khay đích (khay OK cần gom phôi lỗi ra ngoài).
        ///      + Tìm ô trống và thả phôi OK vào khay đích bằng đầu hút 3, 4.
        ///      + Qua camera CCD5/CCD6 chụp bù tâm cho đầu hút 1, 2.
        ///      + Thả phôi NG vào khay nguồn bằng đầu hút 1, 2.
        ///      + Cập nhật ma trận dữ liệu khay và giải phóng bộ nhớ Stack hoàn tất chu trình.
        /// </summary>
        protected override string ProduceRun(string strStep)
        {
            switch (strStep)
            {
                case StepEntry: // [Sản xuất] Bắt đầu chu trình tự động
                    return "AllZAxisGoSafePos"; // Chuyển sang: Đưa tất cả trục Z lên an toàn

                case "AllZAxisGoSafePos": // [Sản xuất] Toàn bộ 4 trục Z lên vị trí an toàn tránh va chạm
                    AllAxisZGotoSecurityPosition();
                    return "WaitCommand"; // Chuyển sang: Chờ lệnh điều phối sản xuất

                case "WaitCommand": // [Sản xuất] Chờ lệnh định vị khay hoặc nhiệm vụ tráo đổi phôi
                    if (RegBit.BinARequestGantryLocateTray.Get()) // Bit: Khoang khay A yêu cầu Gantry định vị khay
                    {
                        RegBit.BinARequestGantryLocateTray.Set(false); // Xoá cờ yêu cầu
                        return "UnloadBinASnapProcess"; // Chuyển sang: Chụp định vị khay khoang A
                    }
                    if (RegBit.BinBRequestGantryLocateTray.Get()) // Bit: Khoang khay B yêu cầu Gantry định vị khay
                    {
                        RegBit.BinBRequestGantryLocateTray.Set(false); // Xoá cờ yêu cầu
                        return "UnloadBinBSnapProcess"; // Chuyển sang: Chụp định vị khay khoang B
                    }
                    if (RegBit.BinCRequestGantryLocateTray.Get()) // Bit: Khoang khay C yêu cầu Gantry định vị khay
                    {
                        RegBit.BinCRequestGantryLocateTray.Set(false); // Xoá cờ yêu cầu
                        return "UnloadBinCSnapProcess"; // Chuyển sang: Chụp định vị khay khoang C
                    }
                    if (SwapController.TryGetSwapStack(out SwapStack workStack)) // Lấy nhiệm vụ tráo phôi từ hàng đợi SwapController
                    {                     
                        curStack = workStack;
                        //SwapController.ShowLog($"DEQUEUE {workStack.Info}, Queue={SwapController.swapQueue.Count}");
                        SwapController.ShowLog($"{SwapController.CurrentPair.PairName}_STACK{SwapController.CurrentPair.StackCount} START...", LogType.Warn);
                        return "SwapProductProcess"; // Chuyển sang: Tiến hành quy trình tráo đổi phôi
                    }
                    if (SwapController.UnloadMode == 0)
                    {
                        if (SwapController.b_DisableToPair) // Cảnh báo 3 khoang không thể ghép đôi, yêu cầu hạ khay B
                        {
                            if (0 == ShowMessage("3料仓不能换料给1盘料可以满，需要下B仓料盘".txtTrans(TransType.Trans), "警告".txtTrans(TransType.Trans), new string[] { "下B仓料盘".txtTrans(TransType.Trans), "继续等待".txtTrans(TransType.Trans) }))
                            {

                                if (SwapController.binB != null)
                                {
                                    AllAxisZGotoSecurityPosition();
                                    SwapController.binB.SetStatus(BinStatus.ProcessCompleted); // Đánh dấu khay B hoàn tất để hạ dỡ khay
                                }
                                Logger.Process(this.Name, "B仓不满盘下料");
                                SwapController.b_DisableToPair = false;
                            }
                        }
                    }
                   
                    return "WaitCommand"; // Tiếp tục chờ lệnh

                case "UnloadBinASnapProcess": // [Sản xuất] Camera CCD7 chụp ảnh 4 điểm góc khay tại khoang A
                    bool bRet = true;
                    for (int i = 1; i <= 4; i++)
                    {
                        if (!GantryGoUpCamSnap(SwapController.binA, i)) // Chụp ảnh điểm góc i (1-4) của khay khoang A
                        {
                            bRet = false;
                            ShowMessage($"CCD7在A{i}拍照失败,是否继续拍照?".txtTrans(TransType.Trans), "UnloadBinASnapProcess", new string[] { "重新拍照".txtTrans(TransType.Trans) });
                            return "UnloadBinASnapProcess"; // Chụp thất bại, chụp lại
                        }
                          
                    }
                    RegBit.UnloadGantrySnapAOK.Set(bRet);     // Bit: Kết quả chụp định vị khay khoang A thành công
                    RegBit.UnloadGantrySnapAFinish.Set(true); // Bit: Hoàn tất quá trình chụp khay khoang A
                    return "EndProcess"; // Chuyển sang: Kết thúc lượt lệnh

                case "UnloadBinBSnapProcess": // [Sản xuất] Camera CCD7 chụp ảnh 4 điểm góc khay tại khoang B
                    bRet = true;
                    for (int i = 1; i <= 4; i++)
                    {
                        if (!GantryGoUpCamSnap(SwapController.binB, i)) // Chụp ảnh điểm góc i (1-4) của khay khoang B
                        {
                            bRet = false;
                            ShowMessage($"CCD7在B{i}拍照失败,是否继续拍照?".txtTrans(TransType.Trans), "UnloadBinBSnapProcess", new string[] { "重新拍照".txtTrans(TransType.Trans) });
                            return "UnloadBinBSnapProcess"; // Chụp thất bại, chụp lại
                        }
                    }
                    RegBit.UnloadGantrySnapBOK.Set(bRet);     // Bit: Kết quả chụp định vị khay khoang B thành công
                    RegBit.UnloadGantrySnapBFinish.Set(true); // Bit: Hoàn tất quá trình chụp khay khoang B
                    return "EndProcess"; // Chuyển sang: Kết thúc lượt lệnh

                case "UnloadBinCSnapProcess": // [Sản xuất] Camera CCD7 chụp ảnh 4 điểm góc khay tại khoang C
                    bRet = true;
                    for (int i = 1; i <= 4; i++)
                    {
                        if (!GantryGoUpCamSnap(SwapController.binC, i)) // Chụp ảnh điểm góc i (1-4) của khay khoang C
                        {
                            bRet = false;
                            ShowMessage($"CCD7在C{i}拍照失败,是否继续拍照?".txtTrans(TransType.Trans), "UnloadBinCSnapProcess", new string[] { "重新拍照".txtTrans(TransType.Trans) });
                            return "UnloadBinCSnapProcess"; // Chụp thất bại, chụp lại
                        }
                    }
                    RegBit.UnloadGantrySnapCOK.Set(bRet);     // Bit: Kết quả chụp định vị khay khoang C thành công
                    RegBit.UnloadGantrySnapCFinish.Set(true); // Bit: Hoàn tất quá trình chụp khay khoang C
                    return "EndProcess"; // Chuyển sang: Kết thúc lượt lệnh


                case "SwapProductProcess": // [Sản xuất] Bắt đầu quy trình tráo đổi phôi giữa 2 khay
                    Logger.Process(this.Name, "PrePick: " + curStack.Info);
                    //ProcessSwapStack(curStack);
                    return "GetPickCoordinate"; // Chuyển sang: Lấy toạ độ gắp phôi

                case "GetPickCoordinate": // [Sản xuất] Tính toán toạ độ vị trí gắp trên khay nguồn
                    if (GetPickCoordinate(curStack))
                        return "GantryPickOKFromSourceTray"; // Toạ độ hợp lệ -> chuyển sang gắp phôi OK từ khay nguồn
                    else
                        return "GetPickCoordinate"; // Thử lại

                case "GantryPickOKFromSourceTray": // [Sản xuất] Gantry dùng đầu hút 3 và 4 gắp phôi OK từ khay nguồn
                    foreach (var task in curStack.Tasks())
                    {
                        if (task.VacuumId == 3 || task.VacuumId == 4) // Đầu hút 3 và 4 gắp hàng OK
                            GantryGoSinglePick(task);
                    }
                    //Delay(50);
                    return "UpdatePickSourceTrayData"; // Chuyển sang: Cập nhật dữ liệu khay nguồn

                case "UpdatePickSourceTrayData": // [Sản xuất] Cập nhật trạng thái ô khay nguồn đã được gắp đi
                    curStack.ApplyPick(false);
                    return "GoCCD5orCCD6Snap34"; // Chuyển sang: Chụp bù tâm đầu hút 3, 4 qua CCD5/CCD6

                case "GoCCD5orCCD6Snap34": // [Sản xuất] Di chuyển đầu hút 3 và 4 qua camera dưới (CCD5/CCD6) chụp bù toạ độ
                    if (GantryGoCCD5orCCD6Snap(curStack, false))
                        return "GantryPickNGFromTargetTray"; // Chụp xong -> chuyển sang gắp phôi NG từ khay đích
                    else
                        return "GoCCD5orCCD6Snap34"; // Lỗi chụp, thực hiện lại

                case "GantryPickNGFromTargetTray": // [Sản xuất] Gantry dùng đầu hút 1 và 2 gắp phôi NG từ khay đích
                    foreach (var task in curStack.Tasks())
                    {
                        if (task.VacuumId == 1 || task.VacuumId == 2) // Đầu hút 1 và 2 gắp hàng NG
                            GantryGoSinglePick(task);
                    }
                    //Delay(50);
                    return "UpdatePickTargetTrayData"; // Chuyển sang: Cập nhật dữ liệu khay đích

                case "UpdatePickTargetTrayData": // [Sản xuất] Cập nhật trạng thái ô khay đích đã được gắp phôi NG ra
                    curStack.ApplyPick(true);
                    return "PrepareToPlaceOKToTargetTray"; // Chuyển sang: Chuẩn bị vị trí thả hàng OK vào khay đích

                case "PrepareToPlaceOKToTargetTray": // [Sản xuất] Tìm kiếm ô trống (NA) trên khay đích để chuẩn bị đặt hàng OK
                    SwapTrayHelper.PreparePlace(curStack, false); // Tìm ô trống trên khay đích (OK Tray)
                    return "GetPlaceOKToTargetCoordinate"; // Chuyển sang: Lấy toạ độ đặt hàng OK

                case "GetPlaceOKToTargetCoordinate": // [Sản xuất] Tính toán toạ độ đặt phôi OK vào khay đích
                    if (GetPlaceCoordinate(curStack, false))
                        return "GantryPlaceOKToTargetTray"; // Toạ độ hợp lệ -> chuyển sang thả hàng OK
                    else
                        return "GetPlaceOKToTargetCoordinate"; // Thử lại

                case "GantryPlaceOKToTargetTray": // [Sản xuất] Gantry dùng đầu hút 3 và 4 đặt phôi OK vào khay đích
                    foreach (var task in curStack.Tasks())
                    {
                        if (task.VacuumId == 3 || task.VacuumId == 4) // Đầu hút 3 và 4 thả hàng OK
                        {
                            GantryGoSinglePlace(task);
                        }                     
                    }
                    return "UpdatePlaceTargetTrayData"; // Chuyển sang: Cập nhật dữ liệu khay đích

                case "UpdatePlaceTargetTrayData": // [Sản xuất] Cập nhật ma trận khay đích đã điền đầy phôi OK vào ô trống
                    curStack.ApplyPlace(false);
                    //Delay(100);
                    return "GoCCD5orCCD6Snap12"; // Chuyển sang: Chụp bù tâm đầu hút 1, 2 qua CCD5/CCD6

                case "GoCCD5orCCD6Snap12": // [Sản xuất] Di chuyển đầu hút 1 và 2 qua camera dưới (CCD5/CCD6) chụp bù toạ độ
                    if (GantryGoCCD5orCCD6Snap(curStack, true))
                        return "PrepareToPlaceNGToSourceTray"; // Chụp xong -> chuyển sang chuẩn bị vị trí thả hàng NG
                    else
                        return "GoCCD5orCCD6Snap12"; // Lỗi chụp, thực hiện lại


                case "PrepareToPlaceNGToSourceTray": // [Sản xuất] Tìm ô trống (NA) trên khay nguồn để chuẩn bị đặt hàng NG
                    SwapTrayHelper.PreparePlace(curStack, true); // Tìm ô trống trên khay nguồn (NG Tray)
                    return "GetPlaceNGToSourceTrayCoordinate"; // Chuyển sang: Lấy toạ độ đặt hàng NG

                case "GetPlaceNGToSourceTrayCoordinate": // [Sản xuất] Tính toán toạ độ đặt phôi NG vào khay nguồn
                    if (GetPlaceCoordinate(curStack, true))
                        return "GantryPlaceNGToSourceTray"; // Toạ độ hợp lệ -> chuyển sang thả hàng NG
                    else
                        return "GetPlaceNGToSourceTrayCoordinate"; // Thử lại
                     
                case "GantryPlaceNGToSourceTray": // [Sản xuất] Gantry dùng đầu hút 1 và 2 đặt phôi NG vào khay nguồn
                    foreach (var task in curStack.Tasks())
                    {
                        if (task.VacuumId == 1 || task.VacuumId == 2) // Đầu hút 1 và 2 thả hàng NG
                        {
                            GantryGoSinglePlace(task);
                        }
                    }
                    return "UpdatePlaceSourceTrayData"; // Chuyển sang: Cập nhật dữ liệu khay nguồn

                case "UpdatePlaceSourceTrayData": // [Sản xuất] Cập nhật ma trận khay nguồn đã nhận phôi NG
                    curStack.ApplyPlace(true);
                    //Delay(100);
                    return "NotifyStackFinish"; // Chuyển sang: Báo hoàn tất lượt tráo đổi

                case "NotifyStackFinish": // [Sản xuất] Giải phóng khoá ô khay và thông báo cho bộ điều phối SwapController
                    curStack.ReleaseReservation(); // Giải phóng đặt chỗ các ô khay
                    SwapController.NotifyStackDone(curStack); // Thông báo cho SwapController đã hoàn tất một Stack nhiệm vụ
                    return "EndProcess"; // Chuyển sang: Kết thúc chu trình

                case "EndProcess": // [Sản xuất] Kết thúc một lượt lệnh, quay về trạng thái ban đầu
                    return StepEntry;
                default:
                    return null;
            }
        }


        private (BinInfo, int) GetSlotNA(int vacuumID)
        {
            (BinInfo, int) binAndSlot = (null, -1);
            SlotState[] binaState = SwapController.binA.GetAllSlotState();
            SlotState[] binbState = SwapController.binB.GetAllSlotState();
            SlotState[] bincState = SwapController.binC.GetAllSlotState();
            if (vacuumID<=2)
            {
                if (RegBit.BinBInitSnapHaveTray.Get())
                {
                    for (int i = 0; i < binbState.Length; i++)
                    {
                        if (binbState[i] == SlotState.NA)
                        {
                            return (SwapController.binB, i);
                        }
                    }
                }

                if (RegBit.BinCInitSnapHaveTray.Get())
                {
                    for (int i = 0; i < bincState.Length; i++)
                    {
                        if (bincState[i] == SlotState.NA)
                        {
                            return (SwapController.binC, i);
                        }
                    }
                }


                if (RegBit.BinAInitSnapHaveTray.Get())
                {
                    for (int i = 0; i < binaState.Length; i++)
                    {
                        if (binaState[i] == SlotState.NA)
                        {
                            return (SwapController.binA, i);
                        }
                    }
                }
            }
            else
            {
                if (RegBit.BinAInitSnapHaveTray.Get())
                {
                    for (int i = 0; i < binaState.Length; i++)
                    {
                        if (binaState[i] == SlotState.NA)
                        {
                            return (SwapController.binA, i);
                        }
                    }
                }
                if (RegBit.BinCInitSnapHaveTray.Get())
                {
                    for (int i = 0; i < bincState.Length; i++)
                    {
                        if (bincState[i] == SlotState.NA)
                        {
                            return (SwapController.binC, i);
                        }
                    }
                }
                if (RegBit.BinBInitSnapHaveTray.Get())
                {
                    for (int i = 0; i < binbState.Length; i++)
                    {
                        if (binbState[i] == SlotState.NA)
                        {
                            return (SwapController.binB, i);
                        }
                    }
                }
            }
            return binAndSlot;

        }


        private void PlaceProductToNASlot(int slotNA,int vacuumIndex,ref BinInfo bin)
        {
            AllAxisZGotoSecurityPosition();
            int enableVacuum1 = 9999;
            int enableVacuum2 = 9999;
            //去ccd5拍照
            POINT p;
            if (vacuumIndex <= 2)
            {
                p = POINT.CCD6吸嘴1和2拍照位;
                
            }
            else
            {
                p = POINT.CCD6吸嘴3和4拍照位;
            }
            AxisSingleTo(AXIS.下料取放X轴, p);
            AxisSingleTo(AXIS.下料取放Y轴, p);
            WaitAxis(AXIS.下料取放X轴);
            WaitAxis(AXIS.下料取放Y轴);
            AxisSingleTo(GetUAxisByVaccumIdx(vacuumIndex), p);
            WaitAxis(GetUAxisByVaccumIdx(vacuumIndex));
            AxisSingleTo(GetZAxisByVaccumIdx(vacuumIndex), p);
            WaitAxis(GetZAxisByVaccumIdx(vacuumIndex));
            //拍照
            AXIS axisTemp = AXIS.下料取放吸嘴1U轴;
            
            if (vacuumIndex == 1)
            {
                enableVacuum1 = 1;
                axisTemp = AXIS.下料取放吸嘴2U轴;
            }
            if(vacuumIndex == 2)
            {
                enableVacuum2 = 2;
                axisTemp = AXIS.下料取放吸嘴1U轴;
            }
            if(vacuumIndex == 3)
            {
                enableVacuum1 = 3;
                axisTemp = AXIS.下料取放吸嘴4U轴;
            }
            if (vacuumIndex == 4)
            {
                enableVacuum2 = 4;
                axisTemp = AXIS.下料取放吸嘴3U轴;
            }
            //
            Delay(100);
            CCD5Camera.SendCommand(TcpCommu_Camera.Command.CCD5下料定位拍照, enableVacuum1, enableVacuum2, AXIS.下料取放X轴.GetPos(), AXIS.下料取放Y轴.GetPos(), GetUAxisByVaccumIdx(vacuumIndex).GetPos(),
                AXIS.下料取放X轴.GetPos(), AXIS.下料取放Y轴.GetPos(), axisTemp.GetPos());

            DO.CCD5下料定位拍照.Set(true);
            Delay(200);
            DO.CCD5下料定位拍照.Set(false);
            Coordinate coorBase = CalculateCavityPosByBaseCavity(bin.strRegion, vacuumIndex, slotNA);
            Coordinate coorFromVision = new Coordinate();
            string refString = "";
            //等待结果
            if (CCD5Camera.WaitReply(TcpCommu_Camera.Command.CCD5下料定位拍照,out _))
            {
                if (enableVacuum1 != 9999)
                {
                    if (CCD5Camera.CCD5result1)
                    {
                        //检查视觉坐标是否超限   获取视觉放料坐标

                        if (vacuumIndex == 1)
                        {
                            CCD7Camera.Communicate(TcpCommu_Camera.Command.CCD7索要料盘放料结果对应CCD5, ref refString, slotNA+1, 9999, bin.strRegion, 1, 2);
                            coorFromVision.X = CCD7Camera.dbCCD7GetPlaceResultCCD5NG1[0];
                            coorFromVision.Y = CCD7Camera.dbCCD7GetPlaceResultCCD5NG1[1];
                            coorFromVision.U = CCD7Camera.dbCCD7GetPlaceResultCCD5NG1[2];

                        }
                        else if (vacuumIndex == 2)
                        {
                            CCD7Camera.Communicate(TcpCommu_Camera.Command.CCD7索要料盘放料结果对应CCD5, ref refString, 9999, slotNA + 1, bin.strRegion, 1, 2);
                            coorFromVision.X = CCD7Camera.dbCCD7GetPlaceResultCCD5NG2[0];
                            coorFromVision.Y = CCD7Camera.dbCCD7GetPlaceResultCCD5NG2[1];
                            coorFromVision.U = CCD7Camera.dbCCD7GetPlaceResultCCD5NG2[2];

                        }
                        else if (vacuumIndex == 3)
                        {
                            CCD7Camera.Communicate(TcpCommu_Camera.Command.CCD7索要料盘放料结果对应CCD5, ref refString, slotNA + 1, 9999, bin.strRegion, 3, 4);
                            coorFromVision.X = CCD7Camera.dbCCD7GetPlaceResultCCD5OK1[0];
                            coorFromVision.Y = CCD7Camera.dbCCD7GetPlaceResultCCD5OK1[1];
                            coorFromVision.U = CCD7Camera.dbCCD7GetPlaceResultCCD5OK1[2];

                        }
                        else if (vacuumIndex == 4)
                        {
                            CCD7Camera.Communicate(TcpCommu_Camera.Command.CCD7索要料盘放料结果对应CCD5, ref refString, 9999, slotNA + 1, bin.strRegion, 3, 4);
                            coorFromVision.X = CCD7Camera.dbCCD7GetPlaceResultCCD5OK2[0];
                            coorFromVision.Y = CCD7Camera.dbCCD7GetPlaceResultCCD5OK2[1];
                            coorFromVision.U = CCD7Camera.dbCCD7GetPlaceResultCCD5OK2[2];
                        }

                        string typeOutRange = string.Empty;
                        if (Math.Abs(coorBase.X - coorFromVision.X) > Parameter.GetDouble("XTolerance"))
                        {
                            typeOutRange += "X";
                        }
                        else if (Math.Abs(coorBase.Y - coorFromVision.Y) > Parameter.GetDouble("YTolerance"))
                        {
                            typeOutRange += "Y";
                        }
                        else if (Math.Abs(coorBase.U - coorFromVision.U) > Parameter.GetDouble("UTolerance"))
                        {
                            typeOutRange += "U";
                        }
                        if (!String.IsNullOrEmpty(typeOutRange))
                        {
                            if (0 == ShowMessage($"吸嘴{vacuumIndex},穴位{slotNA+1},放料:基准(".txtTrans(TransType.Trans) + $"{coorBase.X},{coorBase.Y},{coorBase.U})-" + "视觉(".txtTrans(TransType.Trans) + $"{coorFromVision.X.ToString("F3")},{coorFromVision.Y.ToString("F3")},{coorFromVision.U.ToString("F3")})", $"UZ{vacuumIndex}" + "超限".txtTrans(TransType.Trans), new string[] { "重新获取".txtTrans(TransType.Trans), "获取系统坐标".txtTrans(TransType.Trans) }))
                            {
                                return ;
                            }
                            else
                            {
                                coorFromVision.X = coorBase.X;
                                coorFromVision.Y = coorBase.Y;
                                coorFromVision.U = coorBase.U;
                                Logger.Process(this.Name,$"使用系统坐标放料X:{coorFromVision.X},Y:{coorFromVision.Y},U:{coorFromVision.U}");
                            }
                        }

                        AXIS.下料取放X轴.AbsMove(coorFromVision.X);
                        AXIS.下料取放Y轴.AbsMove(coorFromVision.Y);
                        GetUAxisByVaccumIdx(vacuumIndex).AbsMove(coorFromVision.U);
                        WaitAxis(AXIS.下料取放X轴);
                        WaitAxis(AXIS.下料取放Y轴);
                        WaitAxis(GetUAxisByVaccumIdx(vacuumIndex));
                        AxisSingleTo(GetZAxisByVaccumIdx(vacuumIndex), p);
                        WaitAxis(GetZAxisByVaccumIdx(vacuumIndex));
                        Delay(50);
                        if (vacuumIndex == 1)
                        {
                            DO.下料站吸嘴1吸真空.Set(false);
                            DO.下料站吸嘴1破真空.Set(true);
                            Delay(100);
                            DO.下料站吸嘴1破真空.Set(false);
                        }
                        else if (vacuumIndex == 2)
                        {
                            DO.下料站吸嘴2吸真空.Set(false);
                            DO.下料站吸嘴2破真空.Set(true);
                            Delay(100);
                            DO.下料站吸嘴2破真空.Set(false);
                        }
                        else if(vacuumIndex == 3)
                        {
                            DO.下料站吸嘴3吸真空.Set(false);
                            DO.下料站吸嘴3破真空.Set(true);
                            Delay(100);
                            DO.下料站吸嘴3破真空.Set(false);
                        }
                        else if(vacuumIndex == 4)
                        {
                            DO.下料站吸嘴4吸真空.Set(false);
                            DO.下料站吸嘴4破真空.Set(true);
                            Delay(100);
                            DO.下料站吸嘴4破真空.Set(false);
                        }
                        SlotState slotState = SlotState.OK;
                        if (vacuumIndex <= 2)
                        {
                            slotState = SlotState.NG;
                            
                            bin.SetSlotNGReason(slotNA, "9999_");
                        }
                        bin.SetSlotState(slotNA, slotState);
                        bin.SetSlotSN(slotNA, $"{slotNA + 1}" +DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                        bin.SetMatrixBySQL();
                        AllAxisZGotoSecurityPosition();
                    }
                    else
                    {
                        ShowMessage($"CCD5视觉反馈吸嘴结果{vacuumIndex}失败,请排查");
                    }
                }
                else
                {
                    if (CCD5Camera.CCD5result2)
                    {
                        if (vacuumIndex == 1)
                        {
                            CCD7Camera.Communicate(TcpCommu_Camera.Command.CCD7索要料盘放料结果对应CCD5, ref refString, slotNA + 1, 9999, bin.strRegion, 1, 2);
                            coorFromVision.X = CCD7Camera.dbCCD7GetPlaceResultCCD5NG1[0];
                            coorFromVision.Y = CCD7Camera.dbCCD7GetPlaceResultCCD5NG1[1];
                            coorFromVision.U = CCD7Camera.dbCCD7GetPlaceResultCCD5NG1[2];

                        }
                        else if (vacuumIndex == 2)
                        {
                            CCD7Camera.Communicate(TcpCommu_Camera.Command.CCD7索要料盘放料结果对应CCD5, ref refString, 9999, slotNA + 1, bin.strRegion, 1, 2);
                            coorFromVision.X = CCD7Camera.dbCCD7GetPlaceResultCCD5NG2[0];
                            coorFromVision.Y = CCD7Camera.dbCCD7GetPlaceResultCCD5NG2[1];
                            coorFromVision.U = CCD7Camera.dbCCD7GetPlaceResultCCD5NG2[2];

                        }
                        else if (vacuumIndex == 3)
                        {
                            CCD7Camera.Communicate(TcpCommu_Camera.Command.CCD7索要料盘放料结果对应CCD5, ref refString, slotNA + 1, 9999, bin.strRegion, 3, 4);
                            coorFromVision.X = CCD7Camera.dbCCD7GetPlaceResultCCD5OK1[0];
                            coorFromVision.Y = CCD7Camera.dbCCD7GetPlaceResultCCD5OK1[1];
                            coorFromVision.U = CCD7Camera.dbCCD7GetPlaceResultCCD5OK1[2];

                        }
                        else if (vacuumIndex == 4)
                        {
                            CCD7Camera.Communicate(TcpCommu_Camera.Command.CCD7索要料盘放料结果对应CCD5, ref refString, 9999, slotNA + 1, bin.strRegion, 3, 4);
                            coorFromVision.X = CCD7Camera.dbCCD7GetPlaceResultCCD5OK2[0];
                            coorFromVision.Y = CCD7Camera.dbCCD7GetPlaceResultCCD5OK2[1];
                            coorFromVision.U = CCD7Camera.dbCCD7GetPlaceResultCCD5OK2[2];
                        }

                        string typeOutRange = string.Empty;
                        if (Math.Abs(coorBase.X - coorFromVision.X) > Parameter.GetDouble("XTolerance"))
                        {
                            typeOutRange += "X";
                        }
                        else if (Math.Abs(coorBase.Y - coorFromVision.Y) > Parameter.GetDouble("YTolerance"))
                        {
                            typeOutRange += "Y";
                        }
                        else if (Math.Abs(coorBase.U - coorFromVision.U) > Parameter.GetDouble("UTolerance"))
                        {
                            typeOutRange += "U";
                        }
                        if (!String.IsNullOrEmpty(typeOutRange))
                        {
                            if (0 == ShowMessage($"吸嘴{vacuumIndex},穴位{slotNA + 1},放料:基准(".txtTrans(TransType.Trans) + $"{coorBase.X},{coorBase.Y},{coorBase.U})-" + "视觉(".txtTrans(TransType.Trans) + $"{coorFromVision.X.ToString("F3")},{coorFromVision.Y.ToString("F3")},{coorFromVision.U.ToString("F3")})", $"UZ{vacuumIndex}" + "超限".txtTrans(TransType.Trans), new string[] { "重新获取".txtTrans(TransType.Trans), "获取系统坐标".txtTrans(TransType.Trans) }))
                            {
                                return;
                            }
                            else
                            {
                                coorFromVision.X = coorBase.X;
                                coorFromVision.Y = coorBase.Y;
                                coorFromVision.U = coorBase.U;
                                Logger.Process(this.Name, $"使用系统坐标放料X:{coorFromVision.X},Y:{coorFromVision.Y},U:{coorFromVision.U}");
                            }
                        }

                        AXIS.下料取放X轴.AbsMove(coorFromVision.X);
                        AXIS.下料取放Y轴.AbsMove(coorFromVision.Y);
                        GetUAxisByVaccumIdx(vacuumIndex).AbsMove(coorFromVision.U);
                        WaitAxis(AXIS.下料取放X轴);
                        WaitAxis(AXIS.下料取放Y轴);
                        WaitAxis(GetUAxisByVaccumIdx(vacuumIndex));
                        AxisSingleTo(GetZAxisByVaccumIdx(vacuumIndex), p);
                        WaitAxis(GetZAxisByVaccumIdx(vacuumIndex));
                        Delay(50);
                        if (vacuumIndex == 1)
                        {
                            DO.下料站吸嘴1吸真空.Set(false);
                            DO.下料站吸嘴1破真空.Set(true);
                            Delay(100);
                            DO.下料站吸嘴1破真空.Set(false);
                        }
                        else if (vacuumIndex == 2)
                        {
                            DO.下料站吸嘴2吸真空.Set(false);
                            DO.下料站吸嘴2破真空.Set(true);
                            Delay(100);
                            DO.下料站吸嘴2破真空.Set(false);
                        }
                        else if (vacuumIndex == 3)
                        {
                            DO.下料站吸嘴3吸真空.Set(false);
                            DO.下料站吸嘴3破真空.Set(true);
                            Delay(100);
                            DO.下料站吸嘴3破真空.Set(false);
                        }
                        else if (vacuumIndex == 4)
                        {
                            DO.下料站吸嘴4吸真空.Set(false);
                            DO.下料站吸嘴4破真空.Set(true);
                            Delay(100);
                            DO.下料站吸嘴4破真空.Set(false);
                        }
                        SlotState slotState = SlotState.OK;
                        if (vacuumIndex <= 2)
                        {
                            slotState = SlotState.NG;

                            bin.SetSlotNGReason(slotNA, "9999_");
                        }
                        bin.SetSlotState(slotNA, slotState);
                        bin.SetSlotSN(slotNA, $"{slotNA + 1}" + DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                        bin.SetMatrixBySQL();
                        AllAxisZGotoSecurityPosition();
                    }
                    else
                    {
                        ShowMessage($"CCD5视觉反馈吸嘴结果{vacuumIndex}失败,请排查");
                    }
                }
            }
            else
            {
                ShowMessage($"CCD5拍吸嘴{vacuumIndex}失败,请排查");
            }

        }

        private bool GantryGoUpCamSnap(BinInfo binInfo, int position, bool bIsCheckTray = false)
        {
            AllAxisZGotoSecurityPosition();
            TcpCommu_Camera.Command command = TcpCommu_Camera.Command.CCD7料盘定位拍照;
            if(bIsCheckTray)
                command = TcpCommu_Camera.Command.CCD7CheckHaveTray;
            POINT snapPoint = GetSnapPointbyRegionAndIdx(binInfo.strRegion, position);
            AxisSingleTo(AXIS.下料取放X轴, snapPoint, AxisHandle.SpeedPercent);
            AxisSingleTo(AXIS.下料取放Y轴, snapPoint, AxisHandle.SpeedPercent);
            WaitAxis(AXIS.下料取放X轴);
            WaitAxis(AXIS.下料取放Y轴);
            //Delay(300);
            if (IsDryRun)
            {
                DO.CCD7料盘定位拍照.Set(true);
                Delay(CameraTriggerTimeClosed);
                DO.CCD7料盘定位拍照.Set(false);
                return true;
            }
            if (!Parameter.GetBit("UnloadVisionEnable"))
            {
                return true;
            }
            object[] parameters;
            if (bIsCheckTray)
            {
                parameters = new object[] { binInfo.iRegion};
            }
            else
            {
                parameters = new object[] { binInfo.iRegion, position, AXIS.下料取放X轴.GetPos(), AXIS.下料取放Y轴.GetPos() };
            }
            CCD7Camera.SendCommand(command, parameters);
            DO.CCD7料盘定位拍照.Set(true);
            Delay(CameraTriggerTimeClosed);
            DO.CCD7料盘定位拍照.Set(false);

            if (CCD7Camera.WaitReply(command, out string strOrgVision))
            {
                if (!bIsCheckTray)
                {
                    if (CCD7Camera.CCD7result)
                    {
                        if (position == 4)
                        {
                            string[] strAnalys = CCD7Camera.CCD7CavityRet.Split('_');
                            List<int> mismatchIdxs;
                            if (SwapTrayHelper.CompareMatrix(binInfo, strAnalys, out mismatchIdxs))
                            {
                                return true;
                            }
                            else
                            {
                                List<int> mismatchShow = new List<int>();
                                foreach (var item in mismatchIdxs)
                                {
                                    mismatchShow.Add(item + 1);
                                }
                                if (!Parameter.GetBit("UnloadFollowAqiu") && 0 == ShowMessage($"{binInfo.Name}穴位状态和视觉结果不匹配:{string.Join(",", mismatchShow)}", "穴位状态不匹配".txtTrans(TransType.Trans), new string[] { "重新拍照".txtTrans(TransType.Trans), "用视觉数据".txtTrans(TransType.Trans) }))
                                {
                                    return false;
                                }
                                else
                                {
                                    foreach (var index in mismatchIdxs)
                                    {
                                        SlotState state = strAnalys[index] == "1" ? SlotState.NG : SlotState.NA;
                                        binInfo.SetSlotState(index, state);
                                        Logger.Process(this.Name, $"Follow CCD7 set slot{index}={state}");
                                    }
                                    return true;
                                }
                            }
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {

                        Logger.Process(Name, strOrgVision);
                        return false;

                    }
                }
                else
                {
                    if (CCD7Camera.CCD7CheckTrayRet)
                    {
                        return true;
                    }
                    else
                    {

                        Logger.Process(Name, strOrgVision);
                        ShowMessage($"CCD7{command}拍照失败,是否重新拍照?".txtTrans(TransType.Trans), "GantryGoUpCamSnap", new string[] { "重新拍照".txtTrans(TransType.Trans) });
                        return false;

                    }
                }
            }
            else
            {
                ShowMessage($"CCD7在{binInfo.strRegion}{position}获取视觉结果失败，是否继续拍照".txtTrans(TransType.Trans), "GantryGoUpCamSnap", new string[] { "重新拍照".txtTrans(TransType.Trans) });
                return false;
            }
        }


        private bool DownCamSnap(bool bIsUZ12, bool bIsCCD5, params object[] parameters)
        {
            string vaccumName = bIsUZ12 ? "吸嘴12" : "吸嘴34";
            bool bRet1, bRet2;
            object vaccumDisable = 9999;
            TcpCommu_Camera camera = CCD5Camera;
            camera = bIsCCD5 ? CCD5Camera : CCD6Camera;
            TcpCommu_Camera.Command cmd = bIsCCD5 ? TcpCommu_Camera.Command.CCD5下料定位拍照 : TcpCommu_Camera.Command.CCD6下料定位拍照;
            DO snapDO = bIsCCD5 ? DO.CCD5下料定位拍照 : DO.CCD6下料定位拍照;
            int iRetry = 0;
            while (true)
            {
                if (IsDryRun)
                {
                    snapDO.Set(true);
                    Delay(CameraTriggerTimeClosed);
                    snapDO.Set(false);
                    return true;
                }
                if (!Parameter.GetBit("UnloadVisionEnable"))
                {
                    return true;
                }
                camera.SendCommand(cmd, parameters);
                snapDO.Set(true);
                Delay(CameraTriggerTimeClosed);
                snapDO.Set(false);
                Logger.Process(this.Name, $"硬触发：{snapDO.ToString()}，软触发：{cmd}");
                if (camera.WaitReply(cmd, out string strOrgVision))
                {

                    bRet1 = bIsCCD5 ? camera.CCD5result1 && parameters[0] != vaccumDisable : camera.CCD6result1 && parameters[0] != vaccumDisable;
                    bRet2 = bIsCCD5 ? camera.CCD5result2 && parameters[1] != vaccumDisable : camera.CCD6result2 && parameters[1] != vaccumDisable;
                    bool bRet = bRet1 && bRet2;
                    if (bRet)
                    {
                        return true;
                    }
                    else
                    {
                        if (bIsUZ12)
                            vaccumName = $"吸嘴" + (bRet1 ? "" : "1") + (bRet2 ? "" : "2");
                        else
                            vaccumName = $"吸嘴" + (bRet1 ? "" : "3") + (bRet2 ? "" : "4");
                        Logger.Process(Name, strOrgVision);
                        if (iRetry > 2)
                        {
                            ShowMessage($"{vaccumName}-{cmd}失败，是否继续拍照".txtTrans(TransType.Trans), "GantryGoDownCamSnap", new string[] { "重新拍照".txtTrans(TransType.Trans) });
                            return false;
                        }
                        else
                        {
                            iRetry++;
                            Delay(200);
                            continue;
                        }

                    }
                }
                else
                {
                    ShowMessage($"{vaccumName}-{cmd}获取视觉结果失败，是否继续拍照".txtTrans(TransType.Trans), "GantryGoDownCamSnap", new string[] { "重新拍照".txtTrans(TransType.Trans) });
                    return false;
                }            
            }
            
        }


        private POINT GetSnapPointbyRegionAndIdx(string region, int index)
        {
            try
            {
                string strPoint = $"CCD7拍照位{region}{index}";
                foreach (POINT item in Enum.GetValues(typeof(POINT)))
                {
                    if (item.ToString() == strPoint)
                    {
                        return item;
                    }
                }
                return POINT.CCD7拍照位A1;
            }
            catch (Exception)
            {
                throw;
            }
        }
        

        private bool GantryGoCCD5orCCD6Snap(SwapStack stack, bool bIsUZ12)
        {
            bool bRet1 = true;
            bool bRet2 = true;
            AllAxisZGotoSecurityPosition();
            POINT downSnapPoint;
            string CCDname = stack.bIsCCD5 ? "CCD5" : "CCD6";
            DO DOSnap = stack.bIsCCD5 ? DO.CCD5下料定位拍照 : DO.CCD6下料定位拍照;         
            TcpCommu_Camera camera = stack.bIsCCD5 ? CCD5Camera : CCD6Camera;
            TcpCommu_Camera.Command cmd = stack.bIsCCD5 ? TcpCommu_Camera.Command.CCD5下料定位拍照 : TcpCommu_Camera.Command.CCD6下料定位拍照;
            if (bIsUZ12)
            {
                if (stack.UZ1Work || stack.UZ2Work)
                {
                    downSnapPoint = stack.bIsCCD5 ? POINT.CCD5吸嘴1和2拍照位 : POINT.CCD6吸嘴1和2拍照位;
                    AxisSingleTo(AXIS.下料取放X轴, downSnapPoint, AxisHandle.SpeedPercent);
                    AxisSingleTo(AXIS.下料取放Y轴, downSnapPoint, AxisHandle.SpeedPercent);
                    AxisSingleTo(AXIS.下料取放吸嘴1U轴, downSnapPoint);
                    AxisSingleTo(AXIS.下料取放吸嘴2U轴, downSnapPoint);
                    WaitAxis(AXIS.下料取放X轴);
                    WaitAxis(AXIS.下料取放Y轴);
                    WaitAxis(AXIS.下料取放吸嘴1U轴);
                    WaitAxis(AXIS.下料取放吸嘴2U轴);
                    if (stack.UZ1Work)
                        AxisSingleTo(AXIS.下料取放吸嘴1Z轴, downSnapPoint);
                    if (stack.UZ2Work)
                        AxisSingleTo(AXIS.下料取放吸嘴2Z轴, downSnapPoint);
                    WaitAxis(AXIS.下料取放吸嘴1Z轴);
                    WaitAxis(AXIS.下料取放吸嘴2Z轴);

                    while (true)
                    {
                        if (IsDryRun)
                            break;
                        if (stack.UZ1Work && !DI.下料站吸嘴1真空到位.Get())
                        {
                            ShowMessage($"{CCDname}拍照前吸嘴1真空异常，检查是否掉料".txtTrans(TransType.Trans));
                        }
                        else if (stack.UZ2Work && !DI.下料站吸嘴2真空到位.Get())
                        {
                            ShowMessage($"{CCDname}拍照前吸嘴2真空异常，检查是否掉料".txtTrans(TransType.Trans));
                        }
                        else
                        {
                            break;
                        }
                    }

                    int Vacumm1Enable = stack.UZ1Work ? 1 : 9999;
                    int Vacumm2Enable = stack.UZ2Work ? 2 : 9999;
                    Delay(100);
                    bRet1 = DownCamSnap(true, stack.bIsCCD5, Vacumm1Enable, Vacumm2Enable, AXIS.下料取放X轴.GetPos(), AXIS.下料取放Y轴.GetPos(), AXIS.下料取放吸嘴1U轴.GetPos(), AXIS.下料取放X轴.GetPos(), AXIS.下料取放Y轴.GetPos(), AXIS.下料取放吸嘴2U轴.GetPos());
                    
                }
                else
                {
                    bRet1 = true;
                }
            }
            else
            {
                if (stack.UZ3Work || stack.UZ4Work)
                {
                    downSnapPoint = stack.bIsCCD5 ? POINT.CCD5吸嘴3和4拍照位 : POINT.CCD6吸嘴3和4拍照位;
                    AxisSingleTo(AXIS.下料取放X轴, downSnapPoint, AxisHandle.SpeedPercent);
                    AxisSingleTo(AXIS.下料取放Y轴, downSnapPoint, AxisHandle.SpeedPercent);
                    AxisSingleTo(AXIS.下料取放吸嘴3U轴, downSnapPoint);
                    AxisSingleTo(AXIS.下料取放吸嘴4U轴, downSnapPoint);
                    WaitAxis(AXIS.下料取放X轴);
                    WaitAxis(AXIS.下料取放Y轴);
                    WaitAxis(AXIS.下料取放吸嘴3U轴);
                    WaitAxis(AXIS.下料取放吸嘴4U轴);
                    if (stack.UZ3Work)
                        AxisSingleTo(AXIS.下料取放吸嘴3Z轴, downSnapPoint);
                    if (stack.UZ4Work)
                        AxisSingleTo(AXIS.下料取放吸嘴4Z轴, downSnapPoint);
                    WaitAxis(AXIS.下料取放吸嘴3Z轴);
                    WaitAxis(AXIS.下料取放吸嘴4Z轴);

                    while (true)
                    {
                        if (IsDryRun)
                            break;
                        if (stack.UZ3Work && !DI.下料站吸嘴3真空到位.Get())
                        {
                            ShowMessage($"{CCDname}拍照前吸嘴3真空异常，检查是否掉料".txtTrans(TransType.Trans));
                        }
                        else if (stack.UZ4Work && !DI.下料站吸嘴4真空到位.Get())
                        {
                            ShowMessage($"{CCDname}拍照前吸嘴4真空异常，检查是否掉料".txtTrans(TransType.Trans));
                        }
                        else
                        {
                            break;
                        }
                    }

                    int Vacumm3Enable = stack.UZ3Work ? 3 : 9999;
                    int Vacumm4Enable = stack.UZ4Work ? 4 : 9999;

                    Delay(100);
                    bRet2 = DownCamSnap(false, stack.bIsCCD5, Vacumm3Enable, Vacumm4Enable, AXIS.下料取放X轴.GetPos(), AXIS.下料取放Y轴.GetPos(), AXIS.下料取放吸嘴3U轴.GetPos(), AXIS.下料取放X轴.GetPos(), AXIS.下料取放Y轴.GetPos(), AXIS.下料取放吸嘴4U轴.GetPos());

                }
                else
                {
                    bRet2 = true;
                }
            }

            Delay(100);
            

            return bRet1 & bRet2;
        }
        private bool GetPickCoordinate(SwapStack stack)
        {
            bool bRet1 = true;
            bool bRet2 = true;
            int iSlot1, iSlot2, iSlot3, iSlot4;
            int iSourceRegion;
            string refString = "";
            string strSourceRegion;

            iSlot1 = stack.UZ1Work ? stack.UZ1.SourceSlot + 1 : 9999;
            iSlot2 = stack.UZ2Work ? stack.UZ2.SourceSlot + 1 : 9999;
            iSlot3 = stack.UZ3Work ? stack.UZ3.SourceSlot + 1 : 9999;
            iSlot4 = stack.UZ4Work ? stack.UZ4.SourceSlot + 1 : 9999;

            if (stack.UZ1Work || stack.UZ2Work)
            {
                iSourceRegion = stack.UZ1Work ? stack.UZ1.Source.iRegion : stack.UZ2.Source.iRegion;
                strSourceRegion = stack.UZ1Work ? stack.UZ1.Source.strRegion : stack.UZ2.Source.strRegion;
                   
                bRet1 = IsDryRun ? true : CCD7Camera.Communicate(TcpCommu_Camera.Command.CCD7索要料盘取料结果, ref refString, iSlot1, iSlot2, iSourceRegion, 1, 2);

                if (!bRet1)
                {
                    ShowMessage("吸嘴12-CCD7索要料盘取料结果失败，请检查！".txtTrans(TransType.Trans), "获取视觉坐标".txtTrans(TransType.Trans), new string[] { "重新获取".txtTrans(TransType.Trans) });
                    return false;
                }                      
                else
                {
                    if (stack.UZ1Work)
                    {
                        Coordinate coor = CalculateCavityPosByBaseCavity(strSourceRegion, 1, iSlot1);
                        stack.UZ1.PickX = IsDryRun ? coor.X : CCD7Camera.dbCCD7GetPickResultNG1[0];
                        stack.UZ1.PickY = IsDryRun ? coor.Y : CCD7Camera.dbCCD7GetPickResultNG1[1];
                        stack.UZ1.PickU = IsDryRun ? coor.U : CCD7Camera.dbCCD7GetPickResultNG1[2];
                        
                        if (!CheckOutOfRange(stack.UZ1, true, coor.X, coor.Y, coor.U))
                        {
                            return false;
                        }
                        SwapController.ShowLog($"UZ1PickCoordinate_{strSourceRegion}{stack.UZ1.SourceSlot + 1}: X={stack.UZ1.PickX.ToString("F3")},Y={stack.UZ1.PickY.ToString("F3")},U={stack.UZ1.PickU.ToString("F3")}");
                    }
                    if (stack.UZ2Work)
                    {
                        Coordinate coor = CalculateCavityPosByBaseCavity(strSourceRegion, 2, iSlot2);
                        stack.UZ2.PickX = IsDryRun ? coor.X : CCD7Camera.dbCCD7GetPickResultNG2[0];
                        stack.UZ2.PickY = IsDryRun ? coor.Y : CCD7Camera.dbCCD7GetPickResultNG2[1];
                        stack.UZ2.PickU = IsDryRun ? coor.U : CCD7Camera.dbCCD7GetPickResultNG2[2];

                        if (!CheckOutOfRange(stack.UZ2, true, coor.X, coor.Y, coor.U))
                        {
                            return false;
                        }
                        SwapController.ShowLog($"UZ2PickCoordinate_{strSourceRegion}{stack.UZ2.SourceSlot + 1}: X={stack.UZ2.PickX.ToString("F3")},Y={stack.UZ2.PickY.ToString("F3")},U={stack.UZ2.PickU.ToString("F3")}");
                    }
                }             
            }
            else
            {
                bRet1 = true;
            }

            if (stack.UZ3Work || stack.UZ4Work)
            {
                iSourceRegion = stack.UZ3Work ? stack.UZ3.Source.iRegion : stack.UZ4.Source.iRegion;
                strSourceRegion = stack.UZ3Work ? stack.UZ3.Source.strRegion : stack.UZ4.Source.strRegion;
                bRet2 = IsDryRun ? true : CCD7Camera.Communicate(TcpCommu_Camera.Command.CCD7索要料盘取料结果, ref refString, iSlot3, iSlot4, iSourceRegion, 3, 4);
                if (!bRet2)
                {
                    ShowMessage("吸嘴34-CCD7索要料盘取料结果失败，请检查！".txtTrans(TransType.Trans), "获取视觉坐标".txtTrans(TransType.Trans), new string[] { "重新获取".txtTrans(TransType.Trans) });
                    return false;
                }
                else
                {
                    if (stack.UZ3Work)
                    {
                        Coordinate coor = CalculateCavityPosByBaseCavity(strSourceRegion, 3, iSlot3);
                        stack.UZ3.PickX = IsDryRun ? coor.X : CCD7Camera.dbCCD7GetPickResultOK1[0];
                        stack.UZ3.PickY = IsDryRun ? coor.Y : CCD7Camera.dbCCD7GetPickResultOK1[1];
                        stack.UZ3.PickU = IsDryRun ? coor.U : CCD7Camera.dbCCD7GetPickResultOK1[2];

                        if (!CheckOutOfRange(stack.UZ3, true, coor.X, coor.Y, coor.U))
                        {
                            return false;
                        }
                        SwapController.ShowLog($"UZ3PickCoordinate_{strSourceRegion}{stack.UZ3.SourceSlot + 1}: X={stack.UZ3.PickX.ToString("F3")},Y={stack.UZ3.PickY.ToString("F3")},U={stack.UZ3.PickU.ToString("F3")}");
                    }
                    if (stack.UZ4Work)
                    {
                        Coordinate coor = CalculateCavityPosByBaseCavity(strSourceRegion, 4, iSlot4);
                        stack.UZ4.PickX = IsDryRun ? coor.X : CCD7Camera.dbCCD7GetPickResultOK2[0];
                        stack.UZ4.PickY = IsDryRun ? coor.Y : CCD7Camera.dbCCD7GetPickResultOK2[1];
                        stack.UZ4.PickU = IsDryRun ? coor.U : CCD7Camera.dbCCD7GetPickResultOK2[2];

                        if (!CheckOutOfRange(stack.UZ4, true, coor.X, coor.Y, coor.U))
                        {
                            return false;
                        }
                        SwapController.ShowLog($"UZ4PickCoordinate_{strSourceRegion}{stack.UZ4.SourceSlot + 1}: X={stack.UZ4.PickX.ToString("F3")},Y={stack.UZ4.PickY.ToString("F3")},U={stack.UZ4.PickU.ToString("F3")}");
                    }
                }             
            }
            else
            {
                bRet2 = true;
            }
            return bRet1 & bRet2;
        }

        private bool GetPlaceCoordinate(SwapStack stack, bool bIsUZ12)
        {
            bool bRet1 = true;
            bool bRet2 = true;
            int iSlot1, iSlot2, iSlot3, iSlot4;
            int iTargetRegion = 0;
            string strTargetRegion;
            string refString = "";

            iSlot1 = stack.UZ1Work ? stack.UZ1.TargetSlot + 1 : 9999;
            iSlot2 = stack.UZ2Work ? stack.UZ2.TargetSlot + 1 : 9999;
            iSlot3 = stack.UZ3Work ? stack.UZ3.TargetSlot + 1 : 9999;
            iSlot4 = stack.UZ4Work ? stack.UZ4.TargetSlot + 1 : 9999;
          

            bool b_IsCCD5 = stack.bIsCCD5;
            TcpCommu_Camera.Command cmd = b_IsCCD5 ? TcpCommu_Camera.Command.CCD7索要料盘放料结果对应CCD5 : TcpCommu_Camera.Command.CCD7索要料盘放料结果对应CCD6;
            if (!bIsUZ12)
            {
                if (stack.UZ3Work || stack.UZ4Work)
                {
                    iTargetRegion = stack.UZ3Work ? stack.UZ3.Target.iRegion : stack.UZ4.Target.iRegion;
                    strTargetRegion = stack.UZ3Work ? stack.UZ3.Target.strRegion : stack.UZ4.Target.strRegion;
                    bRet2 = IsDryRun ? true : CCD7Camera.Communicate(cmd, ref refString, iSlot3, iSlot4, iTargetRegion, 3, 4);
                    if (!bRet2)
                    {
                        ShowMessage($"吸嘴34-{cmd}失败，请检查！".txtTrans(TransType.Trans), "获取视觉坐标".txtTrans(TransType.Trans), new string[] { "重新获取".txtTrans(TransType.Trans) });
                        return false;
                    }
                    if (stack.UZ3Work)
                    {
                        Coordinate coor = CalculateCavityPosByBaseCavity(strTargetRegion, 3, iSlot3);
                        if (IsDryRun)
                        {                         
                            stack.UZ3.PlaceX = coor.X;
                            stack.UZ3.PlaceY = coor.Y;
                            stack.UZ3.PlaceU = coor.U;
                        }
                        else
                        {
                            stack.UZ3.PlaceX = b_IsCCD5 ? CCD7Camera.dbCCD7GetPlaceResultCCD5OK1[0] : CCD7Camera.dbCCD7GetPlaceResultCCD6OK1[0];
                            stack.UZ3.PlaceY = b_IsCCD5 ? CCD7Camera.dbCCD7GetPlaceResultCCD5OK1[1] : CCD7Camera.dbCCD7GetPlaceResultCCD6OK1[1];
                            stack.UZ3.PlaceU = b_IsCCD5 ? CCD7Camera.dbCCD7GetPlaceResultCCD5OK1[2] : CCD7Camera.dbCCD7GetPlaceResultCCD6OK1[2];
                        }
                        
                        if (!CheckOutOfRange(stack.UZ3, false, coor.X, coor.Y, coor.U))
                        {
                            return false;
                        }
                        SwapController.ShowLog($"UZ3PlaceCoordinate_{strTargetRegion}{stack.UZ3.TargetSlot + 1}: X={stack.UZ3.PlaceX.ToString("F3")},Y={stack.UZ3.PlaceY.ToString("F3")},U={stack.UZ3.PlaceU.ToString("F3")}");
                    }
                    if (stack.UZ4Work)
                    {
                        Coordinate coor = CalculateCavityPosByBaseCavity(strTargetRegion, 4, iSlot4);
                        if (IsDryRun)
                        {                  
                            stack.UZ4.PlaceX = coor.X;
                            stack.UZ4.PlaceY = coor.Y;
                            stack.UZ4.PlaceU = coor.U;
                        }
                        else
                        {
                            stack.UZ4.PlaceX = b_IsCCD5 ? CCD7Camera.dbCCD7GetPlaceResultCCD5OK2[0] : CCD7Camera.dbCCD7GetPlaceResultCCD6OK2[0];
                            stack.UZ4.PlaceY = b_IsCCD5 ? CCD7Camera.dbCCD7GetPlaceResultCCD5OK2[1] : CCD7Camera.dbCCD7GetPlaceResultCCD6OK2[1];
                            stack.UZ4.PlaceU = b_IsCCD5 ? CCD7Camera.dbCCD7GetPlaceResultCCD5OK2[2] : CCD7Camera.dbCCD7GetPlaceResultCCD6OK2[2];
                        }
                        
                        if (!CheckOutOfRange(stack.UZ4, false, coor.X, coor.Y, coor.U))
                        {
                            return false;
                        }
                        SwapController.ShowLog($"UZ4PlaceCoordinate_{strTargetRegion}{stack.UZ4.TargetSlot + 1}: X={stack.UZ4.PlaceX.ToString("F3")},Y={stack.UZ4.PlaceY.ToString("F3")},U={stack.UZ4.PlaceU.ToString("F3")}");
                    }
                }
                else
                {
                    bRet2 = true;
                }
            }
            else
            {
                if (stack.UZ1Work || stack.UZ2Work)
                {
                    iTargetRegion = stack.UZ1Work ? stack.UZ1.Target.iRegion : stack.UZ2.Target.iRegion;
                    strTargetRegion = stack.UZ1Work ? stack.UZ1.Target.strRegion : stack.UZ2.Target.strRegion;
                    bRet1 = IsDryRun ? true : CCD7Camera.Communicate(cmd, ref refString, iSlot1, iSlot2, iTargetRegion, 1, 2);
                    if (!bRet1)
                    {
                        ShowMessage($"吸嘴12-{cmd}失败，请检查！".txtTrans(TransType.Trans),"获取视觉坐标".txtTrans(TransType.Trans), new string[] {"重新获取".txtTrans(TransType.Trans)});
                        return false;
                    }
                    if (stack.UZ1Work)
                    {
                        Coordinate coor = CalculateCavityPosByBaseCavity(strTargetRegion, 1, iSlot1);
                        if (IsDryRun)
                        {                        
                            stack.UZ1.PlaceX = coor.X;
                            stack.UZ1.PlaceY = coor.Y;
                            stack.UZ1.PlaceU = coor.U;
                        }
                        else
                        {
                            stack.UZ1.PlaceX = b_IsCCD5 ? CCD7Camera.dbCCD7GetPlaceResultCCD5NG1[0] : CCD7Camera.dbCCD7GetPlaceResultCCD6NG1[0];
                            stack.UZ1.PlaceY = b_IsCCD5 ? CCD7Camera.dbCCD7GetPlaceResultCCD5NG1[1] : CCD7Camera.dbCCD7GetPlaceResultCCD6NG1[1];
                            stack.UZ1.PlaceU = b_IsCCD5 ? CCD7Camera.dbCCD7GetPlaceResultCCD5NG1[2] : CCD7Camera.dbCCD7GetPlaceResultCCD6NG1[2];
                        }
                       
                        if (!CheckOutOfRange(stack.UZ1, false, coor.X, coor.Y, coor.U))
                        {
                            return false;
                        }
                        SwapController.ShowLog($"UZ1PlaceCoordinate_{strTargetRegion}{stack.UZ1.TargetSlot + 1}: X={stack.UZ1.PlaceX.ToString("F3")},Y={stack.UZ1.PlaceY.ToString("F3")},U={stack.UZ1.PlaceU.ToString("F3")}");
                    }
                    if (stack.UZ2Work)
                    {
                        Coordinate coor = CalculateCavityPosByBaseCavity(strTargetRegion, 2, iSlot2);
                        if (IsDryRun)
                        {                           
                            stack.UZ2.PlaceX = coor.X;
                            stack.UZ2.PlaceY = coor.Y;
                            stack.UZ2.PlaceU = coor.U;
                        }
                        else
                        {
                            stack.UZ2.PlaceX = b_IsCCD5 ? CCD7Camera.dbCCD7GetPlaceResultCCD5NG2[0] : CCD7Camera.dbCCD7GetPlaceResultCCD6NG2[0];
                            stack.UZ2.PlaceY = b_IsCCD5 ? CCD7Camera.dbCCD7GetPlaceResultCCD5NG2[1] : CCD7Camera.dbCCD7GetPlaceResultCCD6NG2[1];
                            stack.UZ2.PlaceU = b_IsCCD5 ? CCD7Camera.dbCCD7GetPlaceResultCCD5NG2[2] : CCD7Camera.dbCCD7GetPlaceResultCCD6NG2[2];
                        }
                        
                        if (!CheckOutOfRange(stack.UZ2, false, coor.X, coor.Y, coor.U))
                        {
                            return false;
                        }
                        SwapController.ShowLog($"UZ2PlaceCoordinate_{strTargetRegion}{stack.UZ2.TargetSlot + 1}: X={stack.UZ2.PlaceX.ToString("F3")},Y={stack.UZ2.PlaceY.ToString("F3")},U={stack.UZ2.PlaceU.ToString("F3")}");
                    }
                }
                else
                {
                    bRet1 = true;
                }
            }
            return bRet1 & bRet2;
        }

        private bool CheckOutOfRange(double valueX, double valueY, double valueU, double baseX, double baseY, double baseU)
        {
            if (Math.Abs(valueX - baseX) > 5)
                return false;
            if (Math.Abs(valueY - baseY) > 5)
                return false;
            if (Math.Abs(valueU - baseU) > 5)
                return false;

            return true;
        }
        private bool CheckOutOfRange(VacuumTask task, bool bIsPick, double baseX, double baseY, double baseU)
        {
            if (task == null)
            {
                EventMgr.Error("Vaccum task invalid");
                return false;
            }
             
            double valueX = bIsPick ? task.PickX : task.PlaceX;
            double valueY = bIsPick ? task.PickY : task.PlaceY;
            double valueU = bIsPick ? task.PickU : task.PlaceU;
            if (Math.Abs(valueX - baseX) > Parameter.GetDouble("XTolerance") || Math.Abs(valueY - baseY) > Parameter.GetDouble("YTolerance") || Math.Abs(valueU - baseU) > Parameter.GetDouble("UTolerance"))
            {
                string typeOutRange = string.Empty;
                if (Math.Abs(valueX - baseX) > Parameter.GetDouble("XTolerance"))
                {
                    typeOutRange += "X";
                }
                else if(Math.Abs(valueY - baseY) > Parameter.GetDouble("YTolerance"))
                {
                    typeOutRange += "Y";
                }
                else if(Math.Abs(valueU - baseU) > Parameter.GetDouble("UTolerance"))
                {
                    typeOutRange += "U";
                }
                string strAction = bIsPick ? $"取料坐标{typeOutRange}超限" : $"放料坐标{typeOutRange}超限";
                if (0 == ShowMessage($"吸嘴{task.VacuumId},穴位{task.TargetSlot + 1},{strAction}:基准(".txtTrans(TransType.Trans) + $"{baseX},{baseY},{baseU})-" + "视觉(".txtTrans(TransType.Trans) + $"{valueX.ToString("F3")},{valueY.ToString("F3")},{valueU.ToString("F3")})", $"UZ{task.VacuumId}" + "超限".txtTrans(TransType.Trans), new string[] { "重新获取".txtTrans(TransType.Trans), "获取系统坐标".txtTrans(TransType.Trans) }))
                {
                    return false;
                }
                else
                {
                    if (bIsPick)
                    {
                        task.PickX = baseX;
                        task.PickY = baseY;
                        task.PickU = baseU;
                    }
                    else
                    {
                        task.PlaceX = baseX;
                        task.PlaceY = baseY;
                        task.PlaceU = baseU;
                    }
                    return true;
                }
            }
            else
            {
                return true;
            }       
        }

        private void GantryGoSinglePick(VacuumTask task)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            if (task == null)
            {
                EventMgr.Error("Vaccum task invalid");
                return;
            }
            AllAxisZGotoSecurityPosition();
            AXIS.下料取放X轴.AbsMove(task.PickX, AxisHandle.SpeedPercent);
            AXIS.下料取放Y轴.AbsMove(task.PickY, AxisHandle.SpeedPercent);
            AXIS UAxis = GetUAxisByVaccumIdx(task.VacuumId);
            UAxis.AbsMove(task.PickU, AxisHandle.SpeedPercent);
            WaitAxis(AXIS.下料取放X轴);
            WaitAxis(AXIS.下料取放Y轴);
            WaitAxis(UAxis);

            POINT basePickPoint = GetBasePointByRegionAndVaccumIdx(task.Source.strRegion, task.VacuumId, bIsPick: true);

            AXIS ZAxis = GetZAxisByVaccumIdx(task.VacuumId);

            double ZPos = basePickPoint.GetPos(ZAxis, this);
            if (IsDryRun)
                ZPos = basePickPoint.GetPos(ZAxis, this) - 5;

            bool b_DestroyTask = false;
            int iRetry = 0;
            while (true)
            {
                ZAxis.AbsMove(ZPos, AxisHandle.SpeedPercent);
                WaitAxis(ZAxis);
                //Delay(100);
                task.DOSuck.Set(true);
                if (!IsDryRun)
                {
                    if (!WaitDI(task.DISuck, true, 2, true))
                    {
                        AxisSingleTo(ZAxis, POINT.安全位);
                        WaitAxis(ZAxis);
                        ShowMessage($"吸嘴{task.VacuumId}取{task.Source.strRegion}_{task.SourceSlot + 1}料失败,请处理", "GantryGoSinglePick", new string[] { "已处理好" });
                        Logger.Process($"吸嘴{task.VacuumId}取{task.Source.strRegion}_{task.SourceSlot + 1}料失败,请处理", "人工处理好了");
                    }
                }
               
                AxisSingleTo(ZAxis, POINT.安全位);
                WaitAxis(ZAxis);

                if(IsDryRun || task.DISuck.Get())
                {
                    break;
                }

                if (iRetry++ > 2)
                {
                    int idx = ShowMessage($"吸嘴{task.VacuumId}取{task.Source.strRegion}_{task.SourceSlot + 1}料失败超过3次，请检查".txtTrans(TransType.Trans), "取料失败".txtTrans(TransType.Trans), new string[] { "重新取料".txtTrans(TransType.Trans), "确认没料".txtTrans(TransType.Trans) });
                    if (idx == 0)
                    {
                        iRetry = 0;
                    }
                    else
                    {
                        b_DestroyTask = true;
                        break;
                    }
                }
                Delay(50);
            }
            if (b_DestroyTask)
            {
                task.Source.SetSlotState(task.SourceSlot, SlotState.NA);             
                curStack.DestroyTask(task.VacuumId);
                task = null;
            }
            else
            {
                task.ApplyPick();
            }

            sw.Stop();
            Logger.Process("下料取放站", $"吸嘴{task.VacuumId}取{task.Source.strRegion}_{task.SourceSlot + 1}料时间为:{sw.ElapsedMilliseconds}ms");
        }

        private void GantryGoSinglePlace(VacuumTask task)
        {
            if (task == null)
            {
                EventMgr.Error("Vaccum task invalid");
                return;
            }
            AllAxisZGotoSecurityPosition();
            AXIS.下料取放X轴.AbsMove(task.PlaceX, AxisHandle.SpeedPercent);
            AXIS.下料取放Y轴.AbsMove(task.PlaceY, AxisHandle.SpeedPercent);
            AXIS UAxis = GetUAxisByVaccumIdx(task.VacuumId);
            UAxis.AbsMove(task.PlaceU, AxisHandle.SpeedPercent);
            WaitAxis(AXIS.下料取放X轴);
            WaitAxis(AXIS.下料取放Y轴);
            WaitAxis(UAxis);

            while (true)
            {
                if (IsDryRun)
                    break;
                if (!task.DISuck.Get())
                {
                    ShowMessage($"放料前吸嘴{task.VacuumId}真空异常，检查是否掉料".txtTrans(TransType.Trans));
                }
                else
                {
                    break;
                }
            }

            POINT basePlacePoint = GetBasePointByRegionAndVaccumIdx(task.Target.strRegion, task.VacuumId, bIsPick: false);

            AXIS ZAxis = GetZAxisByVaccumIdx(task.VacuumId);

            double ZPos = basePlacePoint.GetPos(ZAxis, this);
            if (IsDryRun)
                ZPos = basePlacePoint.GetPos(ZAxis, this) - 5;

            //double tempZPos = ZPos - 1;
            //ZAxis.AbsMove(tempZPos, AxisHandle.SpeedPercent);
            //WaitAxis(ZAxis);

            ZAxis.AbsMove(ZPos, AxisHandle.SpeedPercent);
            WaitAxis(ZAxis);
            Delay(50);
            task.DOSuck.Set(false);
            task.DOBlow.Set(true);
            Delay(100);
            task.DOBlow.Set(false);
            //Delay(100);
         
            AxisSingleTo(ZAxis, POINT.安全位);
            WaitAxis(ZAxis);
            while (task.DISuck.Get())
            {
                ShowMessage($"放料后吸嘴{task.VacuumId}真空异常，请检查".txtTrans(TransType.Trans));
            }
            task.ApplyPlace();

        }
        private AXIS GetUAxisByVaccumIdx(int index)
        {
            switch (index)
            {
                case 1:
                    return AXIS.下料取放吸嘴1U轴;
                case 2:
                    return AXIS.下料取放吸嘴2U轴;
                case 3:
                    return AXIS.下料取放吸嘴3U轴;
                case 4:
                    return AXIS.下料取放吸嘴4U轴;
                default:
                    throw new Exception("Index out of range");
            }
        }
        private AXIS GetZAxisByVaccumIdx(int index)
        {
            switch (index)
            {
                case 1:
                    return AXIS.下料取放吸嘴1Z轴;
                case 2:
                    return AXIS.下料取放吸嘴2Z轴;
                case 3:
                    return AXIS.下料取放吸嘴3Z轴;
                case 4:
                    return AXIS.下料取放吸嘴4Z轴;
                default:
                    throw new Exception("Index out of range");
            }
        }

        private POINT GetBasePointByRegionAndVaccumIdx(string strRegion, int iIdx, bool bIsPick = true)
        {
            string strAction = bIsPick ? "取料" : "放料";
            string strPoint = $"{strRegion}区吸嘴{iIdx}{strAction}对应的第1个穴位";
            foreach (POINT item in Enum.GetValues(typeof(POINT)))
            {
                if (item.ToString() == strPoint)
                {
                    return item;
                }
            }
            throw new Exception($"POINT:{strPoint} not exist!");
        }


        public Coordinate CalculateCavityPosByBaseCavity(string strRegion, int VaccumIdx, int SlotIdx)
        {
            POINT cav1 = GetBasePointByRegionAndVaccumIdx(strRegion, VaccumIdx);


            int rowIdx = (SlotIdx - 1) / Global.trayTotalCol;
            int colIdx = (SlotIdx - 1) % Global.trayTotalCol;

            double x = cav1.GetPos(AXIS.下料取放X轴, this) - Global.trayRowGap * rowIdx;
            double y = cav1.GetPos(AXIS.下料取放Y轴, this) + Global.trayColGap * colIdx;
            double u = cav1.GetPos(GetUAxisByVaccumIdx(VaccumIdx), this);

            Coordinate coor = new Coordinate() { X = x, Y = y, U = u };
            return coor;     
        }

        string CalibRunStep = string.Empty;
        /// <summary>
        /// 每次标定前是否需要跳弹窗停一下
        /// </summary>
        bool CalibNeedJudge = true;

        protected override string CalibRun(string objStep)
        {
            CalibRunStep = objStep.ToString();
            switch (CalibRunStep)
            {
                case "EmptyStep":
                    Delay(1000);
                    return "StepExit";
                #region CCD5九点标定第1个吸嘴
                // ===================== CCD5 第1个吸嘴 九点标定 =====================
                case "CCD5第1个吸嘴相机9点标定":
                    Delay(10);
                    return "CCD5第1个吸嘴走标准位";

                case "CCD5第1个吸嘴走标准位":
                    AllAxisZGotoSecurityPosition();
                    AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放X轴);
                    AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放Y轴);
                    WaitAxis(AXIS.下料取放X轴);
                    WaitAxis(AXIS.下料取放Y轴);
                    basex = AXIS.下料取放X轴.GetPos();
                    basey = AXIS.下料取放Y轴.GetPos();
                    return "CCD5第1个吸嘴的第1个点";

                case "CCD5第1个吸嘴的第1个点":
                    if (!NinePointCalib("CCD5", 1, 1, 1))
                    {
                        return "EmptyStep";
                    }
                    return "CCD5第1个吸嘴的第2个点";

                case "CCD5第1个吸嘴的第2个点":
                    if (!NinePointCalib("CCD5", 1, 2, 1))
                    {
                        return "EmptyStep";
                    }
                    return "CCD5第1个吸嘴的第3个点";

                case "CCD5第1个吸嘴的第3个点":
                    if (!NinePointCalib("CCD5", 1, 3, 1))
                        return "EmptyStep";
                    return "CCD5第1个吸嘴的第4个点";


                case "CCD5第1个吸嘴的第4个点":
                    if (!NinePointCalib("CCD5", 1, 4, 1))
                    {
                        return "EmptyStep";
                    }
                    return "CCD5第1个吸嘴的第5个点";

                case "CCD5第1个吸嘴的第5个点":
                    if (!NinePointCalib("CCD5", 1, 5, 1))
                    {
                        return "EmptyStep";
                    }
                    return "CCD5第1个吸嘴的第6个点";

                case "CCD5第1个吸嘴的第6个点":
                    if (!NinePointCalib("CCD5", 1, 6, 1))
                    {
                        return "EmptyStep";
                    }
                    return "CCD5第1个吸嘴的第7个点";

                case "CCD5第1个吸嘴的第7个点":
                    if (!NinePointCalib("CCD5", 1, 7, 1))
                    {
                        return "EmptyStep";
                    }
                    return "CCD5第1个吸嘴的第8个点";

                case "CCD5第1个吸嘴的第8个点":
                    if (!NinePointCalib("CCD5", 1, 8, 1))
                    {
                        return "EmptyStep";
                    }
                    return "CCD5第1个吸嘴的第9个点";

                case "CCD5第1个吸嘴的第9个点":
                    if (!NinePointCalib("CCD5", 1, 9, 1))
                    {
                        return "EmptyStep";
                    }
                    ShowMessage("CCD5:九点标定结束", "好消息", new string[] { "好的" });
                    return "EmptyStep";
                #endregion

                #region CCD5九点标定第2个吸嘴
                // ===================== CCD5 第2个吸嘴 九点标定 =====================
                case "CCD5第2个吸嘴相机9点标定":
                    Delay(10);
                    return "CCD5第2个吸嘴走标准位";

                case "CCD5第2个吸嘴走标准位":
                    AllAxisZGotoSecurityPosition();
                    AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放X轴);
                    AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放Y轴);
                    WaitAxis(AXIS.下料取放X轴);
                    WaitAxis(AXIS.下料取放Y轴);
                    basex = AXIS.下料取放X轴.GetPos();
                    basey = AXIS.下料取放Y轴.GetPos();
                    return "CCD5第2个吸嘴的第1个点";

                case "CCD5第2个吸嘴的第1个点":
                    if (!NinePointCalib("CCD5", 2, 1, 2))
                    {
                        return "EmptyStep";
                    }
                    return "CCD5第2个吸嘴的第2个点";

                case "CCD5第2个吸嘴的第2个点":
                    if (!NinePointCalib("CCD5", 2, 2, 2))
                    {
                        return "EmptyStep";
                    }
                    return "CCD5第2个吸嘴的第3个点";

                case "CCD5第2个吸嘴的第3个点":
                    if (!NinePointCalib("CCD5", 2, 3, 2))
                    {
                        return "EmptyStep";
                    }
                    return "CCD5第2个吸嘴的第4个点";

                case "CCD5第2个吸嘴的第4个点":
                    if (!NinePointCalib("CCD5", 2, 4, 2))
                    {
                        return "EmptyStep";
                    }
                    return "CCD5第2个吸嘴的第5个点";

                case "CCD5第2个吸嘴的第5个点":
                    if (!NinePointCalib("CCD5", 2, 5, 2))
                    {
                        return "EmptyStep";
                    }
                    return "CCD5第2个吸嘴的第6个点";

                case "CCD5第2个吸嘴的第6个点":
                    if (!NinePointCalib("CCD5", 2, 6, 2))
                    {
                        return "EmptyStep";
                    }
                    return "CCD5第2个吸嘴的第7个点";

                case "CCD5第2个吸嘴的第7个点":
                    if (!NinePointCalib("CCD5", 2, 7, 2))
                    {
                        return "EmptyStep";
                    }
                    return "CCD5第2个吸嘴的第8个点";

                case "CCD5第2个吸嘴的第8个点":
                    if (!NinePointCalib("CCD5", 2, 8, 2))
                    {
                        return "EmptyStep";
                    }
                    return "CCD5第2个吸嘴的第9个点";

                case "CCD5第2个吸嘴的第9个点":
                    if (!NinePointCalib("CCD5", 2, 9, 2))
                    {
                        return "EmptyStep";
                    }
                    ShowMessage("CCD5第2吸嘴:九点标定结束", ProduceRunStepName, new string[] { "好的" });
                    return "EmptyStep";
                #endregion

                #region CCD5九点标定第3个吸嘴
                // ===================== CCD5 第3个吸嘴 九点标定 =====================
                case "CCD5第3个吸嘴相机9点标定":
                    Delay(10);
                    return "CCD5第3个吸嘴走标准位";

                case "CCD5第3个吸嘴走标准位":
                    AllAxisZGotoSecurityPosition();
                    AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放X轴);
                    AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放Y轴);
                    WaitAxis(AXIS.下料取放X轴);
                    WaitAxis(AXIS.下料取放Y轴);
                    basex = AXIS.下料取放X轴.GetPos();
                    basey = AXIS.下料取放Y轴.GetPos();
                    return "CCD5第3个吸嘴的第1个点";

                case "CCD5第3个吸嘴的第1个点":
                    if (!NinePointCalib("CCD5", 3, 1, 3))
                    {
                        return "EmptyStep";
                    }
                    return "CCD5第3个吸嘴的第2个点";

                case "CCD5第3个吸嘴的第2个点":
                    if (!NinePointCalib("CCD5", 3, 2, 3))
                    {
                        return "EmptyStep";
                    }
                    return "CCD5第3个吸嘴的第3个点";

                case "CCD5第3个吸嘴的第3个点":
                    if (!NinePointCalib("CCD5", 3, 3, 3))
                    {
                        return "EmptyStep";
                    }
                    return "CCD5第3个吸嘴的第4个点";

                case "CCD5第3个吸嘴的第4个点":
                    if (!NinePointCalib("CCD5", 3, 4, 3))
                    {
                        return "EmptyStep";
                    }
                    return "CCD5第3个吸嘴的第5个点";

                case "CCD5第3个吸嘴的第5个点":
                    if (!NinePointCalib("CCD5", 3, 5, 3))
                    {
                        return "EmptyStep";
                    }
                    return "CCD5第3个吸嘴的第6个点";

                case "CCD5第3个吸嘴的第6个点":
                    if (!NinePointCalib("CCD5", 3, 6, 3))
                    {
                        return "EmptyStep";
                    }
                    return "CCD5第3个吸嘴的第7个点";

                case "CCD5第3个吸嘴的第7个点":
                    if (!NinePointCalib("CCD5", 3, 7, 3))
                    {
                        return "EmptyStep";
                    }
                    return "CCD5第3个吸嘴的第8个点";

                case "CCD5第3个吸嘴的第8个点":
                    if (!NinePointCalib("CCD5", 3, 8, 3))
                    {
                        return "EmptyStep";
                    }
                    return "CCD5第3个吸嘴的第9个点";

                case "CCD5第3个吸嘴的第9个点":
                    if (!NinePointCalib("CCD5", 3, 9, 3))
                    {
                        return "EmptyStep";
                    }
                    ShowMessage("CCD5第3吸嘴:九点标定结束", "好消息", new string[] { "好的" });
                    return "EmptyStep";
                #endregion

                #region  CCD5九点标定第4个吸嘴
                // ===================== CCD5 第4个吸嘴 九点标定 =====================
                case "CCD5第4个吸嘴相机9点标定":
                    Delay(10);
                    return "CCD5第4个吸嘴走标准位";

                case "CCD5第4个吸嘴走标准位":
                    AllAxisZGotoSecurityPosition();
                    AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放X轴);
                    AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放Y轴);
                    WaitAxis(AXIS.下料取放X轴);
                    WaitAxis(AXIS.下料取放Y轴);
                    basex = AXIS.下料取放X轴.GetPos();
                    basey = AXIS.下料取放Y轴.GetPos();
                    return "CCD5第4个吸嘴的第1个点";

                case "CCD5第4个吸嘴的第1个点":
                    if (!NinePointCalib("CCD5", 4, 1, 4))
                    {
                        return "EmptyStep";
                    }
                    return "CCD5第4个吸嘴的第2个点";

                case "CCD5第4个吸嘴的第2个点":
                    if (!NinePointCalib("CCD5", 4, 2, 4))
                    {
                        return "EmptyStep";
                    }
                    return "CCD5第4个吸嘴的第3个点";

                case "CCD5第4个吸嘴的第3个点":
                    if (!NinePointCalib("CCD5", 4, 3, 4))
                    {
                        return "EmptyStep";
                    }
                    return "CCD5第4个吸嘴的第4个点";

                case "CCD5第4个吸嘴的第4个点":
                    if (!NinePointCalib("CCD5", 4, 4, 4))
                    {
                        return "EmptyStep";
                    }
                    return "CCD5第4个吸嘴的第5个点";

                case "CCD5第4个吸嘴的第5个点":
                    if (!NinePointCalib("CCD5", 4, 5, 4))
                    {
                        return "EmptyStep";
                    }
                    return "CCD5第4个吸嘴的第6个点";

                case "CCD5第4个吸嘴的第6个点":
                    if (!NinePointCalib("CCD5", 4, 6, 4))
                    {
                        return "EmptyStep";
                    }
                    return "CCD5第4个吸嘴的第7个点";

                case "CCD5第4个吸嘴的第7个点":
                    if (!NinePointCalib("CCD5", 4, 7, 4))
                    {
                        return "EmptyStep";
                    }
                    return "CCD5第4个吸嘴的第8个点";

                case "CCD5第4个吸嘴的第8个点":
                    if (!NinePointCalib("CCD5", 4, 8, 4))
                    {
                        return "EmptyStep";
                    }
                    return "CCD5第4个吸嘴的第9个点";

                case "CCD5第4个吸嘴的第9个点":
                    if (!NinePointCalib("CCD5", 4, 9, 4))
                    {
                        return "EmptyStep";
                    }
                    ShowMessage("CCD5第4吸嘴:九点标定结束", ProduceRunStepName, new string[] { "好的" });
                    return "EmptyStep";

                #endregion

                #region CCD6九点标定第1个吸嘴
                // ===================== CCD6 第1个吸嘴 九点标定 =====================
                case "CCD6第1个吸嘴相机9点标定":
                    Delay(10);
                    return "CCD6第1个吸嘴走标准位";

                case "CCD6第1个吸嘴走标准位":
                    AllAxisZGotoSecurityPosition();
                    AxisSingleTo(POINT.CCD6吸嘴1和2拍照位, AXIS.下料取放X轴);
                    AxisSingleTo(POINT.CCD6吸嘴1和2拍照位, AXIS.下料取放Y轴);
                    WaitAxis(AXIS.下料取放X轴);
                    WaitAxis(AXIS.下料取放Y轴);
                    basex = AXIS.下料取放X轴.GetPos();
                    basey = AXIS.下料取放Y轴.GetPos();
                    return "CCD6第1个吸嘴的第1个点";

                case "CCD6第1个吸嘴的第1个点":
                    if (!NinePointCalib("CCD6", 10, 1, 1))
                    {
                        return "EmptyStep";
                    }
                    return "CCD6第1个吸嘴的第2个点";

                case "CCD6第1个吸嘴的第2个点":
                    if (!NinePointCalib("CCD6", 10, 2, 1))
                    {
                        return "EmptyStep";
                    }
                    return "CCD6第1个吸嘴的第3个点";

                case "CCD6第1个吸嘴的第3个点":
                    if (!NinePointCalib("CCD6", 10, 3, 1))
                    {
                        return "EmptyStep";
                    }
                    return "CCD6第1个吸嘴的第4个点";

                case "CCD6第1个吸嘴的第4个点":
                    if (!NinePointCalib("CCD6", 10, 4, 1))
                    {
                        return "EmptyStep";
                    }
                    return "CCD6第1个吸嘴的第5个点";

                case "CCD6第1个吸嘴的第5个点":
                    if (!NinePointCalib("CCD6", 10, 5, 1))
                    {
                        return "EmptyStep";
                    }
                    return "CCD6第1个吸嘴的第6个点";

                case "CCD6第1个吸嘴的第6个点":
                    if (!NinePointCalib("CCD6", 10, 6, 1))
                    {
                        return "EmptyStep";
                    }
                    return "CCD6第1个吸嘴的第7个点";

                case "CCD6第1个吸嘴的第7个点":
                    if (!NinePointCalib("CCD6", 10, 7, 1))
                    {
                        return "EmptyStep";
                    }
                    return "CCD6第1个吸嘴的第8个点";

                case "CCD6第1个吸嘴的第8个点":
                    if (!NinePointCalib("CCD6", 10, 8, 1))
                    {
                        return "EmptyStep";
                    }
                    return "CCD6第1个吸嘴的第9个点";

                case "CCD6第1个吸嘴的第9个点":
                    if (!NinePointCalib("CCD6", 10, 9, 1))
                    {
                        return "EmptyStep";
                    }
                    ShowMessage("CCD6:九点标定结束", "好消息", new string[] { "好的" });
                    return "EmptyStep";
                #endregion

                #region CCD6九点标定第2个吸嘴
                // ===================== CCD6 第2个吸嘴 九点标定 =====================
                case "CCD6第2个吸嘴相机9点标定":
                    Delay(10);
                    return "CCD6第2个吸嘴走标准位";

                case "CCD6第2个吸嘴走标准位":
                    AllAxisZGotoSecurityPosition();
                    AxisSingleTo(POINT.CCD6吸嘴1和2拍照位, AXIS.下料取放X轴);
                    AxisSingleTo(POINT.CCD6吸嘴1和2拍照位, AXIS.下料取放Y轴);
                    WaitAxis(AXIS.下料取放X轴);
                    WaitAxis(AXIS.下料取放Y轴);
                    basex = AXIS.下料取放X轴.GetPos();
                    basey = AXIS.下料取放Y轴.GetPos();
                    return "CCD6第2个吸嘴的第1个点";

                case "CCD6第2个吸嘴的第1个点":
                    if (!NinePointCalib("CCD6", 10, 1, 2))
                    {
                        return "EmptyStep";
                    }
                    return "CCD6第2个吸嘴的第2个点";

                case "CCD6第2个吸嘴的第2个点":
                    if (!NinePointCalib("CCD6", 10, 2, 2))
                    {
                        return "EmptyStep";
                    }
                    return "CCD6第2个吸嘴的第3个点";

                case "CCD6第2个吸嘴的第3个点":
                    if (!NinePointCalib("CCD6", 10, 3, 2))
                    {
                        return "EmptyStep";
                    }
                    return "CCD6第2个吸嘴的第4个点";

                case "CCD6第2个吸嘴的第4个点":
                    if (!NinePointCalib("CCD6", 10, 4, 2))
                    {
                        return "EmptyStep";
                    }
                    return "CCD6第2个吸嘴的第5个点";

                case "CCD6第2个吸嘴的第5个点":
                    if (!NinePointCalib("CCD6", 10, 5, 2))
                    {
                        return "EmptyStep";
                    }
                    return "CCD6第2个吸嘴的第6个点";

                case "CCD6第2个吸嘴的第6个点":
                    if (!NinePointCalib("CCD6", 10, 6, 2))
                    {
                        return "EmptyStep";
                    }
                    return "CCD6第2个吸嘴的第7个点";

                case "CCD6第2个吸嘴的第7个点":
                    if (!NinePointCalib("CCD6", 10, 7, 2))
                    {
                        return "EmptyStep";
                    }
                    return "CCD6第2个吸嘴的第8个点";

                case "CCD6第2个吸嘴的第8个点":
                    if (!NinePointCalib("CCD6", 10, 8, 2))
                    {
                        return "EmptyStep";
                    }
                    return "CCD6第2个吸嘴的第9个点";

                case "CCD6第2个吸嘴的第9个点":
                    if (!NinePointCalib("CCD6", 10, 9, 2))
                    {
                        return "EmptyStep";
                    }
                    ShowMessage("CCD6第2吸嘴:九点标定结束", "好消息", new string[] { "好的" });
                    return "EmptyStep";
                #endregion

                #region CCD6九点标定第3个吸嘴
                // ===================== CCD6 第3个吸嘴 九点标定 =====================
                case "CCD6第3个吸嘴相机9点标定":
                    Delay(10);
                    return "CCD6第3个吸嘴走标准位";

                case "CCD6第3个吸嘴走标准位":
                    AllAxisZGotoSecurityPosition();
                    AxisSingleTo(POINT.CCD6吸嘴3和4拍照位, AXIS.下料取放X轴);
                    AxisSingleTo(POINT.CCD6吸嘴3和4拍照位, AXIS.下料取放Y轴);
                    WaitAxis(AXIS.下料取放X轴);
                    WaitAxis(AXIS.下料取放Y轴);
                    basex = AXIS.下料取放X轴.GetPos();
                    basey = AXIS.下料取放Y轴.GetPos();
                    return "CCD6第3个吸嘴的第1个点";

                case "CCD6第3个吸嘴的第1个点":
                    if (!NinePointCalib("CCD6", 10, 1, 3))
                    {
                        return "EmptyStep";
                    }
                    return "CCD6第3个吸嘴的第2个点";

                case "CCD6第3个吸嘴的第2个点":
                    if (!NinePointCalib("CCD6", 10, 2, 3))
                    {
                        return "EmptyStep";
                    }
                    return "CCD6第3个吸嘴的第3个点";

                case "CCD6第3个吸嘴的第3个点":
                    if (!NinePointCalib("CCD6", 10, 3, 3))
                    {
                        return "EmptyStep";
                    }
                    return "CCD6第3个吸嘴的第4个点";

                case "CCD6第3个吸嘴的第4个点":
                    if (!NinePointCalib("CCD6", 10, 4, 3))
                    {
                        return "EmptyStep";
                    }
                    return "CCD6第3个吸嘴的第5个点";

                case "CCD6第3个吸嘴的第5个点":
                    if (!NinePointCalib("CCD6", 10, 5, 3))
                    {
                        return "EmptyStep";
                    }
                    return "CCD6第3个吸嘴的第6个点";

                case "CCD6第3个吸嘴的第6个点":
                    if (!NinePointCalib("CCD6", 10, 6, 3))
                    {
                        return "EmptyStep";
                    }
                    return "CCD6第3个吸嘴的第7个点";

                case "CCD6第3个吸嘴的第7个点":
                    if (!NinePointCalib("CCD6", 10, 7, 3))
                    {
                        return "EmptyStep";
                    }
                    return "CCD6第3个吸嘴的第8个点";

                case "CCD6第3个吸嘴的第8个点":
                    if (!NinePointCalib("CCD6", 10, 8, 3))
                    {
                        return "EmptyStep";
                    }
                    return "CCD6第3个吸嘴的第9个点";

                case "CCD6第3个吸嘴的第9个点":
                    if (!NinePointCalib("CCD6", 10, 9, 3))
                    {
                        return "EmptyStep";
                    }
                    ShowMessage("CCD6第3吸嘴:九点标定结束", "好消息", new string[] { "好的" });
                    return "EmptyStep";
                #endregion

                #region CCD6九点标定第4个吸嘴
                // ===================== CCD6 第4个吸嘴 九点标定 =====================
                case "CCD6第4个吸嘴相机9点标定":
                    Delay(10);
                    return "CCD6第4个吸嘴走标准位";
                case "CCD6第4个吸嘴走标准位":
                    AllAxisZGotoSecurityPosition();
                    AxisSingleTo(POINT.CCD6吸嘴3和4拍照位, AXIS.下料取放X轴);
                    AxisSingleTo(POINT.CCD6吸嘴3和4拍照位, AXIS.下料取放Y轴);
                    WaitAxis(AXIS.下料取放X轴);
                    WaitAxis(AXIS.下料取放Y轴);
                    basex = AXIS.下料取放X轴.GetPos();
                    basey = AXIS.下料取放Y轴.GetPos();
                    return "CCD6第4个吸嘴的第1个点";

                case "CCD6第4个吸嘴的第1个点":
                    if (!NinePointCalib("CCD6", 10, 1, 4))
                    {
                        return "EmptyStep";
                    }
                    return "CCD6第4个吸嘴的第2个点";

                case "CCD6第4个吸嘴的第2个点":
                    if (!NinePointCalib("CCD6", 10, 2, 4))
                    {
                        return "EmptyStep";
                    }
                    return "CCD6第4个吸嘴的第3个点";

                case "CCD6第4个吸嘴的第3个点":
                    if (!NinePointCalib("CCD6", 10, 3, 4))
                    {
                        return "EmptyStep";
                    }
                    return "CCD6第4个吸嘴的第4个点";

                case "CCD6第4个吸嘴的第4个点":
                    if (!NinePointCalib("CCD6", 10, 4, 4))
                    {
                        return "EmptyStep";
                    }
                    return "CCD6第4个吸嘴的第5个点";

                case "CCD6第4个吸嘴的第5个点":
                    if (!NinePointCalib("CCD6", 10, 5, 4))
                    {
                        return "EmptyStep";
                    }
                    return "CCD6第4个吸嘴的第6个点";

                case "CCD6第4个吸嘴的第6个点":
                    if (!NinePointCalib("CCD6", 10, 6, 4))
                    {
                        return "EmptyStep";
                    }
                    return "CCD6第4个吸嘴的第7个点";

                case "CCD6第4个吸嘴的第7个点":
                    if (!NinePointCalib("CCD6", 10, 7, 4))
                    {
                        return "EmptyStep";
                    }
                    return "CCD6第4个吸嘴的第8个点";

                case "CCD6第4个吸嘴的第8个点":
                    if (!NinePointCalib("CCD6", 10, 8, 4))
                    {
                        return "EmptyStep";
                    }
                    return "CCD6第4个吸嘴的第9个点";

                case "CCD6第4个吸嘴的第9个点":
                    if (!NinePointCalib("CCD6", 10, 9, 4))
                    {
                        return "EmptyStep";
                    }
                    ShowMessage("CCD6第4吸嘴:九点标定结束", "好消息", new string[] { "好的" });
                    return "EmptyStep";
                #endregion


                #region CCD5五点旋转标定第1个吸嘴
                case "CCD5第1个吸嘴的5点标定":
                    //CCD5_第一个吸嘴_第一个位置
                    return "CCD5_1_1_标准位";
                case "CCD5_1_1_标准位":
                    AllAxisZGotoSecurityPosition();
                    AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放X轴);
                    AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放Y轴);
                    WaitAxis(AXIS.下料取放X轴);
                    WaitAxis(AXIS.下料取放Y轴);
                    AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放吸嘴1U轴);
                    AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放吸嘴1Z轴);
                    WaitAxis(AXIS.下料取放吸嘴1U轴);
                    WaitAxis(AXIS.下料取放吸嘴1Z轴);
               
                    u = AXIS.下料取放吸嘴1U轴.GetPos();
                    return "CCD5_1_1";
                case "CCD5_1_1":
                    if (!NozzleRotate5PointCalib("CCD5", 1, 1))
                    {
                        return "EmptyStep";
                    }
                    return "CCD5_1_2";
                case "CCD5_1_2":
                    if (!NozzleRotate5PointCalib("CCD5", 2, 1))
                    {
                        return "EmptyStep";
                    }
                    return "CCD5_1_3";
                case "CCD5_1_3":
                    if (!NozzleRotate5PointCalib("CCD5", 3, 1))
                    {
                        return "EmptyStep";
                    }
                    return "CCD5_1_4";
                case "CCD5_1_4":
                    if (!NozzleRotate5PointCalib("CCD5", 4, 1))
                    {
                        return "EmptyStep";
                    }
                    return "CCD5_1_5";
                case "CCD5_1_5":
                    if (!NozzleRotate5PointCalib("CCD5", 5, 1))
                    {
                        return "EmptyStep";
                    }
                    ShowMessage("CCD5第1吸嘴5点旋转标定完成", ProduceRunStepName, new string[] { "确定" });
                    return "EmptyStep";
                #endregion

                #region CCD5五点旋转标定第2个吸嘴
                case "CCD5第2个吸嘴的5点标定":
                    return "CCD5_2_1_标准位";
                case "CCD5_2_1_标准位":
                    AllAxisZGotoSecurityPosition();
                    AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放X轴);
                    AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放Y轴);
                    WaitAxis(AXIS.下料取放X轴);
                    WaitAxis(AXIS.下料取放Y轴);
                    AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放吸嘴2U轴);
                    AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放吸嘴2Z轴);
                    WaitAxis(AXIS.下料取放吸嘴2U轴);
                    WaitAxis(AXIS.下料取放吸嘴2Z轴);
                    u = AXIS.下料取放吸嘴2U轴.GetPos();
                    return "CCD5_2_1";
                case "CCD5_2_1":
                    if (!NozzleRotate5PointCalib("CCD5", 1, 2))
                    {
                        return "EmptyStep";
                    }
                    return "CCD5_2_2";
                case "CCD5_2_2":
                    if (!NozzleRotate5PointCalib("CCD5", 2, 2))
                    {
                        return "EmptyStep";
                    }
                    return "CCD5_2_3";
                case "CCD5_2_3":
                    if (!NozzleRotate5PointCalib("CCD5", 3, 2))
                    {
                        return "EmptyStep";
                    }
                    return "CCD5_2_4";
                case "CCD5_2_4":
                    if (!NozzleRotate5PointCalib("CCD5", 4, 2))
                    {
                        return "EmptyStep";
                    }
                    return "CCD5_2_5";
                case "CCD5_2_5":
                    if (!NozzleRotate5PointCalib("CCD5", 5, 2))
                    {
                        return "EmptyStep";
                    }
                    ShowMessage("CCD5第2吸嘴5点旋转标定完成", ProduceRunStepName, new string[] { "确定" });
                    return "EmptyStep";
                #endregion

                #region CCD5五点旋转标定第3个吸嘴
                case "CCD5第3个吸嘴的5点标定":
                    return "CCD5_3_1_标准位";
                case "CCD5_3_1_标准位":
                    AllAxisZGotoSecurityPosition();
                    AxisSingleTo(POINT.CCD5吸嘴3和4拍照位, AXIS.下料取放X轴);
                    AxisSingleTo(POINT.CCD5吸嘴3和4拍照位, AXIS.下料取放Y轴);
                    WaitAxis(AXIS.下料取放X轴);
                    WaitAxis(AXIS.下料取放Y轴);
                    AxisSingleTo(POINT.CCD5吸嘴3和4拍照位, AXIS.下料取放吸嘴3U轴);
                    AxisSingleTo(POINT.CCD5吸嘴3和4拍照位, AXIS.下料取放吸嘴3Z轴);
                    WaitAxis(AXIS.下料取放吸嘴3U轴);
                    WaitAxis(AXIS.下料取放吸嘴3Z轴);
                    u = AXIS.下料取放吸嘴3U轴.GetPos();
                    return "CCD5_3_1";
                case "CCD5_3_1":
                    if (!NozzleRotate5PointCalib("CCD5", 1, 3))
                    {
                        return "EmptyStep";
                    }
                    return "CCD5_3_2";
                case "CCD5_3_2":
                    if (!NozzleRotate5PointCalib("CCD5", 2, 3))
                    {
                        return "EmptyStep";
                    }
                    return "CCD5_3_3";
                case "CCD5_3_3":
                    if (!NozzleRotate5PointCalib("CCD5", 3, 3))
                    {
                        return "EmptyStep";
                    }
                    return "CCD5_3_4";
                case "CCD5_3_4":
                    if (!NozzleRotate5PointCalib("CCD5", 4, 3))
                    {
                        return "EmptyStep";
                    }
                    return "CCD5_3_5";
                case "CCD5_3_5":
                    if (!NozzleRotate5PointCalib("CCD5", 5, 3))
                    {
                        return "EmptyStep";
                    }
                    ShowMessage("CCD5第3吸嘴5点旋转标定完成", ProduceRunStepName, new string[] { "确定" });
                    return "EmptyStep";
                #endregion

                #region CCD5五点旋转标定第4个吸嘴
                case "CCD5第4个吸嘴的5点标定":
                    return "CCD5_4_1_标准位";
                case "CCD5_4_1_标准位":
                    AllAxisZGotoSecurityPosition();
                    AxisSingleTo(POINT.CCD5吸嘴3和4拍照位, AXIS.下料取放X轴);
                    AxisSingleTo(POINT.CCD5吸嘴3和4拍照位, AXIS.下料取放Y轴);
                    WaitAxis(AXIS.下料取放X轴);
                    WaitAxis(AXIS.下料取放Y轴);
                    AxisSingleTo(POINT.CCD5吸嘴3和4拍照位, AXIS.下料取放吸嘴4U轴);
                    AxisSingleTo(POINT.CCD5吸嘴3和4拍照位, AXIS.下料取放吸嘴4Z轴);
                    WaitAxis(AXIS.下料取放吸嘴4U轴);
                    WaitAxis(AXIS.下料取放吸嘴4Z轴);
                    u = AXIS.下料取放吸嘴4U轴.GetPos();
                    return "CCD5_4_1";
                case "CCD5_4_1":
                    if (!NozzleRotate5PointCalib("CCD5", 1, 4))
                    {
                        return "EmptyStep";
                    }
                    return "CCD5_4_2";
                case "CCD5_4_2":
                    if (!NozzleRotate5PointCalib("CCD5", 2, 4))
                    {
                        return "EmptyStep";
                    }
                    return "CCD5_4_3";
                case "CCD5_4_3":
                    if (!NozzleRotate5PointCalib("CCD5", 3, 4))
                    {
                        return "EmptyStep";
                    }
                    return "CCD5_4_4";
                case "CCD5_4_4":
                    if (!NozzleRotate5PointCalib("CCD5", 4, 4))
                    {
                        return "EmptyStep";
                    }
                    return "CCD5_4_5";
                case "CCD5_4_5":
                    if (!NozzleRotate5PointCalib("CCD5", 5, 4))
                    {
                        return "EmptyStep";
                    }
                    ShowMessage("CCD5第4吸嘴5点旋转标定完成", ProduceRunStepName, new string[] { "确定" });
                    return "EmptyStep";
                #endregion

                #region CCD6五点旋转标定第1个吸嘴
                //==================== CCD6 第1吸嘴 ====================
                case "CCD6第1个吸嘴的5点标定":
                    return "CCD6_1_1_标准位";
                case "CCD6_1_1_标准位":
                    AllAxisZGotoSecurityPosition();
                    AxisSingleTo(POINT.CCD6吸嘴1和2拍照位, AXIS.下料取放X轴);
                    AxisSingleTo(POINT.CCD6吸嘴1和2拍照位, AXIS.下料取放Y轴);
                    WaitAxis(AXIS.下料取放X轴);
                    WaitAxis(AXIS.下料取放Y轴);
                    AxisSingleTo(POINT.CCD6吸嘴1和2拍照位, AXIS.下料取放吸嘴1U轴);
                    AxisSingleTo(POINT.CCD6吸嘴1和2拍照位, AXIS.下料取放吸嘴1Z轴);
                    WaitAxis(AXIS.下料取放吸嘴1U轴);
                    WaitAxis(AXIS.下料取放吸嘴1Z轴);
                    u = AXIS.下料取放吸嘴1U轴.GetPos();
                    return "CCD6_1_1";
                case "CCD6_1_1":
                    if (!NozzleRotate5PointCalib("CCD6", 1, 1))
                    {
                        return "EmptyStep";
                    }
                    return "CCD6_1_2";
                case "CCD6_1_2":
                    if (!NozzleRotate5PointCalib("CCD6", 2, 1))
                    {
                        return "EmptyStep";
                    }
                    return "CCD6_1_3";
                case "CCD6_1_3":
                    if (!NozzleRotate5PointCalib("CCD6", 3, 1))
                    {
                        return "EmptyStep";
                    }
                    return "CCD6_1_4";
                case "CCD6_1_4":
                    if (!NozzleRotate5PointCalib("CCD6", 4, 1))
                    {
                        return "EmptyStep";
                    }
                    return "CCD6_1_5";
                case "CCD6_1_5":
                    if (!NozzleRotate5PointCalib("CCD6", 5, 1))
                    {
                        return "EmptyStep";
                    }
                    ShowMessage("CCD6第1吸嘴5点旋转标定完成", ProduceRunStepName, new string[] { "确定" });
                    return "EmptyStep";
                #endregion

                #region CCD6五点旋转标定第2个吸嘴
                //==================== CCD6 第2吸嘴 ====================
                case "CCD6第2个吸嘴的5点标定":
                    return "CCD6_2_1_标准位";
                case "CCD6_2_1_标准位":
                    AllAxisZGotoSecurityPosition();
                    AxisSingleTo(POINT.CCD6吸嘴1和2拍照位, AXIS.下料取放X轴);
                    AxisSingleTo(POINT.CCD6吸嘴1和2拍照位, AXIS.下料取放Y轴);
                    WaitAxis(AXIS.下料取放X轴);
                    WaitAxis(AXIS.下料取放Y轴);
                    AxisSingleTo(POINT.CCD6吸嘴1和2拍照位, AXIS.下料取放吸嘴2U轴);
                    AxisSingleTo(POINT.CCD6吸嘴1和2拍照位, AXIS.下料取放吸嘴2Z轴);
                    WaitAxis(AXIS.下料取放吸嘴2U轴);
                    WaitAxis(AXIS.下料取放吸嘴2Z轴);
                    u = AXIS.下料取放吸嘴2U轴.GetPos();
                    return "CCD6_2_1";
                case "CCD6_2_1":
                    if (!NozzleRotate5PointCalib("CCD6", 1, 2))
                    {
                        return "EmptyStep";
                    }
                    return "CCD6_2_2";
                case "CCD6_2_2":
                    if (!NozzleRotate5PointCalib("CCD6", 2, 2))
                    {
                        return "EmptyStep";
                    }
                    return "CCD6_2_3";
                case "CCD6_2_3":
                    if (!NozzleRotate5PointCalib("CCD6", 3, 2))
                    {
                        return "EmptyStep";
                    }
                    return "CCD6_2_4";
                case "CCD6_2_4":
                    if (!NozzleRotate5PointCalib("CCD6", 4, 2))
                    {
                        return "EmptyStep";
                    }
                    return "CCD6_2_5";
                case "CCD6_2_5":
                    if (!NozzleRotate5PointCalib("CCD6", 5, 2))
                    {
                        return "EmptyStep";
                    }
                    ShowMessage("CCD6第2吸嘴5点旋转标定完成", ProduceRunStepName, new string[] { "确定" });
                    return "EmptyStep";
                #endregion

                #region CCD6五点旋转标定第3个吸嘴
                case "CCD6第3个吸嘴的5点标定":
                    return "CCD6_3_1_标准位";
                case "CCD6_3_1_标准位":
                    AllAxisZGotoSecurityPosition();
                    AxisSingleTo(POINT.CCD6吸嘴3和4拍照位, AXIS.下料取放X轴);
                    AxisSingleTo(POINT.CCD6吸嘴3和4拍照位, AXIS.下料取放Y轴);
                    WaitAxis(AXIS.下料取放X轴);
                    WaitAxis(AXIS.下料取放Y轴);
                    AxisSingleTo(POINT.CCD6吸嘴3和4拍照位, AXIS.下料取放吸嘴3U轴);
                    AxisSingleTo(POINT.CCD6吸嘴3和4拍照位, AXIS.下料取放吸嘴3Z轴);
                    WaitAxis(AXIS.下料取放吸嘴3U轴);
                    WaitAxis(AXIS.下料取放吸嘴3Z轴);
                    u = AXIS.下料取放吸嘴3U轴.GetPos();
                    return "CCD6_3_1";
                case "CCD6_3_1":
                    if (!NozzleRotate5PointCalib("CCD6", 1, 3))
                    {
                        return "EmptyStep";
                    }
                    return "CCD6_3_2";
                case "CCD6_3_2":
                    if (!NozzleRotate5PointCalib("CCD6", 2, 3))
                    {
                        return "EmptyStep";
                    }
                    return "CCD6_3_3";
                case "CCD6_3_3":
                    if (!NozzleRotate5PointCalib("CCD6", 3, 3))
                    {
                        return "EmptyStep";
                    }
                    return "CCD6_3_4";
                case "CCD6_3_4":
                    if (!NozzleRotate5PointCalib("CCD6", 4, 3))
                    {
                        return "EmptyStep";
                    }
                    return "CCD6_3_5";
                case "CCD6_3_5":
                    if (!NozzleRotate5PointCalib("CCD6", 5, 3))
                    {
                        return "EmptyStep";
                    }
                    ShowMessage("CCD6第3吸嘴5点旋转标定完成", ProduceRunStepName, new string[] { "确定" });
                    return "EmptyStep";
                #endregion

                #region CCD6五点旋转标定第4个吸嘴
                case "CCD6第4个吸嘴的5点标定":
                    return "CCD6_4_1_标准位";
                case "CCD6_4_1_标准位":
                    AllAxisZGotoSecurityPosition();
                    AxisSingleTo(POINT.CCD6吸嘴3和4拍照位, AXIS.下料取放X轴);
                    AxisSingleTo(POINT.CCD6吸嘴3和4拍照位, AXIS.下料取放Y轴);
                    WaitAxis(AXIS.下料取放X轴);
                    WaitAxis(AXIS.下料取放Y轴);
                    AxisSingleTo(POINT.CCD6吸嘴3和4拍照位, AXIS.下料取放吸嘴4U轴);
                    AxisSingleTo(POINT.CCD6吸嘴3和4拍照位, AXIS.下料取放吸嘴4Z轴);
                    WaitAxis(AXIS.下料取放吸嘴4U轴);
                    WaitAxis(AXIS.下料取放吸嘴4Z轴);
                    u = AXIS.下料取放吸嘴4U轴.GetPos();
                    return "CCD6_4_1";
                case "CCD6_4_1":
                    if (!NozzleRotate5PointCalib("CCD6", 1, 4))
                    {
                        return "EmptyStep";
                    }
                    return "CCD6_4_2";
                case "CCD6_4_2":
                    if (!NozzleRotate5PointCalib("CCD6", 2, 4))
                    {
                        return "EmptyStep";
                    }
                    return "CCD6_4_3";
                case "CCD6_4_3":
                    if (!NozzleRotate5PointCalib("CCD6", 3, 4))
                    {
                        return "EmptyStep";
                    }
                    return "CCD6_4_4";
                case "CCD6_4_4":
                    if (!NozzleRotate5PointCalib("CCD6", 4, 4))
                    {
                        return "EmptyStep";
                    }
                    return "CCD6_4_5";
                case "CCD6_4_5":
                    if (!NozzleRotate5PointCalib("CCD6", 5, 4))
                    {
                        return "EmptyStep";
                    }
                    ShowMessage("CCD6第4吸嘴5点旋转标定完成", ProduceRunStepName, new string[] { "确定" });
                    return "EmptyStep";
                #endregion

                #region CCD5注册模板吸嘴1
                case "CCD5注册模板吸嘴1":
                    RegisterNozzelTemplate(CameraName.CCD5, 6, NozzelID.NG);
                    return "EmptyStep";
                #endregion

                #region CCD5注册模板吸嘴2
                case "CCD5注册模板吸嘴2":
                    RegisterNozzelTemplate(CameraName.CCD5, 7, NozzelID.NG2);
                    return "EmptyStep";
                #endregion

                #region CCD5注册模板吸嘴3
                case "CCD5注册模板吸嘴3":
                    RegisterNozzelTemplate(CameraName.CCD5, 8, NozzelID.OK1);
                    return "EmptyStep";
                #endregion

                #region CCD5注册模板吸嘴4
                case "CCD5注册模板吸嘴4":
                    RegisterNozzelTemplate(CameraName.CCD5, 9, NozzelID.OK2);
                    return "EmptyStep";
                #endregion

                #region CCD6注册模板吸嘴1
                case "CCD6注册模板吸嘴1":
                    RegisterNozzelTemplate(CameraName.CCD6, 15, NozzelID.NG);
                    return "EmptyStep";
                #endregion

                #region CCD6注册模板吸嘴2
                case "CCD6注册模板吸嘴2":
                    RegisterNozzelTemplate(CameraName.CCD6, 16, NozzelID.NG2);
                    return "EmptyStep";
                #endregion

                #region CCD6注册模板吸嘴3
                case "CCD6注册模板吸嘴3":
                    RegisterNozzelTemplate(CameraName.CCD6, 17, NozzelID.OK1);
                    return "EmptyStep";
                #endregion

                #region CCD6注册模板吸嘴4
                case "CCD6注册模板吸嘴4":
                    RegisterNozzelTemplate(CameraName.CCD6, 18, NozzelID.OK2);
                    return "EmptyStep";
                #endregion




                case "CCD7与CCD5关联":
                    ShowMessage("确认CCD7关联CCD5菲林片是否放好", objStep, new string[] { "已经放好" });
                    return "去CCD7与CCD5关联拍照位";
                case "去CCD7与CCD5关联拍照位":
                    AllAxisZGotoSecurityPositionCaib();
                    AxisSingleTo(AXIS.下料取放X轴, POINT.CCD7与CCD5关联拍菲林片);
                    AxisSingleTo(AXIS.下料取放Y轴, POINT.CCD7与CCD5关联拍菲林片);
                    WaitAxis(AXIS.下料取放X轴);
                    WaitAxis(AXIS.下料取放Y轴);
                    return "触发CCD7关联CCD5拍照";
                case "触发CCD7关联CCD5拍照":
                   double x = POINT.吸嘴1取菲林片位置.GetPos(AXIS.下料取放X轴,this);
                    CCD7Camera.SendCommand(TcpCommu_Camera.Command.CCD7与CCD5或CCD6关联, "CCD7", "20", AXIS.下料取放X轴.GetPos(), AXIS.下料取放Y轴.GetPos(), AXIS.下料取放吸嘴1U轴.GetPos(), POINT.吸嘴1取菲林片位置.GetPos(AXIS.下料取放X轴, this), POINT.吸嘴1取菲林片位置.GetPos(AXIS.下料取放Y轴, this), POINT.吸嘴1取菲林片位置.GetPos(AXIS.下料取放吸嘴1U轴, this));
                    Delay(1000);
                    DO.CCD7料盘定位拍照.Set(true);
                    Delay(CameraTriggerTimeClosed);
                    DO.CCD7料盘定位拍照.Set(false);
                    Delay(CameraTriggerTimeClosed);
                   
                    if (!CCD7Camera.WaitReply(TcpCommu_Camera.Command.CCD7与CCD5或CCD6关联, out _))
                    {
                        return "触发CCD7关联CCD5拍照"; ;
                    }
                    return "获取CCD7与CCD5关联视觉结果";

                case "获取CCD7与CCD5关联视觉结果":
                    if (!CCD7Camera.CCD7RelativeToCCD5OrCCD6Result)
                    {
                        return "触发CCD7关联CCD5拍照";
                    }
                    return "CCD5吸嘴1到取菲林片位置";

                case "CCD5吸嘴1到取菲林片位置":
                    AllAxisZGotoSecurityPositionCaib();
                    ShowMessage("CCD7拍完，是否去CCD5取菲林片?", "提示", new string[] { "好的" });
                    AxisSingleTo(AXIS.下料取放X轴, POINT.吸嘴1取菲林片位置);
                    AxisSingleTo(AXIS.下料取放Y轴, POINT.吸嘴1取菲林片位置);
                    WaitAxis(AXIS.下料取放X轴);
                    WaitAxis(AXIS.下料取放Y轴);
                    AxisSingleTo(AXIS.下料取放吸嘴1U轴, POINT.吸嘴1取菲林片位置);
                    WaitAxis(AXIS.下料取放吸嘴1U轴);
                    AxisSingleTo(AXIS.下料取放吸嘴1Z轴, POINT.吸嘴1取菲林片位置);
                    WaitAxis(AXIS.下料取放吸嘴1Z轴);
                    return "吸嘴1开真空";
                case "吸嘴1开真空":
                    if (DI.下料站吸嘴1真空到位.Get())
                    {
                        ShowMessage("检测到真空到位信号，查看吸嘴上是否有物料", "提示", new string[] { "已处理完成" });
                        return "吸嘴1开真空";
                    }

                    DO.下料站吸嘴1吸真空.Set(true);
                    Delay(10000);
                    if (DI.下料站吸嘴1真空到位.Get())
                    {
                        AXIS.下料取放吸嘴1Z轴.AbsMove(AXIS.下料取放吸嘴1Z轴.GetPos() - 10, dbSpeed:5);
                        WaitAxis(AXIS.下料取放吸嘴1Z轴);
                        return "去CCD5拍菲林片";
                    }
                    ShowMessage("吸嘴1真空报警", "提示", new string[] { "已处理完成" });
                    return "吸嘴1开真空";
                case "去CCD5拍菲林片":
                    AllAxisZGotoSecurityPositionCaib();
                    AxisSingleTo(AXIS.下料取放X轴, POINT.CCD5拍菲林片位置);
                    AxisSingleTo(AXIS.下料取放Y轴, POINT.CCD5拍菲林片位置);
                    WaitAxis(AXIS.下料取放X轴);
                    WaitAxis(AXIS.下料取放Y轴);
                    AxisSingleTo(AXIS.下料取放吸嘴1Z轴, POINT.CCD5拍菲林片位置);
                    WaitAxis(AXIS.下料取放吸嘴1Z轴);
                    return "CCD5触发视觉拍照";
               
                case "CCD5触发视觉拍照":
                    ShowMessage("是否开始CCD5拍照?", "提示", new string[] { "好的" });
                    CCD5Camera.SendCommand(TcpCommu_Camera.Command.CCD5拍菲林片, "CCD5", "19", AXIS.下料取放X轴.GetPos(), AXIS.下料取放Y轴.GetPos(), AXIS.下料取放吸嘴1U轴.GetPos());
                    Delay(1000);
                    DO.CCD5下料定位拍照.Set(true);
                    Delay(CameraTriggerTimeClosed);
                    DO.CCD5下料定位拍照.Set(false);
                    Delay(CameraTriggerTimeClosed);
                    Delay(1000);
                    if (CCD5Camera.WaitReply(TcpCommu_Camera.Command.CCD5拍菲林片, out _))
                    {
                        return "获取CCD5关联结果";
                    }
                    else
                    {
                        return "CCD5触发视觉拍照";
                    }
                case "获取CCD5关联结果":
                    if (!CCD5Camera.CCD7RelativeToCCD5OrCCD6Result)
                    {
                        ShowMessage("CCD5拍照失败", objStep, new string[] { "继续拍照?" });
                        return "CCD5触发视觉拍照";
                    }
                    return "回到CCD7放菲林片位置";
                case "回到CCD7放菲林片位置":
                    ShowMessage("CCD5拍照完成?", "提示", new string[] { "好的" });
                    AllAxisZGotoSecurityPositionCaib();
                    AxisSingleTo(AXIS.下料取放X轴, POINT.吸嘴1取菲林片位置);
                    AxisSingleTo(AXIS.下料取放Y轴, POINT.吸嘴1取菲林片位置);
                    WaitAxis(AXIS.下料取放X轴);
                    WaitAxis(AXIS.下料取放Y轴);
                    AxisSingleTo(AXIS.下料取放吸嘴1Z轴, POINT.吸嘴1取菲林片位置);
                    WaitAxis(AXIS.下料取放吸嘴1Z轴);
                    DO.下料站吸嘴1吸真空.Set(false);
                    Delay(3000);
                    AllAxisZGotoSecurityPositionCaib();
                    ShowMessage("菲林片放料完成", "提示", new string[] { "结束CCD7与CCD5关联" });
                    return StepExit;




                case "CCD7与CCD6关联":
                    ShowMessage("确认CCD7关联CCD6菲林片是否放好", objStep, new string[] { "已经放好" });
                    return "去CCD7与CCD6关联拍照位";
                case "去CCD7与CCD6关联拍照位":
                    AllAxisZGotoSecurityPositionCaib();
                    AxisSingleTo(AXIS.下料取放X轴, POINT.CCD7与CCD5关联拍菲林片);
                    AxisSingleTo(AXIS.下料取放Y轴, POINT.CCD7与CCD5关联拍菲林片);
                    WaitAxis(AXIS.下料取放X轴);
                    WaitAxis(AXIS.下料取放Y轴);
                    return "触发CCD7关联CCD6拍照";
                case "触发CCD7关联CCD6拍照":
                    CCD7Camera.SendCommand(TcpCommu_Camera.Command.CCD7与CCD5或CCD6关联, "CCD7", "22", AXIS.下料取放X轴.GetPos(), AXIS.下料取放Y轴.GetPos(), AXIS.下料取放吸嘴1U轴.GetPos() ,POINT.吸嘴1取菲林片位置.GetPos(AXIS.下料取放X轴, this), POINT.吸嘴1取菲林片位置.GetPos(AXIS.下料取放Y轴, this), POINT.吸嘴1取菲林片位置.GetPos(AXIS.下料取放吸嘴1U轴, this));
                    Delay(1000);
                    DO.CCD7料盘定位拍照.Set(true);
                    Delay(CameraTriggerTimeClosed);
                    DO.CCD7料盘定位拍照.Set(false);
                    Delay(CameraTriggerTimeClosed);
                    
                    if (!CCD7Camera.WaitReply(TcpCommu_Camera.Command.CCD7与CCD5或CCD6关联, out _))
                    {
                        ShowMessage("触发CCD7关联CCD6拍照失败","提示",new string[] { "好的" });
                        return "触发CCD7关联CCD6拍照"; ;
                    }
                    return "获取CCD7与CCD6关联视觉结果";

                case "获取CCD7与CCD6关联视觉结果":
                    if (!CCD7Camera.CCD7RelativeToCCD5OrCCD6Result)
                    {
                        return "触发CCD7关联CCD6拍照";
                    }
                    return "CCD6吸嘴1到取菲林片位置";

                case "CCD6吸嘴1到取菲林片位置":
                    AllAxisZGotoSecurityPositionCaib();
                    ShowMessage("CCD7拍完，是否去CCD6取菲林片?", "提示", new string[] { "好的" });
                    AxisSingleTo(AXIS.下料取放X轴, POINT.吸嘴1取菲林片位置);
                    AxisSingleTo(AXIS.下料取放Y轴, POINT.吸嘴1取菲林片位置);
                    WaitAxis(AXIS.下料取放X轴);
                    WaitAxis(AXIS.下料取放Y轴);
                    AxisSingleTo(AXIS.下料取放吸嘴1U轴, POINT.吸嘴1取菲林片位置);
                    WaitAxis(AXIS.下料取放吸嘴1U轴);
                    AxisSingleTo(AXIS.下料取放吸嘴1Z轴, POINT.吸嘴1取菲林片位置);
                    WaitAxis(AXIS.下料取放吸嘴1Z轴);
                    return "吸嘴1开真空CCD6";
                case "吸嘴1开真空CCD6":
                    if (DI.下料站吸嘴1真空到位.Get())
                    {
                        ShowMessage("检测到真空到位信号，查看吸嘴上是否有物料", "提示", new string[] { "已处理完成" });
                        return "吸嘴1开真空CCD6";
                    }

                    DO.下料站吸嘴1吸真空.Set(true);
                    Delay(10000);
                    if (DI.下料站吸嘴1真空到位.Get())
                    {
                        AXIS.下料取放吸嘴1Z轴.AbsMove(AXIS.下料取放吸嘴1Z轴.GetPos() - 10, dbSpeed: 5);
                        WaitAxis(AXIS.下料取放吸嘴1Z轴);
                        return "去CCD6拍菲林片";
                    }
                    ShowMessage("吸嘴1开真空CCD6", "提示", new string[] { "已处理完成" });
                    return "吸嘴1开真空CCD6";
                case "去CCD6拍菲林片":
                    AllAxisZGotoSecurityPositionCaib();
                    AxisSingleTo(AXIS.下料取放X轴, POINT.CCD6拍菲林片位置);
                    AxisSingleTo(AXIS.下料取放Y轴, POINT.CCD6拍菲林片位置);
                    WaitAxis(AXIS.下料取放X轴);
                    WaitAxis(AXIS.下料取放Y轴);
                    AxisSingleTo(AXIS.下料取放吸嘴1U轴, POINT.CCD6拍菲林片位置);
                    WaitAxis(AXIS.下料取放吸嘴1U轴);
                    AxisSingleTo(AXIS.下料取放吸嘴1Z轴, POINT.CCD6拍菲林片位置);
                    WaitAxis(AXIS.下料取放吸嘴1Z轴);
                    return "CCD6触发视觉拍照";

                case "CCD6触发视觉拍照":
                    ShowMessage("是否开始CCD6拍照?", "提示", new string[] { "好的" });
                    Delay(1000);
                    CCD6Camera.SendCommand(TcpCommu_Camera.Command.CCD6拍菲林片, "CCD6", "21", AXIS.下料取放X轴.GetPos(), AXIS.下料取放Y轴.GetPos(), AXIS.下料取放吸嘴1U轴.GetPos());
                    DO.CCD6下料定位拍照.Set(true);
                    Delay(CameraTriggerTimeClosed);
                    DO.CCD6下料定位拍照.Set(false);
                    Delay(CameraTriggerTimeClosed);
                   
                    if (CCD6Camera.WaitReply(TcpCommu_Camera.Command.CCD6拍菲林片, out _))
                    {
                        return "获取CCD6关联结果";
                    }
                    else
                    {
                        return "CCD6触发视觉拍照";
                    }
                case "获取CCD6关联结果":
                    if (!CCD6Camera.CCD7RelativeToCCD5OrCCD6Result)
                    {
                        ShowMessage("CCD6拍照失败", objStep, new string[] { "继续拍照?" });
                        return "CCD6触发视觉拍照";
                    }
                    return "回到CCD7放菲林片位置CCD6";
                case "回到CCD7放菲林片位置CCD6":
                    ShowMessage("CCD6拍照完成?", "提示", new string[] { "好的" });
                    AllAxisZGotoSecurityPositionCaib();
                    AxisSingleTo(AXIS.下料取放X轴, POINT.吸嘴1取菲林片位置);
                    AxisSingleTo(AXIS.下料取放Y轴, POINT.吸嘴1取菲林片位置);
                    WaitAxis(AXIS.下料取放X轴);
                    WaitAxis(AXIS.下料取放Y轴);
                    AxisSingleTo(AXIS.下料取放吸嘴1Z轴, POINT.吸嘴1取菲林片位置);
                    WaitAxis(AXIS.下料取放吸嘴1Z轴);
                    DO.下料站吸嘴1吸真空.Set(false);
                    Delay(3000);
                    AllAxisZGotoSecurityPositionCaib();
                    ShowMessage("菲林片放料完成", "提示", new string[] { "结束CCD7与CCD6关联" });
                    return StepExit;


                default:
                    return null;
            }
        }

        double basex = 0;
        double basey = 0;
        int deltaXAndDeltaY = 15;//九点标定
        /// <summary>
        /// 九点标定
        /// </summary>
        /// <param name="cameraName"></param>
        /// <param name="ProcessID"></param>
        /// <param name="point"></param>
        /// <param name="nozzelID"></param>
        /// <returns></returns>
        private bool NinePointCalib(string cameraName, int ProcessID, int point, int nozzelID)
        {
            if (CalibNeedJudge)
            {
                //是否跳一个弹窗  走一个点
                 ShowMessage($"当前为{cameraName}9点标定的吸嘴{nozzelID}走第{point}点是否继续", ProduceRunStepName, new string[] { "是", "否" });
            }
            double x = 0;
            double y = 0;
            double r = 0;
            // 判断点位编号，对应计算坐标x、y
            if (point == 1)
            {
                x = basex - deltaXAndDeltaY;
                y = basey + deltaXAndDeltaY;
            }
            else if (point == 2)
            {
                x = basex;
                y = basey + deltaXAndDeltaY;
            }
            else if (point == 3)
            {
                x = basex + deltaXAndDeltaY;
                y = basey + deltaXAndDeltaY;
            }
            else if (point == 4)
            {
                x = basex - deltaXAndDeltaY;
                y = basey;
            }
            else if (point == 5)
            {
                x = basex;
                y = basey;
            }
            else if (point == 6)
            {
                x = basex + deltaXAndDeltaY;
                y = basey;
            }
            else if (point == 7)
            {
                x = basex - deltaXAndDeltaY;
                y = basey - deltaXAndDeltaY;
            }
            else if (point == 8)
            {
                x = basex;
                y = basey - deltaXAndDeltaY;
            }
            else if (point == 9)
            {
                x = basex + deltaXAndDeltaY;
                y = basey - deltaXAndDeltaY;
            }

            AllAxisZGotoSecurityPosition();
            AXIS.下料取放X轴.AbsMove(x, AxisHandle.SpeedPercent);
            AXIS.下料取放Y轴.AbsMove(y, AxisHandle.SpeedPercent);
            WaitAxis(AXIS.下料取放X轴);
            WaitAxis(AXIS.下料取放Y轴);
            x = AXIS.下料取放X轴.GetPos();
            y = AXIS.下料取放Y轴.GetPos();
            if (cameraName == "CCD5")
            {
                if (nozzelID == 1)
                {
                    AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放吸嘴1Z轴);
                    AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放吸嘴1U轴);
                    WaitAxis(AXIS.下料取放吸嘴1U轴);
                    WaitAxis(AXIS.下料取放吸嘴1Z轴);
                    r = AXIS.下料取放吸嘴1U轴.GetPos();
                }
                else if (nozzelID == 2)
                {
                    AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放吸嘴2Z轴);
                    AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放吸嘴2U轴);
                    WaitAxis(AXIS.下料取放吸嘴2U轴);
                    WaitAxis(AXIS.下料取放吸嘴2Z轴);
                    r = AXIS.下料取放吸嘴2U轴.GetPos();
                }
                else if (nozzelID == 3)
                {
                    AxisSingleTo(POINT.CCD5吸嘴3和4拍照位, AXIS.下料取放吸嘴3Z轴);
                    AxisSingleTo(POINT.CCD5吸嘴3和4拍照位, AXIS.下料取放吸嘴3U轴);
                    WaitAxis(AXIS.下料取放吸嘴3U轴);
                    WaitAxis(AXIS.下料取放吸嘴3Z轴);
                    r = AXIS.下料取放吸嘴3U轴.GetPos();
                }
                else
                {
                    AxisSingleTo(POINT.CCD5吸嘴3和4拍照位, AXIS.下料取放吸嘴4Z轴);
                    AxisSingleTo(POINT.CCD5吸嘴3和4拍照位, AXIS.下料取放吸嘴4U轴);
                    WaitAxis(AXIS.下料取放吸嘴4U轴);
                    WaitAxis(AXIS.下料取放吸嘴4Z轴);
                    r = AXIS.下料取放吸嘴4U轴.GetPos();
                }

                Delay(500);
                CCD5Camera.SendCommand(TcpCommu_Camera.Command.九点标定, cameraName, ProcessID, point, x, y, r);
                DO.CCD5下料定位拍照.Set(true);
                Delay(CameraTriggerTimeClosed);
                DO.CCD5下料定位拍照.Set(false);
                if (CCD5Camera.WaitReply(TcpCommu_Camera.Command.九点标定, out _))
                {
                    if (!CCD5Camera.CCD5CalibResult)
                    {
                        CCD5Camera.SendCommand(TcpCommu_Camera.Command.九点标定, cameraName, ProcessID, point, x, y, r);
                        DO.CCD5下料定位拍照.Set(true);
                        Delay(CameraTriggerTimeClosed);
                        DO.CCD5下料定位拍照.Set(false);
                        if (CCD5Camera.WaitReply(TcpCommu_Camera.Command.九点标定, out _))
                        {
                            if (!CCD5Camera.CCD5CalibResult)
                            {
                                return ShowMessage($"{cameraName} 第{point}点，连续拍2次失败，是否重新标定?", ProduceRunStepName, new string[] { "是" }) == 0;
                            }
                            else
                            {
                                return true;
                            }
                        }
                        else
                        {
                            ShowMessage($"九点标定结果，{cameraName} 第{point}点,结果等待超时");
                            return false;
                        }

                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    ShowMessage($"九点标定结果，{cameraName} 第{point}点,结果等待超时");
                    return false;
                }
            }
            else if (cameraName == "CCD6")
            {
                if (nozzelID == 1)
                {
                    AxisSingleTo(POINT.CCD6吸嘴1和2拍照位, AXIS.下料取放吸嘴1Z轴);
                    AxisSingleTo(POINT.CCD6吸嘴1和2拍照位, AXIS.下料取放吸嘴1U轴);
                    WaitAxis(AXIS.下料取放吸嘴1U轴);
                    WaitAxis(AXIS.下料取放吸嘴1Z轴);
                    r = AXIS.下料取放吸嘴1U轴.GetPos();
                }
                else if (nozzelID == 2)
                {
                    AxisSingleTo(POINT.CCD6吸嘴1和2拍照位, AXIS.下料取放吸嘴2Z轴);
                    AxisSingleTo(POINT.CCD6吸嘴1和2拍照位, AXIS.下料取放吸嘴2U轴);
                    WaitAxis(AXIS.下料取放吸嘴2U轴);
                    WaitAxis(AXIS.下料取放吸嘴2Z轴);
                    r = AXIS.下料取放吸嘴2U轴.GetPos();
                }
                else if (nozzelID == 3)
                {
                    AxisSingleTo(POINT.CCD6吸嘴3和4拍照位, AXIS.下料取放吸嘴3Z轴);
                    AxisSingleTo(POINT.CCD6吸嘴3和4拍照位, AXIS.下料取放吸嘴3U轴);
                    WaitAxis(AXIS.下料取放吸嘴3U轴);
                    WaitAxis(AXIS.下料取放吸嘴3Z轴);
                    r = AXIS.下料取放吸嘴3U轴.GetPos();
                }
                else if (nozzelID == 4)
                {
                    AxisSingleTo(POINT.CCD6吸嘴3和4拍照位, AXIS.下料取放吸嘴4Z轴);
                    AxisSingleTo(POINT.CCD6吸嘴3和4拍照位, AXIS.下料取放吸嘴4U轴);
                    WaitAxis(AXIS.下料取放吸嘴4U轴);
                    WaitAxis(AXIS.下料取放吸嘴4Z轴);
                    r = AXIS.下料取放吸嘴4U轴.GetPos();
                }
                else
                {
                    ShowMessage("吸嘴号填写错误", ProduceRunStepName);
                    return false;
                }
                //AllAxisZGotoSecurityPosition();
                //AXIS.下料取放X轴.AbsMove(x, AxisHandle.SpeedPercent);
                //AXIS.下料取放Y轴.AbsMove(y, AxisHandle.SpeedPercent);
                //WaitAxis(AXIS.下料取放X轴);
                //WaitAxis(AXIS.下料取放Y轴);
                //x = AXIS.下料取放X轴.GetPos();
                //y = AXIS.下料取放Y轴.GetPos();
                Delay(500);
                CCD6Camera.SendCommand(TcpCommu_Camera.Command.九点标定, cameraName, ProcessID, point, x, y, r);
                DO.CCD6下料定位拍照.Set(true);
                Delay(CameraTriggerTimeClosed);
                DO.CCD6下料定位拍照.Set(false);
                if (CCD6Camera.WaitReply(TcpCommu_Camera.Command.九点标定, out _))
                {
                    if (!CCD6Camera.CCD6CalibResult)
                    {
                        CCD6Camera.SendCommand(TcpCommu_Camera.Command.九点标定, cameraName, ProcessID, point, x, y, r);
                        DO.CCD6下料定位拍照.Set(true);
                        Delay(CameraTriggerTimeClosed);
                        DO.CCD6下料定位拍照.Set(false);
                        if (CCD6Camera.WaitReply(TcpCommu_Camera.Command.九点标定, out _))
                        {
                            if (!CCD6Camera.CCD6CalibResult)
                            {
                                return ShowMessage($"{cameraName} 第{point}点，连续拍2次失败，是否重新标定?", ProduceRunStepName, new string[] { "是" }) == 0;
                            }
                            else
                            {
                                return true;
                            }
                        }
                        else
                        {
                            ShowMessage($"九点标定结果，{cameraName} 第{point}点,结果等待超时");
                            return false;
                        }

                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    ShowMessage($"九点标定结果，{cameraName} 第{point}点,结果等待超时");
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        double u = -1;
        int calibFiveDelta = 30;
        /// <summary>
        /// 吸嘴5点旋转标定
        /// 以九点标定第5点(base5x/base5y)为XY基准，U轴左右各旋转30°共5个角度点位
        /// </summary>
        /// <param name="cameraName">相机名称 CCD5/CCD6</param>
        /// <param name="point">当前旋转点位 1~5</param>
        /// <param name="nozzelID">吸嘴编号 1/2/3/4</param>
        /// <returns>0成功 1失败</returns>
        private bool NozzleRotate5PointCalib(string cameraName, int point, int nozzelID)
        {
            if (CalibNeedJudge)
            {
                //是否跳一个弹窗  走一个点
                 ShowMessage($"当前为{cameraName}5点标定的吸嘴{nozzelID}走第{point}点是否继续", ProduceRunStepName, new string[] { "是", "否" });
            }
            double targetU = 0;
            int ProcessID = -1;
            double x, y, r;
            // 判断点位编号，对应计算坐标x、y
            if (point == 1)
            {
                targetU = u - 2*calibFiveDelta;
            }
            else if (point == 2)
            {
                targetU = u - calibFiveDelta;
            }
            else if (point == 3)
            {
                targetU = u;
            }
            else if (point == 4)
            {
                targetU = u + calibFiveDelta;
            }
            else if (point == 5)
            {
                targetU = u + 2 * calibFiveDelta;
            }
            x = AXIS.下料取放X轴.GetPos();
            y = AXIS.下料取放Y轴.GetPos();
            if (cameraName == "CCD5")
            {
                if (nozzelID == 1)
                {
                    ProcessID = 2;
                    AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放吸嘴1Z轴);
                    AXIS.下料取放吸嘴1U轴.AbsMove(targetU, AxisHandle.SpeedPercent);
                    WaitAxis(AXIS.下料取放吸嘴1U轴);
                    WaitAxis(AXIS.下料取放吸嘴1Z轴);
                    r = AXIS.下料取放吸嘴1U轴.GetPos();
                }
                else if (nozzelID == 2)
                {
                    ProcessID = 3;
                    AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放吸嘴2Z轴);
                    AXIS.下料取放吸嘴2U轴.AbsMove(targetU, AxisHandle.SpeedPercent);
                    WaitAxis(AXIS.下料取放吸嘴2U轴);
                    WaitAxis(AXIS.下料取放吸嘴2Z轴);
                    r = AXIS.下料取放吸嘴2U轴.GetPos();
                }
                else if (nozzelID == 3)
                {
                    ProcessID = 4;
                    AxisSingleTo(POINT.CCD5吸嘴3和4拍照位, AXIS.下料取放吸嘴3Z轴);
                    AXIS.下料取放吸嘴3U轴.AbsMove(targetU, AxisHandle.SpeedPercent);
                    WaitAxis(AXIS.下料取放吸嘴3U轴);
                    WaitAxis(AXIS.下料取放吸嘴3Z轴);
                    r = AXIS.下料取放吸嘴3U轴.GetPos();
                }
                else
                {
                    ProcessID = 5;
                    AxisSingleTo(POINT.CCD5吸嘴3和4拍照位, AXIS.下料取放吸嘴4Z轴);
                    AXIS.下料取放吸嘴4U轴.AbsMove(targetU, AxisHandle.SpeedPercent);
                    WaitAxis(AXIS.下料取放吸嘴4U轴);
                    WaitAxis(AXIS.下料取放吸嘴4Z轴);
                    r = AXIS.下料取放吸嘴4U轴.GetPos();
                }
                Delay(500);
                CCD5Camera.SendCommand(TcpCommu_Camera.Command.吸嘴旋转标定, cameraName, ProcessID, point, x, y, r);
                DO.CCD5下料定位拍照.Set(true);
                Delay(CameraTriggerTimeClosed);
                DO.CCD5下料定位拍照.Set(false);
                if (CCD5Camera.WaitReply(TcpCommu_Camera.Command.吸嘴旋转标定, out _))
                {
                    if (!CCD5Camera.CCD5CalibResult)
                    {
                        CCD5Camera.SendCommand(TcpCommu_Camera.Command.吸嘴旋转标定, cameraName, ProcessID, point, x, y, r);
                        DO.CCD5下料定位拍照.Set(true);
                        Delay(CameraTriggerTimeClosed);
                        DO.CCD5下料定位拍照.Set(false);
                        if (CCD5Camera.WaitReply(TcpCommu_Camera.Command.吸嘴旋转标定, out _))
                        {
                            if (!CCD5Camera.CCD5CalibResult)
                            {
                                return ShowMessage($"{cameraName} 第{point}点，连续拍2次失败，是否重新标定?", ProduceRunStepName, new string[] { "是" }) == 0;
                            }
                            else
                            {
                                return true;
                            }
                        }
                        else
                        {
                            ShowMessage($"5点标定结果，{cameraName} 第{point}点,结果等待超时");
                            return false;
                        }

                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    ShowMessage($"5点标定结果，{cameraName} 第{point}点,结果等待超时");
                    return false;
                }


            }
            else if (cameraName == "CCD6")
            {
                if (nozzelID == 1)
                {
                    ProcessID = 11;
                    AxisSingleTo(POINT.CCD6吸嘴1和2拍照位, AXIS.下料取放吸嘴1Z轴);
                    AXIS.下料取放吸嘴1U轴.AbsMove(targetU, AxisHandle.SpeedPercent);
                    WaitAxis(AXIS.下料取放吸嘴1U轴);
                    WaitAxis(AXIS.下料取放吸嘴1Z轴);
                    r = AXIS.下料取放吸嘴1U轴.GetPos();
                }
                else if (nozzelID == 2)
                {
                    ProcessID = 12;
                    AxisSingleTo(POINT.CCD6吸嘴1和2拍照位, AXIS.下料取放吸嘴2Z轴);
                    AXIS.下料取放吸嘴2U轴.AbsMove(targetU, AxisHandle.SpeedPercent);
                    WaitAxis(AXIS.下料取放吸嘴2U轴);
                    WaitAxis(AXIS.下料取放吸嘴2Z轴);
                    r = AXIS.下料取放吸嘴2U轴.GetPos();
                }
                else if (nozzelID == 3)
                {
                    ProcessID = 13;
                    AxisSingleTo(POINT.CCD6吸嘴3和4拍照位, AXIS.下料取放吸嘴3Z轴);
                    AXIS.下料取放吸嘴3U轴.AbsMove(targetU, AxisHandle.SpeedPercent);
                    WaitAxis(AXIS.下料取放吸嘴3U轴);
                    WaitAxis(AXIS.下料取放吸嘴3Z轴);
                    r = AXIS.下料取放吸嘴3U轴.GetPos();
                }
                else if (nozzelID == 4)
                {
                    ProcessID = 14;
                    AxisSingleTo(POINT.CCD6吸嘴3和4拍照位, AXIS.下料取放吸嘴4Z轴);
                    AXIS.下料取放吸嘴4U轴.AbsMove(targetU, AxisHandle.SpeedPercent);
                    WaitAxis(AXIS.下料取放吸嘴4U轴);
                    WaitAxis(AXIS.下料取放吸嘴4Z轴);
                    r = AXIS.下料取放吸嘴4U轴.GetPos();
                }
                else
                {
                    ShowMessage("吸嘴号填写错误", ProduceRunStepName);
                    return false;
                }
                Delay(500);
                CCD6Camera.SendCommand(TcpCommu_Camera.Command.吸嘴旋转标定, cameraName, ProcessID, point, x, y, r);
                DO.CCD6下料定位拍照.Set(true);
                Delay(CameraTriggerTimeClosed);
                DO.CCD6下料定位拍照.Set(false);
                if (CCD6Camera.WaitReply(TcpCommu_Camera.Command.吸嘴旋转标定, out _))
                {
                    if (!CCD6Camera.CCD6CalibResult)
                    {
                        CCD6Camera.SendCommand(TcpCommu_Camera.Command.吸嘴旋转标定, cameraName, ProcessID, point, x, y, r);
                        DO.CCD6下料定位拍照.Set(true);
                        Delay(CameraTriggerTimeClosed);
                        DO.CCD6下料定位拍照.Set(false);
                        if (CCD6Camera.WaitReply(TcpCommu_Camera.Command.吸嘴旋转标定, out _))
                        {
                            if (!CCD6Camera.CCD6CalibResult)
                            {
                                return ShowMessage($"{cameraName} 第{point}点，连续拍2次失败，是否重新标定?", ProduceRunStepName, new string[] { "是" }) == 0;
                            }
                            else
                            {
                                return true;
                            }
                        }
                        else
                        {
                            ShowMessage($"5点标定结果，{cameraName} 第{point}点,结果等待超时");
                            return false;
                        }

                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    ShowMessage($"5点标定结果，{cameraName} 第{point}点,结果等待超时");
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        string GRRStep = string.Empty;
        protected override string GrrRun(string objStep)
        {
            GRRStep = objStep.ToString();
            switch (objStep.ToString())
            {
                case "EmptyStep":
                    Delay(1000);
                    return "StepExit";

                #region CCD5吸嘴静态测试
                case "CCD5吸嘴1静态测试":
                    ShowMessage("CCD5吸嘴1静态测试", "提示", new string[] { "开始?" });
                    NozzelStaticTest(CameraName.CCD5, NozzelID.NG);
                    ShowMessage("CCD5吸嘴1静态测试完成");
                    return "EmptyStep";

                case "CCD5吸嘴2静态测试":
                    NozzelStaticTest(CameraName.CCD5, NozzelID.NG2);
                    return "EmptyStep";

                case "CCD5吸嘴3静态测试":
                    NozzelStaticTest(CameraName.CCD5, NozzelID.OK1);
                    return "EmptyStep";

                case "CCD5吸嘴4静态测试":
                    NozzelStaticTest(CameraName.CCD5, NozzelID.OK2);
                    return "EmptyStep";
                #endregion

                #region CCD6吸嘴静态测试
                case "CCD6吸嘴1静态测试":
                    ShowMessage("CCD6吸嘴1静态测试", "提示", new string[] { "开始?" });
                    NozzelStaticTest(CameraName.CCD6, NozzelID.NG);
                    ShowMessage("CCD6吸嘴1静态测试完成");
                    return "EmptyStep";

                case "CCD6吸嘴2静态测试":
                    NozzelStaticTest(CameraName.CCD6, NozzelID.NG2);
                    return "EmptyStep";

                case "CCD6吸嘴3静态测试":
                    NozzelStaticTest(CameraName.CCD6, NozzelID.OK1);
                    return "EmptyStep";

                case "CCD6吸嘴4静态测试":
                    NozzelStaticTest(CameraName.CCD6, NozzelID.OK2);
                    return "EmptyStep";
                #endregion


                #region CCD5吸嘴动态测试
                case "CCD5吸嘴1动态测试":
                    ShowMessage("CCD5吸嘴1动态测试", "提示", new string[] { "开始?" });
                    NozzelDynamicTest(CameraName.CCD5, NozzelID.NG);
                    ShowMessage("CCD5吸嘴1动态测试完成");
                    return "EmptyStep";

                case "CCD5吸嘴2动态测试":
                    NozzelDynamicTest(CameraName.CCD5, NozzelID.NG2);
                    return "EmptyStep";

                case "CCD5吸嘴3动态测试":
                    NozzelDynamicTest(CameraName.CCD5, NozzelID.OK1);
                    return "EmptyStep";

                case "CCD5吸嘴4动态测试":
                    NozzelDynamicTest(CameraName.CCD5, NozzelID.OK2);
                    return "EmptyStep";
                #endregion

                #region CCD6吸嘴动态测试
                case "CCD6吸嘴1动态测试":
                    ShowMessage("CCD6吸嘴1动态测试", "提示", new string[] { "开始?" });
                    NozzelDynamicTest(CameraName.CCD6, NozzelID.NG);
                    ShowMessage("CCD6吸嘴1动态测试完成");
                    return "EmptyStep";

                case "CCD6吸嘴2动态测试":
                    NozzelDynamicTest(CameraName.CCD6, NozzelID.NG2);
                    return "EmptyStep";

                case "CCD6吸嘴3动态测试":
                    NozzelDynamicTest(CameraName.CCD6, NozzelID.OK1);
                    return "EmptyStep";

                case "CCD6吸嘴4动态测试":
                    NozzelDynamicTest(CameraName.CCD6, NozzelID.OK2);
                    return "EmptyStep";
                #endregion

                case "CCD7静态测试":
                    ShowMessage("CCD7静态测试", "提示", new string[] { "开始?" });
                    CCD7CameraStaticTest();
                    ShowMessage("CCD7静态测试完成");
                    return "EmptyStep";

                case "CCD7动态测试":
                    ShowMessage("CCD7动态测试", "提示", new string[] { "开始?" });
                    CCD7CameraDynamicTest();
                    ShowMessage("CCD7动态测试完成");
                    return "EmptyStep";

                default:
                    return null;
            }
        }
        int staticTestCount = 200;
        int dynamicTestCount = 200;
        /// <summary>
        /// 相机静态测试
        /// </summary>
        /// <param name="cameraName"></param>
        /// <param name="nozzelID"></param>
        private void NozzelStaticTest(CameraName cameraName, NozzelID nozzelID)
        {
            AllAxisZGotoSecurityPosition();
            if (cameraName == CameraName.CCD5)
            {
                if (nozzelID == NozzelID.NG)
                {
                    AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放X轴);
                    AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放Y轴);
                    AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放吸嘴1Z轴);
                    AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放吸嘴1U轴);
                    WaitAxis(AXIS.下料取放X轴);
                    WaitAxis(AXIS.下料取放Y轴);
                    WaitAxis(AXIS.下料取放吸嘴1U轴);
                    WaitAxis(AXIS.下料取放吸嘴1Z轴);
                }
                else if (nozzelID == NozzelID.NG2)
                {
                    AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放X轴);
                    AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放Y轴);
                    AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放吸嘴2Z轴);
                    AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放吸嘴2U轴);
                    WaitAxis(AXIS.下料取放X轴);
                    WaitAxis(AXIS.下料取放Y轴);
                    WaitAxis(AXIS.下料取放吸嘴2U轴);
                    WaitAxis(AXIS.下料取放吸嘴2Z轴);
                }
                else if (nozzelID == NozzelID.OK1)
                {
                    AxisSingleTo(POINT.CCD5吸嘴3和4拍照位, AXIS.下料取放X轴);
                    AxisSingleTo(POINT.CCD5吸嘴3和4拍照位, AXIS.下料取放Y轴);
                    AxisSingleTo(POINT.CCD5吸嘴3和4拍照位, AXIS.下料取放吸嘴3Z轴);
                    AxisSingleTo(POINT.CCD5吸嘴3和4拍照位, AXIS.下料取放吸嘴3U轴);
                    WaitAxis(AXIS.下料取放X轴);
                    WaitAxis(AXIS.下料取放Y轴);
                    WaitAxis(AXIS.下料取放吸嘴3U轴);
                    WaitAxis(AXIS.下料取放吸嘴3Z轴);
                }
                else if (nozzelID == NozzelID.OK2)
                {
                    AxisSingleTo(POINT.CCD5吸嘴3和4拍照位, AXIS.下料取放X轴);
                    AxisSingleTo(POINT.CCD5吸嘴3和4拍照位, AXIS.下料取放Y轴);
                    AxisSingleTo(POINT.CCD5吸嘴3和4拍照位, AXIS.下料取放吸嘴4Z轴);
                    AxisSingleTo(POINT.CCD5吸嘴3和4拍照位, AXIS.下料取放吸嘴4U轴);
                    WaitAxis(AXIS.下料取放X轴);
                    WaitAxis(AXIS.下料取放Y轴);
                    WaitAxis(AXIS.下料取放吸嘴4U轴);
                    WaitAxis(AXIS.下料取放吸嘴4Z轴);
                }
                for (int i = 0; i < staticTestCount; i++)
                {
                    CCD5Camera.SendCommand(TcpCommu_Camera.Command.CCD5静态);
                    DO.CCD5下料定位拍照.Set(true);
                    Delay(CameraTriggerTimeClosed);
                    DO.CCD5下料定位拍照.Set(false);
                    Delay(CameraTriggerTimeClosed);
                }
            }
            else if (cameraName == CameraName.CCD6)
            {
                if (nozzelID == NozzelID.NG)
                {
                    AxisSingleTo(POINT.CCD6吸嘴1和2拍照位, AXIS.下料取放X轴);
                    AxisSingleTo(POINT.CCD6吸嘴1和2拍照位, AXIS.下料取放Y轴);
                    AxisSingleTo(POINT.CCD6吸嘴1和2拍照位, AXIS.下料取放吸嘴1Z轴);
                    AxisSingleTo(POINT.CCD6吸嘴1和2拍照位, AXIS.下料取放吸嘴1U轴);
                    WaitAxis(AXIS.下料取放X轴);
                    WaitAxis(AXIS.下料取放Y轴);
                    WaitAxis(AXIS.下料取放吸嘴1U轴);
                    WaitAxis(AXIS.下料取放吸嘴1Z轴);
                }
                else if (nozzelID == NozzelID.NG2)
                {
                    AxisSingleTo(POINT.CCD6吸嘴1和2拍照位, AXIS.下料取放X轴);
                    AxisSingleTo(POINT.CCD6吸嘴1和2拍照位, AXIS.下料取放Y轴);
                    AxisSingleTo(POINT.CCD6吸嘴1和2拍照位, AXIS.下料取放吸嘴2Z轴);
                    AxisSingleTo(POINT.CCD6吸嘴1和2拍照位, AXIS.下料取放吸嘴2U轴);
                    WaitAxis(AXIS.下料取放X轴);
                    WaitAxis(AXIS.下料取放Y轴);
                    WaitAxis(AXIS.下料取放吸嘴2U轴);
                    WaitAxis(AXIS.下料取放吸嘴2Z轴);
                }
                else if (nozzelID == NozzelID.OK1)
                {
                    AxisSingleTo(POINT.CCD6吸嘴3和4拍照位, AXIS.下料取放X轴);
                    AxisSingleTo(POINT.CCD6吸嘴3和4拍照位, AXIS.下料取放Y轴);
                    AxisSingleTo(POINT.CCD6吸嘴3和4拍照位, AXIS.下料取放吸嘴3Z轴);
                    AxisSingleTo(POINT.CCD6吸嘴3和4拍照位, AXIS.下料取放吸嘴3U轴);
                    WaitAxis(AXIS.下料取放X轴);
                    WaitAxis(AXIS.下料取放Y轴);
                    WaitAxis(AXIS.下料取放吸嘴3U轴);
                    WaitAxis(AXIS.下料取放吸嘴3Z轴);
                }
                else if (nozzelID == NozzelID.OK2)
                {
                    AxisSingleTo(POINT.CCD6吸嘴3和4拍照位, AXIS.下料取放X轴);
                    AxisSingleTo(POINT.CCD6吸嘴3和4拍照位, AXIS.下料取放Y轴);
                    AxisSingleTo(POINT.CCD6吸嘴3和4拍照位, AXIS.下料取放吸嘴4Z轴);
                    AxisSingleTo(POINT.CCD6吸嘴3和4拍照位, AXIS.下料取放吸嘴4U轴);
                    WaitAxis(AXIS.下料取放X轴);
                    WaitAxis(AXIS.下料取放Y轴);
                    WaitAxis(AXIS.下料取放吸嘴4U轴);
                    WaitAxis(AXIS.下料取放吸嘴4Z轴);
                }
                for (int i = 0; i < staticTestCount; i++)
                {
                    CCD6Camera.SendCommand(TcpCommu_Camera.Command.CCD6静态);
                    DO.CCD6下料定位拍照.Set(true);
                    Delay(CameraTriggerTimeClosed);
                    DO.CCD6下料定位拍照.Set(false);
                    Delay(CameraTriggerTimeClosed);
                }
            }
        }
        /// <summary>
        /// 相机动态测试
        /// </summary>
        /// <param name="cameraName"></param>
        /// <param name="nozzelID"></param>
        private void NozzelDynamicTest(CameraName cameraName, NozzelID nozzelID)
        {
            double x = -1;
            double y = -1;
           
            if (cameraName == CameraName.CCD5)
            {
                for (int i = 0; i < staticTestCount; i++)
                {
                    AllAxisZGotoSecurityPosition();
                    if (nozzelID == NozzelID.NG)
                    {
                        AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放X轴);
                        AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放Y轴);
                    
                        WaitAxis(AXIS.下料取放X轴);
                        WaitAxis(AXIS.下料取放Y轴);
                        AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放吸嘴1Z轴);
                        AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放吸嘴1U轴);
                        WaitAxis(AXIS.下料取放吸嘴1U轴);
                        WaitAxis(AXIS.下料取放吸嘴1Z轴);
                        x = AXIS.下料取放X轴.GetPos();
                        y = AXIS.下料取放Y轴.GetPos();
                    }
                    else if (nozzelID == NozzelID.NG2)
                    {
                        AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放X轴);
                        AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放Y轴);
                        AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放吸嘴2Z轴);
                        AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放吸嘴2U轴);
                        WaitAxis(AXIS.下料取放X轴);
                        WaitAxis(AXIS.下料取放Y轴);
                        WaitAxis(AXIS.下料取放吸嘴2U轴);
                        WaitAxis(AXIS.下料取放吸嘴2Z轴);
                        x = AXIS.下料取放X轴.GetPos();
                        y = AXIS.下料取放Y轴.GetPos();
                    }
                    else if (nozzelID == NozzelID.OK1)
                    {
                        AxisSingleTo(POINT.CCD5吸嘴3和4拍照位, AXIS.下料取放X轴);
                        AxisSingleTo(POINT.CCD5吸嘴3和4拍照位, AXIS.下料取放Y轴);
                        AxisSingleTo(POINT.CCD5吸嘴3和4拍照位, AXIS.下料取放吸嘴3Z轴);
                        AxisSingleTo(POINT.CCD5吸嘴3和4拍照位, AXIS.下料取放吸嘴3U轴);
                        WaitAxis(AXIS.下料取放X轴);
                        WaitAxis(AXIS.下料取放Y轴);
                        WaitAxis(AXIS.下料取放吸嘴3U轴);
                        WaitAxis(AXIS.下料取放吸嘴3Z轴);
                        x = AXIS.下料取放X轴.GetPos();
                        y = AXIS.下料取放Y轴.GetPos();
                    }
                    else if (nozzelID == NozzelID.OK2)
                    {
                        AxisSingleTo(POINT.CCD5吸嘴3和4拍照位, AXIS.下料取放X轴);
                        AxisSingleTo(POINT.CCD5吸嘴3和4拍照位, AXIS.下料取放Y轴);
                        AxisSingleTo(POINT.CCD5吸嘴3和4拍照位, AXIS.下料取放吸嘴4Z轴);
                        AxisSingleTo(POINT.CCD5吸嘴3和4拍照位, AXIS.下料取放吸嘴4U轴);
                        WaitAxis(AXIS.下料取放X轴);
                        WaitAxis(AXIS.下料取放Y轴);
                        WaitAxis(AXIS.下料取放吸嘴4U轴);
                        WaitAxis(AXIS.下料取放吸嘴4Z轴);
                        x = AXIS.下料取放X轴.GetPos();
                        y = AXIS.下料取放Y轴.GetPos();
                    }
                    CCD5Camera.SendCommand(TcpCommu_Camera.Command.CCD5动态);
                    DO.CCD5下料定位拍照.Set(true);
                    Delay(CameraTriggerTimeClosed);
                    DO.CCD5下料定位拍照.Set(false);
                    Delay(CameraTriggerTimeClosed);
                    x -= 10;
                    y -= 10;
                    if (nozzelID == NozzelID.NG)
                    {
                        AllAxisZGotoSecurityPosition();
                        AXIS.下料取放X轴.AbsMove(x, AxisHandle.SpeedPercent);
                        AXIS.下料取放Y轴.AbsMove(y, AxisHandle.SpeedPercent);
                        AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放吸嘴1Z轴);
                        AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放吸嘴1U轴);
                        WaitAxis(AXIS.下料取放X轴);
                        WaitAxis(AXIS.下料取放Y轴);
                        WaitAxis(AXIS.下料取放吸嘴1U轴);
                        WaitAxis(AXIS.下料取放吸嘴1Z轴);
                    }
                    else if (nozzelID == NozzelID.NG2)
                    {
                        AXIS.下料取放X轴.AbsMove(x, AxisHandle.SpeedPercent);
                        AXIS.下料取放Y轴.AbsMove(y, AxisHandle.SpeedPercent);
                        AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放吸嘴2Z轴);
                        AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放吸嘴2U轴);
                        WaitAxis(AXIS.下料取放X轴);
                        WaitAxis(AXIS.下料取放Y轴);
                        WaitAxis(AXIS.下料取放吸嘴2U轴);
                        WaitAxis(AXIS.下料取放吸嘴2Z轴);
                        x = AXIS.下料取放X轴.GetPos();
                        y = AXIS.下料取放Y轴.GetPos();
                    }
                    else if (nozzelID == NozzelID.OK1)
                    {
                        AXIS.下料取放X轴.AbsMove(x, AxisHandle.SpeedPercent);
                        AXIS.下料取放Y轴.AbsMove(y, AxisHandle.SpeedPercent);
                        AxisSingleTo(POINT.CCD5吸嘴3和4拍照位, AXIS.下料取放吸嘴3Z轴);
                        AxisSingleTo(POINT.CCD5吸嘴3和4拍照位, AXIS.下料取放吸嘴3U轴);
                        WaitAxis(AXIS.下料取放X轴);
                        WaitAxis(AXIS.下料取放Y轴);
                        WaitAxis(AXIS.下料取放吸嘴3U轴);
                        WaitAxis(AXIS.下料取放吸嘴3Z轴);
                    }
                    else if (nozzelID == NozzelID.OK2)
                    {
                        AXIS.下料取放X轴.AbsMove(x, AxisHandle.SpeedPercent);
                        AXIS.下料取放Y轴.AbsMove(y, AxisHandle.SpeedPercent);
                        AxisSingleTo(POINT.CCD5吸嘴3和4拍照位, AXIS.下料取放吸嘴4Z轴);
                        AxisSingleTo(POINT.CCD5吸嘴3和4拍照位, AXIS.下料取放吸嘴4U轴);
                        WaitAxis(AXIS.下料取放X轴);
                        WaitAxis(AXIS.下料取放Y轴);
                        WaitAxis(AXIS.下料取放吸嘴4U轴);
                        WaitAxis(AXIS.下料取放吸嘴4Z轴);
                    }
                }
            }
            else if (cameraName == CameraName.CCD6)
            {
                for (int i = 0; i < staticTestCount; i++)
                {
                    AllAxisZGotoSecurityPosition();
                    if (nozzelID == NozzelID.NG)
                    {
                        AxisSingleTo(POINT.CCD6吸嘴1和2拍照位, AXIS.下料取放X轴);
                        AxisSingleTo(POINT.CCD6吸嘴1和2拍照位, AXIS.下料取放Y轴);
                        AxisSingleTo(POINT.CCD6吸嘴1和2拍照位, AXIS.下料取放吸嘴1Z轴);
                        AxisSingleTo(POINT.CCD6吸嘴1和2拍照位, AXIS.下料取放吸嘴1U轴);
                        WaitAxis(AXIS.下料取放X轴);
                        WaitAxis(AXIS.下料取放Y轴);
                        WaitAxis(AXIS.下料取放吸嘴1U轴);
                        WaitAxis(AXIS.下料取放吸嘴1Z轴);
                        x = AXIS.下料取放X轴.GetPos();
                        y = AXIS.下料取放Y轴.GetPos();
                    }
                    else if (nozzelID == NozzelID.NG2)
                    {
                        AxisSingleTo(POINT.CCD6吸嘴1和2拍照位, AXIS.下料取放X轴);
                        AxisSingleTo(POINT.CCD6吸嘴1和2拍照位, AXIS.下料取放Y轴);
                        AxisSingleTo(POINT.CCD6吸嘴1和2拍照位, AXIS.下料取放吸嘴2Z轴);
                        AxisSingleTo(POINT.CCD6吸嘴1和2拍照位, AXIS.下料取放吸嘴2U轴);
                        WaitAxis(AXIS.下料取放X轴);
                        WaitAxis(AXIS.下料取放Y轴);
                        WaitAxis(AXIS.下料取放吸嘴2U轴);
                        WaitAxis(AXIS.下料取放吸嘴2Z轴);
                        x = AXIS.下料取放X轴.GetPos();
                        y = AXIS.下料取放Y轴.GetPos();
                    }
                    else if (nozzelID == NozzelID.OK1)
                    {
                        AxisSingleTo(POINT.CCD6吸嘴3和4拍照位, AXIS.下料取放X轴);
                        AxisSingleTo(POINT.CCD6吸嘴3和4拍照位, AXIS.下料取放Y轴);
                        AxisSingleTo(POINT.CCD6吸嘴3和4拍照位, AXIS.下料取放吸嘴3Z轴);
                        AxisSingleTo(POINT.CCD6吸嘴3和4拍照位, AXIS.下料取放吸嘴3U轴);
                        WaitAxis(AXIS.下料取放X轴);
                        WaitAxis(AXIS.下料取放Y轴);
                        WaitAxis(AXIS.下料取放吸嘴3U轴);
                        WaitAxis(AXIS.下料取放吸嘴3Z轴);
                        x = AXIS.下料取放X轴.GetPos();
                        y = AXIS.下料取放Y轴.GetPos();
                    }
                    else if (nozzelID == NozzelID.OK2)
                    {
                        AxisSingleTo(POINT.CCD6吸嘴3和4拍照位, AXIS.下料取放X轴);
                        AxisSingleTo(POINT.CCD6吸嘴3和4拍照位, AXIS.下料取放Y轴);
                        AxisSingleTo(POINT.CCD6吸嘴3和4拍照位, AXIS.下料取放吸嘴4Z轴);
                        AxisSingleTo(POINT.CCD6吸嘴3和4拍照位, AXIS.下料取放吸嘴4U轴);
                        WaitAxis(AXIS.下料取放X轴);
                        WaitAxis(AXIS.下料取放Y轴);
                        WaitAxis(AXIS.下料取放吸嘴4U轴);
                        WaitAxis(AXIS.下料取放吸嘴4Z轴);
                        x = AXIS.下料取放X轴.GetPos();
                        y = AXIS.下料取放Y轴.GetPos();
                    }
                    CCD6Camera.SendCommand(TcpCommu_Camera.Command.CCD6动态);
                    DO.CCD6下料定位拍照.Set(true);
                    Delay(CameraTriggerTimeClosed);
                    DO.CCD6下料定位拍照.Set(false);
                    Delay(CameraTriggerTimeClosed);
                    x -= 10;
                    y -= 10;
                    AllAxisZGotoSecurityPosition();
                    if (nozzelID == NozzelID.NG)
                    {
                        AXIS.下料取放X轴.AbsMove(x, AxisHandle.SpeedPercent);
                        AXIS.下料取放Y轴.AbsMove(y, AxisHandle.SpeedPercent);
                        AxisSingleTo(POINT.CCD6吸嘴1和2拍照位, AXIS.下料取放吸嘴1Z轴);
                        AxisSingleTo(POINT.CCD6吸嘴1和2拍照位, AXIS.下料取放吸嘴1U轴);
                        WaitAxis(AXIS.下料取放X轴);
                        WaitAxis(AXIS.下料取放Y轴);
                        WaitAxis(AXIS.下料取放吸嘴1U轴);
                        WaitAxis(AXIS.下料取放吸嘴1Z轴);
                    }
                    else if (nozzelID == NozzelID.NG2)
                    {
                        AXIS.下料取放X轴.AbsMove(x, AxisHandle.SpeedPercent);
                        AXIS.下料取放Y轴.AbsMove(y, AxisHandle.SpeedPercent);
                        AxisSingleTo(POINT.CCD6吸嘴1和2拍照位, AXIS.下料取放吸嘴2Z轴);
                        AxisSingleTo(POINT.CCD6吸嘴1和2拍照位, AXIS.下料取放吸嘴2U轴);
                        WaitAxis(AXIS.下料取放X轴);
                        WaitAxis(AXIS.下料取放Y轴);
                        WaitAxis(AXIS.下料取放吸嘴2U轴);
                        WaitAxis(AXIS.下料取放吸嘴2Z轴);
                    }
                    else if (nozzelID == NozzelID.OK1)
                    {
                        AXIS.下料取放X轴.AbsMove(x, AxisHandle.SpeedPercent);
                        AXIS.下料取放Y轴.AbsMove(y, AxisHandle.SpeedPercent);
                        AxisSingleTo(POINT.CCD6吸嘴3和4拍照位, AXIS.下料取放吸嘴3Z轴);
                        AxisSingleTo(POINT.CCD6吸嘴3和4拍照位, AXIS.下料取放吸嘴3U轴);
                        WaitAxis(AXIS.下料取放X轴);
                        WaitAxis(AXIS.下料取放Y轴);
                        WaitAxis(AXIS.下料取放吸嘴3U轴);
                        WaitAxis(AXIS.下料取放吸嘴3Z轴);
                    }
                    else if (nozzelID == NozzelID.OK2)
                    {
                        AXIS.下料取放X轴.AbsMove(x, AxisHandle.SpeedPercent);
                        AXIS.下料取放Y轴.AbsMove(y, AxisHandle.SpeedPercent);
                        AxisSingleTo(POINT.CCD6吸嘴3和4拍照位, AXIS.下料取放吸嘴4Z轴);
                        AxisSingleTo(POINT.CCD6吸嘴3和4拍照位, AXIS.下料取放吸嘴4U轴);
                        WaitAxis(AXIS.下料取放X轴);
                        WaitAxis(AXIS.下料取放Y轴);
                        WaitAxis(AXIS.下料取放吸嘴4U轴);
                        WaitAxis(AXIS.下料取放吸嘴4Z轴);
                    }
                }
            }
        }

        /// <summary>
        /// 注册吸嘴模板
        /// </summary>
        /// <param name="cameraName">相机名称</param>
        /// <param name="processId">流程ID</param>
        /// <param name="nozzelID">吸嘴ID</param>
        private bool RegisterNozzelTemplate(CameraName cameraName, int processId, NozzelID nozzelID)
        {

            ShowMessage($"相机{cameraName.ToString()},注册吸嘴{(int)nozzelID}模板","提示",new string[] { "好的" });
            double nozzelAxisU = -1;
            AllAxisZGotoSecurityPosition();
            AXIS aXIS = AXIS.下料取放吸嘴1U轴;
            string registerID = "-1";
            if (cameraName == CameraName.CCD5)
            {
                if (nozzelID == NozzelID.NG)
                {
                    AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放X轴);
                    AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放Y轴);
                    WaitAxis(AXIS.下料取放X轴);
                    WaitAxis(AXIS.下料取放Y轴);
                    AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放吸嘴1Z轴);
                    AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放吸嘴1U轴);
                    WaitAxis(AXIS.下料取放吸嘴1U轴);
                    WaitAxis(AXIS.下料取放吸嘴1Z轴);
                    registerID = "1";
                    nozzelAxisU = AXIS.下料取放吸嘴1U轴.GetPos();
                }
                else if (nozzelID == NozzelID.NG2)
                {
                    AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放X轴);
                    AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放Y轴);
                    WaitAxis(AXIS.下料取放X轴);
                    WaitAxis(AXIS.下料取放Y轴);
                    AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放吸嘴2Z轴);
                    AxisSingleTo(POINT.CCD5吸嘴1和2拍照位, AXIS.下料取放吸嘴2U轴);
                    WaitAxis(AXIS.下料取放吸嘴2U轴);
                    WaitAxis(AXIS.下料取放吸嘴2Z轴);
                    nozzelAxisU = AXIS.下料取放吸嘴2U轴.GetPos();
                    registerID = "2";
                }
                else if (nozzelID == NozzelID.OK1)
                {
                    AxisSingleTo(POINT.CCD5吸嘴3和4拍照位, AXIS.下料取放X轴);
                    AxisSingleTo(POINT.CCD5吸嘴3和4拍照位, AXIS.下料取放Y轴);
                    WaitAxis(AXIS.下料取放X轴);
                    WaitAxis(AXIS.下料取放Y轴);
                    AxisSingleTo(POINT.CCD5吸嘴3和4拍照位, AXIS.下料取放吸嘴3Z轴);
                    AxisSingleTo(POINT.CCD5吸嘴3和4拍照位, AXIS.下料取放吸嘴3U轴);
                    WaitAxis(AXIS.下料取放吸嘴3U轴);
                    WaitAxis(AXIS.下料取放吸嘴3Z轴);
                    nozzelAxisU = AXIS.下料取放吸嘴3U轴.GetPos();
                    registerID = "3";
                }
                else
                {
                    AxisSingleTo(POINT.CCD5吸嘴3和4拍照位, AXIS.下料取放X轴);
                    AxisSingleTo(POINT.CCD5吸嘴3和4拍照位, AXIS.下料取放Y轴);
                    WaitAxis(AXIS.下料取放X轴);
                    WaitAxis(AXIS.下料取放Y轴);
                    AxisSingleTo(POINT.CCD5吸嘴3和4拍照位, AXIS.下料取放吸嘴4Z轴);
                    AxisSingleTo(POINT.CCD5吸嘴3和4拍照位, AXIS.下料取放吸嘴4U轴);
                    WaitAxis(AXIS.下料取放吸嘴4U轴);
                    WaitAxis(AXIS.下料取放吸嘴4Z轴);
                    nozzelAxisU = AXIS.下料取放吸嘴4U轴.GetPos();
                    registerID = "4";
                }
                CCD5Camera.SendCommand(TcpCommu_Camera.Command.CCD5注册吸嘴模板, processId, registerID, AXIS.下料取放X轴.GetPos(), AXIS.下料取放Y轴.GetPos(), nozzelAxisU);
                StringBuilder ss = new StringBuilder();
                ss.AppendLine("processId:" + processId);
                ss.AppendLine("registerID:" + registerID);
                ss.AppendLine("x:" + AXIS.下料取放X轴.GetPos());
                ss.AppendLine("y:" + AXIS.下料取放Y轴.GetPos());
                ss.AppendLine("u:" + nozzelAxisU);
                Logger.Process(this.Name, ss.ToString());
                DO.CCD5下料定位拍照.Set(true);
                Delay(CameraTriggerTimeClosed);
                DO.CCD5下料定位拍照.Set(false);
                Delay(CameraTriggerTimeClosed);
                CCD5Camera.WaitReply(TcpCommu_Camera.Command.CCD5注册吸嘴模板, out _);
                if (!CCD5Camera.CCD5RegisterTemplateResult)
                {
                    CCD5Camera.SendCommand(TcpCommu_Camera.Command.CCD5注册吸嘴模板, processId, registerID, AXIS.下料取放X轴.GetPos(), AXIS.下料取放Y轴.GetPos(), nozzelAxisU);
                    DO.CCD5下料定位拍照.Set(true);
                    Delay(CameraTriggerTimeClosed);
                    DO.CCD5下料定位拍照.Set(false);
                    Delay(CameraTriggerTimeClosed);
                    CCD5Camera.WaitReply(TcpCommu_Camera.Command.CCD5注册吸嘴模板, out _);
                    if (!CCD5Camera.CCD5RegisterTemplateResult)
                    {
                        Logger.Process(Name, $"{cameraName.ToString()}的吸嘴{nozzelID}连续2次拍照失败");
                        ShowMessage($"{cameraName.ToString()}的吸嘴{nozzelID}连续2次拍照失败");
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }


            }
            else if (cameraName == CameraName.CCD6)
            {
                if (nozzelID == NozzelID.NG)
                {
                    AxisSingleTo(POINT.CCD6吸嘴1和2拍照位, AXIS.下料取放X轴);
                    AxisSingleTo(POINT.CCD6吸嘴1和2拍照位, AXIS.下料取放Y轴);
                    WaitAxis(AXIS.下料取放X轴);
                    WaitAxis(AXIS.下料取放Y轴);
                    AxisSingleTo(POINT.CCD6吸嘴1和2拍照位, AXIS.下料取放吸嘴1Z轴);
                    AxisSingleTo(POINT.CCD6吸嘴1和2拍照位, AXIS.下料取放吸嘴1U轴);
                    WaitAxis(AXIS.下料取放吸嘴1U轴);
                    WaitAxis(AXIS.下料取放吸嘴1Z轴);
                    nozzelAxisU = AXIS.下料取放吸嘴1U轴.GetPos();
                    registerID = "1";
                }
                else if (nozzelID == NozzelID.NG2)
                {
                    AxisSingleTo(POINT.CCD6吸嘴1和2拍照位, AXIS.下料取放X轴);
                    AxisSingleTo(POINT.CCD6吸嘴1和2拍照位, AXIS.下料取放Y轴);
                    WaitAxis(AXIS.下料取放X轴);
                    WaitAxis(AXIS.下料取放Y轴);
                    AxisSingleTo(POINT.CCD6吸嘴1和2拍照位, AXIS.下料取放吸嘴2Z轴);
                    AxisSingleTo(POINT.CCD6吸嘴1和2拍照位, AXIS.下料取放吸嘴2U轴);
                    WaitAxis(AXIS.下料取放吸嘴2U轴);
                    WaitAxis(AXIS.下料取放吸嘴2Z轴);
                    nozzelAxisU = AXIS.下料取放吸嘴2U轴.GetPos();
                    registerID = "2";
                }
                else if (nozzelID == NozzelID.OK1)
                {
                    AxisSingleTo(POINT.CCD6吸嘴3和4拍照位, AXIS.下料取放X轴);
                    AxisSingleTo(POINT.CCD6吸嘴3和4拍照位, AXIS.下料取放Y轴);
                    WaitAxis(AXIS.下料取放X轴);
                    WaitAxis(AXIS.下料取放Y轴);
                    AxisSingleTo(POINT.CCD6吸嘴3和4拍照位, AXIS.下料取放吸嘴3Z轴);
                    AxisSingleTo(POINT.CCD6吸嘴3和4拍照位, AXIS.下料取放吸嘴3U轴);
                    WaitAxis(AXIS.下料取放吸嘴3U轴);
                    WaitAxis(AXIS.下料取放吸嘴3Z轴);
                    nozzelAxisU = AXIS.下料取放吸嘴3U轴.GetPos();
                    registerID = "3";
                }
                else
                {
                    AxisSingleTo(POINT.CCD6吸嘴3和4拍照位, AXIS.下料取放X轴);
                    AxisSingleTo(POINT.CCD6吸嘴3和4拍照位, AXIS.下料取放Y轴);
                    WaitAxis(AXIS.下料取放X轴);
                    WaitAxis(AXIS.下料取放Y轴);
                    AxisSingleTo(POINT.CCD6吸嘴3和4拍照位, AXIS.下料取放吸嘴4Z轴);
                    AxisSingleTo(POINT.CCD6吸嘴3和4拍照位, AXIS.下料取放吸嘴4U轴);
                    WaitAxis(AXIS.下料取放吸嘴4U轴);
                    WaitAxis(AXIS.下料取放吸嘴4Z轴);
                    nozzelAxisU = AXIS.下料取放吸嘴4U轴.GetPos();
                    registerID = "4";
                }
                CCD6Camera.SendCommand(TcpCommu_Camera.Command.CCD6注册吸嘴模板, processId,registerID, AXIS.下料取放X轴.GetPos(), AXIS.下料取放Y轴.GetPos(), nozzelAxisU);
                StringBuilder ss = new StringBuilder();
                ss.AppendLine("processId:" + processId);
                ss.AppendLine("registerID:" + registerID);
                ss.AppendLine("x:" + AXIS.下料取放X轴.GetPos());
                ss.AppendLine("y:" + AXIS.下料取放Y轴.GetPos());
                ss.AppendLine("u:" + nozzelAxisU);
                Logger.Process(this.Name, ss.ToString());
                DO.CCD6下料定位拍照.Set(true);
                Delay(CameraTriggerTimeClosed);
                DO.CCD6下料定位拍照.Set(false);
                Delay(CameraTriggerTimeClosed);
                CCD6Camera.WaitReply(TcpCommu_Camera.Command.CCD6注册吸嘴模板, out _);
                if (!CCD6Camera.CCD6RegisterTemplateResult)
                {
                    CCD6Camera.SendCommand(TcpCommu_Camera.Command.CCD6注册吸嘴模板, processId, registerID, AXIS.下料取放X轴.GetPos(), AXIS.下料取放Y轴.GetPos(), nozzelAxisU);
                    DO.CCD6下料定位拍照.Set(true);
                    Delay(CameraTriggerTimeClosed);
                    DO.CCD6下料定位拍照.Set(false);
                    Delay(CameraTriggerTimeClosed);
                    CCD6Camera.WaitReply(TcpCommu_Camera.Command.CCD6注册吸嘴模板, out _);
                    if (!CCD6Camera.CCD6RegisterTemplateResult)
                    {
                        Logger.Process(Name, $"{cameraName.ToString()}的吸嘴{nozzelID}连续2次拍照失败");
                        ShowMessage($"{cameraName.ToString()}的吸嘴{nozzelID}连续2次拍照失败");
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// CCD7静态测试
        /// </summary>
        /// <param name="cameraName"></param>
        private void CCD7CameraStaticTest()
        {
            AllAxisZGotoSecurityPosition();
            AxisSingleTo(POINT.CCD7拍照位A1, AXIS.下料取放X轴);
            AxisSingleTo(POINT.CCD7拍照位A1, AXIS.下料取放Y轴);
            WaitAxis(AXIS.下料取放X轴);
            WaitAxis(AXIS.下料取放Y轴);
            for (int i = 0; i < dynamicTestCount; i++)
            {
                CCD7Camera.SendCommand(TcpCommu_Camera.Command.CCD7静态);
                DO.CCD7料盘定位拍照.Set(true);
                Delay(CameraTriggerTimeClosed);
                DO.CCD7料盘定位拍照.Set(false);
                Delay(CameraTriggerTimeClosed);

            }
        }

        /// <summary>
        /// 相机7 动态测试
        /// </summary>
        private void CCD7CameraDynamicTest()
        {
            
            for (int i = 0; i < dynamicTestCount; i++)
            {
                double x = -1;
                double y = -1;
                AllAxisZGotoSecurityPosition();
                AxisSingleTo(POINT.CCD7拍照位A1, AXIS.下料取放X轴);
                AxisSingleTo(POINT.CCD7拍照位A1, AXIS.下料取放Y轴);
                WaitAxis(AXIS.下料取放X轴);
                WaitAxis(AXIS.下料取放Y轴);
                x = AXIS.下料取放X轴.GetPos();
                y = AXIS.下料取放Y轴.GetPos();
                CCD7Camera.SendCommand(TcpCommu_Camera.Command.CCD7动态);
                DO.CCD7料盘定位拍照.Set(true);
                Delay(CameraTriggerTimeClosed);
                DO.CCD7料盘定位拍照.Set(false);
                Delay(CameraTriggerTimeClosed);
                AllAxisZGotoSecurityPosition();
                x += 10;
                y += 10;
                AXIS.下料取放X轴.AbsMove(x, AxisHandle.SpeedPercent);
                AXIS.下料取放Y轴.AbsMove(y, AxisHandle.SpeedPercent);
                WaitAxis(AXIS.下料取放X轴);
                WaitAxis(AXIS.下料取放Y轴);
            }
        }


        public enum ProductStatus
        {
            Null,
            OK,
            NG,
            NA,


        }

        public enum NozzelID
        {
            NG = 1,
            NG2 = 2,
            OK1 = 3,
            OK2 = 4
        }

        public enum CameraName
        {
            CCD5,
            CCD6,
            CCD7
        }
                   

    }

    public static partial class HandleEx
    {
        public static double GetPos(this StationUnloadGantryEx.POINT pt, AXIS axis, StationBase station)
        {
            return station.dictPoint[(int)pt][$"a{(int)axis}"];
        }

        public static double GetPos(this StationUnloadGantryEx.POINT pt, AXIS axis, string strStation)
        {
            StationBase station = StationMgr.GetStation(strStation);
            if (station == null)
                return 0;

            return station.dictPoint[(int)pt][$"a{(int)axis}"];
        }
    }

    public class Coordinate
    {
        public double X;
        public double Y;
        public double U;
    }
}
