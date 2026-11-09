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
using static AF.Upload.Hive.Form_ResumeMsg;
using static AF.Upload.Hive.HiveTool;
using static System.Windows.Forms.AxHost;

namespace AF.Upload.Hive
{
    public partial class Form_Login : HiveBaseFormText
    {
        public static bool thisshowed = false;
        bool Textbadgechange = false;
        int nSelectedIndex = 0;


        public Form_Login(int selectedIndex)
        {
            InitializeComponent();
            this.TopMost = true;
            //this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.FormBorderStyle = FormBorderStyle.None;
            thisshowed = true;

            nSelectedIndex = selectedIndex;
            //不允许操作主程序按钮(Start,Pause,Stop)
            HiveCommandProcessor.Instance.SetMainProgramButtonsEnabled(false);


            tableLayoutPanel1.Visible = false;

        }

        private void Button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form_MsgWithRepair_FormClosed(object sender, FormClosedEventArgs e)
        {
            thisshowed = false;
            //允许操作主程序按钮(Start,Pause,Stop)

            HiveCommandProcessor.Instance.SetMainProgramButtonsEnabled(true);
        }

        private void Form_MsgWithRepair_Load(object sender, EventArgs e)
        {
            this.Activate();
            customComboBox1.SelectedIndex = nSelectedIndex;
            customComboBox1.BackColor = HiveUIColors.Gray;
            TextBox_badgeTip.BackColor = HiveUIColors.Gray;
            button_Login.BackColor = HiveUIColors.Gray;
            TextBox_badgeTip.ReadOnly = true;
            TextBox_badge.ReadOnly = false;
            //TextBox_刷卡.ReadOnly = true;
            TextBox_badge.BackColor = HiveUIColors.Gray;//灰色 R = 214 ; G = 214 ; B = 214

            TextBox_badge.Focus();
            this.ActiveControl = TextBox_badge;

        }

        private void customComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        public bool badge_confirm()
        {
            if (true)
            {
                return true;
            }
            return false;
        }
        private async void TextBox_badge_TextChanged(object sender, EventArgs e)
        {
            if (Textbadgechange)
            {
                return;
            }
            if (TextBox_badge.Text.EndsWith("\r\n"))
            {
                button_Login_Click(null, null);
            }
        }

        private async void button_Login_Click(object sender, EventArgs e)
        {

            if (Textbadgechange)
            {
                return;
            }




            //操作员只读模式 OP Read only// Security.ChangeOpMode("1");//L1-L9
            //维护人员 Maintenance// Security.ChangeFaeMode("1");//L1\L2\L3\L6\L9
            //设备厂商 Integrator// Security.ChangeEngMode("1");//L1\L2\L3
            //管理员 Administrator// Security.ChangeEngMode("1");//L3


            // 获取用户选择的权限等级
            string selectedPermission = customComboBox1.Text;
            if (selectedPermission == "操作员只读模式 OP Read only")
            {
                HiveMgr.SwitchPermission(selectedPermission);
                return;
            }
            // 验证输入
            if (string.IsNullOrWhiteSpace(selectedPermission))
            {
                //MessageBox.Show("请先选择要登录的权限等级");
                return;
            }

            string badge = TextBox_badge.Text.Trim('\r', '\n');
            if (string.IsNullOrWhiteSpace(badge))
            {
                //MessageBox.Show("请输入卡号");
                return;
            }

            HiveUserInfo hiveUserInfo = new HiveUserInfo() { CardTagid = badge };
            for (int i = 0; i < 5; i++)
            {
                if (bCommError)
                {
                    break;
                }



                if (HiveMgr.GetHiveUserInfo?.Invoke(ref hiveUserInfo) ?? false && !string.IsNullOrWhiteSpace(hiveUserInfo.Name))
                {
                    TextBox_badgeTip.Text = $"badge info send:{hiveUserInfo.strSend}\r\n, MES info received:{hiveUserInfo.strReceive}\r\n. You may now input units.";
                    TextBox_badge.BackColor = HiveUIColors.Green;//绿
                    bCommError = false;
                    break;
                }
                bCommError = true;
                tableLayoutPanel1.Visible = true;
                TextBox_badgeTip.Text = $"badge info send:{hiveUserInfo.strSend}\r\n, MES inforeceived:{hiveUserInfo.strReceive}\r\n.Access denied.";
                TextBox_badge.BackColor = HiveUIColors.Red;//红
            }

            if (!bCommError)//Mes通讯成功，
            {
                Textbadgechange = true;



                TextBox_Name.Text = hiveUserInfo.Name;
                TextBox_Vendor.Text = hiveUserInfo.Vendor;
                TextBox_Level.Text = hiveUserInfo.Level;

                // 解析查询到的Level，移除"L"前缀并转换为数字
                int userLevel = 0;
                if (hiveUserInfo.Level.StartsWith("L") && int.TryParse(hiveUserInfo.Level.Substring(1), out userLevel))
                {
                    // 检查用户是否有权限切换到所选权限
                    if (CheckPermissionAccess(userLevel, selectedPermission))
                    {
                        // 切换到所选权限
                        HiveMgr.SwitchPermission(selectedPermission);
                        //MessageBox.Show($"权限切换成功！\n用户: {sfcData.Name}\n等级: {sfcData.Level}\n权限: {selectedPermission}", "权限切换成功");
                    }
                    else
                    {
                        //MessageBox.Show($"权限切换失败！\n用户等级 {sfcData.Level} 没有 {selectedPermission} 权限", "权限不足");
                    }
                }
                else
                {
                    //MessageBox.Show("无效的权限等级: " + sfcData.Level);
                }

            }
            else//Mes失败，用本地密码
            {
                if (string.IsNullOrWhiteSpace(blackBorderTextBox1.Text))
                {
                    //MessageBox.Show("请输入密码");
                    return;
                }
                if (blackBorderTextBox1.Text == "123")//读取本地配置密码文件
                {
                    HiveMgr.SwitchPermission(selectedPermission);
                }
                else
                {
                    return;
                }
            }




            Textbadgechange = false;
            await Task.Delay(3000);
            this.Close();

            //if (TextBox_badge.Text.EndsWith("\r\n"))
            //{
            //    if (badge_confirm())
            //    {
            //        Textbadgechange = true;
            //        TextBox_badgeTip.Text = "badge info sent, MES info received. You may now input units.";
            //        TextBox_badge.BackColor = HiveUIColors.Green;//绿

            //        //hive上传
            //        StateMessageNew state = new StateMessageNew();
            //        state.MachineState = 1;
            //        state.status = "repair end";
            //        //state.downtime_cause = comboBox_DownReason.SelectedItem.ToString();
            //        //state.action_taken = textBox_DownDescription.Text;

            //        state.badge = TextBox_badge.Text;

            //        IniOperation.WriteValue(HiveCommandProcessor.m_strIniFile, "LocalData", "Status", state.status);

            //        HiveMgr.ChangeState(state);


            //        tableLayoutPanel1.Visible = true;
            //        await Task.Delay(3000);



            //        Textbadgechange = false;
            //        //回复主程序
            //        if (HiveCommandProcessor.ISConnecting)
            //        {
            //            string result = "Mannual,1";
            //            HiveCommandProcessor.Instance.WriteToMain(result);
            //            HiveCommandProcessor.ISConnecting = false;
            //        }
            //        else
            //        {
            //            string sendmsg = "Mannual,1";
            //            tcp2Client.Send(sendmsg, out _);
            //        }

            //        this.Close();

            //    }
            //    else
            //    {
            //        TextBox_badgeTip.Text = "badge info sent, MES inforeceived.Access denied.";
            //        TextBox_badge.BackColor = HiveUIColors.Red;//红
            //    }
            //}
        }

        /// <summary>
        /// 检查用户是否有权限访问所选权限
        /// </summary>
        /// <param name="userLevel">用户等级</param>
        /// <param name="selectedPermission">选择的权限</param>
        /// <returns>是否有权限</returns>
        private bool CheckPermissionAccess(int userLevel, string selectedPermission)
        {
            // 根据权限表格检查权限

            //操作员只读模式 OP Read only// Security.ChangeOpMode("1");//L1-L9
            //维护人员 Maintenance// Security.ChangeFaeMode("1");//L1\L2\L3\L6\L9
            //设备厂商 Integrator// Security.ChangeEngMode("1");//L1\L2\L3
            //管理员 Administrator// Security.ChangeEngMode("1");//L3
            switch (selectedPermission)
            {
                case "操作员只读模式 OP Read only": // 操作员只读模式
                                             // 所有级别都有此权限 (L1-L9)
                    return userLevel >= 1 && userLevel <= 9;

                case "维护人员 Maintenance": // 维护人员
                                         // L1, L2, L3, L6, L9 有此权限
                    return userLevel == 1 || userLevel == 2 || userLevel == 3 || userLevel == 6 || userLevel == 9;

                case "设备厂商 Integrator": // 设备厂商
                                        // L1, L2, L3 有此权限
                    return userLevel == 1 || userLevel == 2 || userLevel == 3;

                case "管理员 Administrator": // 管理员
                                          // 只有 L3 有此权限
                    return userLevel == 3;

                default:
                    MessageBox.Show("未知的权限类型: " + selectedPermission);
                    return false;
            }
        }
    }
}
