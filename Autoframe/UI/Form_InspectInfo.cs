using AF.Data;
using Autoframe;
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

namespace AutoFrame
{
    public partial class Form_InspectInfo : Form
    {
        private Label[,] cellsS1 = new Label[Global.trayTotalRow, Global.trayTotalCol];
        private Label[,] cellsS2 = new Label[Global.trayTotalRow, Global.trayTotalCol];
        private Label[,] cellsS3 = new Label[Global.trayTotalRow, Global.trayTotalCol];
        private Label[,] cellsS4 = new Label[Global.trayTotalRow, Global.trayTotalCol];
        private Label[,] cellsS5 = new Label[Global.trayTotalRow, Global.trayTotalCol];
        private Label[,] cellsS6 = new Label[Global.trayTotalRow, Global.trayTotalCol];
        private Timer uiTimer;
        private string[] arr_moveId = new string[6];
        private Button[] ListButton;

        public Form_InspectInfo()
        {
            InitializeComponent();
        }

        private void Form_InspectInfo_Load(object sender, EventArgs e)
        {
            for (int i = 1; i < 7; i++)
            {
                InitTrayUI(i);
            }

            uiTimer = new Timer();

            uiTimer.Interval = 100;


            uiTimer.Tick += UiTimer_Tick;


            uiTimer.Start();

            ListButton = new Button[] { button_loading, button_wait1, button_lazer, button_wait2, button_aoi1, button_wait3, button_aoi2, button_wait4, button_aoi3, button_wait5, button_unload, button_tranfer };
            timer_monitormover.Start();

        }

        private void UiTimer_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < arr_moveId.Length; i++)
            {
                if (arr_moveId[i] != Global.arr_moveID[i])
                {
                    InitTrayUI(i + 1);
                    arr_moveId[i] = Global.arr_moveID[i];
                }
                else
                {
                    RenderTray(i + 1, arr_moveId[i]);
                }
            }
            grp_S1.Text = $"S1-INPUT-MoveID:{arr_moveId[0]}";
            grp_S2.Text = $"S2-LASER-MoveID:{arr_moveId[1]}";
            grp_S3.Text = $"S3-AOI1-MoveID:{arr_moveId[2]}";
            grp_S4.Text = $"S4-AOI2-MoveID:{arr_moveId[3]}";
            grp_S5.Text = $"S5-AOI3-MoveID:{arr_moveId[4]}";
            grp_S6.Text = $"S6-OUTPUT-MoveID:{arr_moveId[5]}";
        }

        private void InitTrayUI(int idx)
        {
            Label[,] cells = null;
            TableLayoutPanel tb = null;
            switch (idx)
            {
                case 1:
                    cells = cellsS1;
                    tb = tableLayoutPanel_S1;
                    break;
                case 2:
                    cells = cellsS2;
                    tb = tableLayoutPanel_S2;
                    break;
                case 3:
                    cells = cellsS3;
                    tb = tableLayoutPanel_S3;
                    break;
                case 4:
                    cells = cellsS4;
                    tb = tableLayoutPanel_S4;
                    break;
                case 5:
                    cells = cellsS5;
                    tb = tableLayoutPanel_S5;
                    break;
                case 6:
                    cells = cellsS6;
                    tb = tableLayoutPanel_S6;
                    break;
                default:
                    break;
            }
          
            tb.Controls.Clear();
            tb.RowStyles.Clear();
            tb.ColumnStyles.Clear();
            tb.ColumnCount = Global.trayTotalCol;
            tb.RowCount = Global.trayTotalRow;

            float rowPercent = 100f / Global.trayTotalRow;

            float colPercent = 100f / Global.trayTotalCol;

            for (int r = 0; r < Global.trayTotalRow; r++)
            {
                tb.RowStyles.Add(new RowStyle(SizeType.Percent, rowPercent));
            }

            for (int c = 0; c < Global.trayTotalCol; c++)
            {
                tb.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, colPercent));
            }

            for (int i = 0; i < Global.trayTotalCol * Global.trayTotalRow; i++)
            {
                bool bIsASide = Parameter.GetString("MachineSide") == "A";
                int row = bIsASide ? Global.trayTotalRow - 1 - i % Global.trayTotalRow : i % Global.trayTotalRow;
                int col = bIsASide ? i / Global.trayTotalRow : Global.trayTotalCol - 1 - i / Global.trayTotalRow;

                int uiRow = Global.trayTotalRow - 1 - i % Global.trayTotalRow;
                int uiCol = i % Global.trayTotalRow;
                Label lb = new Label();
                lb.Dock = DockStyle.Fill;
                lb.BackColor = Color.LightGray;
                lb.TextAlign = ContentAlignment.MiddleCenter;
                lb.BorderStyle = BorderStyle.FixedSingle;
                lb.Font = new Font("Microsoft YaHei UI", 9, FontStyle.Bold);
                lb.Text = idx == 2 ? $"{i + 1}\nNull" : $"{i + 1}\nNA";
                cells[row, col] = lb;
                tb.Controls.Add(lb, col, row);
            }
        }

        private void RenderTray(int idx, string moveId)
        {
            Label[,] cells = null;
            bool bIsInit = false;
            switch (idx)
            {
                case 1:
                    cells = cellsS1;
                    break;
                case 2:
                    cells = cellsS2;
                    break;
                case 3:
                    cells = cellsS3;
                    break;
                case 4:
                    cells = cellsS4;
                    break;
                case 5:
                    cells = cellsS5;
                    break;
                case 6:
                    cells = cellsS6;
                    break;
                default:
                    break;
            }
            for (int i = 0; i < Global.trayTotalCol * Global.trayTotalRow; i++)
            {
                bool bIsASide = Parameter.GetString("MachineSide") == "A";
                int row = bIsASide ? Global.trayTotalRow - 1 - i % Global.trayTotalRow : i % Global.trayTotalRow;
                int col = bIsASide ? i / Global.trayTotalRow : Global.trayTotalCol - 1 - i / Global.trayTotalRow;


                Label cell = cells[row, col];

                string strRet = RedisHelperEx.Instance.HashGet(moveId, $"S{idx}_{i + 1}");

                if (idx == 2)
                {
                    if (!double.TryParse(strRet, out double dRet))
                        dRet = 0;

                    cell.Text = $"{i + 1}\n{strRet}";
                    cell.BackColor = dRet > 0 ? Color.LightGreen : Color.LightGray;
                }
                else
                {
                    if (!Enum.TryParse(strRet, out SlotState state))
                        state = SlotState.NA;

                    switch (state)
                    {
                        case SlotState.OK:

                            if (cell.Text != $"{i + 1}\nOK")
                            {
                                cell.Text = $"{i + 1}\nOK";
                                cell.BackColor = Color.LightGreen;
                            }
                            else
                            {
                                cell.BackColor = Color.LightGreen;
                            }
                            break;


                        case SlotState.NG:

                            if (cell.Text != $"{i + 1}\nNG")
                            {
                                cell.Text = $"{i + 1}\nNG";
                                cell.BackColor = Color.LightCoral;
                            }
                            else
                            {
                                cell.BackColor = Color.LightCoral;
                            }
                            break;

                        case SlotState.NA:

                            if (cell.Text != $"{i + 1}\nNA")
                            {
                                cell.Text = $"{i + 1}\nNA";
                                cell.BackColor = Color.LightGray;
                            }
                            else
                            {
                                cell.BackColor = Color.LightGray;
                            }
                            break;
                    }
                }
                
            }
       
        }

        private void timer_monitormover_Tick(object sender, EventArgs e)
        {
            try
            {
                for (int i = 0; i < Global.ListNameStation.Length; i++)
                {
                    string m_data = RedisHelperEx.Instance.HashGet(Global.m_RESPMonitorMover, Global.ListNameStation[i]);
                    if (m_data != null)
                    {
                        if (m_data == "Null" || m_data.Split(',')[1] == "0")
                        {
                            if(Global.ListNameStation[i].Contains("Wait"))
                                ListButton[i].Visible = false;
                            ListButton[i].Text = "N/A";
                            ListButton[i].BackColor = Color.LightGray;
                        }
                        else if (m_data.Split(',')[1] == "1")
                        {
                            if (Global.ListNameStation[i].Contains("Wait"))
                            {
                                ListButton[i].Visible = true;
                                ListButton[i].Text = m_data.Split(',')[0];
                                ListButton[i].BackColor = Color.LightGreen;
                            }
                            else
                            {
                                if(Global.ListNameStation[i].Contains("中转取放站"))
                                    ListButton[i].Text = "有料盘";
                                else
                                    ListButton[i].Text = "动子号：" + m_data.Split(',')[0];
                                ListButton[i].BackColor = Color.LightGreen;
                            }
                        }
                        else if (m_data.Split(',')[1] == "2")
                        {
                            if (Global.ListNameStation[i].Contains("Wait"))
                            {
                                ListButton[i].Visible = true;
                                ListButton[i].Text = m_data.Split(',')[0];
                                ListButton[i].BackColor = Color.LightGray;
                            }
                            else
                            {
                                if (Global.ListNameStation[i].Contains("中转取放站"))
                                    ListButton[i].Text = "无料盘";
                                else
                                    ListButton[i].Text = "动子号：" + m_data.Split(',')[0];
                                ListButton[i].BackColor = Color.LightGray;
                            }
                        }
                    }
                }
            }
            catch { }
        }

        private void button_loading_Click(object sender, EventArgs e)
        {

        }
    }
}
