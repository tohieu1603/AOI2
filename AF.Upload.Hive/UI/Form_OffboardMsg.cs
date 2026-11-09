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
    public partial class Form_OffboardMsg : HiveBaseForm
    {
        public static bool thisshowed = false;
        internal event closeForm closeForm_resume;
        private int count = 0;

        private readonly TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();

        public Task<bool> UserResponseTask => _tcs.Task;
        public Form_OffboardMsg()
        {
            InitializeComponent();
            //this.FormBorderStyle = FormBorderStyle.None;
            thisshowed = true;
            //autoForm();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;

            Button1.BackColor = HiveUIColors.Gray;

            //不允许操作主程序按钮(Start,Pause,Stop)
            HiveCommandProcessor.Instance.SetMainProgramButtonsEnabled(false);

            tableLayoutPanel1.BackColor = HiveUIColors.Purple;
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

            // 确保窗体关闭时总是完成Task
            if (!_tcs.Task.IsCompleted)
                _tcs.SetResult(false);
        }

        private void Form_OperationMsg_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
            this.Activate();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            count++;
            if(count>=30)
            {
                count = 0;
                this.Close();
            }
        }
       
    //private void autoForm()
    //{
    //    int formWidth = this.Width;
    //    int formHeight = this.Height;
    //    int formLeft = this.Left;
    //    int formTop = this.Top;

    //    if (Form_OffboardMsg.)//判断界面是否显示
    //    {

    //    }
    //    else
    //    {

    //    }
    //}

}
}
