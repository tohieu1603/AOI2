using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AF.Upload.Hive
{

    public partial class Form_SafetyStatusMonitor : HiveBaseForm
    {
        // 单例实例
        private static Form_SafetyStatusMonitor _instance;

        // 窗体打开状态
        private static bool _isOpened = false;

        // 线程锁用于确保线程安全
        private static readonly object _lockObject = new object();

        // 静态安全设备配置列表
        public static List<SafetyDeviceConfig> _staticSafetyDevices = new List<SafetyDeviceConfig>();
        //{
        //    new SafetyDeviceConfig("上层安全门", SafetyDeviceType.SafetyDoor, "上层安全⻔打开", "Front safety door opened", "SafetyDoor", true),
        //    new SafetyDeviceConfig("下层安全门", SafetyDeviceType.SafetyDoor, "下层安全⻔打开", "Back safety door opened", "SafetyDoor", true),
        //    new SafetyDeviceConfig("LeftSafetyDoor", SafetyDeviceType.SafetyDoor, "左⾯安全⻔打开", "Left safety door opened", "SafetyDoor", true),
        //    new SafetyDeviceConfig("RightSafetyDoor", SafetyDeviceType.SafetyDoor, "右⾯安全⻔打开", "Right safety door opened", "SafetyDoor", true),
        //    new SafetyDeviceConfig("EStop", SafetyDeviceType.EmergencyStop, "急停按下", "Emergency stop", "EStop", true),
        //    new SafetyDeviceConfig("SafetyLight", SafetyDeviceType.LightCurtain, "安全光栅", "Safety light curtain", "LightCurtain", true),
        //     可以继续添加更多设备
        //     new SafetyDeviceConfig("SafetyLight1", SafetyDeviceType.LightCurtain, "安全光栅1", "Safety light curtain1", "LightCurtain"),
        //     new SafetyDeviceConfig("SafetyLight2", SafetyDeviceType.LightCurtain, "安全光栅2", "Safety light curtain2", "LightCurtain"),
        //     new SafetyDeviceConfig("SafetyLight3", SafetyDeviceType.LightCurtain, "安全光栅3", "Safety light curtain3", "LightCurtain"),
        //     new SafetyDeviceConfig("SafetyLight4", SafetyDeviceType.LightCurtain, "安全光栅4", "Safety light curtain4", "LightCurtain")
        //};

        private Timer statusUpdateTimer;
        private List<SafetyDeviceConfig> safetyDevices;
        private Dictionary<string, StatusIndicatorLabel> deviceLabels;


        private Form_SafetyStatusMonitor()
        {
            InitializeSafetyDevices();
            InitializeComponent();
            StartStatusUpdateTimer();

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            //// 获取屏幕分辨率
            Rectangle screen = Screen.GetWorkingArea(this);
            int width = screen.Width / 3 * 1;
            int height = screen.Height / 4 * 1;

            //// 设置窗体客户区大小
            this.ClientSize = new Size(width, height);
            //this.CenterToScreen();

            this.Text = "还原到生产模式失败,请先清掉故障,Failed to resume production,please clear the error first.";
        }
        // 单例实例访问属性
        public static Form_SafetyStatusMonitor Instance
        {
            get
            {
                lock (_lockObject)
                {
                    if (_instance == null || _instance.IsDisposed)
                    {
                        _instance = new Form_SafetyStatusMonitor();
                    }
                    return _instance;
                }
            }
        }
        // 窗体打开状态属性
        public static bool IsOpened
        {
            get { return _isOpened; }
            private set { _isOpened = value; }
        }
        // 显示窗体的新方法，确保单例模式
        public new void Show()
        {
            if (!IsOpened)
            {
                base.Show();
                IsOpened = true;
            }
        }
        // 显示对话框的新方法，确保单例模式
        public new DialogResult ShowDialog()
        {
            if (!IsOpened)
            {
                IsOpened = true;
                return base.ShowDialog();
            }
            return DialogResult.Cancel; // 如果已经打开，返回Cancel
        }
        // 显示对话框并指定父窗体
        public new DialogResult ShowDialog(IWin32Window owner)
        {
            if (!IsOpened)
            {
                IsOpened = true;
                return base.ShowDialog(owner);
            }
            return DialogResult.Cancel; // 如果已经打开，返回Cancel
        }
        // 检测所有安全设备IO状态是否都是安全状态
        public static bool CheckSafetyStatus()
        {
            try
            {
                foreach (var device in _staticSafetyDevices)
                {
                    if (device.IsInPut) continue;

                    if (device.DeviceType == SafetyDeviceType.SafetyDoor)
                    {
                        device?.Set(true);
                    }
                }


                // 检查所有设备的状态
                foreach (var device in _staticSafetyDevices)
                {
                    if (!device.IsInPut) continue;

                    bool ioState = device?.Get() ?? false;
                    // 如果任何一个设备不在安全状态，返回false
                    if (!ioState)
                    {
                        return false;
                    }
                }

                // 所有设备都在安全状态
                return true;
            }
            catch (Exception ex)
            {
                // 如果读取IO出现异常，认为不安全
                Console.WriteLine($"检查安全状态时出错: {ex.Message}");
                return false;
            }
        }
        // 获取当前不安全设备的列表
        public static List<string> GetUnsafeDevices()
        {
            var unsafeDevices = new List<string>();

            try
            {
                foreach (var device in _staticSafetyDevices)
                {
                    if (!device.IsInPut) continue;

                    bool ioState = device?.Get() ?? false;
                    if (!ioState)
                    {
                        unsafeDevices.Add(device.ChineseText);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取不安全设备列表时出错: {ex.Message}");
            }

            return unsafeDevices;
        }
        // 获取安全设备配置的静态方法（可选，用于外部访问）
        public static List<SafetyDeviceConfig> GetSafetyDeviceConfigs()
        {
            return new List<SafetyDeviceConfig>(_staticSafetyDevices);
        }

        public void SetErrorInfo(string errorCode, string errorMessage)
        {
            if (TextBox_Errorcode != null && !TextBox_Errorcode.IsDisposed)
                TextBox_Errorcode.Text = errorCode;
            if (TextBox_Errormessage != null && !TextBox_Errormessage.IsDisposed)
                TextBox_Errormessage.Text = errorMessage;
        }

        // 初始化安全设备配置
        private void InitializeSafetyDevices()
        {
            // 使用静态安全设备列表的副本，避免外部修改影响内部状态
            safetyDevices = new List<SafetyDeviceConfig>(_staticSafetyDevices);
        }

        // 根据图片名称获取图片
        private Image GetImageByName(string imageName)
        {
            if (string.IsNullOrEmpty(imageName))
                return null;

            try
            {
                var properties = typeof(Properties.Resources).GetProperties(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                var property = properties.FirstOrDefault(p => p.Name == imageName);
                return property?.GetValue(null) as Image;
            }
            catch
            {
                return null;
            }
        }

        // 动态创建控件布局
        private void CreateDynamicLayout()
        {
            this.BackColor = HiveUIColors.Red;
            panelMain.BackColor = HiveUIColors.Red;
            tableLayoutPanel1.BackColor = HiveUIColors.Red;
            button_OK.BackColor = HiveUIColors.Gray;
            TextBox_Errorcode.BackColor = HiveUIColors.Gray;
            TextBox_Errormessage.BackColor = HiveUIColors.Gray;

            deviceLabels = new Dictionary<string, StatusIndicatorLabel>();

            // 分组设备
            var safetyDoors = safetyDevices.Where(d => d.DeviceType == SafetyDeviceType.SafetyDoor&&d.IsInPut).ToList();
            var eStops = safetyDevices.Where(d => d.DeviceType == SafetyDeviceType.EmergencyStop && d.IsInPut).ToList();
            var lightCurtains = safetyDevices.Where(d => d.DeviceType == SafetyDeviceType.LightCurtain && d.IsInPut).ToList();

            // 计算需要的行数
            int leftColumnRows = safetyDoors.Count;
            int rightColumnRows = Math.Max(eStops.Count + lightCurtains.Count, leftColumnRows);
            int totalRows = Math.Max(leftColumnRows, rightColumnRows);

            // 配置 TableLayoutPanel 的行和列
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Clear();
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            tableLayoutPanel1.RowCount = totalRows;
            tableLayoutPanel1.RowStyles.Clear();
            for (int i = 0; i < totalRows; i++)
            {
                // 使用AutoSize让行自动调整高度，并设置最小高度
                tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }

            // 清空现有控件
            tableLayoutPanel1.Controls.Clear();

            // 添加安全门到左列
            for (int i = 0; i < safetyDoors.Count; i++)
            {
                var device = safetyDoors[i];
                var label = CreateStatusLabel(device);
                tableLayoutPanel1.Controls.Add(label, 0, i);
                deviceLabels.Add(device.DeviceId, label);
            }

            // 添加急停到右列第一行
            if (eStops.Count > 0)
            {
                var eStop = eStops[0];
                var label = CreateStatusLabel(eStop);
                tableLayoutPanel1.Controls.Add(label, 1, 0);
                deviceLabels.Add(eStop.DeviceId, label);
            }

            // 添加安全光栅到右列，接在急停后面
            for (int i = 0; i < lightCurtains.Count; i++)
            {
                var device = lightCurtains[i];
                var label = CreateStatusLabel(device);
                tableLayoutPanel1.Controls.Add(label, 1, eStops.Count + i);
                deviceLabels.Add(device.DeviceId, label);
            }

            tableLayoutPanel1.PerformLayout();
        }

        private StatusIndicatorLabel CreateStatusLabel(SafetyDeviceConfig device)
        {
            var label = new StatusIndicatorLabel
            {
                Dock = DockStyle.Fill,
                BackColor = HiveUIColors.Red,
                Line1 = device.ChineseText,
                Line2 = device.EnglishText,
                LeftImage = GetImageByName(device.ImageName),
                IsNormalStatus = false, // 默认显示异常状态
                Tag = device,// 存储设备配置信息
                MinimumSize = new Size(0, 10), // 设置最小高度为50像素
                MaximumSize = new Size(0, 60) // 设置最大高度为60像素
            };
            return label;
        }

        private void StartStatusUpdateTimer()
        {
            statusUpdateTimer = new Timer();
            statusUpdateTimer.Interval = 1000; // 1秒更新一次
            statusUpdateTimer.Tick += StatusUpdateTimer_Tick;
            statusUpdateTimer.Start();
        }

        private void StatusUpdateTimer_Tick(object sender, EventArgs e)
        {
            UpdateStatusDisplay();
        }

        private void UpdateStatusDisplay()
        {
            foreach (var device in safetyDevices)
            {
                if (!device.IsInPut) continue;
                if (deviceLabels.ContainsKey(device.DeviceId))
                {
                    bool ioState = device?.Get() ?? false;
                    deviceLabels[device.DeviceId].IsNormalStatus = ioState;
                }
            }
        }

        private bool IsSafetyDevicesClear()
        {
            // 检查所有安全设备是否都正常
            return deviceLabels?.Values.All(label => !label.IsNormalStatus) ?? false;
        }

        private void ShowSafetyWarningDialog()
        {
            var abnormalDevices = safetyDevices.Where(device =>
                deviceLabels.ContainsKey(device.DeviceId) && deviceLabels[device.DeviceId].IsNormalStatus);

            if (!abnormalDevices.Any()) return;

            string warningMessage = "无法恢复生产！以下安全设备异常：\n\n";
            foreach (var device in abnormalDevices)
            {
                warningMessage += $"• {device.ChineseText}\n";
            }
            warningMessage += "\n请检查并解决所有安全问题后再试。";

            MessageBox.Show(warningMessage, "安全警告",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void Form_SafetyStatusMonitor_FormClosing(object sender, FormClosingEventArgs e)
        {
            statusUpdateTimer?.Stop();
            statusUpdateTimer?.Dispose();
            IsOpened = false; // 重置打开状态
        }

        private void Form_SafetyStatusMonitor_FormClosed(object sender, FormClosedEventArgs e)
        {
            IsOpened = false; // 确保窗体关闭后状态被重置
        }
        private void Form_SafetyStatusMonitor_Load(object sender, EventArgs e)
        {
            CreateDynamicLayout();
            this.TopMost = true;
            this.TopLevel = true;
            TextBox_Errorcode.TabStop = false;
            TextBox_Errormessage.TabStop = false;
            button_OK.Select();
            buttonPanel.Layout += (s, args) =>
            {
                button_OK.Location = new Point(
                    (buttonPanel.Width - button_OK.Width) / 2,
                    (buttonPanel.Height - button_OK.Height) / 2
                );
            };
        }

        public void Form_Closing()
        {
            this.Close();
        }
        private void button_OK_Click(object sender, EventArgs e)
        {
            if (IsSafetyDevicesClear())
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                //ShowSafetyWarningDialog();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }


    }
    // 安全设备配置类
    public class SafetyDeviceConfig
    {
        /// <summary>
        /// IO名字
        /// </summary>
        public string DeviceId { get; set; }
        /// <summary>
        /// IO类型
        /// </summary>
        public SafetyDeviceType DeviceType { get; set; }
        public string ChineseText { get; set; }
        public string EnglishText { get; set; }
        public string ImageName { get; set; }
        //public bool IsInverted { get; set; } // 是否取反显示
        public bool IsInPut { get; set; } // 是否是输入信号

        public delegate bool GetHandle();
        /// <summary>
        /// 读DI
        /// </summary>
        public GetHandle Get = null;

        public delegate void SetHandle(bool b);
        /// <summary>
        /// 写DO
        /// </summary>
        public SetHandle Set = null;

        public SafetyDeviceConfig(SafetyDeviceType type, string chinese, string english, bool isInPut = true)
        {
            DeviceId = chinese;
            DeviceType = type;
            ChineseText = chinese;
            EnglishText = english;
            IsInPut = isInPut;

            switch (type)
            {
                case SafetyDeviceType.SafetyDoor:
                    ImageName = "SafetyDoor";
                    break;
                case SafetyDeviceType.EmergencyStop:
                    ImageName = "EStop";
                    break;
                case SafetyDeviceType.LightCurtain:
                    ImageName = "LightCurtain";
                    break;
                default:
                    break;
            }
        }
    }

    // 安全设备类型枚举
    public enum SafetyDeviceType
    {
        SafetyDoor,
        EmergencyStop,
        LightCurtain
    }

}