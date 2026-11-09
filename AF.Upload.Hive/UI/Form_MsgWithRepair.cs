using AF.File;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AF.Upload.Hive
{
    public partial class Form_MsgWithRepair : HiveBaseFormText
    {
        public static bool thisshowed = false;
        //索引本地配置文件
        string RepiarData = "";
        // 动态消息控件列表
        private List<HiveMessageControl> messageControls = new List<HiveMessageControl>();

        internal event closeForm closeForm_resume;
        public Form_MsgWithRepair(string tips, string message)
        {
            InitializeComponent();
            this.TopMost = true;
            //this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.FormBorderStyle = FormBorderStyle.None;
            thisshowed = true;

            if (tips.Contains("维修")) //warning弹窗跳出来的
            {
                string Errormsg = Ini.GetStringValue(HiveCommandProcessor.m_strIniFile, "LocalData", "ErrorMsg", "");
                ErrorCode error = HiveMgr.ErrorMsg(Errormsg);
                RepiarData = error.Repair_Troubleshooting;
            }
            else //operation弹窗跳出来的
            {
                RepiarData = "1. 上下输送机链条上涂抹高温润滑脂，检查上下输送机工作是否顺畅;2. 打开炉盖后检查内部检查设备内部的是否有异物/灰尘颗粒等，如果有，请清用无尘布;3. 定期检查HEPA过滤器使用状态。当更换HEPA 过滤器后，也需清洁炉子的内外部相关位置";
            }

            //不允许操作主程序按钮(Start,Pause,Stop)
            HiveCommandProcessor.Instance.SetMainProgramButtonsEnabled(false);


        }

        private void Button1_Click(object sender, EventArgs e)
        {
            this.Close();
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
            this.panel1.BackColor = this.panel2.BackColor= lineControl1.BackColor = Color.White;

            Button_Pause.BackColor=Button_Stop.BackColor = HiveUIColors.Gray;

            // 清除可能存在的旧控件
            ClearMessageControls();

            // 索引本地配置文件
            if (!string.IsNullOrEmpty(RepiarData))
            {
                SetupMessageControls();
            }
        }
        /// <summary>
        /// 设置消息控件
        /// </summary>
        private void SetupMessageControls()
        {
            List<string> list = RepiarData.Split(';', '；').ToList();
            int panelPadding = 5; // 改用固定间距，更可控
            int currentY = panelPadding;


            // 创建维修步骤消息控件
            for (int i = 0; i < list.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(list[i]))
                    continue;

                var messageControl = CreateMessageControl();

                if (list[i].Contains(':') || list[i].Contains('：'))
                {
                    string[] parts = list[i].Split(new char[] { ':', '：' }, 2);
                    string str1 = parts[0].Trim() + ":";
                    string str2 = parts.Length > 1 ? parts[1].Trim() : "";
                    messageControl.SetValue(str1, str2);
                }
                else
                {
                    messageControl.SetValue(list[i].Trim());
                }
                messageControl.Location = new Point(10, currentY);
                messageControl.Visible = true;
                // 等待布局完成后再获取准确高度
                Application.DoEvents();

                // 更新Y坐标，考虑控件实际高度和间距
                currentY += messageControl.Height + 5;
            }

            // 添加更多信息控件
            if (messageControls.Count > 0)
            {
                var moreInfoControl = CreateMessageControl();
                string Errormsg = Ini.GetStringValue(HiveCommandProcessor.m_strIniFile, "LocalData", "ErrorMsg", "");
                ErrorCode error = HiveMgr.ErrorMsg(Errormsg);
                moreInfoControl.SetValue("图片请参考：", "CGS2_SECOTE_"+ error.Code+".jpg");
                moreInfoControl.Location = new Point(10, currentY);
                moreInfoControl.Visible = true;


                // 更新Y坐标，考虑控件实际高度和间距
                currentY += moreInfoControl.Height + 5;
                var moreInfoControlex = CreateMessageControl();
                moreInfoControlex.SetValue("更多详细内容，请参考", "D:\\Hive\\SOP");
                moreInfoControlex.Location = new Point(10, currentY);
                moreInfoControlex.Visible = true;
                // 等待布局完成
                Application.DoEvents();
            }
        }

        /// <summary>
        /// 创建消息控件
        /// </summary>
        private HiveMessageControl CreateMessageControl()
        {
            var messageControl = new HiveMessageControl
            {
                Width = panel1.Width - 25, // 留出滚动条空间
                AutoSizeControl = true,    // 启用自动调整大小
                MessageFont = new Font("微软雅黑", 9f),
                LinkFont = new Font("微软雅黑", 9f),
                MessageColor = Color.Black,
                LinkColor = Color.Black,
                Visible = false,
                VerticalSpacing = 8,        // 增加垂直间距确保文本完全显示
                 HorizontalSpacing = 0  // 增加水平间距
            };

            panel1.Controls.Add(messageControl);
            messageControls.Add(messageControl);

            return messageControl;
        }

        /// <summary>
        /// 清除所有消息控件
        /// </summary>
        private void ClearMessageControls()
        {
            foreach (var control in messageControls)
            {
                if (panel1.Controls.Contains(control))
                {
                    panel1.Controls.Remove(control);
                    control.Dispose();
                }
            }
            messageControls.Clear();
        }

        /// <summary>
        /// 窗体大小改变时调整控件
        /// </summary>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            // 延迟调整布局，确保尺寸已稳定
           System.Windows.Forms.Timer delayTimer = new System.Windows.Forms.Timer();
            delayTimer.Interval = 100;
            delayTimer.Tick += (s, args) =>
            {
                delayTimer.Stop();
                delayTimer.Dispose();
                AdjustMessageControlsLayout();
            };
            delayTimer.Start();
        }

        /// <summary>
        /// 调整消息控件布局
        /// </summary>
        private void AdjustMessageControlsLayout()
        {
            int currentY = 5;

            for (int i = 0; i < messageControls.Count; i++)
            {
                var control = messageControls[i];
                control.Width = panel1.Width - 25;
                control.Location = new Point(10, currentY);

                // 强制更新布局
                control.UpdateLayout();
                Application.DoEvents();

                currentY += control.Height + 5;
            }
        }

        private void Button_Pause_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Button_Stop_Click(object sender, EventArgs e)
        {
            HiveMgr.MachineStop?.Invoke();

            this.Close();
        }
    }
}
