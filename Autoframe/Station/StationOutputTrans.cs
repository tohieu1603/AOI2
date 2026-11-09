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
    /// StationOutputTrans
    /// </summary>
    public class StationOutputTrans : StationEx
    {
        string moverid;//动子ID号

        string[] S1stadata = new string[Global.slotTotalCount];//S1站结果
        string[] S2stadata = new string[Global.slotTotalCount];//S2站结果
        string[] S3stadata = new string[Global.slotTotalCount];//S3站结果
        string[] S4stadata = new string[Global.slotTotalCount];//S4站结果
        string[] S5stadata = new string[Global.slotTotalCount];//S5站结果    
        string[] upAOIdata = new string[Global.slotTotalCount];//上AOI检测结果
        string[] downAOIdata = new string[Global.slotTotalCount];//下AOI检测结果


        string[] S3_NGReason = new string[Global.slotTotalCount];
        string[] S4_NGReason = new string[Global.slotTotalCount];
        string[] S5_NGReason = new string[Global.slotTotalCount];

        string[] S4_SN = new string[Global.slotTotalCount];

        string[] upAOI_NGReason = new string[Global.slotTotalCount];
        string[] downOI_NGReason = new string[Global.slotTotalCount];

        string[] arr_SN = new string[Global.slotTotalCount];

        int S3AOINum = Global.slotTotalCount  / 2;//下AOI1检测穴位数量
        int S5AOINum = (Global.slotTotalCount + 1) / 2;//下AOI2检测穴位数量


        /// <summary>
        /// 构造函数，需要设置站位当前的IO输入，IO输出，轴方向及轴名称，以显示在手动页面方便操作
        /// </summary>
        /// <param name="strName"></param>
        public StationOutputTrans(string strName) : base(strName)
        {
            HardwareConfig();
            CheckPoint<POINT>();
            LoadContainer<CONTAINER, ProductData>();
        }

        void HardwareConfig()
        {
            //配置站位界面显示输入
            arrayDI = new object[] { DI.出料移载站升降伸出位, DI.出料移载站升降缩回位, DI.出料移载站解锁伸出位, DI.出料移载站解锁缩回位 };
            //配置站位界面显示输出
            arrayDO = new object[] { DO.出料移载站升降伸出,DO.出料移载站升降缩回,DO.出料移载站解锁伸出,DO.出料移载站解锁缩回 };
            //配置站位界面显示气缸
            arrayCylinder = new object[] {Cylinder.出料移载站升降, Cylinder.出料移载站解锁  };
            //配置站位轴
            arrayAxis = new object[] { };
        }

        ProductData productData = new ProductData();

        TrayData trayData = new TrayData();

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
            RegBit.出料接驳站初始化完成.Set(false); // Bit: Reset cờ hoàn tất khởi tạo trạm tiếp nhận dỡ liệu
            RegBit.出料接驳站通知可取料.Set(false); // Bit: Reset cờ thông báo cho trạm trung chuyển có thể gắp liệu
            RegBit.中转取放站取料完成.Set(false); // Bit: Reset cờ trạm trung chuyển đã gắp liệu hoàn tất
        }

        protected override string InitRun(string strStep)
        {
            switch (strStep)
            {
                case StepEntry: // [Khởi tạo] Bắt đầu chu trình khởi tạo trạm tiếp nhận dỡ liệu (StationOutputTrans)
                    
                    return "等待中转取放料站初始化完成"; // Chuyển sang: Chờ trạm gắp đặt trung chuyển khởi tạo hoàn tất

                case "等待中转取放料站初始化完成": // [Khởi tạo] Chờ cờ khởi tạo trạm gắp đặt trung chuyển (StationTransferPick)

                    if (RegBit.中转取放站初始化完成.Get()) // Bit: Trạm trung chuyển đã khởi tạo xong
                    {
                        return "出料解锁松开"; // Chuyển sang: Co xi lanh mở khóa dỡ liệu

                    }
                    else
                    {
                        return "等待中转取放料站初始化完成"; // Tiếp tục chờ trạm trung chuyển khởi tạo
                    }

                case "出料解锁松开": // [Khởi tạo] Co xi lanh mở khóa bàn dỡ liệu
                    CylinderBack(Cylinder.出料移载站解锁); // Co xi lanh mở khóa bàn tiếp nhận dỡ liệu

                    return "出料升降缩回"; // Chuyển sang: Co xi lanh nâng hạ

                case "出料升降缩回": // [Khởi tạo] Co xi lanh nâng hạ bàn dỡ liệu về vị trí thấp ban đầu
                    CylinderBack(Cylinder.出料移载站升降); // Co xi lanh nâng hạ trạm tiếp nhận dỡ liệu
                    return "初始化完成"; // Chuyển sang: Hoàn tất khởi tạo

                case "初始化完成": // [Khởi tạo] Bật cờ hoàn tất khởi tạo trạm dỡ liệu
                    //this[NestEnum.出料接驳站].Product = null;
                    //WaitAllStaReady();
                    RegBit.出料接驳站初始化完成.Set(true); // Bit: Báo hoàn tất khởi tạo trạm tiếp nhận dỡ liệu
                    //结束初始化
                    return StepExit; // Thoát chu trình khởi tạo
                default:
                    return null;
            }
        }

        protected override string ProduceRun(string strStep)
        {
            switch (strStep)
            {
                case StepEntry: // [Sản xuất] Bắt đầu chu trình sản xuất trạm tiếp nhận dỡ liệu
                    
                    return "等待动子到位"; // Chuyển sang: Chờ Mover từ tính đến vị trí

                case "等待动子到位": // [Sản xuất] Đọc tín hiệu PLC kiểm tra Mover đã vào vị trí trạm dỡ liệu chưa
                    if (Global.m_plcHelper.ReadData_Int16(0, new int[] { ((int)ReadPLC.下料工位到位信号) }) == 1) // PLC: Mover đã đến trạm dỡ hàng
                    {
                        return "获取动子号"; // Chuyển sang: Đọc mã số Mover

                    }
                    else
                    {
                        return "等待动子到位"; // Tiếp tục chờ Mover đến
                    }
                case "获取动子号": // [Sản xuất] Đọc ID của Mover từ PLC, lưu bộ nhớ và cập nhật giám sát Redis

                    moverid = Global.m_plcHelper.ReadData_Int16(0, new int[] { ((int)ReadPLC.下料工位动子号) }).ToString(); // Đọc ID Mover
                    Global.arr_moveID[5] = moverid; // Gán ID Mover vào vị trí trạm 5 trong mảng toàn cục
                    Logger.Process(this.Name, $"动子已到位-动子号:{moverid}"); // Ghi log: Mover đã vào vị trí

                    string m_data = RedisHelperEx.Instance.HashGet(Global.m_RESPMonitorMover, "Wait5"); // Lấy thông tin trạm chờ Wait5
                    if (m_data.Split(',')[0] == moverid)
                    {
                        RedisHelperEx.Instance.HashSet(Global.m_RESPMonitorMover, "Wait5", $"Null"); // Xóa trạng thái Wait5
                        RedisHelperEx.Instance.HashSet(Global.m_RESPMonitorMover, this.Name, $"{moverid},{m_data.Split(',')[1]}"); // Cập nhật Mover tại trạm hiện tại
                    }

                    return "检查数据库当前动子是否存在"; // Chuyển sang: Kiểm tra dữ liệu Mover trong Redis

                case "检查数据库当前动子是否存在": // [Sản xuất] Kiểm tra key Mover có tồn tại trong Redis không
                    if (RedisHelperEx.Instance.Keycheck("0", moverid.ToString())) // Key Mover tồn tại
                    {
                        return "前站数据读取"; // Chuyển sang: Đọc kết quả các trạm trước
                    }
                    else // Không tìm thấy Mover trong Redis (Mover rỗng/lỗi)
                    {
                        return "通知动子可离开"; // Chuyển sang: Báo Mover rời trạm
                    }
                case "前站数据读取": // [Sản xuất] Đọc toàn bộ dữ liệu kiểm tra từ các trạm trước trên Redis (S1, S3, S4, S5, lý do NG, SN)
                    for (int i = 0; i < Global.slotTotalCount; i++)
                    {
                        S1stadata[i] = RedisHelperEx.Instance.HashGet(moverid.ToString(), $"S1_{i + 1}"); // Kết quả trạm S1 (cảm biến/camera nạp liệu)
                        //S2stadata[i] = RedisHelperEx.Instance.HashGet(moverid.ToString(), $"S2_{i + 1}"); // Kết quả trạm S2 (Laser đo độ cao)
                        S3stadata[i] = RedisHelperEx.Instance.HashGet(moverid.ToString(), $"S3_{i + 1}"); // Kết quả trạm S3 (AOI dưới 2)
                        S4stadata[i] = RedisHelperEx.Instance.HashGet(moverid.ToString(), $"S4_{i + 1}"); // Kết quả trạm S4 (AOI trên)
                        S5stadata[i] = RedisHelperEx.Instance.HashGet(moverid.ToString(), $"S5_{i + 1}"); // Kết quả trạm S5 (AOI dưới 1)
                        //productData.cavitydata[i] = RedisHelperEx.Instance.HashGet(moverid.ToString(), $"S5_{i + 1}");  

                        S3_NGReason[i] = RedisHelperEx.Instance.HashGet($"{moverid}_Data", $"S3_{i + 1}_NGReason"); // Lý do NG trạm S3
                        S4_NGReason[i] = RedisHelperEx.Instance.HashGet($"{moverid}_Data", $"S4_{i + 1}_NGReason"); // Lý do NG trạm S4
                        S5_NGReason[i] = RedisHelperEx.Instance.HashGet($"{moverid}_Data", $"S5_{i + 1}_NGReason"); // Lý do NG trạm S5

                        S4_SN[i] = RedisHelperEx.Instance.HashGet($"{moverid}_Data", $"S4_{i + 1}_SN"); // Mã SN đọc từ camera trạm S4
                    }

                    return "前站检测结果整理"; // Chuyển sang: Sắp xếp kết quả kiểm tra


                case "前站检测结果整理": // [Sản xuất] Ghép kết quả hai camera AOI mặt dưới (S5 cho nửa đầu, S3 cho nửa sau)
                    for (int i = 0; i < Global.slotTotalCount; i++)
                    {
                        if (Global.slotTotalCount % 2 == 0) // Tổng số ô là số chẵn
                        {
                            if (i < S3AOINum)
                            {
                                Console.WriteLine($"S5_{i}");
                                downAOIdata[i] = S5stadata[i]; // Lấy kết quả camera AOI dưới 1 (S5)
                                downOI_NGReason[i] = S5_NGReason[i]; // Lấy lý do NG AOI dưới 1
                            }
                            else
                            {
                                downAOIdata[i] = S3stadata[i]; // Lấy kết quả camera AOI dưới 2 (S3)
                                downOI_NGReason[i] = S3_NGReason[i]; // Lấy lý do NG AOI dưới 2
                            }
                        }
                        else // Tổng số ô là số lẻ
                        {
                            if (i <= S3AOINum)
                            {
                                Console.WriteLine($"S5_{i}");
                                downAOIdata[i] = S5stadata[i]; // Lấy kết quả camera AOI dưới 1 (S5)
                                downOI_NGReason[i] = S5_NGReason[i]; // Lấy lý do NG AOI dưới 1
                            }
                            else
                            {
                                downAOIdata[i] = S3stadata[i]; // Lấy kết quả camera AOI dưới 2 (S3)
                                downOI_NGReason[i] = S3_NGReason[i]; // Lấy lý do NG AOI dưới 2
                            }
                        }
                       
                       
                    }
                    upAOIdata = S4stadata; // Lấy kết quả toàn bộ ô từ camera AOI mặt trên (S4)
                    upAOI_NGReason = S4_NGReason; // Lý do NG AOI mặt trên

                    //S4stadata = null;
                    //S3stadata = null;

                    return "AR面与IR面检测结果汇总"; // Chuyển sang: Tổng hợp kết quả mặt AR & IR

                case "AR面与IR面检测结果汇总": // [Sản xuất] Tổng hợp kết quả chất lượng tổng thể: S1 có liệu + Down AOI OK + Up AOI OK => OK
                    for (int i = 0; i < Global.slotTotalCount; i++)
                    {
                        bool S1result = S1stadata[i] == "OK"; // Có sản phẩm ở trạm nạp S1
                        bool downAOIresult = downAOIdata[i] == "OK"; // AOI mặt dưới đạt
                        bool upAOIresult = upAOIdata[i] == "OK"; // AOI mặt trên đạt

                        if (S1result) // Có sản phẩm ở ô này
                        {
                            if (downAOIresult && upAOIresult) // Cả 2 mặt trên và dưới đều OK
                            {
                                //productData.cavitydata[i] = "OK";
                                trayData.AllSlotsData[i].Result = SlotState.OK; // Đánh dấu ô sản phẩm OK
                            }
                            else
                            {
                                //productData.cavitydata[i] = "NG";
                                trayData.AllSlotsData[i].Result = SlotState.NG; // Đánh dấu ô sản phẩm NG
                            }
                           
                        }
                        else // Không có sản phẩm ở ô này (khoang trống)
                        {
                            //productData.cavitydata[i] = "NA";
                            trayData.AllSlotsData[i].Result = SlotState.NA; // Đánh dấu ô rỗng NA
                        }

                    }

                    return "出料升降伸出"; // Chuyển sang: Duỗi xi lanh nâng bàn dỡ

                case "出料升降伸出": // [Sản xuất] Duỗi xi lanh nâng bàn dỡ liệu đón Mover (hiện bypass)

                   // CylinderOut(Cylinder.出料移载站升降);

                    return "出料解锁伸出"; // Chuyển sang: Duỗi xi lanh mở khóa bàn dỡ

                case "出料解锁伸出": // [Sản xuất] Duỗi xi lanh mở khóa định vị Mover (hiện bypass)

                   // CylinderOut(Cylinder.出料移载站解锁);

                    return "更新本站数据"; // Chuyển sang: Ghi kết quả trạm S6 lên Redis

                case "更新本站数据": // [Sản xuất] Ghi kết quả tổng hợp trạm S6, chuỗi lý do lỗi ghép nối và SN vào Redis
                    for (int i = 0; i < Global.slotTotalCount; i++)
                    {
                        RedisHelperEx.Instance.HashSet(moverid, $"S6_{i + 1}", trayData.AllSlotsData[i].Result.ToString()); // Ghi kết quả S6 của ô i+1

                        string strNG_Reason = string.Join("_", downOI_NGReason[i], upAOI_NGReason[i]); // Ghép lý do NG mặt trên và mặt dưới
                        RedisHelperEx.Instance.HashSet($"{moverid}_Data", $"{i + 1}_AOINGReason", strNG_Reason); // Ghi lý do NG

                        //string strS3SN = RedisHelperEx.Instance.HashGet($"{moverid}_Data", $"S4_{i+1}_SN");
                        RedisHelperEx.Instance.HashSet($"{moverid}_Data", $"{i + 1}_SN", S4_SN[i]); // Ghi mã SN tương ứng
                    }

                    //this[NestEnum.出料接驳站].Product = productData;

                    Global.moveidnum = moverid; // Lưu ID Mover toàn cục
                    RegString.Str_OutputTransMoveID.Set(moverid); // Gán chuỗi ID Mover vào thanh ghi

                    return "通知中转取放站可取料"; // Chuyển sang: Báo trạm trung chuyển có thể gắp hàng


                case "通知中转取放站可取料": // [Sản xuất] Bật cờ cho phép trạm gắp đặt trung chuyển (StationTransferPick) tiến hành gắp hàng

                    RegBit.出料接驳站通知可取料.Set(true); // Bit: Cho phép trạm trung chuyển gắp phôi

                    return "等待中转取放站取料完成"; // Chuyển sang: Chờ gắp phôi xong


                case "等待中转取放站取料完成": // [Sản xuất] Chờ cờ báo trạm trung chuyển đã gắp xong toàn bộ phôi khỏi Mover

                    if (RegBit.中转取放站取料完成.Get()) // Bit: Trạm trung chuyển đã gắp xong
                    {
                        RedisHelperEx.Instance.HashSet(Global.m_RESPMonitorMover, this.Name, $"{moverid},0"); // Giám sát Redis: Mover đã rỗng
                        RegBit.中转取放站取料完成.Set(false); // Reset cờ gắp xong
                        return "出料解锁缩回"; // Chuyển sang: Co xi lanh mở khóa
                    }
                    else
                    {
                        return "等待中转取放站取料完成"; // Tiếp tục chờ trạm trung chuyển gắp xong
                    }
                case "出料解锁缩回": // [Sản xuất] Co xi lanh mở khóa bàn dỡ liệu

                    CylinderBack(Cylinder.出料移载站解锁); // Co xi lanh mở khóa

                    return "出料升降缩回"; // Chuyển sang: Co xi lanh nâng hạ

                case "出料升降缩回": // [Sản xuất] Co xi lanh nâng hạ bàn dỡ hạ xuống giải phóng Mover
                    CylinderBack(Cylinder.出料移载站升降); // Co xi lanh nâng hạ
                    return "动子到位信号复位"; // Chuyển sang: Reset tín hiệu Mover到位

                case "动子到位信号复位": // [Sản xuất] Ghi PLC reset tín hiệu到位信号 về 0
                    Global.m_plcHelper.WriteData_Int16(0, new int[] { ((int)WritePLC.下料工位到位信号) }, 0); // PLC: Reset cờ到位信号 = 0
                    return "通知动子可离开"; // Chuyển sang: Báo Mover rời đi

                case "通知动子可离开": // [Sản xuất] Ghi PLC phát tín hiệu hoàn thành cho phép Mover rời trạm dỡ
                    Global.m_plcHelper.WriteData_Int16(0, new int[] { ((int)WritePLC.下料工位到位信号) }, 0); // PLC: Đảm bảo到位信号 = 0
                    Global.m_plcHelper.WriteData_Int16(0, new int[] { ((int)WritePLC.下料工位完成信号) }, 1); // PLC: Hoàn thành dỡ liệu = 1 (Mover di chuyển tiếp)
                    Logger.Process(this.Name, $"动子号:{moverid} - 通知动子离开：1"); // Ghi log: Cho phép Mover rời đi

                    return "生产流程结束"; // Chuyển sang: Kết thúc chu trình sản xuất
                
                case "生产流程结束": // [Sản xuất] Kết thúc chu trình sản xuất, quay về StepEntry
                    return StepEntry; // Về bước khởi đầu chu trình
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
        public static double GetPos(this StationOutputTrans.POINT pt, AXIS axis, StationBase station)
        {
            return station.dictPoint[(int)pt][$"a{(int)axis}"];
        }

        public static double GetPos(this StationOutputTrans.POINT pt, AXIS axis, string strStation)
        {
            StationBase station = StationMgr.GetStation(strStation);
            if (station == null)
                return 0;

            return station.dictPoint[(int)pt][$"a{(int)axis}"];
        }
    }
}
