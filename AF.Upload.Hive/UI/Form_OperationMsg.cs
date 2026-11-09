using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AF.Upload.Hive
{
    public partial class Form_OperationMsg : HiveBaseForm
    {
        public static bool thisshowed = false;
        internal event closeForm closeForm_resume;

        public Form_OperationMsg(string text1, string text2)
        {
            InitializeComponent();
            this.TopMost = true;
            //this.FormBorderStyle = FormBorderStyle.None;
            thisshowed = true;
            label1.Text = text1;
            label2.Text = text2;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;

            Button1.BackColor = HiveUIColors.Gray;

            //不允许操作主程序按钮(Start,Pause,Stop)
            HiveCommandProcessor.Instance.SetMainProgramButtonsEnabled(false);

        }

        private void Button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form_OperationMsg_FormClosed(object sender, FormClosedEventArgs e)
        {
            closeForm_resume?.Invoke();
            //允许操作主程序按钮(Start,Pause,Stop)
            thisshowed = false;

            HiveCommandProcessor.Instance.SetMainProgramButtonsEnabled(true);
        }

        private void Form_OperationMsg_Load(object sender, EventArgs e)
        {
            this.Activate();
        }
    }
}
