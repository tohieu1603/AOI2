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
    public partial class Form_ResOnboard : HiveBaseForm
    {
        public Form_ResOnboard(string json)
        {
            InitializeComponent();
            textBox1.Text = json;
            this.TopMost = true;
        }

        private void Form_ResOnboard_Load(object sender, EventArgs e)
        {
            this.Activate();
            textBox1.SelectionStart = textBox1.Text.Length;
            textBox1.SelectionLength = 0;
            textBox1.ReadOnly = true;
           
        }
    }
}
