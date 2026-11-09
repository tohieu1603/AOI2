//m97- New UnloadBin Station
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

namespace Autoframe
{
    public class StationUnloadBinEx : StationEx
    {
        string cavitystatus;
        string m_strName;
        string station_strName;
        bool firstInit;//第一次初始化
        string[] redisdata = new string[Global.slotTotalCount];
        string[] arr_NgReason = new string[Global.slotTotalCount];
        string[] arr_SN = new string[Global.slotTotalCount];
        BinInfo currentBinTray;
       
      
        DI DI料仓上料前解锁伸出位, DI料仓上料后解锁伸出位, DI料仓下料侧推伸出位, DI料仓下料侧推缩回位, DI料仓下料前解锁伸出位, DI料仓下料后解锁伸出位, DI料仓下料满料位, DI料仓启动;
        DO DO料仓上料前解锁伸出, DO料仓上料前解锁缩回, DO料仓上料后解锁伸出, DO料仓上料后解锁缩回, DO料仓下料侧推伸出, DO料仓下料侧推缩回, DO料仓下料前解锁伸出, DO料仓下料前解锁缩回, DO料仓下料后解锁伸出, DO料仓下料后解锁缩回, DO料仓下料启动灯;
        Cylinder Cyl料仓上料前解锁, Cyl料仓上料后解锁, Cyl料仓下料侧推, Cyl料仓下料前解锁, Cyl料仓下料后解锁;

        RegBit Bit中转可放料, Bit中转放料完成, Bit_UnloadGantryInitSafety;
        RegBit Bit_InitSnapRequest, Bit_InitSnapFinish, Bit_InitSnapHaveTray, Bit_UnloadGantrySnapFinish, Bit_UnloadGantrySnapOK, Bit_RequestGantryLocateTray, Bit_NotifyTranferInitSafety;
        AXIS AXIS移载X轴, AXIS顶升Z轴;



        /// <summary>
        /// 构造函数，需要设置站位当前的IO输入，IO输出，轴方向及轴名称，以显示在手动页面方便操作
        /// </summary>
        /// <param name="strName"></param>
        public StationUnloadBinEx(string strName) : base(strName)
        {
            HardwareConfig();
            CheckPoint<POINT>();
            LoadContainer<CONTAINER, ProductData>();
            currentBinTray.SetStatus(BinStatus.NoTray);
        }

        void HardwareConfig()
        {
            if (this.Name == "下料仓A站")
            {
                DI料仓上料前解锁伸出位 = DI.下料仓A上料前解锁伸出位;
                DI料仓上料后解锁伸出位 = DI.下料仓A上料后解锁伸出位;
                DI料仓下料侧推伸出位 = DI.下料仓A下料侧推伸出位;
                DI料仓下料侧推缩回位 = DI.下料仓A下料侧推缩回位;
                DI料仓下料前解锁伸出位 = DI.下料仓A下料前解锁伸出位;
                DI料仓下料后解锁伸出位 = DI.下料仓A下料后解锁伸出位;
                DI料仓下料满料位 = DI.下料仓A下料满料位;
                DI料仓启动 = DI.下料仓A启动;

                DO料仓上料前解锁伸出 = DO.下料仓A上料前解锁伸出;
                DO料仓上料前解锁缩回 = DO.下料仓A上料前解锁缩回;
                DO料仓上料后解锁伸出 = DO.下料仓A上料后解锁伸出;
                DO料仓上料后解锁缩回 = DO.下料仓A上料后解锁缩回;
                DO料仓下料侧推伸出 = DO.下料仓A下料侧推伸出;
                DO料仓下料侧推缩回 = DO.下料仓A下料侧推缩回;
                DO料仓下料前解锁伸出 = DO.下料仓A下料前解锁伸出;
                DO料仓下料前解锁缩回 = DO.下料仓A下料前解锁缩回;
                DO料仓下料后解锁伸出 = DO.下料仓A下料后解锁伸出;
                DO料仓下料后解锁缩回 = DO.下料仓A下料后解锁缩回;
                DO料仓下料启动灯 = DO.下料仓A启动灯;


                Cyl料仓上料前解锁 = Cylinder.下料仓A上料前解锁;
                Cyl料仓上料后解锁 = Cylinder.下料仓A上料后解锁;
                Cyl料仓下料侧推 = Cylinder.下料仓A下料侧推;
                Cyl料仓下料前解锁 = Cylinder.下料仓A下料前解锁;
                Cyl料仓下料后解锁 = Cylinder.下料仓A下料后解锁;


                Bit中转可放料 = RegBit.下料仓A站通知中转可放料;
                Bit中转放料完成 = RegBit.下料仓A移载放料完成;
                Bit_UnloadGantryInitSafety = RegBit.UnloadGantryInitSafety;

                Bit_InitSnapRequest = RegBit.BinAInitSnapRequest;
                Bit_InitSnapHaveTray = RegBit.BinAInitSnapHaveTray;
                Bit_InitSnapFinish = RegBit.BinAInitSnapFinish;
                Bit_RequestGantryLocateTray = RegBit.BinARequestGantryLocateTray;
                Bit_UnloadGantrySnapFinish = RegBit.UnloadGantrySnapAFinish;
                Bit_UnloadGantrySnapOK = RegBit.UnloadGantrySnapAOK;
                Bit_NotifyTranferInitSafety = RegBit.BinANotifyTranferInitSafety;
                AXIS移载X轴 = AXIS.下料仓移载X1轴;
                AXIS顶升Z轴 = AXIS.下料仓顶升Z1轴;


                SwapController.binA = new BinInfo(Global.trayTotalRow, Global.trayTotalCol)
                {
                    Name = "BinA",
                    DbName = "StationUnloadingBinA",
                    iRegion = 1,
                    strRegion = "A",
                    RowDis = Global.trayRowGap,
                    ColDis = Global.trayColGap,
                    Role = BinRole.OK
                };
                currentBinTray = SwapController.binA;

                cavitystatus = "OK";
                m_strName = "下料仓A";
                station_strName = "StationUnloadingBinA";
            }
            if (this.Name == "下料仓B站")
            {
                DI料仓上料前解锁伸出位 = DI.下料仓B上料前解锁伸出位;
                DI料仓上料后解锁伸出位 = DI.下料仓B上料后解锁伸出位;
                DI料仓下料侧推伸出位 = DI.下料仓B下料侧推伸出位;
                DI料仓下料侧推缩回位 = DI.下料仓B下料侧推缩回位;
                DI料仓下料前解锁伸出位 = DI.下料仓B下料前解锁伸出位;
                DI料仓下料后解锁伸出位 = DI.下料仓B下料后解锁伸出位;
                DI料仓下料满料位 = DI.下料仓B下料满料位;
                DI料仓启动 = DI.下料仓B启动;

                DO料仓上料前解锁伸出 = DO.下料仓B上料前解锁伸出;
                DO料仓上料前解锁缩回 = DO.下料仓B上料前解锁缩回;
                DO料仓上料后解锁伸出 = DO.下料仓B上料后解锁伸出;
                DO料仓上料后解锁缩回 = DO.下料仓B上料后解锁缩回;
                DO料仓下料侧推伸出 = DO.下料仓B下料侧推伸出;
                DO料仓下料侧推缩回 = DO.下料仓B下料侧推缩回;
                DO料仓下料前解锁伸出 = DO.下料仓B下料前解锁伸出;
                DO料仓下料前解锁缩回 = DO.下料仓B下料前解锁缩回;
                DO料仓下料后解锁伸出 = DO.下料仓B下料后解锁伸出;
                DO料仓下料后解锁缩回 = DO.下料仓B下料后解锁缩回;
                DO料仓下料启动灯 = DO.下料仓B启动灯;


                Cyl料仓上料前解锁 = Cylinder.下料仓B上料前解锁;
                Cyl料仓上料后解锁 = Cylinder.下料仓B上料后解锁;
                Cyl料仓下料侧推 = Cylinder.下料仓B下料侧推;
                Cyl料仓下料前解锁 = Cylinder.下料仓B下料前解锁;
                Cyl料仓下料后解锁 = Cylinder.下料仓B下料后解锁;


                Bit中转可放料 = RegBit.下料仓B站通知中转可放料;
                Bit中转放料完成 = RegBit.下料仓B移载放料完成;
                Bit_UnloadGantryInitSafety = RegBit.UnloadGantryInitSafety;

                Bit_InitSnapRequest = RegBit.BinBInitSnapRequest;
                Bit_InitSnapFinish = RegBit.BinBInitSnapFinish;
                Bit_InitSnapHaveTray = RegBit.BinBInitSnapHaveTray;
                Bit_RequestGantryLocateTray = RegBit.BinBRequestGantryLocateTray;
                Bit_UnloadGantrySnapFinish = RegBit.UnloadGantrySnapBFinish;
                Bit_UnloadGantrySnapOK = RegBit.UnloadGantrySnapBOK;
                Bit_NotifyTranferInitSafety = RegBit.BinBNotifyTranferInitSafety;
                AXIS移载X轴 = AXIS.下料仓移载X2轴;
                AXIS顶升Z轴 = AXIS.下料仓顶升Z2轴;

                SwapController.binB = new BinInfo(Global.trayTotalRow, Global.trayTotalCol)
                {
                    Name = "BinB",
                    DbName = "StationUnloadingBinB",
                    iRegion = 2,
                    strRegion = "B",
                    RowDis = Global.trayRowGap,
                    ColDis = Global.trayColGap,
                    Role = BinRole.NG
                };
                currentBinTray = SwapController.binB;

                cavitystatus = "NG";
                m_strName = "下料仓B";
                station_strName = "StationUnloadingBinB";
            }
            if (this.Name == "下料仓C站")
            {
                DI料仓上料前解锁伸出位 = DI.下料仓C上料前解锁伸出位;
                DI料仓上料后解锁伸出位 = DI.下料仓C上料后解锁伸出位;
                DI料仓下料侧推伸出位 = DI.下料仓C下料侧推伸出位;
                DI料仓下料侧推缩回位 = DI.下料仓C下料侧推缩回位;
                DI料仓下料前解锁伸出位 = DI.下料仓C下料前解锁伸出位;
                DI料仓下料后解锁伸出位 = DI.下料仓C下料后解锁伸出位;
                DI料仓下料满料位 = DI.下料仓C下料满料位;
                DI料仓启动 = DI.下料仓C启动;

                DO料仓上料前解锁伸出 = DO.下料仓C上料前解锁伸出;
                DO料仓上料前解锁缩回 = DO.下料仓C上料前解锁缩回;
                DO料仓上料后解锁伸出 = DO.下料仓C上料后解锁伸出;
                DO料仓上料后解锁缩回 = DO.下料仓C上料后解锁缩回;
                DO料仓下料侧推伸出 = DO.下料仓C下料侧推伸出;
                DO料仓下料侧推缩回 = DO.下料仓C下料侧推缩回;
                DO料仓下料前解锁伸出 = DO.下料仓C下料前解锁伸出;
                DO料仓下料前解锁缩回 = DO.下料仓C下料前解锁缩回;
                DO料仓下料后解锁伸出 = DO.下料仓C下料后解锁伸出;
                DO料仓下料后解锁缩回 = DO.下料仓C下料后解锁缩回;
                DO料仓下料启动灯 = DO.下料仓C启动灯;



                Cyl料仓上料前解锁 = Cylinder.下料仓C上料前解锁;
                Cyl料仓上料后解锁 = Cylinder.下料仓C上料后解锁;
                Cyl料仓下料侧推 = Cylinder.下料仓C下料侧推;
                Cyl料仓下料前解锁 = Cylinder.下料仓C下料前解锁;
                Cyl料仓下料后解锁 = Cylinder.下料仓C下料后解锁;



                Bit中转可放料 = RegBit.下料仓C站通知中转可放料;
                Bit中转放料完成 = RegBit.下料仓C移载放料完成;
                Bit_UnloadGantryInitSafety = RegBit.UnloadGantryInitSafety;

                Bit_InitSnapRequest = RegBit.BinCInitSnapRequest;
                Bit_InitSnapFinish = RegBit.BinCInitSnapFinish;
                Bit_InitSnapHaveTray = RegBit.BinCInitSnapHaveTray;
                Bit_RequestGantryLocateTray = RegBit.BinCRequestGantryLocateTray;
                Bit_UnloadGantrySnapFinish = RegBit.UnloadGantrySnapCFinish;
                Bit_UnloadGantrySnapOK = RegBit.UnloadGantrySnapCOK;
                Bit_NotifyTranferInitSafety = RegBit.BinCNotifyTranferInitSafety;
                AXIS移载X轴 = AXIS.下料仓移载X3轴;
                AXIS顶升Z轴 = AXIS.下料仓顶升Z3轴;

                SwapController.binC = new BinInfo(Global.trayTotalRow, Global.trayTotalCol)
                {
                    Name = "BinC",
                    DbName = "StationUnloadingBinC",
                    iRegion = 3,
                    strRegion = "C",
                    RowDis = Global.trayRowGap,
                    ColDis = Global.trayColGap,
                    Role = SwapController.UnloadMode == 0 ? BinRole.OK : BinRole.NA
                };
                currentBinTray = SwapController.binC;

                cavitystatus = "OK";
                m_strName = "下料仓C";
                station_strName = "StationUnloadingBinC";
            }
            //配置站位界面显示输入
            arrayDI = new object[] { DI料仓上料前解锁伸出位, DI料仓上料后解锁伸出位, DI料仓下料侧推伸出位, DI料仓下料侧推缩回位, DI料仓下料前解锁伸出位, DI料仓下料后解锁伸出位, DI料仓下料满料位, DI料仓启动 };
            //配置站位界面显示输出
            arrayDO = new object[] { DO料仓上料前解锁伸出, DO料仓上料前解锁缩回, DO料仓上料后解锁伸出, DO料仓上料后解锁缩回, DO料仓下料侧推伸出, DO料仓下料侧推缩回, DO料仓下料前解锁伸出, DO料仓下料前解锁缩回, DO料仓下料后解锁伸出, DO料仓下料后解锁缩回, DO料仓下料启动灯 };
            //配置站位界面显示气缸
            arrayCylinder = new object[] { Cyl料仓上料前解锁, Cyl料仓上料后解锁, Cyl料仓下料侧推, Cyl料仓下料前解锁, Cyl料仓下料后解锁 };
            //配置站位轴
            arrayAxis = new object[] { AXIS移载X轴, AXIS顶升Z轴 };
        }

        ProductData productData = new ProductData();

        /// <summary>
        /// 站位内的点位枚举
        /// </summary>
        public enum POINT
        {
            中转放料位,
            分拣拍照位,
            顶升料盘位,
            顶升安全位


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
            AXIS移载X轴.ServoOn(false);
            AXIS顶升Z轴.ServoOn(false);

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
        /// 初始化时设置站位安全状态，用来设备初始化时各站位之间相互卡站，
        /// 控制各站位先后动作顺序用，比如流水线组装，肯定需要组装的Z轴升
        /// 起后，流水线才能动作这种情况
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
            Bit中转放料完成.Set(false); // Bit: Reset cờ hoàn thành đặt khay từ trung chuyển

            Bit_InitSnapRequest.Set(false); // Bit: Reset yêu cầu chụp ảnh khởi tạo
            Bit_InitSnapFinish.Set(false); // Bit: Reset cờ chụp ảnh khởi tạo hoàn tất
            Bit_InitSnapHaveTray.Set(false); // Bit: Reset cờ phát hiện có khay khởi tạo
            Bit_UnloadGantrySnapFinish.Set(false); // Bit: Reset cờ chụp ảnh Unload Gantry hoàn tất
            Bit_UnloadGantrySnapOK.Set(false); // Bit: Reset cờ chụp ảnh Unload Gantry OK
            Bit_RequestGantryLocateTray.Set(false); // Bit: Reset cờ yêu cầu Gantry định vị khay
            Bit_NotifyTranferInitSafety.Set(false); // Bit: Reset cờ thông báo an toàn khởi tạo cho trung chuyển
        }

        protected override string InitRun(string strStep)
        {
            switch (strStep)
            {
                case StepEntry: // [Khởi tạo] Bắt đầu chu trình khởi tạo trạm kho dỡ khay (Unload Bin)

                    return "AxisServoOn"; // Chuyển sang: Bật servo các trục                 

                case "AxisServoOn": // [Khởi tạo] Đặt trạng thái khay NoTray, bật servo trục X chuyển khay và trục Z nâng
                    currentBinTray.SetStatus(BinStatus.NoTray); // Trạng thái khay: Chưa có khay
                    AXIS移载X轴.ServoOn(true); // Bật Servo trục X chuyển khay
                    AXIS顶升Z轴.ServoOn(true); // Bật Servo trục Z nâng khay
                    Delay(1000); // Trì hoãn 1000ms chờ servo ổn định
                    return "料仓启动灯关"; // Chuyển sang: Tắt đèn nút Start kho khay

                case "料仓启动灯关": // [Khởi tạo] Tắt đèn nút nhấn Start của kho khay                  

                    DO料仓下料启动灯.Set(false); // DO: Tắt đèn nút Start kho dỡ khay

                    return "上料解锁气缸伸出"; // Chuyển sang: Duỗi xi lanh mở khóa nạp khay

                case "上料解锁气缸伸出": // [Khởi tạo] Duỗi xi lanh mở khóa nạp khay trước và sau
                    CylinderOut(Cyl料仓上料前解锁); // Duỗi xi lanh mở khóa nạp khay phía trước
                    CylinderOut(Cyl料仓上料后解锁); // Duỗi xi lanh mở khóa nạp khay phía sau

                    return "下料侧推气缸缩回"; // Chuyển sang: Co xi lanh đẩy cạnh dỡ khay

                case "下料侧推气缸缩回": // [Khởi tạo] Co xi lanh đẩy cạnh dỡ khay

                    CylinderBack(Cyl料仓下料侧推); // Co xi lanh đẩy cạnh/đẩy ngang dỡ khay

                    return "下料解锁气缸伸出"; // Chuyển sang: Duỗi xi lanh mở khóa dỡ khay

                case "下料解锁气缸伸出": // [Khởi tạo] Duỗi xi lanh mở khóa dỡ khay trước và sau
                    CylinderOut(Cyl料仓下料前解锁); // Duỗi xi lanh mở khóa dỡ khay phía trước
                    CylinderOut(Cyl料仓下料后解锁); // Duỗi xi lanh mở khóa dỡ khay phía sau

                    return "ZAxisMoveToSafePos"; // Chuyển sang: Trục Z về vị trí an toàn


                case "ZAxisMoveToSafePos": // [Khởi tạo] Trục Z nâng khay về vị trí an toàn
                    AxisSingleTo(AXIS顶升Z轴, POINT.顶升安全位); // Trục Z di chuyển đến điểm an toàn
                    WaitAxis(AXIS顶升Z轴); // Chờ trục Z dừng hẳn
                    return "WaitTranferGantryInitSafety"; // Chuyển sang: Chờ Gantry trung chuyển an toàn

                //20260921 - 等待中转龙门初始化安全
                case "WaitTranferGantryInitSafety": // [Khởi tạo] Chờ cụm Gantry trung chuyển khởi tạo an toàn
                    if (RegBit.TranferGantryInitSafety.Get()) // Bit: Kiểm tra cờ an toàn của Transfer Gantry
                    {
                        //RegBit.TranferGantryInitSafety.Set(false);
                        return "WaitUnloadGantryInitSafety"; // Chuyển sang: Chờ Unload Gantry an toàn
                    }
                    else
                    {
                        return "WaitTranferGantryInitSafety"; // Tiếp tục chờ Transfer Gantry an toàn
                    }

                //等待下料龙门初始化安全
                case "WaitUnloadGantryInitSafety": // [Khởi tạo] Chờ cụm Gantry dỡ khay khởi tạo an toàn
                    if (Bit_UnloadGantryInitSafety.Get()) // Bit: Kiểm tra cờ an toàn của Unload Gantry
                    {
                        return "XAxisMoveToSnapPos"; // Chuyển sang: Trục X đến vị trí chụp ảnh
                    }
                    else
                    {
                        return "WaitUnloadGantryInitSafety"; // Tiếp tục chờ Unload Gantry an toàn
                    }
                case "XAxisMoveToSnapPos": // [Khởi tạo] Trục X di chuyển đến vị trí chụp ảnh phân loại khay
                    AxisSingleTo(AXIS移载X轴, POINT.分拣拍照位); // Trục X di chuyển đến vị trí chụp ảnh phân loại
                    WaitAxis(AXIS移载X轴); // Chờ trục X dừng hẳn
                    Bit_NotifyTranferInitSafety.Set(true); // Bit: Bật cờ báo an toàn cho trạm trung chuyển
                    return "RequestGantrySnap"; // Chuyển sang: Yêu cầu Gantry chụp ảnh

                case "RequestGantrySnap": // [Khởi tạo] Gửi yêu cầu chụp ảnh kiểm tra có khay trong kho không
                    Bit_InitSnapHaveTray.Set(false); // Reset bit phát hiện khay
                    Bit_InitSnapFinish.Set(false); // Reset bit hoàn tất chụp
                    Bit_InitSnapRequest.Set(true); // Bit: Bật yêu cầu chụp ảnh khởi tạo
                    return "WaitGantrySnapFinish"; // Chuyển sang: Chờ Gantry chụp xong

                case "WaitGantrySnapFinish": // [Khởi tạo] Chờ Gantry chụp ảnh kiểm tra hoàn tất
                    if (Bit_InitSnapFinish.Get()) // Bit: Đã chụp xong
                    {
                        Bit_InitSnapFinish.Set(false); // Reset cờ chụp xong
                        return "WaitUnloadGantryInitSnapFinish"; // Chuyển sang: Chờ quy trình chụp Unload Gantry kết thúc
                    }
                    else
                    {
                        return "WaitGantrySnapFinish"; // Tiếp tục chờ chụp xong
                    }
                case "WaitUnloadGantryInitSnapFinish": // [Khởi tạo] Chờ toàn bộ quy trình chụp khởi tạo của Unload Gantry xong
                    if (RegBit.UnloadInitSnapProcessDone.Get()) // Bit: Toàn bộ quá trình chụp khởi tạo đã xong
                    {
                        return "JudgeInitHaveTrayOrNot"; // Chuyển sang: Phán đoán có khay hay không
                    }
                    else
                    {
                        return "WaitUnloadGantryInitSnapFinish"; // Tiếp tục chờ hoàn thành xử lý ảnh
                    }

                case "JudgeInitHaveTrayOrNot": // [Khởi tạo] Phán đoán xem kho có sẵn khay từ kết quả chụp ảnh không
                    if (Bit_InitSnapHaveTray.Get()) // Nếu có khay sẵn trong kho
                    {
                        currentBinTray.SetStatus(BinStatus.TranferPutTray); // Cập nhật trạng thái: Đã có khay từ trung chuyển
                        currentBinTray.SetMatrixBySQL(); // Tải ma trận dữ liệu khay từ CSDL
                        return "InitializeFinished"; // Chuyển sang: Hoàn tất khởi tạo
                    }
                    else // Nếu không có khay
                    {                    
                        currentBinTray.SetStatus(BinStatus.NoTray); // Cập nhật trạng thái: Không có khay
                        currentBinTray.ClearMatrix(); // Xóa ma trận dữ liệu khay
                        return "XAxisMoveToTranferPos"; // Chuyển sang: Trục X về vị trí nhận khay trung chuyển
                    }


                case "XAxisMoveToTranferPos": // [Khởi tạo] Trục X di chuyển đến vị trí nhận khay từ trung chuyển
                    AxisSingleTo(AXIS移载X轴, POINT.中转放料位); // Trục X di chuyển đến vị trí đặt khay trung chuyển
                    WaitAxis(AXIS移载X轴); // Chờ trục X dừng hẳn
                    return "InitializeFinished"; // Chuyển sang: Hoàn tất khởi tạo

                case "InitializeFinished": // [Khởi tạo] Hoàn tất khởi tạo trạm kho dỡ khay
                    currentBinTray.ClearUnloadCount(); // Xóa bộ đếm số khay đã dỡ
                    firstInit = true; // Đánh dấu lần khởi tạo đầu tiên
                    DO料仓下料启动灯.Set(false); // DO: Tắt đèn nút Start kho dỡ khay
                   // Bit料仓站初始化完成.Set(true);
                    //WaitAllStaReady();
                    //结束初始化
                    return StepExit; // Kết thúc chu trình khởi tạo trạm kho dỡ khay
                default:
                    return null;
            }
        }

        protected override string ProduceRun(string strStep)
        {
            switch (strStep)
            {
                case StepEntry: // [Sản xuất] Bắt đầu chu trình tự động kho dỡ khay

                    return "JudgeBinWhetherHaveTray"; // Chuyển sang: Kiểm tra kho có sẵn khay không
                case "JudgeBinWhetherHaveTray": // [Sản xuất] Kiểm tra khay: nếu vừa khởi tạo và phát hiện có sẵn khay thì chụp ảnh định vị, ngược lại đi đón khay
                    if (firstInit)
                    {
                        firstInit = false; // Đánh dấu đã qua lần khởi tạo đầu
                        if (Bit_InitSnapHaveTray.Get()) // Nếu chụp ảnh khởi tạo thấy có khay
                        {
                            Bit_InitSnapHaveTray.Set(false); // Reset cờ có khay khởi tạo
                            return "MoveToSnapPos"; // Chuyển sang: Di chuyển đến vị trí chụp ảnh
                        }
                        else
                        {
                            return "XAxisMoveToTranferPos"; // Chuyển sang: Đi nhận khay từ trung chuyển
                        }
                    }
                    else
                    {
                        return "XAxisMoveToTranferPos"; // Chuyển sang: Đi nhận khay từ trung chuyển
                    }

                case "XAxisMoveToTranferPos": // [Sản xuất] Trục X di chuyển đến vị trí nhận khay từ trạm trung chuyển (Transfer)
                    AxisSingleTo(AXIS移载X轴, POINT.中转放料位, bAllowInterrupt: true); // Trục X chạy đến vị trí nhận khay trung chuyển
                    WaitAxis(AXIS移载X轴); // Chờ trục X dừng hẳn
                    return "上料解锁气缸缩回"; // Chuyển sang: Co xi lanh mở khóa nạp khay
                case "上料解锁气缸缩回": // [Sản xuất] Co xi lanh mở khóa nạp khay (trước và sau) để kẹp/giữ khay
                    CylinderBack(Cyl料仓上料前解锁, false); // Co xi lanh mở khóa nạp khay trước
                    CylinderBack(Cyl料仓上料后解锁, false); // Co xi lanh mở khóa nạp khay sau
                    WaitDI(DI料仓上料前解锁伸出位, false); // Chờ ngắt cảm biến duỗi xi lanh mở khóa trước
                    WaitDI(DI料仓上料后解锁伸出位, false); // Chờ ngắt cảm biến duỗi xi lanh mở khóa sau
                    Delay(200); // Trì hoãn 200ms ổn định cơ cấu

                    return "NotifyTranferPlaceTray"; // Chuyển sang: Báo trạm trung chuyển thả khay

                case "NotifyTranferPlaceTray": // [Sản xuất] Bật cờ báo cho trạm trung chuyển biết có thể thả khay vào kho

                    //流道模式
                    Bit中转可放料.Set(true); // Bit: Cho phép trạm trung chuyển thả khay
                    return "WaitTranferPlaceFinish"; // Chuyển sang: Chờ trạm trung chuyển thả khay xong

                case "WaitTranferPlaceFinish": // [Sản xuất] Chờ trạm trung chuyển hoàn tất việc thả khay

                    if (Bit中转放料完成.Get()) // Bit: Trạm trung chuyển đã thả khay xong
                    {
                        //productData = StationMgr.GetStation("中转取放站").WorkSpace[StationTransferPick.NestEnum.中转取放站].GetProductData();
                        //StationMgr.GetStation("中转取放站").WorkSpace[StationTransferPick.NestEnum.中转取放站].Product = null;
                        Bit中转放料完成.Set(false); // Reset cờ trung chuyển thả khay xong
                        return "GetRedisData"; // Chuyển sang: Đọc dữ liệu khay từ Redis
                    }
                    else
                    {
                        return "WaitTranferPlaceFinish"; // Tiếp tục chờ trung chuyển thả khay
                    }

                case "GetRedisData": // [Sản xuất] Đọc dữ liệu từ Redis đồng bộ trạng thái các ô khay, mã lỗi NG, SN và thời gian
                    for (int i = 0; i < Global.slotTotalCount; i++) // Vòng lặp cập nhật từng ô (slot)
                    {
                        redisdata[i] = RedisHelperEx.Instance.HashGet(currentBinTray.DbName, $"S_{i + 1}"); // Đọc trạng thái slot S_i từ Redis
                        arr_NgReason[i] = RedisHelperEx.Instance.HashGet(currentBinTray.DbName, $"S_{i + 1}_NGReason"); // Đọc lý do lỗi NG của slot S_i
                        arr_SN[i] = RedisHelperEx.Instance.HashGet(currentBinTray.DbName, $"S_{i + 1}_SN"); // Đọc mã vạch SN của slot S_i
                        if (!Enum.TryParse<SlotState>(redisdata[i], out SlotState item))
                            item = SlotState.NA; // Mặc định NA nếu parse lỗi
                        currentBinTray.SetSlotState(i, item); // Lưu trạng thái slot vào bộ nhớ
                        currentBinTray.SetSlotNGReason(i, arr_NgReason[i]); // Lưu lý do lỗi
                        currentBinTray.SetSlotSN(i, arr_SN[i]); // Lưu mã SN
                    }


                    string str_dtStart = RedisHelperEx.Instance.HashGet(currentBinTray.DbName, $"TrayStartTime"); // Đọc thời gian bắt đầu khay
                    if (!DateTime.TryParse(str_dtStart, out currentBinTray.StartTime))
                        currentBinTray.StartTime = DateTime.Now; // Mặc định thời gian hiện tại nếu rỗng

                    string str_inputQty = RedisHelperEx.Instance.HashGet(currentBinTray.DbName, $"TrayInputQty"); // Đọc số lượng đầu vào
                    int iQty = currentBinTray.TraySlotCount;
                    int.TryParse(str_inputQty, out iQty);
                    currentBinTray.InputQty = iQty; // Gán số lượng khay nhận
                    return "上料解锁伸出1"; // Chuyển sang: Duỗi xi lanh mở khóa nạp khay

                case "上料解锁伸出1": // [Sản xuất] Duỗi xi lanh mở khóa nạp khay (trước và sau) nhả kẹp
                    CylinderOut(Cyl料仓上料前解锁); // Duỗi xi lanh mở khóa trước
                    CylinderOut(Cyl料仓上料后解锁); // Duỗi xi lanh mở khóa sau
                    WaitCylOut(Cyl料仓上料前解锁); // Chờ xi lanh trước duỗi hoàn toàn
                    WaitCylOut(Cyl料仓上料后解锁); // Chờ xi lanh sau duỗi hoàn toàn
                    return "MoveToSnapPos"; // Chuyển sang: Trục X đưa khay đến vị trí chụp ảnh
             
                case "MoveToSnapPos": // [Sản xuất] Trục X đưa khay đến vị trí chụp ảnh phân loại / định vị
                    currentBinTray.SetStatus(BinStatus.TranferPutTray); // Cập nhật trạng thái khay: Đã nhận từ trung chuyển
                    AxisSingleTo(AXIS移载X轴, POINT.分拣拍照位, (int)(AxisHandle.SpeedPercent * 0.7)); // Trục X chạy đến vị trí chụp ảnh (70% tốc độ)
                    WaitAxis(AXIS移载X轴); // Chờ trục X dừng hẳn

                    return "XAxisMultiMove1"; // Chuyển sang: Lắc nhẹ khay lần 1 (nếu bật chế độ)

                case "XAxisMultiMove1": // [Sản xuất] Lắc nhẹ trục X (dịch chuyển qua lại +-5mm 3 lần) để ổn định sản phẩm trong khay
                    if (Parameter.GetInt("UnloadTrayMultiMoveMode") == 1 || Parameter.GetInt("UnloadTrayMultiMoveMode") == 3)
                    {
                        for (int i = 0; i < 3; i++) // Lặp rung lắc 3 lần
                        {
                            AXIS移载X轴.RelativeMove(-5, 100); // Lùi 5mm tốc độ 100
                            WaitAxis(AXIS移载X轴);
                            AXIS移载X轴.RelativeMove(5, 100); // Tiến 5mm tốc độ 100
                            WaitAxis(AXIS移载X轴);
                        }
                    }                  
                    return "RequestGantryLocateTray"; // Chuyển sang: Yêu cầu Gantry chụp ảnh định vị khay

                case "RequestGantryLocateTray": // [Sản xuất] Yêu cầu Gantry dỡ khay chụp ảnh định vị khay
                    //流道模式
                    if (Global.IsPickProduct && !currentBinTray.IsPairing) // Chế độ vét hàng không ghép cặp
                    {
                        return "XAxisMoveToLiftPos"; // Chuyển thẳng sang: Trục X đi đến vị trí nâng khay
                    }
                    else if (Parameter.GetBit("Enable_AOITest") && !currentBinTray.IsPairing) // Chế độ test AOI
                    {
                        return "XAxisMoveToLiftPos"; // Chuyển thẳng sang: Trục X đi đến vị trí nâng khay
                    }
                    else
                    {
                        Bit_RequestGantryLocateTray.Set(true); // Bit: Gửi yêu cầu Unload Gantry chụp ảnh định vị khay
                    }
                    return "WaitGantryLocateFinish"; // Chuyển sang: Chờ Gantry định vị khay xong



                case "WaitGantryLocateFinish": // [Sản xuất] Chờ cụm Unload Gantry hoàn thành chụp ảnh định vị khay

                    if (Bit_UnloadGantrySnapFinish.Get()) // Bit: Unload Gantry đã chụp xong
                    {
                        Bit_UnloadGantrySnapFinish.Set(false); // Reset cờ chụp xong
                        return "AnalysSnapResult"; // Chuyển sang: Phân tích kết quả chụp
                    }
                    return "WaitGantryLocateFinish"; // Tiếp tục chờ chụp xong

                case "AnalysSnapResult": // [Sản xuất] Phân tích kết quả chụp ảnh định vị khay

                    if (Bit_UnloadGantrySnapOK.Get()) // Bit: Chụp ảnh định vị OK
                    {
                        return "JudgeTrayWhetherFull"; // Chuyển sang: Kiểm tra khay đã đầy sản phẩm chưa
                    }
                    else
                    {
                        return "MoveToSnapPos"; // Định vị lỗi, quay lại vị trí chụp ảnh thử lại
                    }

                case "JudgeTrayWhetherFull": // [Sản xuất] Kiểm tra xem khay đã chứa đủ số lượng sản phẩm theo vai trò (OK/NG) chưa
                    SlotState curBinState = SlotState.OK;
                    if (currentBinTray.Role == BinRole.NG)
                        curBinState = SlotState.NG;
                    if (currentBinTray.Role == BinRole.NA)
                        curBinState = SlotState.NA;

                    bool b_IsMatching = true;
                    foreach (var item in currentBinTray.GetAllSlotState())
                    {
                        if (item != curBinState)
                        {
                            b_IsMatching = false;
                            break;
                        }
                    }
                    if (b_IsMatching) // Tất cả các slot đều đã được lấp đầy đúng trạng thái
                    {
                        currentBinTray.SetStatus(BinStatus.ProcessCompleted); // Khay đã hoàn thành xử lý
                    }
                    else
                    {
                        currentBinTray.SetStatus(BinStatus.Ready); // Khay sẵn sàng đón thêm sản phẩm
                    }

                   
                    return "WaitUnloadTraySignal"; // Chuyển sang: Chờ tín hiệu dỡ khay

                case "WaitUnloadTraySignal": // [Sản xuất] Chờ tín hiệu hoàn tất quá trình nhả liệu vào khay để đưa khay ra kho
                    bool IsPickProduct = Global.IsPickProduct; // Cờ chế độ vét sạch hàng (PickProduct)
                    if (IsPickProduct && !currentBinTray.IsPairing)
                    {
                        currentBinTray.SetStatus(BinStatus.ProcessCompleted); // Ở chế độ vét hàng thì ép trạng thái hoàn tất khay
                    }

                    if (currentBinTray.Status == BinStatus.ProcessCompleted) // Nếu khay đã xong
                    {
                        if (!currentBinTray.CanMove()) // Kiểm tra khóa an toàn liên trạm xem khay có được phép di chuyển không
                        {
                            return "WaitUnloadTraySignal"; // Chưa thể di chuyển thì chờ tiếp
                        }
                        return "XAxisMultiMove2"; // Chuyển sang: Lắc nhẹ khay lần 2 trước khi nâng vào kho
                    }
                    else
                    {
                        return "WaitUnloadTraySignal"; // Khay chưa xong, tiếp tục chờ
                    }
                case "XAxisMultiMove2": // [Sản xuất] Lắc nhẹ trục X lần 2 (dịch chuyển qua lại +-5mm 3 lần) trước khi dỡ
                    if (Parameter.GetInt("UnloadTrayMultiMoveMode") == 2 || Parameter.GetInt("UnloadTrayMultiMoveMode") == 3)
                    {
                        for (int i = 0; i < 3; i++) // Lặp rung lắc 3 lần
                        {
                            AXIS移载X轴.RelativeMove(-5, 100); // Lùi 5mm
                            WaitAxis(AXIS移载X轴);
                            AXIS移载X轴.RelativeMove(5, 100); // Tiến 5mm
                            WaitAxis(AXIS移载X轴);
                        }
                    }

                    return "XAxisMoveToLiftPos"; // Chuyển sang: Trục X đưa khay đến vị trí kích nâng
              

                case "XAxisMoveToLiftPos": // [Sản xuất] Trục X đưa khay đến vị trí kích nâng dỡ khay
                    AxisSingleTo(AXIS移载X轴, POINT.顶升料盘位, (int)(AxisHandle.SpeedPercent * 0.7)); // Chạy đến vị trí nâng khay (70% tốc độ)
                    WaitAxis(AXIS移载X轴); // Chờ trục X dừng hẳn

                    return "UpdateEndTime"; // Chuyển sang: Ghi thời gian kết thúc khay


                case "UpdateEndTime": // [Sản xuất] Cập nhật thời gian hoàn thành khay (TrayEndTime) lên Redis
                    currentBinTray.EndTime = DateTime.Now; // Ghi nhận thời gian kết thúc
                    RedisHelperEx.Instance.HashSet(currentBinTray.DbName, $"TrayEndTime", DateTime.Now.ToString("yyyyMMdd_HH:mm:ss.fff")); // Ghi Redis
                    return "下料解锁气缸缩回"; // Chuyển sang: Co xi lanh mở khóa dỡ khay


                case "下料解锁气缸缩回": // [Sản xuất] Co xi lanh mở khóa dỡ khay (trước và sau) để chuẩn bị đón khay mới nâng lên
                    if (!IsDryRun) // Chế độ sản xuất thật
                    {
                        CylinderBack(Cyl料仓下料前解锁, false); // Co xi lanh mở khóa dỡ khay trước
                        CylinderBack(Cyl料仓下料后解锁, false); // Co xi lanh mở khóa dỡ khay sau
                        WaitDI(DI料仓下料前解锁伸出位, false); // Chờ cảm biến duỗi tắt
                        WaitDI(DI料仓下料后解锁伸出位, false); // Chờ cảm biến duỗi tắt
                        Delay(100); // Trì hoãn 100ms

                    }
                    else // Chế độ chạy không tải (Dry Run)
                    {
                        CylinderBack(Cyl料仓下料前解锁, false);
                        CylinderBack(Cyl料仓下料后解锁, false);
                        WaitDI(DI料仓下料前解锁伸出位, false);
                        WaitDI(DI料仓下料后解锁伸出位, false);
                        Delay(200);
                    }
                    return "下料侧推气缸伸出"; // Chuyển sang: Duỗi xi lanh đẩy cạnh dỡ khay
                case "下料侧推气缸伸出": // [Sản xuất] Duỗi xi lanh đẩy cạnh dỡ khay định vị (bước đệm)
                    //if (!IsDryRun)
                    //{
                    //    CylinderOut(Cyl料仓下料侧推);
                    //    Delay(200);
                    //}
                    return "下料侧推气缸缩回"; // Chuyển sang: Co xi lanh đẩy cạnh dỡ khay

                case "下料侧推气缸缩回": // [Sản xuất] Co xi lanh đẩy cạnh dỡ khay

                    CylinderBack(Cyl料仓下料侧推); // Co xi lanh đẩy cạnh
                    Delay(100); // Trì hoãn 100ms

                    return "ZAxisMoveToLiftPos"; // Chuyển sang: Trục Z nâng khay lên
                    
                case "ZAxisMoveToLiftPos": // [Sản xuất] Trục Z nâng khay lên vị trí nhả vào ngăn chứa khay
                    AxisSingleTo(AXIS顶升Z轴, POINT.顶升料盘位, 10); // Trục Z chạy tốc độ chậm 10 đến vị trí nâng khay
                    WaitAxis(AXIS顶升Z轴); // Chờ trục Z dừng hẳn
                    Delay(300); // Trì hoãn 300ms
                    return "ZAxisMoveToSafePos"; // Chuyển sang: Trục Z hạ về vị trí an toàn

                case "ZAxisMoveToSafePos": // [Sản xuất] Trục Z hạ lùi 12mm rồi hạ tiếp về vị trí an toàn, phát sự kiện hoàn thành 1 khay      
                    double ZPos = POINT.顶升料盘位.GetPos(AXIS顶升Z轴, this) - 12; // Hạ bớt 12mm trước
                    //AXIS顶升Z轴.AbsMove(ZPos, 3);
                    HandleEx.AbsMove(AXIS顶升Z轴, ZPos, 10); // Hạ 12mm ở tốc độ 10
                    WaitAxis(AXIS顶升Z轴); // Chờ trục Z dừng               

                    AxisSingleTo(AXIS顶升Z轴, POINT.顶升安全位, AxisHandle.SpeedPercent); // Trục Z về vị trí an toàn
                    WaitAxis(AXIS顶升Z轴); // Chờ trục Z dừng hẳn
                    EventMgrLib.EventMgr.Ins.GetEvent<AutoFrame.UI.RefreshDataMessage>().Publish(new AutoFrame.UI.RefreshDataMessage() { Message = $"{this.Name}=>下了一盘料" }); // Phát thông báo UI: Đã dỡ xong 1 khay
                    return "UploadData"; // Chuyển sang: Upload dữ liệu MES

                case "UploadData": // [Sản xuất] Upload dữ liệu khay lên hệ thống MES                  
                    if (Parameter.GetBit("MesEnable")) // Nếu bật chức năng kết nối MES
                    {
                        string strMsg = "";
                        if (!MesMgr.SaveTestData(currentBinTray.ConvertToMesData(), out strMsg)) // Gọi lưu dữ liệu MES
                        {
                            int iRet = ShowMessage($"MES上传数据失败:{strMsg}", "MesUploadFail", new string[] { "重新上传".txtTrans(TransType.Trans), "跳过".txtTrans(TransType.Trans) });
                            if (iRet == 0) // Operator chọn tải lại
                                return "UploadData"; // Tải lại dữ liệu MES
                            else
                                Logger.Process(this.Name, "MES upload fail!"); // Ghi log bỏ qua lỗi MES
                        }
                        else
                            Logger.Process(this.Name, "MES upload successfully!"); // Log: Gửi MES thành công
                    }                  
                    return "UpdateData"; // Chuyển sang: Đưa dữ liệu vào hàng đợi xử lý

                case "UpdateData": // [Sản xuất] Đưa dữ liệu khay vào hàng đợi sản phẩm ExtensionHelper.productQueue
                    ExtensionHelper.productQueue.Enqueue(currentBinTray.ConvertToTrayData()); // Thêm vào queue
                    return "下料解锁气缸伸出"; // Chuyển sang: Duỗi xi lanh mở khóa dỡ khay

                case "下料解锁气缸伸出": // [Sản xuất] Duỗi xi lanh mở khóa dỡ khay giữ chặt chồng khay vừa nâng

                    CylinderOut(Cyl料仓下料前解锁); // Duỗi xi lanh mở khóa dỡ khay trước
                    CylinderOut(Cyl料仓下料后解锁); // Duỗi xi lanh mở khóa dỡ khay sau
                    WaitCylOut(Cyl料仓下料前解锁); // Chờ xi lanh trước duỗi hoàn toàn
                    WaitCylOut(Cyl料仓下料后解锁); // Chờ xi lanh sau duỗi hoàn toàn                  
                    return "ResetTrayInfo"; // Chuyển sang: Reset thông tin khay
                case "ResetTrayInfo": // [Sản xuất] Reset trạng thái khay NoTray, tăng đếm khay dỡ, xóa key Redis
                    currentBinTray.SetStatus(BinStatus.NoTray); // Trạng thái khay: Không có khay
                    currentBinTray.IncreaseUnloadCount(); // Tăng bộ đếm số khay đã dỡ
                    RedisHelperEx.Instance.KeyDelete("0", currentBinTray.DbName); // Xóa dữ liệu khay trên Redis
                    return "CheckWhetherFullTray"; // Chuyển sang: Kiểm tra cảm biến đầy khay
                case "CheckWhetherFullTray": // [Sản xuất] Kiểm tra cảm biến đầy kho dỡ khay
                    if (DI料仓下料满料位.Get() && !IsDryRun) // DI: Cảm biến đầy kho dỡ khay tác động (khi không phải dry-run)
                    {
                        return "NotifyOPUnloadTray"; // Chuyển sang: Thông báo Operator lấy khay ra
                    }
                    else
                    {
                        return "上料解锁伸出2"; // Chuyển sang: Mở khóa nạp khay chuẩn bị lượt tiếp
                    }
                case "NotifyOPUnloadTray": // [Sản xuất] Bật đèn và hiện thông báo yêu cầu Operator lấy khay ra và nhấn Start
                    DO料仓下料启动灯.Set(true); // DO: Bật đèn nút Start kho khay
                    ShowMessage($"{m_strName}满料，请拿走然后按启动".txtTrans(TransType.Trans)); // Thông báo: Kho đầy khay, vui lòng lấy ra và bấm Start
                    return "WaitUnloadButton"; // Chuyển sang: Chờ nút bấm Start
                case "WaitUnloadButton": // [Sản xuất] Chờ Operator bấm nút Start kho khay sau khi đã lấy khay ra
                    if (DI料仓启动.Get()) // DI: Operator nhấn nút Start
                    {
                        DO料仓下料启动灯.Set(false); // DO: Tắt đèn nút Start
                        return "CheckWhetherFullTray"; // Quay lại kiểm tra xem cảm biến đầy khay đã tắt chưa

                    }
                    else
                    {
                        return "WaitUnloadButton"; // Tiếp tục chờ nút bấm
                    }

                case "上料解锁伸出2": // [Sản xuất] Duỗi xi lanh mở khóa nạp khay (trước và sau) chuẩn bị đón khay mới
                    CylinderOut(Cyl料仓上料前解锁); // Duỗi xi lanh mở khóa nạp khay trước
                    CylinderOut(Cyl料仓上料后解锁); // Duỗi xi lanh mở khóa nạp khay sau
                    WaitCylOut(Cyl料仓上料前解锁); // Chờ xi lanh trước duỗi hoàn toàn
                    WaitCylOut(Cyl料仓上料后解锁); // Chờ xi lanh sau duỗi hoàn toàn
                    return "EndProcess"; // Chuyển sang: Kết thúc chu trình            
                case "EndProcess": // [Sản xuất] Kết thúc chu trình sản xuất khay, quay về StepEntry
                    return StepEntry; // Về bước khởi đầu chu trình sản xuất
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
        public static double GetPos(this StationUnloadBinEx.POINT pt, AXIS axis, StationBase station)
        {
            return station.dictPoint[(int)pt][$"a{(int)axis}"];
        }

        public static double GetPos(this StationUnloadBinEx.POINT pt, AXIS axis, string strStation)
        {
            StationBase station = StationMgr.GetStation(strStation);
            if (station == null)
                return 0;

            return station.dictPoint[(int)pt][$"a{(int)axis}"];
        }
    }
}
