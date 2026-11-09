using AF.Common;
using AF.Data;
using AF.Data.Permission;
using AF.Station;
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

namespace AutoFrame
{
    public partial class Form_UnloadInfo : Form
    {
        private Label[,] cellsA = new Label[Global.trayTotalRow, Global.trayTotalCol];
        private Label[,] cellsB = new Label[Global.trayTotalRow, Global.trayTotalCol];
        private Label[,] cellsC = new Label[Global.trayTotalRow, Global.trayTotalCol];
        private Timer uiTimer;
        private readonly object mLock = new object();

        bool bInitA, bInitB, bInitC;
        public Form_UnloadInfo()
        {
            InitializeComponent();
        }

        private void Form_TrayInfo_Load(object sender, EventArgs e)
        {
            InitTrayUI(tableLayoutPanelA, 1);
            InitTrayUI(tableLayoutPanelB, 2);
            InitTrayUI(tableLayoutPanelC, 3);

            uiTimer = new Timer();

            uiTimer.Interval = 100;


            uiTimer.Tick += UiTimer_Tick;


            uiTimer.Start();

            SwapController.ShowStackLogEvent += SwapController_ShowStackLogEvent;
            ProgramTool.StationStateChangedEvent += OnStationStateChanged;
            OnStationStateChanged(StationState.Manual, StationMgr.State);
        }
        private void OnStationStateChanged(StationState OldState, StationState NewState)
        {
            this.BeginInvoke(new Action(() =>
            {
                if (OldState == StationState.Manual && NewState == StationState.Init)
                {
                    this.listBoxEx_SwapLog.Clear();
                }
            }));
        }
        private void SwapController_ShowStackLogEvent(string strLog, LogType type)
        {
            try
            {
                if (listBoxEx_SwapLog.Count > 3000)
                {
                    listBoxEx_SwapLog.Clear();
                }
                switch (type)
                {
                    case LogType.Log:
                        listBoxEx_SwapLog.Append(strLog, Color.White, Color.Black);
                        break;
                    case LogType.Warn:
                        listBoxEx_SwapLog.Append(strLog, Color.Yellow, Color.Black);
                        break;
                    case LogType.Error:
                        listBoxEx_SwapLog.Append(strLog, Color.Red, Color.Black);
                        break;
                    default:
                        listBoxEx_SwapLog.Append(strLog, Color.White, Color.Black);
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Show swapLog err:{ex.Message}");
            }
          
        }


        private void UiTimer_Tick( object sender, EventArgs e)
        {
            try
            {
                RefreshSwapTrayInfo();

                grb_func.Text = $"CYCLE TIME: {Global.d_CT.ToString("F2")} (s)";
                //for (int i = 0; i < Global.arr_moveID.Length; i++)
                //{
                //    groupBox_StaInfo.Controls.Find("lb_S" + (i + 1).ToString() + "MoveID", true).First().Text = Global.arr_moveID[i];
                //}

                switch_BinA.Checked = Parameter.GetBit("Enable_BinA");
                switch_BinB.Checked = Parameter.GetBit("Enable_BinB");
                switch_BinC.Checked = Parameter.GetBit("Enable_BinC");
                switch_Random.Checked = Parameter.GetBit("Enable_UnloadRandomResult");
            }
            catch 
            {

            }                 
        }

        private void RefreshSwapTrayInfo()
        {
            try
            {
                RefreshBinInfo(SwapController.binA, 1);
                RefreshBinInfo(SwapController.binB, 2);
                RefreshBinInfo(SwapController.binC, 3);
                lb_queueCount.Text = SwapController.swapQueue.Count.ToString();
                if (SwapController.CurrentPair != null)
                {
                    PairContext temp = SwapController.CurrentPair;
                    groupBox_Pair.Enabled = true;
                    groupBox_Pair.BackColor = Color.PaleGreen;
                    lb_PairCount.Text = "StackCount: " + temp.StackCount.ToString();
                    lb_PairInfo.Text = "PairInfo: " + temp.Pair.PairDesp;
                    lb_PairScore.Text = $"Score = \n" +
                        $"PairPriority: {temp.Pair.Score.PairPriority}*10000={temp.Pair.Score.PairPriority * 10000}\n" +
                        $"+CompletedOK: {temp.Pair.Score.CompleteOK}*5000={temp.Pair.Score.CompleteOK * 5000}\n" +
                        $"+CompletedNG: {temp.Pair.Score.CompleteNG}*2000={temp.Pair.Score.CompleteNG * 2000}\n" +
                        $"+FutureOK: {temp.Pair.Score.FutureOK}*100={temp.Pair.Score.FutureOK * 100}\n" +
                        $"+FutureNG: {temp.Pair.Score.FutureNG}*20={temp.Pair.Score.FutureNG * 20}\n" +
                        $"+MoveCount: {temp.Pair.Score.MoveCount}\n" +
                        $"={temp.Pair.Score.TotalScore}";
                }
                else
                {
                    ResetUI();
                }
                if (SwapController.CurrentStack != null)
                {
                    SwapStack temp = SwapController.CurrentStack;
                    groupBox_Swap.Enabled = true;
                    groupBox_Swap.Text = $"CURRENT SWAPSTACK INFO: BinOK:{SwapController.CurrentStack.BinOK.Name} - BinNG:{SwapController.CurrentStack.BinNG.Name}";
                    groupBox_Swap.BackColor = Color.LightYellow;
                    groupBox_UZ1.Enabled = temp.UZ1 != null;
                    groupBox_UZ2.Enabled = temp.UZ2 != null;
                    groupBox_UZ3.Enabled = temp.UZ3 != null;
                    groupBox_UZ4.Enabled = temp.UZ4 != null;
                    btn_StackPhase.Text = temp.Phase.ToString();
                    btn_StackPhase.BackColor = GetColorByPhase(temp.Phase);
                    rbt_UZ1HaveProduct.Visible = false;
                    rbt_UZ2HaveProduct.Visible = false;
                    rbt_UZ3HaveProduct.Visible = false;
                    rbt_UZ4HaveProduct.Visible = false;
                    foreach (var task in temp.Tasks())
                    {
                        groupBox_Swap.Controls.Find("lb_UZ" + task.VacuumId + "Source", true).First().Text = "SourceTray: " + task.Source.Name;
                        groupBox_Swap.Controls.Find("lb_UZ" + task.VacuumId + "SourceSlot", true).First().Text = "FromSlot: " + (task.SourceSlot < 0 ? task.SourceSlot.ToString() : (task.SourceSlot + 1).ToString());
                        groupBox_Swap.Controls.Find("lb_UZ" + task.VacuumId + "Target", true).First().Text = "TargetTray: " + task.Target.Name;
                        groupBox_Swap.Controls.Find("lb_UZ" + task.VacuumId + "TargetSlot", true).First().Text = "ToSlot: " + (task.TargetSlot < 0 ? task.TargetSlot.ToString() : (task.TargetSlot + 1).ToString());
                        groupBox_Swap.Controls.Find("rbt_UZ" + task.VacuumId + "HaveProduct", true).First().Visible = task.State == VacuumState.Picked;
                    }

                }
                else
                {
                    ResetUI(false, true);
                }
            }
            catch(Exception ex)
            {
                Logger.Process("下料取放站", ex.Message);
            }
           
        }

        private void RefreshBinInfo(BinInfo bin, int index)
        {
            TableLayoutPanel tb_Tray;
            GroupBox grb;
            Label lb_Count, lb_binInfo, lb_lockGantry;
            Button btn_State;
            switch (index)
            {
                case 1:
                    tb_Tray = tableLayoutPanelA;
                    grb = groupBoxA;
                    lb_Count = lb_UnloadCountA;
                    btn_State = btn_StateA;
                    lb_binInfo = lb_BinAInfo;
                    lb_lockGantry = lb_GantryUsingA;
                    break;
                case 2:
                    tb_Tray = tableLayoutPanelB;
                    grb = groupBoxB;
                    lb_Count = lb_UnloadCountB;
                    btn_State = btn_StateB;
                    lb_binInfo = lb_BinBInfo;
                    lb_lockGantry = lb_GantryUsingB;
                    break;
                case 3:
                    tb_Tray = tableLayoutPanelC;
                    grb = groupBoxC;
                    lb_Count = lb_UnloadCountC;
                    btn_State = btn_StateC;
                    lb_binInfo = lb_BinCInfo;
                    lb_lockGantry = lb_GantryUsingC;
                    break;
                default:
                    tb_Tray = tableLayoutPanelA;
                    grb = groupBoxA;
                    lb_Count = lb_UnloadCountA;
                    btn_State = btn_StateA;
                    lb_binInfo = lb_BinAInfo;
                    lb_lockGantry = lb_GantryUsingA;
                    break;
            }

            if (bin == null)
            {
                InitTrayUI(tb_Tray, 1);
                lb_Count.Text = $"Unload Tray Count: 0";
                grb.Text = $"Unload?";
                lb_lockGantry.Visible = false;
            }
            else
            {
                grb.Text = $"Unload{bin.Name}-CT:{bin.CycleTime.ToString("F2")}";
                lb_Count.Text = $"Unload Tray Count: {bin.UnloadCount}";
                btn_State.Text = bin.Status.ToString();
                btn_State.BackColor = GetColorByBinState(bin.Status);
                lb_lockGantry.Visible = bin.IsGantryUsing;
                if (bin.Status == BinStatus.NoTray)
                {
                    InitTrayUI(tb_Tray, index);
                }
                else
                {
                    grb.Enabled = true;
                    if (bin.IsPairing)
                        grb.BackColor = Color.MediumPurple;
                    else
                        grb.BackColor = Color.Gainsboro;
                    RenderTray(bin, index);
                }
                lb_binInfo.Text = $"{bin.Name}: OK={bin.OKCount}-NG={bin.NGCount}-NA={bin.EmptyCount}";
                if (bin.IsPairing)
                    lb_binInfo.BackColor = Color.Yellow;
                else
                    lb_binInfo.BackColor = Color.Transparent;
            }
        }

     
        private Color GetColorByBinState(BinStatus state)
        {
            switch (state)
            {
                case BinStatus.NoTray:
                    return Color.Gainsboro;
                case BinStatus.TranferPutTray:
                    return Color.MediumPurple;
                case BinStatus.SnapOK:
                    return Color.YellowGreen;
                case BinStatus.SnapNG:
                    return Color.Red;
                case BinStatus.Ready:
                    return Color.SpringGreen;
                case BinStatus.Processing:
                    return Color.Yellow;
                case BinStatus.ProcessCompleted:
                    return Color.Green;
                default:
                    return Color.Gainsboro;
            }
        }
        private void ResetUI(bool bResetPair = true, bool bResetStack = true)
        {
            if (bResetPair)
            {
                lb_PairCount.Text = "StackCount: Null";
                lb_PairInfo.Text = "PairInfo: Null";
                lb_PairScore.Text = "Score: 0";
                groupBox_Pair.Enabled = false;
                groupBox_Pair.BackColor = Color.WhiteSmoke;
            }

            if (bResetStack)
            {
                btn_StackPhase.Text = "Null";
                btn_StackPhase.BackColor = Color.WhiteSmoke;
                for (int i = 1; i < 5; i++)
                {
                    groupBox_Swap.Controls.Find("lb_UZ" + i + "Source", true).First().Text = "SourceTray: Null";
                    groupBox_Swap.Controls.Find("lb_UZ" + i + "SourceSlot", true).First().Text = "FromSlot: Null";
                    groupBox_Swap.Controls.Find("lb_UZ" + i + "Target", true).First().Text = "TargetTray: Null";
                    groupBox_Swap.Controls.Find("lb_UZ" + i + "TargetSlot", true).First().Text = "ToSlot: Null";
                    groupBox_Swap.Controls.Find("rbt_UZ" + i + "HaveProduct", true).First().Visible = false;
                }
                groupBox_UZ1.Enabled = false;
                groupBox_UZ2.Enabled = false;
                groupBox_UZ3.Enabled = false;
                groupBox_UZ4.Enabled = false;
                groupBox_Swap.Enabled = false;
                groupBox_Swap.Text = "CURRENT SWAPSTACK INFO";
                groupBox_Swap.BackColor = Color.WhiteSmoke;
            }                   
        }
        private Color GetColorByPhase(StackPhase phase)
        {
            switch (phase)
            {
                case StackPhase.Created:
                    return Color.LightSkyBlue;
                case StackPhase.Picking:
                    return Color.Yellow;
                case StackPhase.PickSourceCompleted:
                case StackPhase.PickTargetCompleted:
                    return Color.Chartreuse;
                case StackPhase.PlacingSource:
                case StackPhase.PlacingTarget:
                    return Color.Thistle;
                case StackPhase.PlaceSourceCompleted:
                case StackPhase.PlaceTargetCompleted:
                    return Color.Aquamarine;
                case StackPhase.Completed:
                    return Color.Lime;
                case StackPhase.Failed:
                    return Color.Red;
                default:
                    return Color.Red;
            }
        }

        //private void btn_maintenanceMode_Click(object sender, EventArgs e)
        //{
        //    if (!PermissionMgr.existsPermition(eRoles.Administrator))
        //    {
        //        MessageBox.Show("请登录管理员".txtTrans(TransType.Trans));
        //        return;
        //    }
        //    if (StationMgr.State == StationState.Auto || StationMgr.State == StationState.Init || StationMgr.State == StationState.Ready)
        //    {
        //        MessageBox.Show("请把机台停下来再操作".txtTrans(TransType.Trans));
        //        return;
        //    }

        //    new Form_MaintenanceLogin().ShowDialog();
        //    grb_func.Focus();
        //}

        private void btn_ModifyDB_Click(object sender, EventArgs e)
        {
            if (StationMgr.State == StationState.Auto)
            {
                MessageBox.Show("请先把机台暂停，再操作Db".txtTrans(TransType.Trans));
                return;
            }
            new Form_ModifyDb().ShowDialog();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            MesUploadDataModel model = new MesUploadDataModel();
            model.reportTime = DateTime.Now.ToString("yyyy-MM-dd");
            model.testStartTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            model.testEndTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            model.executionDuration = 300;
            model.inputQty = 36;
            model.totalPassQty = 30;
            model.totalFailQty = 6;
            model.itemList.Add(new DefectItem()
            {
                errorCode = "D01",
                errorDesc = "不良1",
                failQty = 1

            });
            model.itemList.Add(new DefectItem()
            {
                errorCode = "D02",
                errorDesc = "不良2",
                failQty = 2

            });
            model.itemList.Add(new DefectItem()
            {
                errorCode = "D03",
                errorDesc = "不良3",
                failQty = 3

            });
            string msg = "";
            MesMgr.SaveTestData(model, out msg);
        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            double pos =  HandleEx.GetPos(StationUnloadGantryEx.POINT.安全位, AXIS.下料取放X轴, StationMgr.GetStation("下料取放站"));
        }

        //private void switch_Bin_CheckedChanged(object sender, AntdUI.BoolEventArgs e)
        //{
        //    AntdUI.Switch sw = sender as AntdUI.Switch;
        //    bool b_IsEnable = true;
        //    switch (sw.Tag)
        //    {
        //        case 1:
        //            b_IsEnable = Parameter.GetBit("Enable_BinA");
        //            break;
        //        case 2:
        //            b_IsEnable = Parameter.GetBit("Enable_BinB");
        //            break;
        //        case 3:
        //            b_IsEnable = Parameter.GetBit("Enable_BinC");
        //            break;
        //        default:
        //            break;
        //    }
        //}

        private void InitTrayUI(TableLayoutPanel tb, int idx)
        {
            Label[,] cells = null;
            bool bIsInit = false;
            switch (idx)
            {
                case 1:
                    cells = cellsA;
                    bIsInit = bInitA;
                    break;
                case 2:
                    cells = cellsB;
                    bIsInit = bInitB;
                    break;
                case 3:
                    cells = cellsC;
                    bIsInit = bInitC;
                    break;
                default:
                    break;
            }
            if (bIsInit)
            {
                return;
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
                tb.RowStyles.Add(  new RowStyle(   SizeType.Percent, rowPercent));
            }

            for (int c = 0; c < Global.trayTotalCol; c++)
            {
                tb.ColumnStyles.Add( new ColumnStyle(   SizeType.Percent, colPercent));
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
                lb.BorderStyle =  BorderStyle.FixedSingle;
                lb.Font = new Font("Microsoft YaHei UI", 9, FontStyle.Bold);
                lb.Text = $"{i+1}\nNA";
                cells[row, col] = lb;
                tb.Controls.Add( lb, col, row);                
            }

            switch (idx)
            {
                case 1:
                    bInitA = true;
                    break;
                case 2:
                    bInitB = true;
                    break;
                case 3:
                    bInitC = true;
                    break;
                default:
                    break;
            }
        }

        private void RenderTray(BinInfo binInfo, int idx)
        {
            if (binInfo == null)
                return;
            Label[,] cells = null;
            bool bIsInit = false;
            switch (idx)
            {
                case 1:
                    cells = cellsA;
                    bIsInit = bInitA;
                    break;
                case 2:
                    cells = cellsB;
                    bIsInit = bInitB;
                    break;
                case 3:
                    cells = cellsC;
                    bIsInit = bInitC;
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

                SlotState state = binInfo.GetSlotState(i);

                switch (state)
                {
                    case SlotState.OK:

                        if (cell.Text != $"{i + 1}\nOK")
                        {
                            cell.Text = $"{i+1}\nOK";
                            cell.BackColor =   Color.LightGreen;
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

            switch (idx)
            {
                case 1:
                    bInitA = false;
                    break;
                case 2:
                    bInitB = false;
                    break;
                case 3:
                    bInitC = false;
                    break;
                default:
                    break;
            }
        }

       

        //private void button5_Click(object sender, EventArgs e)
        //{
        //    SwapController.binA = new BinInfo
        //    {
        //        Name = "BinA",
        //        DbName = "UnloadBinA",
        //        iRegion = 1,
        //        strRegion = "A",
        //        RowDis = 1,
        //        ColDis = 1,
        //        Role = BinRole.OK,
        //        Status = BinStatus.NoTray
        //    };

        //    SwapController.binB = new BinInfo
        //    {
        //        Name = "BinB",
        //        DbName = "UnloadBinB",
        //        iRegion = 2,
        //        strRegion = "B",
        //        RowDis = 1,
        //        ColDis = 1,
        //        Role = BinRole.NG,
        //        Status = BinStatus.NoTray
        //    };

        //    SwapController.binC = new BinInfo
        //    {
        //        Name = "BinC",
        //        DbName = "UnloadBinC",
        //        iRegion = 3,
        //        strRegion = "C",
        //        RowDis = 1,
        //        ColDis = 1,
        //        Role = BinRole.OK,
        //        Status = BinStatus.NoTray
        //    };

        //    button5.Text = "TestMode Actived";
        //    button5.BackColor = Color.Green;

        //    //SwapController.Start();
        //}

        //private void RandomClickEvent(object sender, EventArgs e)
        //{
        //    Button btn = (Button)sender;
        //    int idx = Convert.ToInt16(btn.Tag);
        //    Random rand = new Random();
        //    int NACount = rand.Next(3, 10);
        //    Random rd1 = new Random();
        //    switch (idx)
        //    {
        //        case 1:                                
        //            var indicate = Enumerable.Range(0, SwapController.binA.TraySlotCount).OrderBy(x => rand.Next()).ToArray();
        //            for (int i = 0; i < NACount; i++)
        //            {
        //                SwapController.binA.SetSlotState(indicate[i], SlotState.NA);
        //            }
        //            for (int i = NACount; i < SwapController.binA.TraySlotCount; i++)
        //            {
        //                Enum.TryParse<SlotState>(rd1.Next(1, 3).ToString(), out SlotState state);
        //                SwapController.binA.SetSlotState(indicate[i], state);
        //            }
        //            SwapController.binA.SetStatus(BinStatus.TranferPutTray);
        //            SwapController.binA.RefreshCount();
        //            break;
        //        case 2:
        //            indicate = Enumerable.Range(0, SwapController.binB.TraySlotCount).OrderBy(x => rand.Next()).ToArray();
        //            for (int i = 0; i < NACount; i++)
        //            {
        //                SwapController.binB.SetSlotState(indicate[i], SlotState.NA);
        //            }
        //            for (int i = NACount; i < SwapController.binB.TraySlotCount; i++)
        //            {
        //                Enum.TryParse<SlotState>(rd1.Next(1, 3).ToString(), out SlotState state);
        //                SwapController.binB.SetSlotState(indicate[i], state);
        //            }
        //            SwapController.binB.SetStatus(BinStatus.TranferPutTray);
        //            SwapController.binB.RefreshCount();
        //            break;
        //        case 3:
        //            indicate = Enumerable.Range(0, SwapController.binC.TraySlotCount).OrderBy(x => rand.Next()).ToArray();
        //            for (int i = 0; i < NACount; i++)
        //            {
        //                SwapController.binC.SetSlotState(indicate[i], SlotState.NA);
        //            }
        //            for (int i = NACount; i < SwapController.binC.TraySlotCount; i++)
        //            {
        //                Enum.TryParse<SlotState>(rd1.Next(1, 3).ToString(), out SlotState state);
        //                SwapController.binC.SetSlotState(indicate[i], state);
        //            }
        //            SwapController.binC.SetStatus(BinStatus.TranferPutTray);
        //            SwapController.binC.RefreshCount();
        //            break;
        //        default:
        //            break;
        //    }
        //}

        //private void ReadyClickEvent(object sender, EventArgs e)
        //{
        //    Button btn = (Button)sender;
        //    int idx = Convert.ToInt16(btn.Tag);

        //    switch (idx)
        //    {
        //        case 1:
        //            SwapController.binA.SetStatus(BinStatus.Ready);
        //            break;
        //        case 2:
        //            SwapController.binB.SetStatus(BinStatus.Ready);
        //            break;
        //        case 3:
        //            SwapController.binC.SetStatus(BinStatus.Ready);
        //            break;
        //        default:
        //            break;
        //    }
        //}
    }
}
