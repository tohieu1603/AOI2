using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoFrame.PLCExtend
{
    public partial class ModbusTestWindow : Form
    {
        public ModbusTestWindow()
        {
            InitializeComponent();
        }
        PlcHelper plcHelper = null;
        string plcIP = string.Empty;
        string plcPort = string.Empty;
        private void button1_Click(object sender, EventArgs e)
        {
            plcIP = "192.168.1.88";
            plcPort = "502";
            PlcMgrEx.GetInstance().m_dicNETIP.Clear();
            PlcMgrEx.GetInstance().m_dicNETPort.Clear();
            PlcMgrEx.GetInstance().m_dicReg_StartAdress.Clear();

            PlcMgrEx.GetInstance().m_dicNETIP.Add("0", plcIP);
            PlcMgrEx.GetInstance().m_dicNETPort.Add("0", plcPort);
            PlcMgrEx.GetInstance().m_dicReg_StartAdress.Add("0_PLC寄存器起始地址", "1000");
            plcHelper = new PlcHelper();
            
            plcHelper.ConnectPLC(0);
            if(plcHelper.IsConnected(0))
            {
                MessageBox.Show(string.Format("PLC的IP:{0}：连接失败,端口号为：{1}", plcIP, plcPort));
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            plcHelper.DisConnectPLC(0);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //写入单个寄存器的值
            plcHelper.WriteData_Int16(0, new int[] { Convert.ToInt32(txtWriteValue.Text) }, Convert.ToInt32(txtSingleRegister.Text));
        }

        private void button3_Click(object sender, EventArgs e)
        {
           lblReadValue.Text = plcHelper.ReadData_Int16(0, new int[] { Convert.ToInt32(txtSingleReadRegister.Text) }).ToString();
        }
    }
}
