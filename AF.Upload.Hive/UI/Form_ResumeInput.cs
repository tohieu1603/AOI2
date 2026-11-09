using AF.File;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using static AF.Upload.Hive.Form_ResumeMsg;
using static AF.Upload.Hive.HiveTool;
using static AF.Upload.Hive.MachineStateData;

namespace AF.Upload.Hive
{
    public partial class Form_ResumeInput : HiveBaseForm
    {
        public static bool thisshowed = false;
        string CreatethisClass = "";
        bool Textbadgechange = false;
        DateTime Helprequest_resume = DateTime.Now;
        DateTime MaintenanceStart_resume = DateTime.Now;

        internal event closeForm closeForm_resume;
        string BadgeCard = "";
        public Form_ResumeInput(string CreateClass, string badgeCard)
        {
            InitializeComponent();
            this.TopMost = true;
            thisshowed = true;
            this.Load += new EventHandler(Form_Load);
            CreatethisClass = CreateClass;
            BadgeCard = badgeCard;
            if (CreatethisClass == "HiveCommandProcessor")
            {
                Helprequest_resume = Convert.ToDateTime(Ini.GetStringValue(HiveCommandProcessor.m_strIniFile, "Form_Warning", "HeipRequestTimeStart", DateTime.Now.ToString()));
                MaintenanceStart_resume = Convert.ToDateTime(Ini.GetStringValue(HiveCommandProcessor.m_strIniFile, "Form_Warning", "MaintenanceTimeStart", DateTime.Now.ToString()));
            }
            else
            {
                Helprequest_resume = Convert.ToDateTime(Ini.GetStringValue(HiveCommandProcessor.m_strIniFile, "Form_warning", "HeipRequestTimeStart", DateTime.Now.ToString()));
                MaintenanceStart_resume = Convert.ToDateTime(Ini.GetStringValue(HiveCommandProcessor.m_strIniFile, "Form_warning", "MaintenanceTimeStart", DateTime.Now.ToString()));
            }

            //不允许操作主程序按钮(Start,Pause,Stop)
            HiveCommandProcessor.Instance.SetMainProgramButtonsEnabled(false);


            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            //// 获取屏幕分辨率
            //Rectangle screen = Screen.GetWorkingArea(this);
            //int width = screen.Width / 5 * 2;
            //int height = screen.Height / 3 * 1;

            //// 设置窗体客户区大小
            //this.ClientSize = new Size(width, height);
            this.CenterToScreen();
            BadgeCard = badgeCard;
        }

        private void Form_Load(object sender, EventArgs e)
        {
            this.Activate();
            tableLayoutPanel6.BackColor = Color.FromArgb(141, 212, 113);//绿

            button_Submit.BackColor = HiveUIColors.Gray;

            dropDownTextBox1.ClearItems();
            dropDownTextBox3.ClearItems();
            dropDownTextBox2.ClearItems();
            comboBox_UsedUnits.Items.Clear();
            dropDownTextBox1.AddItems(HiveMgr.ActionTaken.ToArray());
            dropDownTextBox2.AddItems(HiveMgr.ToolsUsed.ToArray());
            dropDownTextBox3.AddItems(HiveMgr.SettingsChanged.ToArray());
            comboBox_UsedUnits.Items.AddRange(HiveMgr.SparePartList.ToArray());

            int height = (int)tableLayoutPanel1.GetTipTopToFirstLineDelta() + 5;
            dropDownTextBox1.Height = height;
            dropDownTextBox3.Height = height;
            dropDownTextBox2.Height = height;

            dropDownTextBox1.ButtonClick += OnButtonClick;
            dropDownTextBox1.TextBoxTextChanged += OnTextChanged;
            dropDownTextBox3.ButtonClick += OnButtonClick;
            dropDownTextBox3.TextBoxTextChanged += OnTextChanged;
            dropDownTextBox2.ButtonClick += OnButtonClick;
            dropDownTextBox2.TextBoxTextChanged += OnTextChanged;

            comboBox_DownReason.Items.Clear();
            comboBox_DownReason.Items.AddRange(HiveMgr.Downtime_Cause_List.ToArray());

            if (Form_Operation.sCheckedButton.Trim() == "Weekly Maintenance"|| Form_Operation.sCheckedButton.Trim() == "Monthly Maintenance")
            {
                comboBox_DownReason.SelectedIndex = 0;
            }
        }
        private void OnButtonClick(object sender, EventArgs e)
        {

            // 处理按钮点击
        }

        private void OnTextChanged(object sender, EventArgs e)
        {
            if (dropDownTextBox1.Text != "" || dropDownTextBox3.Text != "" || dropDownTextBox2.Text != "")
            {
                tableLayoutPanel1.TipVisible = false;
                dropDownTextBox1.Dock = DockStyle.Fill;
                dropDownTextBox3.Dock = DockStyle.Fill;
                dropDownTextBox2.Dock = DockStyle.Fill;
            }
            else
            {

                tableLayoutPanel1.TipVisible = true;

                dropDownTextBox1.Dock = DockStyle.Top;
                dropDownTextBox3.Dock = DockStyle.Top;
                dropDownTextBox2.Dock = DockStyle.Top;

                int height = (int)tableLayoutPanel1.GetTipTopToFirstLineDelta() + 5;
                dropDownTextBox1.Height = height;
                dropDownTextBox3.Height = height;
                dropDownTextBox2.Height = height;
            }


        }

        public bool badge_confirm()
        {
            //只有宕机原因和消耗物品选择了之后才能刷卡通过     维护模式下不需要消耗物品
            if (comboBox_DownReason.SelectedItem != null && comboBox_UsedUnits.SelectedItem != null &&
                (dropDownTextBox1.SelectedItem != null || !string.IsNullOrWhiteSpace(dropDownTextBox1.Text)) &&
                (dropDownTextBox3.SelectedItem != null || !string.IsNullOrWhiteSpace(dropDownTextBox3.Text)) &&
                (dropDownTextBox2.SelectedItem != null || !string.IsNullOrWhiteSpace(dropDownTextBox2.Text)))
            {
                return true;
            }
            return false;
        }

        private void TextBox_badge_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form_Resume_FormClosed(object sender, FormClosedEventArgs e)
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

        private void Form_Resume_Load(object sender, EventArgs e)
        {

        }

        private void button_Submit_Click(object sender, EventArgs e)
        {
            //dropDownTextBox1.Invalidate();
            //dropDownTextBox2.Invalidate();
            //dropDownTextBox3.Invalidate();
            //return;
            if (badge_confirm())
            {
                HiveMgr.CheckAndAddToColumn("检查步骤", dropDownTextBox1.Text);
                HiveMgr.CheckAndAddToColumn("用到工具", dropDownTextBox2.Text);
                HiveMgr.CheckAndAddToColumn("调整参数", dropDownTextBox3.Text);
                //hive上传
                var stateParams = new MachineStateParameters
                {
                    MachineState = MachineStateData.MachineState.Idle,
                    status = "report end",
                    event_id = HiveTool.Event_id,
                    downtime_cause = comboBox_DownReason.SelectedItem.ToString(),
                    action_taken = $"{dropDownTextBox1.Text};{dropDownTextBox2.Text};{dropDownTextBox3.Text}",
                    spare_parts = comboBox_UsedUnits.SelectedItem.ToString(),
                    rating = comboBox_rating.SelectedItem?.ToString(),
                    RatingReplace=true,
                };
                Ini.WriteValue(HiveCommandProcessor.m_strIniFile, "LocalData", "Status", stateParams.status);
                HiveCommandProcessor.Instance.ProcessCommand(new List<string> { "Machinestate" }, stateParams);
               



                this.Close();
                Textbadgechange = false;

            }
        }
    }
}
