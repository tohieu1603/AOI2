using AF.File;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AF.Upload.Hive.HiveTool;


namespace AF.Upload.Hive
{
    public partial class Form_Resume : HiveBaseForm
    {
        public static bool thisshowed = false;
        string CreatethisClass = "";
        bool Textbadgechange = false;
        DateTime Helprequest_resume = DateTime.Now;
        DateTime MaintenanceStart_resume = DateTime.Now;

        internal event closeForm closeForm_resume;

        public Form_Resume(string CreateClass)
        {
            InitializeComponent();
            this.TopMost = true;
            thisshowed = true;
            this.Load += new EventHandler(Form_Load);
            CreatethisClass = CreateClass;
            if (CreatethisClass == "HiveCommandProcessor")
            {
                Helprequest_resume = Convert.ToDateTime(Ini.GetStringValue(HiveCommandProcessor.m_strIniFile, "Form_Warning", "HeipRequestTimeStart", DateTime.Now.ToString()));
                MaintenanceStart_resume = Convert.ToDateTime(Ini.GetStringValue(HiveCommandProcessor.m_strIniFile, "Form_Warning", "MaintenanceTimeStart", DateTime.Now.ToString()));
            }
            else
            {
                Helprequest_resume = Convert.ToDateTime(Ini.GetStringValue(HiveCommandProcessor.m_strIniFile, "Form_Warning", "HeipRequestTimeStart", DateTime.Now.ToString()));
                MaintenanceStart_resume = Convert.ToDateTime(Ini.GetStringValue(HiveCommandProcessor.m_strIniFile, "Form_Warning", "MaintenanceTimeStart", DateTime.Now.ToString()));
            }

            //不允许操作主程序按钮(Start,Pause,Stop)
            HiveCommandProcessor.Instance.SetMainProgramButtonsEnabled(false);

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            // 获取屏幕分辨率
            Rectangle screen = Screen.GetWorkingArea(this);
            int width = screen.Width / 3 * 2;
            int height = screen.Height / 2 * 1;

            // 设置窗体客户区大小
            this.ClientSize = new Size(width, height);
            this.CenterToScreen();

        }

        private void Form_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
            this.Activate();
            TableLayoutPanel2.BackColor = HiveUIColors.Blue;//蓝
            tableLayoutPanel6.BackColor = HiveUIColors.Green;//绿
            TextBox_badgeTip.BackColor = HiveUIColors.Gray;
            textBox_HelpTime.BackColor = HiveUIColors.Gray;
            textBox_RepairStart.BackColor = HiveUIColors.Gray;
            textBox_RepairEnd.BackColor = HiveUIColors.Gray;
            textBox_DownTime.BackColor = HiveUIColors.Gray;

            Button1.BackColor = HiveUIColors.Gray;
            Button2.BackColor = HiveUIColors.Gray;
            Button3.BackColor = HiveUIColors.Gray;

            textBox_HelpTime.Text = Helprequest_resume.ToString("G");
            textBox_RepairStart.Text = MaintenanceStart_resume.ToString("G");
            textBox_RepairEnd.Text = DateTime.Now.ToString("G");
            textBox_DownTime.Text = (DateTime.Now - Helprequest_resume).TotalMinutes.ToString("F2") + " minutes";

            textBox_HelpTime.ReadOnly = true;
            textBox_RepairStart.ReadOnly = true;
            textBox_RepairEnd.ReadOnly = true;
            textBox_DownTime.ReadOnly = true;
            TextBox_badgeTip.ReadOnly = true;
            TextBox_badge.ReadOnly = false;
            //TextBox_刷卡.ReadOnly = true;
            TextBox_badge.BackColor = HiveUIColors.Gray;//灰色 R = 214 ; G = 214 ; B = 214

            TextBox_badge.Focus();
            this.ActiveControl = TextBox_badge;

        }


        public bool badge_confirm()
        {
            if (/*comboBox_DownReason.SelectedItem != null*/ /*&&*/ /*(comboBox_UsedUnits.SelectedItem != null ||*/ /*CreatethisClass == "Form_Operation")*/true)//只有宕机原因和消耗物品选择了之后才能刷卡通过     维护模式下不需要消耗物品
            {
                return true;
            }
            return false;
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            textBox_RepairEnd.Text = DateTime.Now.ToString("G");
            textBox_DownTime.Text = (DateTime.Now - Helprequest_resume).TotalMinutes.ToString("F2") + " minutes";

        }
        private async void TextBox_badge_TextChanged(object sender, EventArgs e)
        {
            if (HiveTool.Machine5PcsStatus)
            {
                return;
            }
            if (Textbadgechange)
            {
                return;
            }
            if (TextBox_badge.Text.EndsWith("\r\n"))
            {
                string badge=TextBox_badge.Text.Trim('\r','\n');
                TextBox_badge.Text = "";
                if (badge_confirm())
                {
                    HiveUserInfo hiveUserInfo = new HiveUserInfo() { CardTagid = badge };
                    Textbadgechange = true;
                    bCommError = false;
                    bool success = false;
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
                            TextBox_badgeTip.Text = $"badge info send:{hiveUserInfo.strSend}\r\n, MES info received:{hiveUserInfo.strReceive}\r\n. You may now input units.";
                            TextBox_badge.BackColor = HiveUIColors.Green;//绿
                            bCommError = false;
                            break;
                        }
                        bCommError = true;
                        TextBox_badgeTip.Text = $"badge info send:{hiveUserInfo.strSend}\r\n, MES inforeceived:{hiveUserInfo.strReceive}\r\n.Access denied.";
                        TextBox_badge.BackColor = HiveUIColors.Red;//红
                    }

                    if (bCommError)
                    {
                        Textbadgechange = false;
                        return;
                    }

                    if (!Form_SafetyStatusMonitor.CheckSafetyStatus())
                    {
                        this.BeginInvoke((MethodInvoker)delegate
                        {
                            if (!Form_SafetyStatusMonitor.IsOpened)
                            {
                                ShowSafetyStatusMonitorAtTopRight();
                            }

                        });
                        Textbadgechange = false;
                        return;
                    }
                    else
                    {
                        if (Form_SafetyStatusMonitor.IsOpened)
                            Form_SafetyStatusMonitor.Instance.Close();
                    }

                    await Task.Delay(1000);
                    Textbadgechange = false;

                    if (HiveMgr.MachineStart?.Invoke() ?? false)
                    {

                        HiveTool.Event_id = Ini.GetStringValue(HiveCommandProcessor.m_strIniFile, "LocalData", "Event_id", "");
                        //hive上传
                        var stateParams = new MachineStateParameters
                        {
                            MachineState = MachineStateData.MachineState.Idle,
                            status = "repair end",
                            event_id = HiveTool.Event_id,
                            badge = hiveUserInfo.Level,
                        };


                        if (CreatethisClass == "Form_Operation")
                        {
                            stateParams.downtime_cause = "Others;其他";
                            stateParams.action_taken = "";
                            stateParams.spare_parts = "000;No parts replaced;无更换物品";
                            stateParams.rating = "";
                            stateParams.RatingReplace = true;
                        }

                        Ini.WriteValue(HiveCommandProcessor.m_strIniFile, "LocalData", "Status", stateParams.status);
                        HiveCommandProcessor.Instance.ProcessCommand(new List<string> { "Machinestate" }, stateParams);



                        if (CreatethisClass == "HiveCommandProcessor")
                        {
                            this.Hide();

                            //停止设备
                            Form_ResumeInput t1 = new Form_ResumeInput("Form_Operation", badge);
                            t1.closeForm_resume += new closeForm(this.Form_Closing);
                            t1.StartPosition = FormStartPosition.CenterScreen;
                            //t1.TopLevel = true;
                            t1.Show();
                        }
                        else
                        {
                            this.Close();
                        }
                    }

                }
                else
                {
                    TextBox_badgeTip.Text = "badge info sent, MES inforeceived.Access denied.";
                    TextBox_badge.BackColor = HiveUIColors.Red;//红
                }
            }
        }

        private void Button1_Click(object sender, EventArgs e)
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
                if (!HiveMgr.Machine5PcsStatus)
                {
                    HiveTool.Machine5PcsStatus = true;
                }
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
            safetyForm.SetErrorInfo("repair end", "repair end");
            // 确保安全状态窗体在最前面
            safetyForm.TopMost = true;
            safetyForm.BringToFront();
        }

        // 响应IO启动
        public void TriggerPause()
        {
            if (HiveMgr.MachinePause?.Invoke() ?? false)
            {
                //暂停设备  
                Form_ResumeMsg t1 = new Form_ResumeMsg("设备已暂停", "Machine paused");
                t1.closeForm_resume += new closeForm(this.Form_Closing);
                t1.StartPosition = FormStartPosition.CenterScreen;
                t1.TopLevel = true;
                t1.ShowDialog();
            }
        }
        private void Button2_Click(object sender, EventArgs e)
        {
            HiveTool.Machine5PcsStatus = false;

            //if (HiveMgr.MachineStop?.Invoke() ?? false)
            //{
                //停止设备
                Form_ResumeMsg t1 = new Form_ResumeMsg("设备已停机", "Machine stopped");
                t1.closeForm_resume += new closeForm(this.Form_Closing);
                t1.StartPosition = FormStartPosition.CenterScreen;
                //t1.TopLevel = true;
                t1.ShowDialog();
            //} 
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            TriggerPause();
        }

        private void Form_Resume_FormClosed(object sender, FormClosedEventArgs e)
        {
            HiveTool.Machine5PcsStatus = false;
            closeForm_resume?.Invoke();
            closeForm_resume=null;
            thisshowed = false;
            //允许操作主程序按钮(Start,Pause,Stop)
            HiveCommandProcessor.Instance.SetMainProgramButtonsEnabled(true);
        }

        public void Form_Closing()
        {
            HiveTool.Machine5PcsStatus = false;
            closeForm_resume?.Invoke();
            closeForm_resume = null;
            this.Close();
        }

        private void Form_Resume_Load(object sender, EventArgs e)
        {

        }

    }



}
