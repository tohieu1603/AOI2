using AF.Common;
using AF.Data.Permission;
using AF.Data;
using AF.Station;
using AutoFrame;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Autoframe.UI
{
    public partial class Form_Auto_CustomPanel : UserControl
    {
        private object[] arrayDI = null;
        private object[] arrayDO = null;

        public Form_Auto_CustomPanel()
        {
            InitializeComponent();
        }

        public void Init(object[] arrayDI, object[] arrayDO)
        {
            this.arrayDI = arrayDI;
            this.arrayDO = arrayDO;
        }

        private void Form_Auto_CustomPanel_Load(object sender, EventArgs e)
        {
            if (arrayDI == null)
                this.groupBox_DI.Visible = false;
            else
                this.userControl_DI.Init(arrayDI, 0, Color.LightGray,SystemColors.Control);

            if (arrayDO == null)
                this.groupBox_DO.Visible = false;
            else
                this.userControl_DO.Init(arrayDO, 1, Color.LightGray, SystemColors.Control);

            ProgramTool.StationStateChangedEvent += OnStationStateChanged;
            OnStationStateChanged(StationState.Manual, StationMgr.State);
        }

        private void OnStationStateChanged(StationState oldState, StationState newState)
        {
            if (this.IsDisposed || !this.IsHandleCreated)
                return;
            this.BeginInvoke((Action)delegate
            {
                switch (newState)
                {
                    case StationState.Auto:
                    case StationState.Init:
                    case StationState.Ready:
                        this.userControl_DO.Enabled = false;
                        break;
                    case StationState.Emg:
                    case StationState.Pause:
                    case StationState.Manual:
                        this.userControl_DO.Enabled = true;
                        break;
                    default:
                        break;
                }
            });
        }

        private void btn_maintenanceMode_Click(object sender, EventArgs e)
        {
            if (!PermissionMgr.existsPermition(eRoles.Administrator))
            {
                MessageBox.Show("请登录管理员".txtTrans(TransType.Trans));
                return;
            }
            if (StationMgr.State == StationState.Auto || StationMgr.State == StationState.Init || StationMgr.State == StationState.Ready)
            {
                MessageBox.Show("请把机台停下来再操作".txtTrans(TransType.Trans));
                return;
            }

            new Form_MaintenanceLogin().ShowDialog();
            groupBox_DI.Focus();
        }
    }
}
