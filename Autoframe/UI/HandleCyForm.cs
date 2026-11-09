using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autoframe;
using AF.Station;


namespace AutoFrame.UI
{
    public partial class HandleCyForm : Form
    {
        public HandleCyForm()
        {
            InitializeComponent();
        }
        private void switch1_CheckedChanged(object sender, AntdUI.BoolEventArgs e)
        {
            if (switch1.Checked)
            {
                DO.下料仓A上料前解锁伸出.Set(true);
                DO.下料仓A上料后解锁伸出.Set(true);
                DO.下料仓A上料前解锁缩回.Set(false);
                DO.下料仓A上料后解锁缩回.Set(false);
            }
            else
            {
                DO.下料仓A上料前解锁伸出.Set(false);
                DO.下料仓A上料后解锁伸出.Set(false);
                DO.下料仓A上料前解锁缩回.Set(true);
                DO.下料仓A上料后解锁缩回.Set(true);
            }
        }

        private void switch2_CheckedChanged(object sender, AntdUI.BoolEventArgs e)
        {
            if (switch2.Checked)
            {
                DO.下料仓B上料前解锁伸出.Set(true);
                DO.下料仓B上料后解锁伸出.Set(true);
                DO.下料仓B上料前解锁缩回.Set(false);
                DO.下料仓B上料后解锁缩回.Set(false);
            }
            else
            {
                DO.下料仓B上料前解锁伸出.Set(false);
                DO.下料仓B上料后解锁伸出.Set(false);
                DO.下料仓B上料前解锁缩回.Set(true);
                DO.下料仓B上料后解锁缩回.Set(true);
            }
        }

        private void switch3_CheckedChanged(object sender, AntdUI.BoolEventArgs e)
        {
            if (switch3.Checked)
            {
                DO.下料仓C上料前解锁伸出.Set(true);
                DO.下料仓C上料后解锁伸出.Set(true);
                DO.下料仓C上料前解锁缩回.Set(false);
                DO.下料仓C上料后解锁缩回.Set(false);
            }
            else
            {
                DO.下料仓C上料前解锁伸出.Set(false);
                DO.下料仓C上料后解锁伸出.Set(false);
                DO.下料仓C上料前解锁缩回.Set(true);
                DO.下料仓C上料后解锁缩回.Set(true);
            }
        }

        private void switch4_CheckedChanged(object sender, AntdUI.BoolEventArgs e)
        {
            if (switch4.Checked)
            {
                DO.下料仓A下料前解锁伸出.Set(true);
                DO.下料仓A下料后解锁伸出.Set(true);
                DO.下料仓A下料前解锁缩回.Set(false);
                DO.下料仓A下料后解锁缩回.Set(false);
            }
            else
            {
                DO.下料仓A下料前解锁伸出.Set(false);
                DO.下料仓A下料后解锁伸出.Set(false);
                DO.下料仓A下料前解锁缩回.Set(true);
                DO.下料仓A下料后解锁缩回.Set(true);
            }
        }

        private void switch5_CheckedChanged(object sender, AntdUI.BoolEventArgs e)
        {
            if (switch4.Checked)
            {
                DO.下料仓B下料前解锁伸出.Set(true);
                DO.下料仓B下料后解锁伸出.Set(true);
                DO.下料仓B下料前解锁缩回.Set(false);
                DO.下料仓B下料后解锁缩回.Set(false);
            }
            else
            {
                DO.下料仓B下料前解锁伸出.Set(false);
                DO.下料仓B下料后解锁伸出.Set(false);
                DO.下料仓B下料前解锁缩回.Set(true);
                DO.下料仓B下料后解锁缩回.Set(true);
            }
        }

        private void switch6_CheckedChanged(object sender, AntdUI.BoolEventArgs e)
        {
            if (switch4.Checked)
            {
                DO.下料仓C下料前解锁伸出.Set(true);
                DO.下料仓C下料后解锁伸出.Set(true);
                DO.下料仓C下料前解锁缩回.Set(false);
                DO.下料仓C下料后解锁缩回.Set(false);
            }
            else
            {
                DO.下料仓C下料前解锁伸出.Set(false);
                DO.下料仓C下料后解锁伸出.Set(false);
                DO.下料仓C下料前解锁缩回.Set(true);
                DO.下料仓C下料后解锁缩回.Set(true);
            }
        }
    }
}
