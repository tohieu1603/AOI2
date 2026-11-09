using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AF.Common;
using System.Threading;
using System.Diagnostics;
using AF.Card;
using AF.CardEx;
using AF.Data;
using Autoframe;
using AF.Station;
using AF.File;
using AF.Data.Permission;

namespace Autoframe.UI
{
    public partial class Form_Test: Form
    {
        public Form_Test()
        {
            InitializeComponent();
        }

        private void frm_test_Load(object sender, EventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //MyRotaryFixture.rotaryTable.Rotate(true);
            //MyRotaryFixture.Update();

            //"复检站".GetStation<StationRecheck>()[StationRecheck.CONTAINER.穴位1] = new ProductData() { SN = "AA1AAAAAAAAAAAAAAA666666666699999" };
            //"复检站".GetStation<StationRecheck>()[StationRecheck.CONTAINER.穴位2] = new ProductData() { SN = "AA2" };

            //Carrier carrier = new Carrier();
            //carrier[0] = new ProductData();
            //"下料站".GetStation<StationUnload>()[StationUnload.CONTAINER.载具] = carrier;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //MyRotaryFixture.Get(RotaryFixture.下料).productData = new ProductData() { SN = "AA" };

            
            //MyRotaryFixture.rotaryTable.Reset();

            //Carrier carrier = "下料站".GetStation<StationUnload>()[StationUnload.CONTAINER.载具];
            //carrier.Current = new ProductData();
            //carrier.Next();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //MessageMgr.Show("sfsfs", "tttt", new string[] { "11", "22" }, new object[] { DI.启动, DI.复位 }, "sss", false, 10);
        }
    }
}
