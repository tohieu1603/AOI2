using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AF.File;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static AF.Upload.Hive.HiveTool;

namespace AF.Upload.Hive
{

    public partial class Form_Operation : HiveBaseForm
    {
        public static bool thisshowed = false;
        bool Textbadgechange = false;
        public DateTime HeipRequestTimeStart = DateTime.Now;//请求帮助开始时间
        public DateTime MaintenanceTimeStart = DateTime.Now;//维护开始时间


        Dictionary<RadioButton, string> RadioButtonErrorCode = new Dictionary<RadioButton, string>();

        List<RadioButton> RadioButtons = new List<RadioButton>();
        ConcurrentDictionary<string, bool> _bakState = new ConcurrentDictionary<string, bool>();
        RadioButton radioButton_select;
        TimeSpan secondsLeft = new TimeSpan(0, 0, 120);
        DateTime OpenTime = DateTime.Now;
        bool bRestart = false;
        string Parameter = "stop";
        internal event closeForm closeForm_resume;

        public static int nRemainingClicks = 0;
        public static string sLastErrorCode = "";
        public static DateTime dtLastClickTime = DateTime.Now;
        public static string sCheckedButton = "";

        public Form_Operation(bool restart,string parameter)
        {
            InitializeComponent();
            this.Load += new EventHandler(Form_Load);
            //主程序 = stream;

            thisshowed = true;
            this.bRestart = restart;
            this.Parameter = parameter;

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            // 获取屏幕分辨率
            Rectangle screen = Screen.GetWorkingArea(this);
            int width = screen.Width / 3;
            int height = screen.Height / 3 * 2;

            // 设置窗体客户区大小
            this.ClientSize = new Size(width, height);
            this.CenterToScreen();

        }
        private void Form_Load(object sender, EventArgs e)
        {
            this.Activate();
            this.TopMost = true;
            Ini.WriteValue(HiveCommandProcessor.m_strIniFile, "LocalData", "LastPOP", "Form_Operation");

            UpdatePromptText();

            this.button1.BackColor = HiveUIColors.Blue;//蓝
            this.label2.ForeColor = HiveUIColors.Red;
            this.label3.ForeColor = HiveUIColors.Blue;
            this.label4.ForeColor = HiveUIColors.Green;
            this.TextBox_badgeTip.BackColor = HiveUIColors.Gray;

            panel1.BackColor = HiveUIColors.Gray;
            tableLayoutPanel_button.BackColor = Color.White;

            timer1.Enabled = true;
            secondsLeft = new TimeSpan(0, 0, 120);
            HeipRequestTimeStart = DateTime.Now;
            Ini.WriteValue(HiveCommandProcessor.m_strIniFile, "Form_warning", "HeipRequestTimeStart", HeipRequestTimeStart.ToString());


            for (int i = 1; i < 12; i++)
            {
                RadioButton newButtons = (RadioButton)this.Controls.Find($"radioButton{i}", true).FirstOrDefault();
                _bakState.AddOrUpdate(newButtons.Name, newButtons.Checked, (k, v) => newButtons.Checked);
                if ((newButtons != null))
                    RadioButtons.Add(newButtons);
                newButtons.TabStop = false;
                //newButtons.MouseClick += new System.Windows.Forms.MouseEventHandler(this.radioButton_CheckedChanged);
                newButtons.Click += radioButton1_Click;

                //newButtons.CheckedChanged += radioButton_CheckedChanged;

            }
            //while (RadioButtons.Where(x=>x.Checked).Count()> 0)
            //{
            //    foreach (var item in RadioButtons.Where(x => x.Checked).ToList())
            //    {
            //        item.Checked = false;
            //    }
            //    //RadioButton newButtons = (RadioButton)this.Controls.Find($"radioButton1", true).FirstOrDefault();
            //    //newButtons.PerformClick();
            //}
            RadioButtonErrorCode.Clear();
            AddRadioButtonErrorCode();

            DateTime WeekMaintenanceTime = Convert.ToDateTime(Ini.GetStringValue(HiveCommandProcessor.m_strMaintenanceIniFile, "Weekly", "LastMaintenanceDate", DateTime.Now.ToString()));
            DateTime MonthMaintenanceTime = Convert.ToDateTime(Ini.GetStringValue(HiveCommandProcessor.m_strMaintenanceIniFile, "Monthly", "LastMaintenanceDate", DateTime.Now.ToString()));

            int LastWeeklyMaintenanceTime = (int)(7 - (DateTime.Now - WeekMaintenanceTime).TotalDays)/* > 0 ? (int)(7 - (DateTime.Now - WeekMaintenanceTime).TotalDays) : 0*/;
            int LastMonthlyMaintenceTime = (int)(31 - (DateTime.Now - MonthMaintenanceTime).TotalDays) /*> 0 ? (int)(31 - (DateTime.Now - MonthMaintenanceTime).TotalDays) : 0*/;

            if (LastWeeklyMaintenanceTime > 0)
            {
                label1_WeeklyMaintenanceTime.ForeColor = HiveUIColors.Green;
            }
            else
            {
                label1_WeeklyMaintenanceTime.ForeColor = HiveUIColors.Red;
            }
            if (LastMonthlyMaintenceTime > 0)
            {
                label1_MonthlyMaintenceTime.ForeColor = HiveUIColors.Green;
            }
            else
            {
                label1_MonthlyMaintenceTime.ForeColor = HiveUIColors.Red;
            }

            label1_WeeklyMaintenanceTime.Text = LastWeeklyMaintenanceTime.ToString();
            label1_MonthlyMaintenceTime.Text = LastMonthlyMaintenceTime.ToString();

            TextBox_badgeTip.ReadOnly = true;
            TextBox_badge.ReadOnly = false;
            //TextBox_刷卡.ReadOnly = true;
            TextBox_badge.BackColor = HiveUIColors.Gray;//灰色 R = 214 ; G = 214 ; B = 214

            TextBox_badge.Focus();
            this.ActiveControl = TextBox_badge;

            if (bRestart)
            {
                tableLayoutPanel_button.Visible = false;
                timer1.Enabled = false;
            }




            if (!bRestart)
            {
                HiveMgr.MachinePause?.Invoke();

                HiveTool.Event_id = DateTime.Now.ToString(HiveTool.TimeFormat);

                var stateParams = new MachineStateParameters
                {
                    MachineState = MachineStateData.MachineState.UnplannedDown,
                    error_message = "Machine stopped through UI",
                    code = "F99OOOO-20",
                    status = "help request",
                    event_id = HiveTool.Event_id,
                };

                Ini.WriteValue(HiveCommandProcessor.m_strIniFile, "LocalData", "ErrorCode", stateParams.code);
                Ini.WriteValue(HiveCommandProcessor.m_strIniFile, "LocalData", "Status", stateParams.status);
                Ini.WriteValue(HiveCommandProcessor.m_strIniFile, "LocalData", "Event_id", stateParams.event_id);

                HiveCommandProcessor.Instance.ProcessCommand(new List<string> { "Machinestate" }, stateParams);


                if (sLastErrorCode == stateParams.error_message && (DateTime.Now - dtLastClickTime).TotalMinutes < 30)
                {
                    nRemainingClicks++;
                }
                else
                {
                    sLastErrorCode = stateParams.error_message;
                    nRemainingClicks = 0;
                }
                dtLastClickTime = DateTime.Now;
                if (nRemainingClicks >= 3)
                {
                    tableLayoutPanel_button.Visible = false;
                }
            }




            //不允许操作主程序按钮(Start,Pause,Stop)
            HiveCommandProcessor.Instance.SetMainProgramButtonsEnabled(false);

            label1_retainClicks.Text = $"{3 - nRemainingClicks}";

        }

        private void AddRadioButtonErrorCode()
        {
            RadioButtonErrorCode.Add(radioButton1, "F99OOOO-01");
            RadioButtonErrorCode.Add(radioButton2, "F99OOOO-02");
            RadioButtonErrorCode.Add(radioButton3, "F99OOOO-03");
            RadioButtonErrorCode.Add(radioButton4, "F99OOOO-04");
            RadioButtonErrorCode.Add(radioButton5, "F99OOOO-11");
            RadioButtonErrorCode.Add(radioButton6, "F99OOOO-07");
            RadioButtonErrorCode.Add(radioButton7, "F99OOOO-05");
            RadioButtonErrorCode.Add(radioButton8, "F99OOOO-06");
            RadioButtonErrorCode.Add(radioButton9, "F99OOOO-09");
            RadioButtonErrorCode.Add(radioButton10, "F99OOOO-08");
            RadioButtonErrorCode.Add(radioButton11, "F99OOOO-10");
        }

        /// <summary>
        /// 获取当前选中的radiobutton
        /// </summary>
        /// <returns></returns>
        private RadioButton GetCheckedButton()
        {
            List<RadioButton> lists = RadioButtons.Where(n => n.Checked).ToList();
            return lists.FirstOrDefault();
        }

        /// <summary>
        /// 把内容里的中文去掉
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        static string RemoveBetweenFirstAndLastChinese(string input)
        {

            // 使用正则表达式找到第一个和最后一个中文字符的位置
            Match firstChineseMatch = Regex.Match(input, @"[\u4e00-\u9fa5]");
            Match lastChineseMatch = Regex.Match(input, @"[\u4e00-\u9fa5]", RegexOptions.RightToLeft);

            if (!firstChineseMatch.Success || !lastChineseMatch.Success)
            {
                return input; // 如果没有找到中文字符，返回原字符串
            }

            // 获取第一个和最后一个中文字符的索引位置
            int firstIndex = firstChineseMatch.Index;
            int lastIndex = lastChineseMatch.Index + 1; // 加1是因为索引是字符开始的位置

            // 切割并返回结果字符串，不包括第一个和最后一个中文字符以及它们之间的内容
            return input.Substring(0, firstIndex) + input.Substring(lastIndex);

        }

        private void radioButton_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton clickedButton = (RadioButton)sender; //获取按钮信息    
            //_bakState[clickedButton.Name] = clickedButton.Checked;
        }

        private void radioButton1_Click(object sender, EventArgs e)
        {
            RadioButton button = (RadioButton)sender;
            foreach (var item in RadioButtons)
            {
                if (item.Name != button.Name)
                {
                    item.Checked = _bakState[item.Name] = false;
                }
            }
            if (button.Checked && _bakState[button.Name])
            {
                button.Checked = false;
                _bakState[button.Name] = false;
            }
            else
                _bakState[button.Name] = true;

            // 新增：更新提示文本
            UpdatePromptText();
        }
        public bool badge_confirm()
        {
            List<RadioButton> lists = RadioButtons.Where(n => n.Checked).ToList();
            if (lists.Count > 0)//选择一样再刷卡
            {
                radioButton_select = lists[0];
                return true;
            }
            return false;
        }
        private async void TextBox_badge_TextChanged(object sender, EventArgs e)
        {
            if (Textbadgechange)
            {
                return;
            }
            if (TextBox_badge.Text.EndsWith("\r\n"))
            {
                string badge = TextBox_badge.Text.Trim('\r', '\n');
                HiveUserInfo hiveUserInfo = new HiveUserInfo() { CardTagid = badge };
                TextBox_badge.Text = "";
                if (badge_confirm())
                {
                    Textbadgechange = true;
                    MaintenanceTimeStart = DateTime.Now;

                    Ini.WriteValue(HiveCommandProcessor.m_strIniFile, "Form_warning", "MaintenanceTimeStart", MaintenanceTimeStart.ToString());

                    bCommError = false;
                    for (int i = 0; i < 5; i++)
                    {
                        if (badge == "123456")
                        {
                            hiveUserInfo.Level = "L1";
                            break;
                        }
                        else if (badge == "234567")
                        {
                            hiveUserInfo.Level = "L2";
                            break;
                        }
                        else if (badge == "345678")
                        {
                            hiveUserInfo.Level = "L3";
                            break;
                        }

                        if (bCommError)
                        {
                            break;
                        }
                        if (HiveMgr.GetHiveUserInfo?.Invoke(ref hiveUserInfo) ?? false)
                        {
                            TextBox_badgeTip.Text = $"badge info send:{hiveUserInfo.strSend}\r\n, MES info received:{hiveUserInfo.strReceive}\r\n. You may start repair.";
                            TextBox_badge.BackColor = HiveUIColors.Green;//绿
                            bCommError = false;
                            break;
                        }
                        bCommError = true;
                        TextBox_badgeTip.Text = $"badge info send:{hiveUserInfo.strSend}\r\n, MES info received:{hiveUserInfo.strReceive}\r\n.Access denied.";
                        TextBox_badge.BackColor = HiveUIColors.Red;//红
                    }

                    if (bCommError)
                    {
                        Textbadgechange = false;
                        return;
                    }


                    // 新增：权限检查
                    string stateCategory = GetCurrentStateCategory();
                    string badgeLevel = hiveUserInfo.Level; // 假设这是从MES获取的工卡等级
                    if (!CheckBadgeAccess(badgeLevel, stateCategory + "_1st")) // 一级弹窗权限检查
                    {
                        //TextBox_badgeTip.Text = "Access denied: Insufficient badge level";
                        //TextBox_badge.BackColor = HiveUIColors.Red;
                        Textbadgechange = false;
                        return;
                    }



                    var stateParams = new MachineStateParameters
                    {
                        MachineState = MachineStateData.MachineState.UnplannedDown,
                        status = "repair start",
                        event_id = HiveTool.Event_id,
                        badge = hiveUserInfo.Level,
                    };

                    if (GetCheckedButton() == radioButton11)
                    {
                        stateParams.code = RadioButtonErrorCode[radioButton11];
                        stateParams.error_message = "Degraded mode";
                    }
                    else
                    {
                        stateParams.code = RadioButtonErrorCode[GetCheckedButton()];
                        stateParams.error_message = RemoveBetweenFirstAndLastChinese(GetCheckedButton().Text);
                    }

                    sCheckedButton = stateParams.error_message;

                    Ini.WriteValue(HiveCommandProcessor.m_strIniFile, "LocalData", "Status", stateParams.status);

                    HiveCommandProcessor.Instance.ProcessCommand(new List<string> { "Machinestate" }, stateParams);

                    int index = RadioButtons.IndexOf(radioButton_select); // 返回2
                    await Task.Delay(500);



                    if (index <= 9)
                    {
                        if (Parameter != "pause")
                        {
                            HiveMgr.MachineStop?.Invoke();
                        }
                    }

                    sLastErrorCode = "";
                    nRemainingClicks = 0;

                    this.Hide();
                    //int index = RadioButtons.IndexOf(radioButton_select); // 返回2
                    if (index < 5)
                    {
                        //维修

                        string str1 = Parameter == "pause" ? "设备已暂停" : "设备已停机";
                        string str2 = Parameter == "pause" ? "Machine paused" : "Machine stopped";
                        Form_OperationMsg t1 = new Form_OperationMsg(str1, str2);
                        t1.StartPosition = FormStartPosition.CenterScreen;
                        t1.TopLevel = true;
                        t1.closeForm_resume += new closeForm(this.Form_Closing);
                        t1.Show();
                    }
                    else if (index > 9)
                    {
                        //非异常停机

                        Form_Resume t1 = new Form_Resume(this.GetType().Name);
                        t1.Button2.Visible = false;
                        t1.Button3.Visible = false;
                        t1.Label4.Visible = false;
                        t1.Label5.Visible = false;
                        t1.Label6.Visible = false;
                        t1.Label7.Visible = false;
                        //t1.comboBox_UsedUnits.Visible = false;
                        //t1.comboBox_rating.Visible = false;

                        t1.StartPosition = FormStartPosition.CenterScreen;
                        t1.TopLevel = true;
                        t1.closeForm_resume += new closeForm(this.Form_Closing);
                        t1.Show();

                    }
                    else
                    {
                        if (index == 6 || index == 7)
                        {
                            Form_MsgWithMaintenance t1 = new Form_MsgWithMaintenance(GetCheckedButton().Text, "");
                            t1.StartPosition = FormStartPosition.CenterScreen;
                            t1.TopLevel = true;
                            t1.closeForm_resume += new closeForm(this.Form_Closing);
                            t1.Show();
                        }
                        else
                        {
                            this.Close();
                        }

                    }

                    //this.Close();
                    Textbadgechange = false;
                    timer1.Enabled = false;


                }
                else
                {
                    TextBox_badgeTip.Text = "badge info sent, MES info received.Access denied.";
                    TextBox_badge.BackColor = HiveUIColors.Red;//红
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            if ((now - OpenTime).TotalSeconds <= 120)
            {
                label1_retainTime.Text = (secondsLeft - (now - OpenTime)).ToString(@"mm\:ss");
            }
            else
            {
                if (label1_retainTime.Text != "00:00")
                {
                    label1_retainTime.Text = "00:00";
                }
            }
        }

        private void label1_retainTime_TextChanged(object sender, EventArgs e)
        {
            if (label1_retainTime.Text == "00:00")
            {
                tableLayoutPanel_button.Visible = false;
            }
        }
        // 响应IO启动
        public void TriggerResume()
        {
            if (label1_retainTime.Text != "00:00")
            {
                if (!Form_SafetyStatusMonitor.CheckSafetyStatus())
                {
                    this.BeginInvoke((MethodInvoker)delegate
                    {
                        if (!Form_SafetyStatusMonitor.IsOpened)
                        {
                            ShowSafetyStatusMonitorAtTopRight();
                        }

                    });
                    return;
                }
                else
                {
                    if (Form_SafetyStatusMonitor.IsOpened)
                        Form_SafetyStatusMonitor.Instance.Close();
                }

                if (HiveMgr.MachineStart?.Invoke() ?? false)
                {
                    var stateParams = new MachineStateParameters
                    {
                        MachineState = HiveMgr.IsRuning() ? MachineStateData.MachineState.Running : MachineStateData.MachineState.Idle,
                        event_id = HiveTool.Event_id,
                    };
                    HiveCommandProcessor.Instance.ProcessCommand(new List<string> { "Machinestate" }, stateParams);

                    this.Close();
                }

               
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!Form_SafetyStatusMonitor.CheckSafetyStatus())
            {
                this.BeginInvoke((MethodInvoker)delegate
                {
                    if (!Form_SafetyStatusMonitor.IsOpened)
                    {
                        ShowSafetyStatusMonitorAtTopRight();
                    }

                });
                return;
            }
            else
            {
                if (Form_SafetyStatusMonitor.IsOpened)
                    Form_SafetyStatusMonitor.Instance.Close();
            }

            if (HiveMgr.MachineStart?.Invoke() ?? false)
            {
                var stateParams = new MachineStateParameters
                {
                    MachineState = HiveMgr.IsRuning() ? MachineStateData.MachineState.Running : MachineStateData.MachineState.Idle,
                    event_id = HiveTool.Event_id,
                };
                HiveCommandProcessor.Instance.ProcessCommand(new List<string> { "Machinestate" }, stateParams);

                this.Close();
            }

        }
        // 在右上角显示安全状态监控窗口
        private void ShowSafetyStatusMonitorAtTopRight()
        {
            var safetyForm = Form_SafetyStatusMonitor.Instance;



            // 设置所有者窗体
            var mainForm = PopupManager.Instance.GetMainForm();
            if (mainForm != null && !mainForm.IsDisposed)
            {
                if (HiveMgr.SetFormOwner)
                    safetyForm.Owner = mainForm;
            }

            // 计算位置：Form_SafetyStatusMonitor的左下角对准Form_Warning的右上角
            int x = this.Right; // Form_SafetyStatusMonitor的左边框与Form_Warning的右边框对齐
            int y = this.Top - safetyForm.Height; // Form_SafetyStatusMonitor的底部与Form_Warning的顶部对齐

            // 确保不会超出屏幕边界
            Screen currentScreen = Screen.FromControl(this);
            Rectangle workingArea = currentScreen.WorkingArea;

            // 如果超出屏幕顶部，调整到屏幕顶部
            if (y < workingArea.Top)
                y = workingArea.Top;

            // 如果超出屏幕右侧，调整到屏幕左侧
            if (x + safetyForm.Width > workingArea.Right)
                x = this.Left - safetyForm.Width;

            // 如果调整后超出屏幕左侧，则居中显示
            if (x < workingArea.Left)
                x = (workingArea.Width - safetyForm.Width) / 2;


            // 设置位置并显示
            safetyForm.StartPosition = FormStartPosition.Manual;
            safetyForm.Location = new Point(x, y);
            safetyForm.Show();
            safetyForm.SetErrorInfo("F99OOOO-20", "Machine stopped through UI");
            // 确保安全状态窗体在最前面
            safetyForm.TopMost = true;
            safetyForm.BringToFront();
        }
        private void Form_Operation_FormClosed(object sender, FormClosedEventArgs e)
        {
            closeForm_resume?.Invoke();
            closeForm_resume = null;
            thisshowed = false;
            //允许操作主程序按钮(Start,Pause,Stop)

            HiveCommandProcessor.Instance.SetMainProgramButtonsEnabled(true);
        }
        public void Form_Closing()
        {
            closeForm_resume?.Invoke();
            closeForm_resume = null;
            this.Close();
        }
        // 权限检查方法
        private bool CheckBadgeAccess(string badgeLevel, string machineStateCategory)
        {
            // 根据设备状态分类和弹窗级别确定所需权限
            var accessRequirements = GetAccessRequirements();

            // 检查工卡等级是否在允许的权限列表中
            if (accessRequirements.ContainsKey(machineStateCategory))
            {
                var allowedLevels = accessRequirements[machineStateCategory];
                return allowedLevels.Contains(badgeLevel);
            }

            return false; // 如果没有找到对应的权限配置，默认拒绝访问
        }

        // 定义权限要求字典
        private Dictionary<string, List<string>> GetAccessRequirements()
        {
            var requirements = new Dictionary<string, List<string>>();

            // 根据第一张图片的权限表格定义
            // 1.1 Auto triggered DT - 一级弹窗
            requirements["1.1_1st"] = new List<string> { "L1", "L2", "L3", "L6", "L9" }; // OSS/CM AE/MFG Technician
                                                                                         // 1.1 Auto triggered DT - 二级弹窗  
            requirements["1.1_2nd"] = new List<string> { "L1", "L2", "L3", "L6", "L7", "L8", "L9" }; // 增加了CPL7和CM Line Leader

            // 1.2.1 Manual triggered DT - repair - 一级弹窗
            requirements["1.2.1_1st"] = new List<string> { "L1", "L2", "L3", "L6", "L9" };
            // 1.2.1 Manual triggered DT - repair - 二级弹窗
            requirements["1.2.1_2nd"] = new List<string> { "L1", "L2", "L3", "L6", "L7", "L8", "L9" };

            // 1.2.2 Manual triggered DT - Maintenance and others - 一级弹窗
            requirements["1.2.2_1st"] = new List<string> { "L1", "L2", "L3", "L6", "L7", "L8", "L9" };
            // 1.2.2 Manual triggered DT - Maintenance and others - 二级弹窗
            requirements["1.2.2_2nd"] = new List<string> { "L1", "L2", "L3", "L6", "L7", "L8", "L9" };

            // 1.2.3 Manual triggered DT - degraded mode - 一级弹窗
            requirements["1.2.3_1st"] = new List<string> { "L1", "L2", "L3", "L6", "L7", "L8", "L9" };
            // 1.2.3 Manual triggered DT - degraded mode - 二级弹窗
            requirements["1.2.3_2nd"] = new List<string> { "L1", "L2", "L3", "L6", "L7", "L8", "L9" };

            return requirements;
        }
        // 获取当前选择的设备状态分类（一级弹窗）
        private string GetCurrentStateCategory()
        {
            var checkedButton = GetCheckedButton();
            if (checkedButton == null) return null;

            // 根据单选按钮确定状态分类
            if (RadioButtonErrorCode.ContainsKey(checkedButton))
            {
                // 根据按钮索引确定状态分类
                int index = RadioButtons.IndexOf(checkedButton);

                if (index < 5) return "1.2.1"; // 维修 - 一级弹窗
                else if (index < 10) return "1.2.2"; // 保养和其他 - 一级弹窗  
                else if (index == 10) return "1.2.3"; // 非停机异常 - 一级弹窗
            }

            return "1.2.1"; // 默认
        }
        // 更新label5的显示内容
        private void UpdatePromptText()
        {
            var checkedButton = GetCheckedButton();
            if (checkedButton == null)
            {
                label5.Text = "OSS/CM AE: 请刷卡提交选项";
                return;
            }

            string stateCategory = GetCurrentStateCategory();
            string promptText = GetPromptTextByState(stateCategory);
            label5.Text = promptText;
        }

        // 根据状态获取提示文本
        private string GetPromptTextByState(string stateCategory)
        {
            // 根据第二张图片的关键时间点表格定义提示文本
            var promptTexts = new Dictionary<string, string>
            {
                // 1.1 Auto triggered DT
                ["1.1"] = "OSS/CM AE: 请刷卡提交选项",

                // 1.2.1 Manual triggered DT - repair  
                ["1.2.1"] = "OSS/CM AE: 请刷卡提交选项",

                // 1.2.2 Manual triggered DT - Maintenance and others
                ["1.2.2"] = "OSS/CM AE/OP/线长: 请刷卡提交选项",

                // 1.2.3 Manual triggered DT - degraded mode
                ["1.2.3"] = "OSS/CM AE/OP/线长: 请刷卡提交选项"
            };

            return promptTexts.ContainsKey(stateCategory) ? promptTexts[stateCategory] : "OSS/CM AE: 请刷卡提交选项";
        }


    }

    public static class DeepCopyExtensions
    {

        public static T DeepCopy<T>(this T source)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", "source");
            }

            if (source == null)
            {
                return default(T);
            }

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, source);
                memoryStream.Position = 0L;
                return (T)binaryFormatter.Deserialize(memoryStream);
            }

        }
    }
}



