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
    public partial class TraySlotControl : UserControl
    {
        #region 槽位数据模型
        /// <summary>单个槽位数据</summary>
        private class SlotItem
        {
            public int Index { get; set; }
            public string Text { get; set; }
            public Color BackColor { get; set; }
        }
        #endregion

        #region 设计器可见属性
        private string _title = "AOI1";
        [Browsable(true)]
        [Category("托盘配置")]
        [Description("托盘顶部标题文本")]
        [DefaultValue("AOI1")]
        public string Title
        {
            get => _title;
            set
            {
                if (_title == value) return;
                _title = value;
                this.Invalidate();
            }
        }

        private Color _titleColor = Color.LightGray;
        [Browsable(true)]
        [Category("托盘配置")]
        [Description("托盘顶部标题颜色")]
        [DefaultValue(typeof(Color), "LightGray")]
        public Color TitleColor
        {
            get => _titleColor;
            set
            {
                if (_titleColor == value) return;
                _titleColor = value;
                this.Invalidate();
            }
        }

        private bool _newLineDisplayText = true;
        [Browsable(true)]
        [Category("托盘配置")]
        [Description("换行显示文本")]
        [DefaultValue(true)]
        public bool NewLineDisplayText
        {
            get => _newLineDisplayText;
            set
            {
                if (_newLineDisplayText == value) return;
                _newLineDisplayText = value;
                if (!DesignMode) this.Invalidate();
            }
        }

        private int _trayRowCount = 4;
        [Browsable(true)]
        [Category("托盘配置")]
        [Description("托盘行数")]
        [DefaultValue(4)]
        public int TrayRowCount
        {
            get => _trayRowCount;
            set
            {
                if (_trayRowCount == value || value <= 0) return;
                _trayRowCount = value;
                RebuildSlotData(); // 只重建内存数据，不创建控件
                this.Invalidate();
                this.PerformLayout();
            }
        }

        private int _trayColCount = 6;
        [Browsable(true)]
        [Category("托盘配置")]
        [Description("托盘列数，修改后设计器画布立刻刷新预览")]
        [DefaultValue(6)]
        public int TrayColCount
        {
            get => _trayColCount;
            set
            {
                if (_trayColCount == value || value <= 0) return;
                _trayColCount = value;
                RebuildSlotData(); // 只重建内存数据，不创建控件
                this.Invalidate();
                this.PerformLayout();
            }
        }
        #endregion

        #region 内部字段
        private List<SlotItem> _slotDataList; // 全部槽位内存数据，不再有Label数组
        private float _titleHeightPercent = 10f; // 标题占总高度百分比
        private bool _hasCreatedHandle;
        #endregion

        public TraySlotControl()
        {
            // 开启双缓冲，消除闪烁
            DoubleBuffered = true;
            SetStyle(ControlStyles.UserPaint
                | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw, true);

            _slotDataList = new List<SlotItem>();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (!_hasCreatedHandle)
            {
                _hasCreatedHandle = true;
                RebuildSlotData(); // 初始化槽位数据
            }
        }

        /// <summary>【极速】重建槽位内存数据，不创建任何子控件，无卡顿</summary>
        public void RebuildSlotData()
        {
            _slotDataList.Clear();
            int total = TrayRowCount * TrayColCount;
            for (int i = 1; i <= total; i++)
            {
                _slotDataList.Add(new SlotItem
                {
                    Index = i,
                    Text = "NA",
                    BackColor = Color.LightGray
                });
            }
            this.Invalidate();
        }

        #region 对外API
        /// <summary>设置槽位内容（索引从1开始）</summary>
        public void SetSlotContent(int slotIndex, string text = null, string singleText = null)
        {
            if (_slotDataList == null || slotIndex <0 || slotIndex >= _slotDataList.Count)
                return;

            // 跨线程：UI控件不能跨线程修改，但是修改内存数据后Invoke重绘
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => { SetSlotContent(slotIndex, text); }));
                return;
            }

            var slot = _slotDataList[slotIndex];
            if (singleText != null)
            {
                slot.Text = singleText;
                if (singleText == "0.000")
                    slot.BackColor = Color.LightGray;
                else
                    slot.BackColor = Color.LightGreen;
            }
            else
            {
                slot.Text = text;
                // 根据文本设置背景色
                string upperText = text?.Trim().ToUpper() ?? "";
                if (upperText == "OK")
                    slot.BackColor = Color.LightGreen;
                else if (upperText == "NG")
                    slot.BackColor = Color.LightCoral;
                else
                    slot.BackColor = Color.LightGray;
            }
           

            this.Invalidate(); // 只触发重绘，不操作子控件
        }

        public void SetSlotColorByRowCol(int row, int col, string text = null, string singleText = null)
        {
            int idx = row * TrayColCount + col; // 注意索引从1开始
            SetSlotContent(idx, text, singleText);
        }

        public void ResetAllSlot()
        {
            if (_slotDataList == null) return;
            foreach (var slot in _slotDataList)
            {
                slot.Text = "NA";
                slot.BackColor = Color.LightGray;
            }
            this.Invalidate();
        }
        #endregion

        #region GDI+绘制核心
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            // 先保存绘图状态
            System.Drawing.Drawing2D.SmoothingMode oldSmoothing = g.SmoothingMode;

            Rectangle clientRect = this.ClientRectangle;

            // 1.绘制标题区域
            int titleHeight = (int)(clientRect.Height * _titleHeightPercent / 100.0f);
            Rectangle rectTitle = new Rectangle(0, 0, clientRect.Width, titleHeight);
            using (SolidBrush brushTitleBg = new SolidBrush(_titleColor))
            {
                g.FillRectangle(brushTitleBg, rectTitle);
            }
            // 绘制标题文字
            using (Font fontTitle = new Font("微软雅黑", 10, FontStyle.Bold))
            {
                TextRenderer.DrawText(g, Title, fontTitle, rectTitle, Color.Black,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }

            // 2.槽位绘图区域（标题下方）
            Rectangle rectSlotArea = new Rectangle(0, titleHeight, clientRect.Width, clientRect.Height - titleHeight);
            int rows = TrayRowCount;
            int cols = TrayColCount;
            if (rows <= 0 || cols <= 0 || _slotDataList.Count == 0)
            {
                return;
            }

            float cellW = (float)rectSlotArea.Width / cols;
            float cellH = (float)rectSlotArea.Height / rows;

            // ========== 第一步：填充每个格子背景【只填颜色，不画边框】 ==========
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    int slotIndex = r * cols + c;
                    var slot = _slotDataList[slotIndex];

                    float x = rectSlotArea.X + c * cellW;
                    float y = rectSlotArea.Y + r * cellH;
                    RectangleF cellRect = new RectangleF(x, y, cellW, cellH);

                    using (SolidBrush brushCellBg = new SolidBrush(slot.BackColor))
                    {
                        g.FillRectangle(brushCellBg, cellRect);
                    }
                }
            }

            // ========== 第二步：绘制网格线，关闭抗锯齿保证线条粗细一致 ==========
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            using (Pen penGrid = new Pen(Color.Black, 1))
            {
                // 画全部竖线
                for (int c = 0; c <= cols; c++)
                {
                    float x = rectSlotArea.Left + c * cellW;
                    g.DrawLine(penGrid, x, rectSlotArea.Top, x, rectSlotArea.Bottom);
                }
                // 画全部横线
                for (int r = 0; r <= rows; r++)
                {
                    float y = rectSlotArea.Top + r * cellH;
                    g.DrawLine(penGrid, rectSlotArea.Left, y, rectSlotArea.Right, y);
                }
            }

            // ========== 第三步：绘制文字，恢复高质量抗锯齿 ==========
            g.SmoothingMode = oldSmoothing;
            using (Font fontCell = new Font("微软雅黑", 9, FontStyle.Bold))
            {
                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        int slotIndex = r * cols + c;
                        var slot = _slotDataList[slotIndex];

                        float x = rectSlotArea.X + c * cellW;
                        float y = rectSlotArea.Y + r * cellH;
                        RectangleF cellRect = new RectangleF(x, y, cellW, cellH);

                        // 组装绘制文本
                        string drawText;
                        if (NewLineDisplayText)
                        {
                            drawText = $"{slot.Index}\n{slot.Text}";
                        }
                        else
                        {
                            drawText = $"{slot.Index}";
                        }

                        // 绘制单元格文字，居中
                        TextRenderer.DrawText(g, drawText, fontCell,
                            Rectangle.Round(cellRect), Color.Black,
                            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
                    }
                }
            }
        }
        #endregion
    }
}
