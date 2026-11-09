using AF.Data.DB;
using AF.Data.Permission;
using AF.Station;
using Autoframe;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoFrame
{
    public partial class Form_MaintenanceLogin : Form
    {
        SqlLiteHelperEx myDB;

    


        public Form_MaintenanceLogin()
        {
            InitializeComponent();

        }  

        private void Form_MaintenanceLogin_Load(object sender, EventArgs e)
        {
            string strpath = Path.Combine(Application.StartupPath, "db", "rbac.db");
            myDB = new SqlLiteHelperEx(strpath);
            tb_CardID.Focus();
        }

        bool bChanged = false;
        private void tb_CardID_TextChanged(object sender, EventArgs e)
        {

            if (tb_CardID.Text.Length >= 10)
            {
                string sqlCmd = "select userName from tbUser";
                List<object[]> obj = new List<object[]>();
                myDB.ExecuteReader(sqlCmd, ref obj);

                if (obj.Count > 0)
                {
                    for (int i = 0; i < obj.Count; i++)
                    {
                        if (tb_CardID.Text == obj[i][0].ToString())
                        {
                            if (StationMgr.State == AF.Common.StationState.Auto || StationMgr.State == AF.Common.StationState.Init)
                            {
                                MessageBox.Show("请把机台停掉，再操作维修模式");
                                return;
                            }

                            if (!Global.b_IsMaintenanceMode)
                            {
                                Global.b_IsMaintenanceMode = true;
                                Global.strMaintenanceID = tb_CardID.Text;
                            }
                            else
                            {
                                Global.b_IsMaintenanceMode = false;
                                Global.strMaintenanceID = "";
                            }

                          

                            MaintenanceHelper.PushlishEvent(Global.b_IsMaintenanceMode);
                            bChanged = true;
                            tb_CardID.Clear();
                            this.Close();
                        }
                    }
                }
            }
        }

        private void btn_confirm_Click(object sender, EventArgs e)
        {
            string g = tb_CardID.Text;
        }
    }
}
