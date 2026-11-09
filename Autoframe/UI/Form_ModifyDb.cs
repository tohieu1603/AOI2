using AutoFrame.DBExtend;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Autoframe
{
    public partial class Form_ModifyDb : Form
    {
        BinInfo currentBin = null;
        public Form_ModifyDb()
        {
            InitializeComponent();
        }

        private List<string> DbNames = new List<string>();

        private void Form_ModifyDb_Load(object sender, EventArgs e)
        {
            if (SwapController.binA != null)
                DbNames.Add(SwapController.binA.DbName);
            if (SwapController.binB != null)
                DbNames.Add(SwapController.binB.DbName);
            if (SwapController.binC != null)
                DbNames.Add(SwapController.binC.DbName);

            cb_DbName.DataSource = DbNames;

        }

        private void cb_DbName_SelectedIndexChanged(object sender, EventArgs e)
        {
            string[] slotStates = Enum.GetNames(typeof(SlotState));
            cb_SlotState.DataSource = slotStates;
            object[] slotNos = new object[Global.slotTotalCount];
            for (int i = 0; i < Global.slotTotalCount; i++)
            {
                slotNos[i] = i + 1;
            }

            cb_SlotNo.DataSource = slotNos;
        }

        private void btn_Modify_Click(object sender, EventArgs e)
        {
            switch (cb_DbName.SelectedIndex)
            {
                case 0:
                    currentBin = SwapController.binA;
                    break;
                case 1:
                    currentBin = SwapController.binB;
                    break;
                case 2:
                    currentBin = SwapController.binC;
                    break;
                default:
                    currentBin = null;
                    break;
            }
            if (currentBin != null && currentBin.Status == BinStatus.NoTray)
            {
                MessageBox.Show("UnloadBin no tray, not available to update!");
                return;
            }

            if (cb_DbName.Text == "" || cb_SlotNo.Text == "" || cb_SlotState.Text == "")
                return;
            if (MessageBox.Show($"Confirm update Db:{cb_DbName.Text} - Slot:{cb_SlotNo.Text} - State:{cb_SlotState.Text}","Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes)          
                return;

            if (RedisHelperEx.Instance.HashSet(cb_DbName.Text, $"S_{cb_SlotNo.Text}", cb_SlotState.Text))
            {
                MessageBox.Show("Update successfully");
                currentBin.SetMatrixBySQL();
            }               
            else
                MessageBox.Show("Update failed");
        }
    }
}
