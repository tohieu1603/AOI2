using AF.File;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace AF.Upload.Hive
{
    public partial class Form_HiveOnboard : HiveBaseForm
    {
        private HiveStationInfo hiveInfo = new HiveStationInfo();        

        public Form_HiveOnboard()
        {
            InitializeComponent();
            this.TopMost = true;
            this.tableLayoutPanel1.BackColor = Color.FromArgb(81, 143, 255);//蓝
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //hive注册
            for (int i = 1; i < 14; i++)
            {
                TextBox newtextboxs = (TextBox)this.Controls.Find($"textBox{i}", true).FirstOrDefault();
                if (newtextboxs.Text == "")
                {
                    MessageBox.Show("注册数据未填满");
                    return;
                }
            }

            HiveTool.heartbeatdata.machine_sn = textBox1.Text;
            HiveTool.heartbeatdata.machine_model_number = textBox2.Text;
            HiveTool.heartbeatdata.vendor = textBox3.Text;
            HiveTool.heartbeatdata.machine_rfid = textBox4.Text;
            HiveTool.heartbeatdata.site = textBox5.Text;
            HiveTool.heartbeatdata.product = textBox6.Text;
            HiveTool.heartbeatdata.station_type = textBox7.Text;
            HiveTool.heartbeatdata.line = textBox8.Text;
            HiveTool.heartbeatdata.instance = Convert.ToInt32(textBox9.Text);
            HiveTool.heartbeatdata.building = textBox10.Text;
            HiveTool.heartbeatdata.line_type = textBox11.Text;
            HiveTool.heartbeatdata.station_name = textBox12.Text;
            HiveTool.heartbeatdata.build_type = textBox13.Text;

            Ini.WriteValue(HiveCommandProcessor.m_strVendorStationInfoIniFile, "machine_info", "machine_sn", textBox1.Text);
            Ini.WriteValue(HiveCommandProcessor.m_strVendorStationInfoIniFile, "machine_info", "machine_model_number", textBox2.Text);
            Ini.WriteValue(HiveCommandProcessor.m_strVendorStationInfoIniFile, "station_info", "vendor", textBox3.Text);
            Ini.WriteValue(HiveCommandProcessor.m_strVendorStationInfoIniFile, "machine_info", "machine_rfid", textBox4.Text);
            Ini.WriteValue(HiveCommandProcessor.m_strVendorStationInfoIniFile, "station_info", "site", textBox5.Text);
            Ini.WriteValue(HiveCommandProcessor.m_strVendorStationInfoIniFile, "station_info", "product", textBox6.Text);
            Ini.WriteValue(HiveCommandProcessor.m_strVendorStationInfoIniFile, "station_info", "station_type", textBox7.Text);
            Ini.WriteValue(HiveCommandProcessor.m_strVendorStationInfoIniFile, "station_info", "line", textBox8.Text);
            Ini.WriteValue(HiveCommandProcessor.m_strVendorStationInfoIniFile, "station_info", "instance", textBox9.Text);
            Ini.WriteValue(HiveCommandProcessor.m_strVendorStationInfoIniFile, "station_info", "building", textBox10.Text);
            Ini.WriteValue(HiveCommandProcessor.m_strVendorStationInfoIniFile, "station_info", "line_type", textBox11.Text);
            Ini.WriteValue(HiveCommandProcessor.m_strVendorStationInfoIniFile, "station_info", "station_name", textBox12.Text);
            Ini.WriteValue(HiveCommandProcessor.m_strVendorStationInfoIniFile, "station_info", "build_type", textBox13.Text);
            //string url = "http://10.0.0.2:5008/idms/v2/station/status";
            //string postUrl =HiveMgr.GetUrlEx(OnboardingStatus);
            //string jsonss = JsonConvert.SerializeObject(HiveMgr.heartbeatdata);
            //string response = "";
            //if (HiveMgr.Instance.PostData(url, jsonss, ref response))
            //{
            //    HiveMgr.Instance.ShowLog(jsonss, true, response, DataStyle.Heartbeat);
            //    Form_ResOnboard t1 = new Form_ResOnboard(HiveMgr.Instance.ConvertJsonString(jsonss));
            //    t1.StartPosition = FormStartPosition.CenterScreen;
            //    t1.TopLevel = true;
            //    t1.Show();

            //}
            //else
            //{
            //    MessageBox.Show("注册hive失败");
            //    HiveMgr.Instance.ShowLog(jsonss, false, "Registered Hive system failed", DataStyle.Heartbeat);
            //}
            MessageBox.Show("注册数据已保存本地");
        }

        private void Form_HiveOnboard_Load(object sender, EventArgs e)
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.Activate();
            HiveStationInfo hiveInfo = HiveMgr.GetHiveStationInfo?.Invoke();
            if (hiveInfo == null)
                return;

            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "SECOTE";
            textBox4.Text = "";
            textBox5.Text = hiveInfo.site;
            textBox6.Text = hiveInfo.product;
            textBox7.Text = hiveInfo.station_type;
            textBox8.Text = hiveInfo.line;
            textBox9.Text = hiveInfo.instance.ToString();
            textBox10.Text = hiveInfo.building;
            textBox11.Text = hiveInfo.line_type;
            textBox12.Text = hiveInfo.station_name;
            textBox13.Text = hiveInfo.build_type;
        }
    }
}
