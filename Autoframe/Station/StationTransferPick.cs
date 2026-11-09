using AF.Card;
using AF.Common;
using AF.Communicate;
using AF.Data;
using AF.Station;
using Autoframe;
using AutoFrame.DBExtend;
using Microsoft.VisualBasic.Logging;
using OxyPlot.Axes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Autoframe
{
    /// <summary>
    /// StationTransferPick
    /// </summary>
    public class StationTransferPick : StationEx
    {
        int sig = 0;
        bool bufferhasmaterial = false;//暂放平台有无料
        string currentbin;//当前料仓
        string[] bufferdata = new string[Global.slotTotalCount];
        string[] Pickdata = new string[Global.slotTotalCount];
        string[] NGReason = new string[Global.slotTotalCount];
        string[] arrSN = new string[Global.slotTotalCount];
        string S1StartTime = "";
        string S1InputQty = "";
        string redisName = "";
        DateTime dt_Temp = DateTime.MinValue;
        public int checksingal(bool s1,bool s2,bool s3)
        {
            if (s1) return 1;
        
            if (s2) return 2;
        
            if (s3) return 3;

            return 0;
        }

        /// <summary>
        /// 构造函数，需要设置站位当前的IO输入，IO输出，轴方向及轴名称，以显示在手动页面方便操作
        /// </summary>
        /// <param name="strName"></param>
        public StationTransferPick(string strName) : base(strName)
        {
            HardwareConfig();
            CheckPoint<POINT>();
            LoadContainer<CONTAINER, ProductData>();
        }

        void HardwareConfig()
        {
            //配置站位界面显示输入
            arrayDI = new object[] { DI.中转取放站料盘夹紧位, DI.中转取放站料盘松开位,DI.中转暂放平台伸出位,DI.中转暂放平台缩回位};
            //配置站位界面显示输出
            arrayDO = new object[] { DO.中转取放站料盘夹紧,DO.中转取放站料盘松开,DO.中转暂放平台伸出, DO.中转暂放平台缩回 };
            //配置站位界面显示气缸
            arrayCylinder = new object[] {Cylinder.中转取放站料盘, Cylinder.中转暂放平台  };
            //配置站位轴
            arrayAxis = new object[] { AXIS.中转取放X轴, AXIS.中转取放Y轴,AXIS.中转取放Z轴  };
        }

        ProductData productData = new ProductData();

        //TcpCommu_Camera camera = new TcpCommu_Camera();
        /// <summary>
        /// 站位内的点位枚举
        /// </summary>
        public enum POINT
        {
            安全位,
            出料接驳取料位,
            下料仓A移载放料位,
            下料仓B移载放料位,
            下料仓C移载放料位,
            中转暂放平台取放位,
            移载放料等待位


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
            AXIS.中转取放X轴.ServoOn(false);
            AXIS.中转取放Y轴.ServoOn(false);
            AXIS.中转取放Z轴.ServoOn(false);
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
                default:
                    this.InitEntry = StepExit;
                    this.AutoEntry = StepEmpty;
                    break;
            }
            RegBit.中转取放站初始化完成.Set(false); // Bit: Reset cờ hoàn tất khởi tạo trạm gắp đặt trung chuyển
            RegBit.出料接驳站通知可取料.Set(false); // Bit: Reset cờ trạm tiếp nhận dỡ liệu báo có thể gắp

            RegBit.下料仓A站通知中转可放料.Set(false); // Bit: Reset cờ kho A báo có thể thả khay
            RegBit.下料仓B站通知中转可放料.Set(false); // Bit: Reset cờ kho B báo có thể thả khay
            RegBit.下料仓C站通知中转可放料.Set(false); // Bit: Reset cờ kho C báo có thể thả khay
            RegBit.TranferGantryInitSafety.Set(false); // Bit: Reset cờ an toàn khởi tạo của cụm Gantry trung chuyển
        }

        protected override string InitRun(string strStep)
        {
            switch (strStep)
            {
                case StepEntry: // [Khởi tạo] Bắt đầu khởi tạo trạm gắp đặt trung chuyển (Transfer Pick)
                    
                    return "轴上使能"; // Chuyển sang: Bật servo các trục

                case "轴上使能": // [Khởi tạo] Bật servo các trục X, Y, Z gắp đặt trung chuyển
                    AXIS.中转取放X轴.ServoOn(true); // Bật Servo trục X
                    AXIS.中转取放Y轴.ServoOn(true); // Bật Servo trục Y
                    AXIS.中转取放Z轴.ServoOn(true); // Bật Servo trục Z
                    Delay(1000); // Chờ 1000ms cho servo ổn định
                    return "料盘夹爪夹紧"; // Chuyển sang: Kẹp tay gắp khay

                case "料盘夹爪夹紧": // [Khởi tạo] Duỗi xi lanh kẹp khay hàng của cụm gắp trung chuyển

                    CylinderOut(Cylinder.中转取放站料盘, false); // Duỗi xi lanh kẹp khay
                    Delay(200); // Trì hoãn 200ms
                    return "Z轴回安全位"; // Chuyển sang: Trục Z về vị trí an toàn

                case "Z轴回安全位": // [Khởi tạo] Trục Z gắp trung chuyển nâng về vị trí an toàn
                    AxisSingleTo(AXIS.中转取放Z轴, POINT.安全位); // Trục Z về vị trí an toàn
                    WaitAxis(AXIS.中转取放Z轴); // Chờ trục Z dừng hẳn
                    return "XY轴回安全位"; // Chuyển sang: Trục XY về vị trí an toàn

                case "XY轴回安全位": // [Khởi tạo] Trục X và Y di chuyển về vị trí an toàn và phát cờ an toàn cho các kho dỡ
                    AxisSingleTo(AXIS.中转取放X轴, POINT.安全位); // Trục X về an toàn
                    AxisSingleTo(AXIS.中转取放Y轴, POINT.安全位); // Trục Y về an toàn
                    WaitAxis(AXIS.中转取放X轴); // Chờ trục X dừng
                    WaitAxis(AXIS.中转取放Y轴); // Chờ trục Y dừng
                    RegBit.TranferGantryInitSafety.Set(true); // Bit: Báo Transfer Gantry đã ở vị trí an toàn
                    return "WaitUnloadBinInitSafety"; // Chuyển sang: Chờ các kho dỡ khay an toàn

                case "WaitUnloadBinInitSafety": // [Khởi tạo] Chờ cả 3 kho Bin A, Bin B, Bin C xác nhận khởi tạo an toàn
                    if (RegBit.BinANotifyTranferInitSafety.Get() && RegBit.BinBNotifyTranferInitSafety.Get() && RegBit.BinCNotifyTranferInitSafety.Get())
                    {
                        RegBit.BinANotifyTranferInitSafety.Set(false); // Reset cờ an toàn Bin A
                        RegBit.BinBNotifyTranferInitSafety.Set(false); // Reset cờ an toàn Bin B
                        RegBit.BinCNotifyTranferInitSafety.Set(false); // Reset cờ an toàn Bin C
                        RegBit.TranferGantryInitSafety.Set(false); // Reset cờ an toàn Transfer Gantry
                        return "暂放平台启动灯关"; // Chuyển sang: Tắt đèn nút Start bàn tạm
                    }
                    else
                    {
                        return "WaitUnloadBinInitSafety"; // Tiếp tục chờ các kho dỡ khay an toàn
                    }
                case "暂放平台启动灯关": // [Khởi tạo] Tắt đèn nút bấm Start của bàn đệm tạm (Buffer Platform)
                    DO.暂放平台启动灯.Set(false); // DO: Tắt đèn nút Start bàn tạm
                    return "暂放平台气缸缩回"; // Chuyển sang: Co xi lanh bàn tạm

                case "暂放平台气缸缩回": // [Khởi tạo] Co xi lanh bàn tạm về vị trí thụt vào
                    CylinderBack(Cylinder.中转暂放平台); // Co xi lanh bàn đệm tạm
                   
                    return "暂放平台物料检测"; // Chuyển sang: Kiểm tra vật liệu trên bàn tạm

                case "暂放平台物料检测": // [Khởi tạo] Kiểm tra cảm biến quang có khay sót trên bàn tạm không

                    //if (!IsDryRun)
                    //{
                    Delay(500); // Trì hoãn 500ms
                    if (DI.暂放平台料盘有无.Get()) // DI: Phát hiện còn khay trên bàn đệm tạm
                    {
                        ShowMessage("请确认暂放平台物料有无并取走料盘", null, new string[] { "已取走" }); // Cảnh báo: Vui lòng lấy khay trên bàn tạm ra
                        return "暂放平台启动灯开"; // Chuyển sang: Bật đèn bàn tạm
                    }
                    else // Không có khay sót
                    {
                        return "等待下料初始化完成"; // Chuyển sang: Chờ cụm Unload hoàn thành khởi tạo
                    }
                    //}
                    //else
                    //{
                    //    if (DI.暂放平台料盘有无.Get())
                    //    {
                    //        ShowMessage("请确认暂放平台物料有无并取走料盘", null, new string[] { "已取走" });
                    //        return "暂放平台启动灯开";
                    //    }
                    //    else
                    //    {
                    //        return "等待下料初始化完成";
                    //    }
                    //}
                case "暂放平台启动灯开": // [Khởi tạo] Bật đèn nút nhấn Start bàn tạm để người vận hành nhấn
                    DO.暂放平台启动灯.Set(true); // DO: Bật đèn nút Start bàn tạm
                    return "等待暂放平台启动信号"; // Chuyển sang: Chờ nút nhấn

                case "等待暂放平台启动信号": // [Khởi tạo] Chờ tín hiệu nhấn nút Start bàn tạm sau khi người vận hành đã lấy khay ra
                    if (true || DI.暂放平台启动.Get()) // Đã có tín hiệu nhấn nút Start
                    {
                        DO.暂放平台启动灯.Set(false); // DO: Tắt đèn nút Start bàn tạm
                        return "暂放平台物料检测"; // Quay lại kiểm tra xem cảm biến đã hết khay chưa
                    }
                    else
                    {
                        return "等待暂放平台启动信号"; // Tiếp tục chờ tín hiệu
                    }

                       
                case "等待下料初始化完成": // [Khởi tạo] Chờ cụm Unload Gantry khởi tạo hoàn tất
                    if (RegBit.UnloadGantryInitFinished.Get()) // Bit: Cụm Unload Gantry đã khởi tạo xong
                    {
                        return "暂放平台气缸伸出"; // Chuyển sang: Duỗi xi lanh bàn tạm
                    }
                    else
                    {
                        return "等待下料初始化完成"; // Tiếp tục chờ Unload Gantry
                    }
                case "暂放平台气缸伸出": // [Khởi tạo] Duỗi xi lanh bàn đệm tạm đưa khay vào vị trí
                    CylinderOut(Cylinder.中转暂放平台); // Duỗi xi lanh bàn tạm
                    return "移动到暂放平台放料位"; // Chuyển sang: Trục XY đến vị trí bàn tạm

                case "移动到暂放平台放料位": // [Khởi tạo] Trục X và Y di chuyển đến vị trí đặt khay trên bàn tạm
                    AxisSingleTo(AXIS.中转取放X轴, POINT.中转暂放平台取放位); // Trục X đến vị trí bàn tạm
                    AxisSingleTo(AXIS.中转取放Y轴, POINT.中转暂放平台取放位); // Trục Y đến vị trí bàn tạm
                    WaitAxis(AXIS.中转取放X轴); // Chờ trục X dừng
                    WaitAxis(AXIS.中转取放Y轴); // Chờ trục Y dừng
                    return "Z轴移动到暂放平台放料位"; // Chuyển sang: Trục Z hạ xuống bàn tạm

                case "Z轴移动到暂放平台放料位": // [Khởi tạo] Trục Z di chuyển 2 giai đoạn (chạy nhanh đến Z-10mm rồi chạy chậm 5mm/s đến điểm đặt)

                    double ZPos = POINT.中转暂放平台取放位.GetPos(AXIS.中转取放Z轴, this);
                    AXIS.中转取放Z轴.AbsMove(ZPos - 10, AxisHandle.SpeedPercent); // Hạ nhanh đến cách điểm 10mm
                    WaitAxis(AXIS.中转取放Z轴);
                    AXIS.中转取放Z轴.AbsMove(ZPos, 5.0); // Hạ chậm 5mm/s đến vị trí nhả
                    WaitAxis(AXIS.中转取放Z轴);


                    //AxisSingleTo(AXIS.中转取放Z轴,POINT.中转暂放平台取放位 );
                    //WaitAxis(AXIS.中转取放Z轴);

                    return "中转夹爪气缸松开"; // Chuyển sang: Nhả kẹp khay

                case "中转夹爪气缸松开": // [Khởi tạo] Co xi lanh kẹp khay để nhả kẹp
                    CylinderBack(Cylinder.中转取放站料盘); // Co xi lanh kẹp khay (nhả khay)
                    return "Z轴移动到安全位"; // Chuyển sang: Trục Z lên an toàn

                case "Z轴移动到安全位": // [Khởi tạo] Trục Z nâng lên vị trí an toàn
                    AxisSingleTo(AXIS.中转取放Z轴, POINT.安全位); // Trục Z lên điểm an toàn
                    WaitAxis(AXIS.中转取放Z轴); // Chờ trục Z dừng
                    return "XY轴移动安全位"; // Chuyển sang: Trục XY về an toàn

                case "XY轴移动安全位": // [Khởi tạo] Trục X và Y di chuyển về vị trí an toàn
                   POINT _placePoint;
        protected override string ProduceRun(string strStep)
        {
            switch (strStep)
            {
                case StepEntry: // [Sản xuất] Bắt đầu chu trình gắp đặt khay trung chuyển
                   
                    return "Z轴到安全位"; // Chuyển sang: Trục Z lên an toàn

                case "Z轴到安全位": // [Sản xuất] Nâng trục Z lên vị trí an toàn trước khi di chuyển ngang

                    AxisSingleTo(AXIS.中转取放Z轴, POINT.安全位); // Trục Z về an toàn
                    WaitAxis(AXIS.中转取放Z轴); // Chờ trục Z dừng

                    return "移动至出料接驳取料位上方"; // Chuyển sang: Trục XY đến vị trí gắp trên trạm dỡ liệu

                #region 暂放平台放空盘流程--已取消
                //case "提示OP放料":
                //...
                #endregion


                case "移动至出料接驳取料位上方": // [Sản xuất] Trục X và Y di chuyển đến phía trên vị trí gắp của trạm tiếp nhận dỡ liệu

                    AxisSingleTo(AXIS.中转取放X轴, POINT.出料接驳取料位); // Trục X đến vị trí gắp dỡ liệu
                    AxisSingleTo(AXIS.中转取放Y轴, POINT.出料接驳取料位); // Trục Y đến vị trí gắp dỡ liệu
                    WaitAxis(AXIS.中转取放X轴); // Chờ trục X dừng
                    WaitAxis(AXIS.中转取放Y轴); // Chờ trục Y dừng
                    return "等待出料接驳可取料"; // Chuyển sang: Chờ trạm dỡ liệu sẵn sàng

                case "等待出料接驳可取料": // [Sản xuất] Chờ tín hiệu báo có thể gắp hàng và đọc toàn bộ dữ liệu khay từ Redis
                    if (RegBit.出料接驳站通知可取料.Get()) // Bit: Trạm dỡ liệu cho phép gắp phôi
                    {
                        string strOutputTransMovID = RegString.Str_OutputTransMoveID.Get(); // Lấy mã số Mover
                        if (strOutputTransMovID == null || strOutputTransMovID == "")
                            Logger.Process(this.Name, "出料接驳动子号为空"); // Cảnh báo nếu ID Mover rỗng
                        //productData = StationMgr.GetStation("出料接驳站").WorkSpace[StationOutputTrans.NestEnum.出料接驳站].GetProductData();

                        Pickdata = new string[Global.slotTotalCount];
                        NGReason = new string[Global.slotTotalCount];
                        S1StartTime = RedisHelperEx.Instance.HashGet($"{strOutputTransMovID}_Data", $"S1_StartTime"); // Thời gian bắt đầu trạm S1
                        S1InputQty = RedisHelperEx.Instance.HashGet($"{strOutputTransMovID}_Data", $"S1_InputQty"); // Số lượng đầu vào trạm S1
                        for (int i = 0; i < Global.slotTotalCount; i++) // Đọc kết quả từng ô từ Redis
                        {
                            //productData.cavitydata[i] = RedisHelperEx.Instance.HashGet(Global.moveidnum, $"S6_{i + 1}");
                            Pickdata[i] = RedisHelperEx.Instance.HashGet(strOutputTransMovID, $"S6_{i + 1}"); // Kết quả trạm S6 (OK/NG/NA)
                            NGReason[i] = RedisHelperEx.Instance.HashGet($"{strOutputTransMovID}_Data", $"{i + 1}_AOINGReason"); // Lý do lỗi AOI
                            arrSN[i] = RedisHelperEx.Instance.HashGet($"{strOutputTransMovID}_Data", $"{i + 1}_SN"); // Mã SN sản phẩm
                        }


                        RegBit.出料接驳站通知可取料.Set(false); // Reset cờ báo có thể gắp
                        //获取出料接驳站数控
                        return "Z轴到取料位"; // Chuyển sang: Trục Z hạ xuống gắp

                    }
                    else
                    {
                        return "等待出料接驳可取料"; // Tiếp tục chờ tín hiệu
                    }

                case "Z轴到取料位": // [Sản xuất] Duỗi xi lanh bàn dỡ liệu và hạ trục Z gắp khay (2 giai đoạn tốc độ)
                    //AxisSingleTo(AXIS.中转取放Z轴,POINT.出料接驳取料位 );
                    //WaitAxis(AXIS.中转取放Z轴);
                    CylinderOut(Cylinder.出料移载站升降); // Duỗi xi lanh nâng bàn dỡ liệu
                    CylinderOut(Cylinder.出料移载站解锁); // Duỗi xi lanh mở khóa bàn dỡ liệu
                    Delay(300); // Trì hoãn 300ms

                    double ZPos = POINT.出料接驳取料位.GetPos(AXIS.中转取放Z轴, this);
                    AXIS.中转取放Z轴.AbsMove(ZPos - 6, AxisHandle.SpeedPercent); // Hạ nhanh đến cách điểm gắp 6mm
                    WaitAxis(AXIS.中转取放Z轴);
                    AXIS.中转取放Z轴.AbsMove(ZPos, 5.0); // Hạ chậm 5mm/s đến điểm kẹp
                    WaitAxis(AXIS.中转取放Z轴);
                    return "取料时夹料盘夹紧"; // Chuyển sang: Kẹp khay hàng

                case "取料时夹料盘夹紧": // [Sản xuất] Duỗi xi lanh tay gắp kẹp chặt khay hàng
                    if (!IsDryRun) // Chế độ chạy thật
                    {
                        CylinderOut(Cylinder.中转取放站料盘); // Duỗi xi lanh kẹp khay hàng

                    }
                    else // Chế độ chạy mô phỏng
                    {
                        CylinderOut(Cylinder.中转取放站料盘, false);
                        Delay(2000);
                    }
                    return "Z轴到安全高度"; // Chuyển sang: Trục Z nâng khay lên an toàn

                case "Z轴到安全高度": // [Sản xuất] Nâng chậm 5mm sau đó nâng nhanh lên an toàn, cập nhật giám sát và đo CT
                    ZPos = POINT.出料接驳取料位.GetPos(AXIS.中转取放Z轴, this);
                    AXIS.中转取放Z轴.AbsMove(ZPos - 5, 5.0); // Nhấc khay lên 5mm tốc độ chậm 5mm/s
                    WaitAxis(AXIS.中转取放Z轴);
                    AxisSingleTo(AXIS.中转取放Z轴, POINT.安全位); // Nâng lên vị trí an toàn
                    WaitAxis(AXIS.中转取放Z轴);

                    string m_data = RedisHelperEx.Instance.HashGet(Global.m_RESPMonitorMover, "出料接驳站");
                    RedisHelperEx.Instance.HashSet(Global.m_RESPMonitorMover, this.Name, $"0,{m_data.Split(',')[1]}"); // Cập nhật giám sát Redis

                    RegBit.中转取放站取料完成.Set(true); // Bit: Báo hoàn tất gắp phôi khỏi bàn dỡ liệu

                    //CtData.Update();
                    Global.d_CT = DateTime.Now.Subtract(dt_Temp).TotalSeconds; // Tính chu kỳ chu trình (CT)

                    dt_Temp = DateTime.Now; // Đặt mốc thời gian mới

                    return "料盘不良率判断"; // Chuyển sang: Đánh giá tỷ lệ lỗi khay

                case "料盘不良率判断": // [Sản xuất] Tính toán tỷ lệ OK của khay để quyết định xả thẳng vào kho hay đưa lên bàn tạm
                    int oknum = 0;
                    for (int i = 0; i < Global.slotTotalCount; i++)
                    {
                        if (Pickdata[i] == "OK")
                        {
                            oknum++;
                        }    
                    }

                    if ((oknum / Global.slotTotalCount) >= 0.8 || true) // Tỷ lệ OK >= 80% (hiện tại ép true đi thẳng vào kho)
                    {
                        return "移动到放料等待位"; // Chuyển sang: Đến vị trí chờ nhả khay vào kho
                    }
                    else
                    {
                        return "暂放平台有无料判断"; // Chuyển sang: Kiểm tra đặt vào bàn đệm tạm
                    }
                case "暂放平台有无料判断": // [Sản xuất] Kiểm tra xem bàn đệm tạm đã có khay chưa

                    if (!RegBit.暂放平台有料.Get()) // Nếu bàn tạm rỗng
                    {
                       
                        return "暂放平台放料流程"; // Đưa khay lên bàn tạm
                    }
                    else
                    {
                        return "移动到放料等待位"; // Bàn tạm đã có khay, tiếp tục đưa vào kho
                    }
                case "移动到放料等待位": // [Sản xuất] Trục X và Y di chuyển đến vị trí chờ nhả liệu vào các kho dỡ A/B/C

                    AxisSingleTo(AXIS.中转取放X轴, POINT.移载放料等待位); // Trục X đến vị trí chờ nhả khay
                    AxisSingleTo(AXIS.中转取放Y轴, POINT.移载放料等待位); // Trục Y đến vị trí chờ nhả khay
                    WaitAxis(AXIS.中转取放X轴); // Chờ trục X dừng
                    WaitAxis(AXIS.中转取放Y轴); // Chờ trục Y dừng

                    return "等待料仓移载可放料"; // Chuyển sang: Chờ kho dỡ báo sẵn sàng nhận khay伸出";
                //    }
                //    else
                //    {
                //        ShowMessage("暂放平台物料料盘未感应到", null, new string[] { "再次检测" });
                //        return "暂放平台检测有无料";
                //} 
                #endregion


                case "移动至出料接驳取料位上方":

                    AxisSingleTo(AXIS.中转取放X轴,POINT.出料接驳取料位 );
                    AxisSingleTo(AXIS.中转取放Y轴,POINT.出料接驳取料位 );
                    WaitAxis(AXIS.中转取放X轴);
                    WaitAxis(AXIS.中转取放Y轴);
                return "等待出料接驳可取料";

                case "等待出料接驳可取料":
                    if (RegBit.出料接驳站通知可取料.Get())
                    {
                        string strOutputTransMovID = RegString.Str_OutputTransMoveID.Get();
                        if (strOutputTransMovID == null || strOutputTransMovID == "")
                            Logger.Process(this.Name, "出料接驳动子号为空");
                        //productData = StationMgr.GetStation("出料接驳站").WorkSpace[StationOutputTrans.NestEnum.出料接驳站].GetProductData();

                        Pickdata = new string[Global.slotTotalCount];
                        NGReason = new string[Global.slotTotalCount];
                        S1StartTime = RedisHelperEx.Instance.HashGet($"{strOutputTransMovID}_Data", $"S1_StartTime");
                        S1InputQty = RedisHelperEx.Instance.HashGet($"{strOutputTransMovID}_Data", $"S1_InputQty");
                        for (int i = 0; i < Global.slotTotalCount; i++)//使用数据库传递
                        {
                            //productData.cavitydata[i] = RedisHelperEx.Instance.HashGet(Global.moveidnum, $"S6_{i + 1}");//S6站数据结果
                            Pickdata[i] = RedisHelperEx.Instance.HashGet(strOutputTransMovID, $"S6_{i + 1}");//S6站数据结果
                            NGReason[i] = RedisHelperEx.Instance.HashGet($"{strOutputTransMovID}_Data", $"{i + 1}_AOINGReason");
                            arrSN[i] = RedisHelperEx.Instance.HashGet($"{strOutputTransMovID}_Data", $"{i + 1}_SN");
                        }


                        RegBit.出料接驳站通知可取料.Set(false);
                        //获取出料接驳站数控
                        return "Z轴到取料位";

                    }
                    else
                     {
                      return "等待出料接驳可取料";
                     }

                case "Z轴到取料位":
                    //AxisSingleTo(AXIS.中转取放Z轴,POINT.出料接驳取料位 );
                    //WaitAxis(AXIS.中转取放Z轴);
                    CylinderOut(Cylinder.出料移载站升降);
                    CylinderOut(Cylinder.出料移载站解锁);
                    Delay(300);

                    double ZPos = POINT.出料接驳取料位.GetPos(AXIS.中转取放Z轴, this);
                    AXIS.中转取放Z轴.AbsMove(ZPos - 6, AxisHandle.SpeedPercent);
                    WaitAxis(AXIS.中转取放Z轴);
                    AXIS.中转取放Z轴.AbsMove(ZPos, 5.0);
                    WaitAxis(AXIS.中转取放Z轴);
                    return "取料时夹料盘夹紧";

                case "取料时夹料盘夹紧":
                    if (!IsDryRun)//正常跑
                     {
                        CylinderOut(Cylinder.中转取放站料盘);

                     }
                    else
                    {
                        CylinderOut(Cylinder.中转取放站料盘, false);
                        Delay(2000);
                     }
                  return "Z轴到安全高度";

                case "Z轴到安全高度":
                    ZPos = POINT.出料接驳取料位.GetPos(AXIS.中转取放Z轴, this);
                    AXIS.中转取放Z轴.AbsMove(ZPos - 5, 5.0);
                    WaitAxis(AXIS.中转取放Z轴);
                    AxisSingleTo(AXIS.中转取放Z轴, POINT.安全位);
                    WaitAxis(AXIS.中转取放Z轴);

                    string m_data = RedisHelperEx.Instance.HashGet(Global.m_RESPMonitorMover, "出料接驳站");
                    RedisHelperEx.Instance.HashSet(Global.m_RESPMonitorMover, this.Name, $"0,{m_data.Split(',')[1]}");

                    RegBit.中转取放站取料完成.Set(true);

                    //CtData.Update();
                    Global.d_CT = DateTime.Now.Subtract(dt_Temp).TotalSeconds;

                    dt_Temp = DateTime.Now;

                    return "料盘不良率判断";

                case "料盘不良率判断":
                    int oknum = 0;
                    for (int i = 0; i < Global.slotTotalCount; i++)
                    {
                        if (Pickdata[i] == "OK")
                        {
                            oknum++;
                        }    
                    }

                    if ((oknum / Global.slotTotalCount)>=0.8 || true)//数据整理部分,暂放平台禁用
                    {
                        return "移动到放料等待位";
                    }
                    else
                    {
                        return "暂放平台有无料判断";
                    }
                case "暂放平台有无料判断":

                    if (!RegBit.暂放平台有料.Get())//暂放平台无料就放料，有料继续
                    {
                       
                        return "暂放平台放料流程";
                    }
                    else
                    {
                        return "移动到放料等待位";
                    }
                case "移动到放料等待位":

                    AxisSingleTo(AXIS.中转取放X轴,POINT.移载放料等待位 );
                    AxisSingleTo(AXIS.中转取放Y轴,POINT.移载放料等待位 );
                    WaitAxis(AXIS.中转取放X轴);
                    WaitAxis(AXIS.中转取放Y轴);

                    return "等待料仓移载可放料";

                #region 料仓移载放料流程
                case "等待料仓移载可放料": // [Sản xuất] Chờ tín hiệu từ kho dỡ khay A, B hoặc C sẵn sàng nhận khay (kết hợp cấu hình bật/tắt kho)

                    if (RegBit.下料仓A站通知中转可放料.Get() && Parameter.GetBit("Enable_BinA")) // Kho A báo sẵn sàng và có bật sử dụng kho A
                    {
                        currentbin = "A"; // Chọn kho đích: Kho A
                        RegBit.下料仓A站通知中转可放料.Set(false); // Reset cờ
                        return "移动至料仓移载放料位上方"; // Chuyển sang: Trục XY đến phía trên kho A
                    }
                    else
                    if (RegBit.下料仓B站通知中转可放料.Get() && Parameter.GetBit("Enable_BinB") && !Parameter.GetBit("Enable_AOITest")) // Kho B sẵn sàng
                    {
                        currentbin = "B"; // Chọn kho đích: Kho B
                        RegBit.下料仓B站通知中转可放料.Set(false); // Reset cờ
                        return "移动至料仓移载放料位上方"; // Chuyển sang: Trục XY đến phía trên kho B
                    }
                    else
                    if (RegBit.下料仓C站通知中转可放料.Get() && Parameter.GetBit("Enable_BinC") && !Parameter.GetBit("Enable_AOITest")) // Kho C sẵn sàng
                    {
                        currentbin = "C"; // Chọn kho đích: Kho C
                        RegBit.下料仓C站通知中转可放料.Set(false); // Reset cờ
                        return "移动至料仓移载放料位上方"; // Chuyển sang: Trục XY đến phía trên kho C
                    }
                    else
                    {
                        return "等待料仓移载可放料"; // Tiếp tục chờ kho dỡ khay sẵn sàng
                    }
                case "移动至料仓移载放料位上方": // [Sản xuất] Trục X và Y di chuyển đến phía trên vị trí đặt khay của kho đã chọn
                    switch (currentbin)
                    {
                        case "A":
                            RegBit.下料仓A站通知中转可放料.Set(false);
                            AxisSingleTo(AXIS.中转取放X轴, POINT.下料仓A移载放料位); // Trục X đến vị trí kho A
                            AxisSingleTo(AXIS.中转取放Y轴, POINT.下料仓A移载放料位); // Trục Y đến vị trí kho A
                            WaitAxis(AXIS.中转取放X轴);
                            WaitAxis(AXIS.中转取放Y轴);
                            return "Z轴移动至料仓移载放料位"; // Chuyển sang: Trục Z hạ xuống kho A

                        case "B":
                            RegBit.下料仓B站通知中转可放料.Set(false);
                            AxisSingleTo(AXIS.中转取放X轴, POINT.下料仓B移载放料位); // Trục X đến vị trí kho B
                            AxisSingleTo(AXIS.中转取放Y轴, POINT.下料仓B移载放料位); // Trục Y đến vị trí kho B
                            WaitAxis(AXIS.中转取放X轴);
                            WaitAxis(AXIS.中转取放Y轴);
                            return "Z轴移动至料仓移载放料位"; // Chuyển sang: Trục Z hạ xuống kho B

                        case "C":
                            RegBit.下料仓C站通知中转可放料.Set(false);
                            AxisSingleTo(AXIS.中转取放X轴, POINT.下料仓C移载放料位); // Trục X đến vị trí kho C
                            AxisSingleTo(AXIS.中转取放Y轴, POINT.下料仓C移载放料位); // Trục Y đến vị trí kho C
                            WaitAxis(AXIS.中转取放X轴);
                            WaitAxis(AXIS.中转取放Y轴);
                            return "Z轴移动至料仓移载放料位"; // Chuyển sang: Trục Z hạ xuống kho C

                        default: 
                            return "移动至料仓移载放料位上方";
                    }
                case "Z轴移动至料仓移载放料位": // [Sản xuất] Trục Z hạ xuống vị trí đặt khay (hạ nhanh đến cách 7mm rồi hạ chậm 5mm/s)
                   
                    switch (currentbin)
                    {
                        case "A":
                            _placePoint = POINT.下料仓A移载放料位; // Điểm đặt kho A
                            break;

                        case "B":
                            _placePoint = POINT.下料仓B移载放料位; // Điểm đặt kho B
                            break;
                        case "C":
                            _placePoint = POINT.下料仓C移载放料位; // Điểm đặt kho C
                            break;
                        default:
                            _placePoint = POINT.下料仓A移载放料位;
                            return "Z轴移动至料仓移载放料位";
                    }

                    ZPos = _placePoint.GetPos(AXIS.中转取放Z轴, this);
                    AXIS.中转取放Z轴.AbsMove(ZPos - 7, AxisHandle.SpeedPercent); // Hạ nhanh đến cách điểm đặt 7mm
                    WaitAxis(AXIS.中转取放Z轴);
                    AXIS.中转取放Z轴.AbsMove(ZPos, 5.0); // Hạ chậm 5mm/s đến điểm đặt
                    WaitAxis(AXIS.中转取放Z轴);
                    return "夹爪气缸松开放料"; // Chuyển sang: Nhả kẹp khay

                case "夹爪气缸松开放料": // [Sản xuất] Co xi lanh tay gắp để nhả khay vào kho
                    if (!IsDryRun)
                    {
                        CylinderBack(Cylinder.中转取放站料盘, true); // Co xi lanh nhả khay
                    }
                    else
                    {
                        CylinderBack(Cylinder.中转取放站料盘, false);
                        Delay(200);
                    }
                

                    return "料仓移载解锁伸出"; // Chuyển sang: Mở khóa kho nhận khay

                case "料仓移载解锁伸出": // [Sản xuất] Duỗi xi lanh mở khóa nạp khay phía trước và sau của kho tương ứng
                    switch (currentbin)

                    {
                        case "A":
                            CylinderOut(Cylinder.下料仓A上料前解锁, false); // Duỗi xi lanh mở khóa kho A trước
                            CylinderOut(Cylinder.下料仓A上料后解锁, false); // Duỗi xi lanh mở khóa kho A sau
                            WaitCylOut(Cylinder.下料仓A上料前解锁);
                            WaitCylOut(Cylinder.下料仓A上料后解锁);
                            return "中转Z轴放料后离开"; // Chuyển sang: Trục Z rời khỏi kho

                        case "B":
                            CylinderOut(Cylinder.下料仓B上料前解锁, false); // Duỗi xi lanh mở khóa kho B trước
                            CylinderOut(Cylinder.下料仓B上料后解锁, false); // Duỗi xi lanh mở khóa kho B sau
                            WaitCylOut(Cylinder.下料仓B上料前解锁);
                            WaitCylOut(Cylinder.下料仓B上料后解锁);
                            return "中转Z轴放料后离开"; // Chuyển sang: Trục Z rời khỏi kho


                        case "C":
                            CylinderOut(Cylinder.下料仓C上料前解锁, false); // Duỗi xi lanh mở khóa kho C trước
                            CylinderOut(Cylinder.下料仓C上料后解锁, false); // Duỗi xi lanh mở khóa kho C sau
                            WaitCylOut(Cylinder.下料仓C上料前解锁);
                            WaitCylOut(Cylinder.下料仓C上料后解锁);
                            return "中转Z轴放料后离开"; // Chuyển sang: Trục Z rời khỏi kho


                        default: 
                            return "料仓移载解锁伸出";

                    }

                case "中转Z轴放料后离开": // [Sản xuất] Trục Z nhấc chậm 10mm rồi nâng nhanh về vị trí an toàn
                    ZPos = _placePoint.GetPos(AXIS.中转取放Z轴, this);
                    AXIS.中转取放Z轴.AbsMove(ZPos - 10, 10.0); // Nhấc lên 10mm tốc độ 10
                    WaitAxis(AXIS.中转取放Z轴);

                    AxisSingleTo(AXIS.中转取放Z轴, POINT.安全位); // Về vị trí an toàn
                    WaitAxis(AXIS.中转取放Z轴);
                    //this[NestEnum.中转取放站].Product = productData;//更新本站数据

                    return "更新数据到数据库"; // Chuyển sang: Cập nhật dữ liệu vào Redis

                case "更新数据到数据库": // [Sản xuất] Ghi dữ liệu kết quả từng ô, lý do NG, SN, thời gian vào Redis của kho tương ứng
                    switch (currentbin)

                    {
                        case "A":

                            redisName = "StationUnloadingBinA"; // Tên database kho A
                            break;

                        case "B":
                            redisName = "StationUnloadingBinB"; // Tên database kho B
                            break;
                        case "C":
                            redisName = "StationUnloadingBinC"; // Tên database kho C
                            break;
                        default:
                            ShowMessage($"{this.Name}数据库工站料仓不对", null, new string[] { "再次更新" });
                            return "更新数据到数据库";

                    }

                    for (int i = 0; i < Global.slotTotalCount; i++) // Ghi kết quả từng ô
                    {
                        RedisHelperEx.Instance.HashSet(redisName, $"S_{(i + 1)}", Pickdata[i]); // Kết quả ô S_i
                        RedisHelperEx.Instance.HashSet(redisName, $"S_{(i + 1)}_NGReason", NGReason[i]); // Lý do NG
                        RedisHelperEx.Instance.HashSet(redisName, $"S_{(i + 1)}_SN", arrSN[i]); // Mã SN
                    }
                    RedisHelperEx.Instance.HashSet(redisName, $"TrayStartTime", S1StartTime); // Thời gian bắt đầu
                    RedisHelperEx.Instance.HashSet(redisName, $"TrayInputQty", S1InputQty); // Số lượng khay

                    if (Parameter.GetBit("Enable_UnloadRandomResult")) // Chế độ test kết quả ngẫu nhiên (nếu bật cấu hình)
                    {
                        string[] randomNGNum = GenerateDetectStringArray(Global.slotTotalCount, 0.25, 0);

                        if (randomNGNum.Length == Global.slotTotalCount)
                        {
                            for (int i = 0; i < Global.slotTotalCount; i++)
                            {
                                string slotrResult = randomNGNum[i];
                                RedisHelperEx.Instance.HashSet(redisName, $"S_{(i + 1)}", slotrResult);
                            }
                        }
                    }
                   

                    AxisSingleTo(AXIS.中转取放Z轴, POINT.安全位); // Đảm bảo trục Z ở an toàn
                    WaitAxis(AXIS.中转取放Z轴);
                    //this[NestEnum.中转取放站].Product = productData;

                    return "通知料仓移载放料完成"; // Chuyển sang: Báo kho dỡ đã thả khay xong


                case "通知料仓移载放料完成": // [Sản xuất] Bật bit báo cho kho dỡ khay (A, B hoặc C) biết đã thả khay hoàn tất
                   
                    switch (currentbin)

                    {
                        case "A":

                            RegBit.下料仓A移载放料完成.Set(true); // Bit: Kho A hoàn thành nhận khay
                            return "等待移载放料完成信号复位";

                        case "B":
                            RegBit.下料仓B移载放料完成.Set(true); // Bit: Kho B hoàn thành nhận khay
                            return "等待移载放料完成信号复位";


                        case "C":
                            RegBit.下料仓C移载放料完成.Set(true); // Bit: Kho C hoàn thành nhận khay
                            return "等待移载放料完成信号复位";

                        default: return "通知料仓移载放料完成";

                    }
                case "等待移载放料完成信号复位": // [Sản xuất] Chờ kho dỡ khay nhận được tín hiệu và reset bit hoàn tất
                    switch (currentbin)
                    {
                        case "A":
                            if (RegBit.下料仓A移载放料完成.Get())
                            {
                                return "等待移载放料完成信号复位"; // Chờ kho A reset bit
                            }
                            else
                            {
                                return "判断暂放平台有无料"; // Chuyển sang: Kiểm tra bàn đệm tạm
                            }
                            
                        case "B":
                            if (RegBit.下料仓B移载放料完成.Get())
                            {
                                return "等待移载放料完成信号复位"; // Chờ kho B reset bit
                            }
                            else
                            {
                                return "判断暂放平台有无料"; // Chuyển sang: Kiểm tra bàn đệm tạm
                            }

                        case "C":
                            if (RegBit.下料仓C移载放料完成.Get())
                            {
                                return "等待移载放料完成信号复位"; // Chờ kho C reset bit
                            }
                            else
                            {
                                return "判断暂放平台有无料"; // Chuyển sang: Kiểm tra bàn đệm tạm
                            }

                        default: 
                            return "等待移载放料完成信号复位";

                    }

                #endregion

                #region 暂放平台取料流程
                case "判断暂放平台有无料": // [Sản xuất] Kiểm tra bàn đệm tạm có khay cần lấy ra xử lý không
                    if (false/*RegBit.暂放平台有料.Get()*//*|| IsDryRun*/)
                    {

                        return "暂放平台气缸伸出"; // Chuyển sang: Duỗi xi lanh bàn tạm

                    }
                    else
                    {
                        RedisHelperEx.Instance.HashSet(Global.m_RESPMonitorMover, this.Name, "0,0"); // Cập nhật giám sát: Đã hoàn tất chu trình
                        return "生产流程结束"; // Chuyển sang: Kết thúc chu trình sản xuất
                    }
                case "暂放平台气缸伸出": // [Sản xuất] Duỗi xi lanh bàn đệm tạm
                    CylinderOut(Cylinder.中转暂放平台); // Duỗi xi lanh bàn tạm

                    return "XY轴动到暂放平台取料位上方"; // Chuyển sang: Trục XY đến bàn tạm

                case "XY轴动到暂放平台取料位上方": // [Sản xuất] Trục X và Y di chuyển đến phía trên bàn đệm tạm
                    AxisSingleTo(AXIS.中转取放X轴, POINT.中转暂放平台取放位); // Trục X đến vị trí bàn tạm
                    AxisSingleTo(AXIS.中转取放Y轴, POINT.中转暂放平台取放位); // Trục Y đến vị trí bàn tạm
                    WaitAxis(AXIS.中转取放X轴);
                    WaitAxis(AXIS.中转取放Y轴);

                    return "Z轴到暂放平台取料位"; // Chuyển sang: Trục Z hạ xuống bàn tạm

                case "Z轴到暂放平台取料位": // [Sản xuất] Trục Z hạ xuống vị trí gắp trên bàn tạm
                    AxisSingleTo(AXIS.中转取放Z轴, POINT.中转暂放平台取放位);
                    WaitAxis(AXIS.中转取放Z轴);

                    return "取料夹爪气缸夹紧"; // Chuyển sang: Kẹp khay

                case "取料夹爪气缸夹紧": // [Sản xuất] Duỗi xi lanh kẹp chặt khay hàng trên bàn tạm
                    CylinderOut(Cylinder.中转取放站料盘); // Duỗi kẹp khay

                    return "中转Z轴上抬安全位"; // Chuyển sang: Trục Z lên an toàn

                case "中转Z轴上抬安全位": // [Sản xuất] Trục Z nhấc khay lên vị trí an toàn
                    AxisSingleTo(AXIS.中转取放Z轴, POINT.安全位);
                    WaitAxis(AXIS.中转取放Z轴);
                    return "取料后平台检测"; // Chuyển sang: Kiểm tra cảm biến sau gắp

                case "取料后平台检测": // [Sản xuất] Kiểm tra cảm biến bàn tạm sau khi gắp khay
                    if (DI.暂放平台料盘有无.Get())
                    {

                        ShowMessage("暂放平台信号仍感应有料", null, new string[] { "已取走" });

                        return "取料后平台检测";

                    }
                    else
                    {
                        Pickdata = bufferdata; // Cập nhật dữ liệu từ khay tạm vào mảng xử lý

                        RegBit.暂放平台有料.Set(false); // Reset bit bàn tạm có liệu
                        CylinderBack(Cylinder.中转暂放平台, false); // Co xi lanh bàn tạm
                        Delay(200);
                        return "移动到放料等待位"; // Đưa khay vào vị trí chờ nhả vào kho
                    } 
                #endregion

                #region 暂放平台放料流程
                case "暂放平台放料流程": // [Sản xuất] Bắt đầu quy trình đưa khay lên bàn đệm tạm
                    return "平台气缸伸出";
                case "平台气缸伸出": // [Sản xuất] Duỗi xi lanh bàn đệm tạm
                    CylinderOut(Cylinder.中转暂放平台);
                    return "移动至暂放平台放料位上方"; // Chuyển sang: Trục XY đến phía trên bàn tạm
                case "移动至暂放平台放料位上方": // [Sản xuất] Trục X và Y di chuyển đến phía trên vị trí đặt của bàn tạm

                    AxisSingleTo(AXIS.中转取放X轴, POINT.中转暂放平台取放位);
                    AxisSingleTo(AXIS.中转取放Y轴, POINT.中转暂放平台取放位);
                    WaitAxis(AXIS.中转取放X轴);
                    WaitAxis(AXIS.中转取放Y轴);

                    return "Z轴移动到暂放平台放料位"; // Chuyển sang: Trục Z hạ xuống bàn tạm

                case "Z轴移动到暂放平台放料位": // [Sản xuất] Trục Z hạ xuống điểm đặt trên bàn tạm
                    AxisSingleTo(AXIS.中转取放Z轴, POINT.中转暂放平台取放位);
                    WaitAxis(AXIS.中转取放Z轴);
                    return "夹爪气缸松开"; // Chuyển sang: Nhả kẹp khay

                case "夹爪气缸松开": // [Sản xuất] Co xi lanh nhả khay lên bàn đệm tạm
                    CylinderOut(Cylinder.中转取放站料盘);
                    return "Z轴上抬安全位"; // Chuyển sang: Trục Z lên an toàn

                case "Z轴上抬安全位": // [Sản xuất] Trục Z nâng lên vị trí an toàn
                    AxisSingleTo(AXIS.中转取放Z轴, POINT.安全位);
                    WaitAxis(AXIS.中转取放Z轴);
                    return "放料后平台检测"; // Chuyển sang: Kiểm tra cảm biến bàn tạm

                case "放料后平台检测": // [Sản xuất] Kiểm tra cảm biến bàn tạm xác nhận khay đã nằm đúng vị trí

                    if (DI.暂放平台料盘有无.Get()) // Đã cảm nhận được khay
                    {
                        CylinderBack(Cylinder.中转暂放平台); // Co xi lanh bàn tạm
                        bufferdata = Pickdata; // Lưu dữ liệu khay vào bộ đệm bàn tạm
                        RegBit.暂放平台有料.Set(true); // Bit: Đánh dấu bàn tạm đã có khay
                        return "移动至出料接驳取料位上方"; // Quay lại đón lượt khay tiếp theo từ bàn dỡ
                    }
                    else
                    {
                        ShowMessage("暂放平台物料料盘未感应到", null, new string[] { "再次检测" });
                        return "放料后平台检测";
                    }
                #endregion


                case "生产流程结束": // [Sản xuất] Hoàn thành chu trình sản xuất, quay về StepEntry
                    return StepEntry; // Về bước khởi đầu chu trình
                default:
                    return null;
            }

        }


        public string[] GenerateDetectStringArray(int totalCount, double maxNgRate, double naRate)
        {
            if (totalCount <= 0)
                throw new ArgumentException("总数必须大于0");
            if (maxNgRate + naRate >= 1)
                throw new ArgumentException("NG比例 + NA比例 不能大于等于1");

            Random rand = new Random();

            int naCount = (int)(totalCount * naRate);
            int maxNgCount = (int)(totalCount * maxNgRate);

            // =====模式选择，二选一注释切换=====
            // 模式1：NG直接使用最大允许数量（推荐，稳定接近不良率上限）
            int ngCount = maxNgCount;

            // 模式2：NG数量随机 0 ~ maxNgCount 之间波动，永远不超标
            //int ngCount = rand.Next(0, maxNgCount + 1);

            int okCount = totalCount - naCount - ngCount;

            List<string> list = new List<string>();
            list.AddRange(Enumerable.Repeat("OK", okCount));
            list.AddRange(Enumerable.Repeat("NG", ngCount));
            list.AddRange(Enumerable.Repeat("NA", naCount));

            // 随机打乱顺序
            for (int i = list.Count - 1; i > 0; i--)
            {
                int r = rand.Next(i + 1);
                string temp = list[i];
                list[i] = list[r];
                list[r] = temp;
            }

            return list.ToArray();
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
        public static double GetPos(this StationTransferPick.POINT pt, AXIS axis, StationBase station)
        {
            return station.dictPoint[(int)pt][$"a{(int)axis}"];
        }

        public static double GetPos(this StationTransferPick.POINT pt, AXIS axis, string strStation)
        {
            StationBase station = StationMgr.GetStation(strStation);
            if (station == null)
                return 0;

            return station.dictPoint[(int)pt][$"a{(int)axis}"];
        }
    }
}
