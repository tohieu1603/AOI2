using AF.Card;
using AF.Common;
using AF.Communicate;
using AF.File;
using AF.Station;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AF.Upload.Hive.HiveTool;
using AF.Data;
using AF.Data.Permission;

namespace AF.Upload.Hive
{
    public class HiveStationInfo
    {
        public string site = "";
        public string building = "";
        public string product = "";
        public string station_type = "";
        public string line = "";
        public int instance = 0;
        public string line_type = "";
        public string station_name = "";
        public string build_type = "";
        public string vendor = "";

        public string stationID
        {
            get
            {
                return $"{site}_{line}_{instance}_{line_type}";
            }
            set
            {
                stationID = value;
            }
        }
    }

    public class HiveUserInfo
    {
        public string Name = "";
        public string Vendor = "";
        public string Level = "";

        public string CardTagid = "";

        public string strSend = "";
        public string strReceive = "";
    }

    public static class HiveMgr
    {
        /// <summary>
        /// 是否启用Hive
        /// </summary>
        public static bool Enable { get; private set; } = false;
        /// <summary>
        /// 是否 生产或空跑 流程
        /// </summary>
        public static bool IsProduceRun = true;

        public delegate bool MachineHandle();
        /// <summary>
        /// 设备启动
        /// </summary>
        public static MachineHandle MachineStart;
        /// <summary>
        /// 设备停止
        /// </summary>
        public static MachineHandle MachineStop;
        /// <summary>
        /// 设备暂停
        /// </summary>
        public static MachineHandle MachinePause;

        public static HiveStationInfo hiveStationInfo = new HiveStationInfo();


        public static void ClickStart()
        {
            HiveCommandProcessor.Instance.ProcessCommand(new List<string> { "Mannual", "start" });
        }

        public static void ClickPause()
        {
            HiveCommandProcessor.Instance.ProcessCommand(new List<string> { "Mannual", "pause" });
        }

        public static void ClickStop()
        {
            HiveCommandProcessor.Instance.ProcessCommand(new List<string> { "Mannual", "stop" });
        }

        public delegate HiveStationInfo GetHiveStationInfoHandler();

        /// <summary>
        /// 获取Hive信息，通过Mes获取 StationID、Product
        /// </summary>
        public static GetHiveStationInfoHandler GetHiveStationInfo;

        public delegate bool GetHiveUserInfoHandler(ref HiveUserInfo hiveUserInfo);

        /// <summary>
        /// 获取Hive信息，通过Mes获取 StationID、Product
        /// </summary>
        public static GetHiveUserInfoHandler GetHiveUserInfo;


        public static string CurrentMachineState { get { return HiveTool.CurrentMachineState; } }
        public static bool Machine5PcsStatus { get { return HiveTool.Machine5PcsStatus; } }
        public static List<ErrorCode> ErrorCodelist = new List<ErrorCode>();

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="tcpHive">Hive网口</param>
        public static void Init(TcpClient tcpHive)
        {
            Enable = true;
            HiveTool.Instance.m_tcpHive = tcpHive;
            _ = HiveTool.Instance;

            //AF.Upload.Hive.GlobalMessageBoxHook.Install();//安装 MessageBox Hook//MessageBox默认显示到Hive弹框上方
            HiveCommandProcessor.m_strIniFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Hive", "HiveLocalData.ini");
            HiveCommandProcessor.m_strMaintenanceIniFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Hive", "保养信息.ini");
            HiveCommandProcessor.m_strVendorStationInfoIniFile = Path.Combine(@"D:\Hive", "VendorStationInfo.ini");

            Process current = Process.GetCurrentProcess();
            Process[] processes = Process.GetProcessesByName(current.ProcessName);
            // 当前是第一个实例
            HiveTool.SendFlag = false;
            HiveTool.HiveIsResponseApple = Ini.GetStringValue(HiveCommandProcessor.m_strIniFile, "LocalData", "HiveIsResponseApple", "True").ToUpper() == "TRUE";
            HiveTool.IsNormalRunMode = Ini.GetStringValue(HiveCommandProcessor.m_strIniFile, "LocalData", "IsNormalRunMode", "True").ToUpper() == "TRUE";
            HiveTool.CurrentMachineState = Ini.GetStringValue(HiveCommandProcessor.m_strIniFile, "LocalData", "MachineState", "2");

            //如果需要用Labview和C#封装的com组件dll库，就用下面的方式，先对dll进行com注册，
            //// 动态创建COM对象
            //Type comType = Type.GetTypeFromProgID("StringToArrayDLL.StringToArrayConverter");
            //dynamic converter = Activator.CreateInstance(comType);
            //// 测试连接
            //string testResult = converter.TestConnection("Hello C#");
            //Console.WriteLine(testResult);
            //Downtime_Cause_List = converter.ProcessLineByLine(new string[] { });

            Downtime_Cause_List = DowntimeCauseDLL.DowntimeCause.GetAllDowntimeCauses();


            BindSystemIO();

            //处理Hive报警弹框、TRdata、Machinestate_5
            EventMgr.NewEvent += OnNewEvent;
            Statistics.Throw2Hive += OnThrow;

            string ErrorCode = Ini.GetStringValue(HiveCommandProcessor.m_strIniFile, "LocalData", "ErrorCode", "");
            CurrentHiveStatus = Convert.ToInt32(HiveTool.CurrentMachineState);//// 用于Form_Main界面状态显示
            _currentHiveStatus = CurrentHiveStatus;
            HiveTool.CurrentErrorCode = ErrorCode;

            // 初始化上传队列管理器（会自动启动后台线程）
            var queueManager = UploadQueueManager.Instance;
            // 注册应用程序退出时的清理
            Application.ApplicationExit += (s, e) =>
            {
                queueManager.Stop();
                // 程序退出时自动卸载 Hook
                //AF.Upload.Hive.GlobalMessageBoxHook.Uninstall();
            };

            GetHeartbeartdata();
            SelectFromErrorCodexlsx();
            ReadMaintenanceItems();
            ReadSparePartList();
            ReadReportEndExcel();

            HiveCommandProcessor.Instance.ProcessCommand(new List<string> { "OnboardingStatus" }, HiveTool.heartbeatdata);//查询注册状态
            HiveCommandProcessor.Instance.ProcessCommand(new List<string> { "StationSwVersion" }, LoadVisionInfo());//上传软件版本
            HiveCommandProcessor.Instance.ProcessCommand(new List<string> { "HeartBeat" }, HiveTool.heartbeatdata);//上传心跳

            HeartbeatUploadTime = DateTime.Now;
            Heartbeart(null, null);
            heartbeatTimer = new System.Windows.Forms.Timer();
            heartbeatTimer.Interval = 1000; // 1S检查一次
            heartbeatTimer.Tick += Heartbeart;
            heartbeatTimer.Start();

            StartHiveOperation();//软件打开显示Hive关闭前的界面
        }




        private static System.Windows.Forms.Timer heartbeatTimer;
        private static DateTime HeartbeatUploadTime = DateTime.Now;
        private static DateTime lastStationCheckDate = DateTime.MinValue;
        private static readonly object lock_log = new object();
        internal static List<string> WeeklyMaintenanceItems = new List<string>();
        internal static List<string> MonthlyMaintenanceItems = new List<string>();
        internal static List<string> SparePartList = new List<string>();
        internal static List<string> ActionTaken = new List<string>();
        internal static List<string> ToolsUsed = new List<string>();
        internal static List<string> SettingsChanged = new List<string>();
        //宕机原因从dll获取
        internal static string[] Downtime_Cause_List;
        /// <summary>
        /// 是否设置窗体所有者关系。当设置为true时，弹出的窗体会随着主窗体最小化和显示；
        /// 当设置为false时，弹出的窗体将独立于主窗体。永远显示在最前
        /// </summary>
        internal static bool SetFormOwner = true;


        /// <summary>
        /// 切换到指定的权限
        /// </summary>
        /// <param name="permission">要切换的权限</param>
        internal static void SwitchPermission(string permission)
        {
            // 根据选择的权限设置相应的模式
            switch (permission)
            {
                case "维护人员 Maintenance": // 维护人员
                    PermissionMgr.Instance.setUser(eRoles.Adjustor);
                    break;
                case "设备厂商 Integrator": // 设备厂商
                    PermissionMgr.Instance.setUser(eRoles.Engineer);
                    break;
                case "管理员 Administrator": // 管理员
                    PermissionMgr.Instance.setUser(eRoles.Engineer);
                    break;
                default:
                    PermissionMgr.Instance.setUser(eRoles.Operator);
                    break;
            }
        }

        /// <summary>
        /// 绑定系统IO
        /// </summary>
        private static void BindSystemIO()
        {
            //急停
            if (SystemIO.diEmg != null)
            {
                SafetyDeviceConfig device = new SafetyDeviceConfig(SafetyDeviceType.EmergencyStop, SystemIO.diEmg.strName, SystemIO.diEmg.strName.txtTrans(TransType.Trans));
                device.Get = () => { return SystemIO.diEmg.Value; };
                Form_SafetyStatusMonitor._staticSafetyDevices.Add(device);
            }
            //安全门
            if (SystemIO.diSafetyDoor != null)
            {
                for (int i = 0; i < SystemIO.diSafetyDoor.Length; i++)
                {
                    for (int j = 0; j < SystemIO.diSafetyDoor[i].Length; j++)
                    {
                        if (SystemIO.diSafetyDoor[i][j] != null)
                        {
                            SafetyDeviceConfig device = new SafetyDeviceConfig(SafetyDeviceType.SafetyDoor, SystemIO.diSafetyDoor[i][j].strName, SystemIO.diSafetyDoor[i][j].strName.txtTrans(TransType.Trans));
                            device.Get = () => { return SystemIO.diSafetyDoor[i][j].Value; };
                            Form_SafetyStatusMonitor._staticSafetyDevices.Add(device);
                        }
                    }
                }
            }
            //安全光栅
            if (SystemIO.lsSafetyCurtain.Count > 0)
            {
                foreach (var item in SystemIO.lsSafetyCurtain)
                {
                    if (item.DI_SafetyCurtain != null)
                    {
                        SafetyDeviceConfig device = new SafetyDeviceConfig(SafetyDeviceType.LightCurtain, item.DI_SafetyCurtain.strName, item.DI_SafetyCurtain.strName.txtTrans(TransType.Trans));
                        device.Get = () => { return item.DI_SafetyCurtain.Value; };
                        Form_SafetyStatusMonitor._staticSafetyDevices.Add(device);
                    }
                }
            }

            //安全门锁
            if (SystemIO.doSafetyDoor != null)
            {
                for (int i = 0; i < SystemIO.doSafetyDoor.Length; i++)
                {
                    for (int j = 0; j < SystemIO.doSafetyDoor[i].Length; j++)
                    {
                        if (SystemIO.doSafetyDoor[i][j] != null)
                        {
                            SafetyDeviceConfig device = new SafetyDeviceConfig(SafetyDeviceType.SafetyDoor, SystemIO.doSafetyDoor[i][j].strName, SystemIO.diSafetyDoor[i][j].strName.txtTrans(TransType.Trans), true);
                            device.Get = () => { return SystemIO.doSafetyDoor[i][j].Value; };
                            device.Set = (b) => { SystemIO.doSafetyDoor[i][j].Value = b; };
                            Form_SafetyStatusMonitor._staticSafetyDevices.Add(device);
                        }
                    }
                }
            }
        }
        private static void Heartbeart(object sender, EventArgs e)
        {

            //上传心跳 5min一次
            if ((DateTime.Now - HeartbeatUploadTime).TotalMinutes >= 5)
            {
                HiveCommandProcessor.Instance.ProcessCommand(new List<string> { "HeartBeat" }, HiveTool.heartbeatdata);
                HeartbeatUploadTime = DateTime.Now;
            }

            if (DateTime.Now.Hour == 8 && DateTime.Now.Minute == 0)
            {

                // 使用标志位确保每分钟只执行一次（实际上就是每天8:00这一分钟执行一次）
                if (DateTime.Now.Date != lastStationCheckDate ||
                    (DateTime.Now.Date == lastStationCheckDate && DateTime.Now.Hour == 8 && DateTime.Now.Minute == 0 && lastStationCheckDate.Minute != 0))
                {

                    ////SFC获取部分心跳数据
                    //AF.Upload.Hive.HiveMgr.SFCData sfcData =new HiveMgr.SFCData();
                    //bool success = HiveMgr.Instance.M_UploadSFC(SFCMode.GetStationID, "", ref sfcData);
                    //string site = "";
                    //string product = "";
                    //string station_type = "";
                    //string line = "";
                    //string instance = "";

                    //if (site != Ini.GetStringValue(HiveCommandProcessor.m_strVendorStationInfoIniFile, "station_info", "site", "") || product != Ini.GetStringValue(HiveCommandProcessor.m_strVendorStationInfoIniFile, "station_info", "product", "") || station_type != Ini.GetStringValue(HiveCommandProcessor.m_strVendorStationInfoIniFile, "station_info", "station_type", "") || line != Ini.GetStringValue(HiveCommandProcessor.m_strVendorStationInfoIniFile, "station_info", "line", "") || instance != Ini.GetStringValue(HiveCommandProcessor.m_strVendorStationInfoIniFile, "station_info", "instance", ""))
                    //{
                    //    MessageBox.Show("StationID不一致，请重新注册Hive");
                    //}

                    //Ini.WriteValue(HiveCommandProcessor.m_strVendorStationInfoIniFile, "station_info", "site", site);
                    //Ini.WriteValue(HiveCommandProcessor.m_strVendorStationInfoIniFile, "station_info", "product", product);
                    //Ini.WriteValue(HiveCommandProcessor.m_strVendorStationInfoIniFile, "station_info", "station_type", station_type);
                    //Ini.WriteValue(HiveCommandProcessor.m_strVendorStationInfoIniFile, "station_info", "line", line);
                    //Ini.WriteValue(HiveCommandProcessor.m_strVendorStationInfoIniFile, "station_info", "instance", instance);

                    lastStationCheckDate = DateTime.Now;
                }
            }
        }

        //每次上传产品调用
        public static void OnSendProductData(MachineDataParameters productData)
        {
            if (HiveTool.Machine5PcsStatus)
                HiveTool.Hive5Pcs++;

            Ini.WriteValue(HiveCommandProcessor.m_strIniFile, "LocalData", "LastOutPutTime", productData.OutputTime.AddSeconds(1).ToString("G"));
            HiveCommandProcessor.Instance.ProcessCommand(new List<string> { "Machinedata" }, productData);
        }

        internal static void GetHeartbeartdata()
        {
            //HiveMgr.heartbeatdata = new HiveMgr.Heartbeat();
            HiveTool.heartbeatdata.machine_sn = Ini.GetStringValue(HiveCommandProcessor.m_strVendorStationInfoIniFile, "machine_info", "machine_sn", "");
            HiveTool.heartbeatdata.machine_model_number = Ini.GetStringValue(HiveCommandProcessor.m_strVendorStationInfoIniFile, "machine_info", "machine_model_number", "");
            hiveStationInfo.vendor = HiveTool.heartbeatdata.vendor = Ini.GetStringValue(HiveCommandProcessor.m_strVendorStationInfoIniFile, "station_info", "vendor", "");
            HiveTool.heartbeatdata.machine_rfid = Ini.GetStringValue(HiveCommandProcessor.m_strVendorStationInfoIniFile, "machine_info", "machine_rfid", "");
            hiveStationInfo.site = HiveTool.heartbeatdata.site = Ini.GetStringValue(HiveCommandProcessor.m_strVendorStationInfoIniFile, "station_info", "site", "");
            hiveStationInfo.building = HiveTool.heartbeatdata.building = Ini.GetStringValue(HiveCommandProcessor.m_strVendorStationInfoIniFile, "station_info", "building", "");
            hiveStationInfo.product = HiveTool.heartbeatdata.product = Ini.GetStringValue(HiveCommandProcessor.m_strVendorStationInfoIniFile, "station_info", "product", "");
            hiveStationInfo.station_type = HiveTool.heartbeatdata.station_type = Ini.GetStringValue(HiveCommandProcessor.m_strVendorStationInfoIniFile, "station_info", "station_type", "");
            hiveStationInfo.line = HiveTool.heartbeatdata.line = Ini.GetStringValue(HiveCommandProcessor.m_strVendorStationInfoIniFile, "station_info", "line", "");
            hiveStationInfo.instance = HiveTool.heartbeatdata.instance = Convert.ToInt32(Ini.GetStringValue(HiveCommandProcessor.m_strVendorStationInfoIniFile, "station_info", "instance", "0"));
            hiveStationInfo.build_type = HiveTool.heartbeatdata.build_type = Ini.GetStringValue(HiveCommandProcessor.m_strVendorStationInfoIniFile, "station_info", "build_type", "0");
            hiveStationInfo.line_type = HiveTool.heartbeatdata.line_type = "default";
            hiveStationInfo.station_name = HiveTool.heartbeatdata.station_name = "default";

        }

        #region 读xlsx
        internal static void SelectFromErrorCodexlsx()
        {
            //ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            string strFileName = Ini.GetStringValue(HiveCommandProcessor.m_strIniFile, "LocalData", "ErrorCodeFileName", "V64 CGS SEC ERROR LIST.xlsx");
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Hive", strFileName);
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];

                var headers = new List<string>();
                for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                {
                    headers.Add(worksheet.Cells[1, col].Text);
                }

                for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                {
                    var errorCode = new ErrorCode
                    {
                        Error_Description_English = worksheet.Cells[row, headers.IndexOf("Error Description") + 1].Text,
                        Code = worksheet.Cells[row, headers.IndexOf("Code") + 1].Text,
                        Category = worksheet.Cells[row, headers.IndexOf("Category") + 1].Text,
                        Error_Description_Chinese = worksheet.Cells[row, headers.IndexOf("Error Description (Chinese)") + 1].Text,
                        Repair_Troubleshooting = worksheet.Cells[row, headers.IndexOf("Repair Actions") + 1].Text,
                        AutoTossingTime = worksheet.Cells[row, headers.IndexOf("AutoTossing Time") + 1].Text,
                    };
                    ErrorCodelist.Add(errorCode);
                }


            }
        }

        // 添加新的公共方法用于获取保养时间
        internal static DateTime? GetMaintenanceTime(string maintenanceItem, bool isWeekly)
        {
            try
            {
                string maintenanceExcelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Hive", "设备保养点检表.xlsx");
                using (var package = new ExcelPackage(new FileInfo(maintenanceExcelPath)))
                {
                    var worksheet = package.Workbook.Worksheets["保养项目"];
                    if (worksheet == null)
                    {
                        worksheet = package.Workbook.Worksheets[0];
                    }

                    // 从第2行开始查找对应的保养项目
                    for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                    {
                        string itemText = worksheet.Cells[row, isWeekly ? 1 : 3].Text?.Trim();

                        if (itemText == maintenanceItem)
                        {
                            string timeStr = worksheet.Cells[row, isWeekly ? 2 : 4].Text?.Trim();
                            if (!string.IsNullOrEmpty(timeStr) && DateTime.TryParse(timeStr, out DateTime savedTime))
                            {
                                return savedTime;
                            }
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"从Excel读取保养时间时出错: {ex.Message}");
            }

            return null;
        }

        // 添加新的公共方法用于更新保养时间
        internal static bool UpdateMaintenanceTime(List<string> maintenanceItems, bool isWeekly)
        {
            try
            {
                string maintenanceExcelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Hive", "设备保养点检表.xlsx");
                using (var package = new ExcelPackage(new FileInfo(maintenanceExcelPath)))
                {
                    var worksheet = package.Workbook.Worksheets["保养项目"];
                    if (worksheet == null)
                    {
                        worksheet = package.Workbook.Worksheets[0];
                    }

                    string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    bool updated = false;

                    // 从第2行开始查找对应的保养项目并更新时间
                    for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                    {
                        string itemText = worksheet.Cells[row, isWeekly ? 1 : 3].Text?.Trim();

                        // 检查当前行是否在保养项目中
                        if (maintenanceItems.Contains(itemText))
                        {
                            // 更新保养时间
                            worksheet.Cells[row, isWeekly ? 2 : 4].Value = currentTime;
                            updated = true;
                        }
                    }

                    if (updated)
                    {
                        package.Save();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新保养时间到Excel失败: {ex.Message}");
            }

            return false;
        }

        // 修改现有的 ReadMaintenanceItems 方法
        internal static void ReadMaintenanceItems()
        {
            string maintenanceExcelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Hive", "设备保养点检表.xlsx");
            using (var package = new ExcelPackage(new FileInfo(maintenanceExcelPath)))
            {
                // 获取"保养项目"工作表
                var worksheet = package.Workbook.Worksheets["保养项目"];
                if (worksheet == null)
                {
                    // 如果找不到指定名称的工作表，使用第一个工作表
                    worksheet = package.Workbook.Worksheets[0];
                }

                // 清空全局变量（防止重复添加）
                WeeklyMaintenanceItems.Clear();
                MonthlyMaintenanceItems.Clear();

                // 从第2行开始读取（跳过表头）
                for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                {
                    // 读取周保养项目（A列）和周保养时间（B列）
                    string weeklyItem = worksheet.Cells[row, 1].Text?.Trim();
                    string weeklyTime = worksheet.Cells[row, 2].Text?.Trim();

                    // 读取月保养项目（C列）和月保养时间（D列）
                    string monthlyItem = worksheet.Cells[row, 3].Text?.Trim();
                    string monthlyTime = worksheet.Cells[row, 4].Text?.Trim();

                    // 添加周保养项目（如果项目不为空）
                    if (!string.IsNullOrEmpty(weeklyItem) && weeklyItem != "NA")
                    {
                        WeeklyMaintenanceItems.Add(weeklyItem);
                    }

                    // 添加月保养项目（如果项目不为空）
                    if (!string.IsNullOrEmpty(monthlyItem) && monthlyItem != "NA")
                    {
                        MonthlyMaintenanceItems.Add(monthlyItem);
                    }
                }
            }
        }


        internal static void ReadSparePartList()
        {
            // 文件路径 - 根据你的实际情况调整
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Hive", "spare_part_list.xlsx");

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                // 获取第一个工作表
                var worksheet = package.Workbook.Worksheets[0];

                // 清空全局变量（防止重复添加）
                SparePartList.Clear();

                // 从第1行开始读取（无表头）
                for (int row = 1; row <= worksheet.Dimension.End.Row; row++)
                {
                    // 读取三列数据
                    string partNumber = worksheet.Cells[row, 1].Text?.Trim();
                    string englishName = worksheet.Cells[row, 2].Text?.Trim();
                    string chineseName = worksheet.Cells[row, 3].Text?.Trim();

                    // 组合成指定格式并添加到列表
                    if (!string.IsNullOrEmpty(partNumber) && !string.IsNullOrEmpty(englishName) && !string.IsNullOrEmpty(chineseName))
                    {
                        string formattedItem = $"{partNumber};{englishName};{chineseName}";
                        SparePartList.Add(formattedItem);
                    }
                }
            }
        }


        internal static void ReadReportEndExcel()
        {
            // 文件路径
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Hive", "report_end.xlsx");

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                // 获取第一个工作表
                var worksheet = package.Workbook.Worksheets[0];

                // 清空全局变量（防止重复添加）
                ActionTaken.Clear();
                ToolsUsed.Clear();
                SettingsChanged.Clear();

                // 从第2行开始读取（跳过表头）
                for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                {
                    // 读取三列数据
                    string checkStep = worksheet.Cells[row, 1].Text?.Trim();
                    string usedTool = worksheet.Cells[row, 2].Text?.Trim();
                    string adjustmentParam = worksheet.Cells[row, 3].Text?.Trim();

                    // 添加到对应的全局变量中
                    if (!string.IsNullOrEmpty(checkStep))
                    {
                        ActionTaken.Add(checkStep);
                    }
                    if (!string.IsNullOrEmpty(usedTool))
                    {
                        ToolsUsed.Add(usedTool);
                    }
                    if (!string.IsNullOrEmpty(adjustmentParam))
                    {
                        SettingsChanged.Add(adjustmentParam);
                    }
                }
            }
        }

        internal static bool CheckAndAddToColumn(string columnName, string value)
        {
            // 根据列名称获取对应的全局变量
            List<string> targetList = null;
            int columnIndex = 0;

            switch (columnName)
            {
                case "检查步骤":
                    targetList = ActionTaken;
                    columnIndex = 1;
                    break;
                case "用到工具":
                    targetList = ToolsUsed;
                    columnIndex = 2;
                    break;
                case "调整参数":
                    targetList = SettingsChanged;
                    columnIndex = 3;
                    break;
                default:
                    return false;
            }

            // 检查值是否已存在
            if (targetList.Contains(value))
            {
                return true;
            }

            // 文件路径
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Hive", "report_end.xlsx");

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                // 获取第一个工作表
                var worksheet = package.Workbook.Worksheets[0];

                // 找到指定列的最后一行（从第2行开始找，跳过表头）
                int lastRow = 2; // 从第2行开始（表头在第1行）
                while (lastRow <= worksheet.Dimension.End.Row &&
                       !string.IsNullOrEmpty(worksheet.Cells[lastRow, columnIndex].Text?.Trim()))
                {
                    lastRow++;
                }

                // 将值添加到全局变量
                targetList.Add(value);

                // 将值追加到Excel的指定列
                worksheet.Cells[lastRow, columnIndex].Value = value;

                // 保存文件
                package.Save();
            }

            return true;
        }
        #endregion


        public class VersionInfo
        {
            public string version { get; set; }
            public string hashKey { get; set; }
            public string old { get; set; } /*= string.Empty;*/
            public string NEW { get; set; } /*= string.Empty;*/
        }

        /// <summary>
        /// 从INI文件中获取所有节点的完整信息（包括version和hash_key）
        /// </summary>
        /// <returns>字典，key为节点名称，value为模块信息对象</returns>
        internal static Dictionary<string, VersionInfo> LoadAllVersionInfo()
        {
            var moduleDict = new Dictionary<string, VersionInfo>();
            try
            {
                string[] sectionNames = Ini.GetAllSectionNames(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Hive", "ms_version.ini"));
                if (sectionNames == null || sectionNames.Length == 0)
                    return moduleDict;

                foreach (string sectionName in sectionNames)
                {
                    try
                    {
                        string strVersion = Ini.GetStringValue(
                            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Hive", "ms_version.ini"), sectionName, "version", "");

                        string hashcode = null;
                        string filePath = null;
                        byte[] fileBytes = null;
                        int sum = 0;
                        switch (sectionName)
                        {
                            case "main_software":
                                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AutoFrame.exe");
                                fileBytes = System.IO.File.ReadAllBytes(filePath);

                                foreach (byte b in fileBytes)
                                {
                                    sum += b;
                                }
                                hashcode = Convert.ToString(sum);
                                var moduleInfo = new VersionInfo
                                {
                                    version = strVersion,

                                    hashKey = HashHelper.GetStringSHA1(hashcode)
                                };
                                moduleDict[sectionName] = moduleInfo;
                                break;
                            case "repair_sop":
                                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Hive", "repair_sop.xlsx");
                                fileBytes = System.IO.File.ReadAllBytes(filePath);
                                foreach (byte b in fileBytes)
                                {
                                    sum += b;
                                }
                                hashcode = Convert.ToString(sum);
                                moduleInfo = new VersionInfo
                                {
                                    version = strVersion,

                                    hashKey = HashHelper.GetStringSHA1(hashcode)
                                };
                                moduleDict[sectionName] = moduleInfo;
                                break;
                            case "spare_part_list":
                                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Hive", "spare_part_list.xlsx");
                                fileBytes = System.IO.File.ReadAllBytes(filePath);
                                foreach (byte b in fileBytes)
                                {
                                    sum += b;
                                }
                                hashcode = Convert.ToString(sum);
                                moduleInfo = new VersionInfo
                                {
                                    version = strVersion,

                                    hashKey = HashHelper.GetStringSHA1(hashcode)
                                };
                                moduleDict[sectionName] = moduleInfo;
                                break;
                            case "x_offset":
                                moduleInfo = new VersionInfo
                                {
                                    old = "1.200000",

                                    NEW = "1.300000"
                                };
                                moduleDict[sectionName] = moduleInfo;

                                break;
                            case "z_offset":
                                moduleInfo = new VersionInfo
                                {
                                    old = "1.200000",

                                    NEW = "1.300000"
                                };
                                moduleDict[sectionName] = moduleInfo;


                                break;
                            default:
                                break;
                        }


                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"读取节点 {sectionName} 时发生错误: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载模块信息时发生错误: {ex.Message}");
            }

            return moduleDict;
        }
        /// <summary>
        /// 使用配置映射的方式加载模块信息并转换为参数列表
        /// </summary>
        /// <returns>参数列表</returns>
        internal static StationSwVersionParameters LoadVisionInfo()
        {
            var parameters = new StationSwVersionParameters();
            var moduleDict = LoadAllVersionInfo();

            // 使用字典映射来设置属性，避免重复代码
            var moduleMappings = new Dictionary<string, Action<VersionInfo>>
            {
                ["main_software"] = info => parameters.MainSoftware = info,
                ["repair_sop"] = info => parameters.RepairSop = info,
                ["spare_part_list"] = info => parameters.SparePartList = info,
                ["vision_software"] = info => parameters.VisionSoftware = info,
                ["laser_software"] = info => parameters.LaserSoftware = info,
                ["robot_software"] = info => parameters.RobotSoftware = info,
                ["ide_software"] = info => parameters.IdeSoftware = info,
                ["config_file"] = info => parameters.ConfigFile = info,
                ["vision_template"] = info => parameters.VisionTemplate = info,
                ["x_offset"] = info => parameters.XOffset = info,
                ["z_offset"] = info => parameters.ZOffset = info
            };

            foreach (var mapping in moduleMappings)
            {
                if (moduleDict.TryGetValue(mapping.Key, out var moduleInfo))
                {
                    mapping.Value(moduleInfo);
                }
                else
                {
                    // 设置空版本信息
                    mapping.Value(new VersionInfo());
                }
            }

            return parameters;
        }

        ////根据中文描述，查询Error信息
        //public ErrorCode ErrorMsg(string strKey)
        //{
        //    ErrorCode Msg = new ErrorCode();
        //    var sss = ErrorCodelist.Where(n => n.Code == strKey).ToList();
        //    if (sss.Count != 0)
        //    {
        //        Msg = (ErrorCode)sss[0];
        //    }
        //    else
        //    {
        //        Msg = new ErrorCode();
        //        Msg.Code = "TBD";
        //        Msg.Category = "Actual Downtime";
        //        Msg.Error_Description_English = "Undefined error";
        //        Msg.Error_Description_Chinese = "未定义的异常";
        //    }
        //    return Msg;
        //}

        //根据中文描述，查询Error信息
        public static ErrorCode ErrorMsg(string strKey)
        {
            ErrorCode Msg = new ErrorCode();
            var sss1 = ErrorCodelist.Where(n => n.Error_Description_Chinese == strKey).ToList();
            var sss2 = ErrorCodelist.Where(n => n.Error_Description_English == strKey).ToList();
            var sss3 = ErrorCodelist.Where(n => n.Code == strKey).ToList();

            if (sss1.Count != 0)
            {
                Msg = (ErrorCode)sss1[0];
            }
            else if (sss2.Count != 0)
            {
                Msg = (ErrorCode)sss2[0];
            }
            else if (sss3.Count != 0)
            {
                Msg = (ErrorCode)sss3[0];
            }
            else
            {
                Msg = new ErrorCode();
                Msg.Code = "TBD";
                Msg.Category = "Actual Downtime";
                Msg.Error_Description_English = "Undefined error";
                Msg.Error_Description_Chinese = "未定义的异常";
            }
            return Msg;
        }
        internal static void StartHiveOperation()
        {
            //读取INI
            string status = Ini.GetStringValue(HiveCommandProcessor.m_strIniFile, "LocalData", "Status", "");
            string Errormsg = Ini.GetStringValue(HiveCommandProcessor.m_strIniFile, "LocalData", "ErrorMsg", "");
            string ErrorCode = Ini.GetStringValue(HiveCommandProcessor.m_strIniFile, "LocalData", "ErrorCode", "");
            string creatclass = Ini.GetStringValue(HiveCommandProcessor.m_strIniFile, "LocalData", "LastPOP", "Form_warning");
            if (HiveTool.CurrentMachineState == "5")
            {
                if (status == "help request")
                {
                    if (ErrorCode == "F99OOOO-20")
                    {
                        HiveCommandProcessor.Instance.ProcessCommand(new List<string> { "Mannual", "stop", "0" });
                        //Task.Run(() =>
                        //{
                        //    Form_Operation frm = new Form_Operation();
                        //    frm.StartPosition = FormStartPosition.CenterScreen;
                        //    frm.Label12.Visible = false;
                        //    frm.button1.Visible = false;
                        //    frm.label1_retainTime.Visible = false;
                        //    frm.TopLevel = true;
                        //    frm.Show();
                        //});

                    }
                    else
                    {
                        HiveCommandProcessor.Instance.ProcessCommand(new List<string> { "Machinestate", "5", Errormsg, "0" });
                        Task.Run(() =>
                        {
                            //Form_Warning frm = new Form_Warning(Errormsg,true);
                            //frm.Label12.Visible = false;
                            //frm.Button1.Visible = false;
                            //frm.label1_retainTime.Visible = false;
                            //frm.StartPosition = FormStartPosition.CenterScreen;
                            //frm.TopLevel = true;
                            //frm.Show();
                        });

                    }
                }
                else if (status == "repair start")
                {
                    HiveCommandProcessor.Instance.ProcessCommand(new List<string> { "Mannual", "start" });
                    //Task.Run(() =>
                    //{
                    //    string classtemp = creatclass == "Form_warning" ? "HiveCommandProcessor" : "Form_Operation";
                    //    Form_Resume frm1 = new Form_Resume(classtemp);
                    //    frm1.StartPosition = FormStartPosition.CenterScreen;
                    //    frm1.TopLevel = true;
                    //    frm1.Show();
                    //});

                }
            }
            else
            {
                HiveCommandProcessor.Instance.ProcessCommand(new List<string> { "Machinestate", "5", "PC/SW crashed", "1" });
                //var stateParams2 = new MachineStateParameters
                //{
                //    MachineState = MachineStateData.MachineState.Idle,
                //};
                //HiveCommandProcessor.Instance.ProcessCommand(new List<string> { "Machinestate" }, stateParams2);
            }
        }


        /// <summary>
        /// 报警跳变
        /// </summary>
        private static void OnNewEvent(object sender, AlarmEventArgs e)
        {
            try
            {
                if (e == null)
                    return;

                if (e.eventRecord == null || e.eventRecord.Length < 1)
                    return;

                switch (e.eventArgsType)
                {
                    case EventArgsType.AddAlarm:
                        if (HiveMgr.CurrentMachineState != Convert.ToInt32(MachineStateData.MachineState.UnplannedDown).ToString() /*&& IsWarningToHive*/)
                        {
                            ErrorCode Errormsg = ErrorMsg(e.eventRecord[0].GetCustomCode());
                            if (Errormsg.Category.ToLower() == "actual downtime" || Errormsg.Category.ToLower() == "waiting for op"/* && Errormsg.Code != "TBD"*/)
                            {
                                HiveCommandProcessor.Instance.ProcessCommand(new List<string> { "Machinestate", "5", $"{Errormsg.Code}" });
                            }
                            else
                            {
                                ErrorMessage LastError = new ErrorMessage()
                                {
                                    code = Errormsg.Code,
                                    category = Errormsg.Category,
                                    strMsgChinese = Errormsg.Error_Description_Chinese,
                                    strMsgEnglish = Errormsg.Error_Description_English,
                                    tm_begin = DateTime.Now,
                                };
                                HiveTool.LastError.Add(LastError);
                                // HiveCommandProcessor.Instance.ProcessCommand(new List<string> { "Machinestate", "5", $"{Errormsg.Code}" });
                            }
                        }

                        break;
                    case EventArgsType.RemoveAlarm:
                        foreach (var item in e.eventRecord)
                        {
                            ErrorCode Errormsg = ErrorMsg(e.eventRecord[0].GetCustomCode());
                            ErrorMessage LastError = new ErrorMessage();
                            foreach (var err in HiveTool.LastError)
                            {
                                if (err.category == "retry" && err.code == Errormsg.Code)
                                {
                                    LastError = err;
                                    DateTime tm_end = DateTime.Now;
                                    if ((tm_end - err.tm_begin).TotalSeconds <= 0)
                                    {
                                        tm_end = err.tm_begin.AddSeconds(0.1);
                                    }

                                    // 创建TR数据参数
                                    var trParams = new TRDataParameters
                                    {
                                        Message = err.strMsgEnglish,
                                        Code = err.code,
                                        Severity = err.category,
                                        OccurrenceTime = err.tm_begin,
                                        ResolvedTime = tm_end,
                                        Parallel = 3,
                                    };
                                    // 处理TR数据
                                    HiveCommandProcessor.Instance.ProcessCommand(new List<string> { "TRdata" }, trParams);
                                }
                            }
                            HiveTool.LastError.Remove(LastError);
                        }
                        break;
                    case EventArgsType.ClearAllAlarm:
                        foreach (var item in HiveTool.LastError)
                        {
                            if (item.category == "retry")
                            {
                                DateTime tm_end = DateTime.Now;
                                if ((tm_end - item.tm_begin).TotalSeconds <= 0)
                                {
                                    tm_end = item.tm_begin.AddSeconds(0.1);
                                }

                                // 创建TR数据参数
                                var trParams = new TRDataParameters
                                {
                                    Message = item.strMsgEnglish,
                                    Code = item.code,
                                    Severity = item.category,
                                    OccurrenceTime = item.tm_begin,
                                    ResolvedTime = tm_end,
                                    Parallel = 3,
                                };
                                // 处理TR数据
                                HiveCommandProcessor.Instance.ProcessCommand(new List<string> { "TRdata" }, trParams);
                            }
                            HiveTool.LastError.Clear();
                        }
                        break;
                    case EventArgsType.ChangeState:
                        break;
                    default:
                        break;
                }
            }
            catch
            {

            }

        }

        // 抛料事件处理方法
        private static void OnThrow(Statistics.ThrowHiveInfo throwInfo)
        {
            ErrorCode Errormsg;
            string strMsg = throwInfo.ThrowReason.Split(new char[] { ';', '；', ',', '，' })[0];
            Errormsg = ErrorMsg(strMsg);

            if (HiveTool.CurrentMachineState != Convert.ToInt32(MachineStateData.MachineState.UnplannedDown).ToString() /*&& IsWarningToHive*/)
            {
                if (Errormsg.Category.ToLower() == "retry")
                {
                    DateTime tm_end = (DateTime)throwInfo.EndTime;
                    if ((tm_end - throwInfo.StartTime).TotalSeconds <= 0)
                    {
                        tm_end = tm_end.AddSeconds(0.1);
                    }

                    // 创建TR数据参数
                    var trParams = new TRDataParameters
                    {
                        Message = Errormsg.Error_Description_English,
                        Code = Errormsg.Code,
                        Severity = throwInfo.Retry ? "retry" : "tossing",
                        OccurrenceTime = throwInfo.StartTime,
                        ResolvedTime = tm_end,
                        Parallel = throwInfo.Fixture,
                        keypart_sn = throwInfo.KeyPartSN,
                        smallpart_sn = throwInfo.SmallPartSN,
                        module_sn = throwInfo.ModulePartSN,
                    };
                    // 处理TR数据
                    HiveCommandProcessor.Instance.ProcessCommand(new List<string> { "TRdata" }, trParams);
                }
            }
        }


        internal static StationState stationState { get { return ProgramTool.stationState; } }

        /// <summary>
        /// 处理恢复逻辑（提取公共代码）
        /// </summary>
        public static void HandleResumeLogic()
        {
            if (ProgramTool.stationState != StationState.Pause)
                return;

            if (HiveTool.Instance.IsFormOpenInNamespace("AF.Upload.Hive", "Form_Resume"))
            {
                // 在这个界面只能刷卡启动和点击生产5pcs启动，不响应IO启动
                // 保留原有注释代码
                //if (StationMgr.GetInstance().IsPause())
                //{
                //    if (StationMgr.GetInstance().ResumeAllStation())
                //    {
                //
                //    }
                //}
            }
            else if (HiveTool.Instance.IsFormOpenInNamespace("AF.Upload.Hive", "Form_Operation"))
            {
                // 获取Form_Operation实例并触发恢复事件
                var formOperation = Application.OpenForms.OfType<Form_Operation>().FirstOrDefault();
                if (formOperation != null)
                {
                    InvokeIfRequiredBegin(formOperation, () => formOperation.TriggerResume());
                }
            }
            else if (HiveTool.Instance.IsFormOpenInNamespace("AF.Upload.Hive", "Form_Warning"))
            {
                // 获取Form_Warning实例并触发恢复事件
                var formWarning = Application.OpenForms.OfType<Form_Warning>().FirstOrDefault();
                if (formWarning != null)
                {
                    InvokeIfRequiredBegin(formWarning, () => formWarning.TriggerResume());
                }
            }
            else
            {
                HiveCommandProcessor.Instance.ProcessCommand(new List<string> { "Mannual", "start" });
            }
        }

        /// <summary>
        /// 处理暂停逻辑（提取公共代码）
        /// </summary>
        public static void HandlePauseLogic()
        {
            if (HiveTool.Instance.IsFormOpenInNamespace("AF.Upload.Hive", "Form_Resume"))
            {
                // 获取Form_Resume实例并触发暂停事件
                var formResume = Application.OpenForms.OfType<Form_Resume>().FirstOrDefault();
                if (formResume != null)
                {
                    InvokeIfRequiredBegin(formResume, () => formResume.TriggerPause());
                }
            }
            else
            {
                HiveCommandProcessor.Instance.ProcessCommand(new List<string> { "Mannual", "pause" });
            }
        }
        // 或者使用BeginInvoke的版本（推荐，更高效）
        private static void InvokeIfRequiredBegin(Control control, Action action)
        {
            if (control.InvokeRequired)
            {
                control.BeginInvoke(action);
            }
            else
            {
                action();
            }
        }

        internal static bool IsRuning()
        {
            return SystemIO.CarrierCount > 0;
        }

        /// <summary>
        /// 保存上传日志
        /// </summary>
        public static void SaveLog(string message, bool result, string returnResult, DataStyle dataStyle)
        {
            HiveTool.Instance.ShowLog(message, result, returnResult, dataStyle);
        }
    }

    public class ErrorCode
    {
        public string Error_Description_English { get; set; }

        public string Code { get; set; }

        public string Category { get; set; }

        public string Error_Description_Chinese { get; set; }

        public string Repair_Troubleshooting { get; set; }

        //附加功能
        public string AutoTossingTime { get; set; }
    }
    internal delegate void closeForm();
}
