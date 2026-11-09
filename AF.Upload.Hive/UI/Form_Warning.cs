using AF.File;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AF.Upload.Hive.Form_ResumeMsg;
using static AF.Upload.Hive.HiveTool;
using static AF.Upload.Hive.MachineStateData;
using static System.Windows.Forms.AxHost;

namespace AF.Upload.Hive
{

    public partial class Form_Warning : HiveBaseForm
    {
        public DateTime HeipRequestTimeStart = DateTime.Now;
        public DateTime MaintenanceTimeStart = DateTime.Now;
        public static bool thisshowed = false;
        bool Textbadgechange = false;
        TimeSpan secondsLeft = new TimeSpan(0, 0, 120);
        DateTime OpenTime = DateTime.Now;

        bool bRestart = false;
        DateTime dtLastOutPutTime = DateTime.Now;
        internal event closeForm closeForm_resume;

        public static int nRemainingClicks = 0;
        public static string sLastErrorCode= "";
        public static DateTime dtLastClickTime = DateTime.Now;
        public Form_Warning(string ErrorDetail, bool restart)
        {
            InitializeComponent();
            this.TopMost = true;
            this.bRestart = restart;
            thisshowed = true;
            secondsLeft = new TimeSpan(0, 0, 120);
            ErrorCode errormsg = HiveMgr.ErrorMsg(ErrorDetail);
            TextBox_Errorcode.Text = errormsg.Code;
            TextBox_Errormessage.Text = errormsg.Error_Description_English;
            TextBox_Errordetail.Text = errormsg.Error_Description_Chinese;
            Ini.WriteValue(HiveCommandProcessor.m_strIniFile, "LocalData", "ErrorMsg", ErrorDetail);


            this.Load += new EventHandler(Form_Load);
            HeipRequestTimeStart = DateTime.Now;

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            // 获取屏幕分辨率
            Rectangle screen = Screen.GetWorkingArea(this);
            int width = screen.Width / 3;
            int height = screen.Height / 3;

            // 设置窗体客户区大小
            this.ClientSize = new Size(width, height);

            this.CenterToScreen();
        }
        private void Form_Load(object sender, EventArgs e)
        {
            this.Activate();
            Ini.WriteValue(HiveCommandProcessor.m_strIniFile, "LocalData", "LastPOP", "Form_Warning");

            TableLayoutPanel1.BackColor = HiveUIColors.Red;//红
            Button1.BackColor = HiveUIColors.Blue;//蓝
            TextBox_badgeTip.BackColor = HiveUIColors.Gray;//灰
            TextBox_badge.BackColor = HiveUIColors.Gray;//灰色 R = 214 ; G = 214 ; B = 214


            TextBox_Help.BackColor = HiveUIColors.Gray;//灰
            TextBox_Errorcode.BackColor = HiveUIColors.Gray;//灰
            TextBox_Errormessage.BackColor = HiveUIColors.Gray;//灰
            TextBox_Errordetail.BackColor = HiveUIColors.Gray;//灰
            panel1.BackColor = HiveUIColors.Gray;
            tableLayoutPanel_button.BackColor = HiveUIColors.Red;

            timer1.Enabled = true;


            Ini.WriteValue(HiveCommandProcessor.m_strIniFile, "Form_warning", "HeipRequestTimeStart", HeipRequestTimeStart.ToString());

            TextBox_Help.Text = HeipRequestTimeStart.ToString("G");

            TextBox_Help.ReadOnly = true;
            TextBox_Errorcode.ReadOnly = true;
            TextBox_Errormessage.ReadOnly = true;
            TextBox_Errordetail.ReadOnly = true;
            TextBox_badgeTip.ReadOnly = true;
            //TextBox_刷卡.ReadOnly = true;

            TextBox_badge.ReadOnly = false;
            this.TopMost = true;

            if (bRestart)
            {
                tableLayoutPanel_button.Visible = false;
                HiveTool.Event_id = Ini.GetStringValue(HiveCommandProcessor.m_strIniFile, "LocalData", "Event_id", "");

                if (TextBox_Errormessage.Text == "PC/SW crashed")
                {
                    string StrLastOutPutTime = Ini.GetStringValue(HiveCommandProcessor.m_strIniFile, "LocalData", "LastOutPutTime", DateTime.Now.AddSeconds(-10).ToString("G"));

                    if (DateTime.TryParse(StrLastOutPutTime, out dtLastOutPutTime))
                    {
                        HiveTool.Event_id = dtLastOutPutTime.ToString(HiveTool.TimeFormat);
                    }
                    else
                    {
                        dtLastOutPutTime = DateTime.Now.AddSeconds(-10);
                        HiveTool.Event_id = dtLastOutPutTime.ToString(HiveTool.TimeFormat); ;
                    }
                }
            }

            TextBox_badge.Focus();
            this.ActiveControl = TextBox_badge;


            if (!bRestart)
            {
                if (!(HiveMgr.MachinePause?.Invoke() ?? false))
                    tableLayoutPanel_button.Visible = false;
              

                if (sLastErrorCode== TextBox_Errordetail.Text&&(DateTime.Now- dtLastClickTime).TotalMinutes<30)
                {
                    nRemainingClicks++;
                }
                else
                {
                    sLastErrorCode = TextBox_Errordetail.Text;
                    nRemainingClicks = 0;
                }
                dtLastClickTime = DateTime.Now;
                if (nRemainingClicks>=3)
                {
                    tableLayoutPanel_button.Visible = false;
                }

                //发送hive
                HiveTool.Event_id = DateTime.Now.ToString(HiveTool.TimeFormat);
            }
            var stateParams = new MachineStateParameters
            {
                MachineState = MachineStateData.MachineState.UnplannedDown,
                error_message = TextBox_Errormessage.Text,
                code = TextBox_Errorcode.Text,
                status = "help request",
                event_id = HiveTool.Event_id,
            };

            if (TextBox_Errormessage.Text == "PC/SW crashed")
                stateParams.StateChangeTime = dtLastOutPutTime;

            Ini.WriteValue(HiveCommandProcessor.m_strIniFile, "LocalData", "ErrorCode", stateParams.code);
            Ini.WriteValue(HiveCommandProcessor.m_strIniFile, "LocalData", "Status", stateParams.status);
            Ini.WriteValue(HiveCommandProcessor.m_strIniFile, "LocalData", "Event_id", stateParams.event_id);

            HiveCommandProcessor.Instance.ProcessCommand(new List<string> { "Machinestate" }, stateParams);

            Form_Operation.sCheckedButton = TextBox_Errormessage.Text;
            //不允许操作主程序按钮(Start,Pause,Stop)
            HiveCommandProcessor.Instance.SetMainProgramButtonsEnabled(false);


            this.BeginInvoke((MethodInvoker)delegate
            {
                if (!Form_SafetyStatusMonitor.IsOpened)
                {
                    //ShowSafetyStatusMonitorAtTopRight();
                }

            });

            label1_retainClicks.Text = $"{3 - nRemainingClicks}";
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
            safetyForm.SetErrorInfo(TextBox_Errorcode.Text, TextBox_Errormessage.Text);
            // 确保安全状态窗体在最前面
            safetyForm.TopMost = true;
            safetyForm.BringToFront();
        }

        public bool badge_confirm()
        {
            return true;
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
                        TextBox_badgeTip.Text = $"badge info send:{hiveUserInfo.strSend}\r\n, MES info received:{hiveUserInfo.strReceive}\r\n. Access denied.";
                        TextBox_badge.BackColor = HiveUIColors.Red;//红
                    }

                    if (bCommError)
                    {
                        Textbadgechange = false;
                        return;
                    }

                    List<string> CheckBadge = new List<string> { "L1", "L2", "L3", "L6", "L9" }; // OSS/CM AE/

                    string badgeLevel = hiveUserInfo.Level; // 假设这是从MES获取的工卡等级
                    if (!CheckBadge.Contains(badgeLevel)) // 一级弹窗权限检查
                    {
                        //TextBox_badgeTip.Text = "Access denied: Insufficient badge level";
                        //TextBox_badge.BackColor = HiveUIColors.Red;
                        Textbadgechange = false;
                        return;
                    }

                    //发送hive

                    var stateParams = new MachineStateParameters
                    {
                        MachineState = MachineStateData.MachineState.UnplannedDown,
                        error_message = TextBox_Errormessage.Text,
                        code = TextBox_Errorcode.Text,
                        status = "repair start",
                        event_id = HiveTool.Event_id,
                        badge = badgeLevel
                    };

                    Ini.WriteValue(HiveCommandProcessor.m_strIniFile, "LocalData", "ErrorCode", stateParams.code);
                    Ini.WriteValue(HiveCommandProcessor.m_strIniFile, "LocalData", "Status", stateParams.status);
                    Ini.WriteValue(HiveCommandProcessor.m_strIniFile, "LocalData", "Event_id", stateParams.event_id);

                    HiveCommandProcessor.Instance.ProcessCommand(new List<string> { "Machinestate" }, stateParams);

                    await Task.Delay(1000);



                    sLastErrorCode = "";
                    nRemainingClicks = 0;


                    this.Hide();
                    timer1.Enabled = false;
                    Form_MsgWithRepair t1 = new Form_MsgWithRepair("维修/Troubleshooting SOP", "设备已停机 Machine stopped");
                    t1.StartPosition = FormStartPosition.CenterScreen;
                    t1.TopLevel = true;
                    t1.ShowDialog();

                    this.Close();
                    Textbadgechange = false;
                }
                else
                {
                    TextBox_badgeTip.Text = "badge info sent, MES info received.Access denied.";
                    TextBox_badge.BackColor = HiveUIColors.Red;//红
                }
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
                var stateParams = new MachineStateParameters
                {
                    MachineState = HiveMgr.IsRuning() ? MachineStateData.MachineState.Running : MachineStateData.MachineState.Idle,
                    event_id = HiveTool.Event_id,
                };
                HiveCommandProcessor.Instance.ProcessCommand(new List<string> { "Machinestate" }, stateParams);

                this.Close();
            }

        }

        private void Form_Warning_FormClosed(object sender, FormClosedEventArgs e)
        {
            closeForm_resume?.Invoke();
            thisshowed = false;
            //允许操作主程序按钮(Start,Pause,Stop)
            HiveCommandProcessor.Instance.SetMainProgramButtonsEnabled(true);

        }


    }


}
