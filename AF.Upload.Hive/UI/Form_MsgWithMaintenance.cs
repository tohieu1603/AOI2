using AF.File;
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
    public partial class Form_MsgWithMaintenance : HiveBaseFormText
    {
        public static bool thisshowed = false;
        //索引本地配置文件
        List<string> RepiarData;
        // 存储动态生成的CheckBox控件
        private List<CheckBox> dynamicCheckBoxes = new List<CheckBox>();

        internal event closeForm closeForm_resume;

        string StrTips = "";

        public DateTime WeeklyMaintenanceTimeStart = DateTime.Now;//周保养
        public DateTime MonthlyMaintenanceTimeStart = DateTime.Now;//月保养

        // 保养类型
        private bool isWeeklyMaintenance = false;

        public Form_MsgWithMaintenance(string tips, string message)
        {
            InitializeComponent();
            this.TopMost = true;
            //this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.FormBorderStyle = FormBorderStyle.None;
            thisshowed = true;
            StrTips = tips;
            if (tips.Contains("周保养")) //周保养
            {
                isWeeklyMaintenance = true;
                this.Text = "周保养 / Weekly Maintenance";
                RepiarData = HiveMgr.WeeklyMaintenanceItems;
            }
            else //月保养
            {
                isWeeklyMaintenance = false;
                this.Text = "月保养 / Monthly Maintenance";
                RepiarData = HiveMgr.MonthlyMaintenanceItems;
            }

            //不允许操作主程序按钮(Start,Pause,Stop)
            HiveCommandProcessor.Instance.SetMainProgramButtonsEnabled(false);


        }

        private void Button1_Click(object sender, EventArgs e)
        {
            // 检查所有CheckBox是否都已选中
            if (AreAllCheckBoxesChecked())
            {
                //MessageBox.Show("请完成所有维护项目后再提交！", "提示",
                //    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //return;

                if (StrTips.Contains("周保养")) //周保养
                {
                    WeeklyMaintenanceTimeStart = DateTime.Now;
                    Ini.WriteValue(HiveCommandProcessor.m_strMaintenanceIniFile, "Weekly", "LastMaintenanceDate", WeeklyMaintenanceTimeStart.ToString());
                }
                else
                {
                    MonthlyMaintenanceTimeStart = DateTime.Now;
                    Ini.WriteValue(HiveCommandProcessor.m_strMaintenanceIniFile, "Monthly", "LastMaintenanceDate", MonthlyMaintenanceTimeStart.ToString());
                }
            }

            this.Close();
        }
        /// <summary>
        /// 检查所有动态生成的CheckBox是否都已选中
        /// </summary>
        /// <returns>如果全部选中返回true，否则返回false</returns>
        private bool AreAllCheckBoxesChecked()
        {
            if (dynamicCheckBoxes.Count == 0)
                return true; // 如果没有CheckBox，直接返回true

            foreach (var checkBox in dynamicCheckBoxes)
            {
                if (!checkBox.Checked)
                {
                    return false;
                }
            }
            return true;
        }

        private void Form_MsgWithRepair_FormClosed(object sender, FormClosedEventArgs e)
        {
            thisshowed = false;
            closeForm_resume?.Invoke();
            //允许操作主程序按钮(Start,Pause,Stop)

            HiveCommandProcessor.Instance.SetMainProgramButtonsEnabled(true);
        }

        private void Form_MsgWithRepair_Load(object sender, EventArgs e)
        {
            this.Activate();
            this.panel1.BackColor = this.panel2.BackColor = lineControl1.BackColor = Color.White;

            // 清空可能存在的动态控件
            ClearDynamicControls();

            //索引本地配置文件

            CreateDynamicCheckBoxes();
        }

        /// <summary>
        /// 创建动态CheckBox控件
        /// </summary>
        private void CreateDynamicCheckBoxes()
        {
            List<string> list = RepiarData.ToArray().ToList();
            int startY = 10; // 起始Y坐标
            int spacing = 30; // 控件间距
            int currentY = startY;

            // 创建维护项目的CheckBox
            for (int i = 0; i < list.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(list[i]))
                {
                    CheckBox checkBox = CreateCheckBox(list[i].Trim(), currentY);
                    dynamicCheckBoxes.Add(checkBox);
                    panel1.Controls.Add(checkBox);
                    currentY += spacing;
                }
            }
            // 如果内容太多超过面板高度，调整面板的AutoScroll属性
            if (currentY > panel1.Height)
            {
                panel1.AutoScroll = true;
            }
        }
        /// <summary>
        /// 创建单个CheckBox控件
        /// </summary>
        private CheckBox CreateCheckBox(string text, int yPosition)
        {
            CheckBox checkBox = new CheckBox()
            {
                AutoSize = true,
                Text = text,
                Location = new Point(20, yPosition),
                Font = new Font("微软雅黑", 10),
                BackColor = Color.Transparent
            };

            // 为CheckBox添加选中状态改变事件
            checkBox.CheckedChanged += (sender, e) =>
            {
                if (checkBox.Checked)
                {
                    // 通过 HiveExManager 更新保养时间到Excel
                    bool success = HiveMgr.UpdateMaintenanceTime(new List<string>(){ checkBox.Text}, isWeeklyMaintenance);

                    if (!success)
                    {
                        MessageBox.Show("保存保养时间失败，请检查Excel文件是否被占用！", "错误",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };

            // 加载时检查是否需要自动勾选（从Excel中读取时间）
            LoadCheckBoxStateFromExcel(checkBox);
            return checkBox;
        }
        /// <summary>
        /// 从Excel加载CheckBox的状态，根据保存的时间决定是否自动勾选
        /// </summary>
        /// <param name="checkBox">要检查的CheckBox控件</param>
        private void LoadCheckBoxStateFromExcel(CheckBox checkBox)
        {
            try
            {
                // 通过 HiveExManager 获取保养时间
                DateTime? savedTime = HiveMgr.GetMaintenanceTime(checkBox.Text, isWeeklyMaintenance);

                if (savedTime.HasValue)
                {
                    TimeSpan timeDifference = DateTime.Now - savedTime.Value;

                    // 根据保养类型设置不同的时间阈值
                    int daysThreshold = isWeeklyMaintenance ? 7 : 30;

                    // 如果时间差小于阈值，则自动勾选
                    if (timeDifference.TotalDays < daysThreshold)
                    {
                        checkBox.Checked = true;
                    }
                }
            }
            catch (Exception ex)
            {
                // 如果读取时间失败，记录错误但不影响程序运行
                Console.WriteLine($"加载CheckBox状态时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 清空动态创建的控件
        /// </summary>
        private void ClearDynamicControls()
        {
            foreach (var checkBox in dynamicCheckBoxes)
            {
                if (panel1.Controls.Contains(checkBox))
                {
                    panel1.Controls.Remove(checkBox);
                    checkBox.Dispose();
                }
            }
            dynamicCheckBoxes.Clear();


        }

    }
}
