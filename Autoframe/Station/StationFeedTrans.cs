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

namespace Autoframe
{
    /// <summary>
    /// 入料接驳站
    /// </summary>
    public class StationFeedTrans : StationEx
    {
        string moverid;//动子ID号
        string moverid2;//动子ID号                                
        TcpClient PLC;
        int i_count;
        //PlcHelper m_plcHelper;

        /// <summary>
        /// 构造函数，需要设置站位当前的IO输入，IO输出，轴方向及轴名称，以显示在手动页面方便操作
        /// </summary>
        /// <param name="strName"></param>
        public StationFeedTrans(string strName) : base(strName)
        {
            HardwareConfig();
            CheckPoint<POINT>();
            LoadContainer<CONTAINER, ProductData>();

            //PLC = TcpClientMgr.GetClient(8);
            //PlcMgrEx.GetInstance().m_dicNETIP.Clear();
            //PlcMgrEx.GetInstance().m_dicNETPort.Clear();
            //PlcMgrEx.GetInstance().m_dicReg_StartAdress.Clear();

            //PlcMgrEx.GetInstance().m_dicNETIP.Add("0", PLC.m_strIP);
            //PlcMgrEx.GetInstance().m_dicNETPort.Add("0", PLC.m_nPort.ToString());
            //PlcMgrEx.GetInstance().m_dicReg_StartAdress.Add("0_PLC寄存器起始地址", "1000");
            //m_plcHelper = new PlcHelper();
        }

        void HardwareConfig()
        {
            //配置站位界面显示输入
            arrayDI = new object[] { DI.入料移载站升降伸出位, DI.入料移载站升降缩回位,DI.入料移载站解锁伸出位, DI.入料移载站解锁缩回位 };
            //配置站位界面显示输出
            arrayDO = new object[] { DO.入料移载站升降伸出,DO.入料移载站升降缩回,DO.入料移载站解锁伸出,DO.入料移载站解锁缩回 };
            //配置站位界面显示气缸
            arrayCylinder = new object[] {Cylinder.入料移载站升降, Cylinder.入料移载站解锁  };
            //配置站位轴
            arrayAxis = new object[] { };

            PLC = TcpClientMgr.GetClient(8);
            PlcMgrEx.GetInstance().m_dicNETIP.Clear();
            PlcMgrEx.GetInstance().m_dicNETPort.Clear();
            PlcMgrEx.GetInstance().m_dicReg_StartAdress.Clear();

            PlcMgrEx.GetInstance().m_dicNETIP.Add("0", PLC.m_strIP);
            PlcMgrEx.GetInstance().m_dicNETPort.Add("0", PLC.m_nPort.ToString());
            PlcMgrEx.GetInstance().m_dicReg_StartAdress.Add("0_PLC寄存器起始地址", "1000");
            Global.m_plcHelper = new PlcHelper();

            if (Global.RedisInit())
            {
                Logger.Process(this.Name, "数据库连接成功".txtTrans(TransType.Trans));
            }
            else
            {
                Logger.Process(this.Name, "数据库连接失败".txtTrans(TransType.Trans));
            }
        }

        ProductData productData = new ProductData();

        //TcpCommu_Camera camera = new TcpCommu_Camera();
        /// <summary>
        /// 站位内的点位枚举
        /// </summary>
        public enum POINT
        {


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
        //protected override void SetFirstStep()
        {
            switch (Process)
            {
                case AutoProcess.正常生产:
                case AutoProcess.空跑:

                    break;
                //case AutoProcess.相机标定:

                //    break;
                default:
                    this.InitEntry = StepExit;
                    this.AutoEntry = StepEmpty;
                    break;
            }

            RegBit.入料接驳站初始化完成.Set(false);
            RegBit.上料取放站放料完成.Set(false);
            RegBit.上料取放站拍照完成.Set(false);
            RegBit.入料接驳站通知可拍照.Set(false);
            RegBit.入料接驳站通知可放料.Set(false);
            //Global.m_plcHelper.WriteData_Int16(0, new int[] { ((int)WritePLC.上位机准备ready) }, 0);//上位机Reday F
        }

        protected override string InitRun(string strStep)
        {
        protected override string InitRun(string strStep)
        {
            switch (strStep)
            {
                case StepEntry:
				    return "ConnectPLC"; // [Khởi tạo] Bắt đầu kết nối PLC

                case "ConnectPLC": // [Khởi tạo] Kiểm tra và thiết lập kết nối TCP/IP tới PLC Omron/Mitsubishi
                    if (!Global.m_plcHelper.IsConnected(0))
                    {
                        if (!Global.m_plcHelper.ConnectPLC(0))
                        {
                            if (!Global.m_plcHelper.IsConnected(0))
                            {
                                ShowMessage("PLC连接失败，是否重连"); // Báo lỗi: Kết nối PLC thất bại, có muốn thử lại không?
                            }
                        }
                    }
                    else
                    {
                        Logger.Process(this.Name, "PLC连接成功"); // Log: Kết nối PLC thành công
                    }
                    return "通知上位机Ready置F"; // Báo PLC trạng thái PC Ready = False

                case "通知上位机Ready置F": // [Khởi tạo] Reset bit PC Ready về 0 trong PLC
                    Global.m_plcHelper.WriteData_Int16(0, new int[] { ((int)WritePLC.上位机准备ready) }, 0); // Ghi PLC: PC Ready = 0
                    return "等待上料取放料站初始化完成"; // Chờ trạm gắp đặt上料 khởi tạo xong

                case "等待上料取放料站初始化完成": // [Khởi tạo] Đợi cờ khởi tạo xong từ StationLoadingPickEx
                    if (RegBit.上料取放站初始化完成.Get())
                    {
                        return "入料解锁松开"; // Mở khóa bàn tiếp nhận
                    }
                    else
                    {
                        return "等待上料取放料站初始化完成";
                    }

                case "入料解锁松开": // [Khởi tạo] Thu xi lanh khóa bàn tiếp nhận mover
                    CylinderBack(Cylinder.入料移载站解锁); // Thu xi lanh mở khóa
                    return "入料升降缩回"; // Thu xi lanh nâng hạ bàn tiếp nhận

                case "入料升降缩回": // [Khởi tạo] Hạ bàn tiếp nhận xuống vị trí thấp
                    CylinderBack(Cylinder.入料移载站升降); // Thu xi lanh nâng hạ
                    return "等待磁悬浮各站初始化完成"; // Chờ tất cả các trạm trên ray từ tính hoàn tất

                case "等待磁悬浮各站初始化完成": // [Khởi tạo] Chờ cờ khởi tạo từ trạm Laser, AOI dưới 1, AOI dưới 2, AOI trên
                    if (RegBit.激光位移检测站初始化完成.Get() && RegBit.下相机AOI1站初始化完成.Get() && RegBit.下相机AOI2站初始化完成.Get() && RegBit.上相机AOI站初始化完成.Get())
                    {
                        return "通知PLC上位机安全"; // Báo PLC toàn bộ máy đã an toàn
                    }
                    else
                    {
                        return "等待磁悬浮各站初始化完成";
                    }

                case "通知PLC上位机安全": // [Khởi tạo] Bật tín hiệu an toàn và tín hiệu Ready gửi PLC
                    Global.m_plcHelper.WriteData_Int16(0, new int[] { ((int)WritePLC.上位机安全信号) }, 1); // Ghi PLC: Tín hiệu an toàn PC = 1
                    Global.m_plcHelper.WriteData_Int16(0, new int[] { ((int)WritePLC.上位机准备ready) }, 1); // Ghi PLC: PC Ready = 1
                    return "SendPLCTrayID"; // Gửi mã Tray ID xuống PLC

                case "SendPLCTrayID": // [Khởi tạo] Gửi mã loại khay và tên khay sang PLC
                    Delay(1000);
                    Global.m_plcHelper.WriteData_Int16(0, new int[] { ((int)WritePLC.TrayIDRegister) }, Global.TrayID); // Ghi PLC: Mã khay TrayID
                    Global.m_plcHelper.WriteData_String(0, new int[] { ((int)WritePLC.TrayName) }, Global.trayTypeName); // Ghi PLC: Tên khay
                    i_count = 0;
                    Delay(1000);
                    return "ConfirmPLCTrayID"; // Đọc lại từ PLC để xác nhận trùng khớp

                case "ConfirmPLCTrayID": // [Khởi tạo] Đối chiếu TrayID đọc về từ PLC với phần mềm
                    int iRet = Global.m_plcHelper.ReadData_Int16(0, new int[] { ((int)ReadPLC.TrayIDRegister) });
                    if (iRet == Global.TrayID)
                    {
                        return "WaitPLCReadySignal"; // Trùng khớp -> chờ tín hiệu PLC Ready
                    }
                    else
                    {
                        i_count++;
                        Global.m_plcHelper.WriteData_Int16(0, new int[] { ((int)WritePLC.TrayIDRegister) }, Global.TrayID);
                        Delay(200);
                        if (i_count > 3)
                            ShowMessage($"PC TrayID:{Global.TrayID} and PLC TrayID{iRet} not matching"); // Cảnh báo không trùng khớp TrayID
                        return "ConfirmPLCTrayID";
                    }

                case "WaitPLCReadySignal": // [Khởi tạo] Chờ tín hiệu PLC Ready = 1
                    if (Global.m_plcHelper.ReadData_Int16(0, new int[] { ((int)ReadPLC.PLC准备ready) }) == 1)
                    {
                        return "数据库设置动子位置"; // Khởi tạo vị trí mover trong Redis
                    }
                    else
                    {
                        return "WaitPLCReadySignal";
                    }

                case "数据库设置动子位置": // [Khởi tạo] Xóa dữ liệu giám sát mover cũ và reset về Null trong Redis
                    if (RedisHelperEx.Instance.Keycheck("0", Global.m_RESPMonitorMover))
                    {
                        if (RedisHelperEx.Instance.KeyDelete("0", Global.m_RESPMonitorMover))
                        {
                            Delay(100);
                            for (int i = 0; i < Global.ListNameStation.Length; i++)
                            {
                                RedisHelperEx.Instance.HashSet(Global.m_RESPMonitorMover, Global.ListNameStation[i], "Null"); // Gán trạng thái trạm = Null
                            }
                            return "初始化完成";
                        }
                        else
                            return "数据库设置动子位置";
                    }
                    else
                    {
                        Delay(100);
                        for (int i = 0; i < Global.ListNameStation.Length; i++)
                        {
                            RedisHelperEx.Instance.HashSet(Global.m_RESPMonitorMover, Global.ListNameStation[i], "Null");
                        }
                        return "初始化完成";
                    }

                case "初始化完成": // [Khởi tạo] Hoàn tất khởi tạo trạm tiếp nhận
                    RegBit.入料接驳站初始化完成.Set(true); // Bật bit: Trạm tiếp nhận khởi tạo xong
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
				    return "生产流程开始";

			    case "生产流程开始":
                    return "等待动子到位"; // Chờ mover vào vị trí nạp khay

                case "等待动子到位": // [Sản xuất] Đọc tín hiệu PLC báo mover đã đến vị trí tiếp nhận
                    if (Global.m_plcHelper.ReadData_Int16(0, new int[] { ((int)ReadPLC.磁浮上料工位到位信号) }) == 1)
                    {
                        return "获取动子号"; // Đọc ID mover
                    }
                    return "等待动子到位";
						
                case "获取动子号": // [Sản xuất] Lấy mã ID của mover từ PLC và lưu vào mảng
                    moverid = Global.m_plcHelper.ReadData_Int16(0, new int[] { ((int)ReadPLC.磁浮上料工位动子号) }).ToString();
                    Global.arr_moveID[0] = moverid;
                    Logger.Process(this.Name, $"动子已到位-动子号:{moverid}"); // Log: Mover đã vào vị trí - Mã: moverid

                    RedisHelperEx.Instance.HashSet(Global.m_RESPMonitorMover, this.Name, $"{moverid},2"); // Cập nhật giám sát mover lên Redis: mã mover, trạng thái 2 (đang nạp)

                    return "判断是否是流道模式"; // Kiểm tra chế độ chạy xả khay

                case "判断是否是流道模式": // [Sản xuất] Kiểm tra xem có đang kích hoạt chế độ gắp xả sản phẩm không
                    if (Global.IsPickProduct)
                    {
                        if (ShowMessage("选择动子流走还是等待清空料盘", "动子状态判定", new string[] { "动子流走", "等待清空料盘" }) == 0)
                        {
                            return "通知动子可离开到拍照位"; // Cho mover trôi thẳng sang vị trí chụp
                        }
                        else
                        {
                            return "等待清空料盘"; // Chờ gắp hết sản phẩm
                        }
                    }
                    return "检查数据库当前动子是否存在"; // Kiểm tra dữ liệu mover trên Redis

                case "等待清空料盘":
                    Delay(3000);
                    return "判断是否是流道模式";

                case "检查数据库当前动子是否存在": // [Sản xuất] Kiểm tra xem ID mover này đã có trong Redis chưa
                    if (RedisHelperEx.Instance.Keycheck("0", moverid.ToString()))
                    {
                        return "删除数据库当前动子号"; // Đã có -> xóa dữ liệu cũ của lượt chạy trước
                    }
                    else
                    {
                        return "数据库设置初始值"; // Chưa có -> tạo mới
                    }

                case "删除数据库当前动子号": // [Sản xuất] Xóa sạch dữ liệu cũ của mover này trong Redis
                    if (RedisHelperEx.Instance.KeyDelete("0", moverid.ToString()))
                    {
                        RedisHelperEx.Instance.KeyDelete("0", $"{moverid}_Data"); // Xóa dữ liệu phụ
                        return "数据库设置初始值";
                    }
                    else
                    {
                        int select = ShowMessage($"动子{moverid}数据库删除失败", null, new string[] { "再次删除", "已删除OK" });
                        return select == 0 ? "删除数据库当前动子号" : "数据库设置初始值";
                    }

                case "数据库设置初始值": // [Sản xuất] Khởi tạo toàn bộ trạng thái các ô khay cho 6 trạm (S1..S6) thành "Null"
                    // S1: 入料接驳站 (Tiếp nhận/Gắp khay), S2: 激光位移检测站 (Đo cao Laser), S3: 下相机AOI1站 (AOI dưới 1), S4: 上相机AOI站 (AOI trên), S5: 下相机AOI2站 (AOI dưới 2), S6: 出料接驳站 (Xuất khay)
                    for (int i = 0; i < 6; i++)
                    {
                        int currstationid = i + 1;
                        for (int j = 0; j < Global.slotTotalCount; j++)
                        {
                            RedisHelperEx.Instance.HashSet(moverid, $"S{currstationid}_{j+1}", "Null"); // Gán trạng thái ban đầu là Null
                        }
                    }
                    return "入料升降伸出";

                case "入料升降伸出": // [Sản xuất] Nâng bàn tiếp nhận khay lên
                    return "入料解锁伸出";

                case "入料解锁伸出": // [Sản xuất] Mở chốt định vị mover
                    return "通知上料取放站可放料"; // Báo cho trạm gắp (StationLoadingPickEx) có thể thả khay

                case "通知上料取放站可放料": // [Sản xuất] Bật cờ cho phép trạm gắp đặt khay vào mover
                    RegBit.入料接驳站通知可放料.Set(true); // Bật bit: Cho phép đặt khay
                    return "等待上料取放站放料完成"; // Chờ trạm gắp thả khay xong

                case "等待上料取放站放料完成": // [Sản xuất] Đợi trạm gắp xác nhận đã đặt khay lên mover xong
                    if (RegBit.上料取放站放料完成.Get())
                    {
                        RedisHelperEx.Instance.HashSet(Global.m_RESPMonitorMover, this.Name, $"{moverid},1"); // Cập nhật trạng thái mover: 1 (đã có khay)
                        RegBit.上料取放站放料完成.Set(false); // Reset bit
                        return "入料解锁缩回"; // Khóa chốt định vị mover lại
                    }
                    else
                    {
                        return "等待上料取放站放料完成";
                    }

                case "入料解锁缩回": // [Sản xuất] Thu xi lanh mở khóa mover (khóa định vị)
                    CylinderBack(Cylinder.入料移载站解锁);
                    return "入料升降缩回"; // Hạ bàn tiếp nhận xuống ray

                case "入料升降缩回": // [Sản xuất] Hạ xi lanh nâng bàn tiếp nhận xuống ray từ tính
                    CylinderBack(Cylinder.入料移载站升降);
                    return "通知动子可离开到拍照位"; // Báo PLC cho mover chạy sang vị trí chụp

                case "通知动子可离开到拍照位": // [Sản xuất] Ghi PLC cho phép mover chạy từ vị trí nạp khay sang vị trí chụp ảnh
                    Global.m_plcHelper.WriteData_Int16(0, new int[] { ((int)WritePLC.磁浮上料工位完成信号) }, 1); // Ghi PLC: Hoàn tất trạm tiếp nhận = 1
                    Logger.Process(this.Name, $"动子号:{moverid} - 通知动子离开：1");
                    return "等待动子到拍照位"; // Chờ mover sang đến vị trí chụp

                case "等待动子到拍照位": // [Sản xuất] Đọc tín hiệu PLC kiểm tra mover đã tới vị trí chụp ảnh chưa
                    if (Global.m_plcHelper.ReadData_Int16(0, new int[] { ((int)ReadPLC.磁浮拍照工位到位信号) }) == 1)
                    {
                        return "再次获取动子号"; // Đọc lại ID mover ở vị trí chụp
                    }
                    else
                    {
                        return "等待动子到拍照位";
                    }

                case "再次获取动子号": // [Sản xuất] Đọc ID mover tại trạm chụp từ PLC
                    moverid2 = Global.m_plcHelper.ReadData_Int16(0, new int[] { ((int)ReadPLC.磁浮拍照工位动子号) }).ToString();
                    Logger.Process(this.Name, $"拍照动子已到位-动子号:{moverid2}");
                    return "动子号比较"; // So sánh ID mover có trùng khớp không

                case "动子号比较": // [Sản xuất] Đối chiếu xem mover tới vị trí chụp có đúng là mover vừa nhận khay không
                    Logger.Process(this.Name, $"上料动子号:{moverid}-拍照动子号:{moverid2}");
                    if (moverid == moverid2)
                    {
                        if (Global.IsPickProduct)
                        {
                            return "等待拍照动子到位";
                        }
                        return "通知上料取放站可拍照"; // Trùng khớp -> báo trạm gắp tiến hành chụp ảnh định vị
                    }
                    else
                    {
                        return "再次获取动子号"; // Chưa trùng -> đọc lại
                    }

                case "通知上料取放站可拍照": // [Sản xuất] Bật cờ cho phép trạm gắp StationLoadingPickEx kích camera chụp khay
                    productData = new ProductData();
                    RegBit.入料接驳站通知可拍照.Set(true); // Bật bit: Cho phép trạm gắp chụp ảnh
                    return "等待上料取放站拍照完成"; // Chờ trạm gắp chụp xong

                case "等待上料取放站拍照完成": // [Sản xuất] Đợi trạm gắp chụp ảnh và ghi dữ liệu Redis xong
                    if (RegBit.上料取放站拍照完成.Get())
                    {                      
                        RegBit.上料取放站拍照完成.Set(false); // Reset bit
                        return "获取前站数据";
                    }
                    else
                    {
                        return "等待上料取放站拍照完成";
                    }

                case "获取前站数据":
                    return "更新redis数据库";

                case "更新redis数据库": // [Sản xuất] Kiểm tra tính hợp lệ của mã mover
                    if (string.IsNullOrEmpty(moverid2))
                    {
                        Logger.Step(this.Name, "当前工站未找到动子号", 00, "上料动子到位信号复位");
                        return "上料动子到位信号复位";
                    }
                    return "等待拍照动子到位";


#region 动子号为空
                case "上料动子到位信号复位":
                    if (Global.m_plcHelper.ReadData_Int16(0, new int[] { ((int)ReadPLC.磁浮上料工位到位信号) }) == 1)
                    {
                        Global.m_plcHelper.WriteData_Int16(0, new int[] { ((int)WritePLC.磁浮上料工位完成信号) }, 1);
                        Logger.Process(this.Name, "磁浮上料工位到位信号为1");
                    }
                    return "通知上料动子可离开";

                case "通知上料动子可离开":
                    Global.m_plcHelper.WriteData_Int16(0, new int[] { ((int)WritePLC.磁浮上料工位完成信号) }, 1);
                    return "等待拍照动子到位";


                case "等待拍照动子到位": // [Sản xuất] Đợi tín hiệu hoàn tất trạm chụp từ PLC
                    if (Global.m_plcHelper.ReadData_Int16(0, new int[] { ((int)ReadPLC.磁浮拍照工位到位信号) }) == 1)
                    {
                        Global.m_plcHelper.WriteData_Int16(0, new int[] { ((int)WritePLC.磁浮拍照工位到位信号) }, 0);
                        Logger.Process(this.Name, "磁浮拍照工位到位信号为1");
                    }
                    return "通知拍照动子可离开"; // Báo mover rời khỏi vị trí chụp để sang trạm Laser

                case "通知拍照动子可离开": // [Sản xuất] Báo PLC cho mover rời khỏi trạm chụp sang trạm Laser, cập nhật trạng thái Redis Wait1
                    if (Global.IsPickProduct)
                    {
                        for (int i = 0; i < Global.slotTotalCount; i++)
                            RedisHelperEx.Instance.HashSet(Global.arr_moveID[0], $"S1_{i + 1}", "NA");
                    }
                    Global.m_plcHelper.WriteData_Int16(0, new int[] { ((int)WritePLC.磁浮拍照工位完成信号) }, 1); // Ghi PLC: Hoàn tất trạm chụp = 1

                    string m_data = RedisHelperEx.Instance.HashGet(Global.m_RESPMonitorMover, this.Name);
                    if (m_data.Split(',')[1] == "1")
                    {
                        RedisHelperEx.Instance.HashSet(Global.m_RESPMonitorMover, "Wait1", $"{m_data.Split(',')[0]},{m_data.Split(',')[1]}"); // Cập nhật hàng chờ Wait1 cho trạm Laser
                        RedisHelperEx.Instance.HashSet(Global.m_RESPMonitorMover, this.Name, $"{moverid},0"); // Đánh dấu trạm tiếp nhận hiện đã trống (0)
                    }
                    else
                    {
                        RedisHelperEx.Instance.HashSet(Global.m_RESPMonitorMover, "Wait1", $"{m_data.Split(',')[0]},{m_data.Split(',')[1]}");
                        RedisHelperEx.Instance.HashSet(Global.m_RESPMonitorMover, this.Name, $"{moverid},0");
                    }

                    Logger.Process(this.Name, $"动子号:{moverid} - 通知动子离开：1"); // Log: Mover rời đi
                    return StepEntry; // Hoàn tất chu trình 1 mover, quay lại đón mover mới
#endregion

                default:
                    return null;
            }
        }

        }

        protected override string CalibRun(string strStep)
        {
            switch (strStep)
            {
                case StepEntry:
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
        public static double GetPos(this StationFeedTrans.POINT pt, AXIS axis, StationBase station)
        {
            return station.dictPoint[(int)pt][$"a{(int)axis}"];
        }

        public static double GetPos(this StationFeedTrans.POINT pt, AXIS axis, string strStation)
        {
            StationBase station = StationMgr.GetStation(strStation);
            if (station == null)
                return 0;

            return station.dictPoint[(int)pt][$"a{(int)axis}"];
        }
    }
}
