using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IO;
using System.Threading;
using System.Data;
using RestSharp;
using System.Windows.Forms;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Serialization;
using static AF.Upload.Hive.HiveMgr;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using AF.Communicate;
using AF.File;
using AF.Common;

namespace AF.Upload.Hive
{

    #region 数据类型
    /// <summary>
    /// Error Information
    /// </summary>
    public struct ErrorMessage
    {
        public string code;
        public string category;
        public string strMsgChinese;
        public string strMsgEnglish;

        public DateTime tm_begin;
        public DateTime tm_end;

        public int nIndex;

        public Dictionary<string, string> data;
    }

    public enum ProcessingMode
    {
        NormalRunWithPdcaSfc = 0,    // 生产单元，上传到 Insight/PDCA
        CPK = 1,           // GPK
        GRR = 2,           // GRR
        SCS = 3,           // SCS
        BG_C = 4,          // BG+C (IT)
        GMC = 11,           // GMC
        DryRun = 5,         // Dry run
        NormalRunNotWithPdcaSfc = 6
    }

    /// <summary>
    /// Error Machine State(包含报error 时的信息)
    /// 当状态为5时，可以保存error信息
    /// 用来暂存状态信息
    /// </summary>
    public struct StateMessage
    {
        /// <summary>
        /// 机台之前状态
        /// </summary>
        public int MachineState;
        /// <summary>
        /// 故障代码
        /// </summary>
        public string code;
        /// <summary>
        /// 故障信息
        /// </summary>       
        public string strMsg;
        /// <summary>
        /// 故障等级
        /// </summary>       
        public string strLevel;
    }

    /// <summary>
    /// 用于上传的数据类型
    /// （MachineData,MachineState,ErrorData,EnvironmentData）
    /// </summary>
    public enum DataStyle
    {
        MachineData = 1,
        MachineState = 2,
        TRData = 3,
        SWVersion = 4,
        Heartbeat = 5,
        OnboardingStatus = 6,
        Other_sfc = 7,
    }

    /// <summary>
    /// 设备根据Hive要求的运行状态
    /// </summary>
    public enum MachineLocalState
    {
        MachineRunning,
        MachineStop,
        MachineResume,
        MachinePause,
        Machine5Pcs,
    }

    #endregion 数据类型

    public class MachineStateParameters
    {
        // 必需字段
        public MachineStateData.MachineState MachineState { get; set; }
        public DateTime StateChangeTime { get; set; } = DateTime.Now;

        // 可选字段
        public string error_message { get; set; }
        public string code { get; set; }
        public string status { get; set; }
        public string downtime_cause { get; set; }
        public string action_taken { get; set; }
        public string spare_parts { get; set; }
        public string badge { get; set; }
        public string rating { get; set; }
        public string event_id { get; set; }

        // 特殊处理标志
        public bool RatingReplace { get; set; } = false;

        // 额外数据字段
        private Dictionary<string, object> _additionalData;
        public Dictionary<string, object> AdditionalData
        {
            get => _additionalData ?? (_additionalData = new Dictionary<string, object>());
            set => _additionalData = value;
        }

        public bool Validate()
        {
            // 基本验证：状态值在有效范围内
            return Enum.IsDefined(typeof(MachineStateData.MachineState), MachineState);
        }
    }

    /// <summary>
    /// 机器状态数据
    /// </summary>
    public class MachineStateData
    {
        public MachineState machine_state { get; set; } = MachineState.UnplannedDown;
        public string state_change_time { get; set; } = string.Empty;
        public Dictionary<string, object> data { get; set; } = new Dictionary<string, object>();
        public Heartbeat station_info { get; set; } = new Heartbeat();

        public enum MachineState
        {
            Running = 1,
            Idle = 2,
            UnplannedDown = 5,
        }
    }


    /// <summary>
    /// Hive数据管理类 - 负责数据上传和管理
    /// </summary>
    public class HiveTool
    {
        #region 单例模式
        private static readonly Lazy<HiveTool> lazyInstance = new Lazy<HiveTool>(() => new HiveTool());
        public static HiveTool Instance => lazyInstance.Value;
        #endregion

        #region 常量和字段
        public static object m_lock_hive = new object();

        public static Heartbeat heartbeatdata = new Heartbeat();
        public static bool HiveIsResponseApple = true;
        public static bool IsNormalRunMode = true;
        public static DateTime DT_Running2Pause = DateTime.Now;
        public static List<ErrorMessage> LastError = new List<ErrorMessage>();
        public static bool SendFlag = false;
        public static bool[] BackUp_SendFlag = new bool[] { false, false, false, false };

        public static bool HiveIsConnected = true;

        private const string Time_Format = "yyyy-MM-ddTHH:mm:ss.ff+0800"; /*"yyyy-MM-ddTHH:mm:ss.ffzzz";*/
        private static readonly object lock_log = new object();
        private static readonly object lock_showlog = new object();
        private static readonly object lock_movefile = new object();

        public static string Event_id = "";

        private static readonly object _lock = new object();
        public static string _machinestate;

        public static string CurrentErrorCode;
        public static bool StationSimulator = false;//Hive仿真器
        public static string CurrentMachineState
        {
            get { return _machinestate; }
            set
            {
                lock (_lock)
                {
                    if (_machinestate != value)
                    {
                        _machinestate = value;
                        Ini.WriteValue(HiveCommandProcessor.m_strIniFile, "LocalData", "MachineState", _machinestate);
                    }
                }
            }
        }
        /// <summary>
        /// 用于Form_Main界面状态显示
        /// </summary>
        public static int CurrentHiveStatus;
        /// <summary>
        /// 用于Form_Main界面状态显示，通信失败用于缓存
        /// </summary>
        public static int _currentHiveStatus;

        public static string TimeFormat => Time_Format;

        public TcpClient m_tcpHive = null;

        private static int _Hive5Pcs = 0;
        public static int Hive5Pcs
        {
            get { return _Hive5Pcs; }
            set
            {
                _Hive5Pcs = value;
                if (_Hive5Pcs == 5 && _machine5PcsStatus)
                {
                    HiveMgr.MachinePause?.Invoke();
                    _machine5PcsStatus = false;
                }
            }
        }
        private static bool _machine5PcsStatus = false;
        public static bool Machine5PcsStatus
        {
            get { return _machine5PcsStatus; }
            set
            {
                _machine5PcsStatus = value;
                if (_machine5PcsStatus)
                {
                    _Hive5Pcs = 0;
                }
            }
        }
        #endregion

        #region 构造函数和初始化
        public HiveTool()
        {

        }

        #endregion

        #region 公共方法
        /// <summary>
        /// 获取API URL
        /// </summary>
        public string GetUrl(DataStyle dataStyle)
        {
            string url = string.Empty;
            switch (dataStyle)
            {
                case DataStyle.MachineData:
                    url = "http://10.0.0.2:5008/capture/v6/machinedata";
                    break;
                case DataStyle.MachineState:
                    url = "http://10.0.0.2:5008/capture/v6/machinestate";
                    break;
                case DataStyle.TRData:
                    url = "http://10.0.0.2:5008/capture/v6/errordata";
                    break;
                case DataStyle.SWVersion:
                    url = "http://10.0.0.2:5008/capture/v6/softwareversions";
                    //url = "http://172.0.0.1:8080/capture/v6/softwareversions";
                    break;
                case DataStyle.Heartbeat:
                    url = "http://10.0.0.2:5008/capture/v6/heartbeat";
                    //url = "http://172.0.0.1:8080/capture/v6/heartbeat";
                    break;
                case DataStyle.OnboardingStatus:
                    url = "http://10.0.0.2:5008/idms/v2/station/status";
                    break;
            }

            return url;
        }
        public string GetUrlEx(DataStyle dataStyle)
        {
            string strURL = "";
            string strFile = "";
            if (!StationSimulator)
            {

                strFile = AppDomain.CurrentDomain.BaseDirectory + "HiveParam.ini";
            }
            else
            {
                strFile = AppDomain.CurrentDomain.BaseDirectory + "HiveParamSimulator.ini";
            }

            strURL = Ini.GetStringValue(strFile, "HiveURL", dataStyle.ToString(), "null");
            return strURL;
        }
        /// <summary>
        /// 机器数据上传
        /// </summary>
        public async static void MachineDataUpload(MachineDataParameters parameters, Dictionary<string, object> data)
        {
            var machineData = new MachineData
            {
                unit_sn = parameters.UnitSn.TrimToNull() ?? Guid.NewGuid().ToString(),
                pass = parameters.Pass,
                input_time = parameters.InputTime.ToString(TimeFormat),
                output_time = parameters.OutputTime.ToString(TimeFormat),
                station_info = heartbeatdata,
                serials = parameters.Serials ?? new Dictionary<string, object>(),
                data = data
            };

            string json = SerializeToJson(machineData,
                formatting: Formatting.Indented,
                nullValueHandling: NullValueHandling.Ignore);

            //Instance.ProcessJson(DataStyle.MachineData, json);
            await Instance.ProcessJsonAsync(DataStyle.MachineData, json);
        }


        ///// <summary>
        ///// 机台状态上传
        ///// </summary>
        //public static void ChangeStateUpload(StateMessageNew state)
        //{
        //    HiveCommandProcessor.CurrentMachineState = state.MachineState.ToString();

        //    var ms = new MachineStateData
        //    {
        //        machine_state = (MachineStateData.MachineState)state.MachineState,
        //        state_change_time = DateTime.Now.ToString(TimeFormat),
        //        station_info = heartbeatdata
        //    };

        //    // 添加可选数据字段
        //    AddOptionalData(ms.data, "error_message", state.error_message);
        //    AddOptionalData(ms.data, "code", state.code);
        //    AddOptionalData(ms.data, "downtime_cause", state.downtime_cause);
        //    AddOptionalData(ms.data, "action_taken", state.action_taken);
        //    AddOptionalData(ms.data, "spare_parts", state.spare_parts);
        //    AddOptionalData(ms.data, "rating", state.rating);
        //    AddOptionalData(ms.data, "status", state.status);
        //    AddOptionalData(ms.data, "badge", state.badge);
        //    AddOptionalData(ms.data, "event_id", state.event_id);

        //    string strJson = SerializeToJson(ms, formatting: Formatting.Indented);
        //    Instance.ProcessJson(DataStyle.MachineState, strJson);
        //}
        /// <summary>
        /// 机器状态数据上传
        /// </summary>
        public async static void MachineStateUpload(MachineStateParameters parameters, Dictionary<string, object> data)
        {
            var machineStateData = new MachineStateData
            {
                machine_state = parameters.MachineState,
                state_change_time = parameters.StateChangeTime.ToString(TimeFormat),
                station_info = heartbeatdata,
                data = data
            };

            string strJson = SerializeToJson(machineStateData, formatting: Formatting.Indented);

            // 处理Rating字段特殊替换
            if (parameters.RatingReplace)
                strJson = Regex.Replace(strJson, @"""rating""\s*:\s*(""""|""|null)\s*,", @"""rating"": ,");

            //Instance.ProcessJson(DataStyle.MachineState, strJson);
            await Instance.ProcessJsonAsync(DataStyle.MachineState, strJson);
        }
        ///// <summary>
        ///// 机台状态上传
        ///// </summary>
        //public static void ChangeStateUpload(MachineStateData ms, bool RatingReplace = false)
        //{
        //    string strJson = SerializeToJson(ms, formatting: Formatting.Indented);
        //    // 然后替换rating字段
        //    if (RatingReplace)
        //        strJson = Regex.Replace(strJson, @"""rating""\s*:\s*(""""|""|null)\s*,", @"""rating"": ,");
        //    Instance.ProcessJson(DataStyle.MachineState, strJson);
        //}
        /// <summary>
        /// TR数据上传
        /// </summary>
        public async static void TRDataUpload(TRDataParameters parameters, Dictionary<string, object> data)
        {
            var trData = new TRData
            {
                message = parameters.Message,
                code = parameters.Code,
                severity = parameters.Severity,
                occurrence_time = parameters.OccurrenceTime.ToString(TimeFormat),
                resolved_time = parameters.ResolvedTime.ToString(TimeFormat),
                station_info = heartbeatdata,
                data = data
            };

            string json = SerializeToJson(trData,
                formatting: Formatting.Indented,
                nullValueHandling: NullValueHandling.Ignore);

            //Instance.ProcessJson(DataStyle.TRData, json);
            await Instance.ProcessJsonAsync(DataStyle.TRData, json);
        }
        /// <summary>
        /// SWVersion上传
        /// </summary>
        public async static void StationSwVersionUpload(StationSwVersionParameters parameters)
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
                station_info = heartbeatdata
            };
            string upload;
            string json = SerializeToJson(stationSWVersion,
                formatting: Formatting.Indented,
                nullValueHandling: NullValueHandling.Ignore,
                defaultValueHandling: DefaultValueHandling.Ignore);
            upload = json.Replace("NEW", "new");

            //Instance.ProcessJson(DataStyle.SWVersion, json);

            await Instance.ProcessJsonAsync(DataStyle.SWVersion, upload);
        }
        /// <summary>
        /// Heartbeat数据上传
        /// </summary>
        public async static void HeartbeatUpload(Heartbeat heartbeat)
        {
            string json = SerializeToJson(heartbeat,
                formatting: Formatting.Indented,
                nullValueHandling: NullValueHandling.Ignore);

            //Instance.ProcessJson(DataStyle.Heartbeat, json);
            await Instance.ProcessJsonAsync(DataStyle.Heartbeat, json);
        }
        /// <summary>
        /// Heartbeat数据上传
        /// </summary>
        public async static void OnboardingStatusUpload(Heartbeat heartbeat)
        {
            string json = SerializeToJson(heartbeat,
                formatting: Formatting.Indented,
                nullValueHandling: NullValueHandling.Ignore);

            //Instance.ProcessJson(DataStyle.OnboardingStatus, json);
            await Instance.ProcessJsonAsync(DataStyle.OnboardingStatus, json);
        }

        #endregion

        #region 核心上传逻辑


        /// <summary>
        /// 同步处理上传
        /// </summary>
        public void ProcessUploadSync(DataStyle dataStyle, string jsonData)
        {
            string responseString = "";
            bool result = false;
            string postUrl = GetUrlEx(dataStyle);
            int maxRetries = 5;
            int retryDelayMs = 100;

            bool isRetry = false;

            while (true)
            {
                for (int i = 0; i < maxRetries; i++)
                {
                    try
                    {
                        if (!HiveTool.HiveIsResponseApple)
                        {
                            result = true;
                            responseString = "Hive does not respond to Apple";
                            ShowLog(jsonData, result, "", dataStyle);
                            return;
                        }

                        result = HiveTool.Instance.PostData(postUrl, jsonData, ref responseString);

                        // 处理上传结果
                        bool uploadSuccess = ProcessUploadResult(dataStyle, result, responseString);

                        if (uploadSuccess)
                        {
                            if (dataStyle == DataStyle.OnboardingStatus)
                            {
                                string statusString = responseString?.ToString().ToLower() ?? "";
                                if (!statusString.Contains("onboarded"))
                                {
                                    HiveTool.CurrentHiveStatus = (int)AF.Upload.Hive.HiveStatusControl.HiveStatus.Offboard;
                                    break;
                                }
                            }

                            ShowLog(jsonData, true, responseString, dataStyle);
                            if (isRetry)
                            {
                                HiveTool.CurrentHiveStatus = HiveTool._currentHiveStatus;
                            }
                            HiveIsConnected = true;
                            return;
                        }
                        else
                        {
                            HiveTool.CurrentHiveStatus = (int)AF.Upload.Hive.HiveStatusControl.HiveStatus.CommandError;
                            string errorMsg = $"第 {i + 1} 次尝试失败: {GetFailureReason(result, responseString, dataStyle)}";
                            ShowLog(jsonData, false, errorMsg, dataStyle);

                            if (i < maxRetries - 1)
                            {
                                //await Task.Delay(retryDelayMs);
                                Thread.Sleep(retryDelayMs);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        string exceptionMsg = $"第 {i + 1} 次尝试发生异常: {ex.Message}";
                        ShowLog(jsonData, false, exceptionMsg, dataStyle);

                        if (i < maxRetries - 1)
                        {
                            //await Task.Delay(retryDelayMs);
                            Thread.Sleep(retryDelayMs);
                        }
                    }
                }

                isRetry = true;
                HiveIsConnected = false;
                // 通信失败，需要用户响应
                ShowOffboardMessageNonModal().GetAwaiter().GetResult();
            }
        }
        /// <summary>
        /// 使用非模态窗体显示离线消息（不阻塞UI）
        /// </summary>
        private Task<bool> ShowOffboardMessageNonModal()
        {
            var tcs = new TaskCompletionSource<bool>();

            // 添加应用程序状态检查
            if (Application.OpenForms.Count == 0)
            {
                tcs.SetResult(false);
                return tcs.Task;
            }

            // 使用反射获取Form_Main类型
            var mainFormType = Type.GetType("AutoFrame.Form_Main, AutoFrame");
            if (mainFormType == null)
            {
                tcs.SetResult(false);
                return tcs.Task;
            }
            // 通过类型查找窗体
            var mainForm = Application.OpenForms.OfType<Form>()
                .FirstOrDefault(form => form.GetType() == mainFormType);

            if (mainForm == null || mainForm.IsDisposed || !mainForm.IsHandleCreated)
            {
                tcs.SetResult(false);
                return tcs.Task;
            }



            mainForm.BeginInvoke(new Action(() =>
            {
                try
                {
                    var form = new Form_OffboardMsg();

                    // 显式设置Owner为主窗体
                    if (HiveMgr.SetFormOwner)
                        form.Owner = mainForm;

                    // 订阅窗体关闭事件
                    form.FormClosed += (s, e) =>
                        {
                            // 根据窗体结果设置任务结果
                            bool result = form.DialogResult == DialogResult.OK ||
                                         form.DialogResult == DialogResult.Yes;
                            tcs.TrySetResult(result);
                            form.Dispose();
                        };
                    form.Load += (s, e) =>
                        {
                            form.BringToFront();
                            form.Focus();
                        };
                    // 以非模态方式显示窗体
                    form.Show();
                }
                catch (Exception ex)
                {
                    tcs.TrySetResult(false);
                    Debug.WriteLine($"ShowOffboardMessageNonModal failed: {ex.Message}");
                }
            }));


            return tcs.Task;
        }
        /// <summary>
        /// 同步显示离线消息弹窗
        /// </summary>
        private bool ShowOffboardMessageSync()
        {
            if (Application.OpenForms.Count > 0 && !Application.OpenForms[0].IsDisposed)
            {
                var mainForm = Application.OpenForms[0];
                bool result = false;

                mainForm.Invoke(new Action(() =>
                {
                    using (var form = new Form_OffboardMsg())
                    {
                        var dialogResult = form.ShowDialog();
                        result = dialogResult == DialogResult.OK || dialogResult == DialogResult.Yes;
                    }
                }));

                return result;
            }
            else
            {
                using (var form = new Form_OffboardMsg())
                {
                    var dialogResult = form.ShowDialog();
                    return dialogResult == DialogResult.OK || dialogResult == DialogResult.Yes;
                }
            }
        }



        /// <summary>
        /// 处理JSON - 异步版本
        /// </summary>
        public async Task ProcessJsonAsync(DataStyle dataStyle, string strJson)
        {
            if (string.IsNullOrEmpty(strJson))
            {
                ShowLog(strJson, false, "上传数据为空", dataStyle);
                return;
            }

            string responseString = "";
            bool result = false;
            string postUrl = GetUrlEx(dataStyle);
            int maxRetries = 5;
            int retryDelayMs = 1000;

            bool IsRetry = false;

            while (true)
            {
                // 重试机制
                for (int i = 0; i < maxRetries; i++)
                {
                    try
                    {
                        if (!HiveIsResponseApple)
                        {
                            result = true;
                            responseString = "Hive does not respond to Apple";
                            ShowLog(strJson, result, "", dataStyle);
                            return;
                        }

                        // 执行POST请求
                        result = PostData(postUrl, strJson, ref responseString);

                        // 处理上传结果
                        bool uploadSuccess = ProcessUploadResult(dataStyle, result, responseString);

                        if (uploadSuccess)
                        {
                            if (dataStyle == DataStyle.OnboardingStatus)
                            {
                                string statusString = responseString?.ToString().ToLower() ?? "";

                                if (!statusString.Contains("onboarded"))
                                {
                                    CurrentHiveStatus = (int)AF.Upload.Hive.HiveStatusControl.HiveStatus.Offboard;
                                    break;
                                }
                            }

                            ShowLog(strJson, true, responseString, dataStyle);
                            if (IsRetry)
                            {
                                CurrentHiveStatus = _currentHiveStatus;
                            }

                            return; // 成功则退出
                        }
                        else
                        {
                            CurrentHiveStatus = (int)AF.Upload.Hive.HiveStatusControl.HiveStatus.CommandError;
                            // 记录失败日志
                            string errorMsg = $"第 {i + 1} 次尝试失败: {GetFailureReason(result, responseString, dataStyle)}";
                            ShowLog(strJson, false, errorMsg, dataStyle);

                            if (i < maxRetries - 1)
                            {
                                await Task.Delay(retryDelayMs);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        string exceptionMsg = $"第 {i + 1} 次尝试发生异常: {ex.Message}";
                        ShowLog(strJson, false, exceptionMsg, dataStyle);

                        if (i < maxRetries - 1)
                        {
                            await Task.Delay(retryDelayMs);
                            //retryDelayMs *= 2;
                        }
                    }
                }

                IsRetry = true;

                // 异步显示弹窗，等待用户响应但不阻塞UI
                await ShowOffboardMessageAsync();



                // 用户确认继续，重新尝试上传
                ShowLog(strJson, false, "用户确认后重新尝试上传", dataStyle);
            }
        }

        /// <summary>
        /// 异步显示离线消息弹窗
        /// </summary>
        private async Task<bool> ShowOffboardMessageAsync()
        {
            // 确保在UI线程上创建窗体
            if (Application.OpenForms.Count > 0 && !Application.OpenForms[0].IsDisposed)
            {
                var mainForm = Application.OpenForms[0];

                // 在 UI 线程上创建和显示窗体
                Form_OffboardMsg form_Offboard = null;
                mainForm.Invoke(new Action(() =>
                {
                    form_Offboard = new Form_OffboardMsg();
                    form_Offboard.Show(); // 非模态显示，不阻塞UI
                }));

                try
                {
                    // 在后台线程等待用户响应
                    bool result = await form_Offboard.UserResponseTask;
                    return result;
                }
                finally
                {
                    // 确保窗体被正确释放
                    mainForm.Invoke(new Action(() =>
                    {
                        form_Offboard?.Close();
                        form_Offboard?.Dispose();
                    }));
                }
            }
            else
            {
                // 如果没有打开的窗体，使用同步方式
                using (var form_Offboard = new Form_OffboardMsg())
                {
                    var dialogResult = form_Offboard.ShowDialog();
                    return dialogResult == DialogResult.OK || dialogResult == DialogResult.Yes;
                }
            }
        }
        /// <summary>
        /// 处理JSON
        /// </summary>
        private void ProcessJson(DataStyle dataStyle, string strJson)
        {
            if (string.IsNullOrEmpty(strJson))
            {
                ShowLog(strJson, false, "上传数据为空", dataStyle);
                return;
            }

            string responseString = "";
            bool result = false;
            string postUrl = GetUrlEx(dataStyle);
            int maxRetries = 5;
            int retryDelayMs = 1000; // 重试延迟1秒

            bool IsRetry = false; ;
            while (true)
            {
                // 重试机制
                for (int i = 0; i < maxRetries; i++)
                {
                    try
                    {
                        if (!HiveIsResponseApple)
                        {
                            result = true;
                            responseString = "Hive does not respond to Apple";
                            ShowLog(strJson, result, "", dataStyle);
                            return;
                        }

                        // 执行POST请求
                        result = PostData(postUrl, strJson, ref responseString);

                        // 处理上传结果
                        bool uploadSuccess = ProcessUploadResult(dataStyle, result, responseString);

                        if (uploadSuccess)
                        {
                            if (dataStyle == DataStyle.OnboardingStatus)
                            {
                                string statusString = responseString?.ToString().ToLower() ?? "";

                                if (!statusString.Contains("onboarded"))
                                {
                                    CurrentHiveStatus = (int)AF.Upload.Hive.HiveStatusControl.HiveStatus.Offboard;
                                    break;
                                }
                            }

                            ShowLog(strJson, true, responseString, dataStyle);
                            if (IsRetry)
                            {
                                CurrentHiveStatus = _currentHiveStatus;
                            }

                            return; // 成功则退出
                        }
                        else
                        {

                            CurrentHiveStatus = (int)AF.Upload.Hive.HiveStatusControl.HiveStatus.CommandError;
                            // 记录失败日志
                            string errorMsg = $"第 {i + 1} 次尝试失败: {GetFailureReason(result, responseString, dataStyle)}";
                            ShowLog(strJson, false, errorMsg, dataStyle);

                            // 如果不是最后一次尝试，则延迟后重试
                            if (i < maxRetries - 1)
                            {
                                Thread.Sleep(retryDelayMs);
                                //retryDelayMs *= 2; // 指数退避
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        string exceptionMsg = $"第 {i + 1} 次尝试发生异常: {ex.Message}";
                        ShowLog(strJson, false, exceptionMsg, dataStyle);

                        if (i < maxRetries - 1)
                        {
                            Thread.Sleep(retryDelayMs);
                            retryDelayMs *= 2;
                        }
                    }
                }
                IsRetry = true;

                Form_OffboardMsg form_Offboard = new Form_OffboardMsg();
                form_Offboard.ShowDialog();


                // 所有重试都失败
                ShowLog(strJson, false, $"所有 {maxRetries} 次尝试均失败", dataStyle);
            }

        }
        /// <summary>
        /// 获取失败原因
        /// </summary>
        private string GetFailureReason(bool networkSuccess, string response, DataStyle dataStyle)
        {
            if (!networkSuccess)
            {
                return "网络通信失败";
            }

            var hiveResponse = response.GetHiveServerModel();
            if (hiveResponse == null)
            {
                return "服务器响应格式错误";
            }

            return hiveResponse.GetErrorMessage();
        }
        /// <summary>
        /// 处理上传结果
        /// </summary>
        private bool ProcessUploadResult(DataStyle dataStyle, bool networkSuccess, string response)
        {

            if (!HiveIsResponseApple) return true;


            // 网络通信失败
            if (!networkSuccess)
                return false;


            try
            {
                var hiveResponse = response.GetHiveServerModel();
                bool hasValidResponse = hiveResponse != null;

                if (hasValidResponse)
                {
                    // 检查响应是否成功
                    if (hiveResponse.IsSuccess(dataStyle))
                    {
                        if (true)
                        {

                        }

                        return true;
                    }
                    else
                    {
                        // 记录服务器返回的错误信息
                        string errorMsg = hiveResponse.machine_state.ToString();
                        Console.WriteLine($"服务器返回错误: {errorMsg}");
                        return false;
                    }
                }
                else
                {
                    // 响应格式错误
                    Console.WriteLine($"无效的服务器响应格式: {response}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"处理上传结果时发生异常: {ex.Message}");
                return false;
            }
        }


        #endregion

        #region 文件操作
        /// <summary>
        /// 保存日志到本地文件
        /// </summary>
        public bool SaveHiveLog(DataStyle dataStyle, string message)
        {
            string basePath = @"D:\Hive\Hive Log";
            string subPath = "", fullPath = "", filePath = "";

            switch (dataStyle)
            {
                case DataStyle.MachineData:
                    subPath = "Machine Data";
                    fullPath = Path.Combine(basePath, subPath, DateTime.Now.ToString("yyyyMMdd"));
                    filePath = Path.Combine(fullPath, $"Hour_{DateTime.Now:HH}.txt");
                    break;
                case DataStyle.MachineState:
                    subPath = "Machine State";
                    fullPath = Path.Combine(basePath, subPath);
                    filePath = Path.Combine(fullPath, $"{DateTime.Now.ToString("yyyyMMdd")}.txt");
                    break;
                case DataStyle.TRData:
                    subPath = "TR Data";
                    fullPath = Path.Combine(basePath, subPath);
                    filePath = Path.Combine(fullPath, $"{DateTime.Now.ToString("yyyyMMdd")}.txt");
                    break;
                case DataStyle.SWVersion:
                    subPath = "SW Version";
                    fullPath = Path.Combine(basePath, subPath);
                    filePath = Path.Combine(fullPath, $"{DateTime.Now.ToString("yyyyMMdd")}.txt");
                    break;
                case DataStyle.Heartbeat:
                    subPath = "Heartbeat";
                    fullPath = Path.Combine(basePath, subPath);
                    filePath = Path.Combine(fullPath, $"{DateTime.Now.ToString("yyyyMMdd")}.txt");
                    break;
                case DataStyle.OnboardingStatus:
                    subPath = "Onboarding Status";
                    fullPath = Path.Combine(basePath, subPath);
                    filePath = Path.Combine(fullPath, $"onboarding_status.txt");
                    break;
                case DataStyle.Other_sfc:
                    subPath = "Other";
                    fullPath = Path.Combine(basePath, subPath);
                    filePath = Path.Combine(fullPath, $"Card Reader_log.txt");
                    break;
                default:
                    subPath = "Other";
                    fullPath = Path.Combine(basePath, subPath, DateTime.Now.ToString("yyyyMMdd"));
                    filePath = Path.Combine(fullPath, $"Hour_{DateTime.Now:HH}.txt");
                    break;

            }

            lock (lock_log)
            {
                try
                {
                    Directory.CreateDirectory(fullPath);
                    System.IO.File.AppendAllText(filePath, message);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
        #endregion

        #region HTTP通信
        /// <summary>
        /// 发送POST请求
        /// </summary>
        public bool PostData(string url, string postData, ref string responseString)
        {

            try
            {
                var client = new RestClient(url)
                {
                    Timeout = 1000,
                    // 添加重试策略
                    // RetryPolicy = new RetryPolicy(3, 1000)
                };

                var request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", "application/json; charset=utf-8");
                request.AddHeader("Accept", "application/json; charset=utf-8\"");
                request.AddParameter("application/json; charset=utf-8", postData, ParameterType.RequestBody);

                var response = client.Execute(request);

                // 检查HTTP状态码
                if (response.ResponseStatus != ResponseStatus.Completed)
                {
                    responseString = $"请求未完成: {response.ResponseStatus} - {response.ErrorMessage}";
                    return false;
                }

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    responseString = $"HTTP错误: {(int)response.StatusCode} - {response.StatusDescription}";
                    return false;
                }

                responseString = response.Content ?? "响应内容为空";
                return true;
            }
            catch (WebException ex)
            {
                responseString = $"网络异常: {ex.Message}";
                if (ex.Status == WebExceptionStatus.Timeout)
                {
                    responseString = "请求超时";
                }
                return false;
            }
            catch (Exception ex)
            {
                responseString = $"请求异常: {ex.Message}";
                return false;
            }

            //try
            //{
            //    var client = new RestClient(url) { Timeout = 10000 };
            //    var request = new RestRequest(Method.POST);
            //    request.AddHeader("Content-Type", "application/json");
            //    request.AddParameter("application/json", postData, ParameterType.RequestBody);

            //    var response = client.Execute(request);
            //    responseString = response.Content ?? response.ErrorMessage;

            //    return responseString != null;
            //}
            //catch (Exception ex)
            //{
            //    responseString = ex.ToString();
            //    return false;
            //}
        }
        #endregion

        #region JSON处理
        /// <summary>
        /// 压缩JSON字符串（移除空白字符）
        /// </summary>
        public static string Compress(string json)
        {
            var sb = new StringBuilder();
            using (var reader = new StringReader(json))
            {
                int ch, lastch = -1;
                bool inQuotes = false;

                while ((ch = reader.Read()) > -1)
                {
                    if ((char)lastch != '\\' && (char)ch == '\"')
                        inQuotes = !inQuotes;

                    if (!char.IsWhiteSpace((char)ch) || inQuotes)
                        sb.Append((char)ch);

                    lastch = ch;
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 格式化JSON字符串
        /// </summary>
        public string ConvertJsonString(string str)
        {
            try
            {
                var obj = JsonConvert.DeserializeObject(str);
                return JsonConvert.SerializeObject(obj, Formatting.Indented);
            }
            catch
            {
                return str;
            }
        }



        /// <summary>
        /// 将对象序列化为JSON字符串
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="obj">要序列化的对象</param>
        /// <param name="formatting">格式化选项，默认：None（紧凑格式）</param>
        /// <param name="nullValueHandling">Null值处理方式，默认：Include（包含null值）</param>
        /// <param name="defaultValueHandling">默认值处理方式，默认：Include（包含默认值）</param>
        /// <param name="dateFormatString">日期格式字符串，默认：yyyy-MM-ddTHH:mm:ss.fffzzz</param>
        /// <param name="dateTimeZoneHandling">日期时区处理方式，默认：RoundtripKind（保持原有时区）</param>
        /// <param name="referenceLoopHandling">循环引用处理方式，默认：Error（遇到循环引用时抛出异常）</param>
        /// <param name="missingMemberHandling">缺少成员处理方式，默认：Ignore（忽略JSON中多余的成员）</param>
        /// <param name="typeNameHandling">类型名称处理方式，默认：None（不包含类型信息）</param>
        /// <param name="stringEscapeHandling">字符串转义处理方式，默认：Default（默认转义）</param>
        /// <param name="floatFormatHandling">浮点数格式处理方式，默认：String（作为字符串处理）</param>
        /// <param name="floatParseHandling">浮点数解析处理方式，默认：Double（作为double处理）</param>
        /// <param name="contractResolver">合约解析器，默认：null（使用默认解析器）</param>
        /// <param name="converters">自定义转换器集合，默认：null</param>
        /// <returns>JSON字符串</returns>
        public static string SerializeToJson<T>(
            T obj,
            Formatting formatting = Formatting.None,
            NullValueHandling nullValueHandling = NullValueHandling.Include,
            DefaultValueHandling defaultValueHandling = DefaultValueHandling.Include,
            string dateFormatString = "yyyy-MM-ddTHH:mm:ss.fffzzz",
            DateTimeZoneHandling dateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
            ReferenceLoopHandling referenceLoopHandling = ReferenceLoopHandling.Error,
            MissingMemberHandling missingMemberHandling = MissingMemberHandling.Ignore,
            TypeNameHandling typeNameHandling = TypeNameHandling.None,
            StringEscapeHandling stringEscapeHandling = StringEscapeHandling.Default,
            FloatFormatHandling floatFormatHandling = FloatFormatHandling.String,
            FloatParseHandling floatParseHandling = FloatParseHandling.Double,
            IContractResolver contractResolver = null,
            IEnumerable<JsonConverter> converters = null)
        {
            if (obj == null)
                return "null";

            try
            {
                var settings = new JsonSerializerSettings
                {
                    // 格式化选项
                    // 默认：Formatting.None - 紧凑格式，无缩进
                    // 可选：Formatting.Indented - 缩进格式，便于阅读
                    Formatting = formatting,

                    // Null值处理方式
                    // 默认：NullValueHandling.Include - 包含null值
                    // 可选：NullValueHandling.Ignore - 忽略null值属性
                    NullValueHandling = nullValueHandling,

                    // 默认值处理方式
                    // 默认：DefaultValueHandling.Include - 包含默认值
                    // 可选：DefaultValueHandling.Ignore - 忽略默认值属性
                    DefaultValueHandling = defaultValueHandling,

                    // 日期格式字符串
                    // 默认："yyyy-MM-ddTHH:mm:ss.fffzzz" - ISO 8601格式
                    DateFormatString = dateFormatString,

                    // 日期时区处理方式
                    // 默认：DateTimeZoneHandling.RoundtripKind - 保持原有时区信息
                    // 可选：DateTimeZoneHandling.Utc - 转换为UTC时间
                    // 可选：DateTimeZoneHandling.Local - 转换为本地时间
                    // 可选：DateTimeZoneHandling.Unspecified - 不指定时区
                    DateTimeZoneHandling = dateTimeZoneHandling,

                    // 循环引用处理方式
                    // 默认：ReferenceLoopHandling.Error - 遇到循环引用时抛出异常
                    // 可选：ReferenceLoopHandling.Ignore - 忽略循环引用
                    // 可选：ReferenceLoopHandling.Serialize - 序列化循环引用
                    ReferenceLoopHandling = referenceLoopHandling,

                    // 缺少成员处理方式
                    // 默认：MissingMemberHandling.Ignore - 忽略JSON中多余的成员
                    // 可选：MissingMemberHandling.Error - 遇到多余成员时抛出异常
                    MissingMemberHandling = missingMemberHandling,

                    // 类型名称处理方式
                    // 默认：TypeNameHandling.None - 不包含类型信息
                    // 可选：TypeNameHandling.Objects - 对象类型包含类型信息
                    // 可选：TypeNameHandling.Arrays - 数组类型包含类型信息
                    // 可选：TypeNameHandling.All - 所有类型都包含类型信息
                    // 可选：TypeNameHandling.Auto - 自动决定是否包含类型信息
                    TypeNameHandling = typeNameHandling,

                    // 字符串转义处理方式
                    // 默认：StringEscapeHandling.Default - 默认转义规则
                    // 可选：StringEscapeHandling.EscapeNonAscii - 转义非ASCII字符
                    // 可选：StringEscapeHandling.EscapeHtml - 转义HTML字符
                    StringEscapeHandling = stringEscapeHandling,

                    // 浮点数格式处理方式
                    // 默认：FloatFormatHandling.String - 将特殊浮点数值（NaN、Infinity）作为字符串
                    // 可选：FloatFormatHandling.Symbol - 使用符号表示特殊浮点数值
                    // 可选：FloatFormatHandling.DefaultValue - 使用默认值替换特殊浮点数值
                    FloatFormatHandling = floatFormatHandling,

                    // 浮点数解析处理方式
                    // 默认：FloatParseHandling.Double - 作为double类型处理
                    // 可选：FloatParseHandling.Decimal - 作为decimal类型处理
                    FloatParseHandling = floatParseHandling
                };

                // 合约解析器
                // 默认：null - 使用默认合约解析器
                // 常用：CamelCasePropertyNamesContractResolver - 驼峰命名
                if (contractResolver != null)
                {
                    settings.ContractResolver = contractResolver;
                }

                // 自定义转换器集合
                // 默认：null - 不使用自定义转换器
                if (converters != null && converters.Any())
                {
                    settings.Converters = converters.ToList();
                }

                return JsonConvert.SerializeObject(obj, settings);
            }
            catch (Exception ex)
            {
                return "null";
            }
        }
        #endregion

        #region SFC 查询
        /// <summary>
        /// SFC选择模式
        /// </summary>
        public enum SFCMode
        {
            GetStationID,
            GetCardTagid
        }
        /// <summary>
        /// 连接PDCA
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        public bool Connect(TcpClient link)
        {
            //link.BeginAsynReceive(OnDataReceive);
            //m_tcpPDCA = link;

            if (link != null)
            {
                //if (link.IsOpen())
                //{
                //    link.Close();
                //    Thread.Sleep(50);
                //}

                link.Close();
                Thread.Sleep(50);
                return link.Open();
            }

            return false;
        }

        /// <summary>
        /// 断开PDCA连接
        /// </summary>
        public void Disconnect(TcpClient link)
        {
            if (link != null)
            {
                link.Close();
            }
        }
        /// <summary>
        /// 发送信息
        /// </summary>
        /// <param name="msg"></param>
        //public void SendMessage(string msg)
        //{
        //    if (m_tcpPDCA != null)
        //    {
        //        m_tcpPDCA.WriteString(msg);
        //    }
        //}

        public void SendMessage(TcpClient link, string msg)
        {
            if (link != null)
            {
                link.WriteString(msg);
            }
        }
        /// <summary>
        /// 判断PDCA回复是否成功
        /// </summary>
        /// <param name="reply"></param>
        /// <returns></returns>
        public bool IsSuccess(string reply)
        {
            string[] strReplyArray = reply.Split('\n');

            bool bSuccess = true;

            if (reply == "" || reply == null)
            {
                return false;
            }

            foreach (string str in strReplyArray)
            {
                string[] strSplits = str.Split('@');

                if (strSplits.Length > 0)
                {
                    if (strSplits[0].ToLower().Contains("bad") || strSplits[0].ToLower().Contains("err"))
                    {
                        bSuccess = false;
                        break;
                    }
                }
            }

            return bSuccess;
        }
        /// <summary>
        /// 解析SFC反馈
        /// </summary>
        /// <param name="strMsg"></param>
        /// <param name="regular"></param>
        /// <returns></returns>
        private string SFCResult(string strMsg, string regular)
        {
            Match match = Regex.Match(strMsg, regular);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                return "";
            }
        }

       



        #endregion

        #region 日志显示
        /// <summary>
        /// 显示上传日志
        /// </summary>
        public void ShowLog(string message, bool result, string returnResult, DataStyle dataStyle)
        {
            string formattedMessage = message;/* ConvertJsonString(message);*/
            if (dataStyle == DataStyle.OnboardingStatus)
            {
                formattedMessage = ConvertJsonString(message);
            }
            lock (lock_showlog)
            {
                SaveHiveLog(dataStyle, $"{DateTime.Now:HH:mm:ss.fff}\r\n ");
                SaveHiveLog(dataStyle, $"Send:\r\n");
                SaveHiveLog(dataStyle, formattedMessage + "\r\n");
                SaveHiveLog(dataStyle, "Receive:\r\n");
                SaveHiveLog(dataStyle, returnResult + "\r\n\r\n");
            }
        }
        #endregion

        #region 工具方法
        /// <summary>
        /// 验证pass参数值
        /// </summary>
        private bool ValidatePassValue(string pass)
        {
            bool bPass = false;
            bool.TryParse(pass, out bPass);
            return bPass;
        }

        /// <summary>
        /// 添加可选数据字段
        /// </summary>
        private static void AddOptionalData(Dictionary<string, object> dict, string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
                dict[key] = value;
        }

        ///// <summary>
        ///// 检查指定类型的窗体是否已打开
        ///// </summary>
        //public bool IsFormOpen<T>() where T : Form => Application.OpenForms.OfType<T>().Any();

        /// <summary>
        /// 检查是否为需要排除的窗体（HiveEx命名空间的Form_Login）
        /// </summary>
        private bool IsExcludedForm(Form form) =>
            form.GetType().Namespace == "AF.Upload.Hive" && form.GetType().Name == "Form_Login";

        /// <summary>
        /// 检查指定类型的窗体是否已打开（排除HiveEx命名空间的Form_Login窗体）
        /// </summary>
        public bool IsFormOpen<T>() where T : Form =>
            Application.OpenForms.OfType<T>()
                .Any(form => !IsExcludedForm(form));

        /// <summary>
        /// 检查当前命名空间内的窗体是否已打开（排除HiveEx命名空间的Form_Login窗体）
        /// </summary>
        public bool IsAnyFormOpenInNamespace(string namespaceName) =>
            Application.OpenForms.Cast<Form>()
                .Any(form => form.GetType().Namespace == namespaceName &&
                            form.Visible &&
                            !IsExcludedForm(form));

        /// <summary>
        /// 检查当前命名空间内的指定名称的窗体是否已打开（排除HiveEx命名空间的Form_Login窗体）
        /// </summary>
        /// <param name="namespaceName">命名空间名称</param>
        /// <param name="formName">窗体类型名称（不包含命名空间）</param>
        /// <returns>如果找到指定命名空间和名称的可见窗体则返回true，否则返回false</returns>
        public bool IsFormOpenInNamespace(string namespaceName, string formName) =>
            Application.OpenForms.Cast<Form>()
                .Any(form => form.GetType().Namespace == namespaceName &&
                            form.GetType().Name == formName &&
                            form.Visible &&
                            !IsExcludedForm(form));

        /// <summary>
        /// 检查当前命名空间内的指定名称的窗体是否已打开（使用完整类型名称，排除HiveEx命名空间的Form_Login窗体）
        /// </summary>
        /// <param name="fullTypeName">完整类型名称（包含命名空间）</param>
        /// <returns>如果找到指定完整类型名称的可见窗体则返回true，否则返回false</returns>
        public bool IsFormOpenByFullName(string fullTypeName) =>
            Application.OpenForms.Cast<Form>()
                .Any(form => form.GetType().FullName == fullTypeName &&
                            form.Visible &&
                            !IsExcludedForm(form));
        #endregion

        #region 数据定义类
        /// <summary>
        /// 机器数据定义
        /// </summary>
        public class MachineData
        {
            public string unit_sn { get; set; } = string.Empty;
            public Dictionary<string, object> serials { get; set; } = new Dictionary<string, object>();
            public bool pass { get; set; } = false;
            public string input_time { get; set; } = string.Empty;
            public string output_time { get; set; } = string.Empty;
            public Dictionary<string, object> data { get; set; } = new Dictionary<string, object>();
            public Heartbeat station_info { get; set; } = new Heartbeat();

            public void AddSerial(string name, object serial)
            {
                if (!serials.ContainsKey(name))
                    serials.Add(name, serial);
            }

            public void AddData(string name, object dataItem)
            {
                if (!data.ContainsKey(name))
                    data.Add(name, dataItem);
            }
        }

        #region   MachineData拓展类

        public class MachineDataParameters
        {
            // 必需字段
            public string UnitSn { get; set; }
            public bool Pass { get; set; }
            public DateTime InputTime { get; set; }
            public DateTime OutputTime { get; set; }
            public ProcessingMode Mode { get; set; } // 使用枚举类型

            // 可选字段
            public string CarrierSn { get; set; }
            public int? Quantity { get; set; }
            public string ErrorCode { get; set; }
            public int? Parallel { get; set; }


            // 集合字段 - 延迟初始化
            private Dictionary<string, object> _serials;
            public Dictionary<string, object> Serials
            {
                get => _serials ?? (_serials = new Dictionary<string, object>());
                set => _serials = value;
            }

            private Dictionary<string, object> _additionalData;
            public Dictionary<string, object> AdditionalData
            {
                get => _additionalData ?? (_additionalData = new Dictionary<string, object>());
                set => _additionalData = value;
            }

            public bool Validate()
            {
                if (string.IsNullOrWhiteSpace(UnitSn))
                    return false;

                if (InputTime >= OutputTime)
                    return false;

                if (!Pass && string.IsNullOrWhiteSpace(ErrorCode))
                    return false;

                return true;
            }
        }

        #endregion

        #region MachineState拓展类

        #endregion
        [Serializable]
        public class TRData
        {
            public string message { get; set; } = string.Empty;
            public string code { get; set; } = string.Empty;
            public string severity { get; set; } = string.Empty;

            public string occurrence_time { get; set; } = string.Empty;
            public string resolved_time { get; set; } = string.Empty;

            public Dictionary<string, object> data { get; set; } = new Dictionary<string, object>();
            public Heartbeat station_info { get; set; } = new Heartbeat();
        }
        #region TRData拓展类

        public class TRDataParameters
        {
            // 必需字段
            public string Message { get; set; }
            public string Code { get; set; }
            public string Severity { get; set; }
            public DateTime OccurrenceTime { get; set; }
            public DateTime ResolvedTime { get; set; }

            // 可选字段
            public int? Parallel { get; set; }
            /// <summary>
            /// 主要部件被丢弃，例如电池、主板。"keypart_sn":"名称&SN"。例如："HSG&GMJVWNM40"
            /// </summary>
            public string keypart_sn { get; set; }
            /// <summary>
            /// WIP SN 
            /// </summary>
            public string module_sn { get; set; }
            /// <summary>
            /// 次要部件被丢弃，
            /// </summary>
            public string smallpart_sn { get; set; }
            public string Series { get; set; }

            // 额外数据字段
            private Dictionary<string, object> _additionalData;
            public Dictionary<string, object> AdditionalData
            {
                get => _additionalData ?? (_additionalData = new Dictionary<string, object>());
                set => _additionalData = value;
            }

            public bool Validate()
            {
                if (string.IsNullOrWhiteSpace(Message))
                    return false;

                if (string.IsNullOrWhiteSpace(Code))
                    return false;

                if (string.IsNullOrWhiteSpace(Severity))
                    return false;

                if (OccurrenceTime >= ResolvedTime)
                    return false;

                return true;
            }
        }

        #endregion
        /// <summary>
        /// 站台软件版本信息
        /// </summary>
        public class StationSwVersion
        {
            public SW_Version main_software { get; set; } = new SW_Version();
            public SubModule sub_module { get; set; } = new SubModule();
            public Attributes attributes { get; set; } = new Attributes();
            public Heartbeat station_info { get; set; } = new Heartbeat();
        }

        #region 

        public class Attributes
        {
            public attributes x_offset { get; set; } = new attributes();
            public attributes z_offset { get; set; } = new attributes();

            private bool IsEmpty(attributes swVersion)
            {
                return swVersion != null &&
                       string.IsNullOrEmpty(swVersion.old) &&
                       string.IsNullOrEmpty(swVersion.NEW);
            }

            public bool ShouldSerializex_offset() => !IsEmpty(x_offset);
            public bool ShouldSerializez_offset() => !IsEmpty(z_offset);
        }
        public class attributes
        {
            //public class x_offset
            //{
            public string old { get; set; } = string.Empty;
            public string NEW { get; set; } = string.Empty;

        }
        public class z_offset
        {
            public string old { get; set; } = string.Empty;
            public string NEW { get; set; } = string.Empty;
        }

    }

    public class SW_Version
    {
        public string version { get; set; } = string.Empty;
        public string hash_key { get; set; } = string.Empty;
    }

    public class SubModule
    {
        public SW_Version repair_sop { get; set; } = new SW_Version();
        public SW_Version spare_part_list { get; set; } = new SW_Version();
        public SW_Version vision_software { get; set; } = new SW_Version();
        public SW_Version laser_software { get; set; } = new SW_Version();
        public SW_Version robot_software { get; set; } = new SW_Version();
        public SW_Version ide_software { get; set; } = new SW_Version();
        public SW_Version config_file { get; set; } = new SW_Version();
        public SW_Version vision_template { get; set; } = new SW_Version();


        private bool IsEmptySWVersion(SW_Version swVersion)
        {
            return swVersion != null &&
                   string.IsNullOrEmpty(swVersion.version) &&
                   string.IsNullOrEmpty(swVersion.hash_key);
        }

        public bool ShouldSerializerepair_sop() => !IsEmptySWVersion(repair_sop);
        public bool ShouldSerializespare_part_list() => !IsEmptySWVersion(spare_part_list);
        public bool ShouldSerializevision_software() => !IsEmptySWVersion(vision_software);
        public bool ShouldSerializelaser_software() => !IsEmptySWVersion(laser_software);
        public bool ShouldSerializerobot_software() => !IsEmptySWVersion(robot_software);
        public bool ShouldSerializeide_software() => !IsEmptySWVersion(ide_software);
        public bool ShouldSerializeconfig_file() => !IsEmptySWVersion(config_file);
        public bool ShouldSerializevision_template() => !IsEmptySWVersion(vision_template);
    }
    #region StationIdentity拓展类

    public class StationSwVersionParameters
    {
        // 主软件信息
        public VersionInfo MainSoftware { get; set; } = new VersionInfo();

        // 子模块信息
        public VersionInfo RepairSop { get; set; } = new VersionInfo();
        public VersionInfo SparePartList { get; set; } = new VersionInfo();
        public VersionInfo VisionSoftware { get; set; } = new VersionInfo();
        public VersionInfo LaserSoftware { get; set; } = new VersionInfo();
        public VersionInfo RobotSoftware { get; set; } = new VersionInfo();
        public VersionInfo IdeSoftware { get; set; } = new VersionInfo();
        public VersionInfo ConfigFile { get; set; } = new VersionInfo();
        public VersionInfo VisionTemplate { get; set; } = new VersionInfo();
        public VersionInfo XOffset { get; set; } = new VersionInfo();
        public VersionInfo ZOffset { get; set; } = new VersionInfo();

        public bool Validate()
        {
            return !string.IsNullOrWhiteSpace(MainSoftware.version) &&
                   !string.IsNullOrWhiteSpace(RepairSop.version) &&
                   !string.IsNullOrWhiteSpace(VisionSoftware.version) &&
                   !string.IsNullOrWhiteSpace(XOffset.old) &&
                   !string.IsNullOrWhiteSpace(ZOffset.old);


        }
    }

    #endregion
    #endregion

    /// <summary>
    /// 心跳数据
    /// </summary>
    public class Heartbeat
    {
        public string machine_sn { get; set; } = string.Empty;
        public string machine_model_number { get; set; } = string.Empty;
        public string machine_rfid { get; set; } = string.Empty;
        public string vendor { get; set; } = string.Empty;
        public string site { get; set; } = string.Empty;
        public string building { get; set; } = string.Empty;

        public string product { get; set; } = string.Empty;
        public string station_type { get; set; } = string.Empty;
        public string line { get; set; } = string.Empty;
        public int instance { get; set; } = 0;
        public string line_type { get; set; } = string.Empty;
        public string station_name { get; set; } = string.Empty;
        public string build_type { get; set; } = string.Empty;
    }
    #endregion


    #region 扩展类
    /// <summary>
    /// Hive管理扩展方法
    /// </summary>
    public static class HiveMgrExtensions
    {
        /// <summary>
        /// 从文件读取JSON内容
        /// </summary>
        public static string ReadJsonContentFromFile(this string filePath)
        {
            try { return System.IO.File.ReadAllText(filePath); } catch { return string.Empty; }
        }

        /// <summary>
        /// JSON字符串转换为Hive服务器响应模型
        /// </summary>
        public static HiveServerModel GetHiveServerModel(this string json)
        {
            try { return JsonConvert.DeserializeObject<HiveServerModel>(json); } catch { return null; }
        }

        /// <summary>
        /// JSON字符串转换为Hive服务器响应模型
        /// </summary>
        public static HiveServerRegisterModel GetHiveServerModelRegister(this string json)
        {
            try { return JsonConvert.DeserializeObject<HiveServerRegisterModel>(json); } catch { return null; }
        }
        /// <summary>
        /// 检查响应是否成功
        /// </summary>
        public static bool IsSuccess(this HiveServerModel response, DataStyle dataStyle)
        {
            if (response == null) return false;

            // 心跳接口有特殊的成功状态
            if (dataStyle == DataStyle.Heartbeat)
            {
                return response.ErrorCode == null &&
                       (response.msg?.ToString().ToLower() == "heartbeat received" || response.msg.ToString() == "请求处理成功");
            }
            if (dataStyle == DataStyle.OnboardingStatus)
            {
                string statusString = response.onboarding_status?.ToString().ToUpper() ?? "";
                return statusString.Contains("ONBOARDED");
            }
            // 其他接口使用通用的成功判断
            string status = response.msg?.ToString().ToLower() ?? "";
            return response.ErrorCode == null &&
                   (status.Contains("success") || (status.Contains("请求处理成功")));
        }

        /// <summary>
        /// 检查响应是否成功
        /// </summary>
        public static bool IsRegisterSuccess(this HiveServerRegisterModel response, DataStyle dataStyle)
        {
            if (response == null) return false;


            string statusString = response.onboarding_status?.ToString().ToUpper() ?? "";
            return statusString.Contains("ONBOARDED");

        }
        /// <summary>
        /// 获取错误信息
        /// </summary>
        public static string GetErrorMessage(this HiveServerModel response)
        {
            if (response == null) return "响应为空";
            string errorCode = response.ErrorCode?.ToString() ?? "";
            string errorText = response.ErrorText?.ToString() ?? "";

            if (string.IsNullOrEmpty(errorCode) && string.IsNullOrEmpty(errorText))
                return response.Status?.ToString() ?? "未知状态";

            return $"错误代码: {errorCode}, 错误信息: {errorText}";
        }
    }

    #endregion



    #region 扩展辅助类
    /// <summary>
    /// hive 服务器返回信息的注册model
    /// </summary>
    [Serializable]
    public class HiveServerRegisterModel
    {
        //  ErrorCode
        //      ErrorText
        //      ErrorValidation
        //pu      Status
        /// <summary>
        /// 错误编码
        /// </summary>
        public Object is_active { get; set; }
        /// <summary>
        /// 错误信息
        /// </summary>
        public Object machine_state { get; set; }
        /// <summary>
        /// 错误校验明细
        /// </summary>
        public object state_change_time { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public Object onboarding_status { get; set; }
        /// <summary>
        /// 数据内容
        /// </summary>
        public object station_id { get; set; }
    }

    /// <summary>
    /// hive 服务器返回信息的model
    /// </summary>
    [Serializable]
    public class HiveServerModel
    {
        //  ErrorCode
        //      ErrorText
        //      ErrorValidation
        //pu      Status
        /// <summary>
        /// 错误编码
        /// </summary>
        public Object ErrorCode { get; set; }
        /// <summary>
        /// 错误信息
        /// </summary>
        public Object ErrorText { get; set; }
        /// <summary>
        /// 错误校验明细
        /// </summary>
        public object ErrorValidation { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public Object Status { get; set; }
        /// <summary>
        /// 数据内容
        /// </summary>
        public object Data { get; set; }

        //  ErrorCode
        //      ErrorText
        //      ErrorValidation
        //pu      Status
        /// <summary>
        /// 错误编码
        /// </summary>
        public Object is_active { get; set; }
        /// <summary>
        /// 错误信息
        /// </summary>
        public Object machine_state { get; set; }
        /// <summary>
        /// 错误校验明细
        /// </summary>
        public object state_change_time { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public Object onboarding_status { get; set; }
        /// <summary>
        /// 数据内容
        /// </summary>
        public object station_id { get; set; }

        public Object msg { get; set; }
    }
    // 扩展方法辅助类
    public static class ExtensionMethods
    {
        public static void IfNotEmpty(this string value, Action<string> action)
        {
            if (!string.IsNullOrWhiteSpace(value)) action(value);
        }

        public static void IfNotNull<T>(this T? value, Action<T> action) where T : struct
        {
            if (value.HasValue) action(value.Value);
        }

        public static string TrimToNull(this string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
    #endregion 扩展辅助类
}

