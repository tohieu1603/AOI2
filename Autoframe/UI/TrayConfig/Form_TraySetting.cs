using AF.Common;
using Autoframe;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AntdUI;
using AF.Common.UI;

namespace Autoframe
{
    public partial class Form_TraySetting : Form
    {
        #region 所有控件定义
        //private ComboBox cbx_Style;
      //  private Label lbl_Row, lbl_Column;
        //private NumericUpDown nud_Row, nud_Column;
        //private Button btn_Apply, btn_Cancel;
        #endregion

        private TrayGridConfig _currentConfig;

        public Form_TraySetting()
        {
            InitializeComponent();
            // 基础窗体属性
            Text = "Tray托盘规格配置";
            //Size = new System.Drawing.Size(420, 260);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ControlBox = false;
            AF.Data.DataGroup.GetGroupData("支持料盘型号", "型号1");


            cbx_Style.Items.AddRange(new[] { AF.Data.DataGroup.GetGroup("PKK4-WIDE").m_strGroupName,
                AF.Data.DataGroup.GetGroup("4CS4TELE").m_strGroupName,
                AF.Data.DataGroup.GetGroup("R13").m_strGroupName,
                AF.Data.DataGroup.GetGroup("料盘型号5x5").m_strGroupName,
                AF.Data.DataGroup.GetGroup("料盘型号4x4").m_strGroupName,
             });
            BindControlEvents();
            // 加载XML配置
            LoadConfigData();
            this.Shown += (s, e) =>
            {
                System.Drawing.Point trayScreenPos = this.DesktopLocation;
                int targetY = trayScreenPos.Y + this.Height + 10;
                Form_Splash.Instance.Invoke(new Action(() => {
                    Form_Splash.Instance.Location = new System.Drawing.Point(Form_Splash.Instance.Location.X, targetY);
                }));

            };
        }


        /// <summary>绑定所有控件事件</summary>
        private void BindControlEvents()
        {
            cbx_Style.SelectedIndexChanged += Cbx_Style_SelectedIndexChanged;
            btn_Save.Click += Btn_Save_Click;
        }

        /// <summary>加载XML配置赋值控件</summary>
        private void LoadConfigData()
        {
            _currentConfig = TrayGridConfig.LoadFromXml();
            lbl_Row.Text = _currentConfig.RowCount.ToString();
            lbl_Column.Text = _currentConfig.ColumnCount.ToString();
            lbl_RowGap.Text = _currentConfig.RowGap.ToString("F2");
            lbl_ColumnGap.Text = _currentConfig.ColumnGap.ToString("F2");
            MatchComboFromRowCol(_currentConfig.TrayTypeName, _currentConfig.RowCount, _currentConfig.ColumnCount);
        }

        /// <summary>行列匹配下拉选项</summary>
        private void MatchComboFromRowCol(string name,int row, int col)
        {
            cbx_Style.SelectedIndex = -1;
           // if (row == col)
            {
                string key = name;
                // 遍历比对字符串，比Contains更稳
                foreach (object item in cbx_Style.Items)
                {
                    string itemStr = item as string;
                    if (itemStr == key)
                    {
                        cbx_Style.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        /// <summary>下拉选择规格自动回填行列</summary>
        private void Cbx_Style_SelectedIndexChanged(object sender, EventArgs e)
        {
            object selectedObj = cbx_Style.SelectedItem;
            string sel = selectedObj as string;
            if (sel == null)
                return;
            string strRow =  AF.Data.DataGroup.GetGroupData(sel, "料盘行数").m_dbOffset.ToString();
            string strColumn = AF.Data.DataGroup.GetGroupData(sel, "料盘列数").m_dbOffset.ToString();
            string strRowGap = AF.Data.DataGroup.GetGroupData(sel, "料盘行间距").m_dbOffset.ToString();
            string strColumnGap = AF.Data.DataGroup.GetGroupData(sel, "料盘列间距").m_dbOffset.ToString();
            int r = Convert.ToInt32(strRow);
            int c = Convert.ToInt32(strColumn);
            lbl_Row.Text = r.ToString();
            lbl_Column.Text = c.ToString();
            lbl_RowGap.Text = strRowGap;
            lbl_ColumnGap.Text = strColumnGap;
            tsvTrayDisplay.TrayColCount = c;
            tsvTrayDisplay.TrayRowCount = r;
        }

        /// <summary>保存写入XML</summary>
        private void Btn_Save_Click(object sender, EventArgs e)
        {
            _currentConfig.TrayTypeName = cbx_Style.Text;
            _currentConfig.RowCount = Convert.ToInt32(lbl_Row.Text);
            _currentConfig.ColumnCount = Convert.ToInt32(lbl_Column.Text);
            _currentConfig.RowGap = Convert.ToDouble(lbl_RowGap.Text);
            _currentConfig.ColumnGap = Convert.ToDouble(lbl_ColumnGap.Text);
            Global.trayTotalRow = _currentConfig.RowCount;
            Global.trayTotalCol = _currentConfig.ColumnCount;
            string dataGroupName = cbx_Style.SelectedItem as string;
            Global.trayRowGap = AF.Data.DataGroup.GetGroupData(dataGroupName, "料盘行间距").m_dbOffset.D;
            Global.trayColGap = AF.Data.DataGroup.GetGroupData(dataGroupName, "料盘列间距").m_dbOffset.D;
            Global.TrayID = AF.Data.DataGroup.GetGroupData(dataGroupName, "TrayID").m_dbStandardValue.I;
            Global.trayTypeName = cbx_Style.Text;
            _currentConfig.SaveToXml();
            MatchComboFromRowCol(dataGroupName,_currentConfig.RowCount, _currentConfig.ColumnCount);
            AntdUI.Message.success(this, "配置已保存至 TrayConfig.xml");
            //MessageBox.Show("配置已保存至 TrayConfig.xml", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
            this.Shown -= (s, e1) => { };
        }

        /// <summary>对外暴露最终配置</summary>
        public TrayGridConfig GetResultConfig() => _currentConfig;

    }
}
