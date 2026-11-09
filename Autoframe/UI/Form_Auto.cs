using AF.Common;
using AF.Common.UI;
using AF.Data;
using AF.Data.Language;
using AF.Data.Permission;
using AF.Station;
using AF.Station.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static AF.Station.StationBase;
using AF.File;
using Autoframe;
using AF.Card;
using AF.Upload.Hive;
using System.Threading.Tasks;
using AF.Communicate;
using AutoFrame;
using System.ComponentModel;
using System.Collections.Concurrent;

namespace Autoframe.UI
{

    #region ═══ High-Performance DataGridView Helpers ═══

    /// <summary>
    /// Extension: Bật DoubleBuffered cho DataGridView (mặc định WinForms tắt → flicker)
    /// </summary>
    public static class DataGridViewExtensions
    {
        public static void EnableDoubleBuffered(this DataGridView dgv)
        {
            typeof(DataGridView).InvokeMember(
                "DoubleBuffered",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                null, dgv, new object[] { true });
        }
    }

    /// <summary>
    /// Circular Buffer — O(1) push, O(1) read, không GC pressure
    /// Dòng mới nhất luôn ở index 0 (hiển thị trên cùng)
    /// </summary>
    public class ProductDataRingBuffer
    {
        private readonly string[][] _buffer;
        private readonly Color[] _foreColors;
        private readonly int _capacity;
        private int _head;
        private int _count;

        public ProductDataRingBuffer(int capacity)
        {
            _capacity = capacity;
            _buffer = new string[capacity][];
            _foreColors = new Color[capacity];
            _head = 0;
            _count = 0;
        }

        public int Count => _count;

        /// <summary>
        /// Thêm 1 dòng (dòng mới nhất sẽ là index 0 khi đọc)
        /// </summary>
        public void Push(string[] values, Color foreColor)
        {
            _buffer[_head] = values;
            _foreColors[_head] = foreColor;
            _head = (_head + 1) % _capacity;
            if (_count < _capacity) _count++;
        }

        /// <summary>
        /// Lấy giá trị cell tại displayRow (0 = mới nhất), columnIndex
        /// </summary>
        public string GetCellValue(int displayRow, int col)
        {
            int idx = ((_head - 1 - displayRow) % _capacity + _capacity) % _capacity;
             if (row == null || col >= row.Length) return "";
            return row[col] ?? "";
        }

        /// <summary>
        /// Lấy ForeColor tại displayRow
        /// </summary>
        public Color GetForeColor(int displayRow)
        {
            int idx = ((_head - 1 - displayRow) % _capacity + _capacity) % _capacity;
            return _foreColors[idx];
        }

        /// <summary>
        /// Xóa toàn bộ buffer
        /// </summary>
        public void Clear()
        {
            _count = 0;
            _head = 0;
            Array.Clear(_buffer, 0, _capacity);
            Array.Clear(_foreColors, 0, _capacity);
        }
    }

    #endregion

    public partial class Form_Auto : AF.Data.Language.BaseForm
    {
        private static List<Form> formCustom = new List<Form>();
        Timer timer = new Timer();

        bool bFirstInit = false;

        // ═══ Virtual Mode + Throttle fields ═══
        private ProductDataRingBuffer _ringBuffer;
        private readonly ConcurrentQueue<Dictionary<string, string>> _pendingProducts = new ConcurrentQueue<Dictionary<string, string>>();
        private System.Windows.Forms.Timer _throttleTimer;
        private string[] _columnKeys;
        private const int REFRESH_INTERVAL_MS = 20;  // ~50fps (cực mượt, CPU < 0.5%)
        private volatile bool _hasPendingData = false;


        public Form_Auto()
        {
            InitializeComponent();
            //InitCtrlAnchor();

            bFirstInit = true;


        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            string[] strArrayDefect = ExtensionHelper.dicDecfect.Keys.Select(p => p).ToArray<string>();
            int[] intArrayCount = ExtensionHelper.dicDecfect.Values.Select(p => p).ToArray<int>();
            chartPie.Series[0].Points.AddXY(strArrayDefect, intArrayCount);
        }

        /// <summary>
        /// 添加自定义TabPage
        /// </summary>
        /// <param name="form"></param>
        public static void AddPage(params Form[] form)
        {
            foreach (var item in form)
                formCustom.Add(item);
        }

        /// <summary>
        /// 添加自定义面板
        /// </summary>
        /// <param name="arrayDI"></param>
        /// <param name="arrayDO"></param>
        public static void SetCustomPanel(object[] arrayDI = null, object[] arrayDO = null)
        {
            ucQuickPanel = new Form_Auto_CustomPanel();
            ((Form_Auto_CustomPanel)ucQuickPanel).Init(arrayDI, arrayDO);
        }

        /// <summary>
        /// 添加自定义面板
        /// </summary>
        /// <param name="uc">自定义</param>
        public static void SetCustomPanel(UserControl uc)
        {
            ucQuickPanel = uc;
        }

        private static UserControl ucQuickPanel = null;

        void LoadCustom()
        {
            if(ucQuickPanel != null)
            {
                ucQuickPanel.Dock = DockStyle.Fill;
                this.tableLayoutPanel_Main.Controls.Add(ucQuickPanel, 0, 2);
            }
            else
            {
                this.tableLayoutPanel_Main.SetRowSpan(this.dataGridViewProductDataShow, 3);
            }

            if (formCustom == null)
                return;
            if (formCustom.Count < 1)
                return;

            foreach (var form in formCustom)
            {
                TabPage tabPage = new TabPage();
                tabPage.AutoScroll = true;
                tabPage.Name = form.Text;
                tabPage.Text = form.Text;
                form.TopLevel = false;
                form.FormBorderStyle = FormBorderStyle.None;
                form.Dock = DockStyle.Fill;
                tabPage.Controls.Add(form);
                this.tabControl_Auto.Controls.Add(tabPage);
                form.Show();
            }
        }

        private void Form_Auto_Load(object sender, EventArgs e)
        {
            //AddPage(new Form_InspectInfo(), new Form_UnloadInfo());
            LoadCustom();
            StationMgr.Stop();
            this.logGroup1.Init();

            ProgramTool.StationStateChangedEvent += OnStationStateChanged;
            OnStationStateChanged(StationState.Manual, StationMgr.State);

            ProductConfig.StoreDataChangedEvent += InitProductDataGridViewHead;            
            InitProductDataGridViewHead();
            ProductConfig.ShowProductDataEvent += OnShowProductData;
            UpdateProductCounter();
            ProductConfig.UpdateCarrierCounter += OnUpdateCarrierCounter;
            OnUpdateCarrierCounter();

            //Parameter.UpdateParameter += OnUpdateParameter;
            //OnUpdateParameter();
            

            panel_IoSummary.ContextMenu = new ContextMenu();
            MenuItem clearItem = new MenuItem("数据清空");
            clearItem.Click += button_clear_Click;
            panel_IoSummary.ContextMenu.MenuItems.Add(clearItem);

            this.label_MachineName.Text = Ini.GetStringValue(Program.g_IniFile, "Info", "Machine", "");
            this.lb_SiteValue.Text = Ini.GetStringValue(Program.g_IniFile, "Info", "Site", "");
            this.lb_SWVersionValue.Text = Form_Main.SoftwareVersion;
            this.lb_MSPathValue.Text = Assembly.GetExecutingAssembly().Location;
            string hashCode = ProgramTool.GetFileHash(Assembly.GetExecutingAssembly().Location);
            if (!string.IsNullOrEmpty(hashCode) && hashCode.Length > 6)
                this.lb_MSHashValue.Text = hashCode/*.Substring(0, 6)*/;

            if (HiveMgr.Enable)
            {
                HiveMgr.GetHiveStationInfo += OnGetHiveStationInfo;
                HiveMgr.GetHiveUserInfo += OnGetHiveUserInfo;
            }
               

            LanguageMgr.Instance.eventChangeLanguage += OnChangeLanguage;

            if (bFirstInit)
            {
                bFirstInit = false;
                timer.Interval = 1000;
                timer.Tick += Timer_Tick;
                timer.Start();
            }

            // ======= Throttle Timer: ~30fps flush, real-time ======= 
            _throttleTimer = new System.Windows.Forms.Timer();
            _throttleTimer.Interval = REFRESH_INTERVAL_MS;
            _throttleTimer.Tick += ThrottleFlush;
            _throttleTimer.Start();
           
        }

        private void OnUpdateCarrierCounter()
        {
            if (this.IsDisposed || !this.IsHandleCreated)
                return;
            this.BeginInvoke((Action)delegate
            {
                this.label_CarrierCount.Text = SystemIO.CarrierCount.ToString();
            });
        }

        //private void OnUpdateParameter()
        //{
        //    this.dataGridView_CriticalParam.Rows.Clear();
        //    foreach (var item in Parameter.GetKeyParameters())
        //    {
        //        this.dataGridView_CriticalParam.Rows.Add(item.m_strDescribe.txtTrans(TransType.Trans), item.m_strValue, item.m_strMin, item.m_strMax);
        //    }
        //}

        private void OnChangeLanguage()
        {
            //OnUpdateParameter();
            this.logGroup1.OnChangeLanguage();
            foreach (KeyValuePair<string, ProductConfig.SaveOption> keyValuePair in ProductConfig.Instance.Option)
            {
                if (keyValuePair.Value.bShow)
                    dataGridViewProductDataShow.Columns[keyValuePair.Key].HeaderText = keyValuePair.Key.txtTrans(TransType.Trans);
            }
        }


        private void OnStationStateChanged(StationState OldState, StationState NewState)
        {
            if (this.IsDisposed || !this.IsHandleCreated)
                return;
            this.BeginInvoke((Action)delegate
            {
                if (OldState == StationState.Manual && NewState == StationState.Init)
                {
                    this.logGroup1.Clear();
                }
            });
        }

        /// <summary>
        /// 初始化界面生产数据表 — Virtual Mode + DoubleBuffered
        /// </summary>
        private void InitProductDataGridViewHead()
        {
            if (this.IsDisposed || !this.IsHandleCreated)
                return;
            this.BeginInvoke((Action)delegate
            {
                var dgv = dataGridViewProductDataShow;
                if (dgv.VirtualMode)
                    dgv.RowCount = 0;
                else
                    dgv.Rows.Clear();
                dgv.Columns.Clear();

                // ★ Virtual Mode — DataGridView không tạo Row object, chỉ hỏi data khi cần vẽ
                dgv.VirtualMode = true;
                dgv.ReadOnly = true;
                dgv.AllowUserToAddRows = false;
                dgv.CellValueNeeded -= Dgv_CellValueNeeded;
                dgv.CellValueNeeded += Dgv_CellValueNeeded;
                dgv.CellFormatting -= Dgv_CellFormatting;
                dgv.CellFormatting += Dgv_CellFormatting;

                // ★ DoubleBuffered — hết flicker
                dgv.EnableDoubleBuffered();

                dgv.RowsDefaultCellStyle.BackColor = Color.Ivory;
                dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.PaleTurquoise;

                if (ProductConfig.Instance.Option != null && ProductConfig.Instance.Option.Count > 0)
                {
                    var keys = new List<string>();
                    foreach (KeyValuePair<string, ProductConfig.SaveOption> keyValuePair in ProductConfig.Instance.Option)
                    {
                        if (keyValuePair.Value.bShow)
                        {
                            dgv.Columns.Add(keyValuePair.Key, keyValuePair.Key.txtTrans(TransType.Trans));
                            keys.Add(keyValuePair.Key);
                        }
                    }
                    _columnKeys = keys.ToArray();

                    foreach (DataGridViewColumn column in dgv.Columns)
                    {
                        column.SortMode = DataGridViewColumnSortMode.NotSortable;
                        column.Width = 100;
                    }
                }

                // ★ Khởi tạo Ring Buffer
                _ringBuffer = new ProductDataRingBuffer(ShowCount);
            });
        }

        /// <summary>
        /// 界面最大显示100行物料数据
        /// </summary>
        public static int ShowCount = 100;

        /// <summary>
        /// 生产数据展示 — Cấp 3: Chỉ enqueue, không chạm UI thread
        /// ThrottleFlush sẽ flush queue mỗi 33ms (~30fps)
        /// </summary>
        /// <param name="dicProduct"></param>
        private void OnShowProductData(Dictionary<string, string> dicProduct)
        {
            if (this.IsDisposed || !this.IsHandleCreated || dicProduct == null || dicProduct.Count < 1)
                return;

            // ★ Chỉ đẩy vào queue — KHÔNG BeginInvoke, KHÔNG chạm UI (Lock-Free)
            _pendingProducts.Enqueue(dicProduct);
            _hasPendingData = true;

            if(HiveMgr.Enable)
            {
                Task.Run(() => 
                {
                    HiveTool.MachineDataParameters machineDataParameters = new HiveTool.MachineDataParameters();                    
                    switch (StationEx.Process)
                    {
                        case AutoProcess.正常生产:
                            machineDataParameters.Mode = Parameter.GetBit("Enable_PDCA") ? ProcessingMode.NormalRunWithPdcaSfc : ProcessingMode.NormalRunNotWithPdcaSfc;
                            break;
                        case AutoProcess.空跑:
                            machineDataParameters.Mode = ProcessingMode.DryRun;
                            break;
                        case AutoProcess.CPK:
                            machineDataParameters.Mode = ProcessingMode.CPK;
                            break;
                        //case AutoProcess.GRR:
                        //    machineDataParameters.Mode = ProcessingMode.GRR;
                        //    break;
                        default:
                            break;
                    }
                    //填充产品其他数据
                    HiveMgr.OnSendProductData(machineDataParameters);
                });
            }
        }

        /// <summary>
        /// Virtual Mode: DataGridView hỏi giá trị cell → trả từ Ring Buffer
        /// </summary>
        private void Dgv_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            if (_ringBuffer == null || e.RowIndex >= _ringBuffer.Count)
            {
                e.Value = "";
                return;
            }
            e.Value = _ringBuffer.GetCellValue(e.RowIndex, e.ColumnIndex);
        }

        /// <summary>
        /// Virtual Mode: Tô màu đỏ cho hàng NG
        /// </summary>
        private void Dgv_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (_ringBuffer == null || e.RowIndex >= _ringBuffer.Count)
                return;

            Color fg = _ringBuffer.GetForeColor(e.RowIndex);
            if (fg != Color.Empty && fg != Color.Black)
            {
                e.CellStyle.ForeColor = fg;
            }
        }

        /// <summary>
        /// Throttle Timer (~30fps): Flush toàn bộ pending data, vẽ 1 lần duy nhất
        /// Gom N event thành 1 lần Invalidate → real-time mượt mà
        /// </summary>
        private void ThrottleFlush(object sender, EventArgs e)
        {
            if (this.IsDisposed || !this.IsHandleCreated)
                return;

            if (_pendingProducts.IsEmpty || _ringBuffer == null || _columnKeys == null)
                return;
            _hasPendingData = false;

            Dictionary<string, string> dicProduct;
            bool anyAdded = false;

            // ★ Flush toàn bộ queue — gom N sản phẩm thành 1 lần vẽ
            while (_pendingProducts.TryDequeue(out dicProduct))
            {
                if (dicProduct.Count < 1) continue;

                // Chuyển Dictionary → array theo thứ tự column
                string[] values = new string[_columnKeys.Length];
                for (int i = 0; i < _columnKeys.Length; i++)
                {
                    string v;
                    dicProduct.TryGetValue(_columnKeys[i], out v);
                    values[i] = v ?? "";
                }

                // Xác định màu NG
                Color fg = Color.Empty;
                if (dicProduct.ContainsKey("结果"))
                {
                    bool result;
                    if (bool.TryParse(dicProduct["结果"], out result) && !result)
                        fg = Color.Red;
                }

                // Push vào ring buffer — O(1)
                _ringBuffer.Push(values, fg);
                anyAdded = true;
            }

            if (!anyAdded) return;

            // ★ Cập nhật counter — inline, không BeginInvoke lồng
            lb_TotalValue.Text = ProductConfig.TotalCount.ToString();
            lb_PassValue.Text = ProductConfig.PassCount.ToString();
            lb_FailValue.Text = ProductConfig.FailCount.ToString();
            if (ProductConfig.TotalCount > 0)
            {
                double yield = (double)ProductConfig.PassCount / ProductConfig.TotalCount * 100;
                lb_YieldValue.Text = yield.ToString("F2") + "%";
            }
            else
            {
                lb_YieldValue.Text = "0.00%";
            }

            // ★ Cập nhật RowCount + Invalidate — CHỈ VẼ LẠI VÙNG NHÌN THẤY
            var dgv = dataGridViewProductDataShow;
            if (dgv.RowCount != _ringBuffer.Count)
                dgv.RowCount = _ringBuffer.Count;
            dgv.Invalidate();
        }

        /// <summary>
        /// 获取Hive站位信息
        /// </summary>
        private HiveStationInfo OnGetHiveStationInfo()
        {
            //可以从MES 获取Hive站点信息
            HiveStationInfo hiveStationInfo = new HiveStationInfo();
            SFCQuery.M_UploadSFC(ref hiveStationInfo);
            return hiveStationInfo;
        }
        /// <summary>
        /// 获取Hive用户信息
        /// </summary>
        private bool OnGetHiveUserInfo(ref HiveUserInfo hiveUserInfo)
        {
            //可以从MES 获取Hive站点信息
            SFCQuery.M_UploadSFC(ref hiveUserInfo);
            return true;
        }

        /// <summary>
        /// 更新计数器显示
        /// </summary>
        private void UpdateProductCounter()
        {
            if (this.IsDisposed || !this.IsHandleCreated)
                return;

            this.BeginInvoke((Action)delegate
            {
                lb_TotalValue.Text = ProductConfig.TotalCount.ToString();
                lb_PassValue.Text = ProductConfig.PassCount.ToString();
                lb_FailValue.Text = ProductConfig.FailCount.ToString();

                if (ProductConfig.TotalCount > 0)
                {
                    double yield = (double)ProductConfig.PassCount / ProductConfig.TotalCount * 100;
                    lb_YieldValue.Text = yield.ToString("F2") + "%";
                }
                else
                {
                    lb_YieldValue.Text = "0.00%";
                }
            });            
        }

        private void button_clear_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("是否清空统计数据？".txtTrans(TransType.Trans), "提示".txtTrans(TransType.Trans),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                ProductConfig.ClearProductCounter();
                UpdateProductCounter();

                // ★ Clear ring buffer + reset grid
                if (_ringBuffer != null)
                    _ringBuffer.Clear();
                dataGridViewProductDataShow.RowCount = 0;
                dataGridViewProductDataShow.Invalidate();
            }
        }
    }
}
