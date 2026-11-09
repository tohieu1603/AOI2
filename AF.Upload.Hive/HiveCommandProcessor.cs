using AF.Common;
using AF.File;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AF.Upload.Hive.HiveTool;
using static AF.Upload.Hive.MachineStateData;

namespace AF.Upload.Hive
{
    /// <summary>
    /// 弹窗请求类
    /// </summary>
    public class PopupRequest
    {
        public string RequestType { get; set; }
        public List<string> Parameters { get; set; }
        public DateTime RequestTime { get; set; }
        public int Priority { get; set; }

        public PopupRequest(string requestType, List<string> parameters, int priority = 0)
        {
            RequestType = requestType;
            Parameters = parameters;
            RequestTime = DateTime.Now;
            Priority = priority;
        }
    }

    /// <summary>
    /// 弹窗管理器 - 单例模式
    /// </summary>
    public class PopupManager
    {
        #region 单例模式
        private static readonly Lazy<PopupManager> _instance = new Lazy<PopupManager>(() => new PopupManager());
        public static PopupManager Instance => _instance.Value;
        #endregion

        private readonly object _lock = new object();
        private readonly List<PopupRequest> _requestQueue = new List<PopupRequest>();
        private bool _isPopupShowing = false;
        private Form _currentPopup = null;
        private SynchronizationContext _uiContext;
        private Type _mainFormType;

        private PopupManager()
        {
            // 保存UI线程上下文
            _uiContext = SynchronizationContext.Current;

            // 使用反射获取主窗体类型,防止软件打开时有进度条会导致闪退
            _mainFormType = Type.GetType("AutoFrame.Form_Main, AutoFrame");
        }
        /// <summary>
        /// 获取主窗体实例
        /// </summary>
        public Form GetMainForm()
        {
            try
            {
                if (_mainFormType != null)
                {
                    // 通过反射类型查找主窗体
                    return Application.OpenForms.OfType<Form>()
                        .FirstOrDefault(form => form.GetType() == _mainFormType);
                }
                else
                {
                    // 备用方案：使用第一个窗体作为主窗体
                    return Application.OpenForms.Count > 0 ? Application.OpenForms[0] : null;
                }
            }
            catch (Exception ex)
            {
                SaveServeLog("PopupManager", $"Error getting main form: {ex.Message}");
                return Application.OpenForms.Count > 0 ? Application.OpenForms[0] : null;
            }
        }
        /// <summary>
        /// 请求显示弹窗
        /// </summary>
        /// <param name="requestType">请求类型</param>
        /// <param name="parameters">参数列表</param>
        /// <param name="priority">优先级</param>
        public void RequestPopup(string requestType, List<string> parameters, int priority = 0)
        {
            var request = new PopupRequest(requestType, parameters, priority);

            lock (_lock)
            {
                if (_isPopupShowing)
                {
                    // 如果已经有弹窗显示，将请求加入队列并按优先级排序
                    _requestQueue.Add(request);
                    _requestQueue.Sort((x, y) => y.Priority.CompareTo(x.Priority));
                    SaveServeLog("PopupManager", $"Popup queued: {requestType}, Priority: {priority}, Queue length: {_requestQueue.Count}");
                }
                else
                {
                    // 直接显示弹窗
                    _isPopupShowing = true;
                    ShowPopupInternal(request);
                }
            }
        }

        /// <summary>
        /// 弹窗关闭回调
        /// </summary>
        private void OnPopupClosed()
        {
            lock (_lock)
            {
                _isPopupShowing = false;
                _currentPopup = null;

                // 检查队列中是否有待处理的请求
                if (_requestQueue.Count > 0)
                {
                    var nextRequest = _requestQueue[0];
                    _requestQueue.RemoveAt(0);
                    _isPopupShowing = true;
                    // 延迟一小段时间再显示下一个弹窗，避免UI冲突
                    Task.Delay(100).ContinueWith(_ =>
                    {
                        ShowPopupInternal(nextRequest);
                    });
                }
            }
        }

        /// <summary>
        /// 内部显示弹窗方法
        /// </summary>
        private void ShowPopupInternal(PopupRequest request)
        {
            try
            {
                // 在UI线程上创建和显示弹窗
                if (_uiContext != null)
                {
                    _uiContext.Post(new SendOrPostCallback(state =>
                    {
                        CreateAndShowPopup(request);
                    }), null);
                }
                else
                {
                    // 如果没有UI上下文，在主线程上执行
                    var mainForm = GetMainForm();
                    if (mainForm != null && !mainForm.IsDisposed)
                    {
                        mainForm.Invoke(new Action(() =>
                        {
                            CreateAndShowPopup(request);
                        }));
                    }
                    else
                    {
                        // 最后的手段：在新线程中创建UI
                        Task.Run(() =>
                        {
                            Application.Run(new Form()); // 创建UI线程上下文
                            _uiContext = SynchronizationContext.Current;
                            CreateAndShowPopup(request);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                SaveServeLog("PopupManager", $"Error showing popup: {ex.Message}");
                OnPopupClosed();
            }
        }
        /// <summary>
        /// 创建并显示弹窗（在UI线程上执行）
        /// </summary>
        private void CreateAndShowPopup(PopupRequest request)
        {
            try
            {
                Form popupForm = CreatePopupForm(request);

                if (popupForm != null)
                {
                    _currentPopup = popupForm;

                    // 获取主窗体并设置为Owner
                    var mainForm = GetMainForm();
                    if (mainForm != null && !mainForm.IsDisposed)
                    {
                        if (HiveMgr.SetFormOwner)
                            popupForm.Owner = mainForm;
                    }

                    // 设置窗体属性
                    popupForm.StartPosition = FormStartPosition.CenterScreen;
                    popupForm.TopLevel = true;
                    popupForm.FormClosed += (s, e) =>
                    {
                        OnPopupClosed();
                    };

                    // 显示弹窗
                    popupForm.Show();

                    // 激活窗口
                    popupForm.Activate();
                    popupForm.Focus();

                    SaveServeLog("PopupManager", $"Popup shown: {request.RequestType}");
                }
                else
                {
                    OnPopupClosed();
                }
            }
            catch (Exception ex)
            {
                SaveServeLog("PopupManager", $"Error in CreateAndShowPopup: {ex.Message}");
                OnPopupClosed();
            }
        }
        /// <summary>
        /// 根据请求创建对应的弹窗窗体
        /// </summary>
        private Form CreatePopupForm(PopupRequest request)
        {
            Form popupForm = null;

            switch (request.RequestType)
            {
                case "Machinestate":
                    if (request.Parameters.Count > 2 && request.Parameters[1] == "5")
                    {
                        if (request.Parameters.Count >= 4 && request.Parameters[3] == "0")
                        {
                            popupForm = new Form_Warning(request.Parameters[2], true);//软件重开后显示
                        }
                        else
                        {
                            popupForm = new Form_Warning(request.Parameters[2], false);//正常运行显示
                        }
                    }
                    break;

                case "Mannual":
                    switch (request.Parameters[1])
                    {
                        case "start":
                            popupForm = new Form_Resume(typeof(HiveCommandProcessor).Name);

                            break;
                        case "pause":
                        case "stop":

                            if (request.Parameters.Count >= 3 && request.Parameters[2] == "0")
                            {
                                popupForm = new Form_Operation(true, request.Parameters[1]);//软件重开后显示
                            }
                            else
                            {
                                popupForm = new Form_Operation(false, request.Parameters[1]);//正常运行显示
                            }

                            break;
                        case "Heartbeat":
                            popupForm = new Form_HiveOnboard();
                            break;
                        case "Onboarding":
                            popupForm = new Form_HiveOnboard();
                            break;
                        case "NewStationOnboard":
                            popupForm = new Form_HiveOnboard();
                            break;
                    }
                    break;
            }

            return popupForm;
        }

        /// <summary>
        /// 获取队列状态
        /// </summary>
        public (int QueueLength, bool IsPopupShowing) GetStatus()
        {
            lock (_lock)
            {
                return (_requestQueue.Count, _isPopupShowing);
            }
        }

        /// <summary>
        /// 清空队列
        /// </summary>
        public void ClearQueue()
        {
            lock (_lock)
            {
                _requestQueue.Clear();
            }
        }

        private bool SaveServeLog(string obj, string Message)
        {
            string path = @"D:\Hive\Hive Log\Other\PopupManager_log";
            Message = $"[{DateTime.Now:HH:mm:ss}] {Message}\r\n";

            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string strFileSavePath = Path.Combine(path, DateTime.Now.ToString("yyyyMMdd") + ".txt");
                System.IO.File.AppendAllText(strFileSavePath, Message);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Hive命令处理器 - 负责处理所有Hive相关命令和弹窗管理
    /// </summary>
    public class HiveCommandProcessor
    {
        public static string m_strIniFile;

        public static string m_strMaintenanceIniFile;

        public static string m_strVendorStationInfoIniFile;
        /// <summary>
        /// 是否允许操作主程序按钮(Start, Pause, Stop)
        /// </summary>
        public static bool AllowMainProgramButtons { get; set; } = true;

        /// <summary>
        /// 是否有弹窗正在显示
        /// </summary>
        public static bool IsPopupShowing => PopupManager.Instance.GetStatus().IsPopupShowing;




        #region 单例
        private static readonly Lazy<HiveCommandProcessor> lazyInstance = new Lazy<HiveCommandProcessor>(() => new HiveCommandProcessor());
        private static readonly object lock_log = new object();

        public static HiveCommandProcessor Instance => lazyInstance.Value;
        #endregion

        // 私有构造函数
        private HiveCommandProcessor() { }


        /// <summary>
        /// 处理机器状态命令
        /// </summary>
        private void ProcessMachineStateCommand(List<string> parameters)
        {
            if (parameters[1] == "5" && CurrentMachineState != "5")
            {
                // 使用弹窗管理器请求弹窗
                if (parameters.Count == 4 && parameters[3] == "1")
                {
                    PopupManager.Instance.RequestPopup("Machinestate", parameters, 1);
                }
                else
                {
                    switch (ProgramTool.stationState)
                    {
                        case StationState.Init:
                        case StationState.Ready:
                        case StationState.Auto:
                            PopupManager.Instance.RequestPopup("Machinestate", parameters, 1);
                            break;
                    }
                }
                return;
            }
            if (parameters.Count == 4)
            {
                if (parameters[3] == "0")
                {
                    // 使用弹窗管理器请求弹窗
                    PopupManager.Instance.RequestPopup("Machinestate", parameters, 1);
                }
            }

        }
        /// <summary>
        /// 处理机器状态命令
        /// </summary>
        private void ProcessMachineStateCommand(MachineStateParameters parameters)
        {
            MachineState(parameters);
        }

        /// <summary>
        /// 处理手动操作命令
        /// </summary>
        private void ProcessManualCommand(List<string> parameters)
        {
            switch (parameters[1])
            {
                case "start":
                    if (HiveTool.CurrentMachineState == "5" || HiveTool.CurrentMachineState == "2")
                    {
                        if (!HiveTool.Instance.IsFormOpenInNamespace(this.GetType().Namespace, "Form_Resume") &&
                            !HiveTool.Instance.IsAnyFormOpenInNamespace(this.GetType().Namespace))
                        {
                            switch (ProgramTool.stationState)
                            {
                                case StationState.Manual:
                                case StationState.Pause:
                                    PopupManager.Instance.RequestPopup("Mannual", parameters, 1);
                                    break;
                            }
                        } 
                    }
                    break;
                case "pause":
                case "stop":
                    if (HiveTool.CurrentMachineState == "1" || HiveTool.CurrentMachineState == "2" || parameters.Count == 3)
                    {
                        if (!HiveTool.Instance.IsAnyFormOpenInNamespace(this.GetType().Namespace))
                        {
                            switch (ProgramTool.stationState)
                            {
                                case StationState.Init:
                                case StationState.Ready:
                                case StationState.Auto:
                                    PopupManager.Instance.RequestPopup("Mannual", parameters, 1);
                                    break;
                            }
                        }   
                    }
                    break;
                case "Heartbeat":
                    PopupManager.Instance.RequestPopup("Mannual", parameters, 0);
                    break;
                case "Onboarding":
                    PopupManager.Instance.RequestPopup("Mannual", parameters, 0);
                    break;
                case "NewStationOnboard":
                    PopupManager.Instance.RequestPopup("Mannual", parameters, 0);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 处理TR数据命令（新版本-基于对象参数）
        /// </summary>
        private void ProcessTRDataCommand(TRDataParameters parameters)
        {
            TRData(parameters);
        }
        /// <summary>
        /// 处理机器数据命令
        /// </summary>
        private void ProcessMachineDataCommand(MachineDataParameters parameters)
        {
            MachineData(parameters);
        }

        /// <summary>
        /// 处理站点身份数据命令
        /// </summary>
        private void ProcessStationSwVersionCommand(StationSwVersionParameters parameters)
        {
            StationSwVersion(parameters);
        }
        /// <summary>
        /// 处理心跳命令
        /// </summary>
        private void ProcessHeartbeatCommand(Heartbeat heartbeat)
        {
            Heartbeat(heartbeat);
        }
        /// <summary>
        /// 处理心跳命令
        /// </summary>
        private void ProcessOnboardingStatusCommand(Heartbeat heartbeat)
        {
            OnboardingStatus(heartbeat);
        }

        /// <summary>
        /// 处理通信设置命令
        /// </summary>
        private void ProcessCommunicationCommand(List<string> parameters)
        {
            if (parameters[1] == "1")
            {
                HiveTool.HiveIsResponseApple = true;
                Ini.WriteValue(HiveCommandProcessor.m_strIniFile, "LocalData", "HiveIsResponseApple", "True");
            }
            else
            {
                HiveTool.HiveIsResponseApple = false;
                Ini.WriteValue(HiveCommandProcessor.m_strIniFile, "LocalData", "HiveIsResponseApple", "False");
            }
        }

        /// <summary>
        /// 处理运行模式命令
        /// </summary>
        private void ProcessRunModeCommand(List<string> parameters)
        {

            if (parameters[1] == "1")
            {
                HiveTool.IsNormalRunMode = true;
                Ini.WriteValue(HiveCommandProcessor.m_strIniFile, "LocalData", "IsNormalRunMode", "True");
            }
            else
            {
                HiveTool.IsNormalRunMode = false;
                Ini.WriteValue(HiveCommandProcessor.m_strIniFile, "LocalData", "IsNormalRunMode", "False");
            }
        }

        /// <summary>
        /// 处理PCS命令
        /// </summary>
        private void ProcessPCSCommand(List<string> parameters)
        {
            // 保留PCS处理逻辑
            // 可以根据需要添加具体实现
        }

        /// <summary>
        /// 处理站点命令
        /// </summary>
        private void ProcessStationCommand(List<string> parameters)
        {
            // 保留Station处理逻辑
            // 可以根据需要添加具体实现
        }

        /// <summary>
        /// 处理未知命令
        /// </summary>
        private void ProcessUnknownCommand(List<string> parameters)
        {
            string commandHeader = parameters.Count > 0 ? parameters[0] : "Unknown";
            SaveServeLog("Hive", $"Unknown command received: {commandHeader}");
        }

        /// <summary>
        /// 机器状态数据处理
        /// </summary>
        public void MachineState(MachineStateParameters parameters)
        {
            // 构建数据字典
            var data = new Dictionary<string, object>();

            // 添加可选数据字段
            if (!string.IsNullOrWhiteSpace(parameters.error_message))
                data["error_message"] = parameters.error_message;

            if (!string.IsNullOrWhiteSpace(parameters.code))
                data["code"] = parameters.code;

            if (!string.IsNullOrWhiteSpace(parameters.downtime_cause))
                data["downtime_cause"] = parameters.downtime_cause;

            if (!string.IsNullOrWhiteSpace(parameters.action_taken))
                data["action_taken"] = parameters.action_taken;

            if (!string.IsNullOrWhiteSpace(parameters.spare_parts))
                data["spare_parts"] = parameters.spare_parts;

            if (!string.IsNullOrWhiteSpace(parameters.rating))
                data["rating"] = parameters.rating;

            if (!string.IsNullOrWhiteSpace(parameters.status))
                data["status"] = parameters.status;

            if (!string.IsNullOrWhiteSpace(parameters.badge))
                data["badge"] = parameters.badge;

            if (!string.IsNullOrWhiteSpace(parameters.event_id))
                data["event_id"] = parameters.event_id;

            // 合并额外数据
            foreach (var item in parameters.AdditionalData)
            {
                if (!data.ContainsKey(item.Key))
                    data.Add(item.Key, item.Value);
            }

            CurrentMachineState = ((int)parameters.MachineState).ToString();

            CurrentHiveStatus = Convert.ToInt32(CurrentMachineState);
            _currentHiveStatus = CurrentHiveStatus;
            if (!string.IsNullOrWhiteSpace(parameters.code))
                CurrentErrorCode = parameters.code;

            HiveTool.MachineStateUpload(parameters, data);
        }
        /// <summary>
        /// machine data上传案例
        /// </summary>
        internal void MachineData(MachineDataParameters parameters)
        {
            // 确保输出时间在输入时间之后
            if (parameters.OutputTime <= parameters.InputTime)
            {
                parameters.InputTime = parameters.OutputTime.AddSeconds(-1);
            }
            DateTime input = parameters.InputTime;
            DateTime output = parameters.OutputTime;

            var data = new Dictionary<string, object>();

            if (!string.IsNullOrWhiteSpace(parameters.CarrierSn))
                data["carrier_sn"] = parameters.CarrierSn;

            if (parameters.Quantity.HasValue)
                data["qty"] = parameters.Quantity.Value;

            if (parameters.Parallel.HasValue)
                data["parallel"] = parameters.Parallel.Value;

            if (!parameters.Pass && !string.IsNullOrWhiteSpace(parameters.ErrorCode))
                data["errorcode"] = parameters.ErrorCode;

            // 合并额外数据
            foreach (var item in parameters.AdditionalData)
            {
                if (!data.ContainsKey(item.Key))
                    data.Add(item.Key, item.Value);
            }
            data["mode"] = parameters.Mode;
            HiveTool.MachineDataUpload(parameters, data);

        }
        /// <summary>
        /// TR数据上传案例
        /// </summary>
        internal void TRData(TRDataParameters parameters)
        {
            // 确保解决时间在发生时间之后
            if (parameters.ResolvedTime <= parameters.OccurrenceTime)
            {
                parameters.OccurrenceTime = parameters.ResolvedTime.AddSeconds(-1);
            }

            DateTime occurrence = parameters.OccurrenceTime;
            DateTime resolved = parameters.ResolvedTime;

            var data = new Dictionary<string, object>();

            if (parameters.Parallel.HasValue)
                data["parallel"] = parameters.Parallel.Value;
            if (string.IsNullOrWhiteSpace(parameters.keypart_sn))
            {
                data["keypart_sn"] = parameters.keypart_sn;
            }
            else
            {
                data["keypart_sn"] = "“”";
            }
            if (!string.IsNullOrWhiteSpace(parameters.Series))
                data["series"] = parameters.Series;

            // 合并额外数据
            foreach (var item in parameters.AdditionalData)
            {
                if (!data.ContainsKey(item.Key))
                    data.Add(item.Key, item.Value);
            }

            HiveTool.TRDataUpload(parameters, data);
        }
        // <summary>
        /// 站点身份数据处理
        /// </summary>
        public void StationSwVersion(StationSwVersionParameters parameters)
        {
            HiveTool.StationSwVersionUpload(parameters);
        }
        /// <summary>
        /// 心跳数据处理
        /// </summary>
        public void Heartbeat(Heartbeat heartbeat)
        {
            HiveTool.HeartbeatUpload(heartbeat);
        }
        /// <summary>
        /// 心跳数据处理
        /// </summary>
        public void OnboardingStatus(Heartbeat heartbeat)
        {
            HiveTool.OnboardingStatusUpload(heartbeat);
        }


        /// <summary>
        /// 设置是否允许操作主程序按钮
        /// </summary>
        /// <param name="allow">是否允许</param>
        public void SetMainProgramButtonsEnabled(bool allow)
        {
            AllowMainProgramButtons = allow;
            SaveServeLog("Hive", $"Main program buttons {(allow ? "enabled" : "disabled")}");


        }

        /// <summary>
        /// 获取弹窗队列状态
        /// </summary>
        public (int QueueLength, bool IsPopupShowing) GetPopupStatus()
        {
            return PopupManager.Instance.GetStatus();
        }

        /// <summary>
        /// 清空弹窗队列
        /// </summary>
        public void ClearPopupQueue()
        {
            PopupManager.Instance.ClearQueue();
        }
        /// <summary>
        /// 格式化命令日志信息
        /// </summary>
        private string FormatCommandLog(List<string> commandParams, object obj)
        {
            var sb = new StringBuilder();

            // 命令头部和参数
            sb.Append($"Command: {commandParams[0]}");
            if (commandParams.Count > 1)
            {
                sb.Append($", Parameters: [{string.Join(" | ", commandParams.Skip(1))}]");
            }

            // 对象信息
            if (obj != null)
            {
                sb.Append($", Object Type: {obj.GetType().Name}");

                // 根据不同类型进行格式化
                switch (obj)
                {
                    case MachineStateParameters state:
                        sb.Append($", MachineState: {state.MachineState}, StateChangeTime: {state.StateChangeTime.ToString("G")}");
                        break;
                    case MachineDataParameters data:
                        sb.Append($", UnitSN: {data.UnitSn}, Pass: {data.Pass}, Mode: {data.Mode}");
                        break;
                    case TRDataParameters tr:
                        sb.Append($", Code: {tr.Code}, Message: {tr.Message?.Substring(0, Math.Min(tr.Message.Length, 50))}...");
                        break;
                    case StationSwVersionParameters sw:
                        sb.Append(", Station Software Version Data");
                        break;
                    case Heartbeat hb:
                        sb.Append($", Heartbeat Data");
                        break;
                    default:
                        sb.Append($", ToString: {obj.ToString()}");
                        break;
                }
            }
            else
            {
                sb.Append(", Object: null");
            }

            SaveServeLog("Hive", sb.ToString());

            return sb.ToString();
        }

        public bool SaveServeLog(string obj, string Message)
        {
            string path = "";
            Message = $"[{DateTime.Now:HH:mm:ss}] {Message}\r\n";

            if (obj == "Hive")
                path = @"D:\Hive\Hive Log\Other\Hive_Internal_log";
            else
                path = @"D:\Hive\Hive Log\Other\MainProgram_Internal_log";

            lock (lock_log)
            {
                try
                {
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    string strFileSavePath = Path.Combine(path, DateTime.Now.ToString("yyyyMMdd") + ".txt");
                    System.IO.File.AppendAllText(strFileSavePath, Message);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }



        #region 数据处理

        /// <summary>
        /// 处理命令 - 修改为使用队列管理上传任务
        /// </summary>
        public void ProcessCommand(List<string> commandParams, object obj = null)
        {
            if (commandParams == null || commandParams.Count == 0)
            {
                SaveServeLog("Hive", "Empty command received");
                return;
            }

            FormatCommandLog(commandParams, obj);
            string commandHeader = commandParams[0];

            if (HiveMgr.IsProduceRun)  //20251207添加用于快速启动
            {
                // 对于需要上传的命令，使用队列管理
                if (IsUploadCommand(commandHeader, obj))
                {
                    string jsonData = GenerateJsonData(commandParams, obj);
                    string upload = jsonData.Replace("NEW", "new");
                    if (!string.IsNullOrEmpty(jsonData))
                    {
                        DataStyle dataStyle = GetDataStyleFromCommand(commandHeader);
                        UploadQueueManager.Instance.EnqueueUpload(dataStyle, upload);
                        return; // 命令已加入队列，直接返回
                    }
                }
                // 其他命令保持原有逻辑
                ProcessCommandInternal(commandParams, obj);
            }
            else   //20251207添加用于快速启动
            {
                if (commandHeader == "Mannual")
                {
                    switch (commandParams[1])
                    {
                        case "start":
                            HiveMgr.MachineStart?.Invoke();
                            break;
                        case "pause":
                            HiveMgr.MachinePause?.Invoke();
                            break;
                        case "stop":
                            HiveMgr.MachineStop?.Invoke();
                            break;
                        default:

                            break;
                    }

                }
            }
        }

        /// <summary>
        /// 判断是否为需要上传的命令
        /// </summary>
        private bool IsUploadCommand(string commandHeader, object obj)
        {
            switch (commandHeader)
            {
                case "TRdata":
                    return true;
                case "Machinedata":
                    return true;
                case "StationSwVersion":
                    return true;
                case "HeartBeat":
                    return true;
                case "OnboardingStatus":
                    return true;
                case "Machinestate":
                    // 只有当obj不为空时才加入队列
                    return obj != null;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 原有的命令处理逻辑（非上传命令）
        /// </summary>
        private void ProcessCommandInternal(List<string> commandParams, object obj)
        {
            string commandHeader = commandParams[0];

            switch (commandHeader)
            {
                case "Machinestate":
                    ProcessMachineStateCommand(commandParams);
                    break;
                case "Mannual":
                    ProcessManualCommand(commandParams);
                    break;

                case "Communication":
                    ProcessCommunicationCommand(commandParams);
                    break;
                default:
                    ProcessUnknownCommand(commandParams);
                    break;
            }
        }

        /// <summary>
        /// 根据命令生成JSON数据
        /// </summary>
        private string GenerateJsonData(List<string> commandParams, object obj)
        {
            try
            {
                string commandHeader = commandParams[0];

                switch (commandHeader)
                {
                    case "TRdata":
                        if (obj is TRDataParameters trDataParams)
                        {
                            return GenerateTRDataJson(trDataParams);
                        }
                        break;

                    case "Machinedata":
                        if (obj is MachineDataParameters machineDataParams)
                        {
                            return GenerateMachineDataJson(machineDataParams);
                        }
                        break;

                    case "StationSwVersion":
                        if (obj is StationSwVersionParameters swVersionParams)
                        {
                            return GenerateStationSwVersionJson(swVersionParams);
                        }
                        break;

                    case "HeartBeat":
                        if (obj is Heartbeat heartbeat)
                        {
                            return GenerateHeartbeatJson(heartbeat);
                        }
                        break;

                    case "OnboardingStatus":
                        if (obj is Heartbeat heartbeat1)
                        {
                            return GenerateOnboardingStatusJson(heartbeat1);
                        }
                        break;

                    case "Machinestate":
                        if (obj is MachineStateParameters stateParams)
                        {
                            return GenerateMachineStateJson(stateParams);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                SaveServeLog("Hive", $"生成JSON数据失败: {ex.Message}");
            }

            return null;
        }
        /// <summary>
        /// 生成TR数据的JSON
        /// </summary>
        private string GenerateTRDataJson(TRDataParameters parameters)
        {
            // 重用TRData方法中的逻辑
            var data = new Dictionary<string, object>();

            if (parameters.Parallel.HasValue)
                data["parallel"] = parameters.Parallel.Value;

            if (parameters.keypart_sn.Length==10)//CG_SN存在默认值，判断长度10来确认是否获取过CG_SN，
            {
                data["keypart_sn"] = parameters.keypart_sn;
            }
            else
            {
                data["keypart_sn"] = "“”";
            }

            if (parameters.module_sn.Length == 10)//WIP_SN存在默认值，判断长度10来确认是否获取过CG_SN，
            {
                data["module_sn"] = parameters.module_sn;
            }
            else
            {
                data["module_sn"] = "“”";
            }

            if (!string.IsNullOrWhiteSpace(parameters.Series))
                data["series"] = parameters.Series;

            // 合并额外数据
            foreach (var item in parameters.AdditionalData)
            {
                if (!data.ContainsKey(item.Key))
                    data.Add(item.Key, item.Value);
            }

            var trData = new TRData
            {
                message = parameters.Message,
                code = parameters.Code,
                severity = parameters.Severity,
                occurrence_time = parameters.OccurrenceTime.ToString(HiveTool.TimeFormat),
                resolved_time = parameters.ResolvedTime.ToString(HiveTool.TimeFormat),
                station_info = HiveTool.heartbeatdata,
                data = data
            };

            return HiveTool.SerializeToJson(trData,
                formatting: Formatting.Indented,
                nullValueHandling: NullValueHandling.Ignore);
        }

        /// <summary>
        /// 生成机器数据的JSON
        /// </summary>
        private string GenerateMachineDataJson(MachineDataParameters parameters)
        {
            // 重用MachineData方法中的逻辑
            // 确保输出时间在输入时间之后
            if (parameters.OutputTime <= parameters.InputTime)
            {
                parameters.InputTime = parameters.OutputTime.AddSeconds(-1);
            }

            var data = new Dictionary<string, object>();

            if (!string.IsNullOrWhiteSpace(parameters.CarrierSn))
                data["carrier_sn"] = parameters.CarrierSn;

            if (parameters.Quantity.HasValue)
                data["qty"] = parameters.Quantity.Value;

            if (parameters.Parallel.HasValue)
                data["parallel"] = parameters.Parallel.Value;

            if (!parameters.Pass && !string.IsNullOrWhiteSpace(parameters.ErrorCode))
                data["errorcode"] = parameters.ErrorCode;

            // 合并额外数据
            foreach (var item in parameters.AdditionalData)
            {
                if (!data.ContainsKey(item.Key))
                    data.Add(item.Key, item.Value);
            }

            data["mode"] = parameters.Mode;

            var machineData = new MachineData
            {
                unit_sn = parameters.UnitSn.TrimToNull() ?? Guid.NewGuid().ToString(),
                pass = parameters.Pass,
                input_time = parameters.InputTime.ToString(HiveTool.TimeFormat),
                output_time = parameters.OutputTime.ToString(HiveTool.TimeFormat),
                station_info = HiveTool.heartbeatdata,
                serials = parameters.Serials ?? new Dictionary<string, object>(),
                data = data
            };

            return HiveTool.SerializeToJson(machineData,
                formatting: Formatting.Indented,
                nullValueHandling: NullValueHandling.Ignore);
        }

        /// <summary>
        /// 生成站点软件版本JSON
        /// </summary>
        private string GenerateStationSwVersionJson(StationSwVersionParameters parameters)
        {
            var stationSWVersion = new StationSwVersion
            {
                main_software = new SW_Version
                {
                    version = parameters.MainSoftware.version ?? "",
                    hash_key = parameters.MainSoftware.hashKey ?? ""
                },
                sub_module = new SubModule
                {
                    repair_sop = new SW_Version
                    {
                        version = parameters.RepairSop.version ?? "",
                        hash_key = parameters.RepairSop.hashKey ?? ""
                    },
                    spare_part_list = new SW_Version
                    {
                        version = parameters.SparePartList.version ?? "",
                        hash_key = parameters.SparePartList.hashKey ?? ""
                    },
                    vision_software = new SW_Version
                    {
                        version = parameters.VisionSoftware.version ?? "",
                        hash_key = parameters.VisionSoftware.hashKey ?? ""
                    },
                    laser_software = new SW_Version
                    {
                        version = parameters.LaserSoftware.version ?? "",
                        hash_key = parameters.LaserSoftware.hashKey ?? ""
                    },
                    robot_software = new SW_Version
                    {
                        version = parameters.RobotSoftware.version ?? "",
                        hash_key = parameters.RobotSoftware.hashKey ?? ""
                    },
                    ide_software = new SW_Version
                    {
                        version = parameters.IdeSoftware.version ?? "",
                        hash_key = parameters.IdeSoftware.hashKey ?? ""
                    },
                    config_file = new SW_Version
                    {
                        version = parameters.ConfigFile.version ?? "",
                        hash_key = parameters.ConfigFile.hashKey ?? ""
                    },
                    vision_template = new SW_Version
                    {
                        version = parameters.VisionTemplate.version ?? "",
                        hash_key = parameters.VisionTemplate.hashKey ?? ""
                    }
                },
                attributes = new Attributes
                {
                    x_offset = new attributes
                    {
                        old = parameters.XOffset.old ?? "",
                        NEW = parameters.ZOffset.NEW ?? ""
                    },
                    z_offset = new attributes
                    {
                        old = parameters.XOffset.old ?? "",
                        NEW = parameters.ZOffset.NEW ?? ""
                    }

                },
                station_info = HiveTool.heartbeatdata
            };

            return HiveTool.SerializeToJson(stationSWVersion,
                formatting: Formatting.Indented,
                nullValueHandling: NullValueHandling.Ignore,
                defaultValueHandling: DefaultValueHandling.Ignore);
        }

        /// <summary>
        /// 生成心跳JSON
        /// </summary>
        private string GenerateHeartbeatJson(Heartbeat heartbeat)
        {
            return HiveTool.SerializeToJson(heartbeat,
                formatting: Formatting.Indented,
                nullValueHandling: NullValueHandling.Ignore);
        }

        /// <summary>
        /// 生成Onboarding状态JSON
        /// </summary>
        private string GenerateOnboardingStatusJson(Heartbeat heartbeat)
        {
            return HiveTool.SerializeToJson(heartbeat,
                formatting: Formatting.Indented,
                nullValueHandling: NullValueHandling.Ignore);
        }

        /// <summary>
        /// 生成机器状态JSON
        /// </summary>
        private string GenerateMachineStateJson(MachineStateParameters parameters)
        {
            // 重用MachineState方法中的逻辑
            var data = new Dictionary<string, object>();

            // 添加可选数据字段
            if (!string.IsNullOrWhiteSpace(parameters.error_message))
                data["error_message"] = parameters.error_message;

            if (!string.IsNullOrWhiteSpace(parameters.code))
                data["code"] = parameters.code;

            if (!string.IsNullOrWhiteSpace(parameters.downtime_cause))
                data["downtime_cause"] = parameters.downtime_cause;

            if (!string.IsNullOrWhiteSpace(parameters.action_taken) || parameters.RatingReplace)
                data["action_taken"] = parameters.action_taken;

            if (!string.IsNullOrWhiteSpace(parameters.spare_parts))
                data["spare_parts"] = parameters.spare_parts;

            if (!string.IsNullOrWhiteSpace(parameters.rating)|| parameters.RatingReplace)
                data["rating"] = parameters.rating;

            if (!string.IsNullOrWhiteSpace(parameters.status))
                data["status"] = parameters.status;

            if (!string.IsNullOrWhiteSpace(parameters.badge))
                data["badge"] = parameters.badge;

            if (!string.IsNullOrWhiteSpace(parameters.event_id))
                data["event_id"] = parameters.event_id;

            // 合并额外数据
            foreach (var item in parameters.AdditionalData)
            {
                if (!data.ContainsKey(item.Key))
                    data.Add(item.Key, item.Value);
            }

            // 设置当前机器状态
            CurrentMachineState = ((int)parameters.MachineState).ToString();
            CurrentHiveStatus = Convert.ToInt32(CurrentMachineState);
            _currentHiveStatus = CurrentHiveStatus;
            if (!string.IsNullOrWhiteSpace(parameters.code))
                CurrentErrorCode = parameters.code;

            // 生成上传数据
            var machineStateData = new
            {
                machine_state = parameters.MachineState,
                state_change_time = parameters.StateChangeTime.ToString(HiveTool.TimeFormat),
                station_info = HiveTool.heartbeatdata,
                data = data
            };

            string strJson= HiveTool.SerializeToJson(machineStateData,
                formatting: Formatting.Indented,
                nullValueHandling: NullValueHandling.Ignore);
            // 处理Rating字段特殊替换
            if (parameters.RatingReplace)
            {
                strJson = Regex.Replace(strJson, @"""rating""\s*:\s*(""""|""|null)\s*,", @"""rating"": 0,");
                strJson = Regex.Replace(strJson, @"""action_taken""\s*:\s*(""""|""|null)\s*,", @"""action_taken"": """",");
            }
                

            return strJson;
        }

        /// <summary>
        /// 根据命令类型获取DataStyle
        /// </summary>
        private DataStyle GetDataStyleFromCommand(string command)
        {
            switch (command)
            {
                case "TRdata":
                    return DataStyle.TRData;
                case "Machinedata":
                    return DataStyle.MachineData;
                case "StationSwVersion":
                    return DataStyle.SWVersion;
                case "HeartBeat":
                    return DataStyle.Heartbeat;
                case "OnboardingStatus":
                    return DataStyle.OnboardingStatus;
                case "Machinestate":
                    return DataStyle.MachineState;
                default:
                    return DataStyle.Other_sfc;
            }
        }


        #endregion
    }

    /// <summary>
    /// 上传请求
    /// </summary>
    public class UploadRequest
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DataStyle DataStyle { get; set; }
        public string JsonData { get; set; }
        public DateTime EnqueueTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 队列状态
    /// </summary>
    public class QueueStatus
    {
        public int QueueLength { get; set; }
        public bool IsProcessing { get; set; }
        public UploadRequest CurrentRequest { get; set; }
    }

    /// <summary>
    /// 上传队列管理器 - 负责管理上传任务的排队和执行
    /// </summary>
    public class UploadQueueManager
    {
        #region 单例模式
        private static readonly Lazy<UploadQueueManager> _instance =
            new Lazy<UploadQueueManager>(() => new UploadQueueManager());
        public static UploadQueueManager Instance => _instance.Value;
        #endregion

        private readonly ConcurrentQueue<UploadRequest> _requestQueue = new ConcurrentQueue<UploadRequest>();
        private readonly object _processingLock = new object();
        private volatile bool _isProcessing = false;
        private Thread _processingThread;
        private readonly ManualResetEvent _queueEvent = new ManualResetEvent(false);
        private bool _shouldStop = false;
        private UploadRequest _currentRequest;

        private UploadQueueManager()
        {
            // 启动后台处理线程
            _processingThread = new Thread(ProcessQueue)
            {
                Name = "UploadQueueProcessor",
                IsBackground = true
            };
            _processingThread.Start();
        }

        /// <summary>
        /// 添加上传请求到队列（非阻塞）
        /// </summary>
        public void EnqueueUpload(DataStyle dataStyle, string jsonData)
        {
            var request = new UploadRequest
            {
                DataStyle = dataStyle,
                JsonData = jsonData
            };

            _requestQueue.Enqueue(request);
            _queueEvent.Set(); // 通知处理线程有新的任务

            SaveQueueLog($"请求入队: {dataStyle}, 队列长度: {_requestQueue.Count}");
        }

        /// <summary>
        /// 后台处理队列
        /// </summary>
        private void ProcessQueue()
        {
            while (!_shouldStop)
            {
                try
                {
                    _queueEvent.WaitOne(1000); // 等待信号或超时1秒

                    if (_shouldStop) break;

                    // 一次只处理一个请求
                    if (_requestQueue.TryDequeue(out var request))
                    {
                        ProcessSingleRequest(request);
                    }
                    else
                    {
                        _queueEvent.Reset(); // 队列为空，重置事件
                    }
                }
                catch (Exception ex)
                {
                    SaveQueueLog($"队列处理异常: {ex.Message}");
                    Thread.Sleep(1000); // 发生异常时暂停1秒
                }
            }
        }

        /// <summary>
        /// 处理单个请求
        /// </summary>
        private async void ProcessSingleRequest(UploadRequest request)
        {
            lock (_processingLock)
            {
                _isProcessing = true;
                _currentRequest = request;

                try
                {
                    SaveQueueLog($"开始处理: {request.DataStyle}, ID: {request.Id}");

                    // 使用原有的同步处理逻辑
                    HiveTool.Instance.ProcessUploadSync(request.DataStyle, request.JsonData);

                    SaveQueueLog($"处理完成: {request.DataStyle}, ID: {request.Id}");
                }
                catch (Exception ex)
                {
                    SaveQueueLog($"处理失败: {request.DataStyle}, 错误: {ex.Message}");
                }
                finally
                {
                    _isProcessing = false;
                    _currentRequest = null;
                }
            }
        }
        /// <summary>
        /// 获取队列状态
        /// </summary>
        public QueueStatus GetQueueStatus()
        {
            return new QueueStatus
            {
                QueueLength = _requestQueue.Count,
                IsProcessing = _isProcessing,
                CurrentRequest = _currentRequest
            };
        }

        /// <summary>
        /// 清空队列
        /// </summary>
        public void ClearQueue()
        {
            while (_requestQueue.TryDequeue(out _)) { }
        }

        /// <summary>
        /// 停止队列处理
        /// </summary>
        public void Stop()
        {
            _shouldStop = true;
            _queueEvent.Set();
            if (_processingThread != null && _processingThread.IsAlive)
            {
                _processingThread.Join(5000);
            }
        }

        private void SaveQueueLog(string message)
        {
            string logMessage = $"[{DateTime.Now:HH:mm:ss}] {message}\r\n";
            string path = @"D:\Hive\Hive Log\Other\UploadQueue_log";

            try
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                string filePath = Path.Combine(path, DateTime.Now.ToString("yyyyMMdd") + ".txt");
                System.IO.File.AppendAllText(filePath, logMessage);
            }
            catch { }
        }
    }


}