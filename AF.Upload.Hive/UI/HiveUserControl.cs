using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Drawing.Imaging;
using ZXing;
using ZXing.QrCode;
using ZXing.Rendering;
using ZXing.QrCode.Internal;
using AF.File;

namespace AF.Upload.Hive
{
    /// <summary>
    /// Hive用户控件基类
    /// </summary>
    internal class HiveUserControl
    {
        // 可以在这里添加公共属性和方法供所有自定义控件使用
    }

    /// <summary>
    /// 带黑色边框的文本框控件，支持热点效果
    /// </summary>
    public class BlackBorderTextBox : TextBox
    {
        #region 私有字段
        private int _borderWidth = 1;
        private bool _hotTrack = false;
        private Color _borderColor = Color.Black;
        private Color _hotColor = Color.Black;
        private bool _isMouseOver = false;
        #endregion

        #region Windows API 导入
        /// <summary>
        /// 获取窗口设备上下文
        /// </summary>
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr hWnd);

        /// <summary>
        /// 释放设备上下文
        /// </summary>
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
        #endregion

        #region 构造函数
        /// <summary>
        /// 初始化BlackBorderTextBox
        /// </summary>
        public BlackBorderTextBox()
        {
            // 设置默认边框样式
            this.BorderStyle = BorderStyle.FixedSingle;
        }
        #endregion

        #region 公共属性
        /// <summary>
        /// 获取或设置边框宽度
        /// </summary>
        [Category("外观")]
        [Description("文本框边框的宽度")]
        [DefaultValue(1)]
        public int BorderWidth
        {
            get { return _borderWidth; }
            set
            {
                if (value < 1) value = 1;
                _borderWidth = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置是否启用热点效果
        /// </summary>
        [Category("行为")]
        [Description("指示当鼠标经过控件时控件边框是否发生变化")]
        [DefaultValue(false)]
        public bool HotTrack
        {
            get { return _hotTrack; }
            set
            {
                _hotTrack = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置边框颜色
        /// </summary>
        [Category("外观")]
        [Description("控件的边框颜色")]
        [DefaultValue(typeof(Color), "Black")]
        public Color BorderColor
        {
            get { return _borderColor; }
            set
            {
                _borderColor = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置热点时边框颜色
        /// </summary>
        [Category("外观")]
        [Description("鼠标经过控件时控件的边框颜色")]
        [DefaultValue(typeof(Color), "Black")]
        public Color HotColor
        {
            get { return _hotColor; }
            set
            {
                _hotColor = value;
                this.Invalidate();
            }
        }
        #endregion

        #region 重写的事件方法
        /// <summary>
        /// 鼠标移动到控件上时触发
        /// </summary>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            _isMouseOver = true;
            if (_hotTrack)
            {
                this.Invalidate();
            }
            base.OnMouseMove(e);
        }

        /// <summary>
        /// 鼠标离开控件时触发
        /// </summary>
        protected override void OnMouseLeave(EventArgs e)
        {
            _isMouseOver = false;
            if (_hotTrack)
            {
                this.Invalidate();
            }
            base.OnMouseLeave(e);
        }

        /// <summary>
        /// 控件获得焦点时触发
        /// </summary>
        protected override void OnGotFocus(EventArgs e)
        {
            if (_hotTrack)
            {
                this.Invalidate();
            }
            base.OnGotFocus(e);
        }

        /// <summary>
        /// 控件失去焦点时触发
        /// </summary>
        protected override void OnLostFocus(EventArgs e)
        {
            if (_hotTrack)
            {
                this.Invalidate();
            }
            base.OnLostFocus(e);
        }
        #endregion

        #region 窗口消息处理
        /// <summary>
        /// 处理Windows消息，用于自定义绘制
        /// </summary>
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            // 处理WM_PAINT(0xF)和WM_NCPAINT(0x85)消息
            if (m.Msg == 0xF || m.Msg == 0x85)
            {
                DrawCustomBorder(ref m);
            }
        }

        /// <summary>
        /// 绘制自定义边框
        /// </summary>
        private void DrawCustomBorder(ref Message m)
        {
            IntPtr hDC = GetWindowDC(m.HWnd);
            if (hDC == IntPtr.Zero) return;

            try
            {
                // 只在边框样式为FixedSingle时绘制自定义边框
                if (this.BorderStyle == BorderStyle.FixedSingle)
                {
                    using (Graphics g = Graphics.FromHdc(hDC))
                    using (Pen borderPen = CreateBorderPen())
                    {
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                        // 绘制边框，考虑边框宽度
                        int halfBorder = _borderWidth / 2;
                        Rectangle borderRect = new Rectangle(
                            halfBorder,
                            halfBorder,
                            this.Width - _borderWidth,
                            this.Height - _borderWidth);

                        g.DrawRectangle(borderPen, borderRect);
                    }
                }
            }
            finally
            {
                m.Result = IntPtr.Zero;
                ReleaseDC(m.HWnd, hDC);
            }
        }

        /// <summary>
        /// 根据当前状态创建边框画笔
        /// </summary>
        private Pen CreateBorderPen()
        {
            Color penColor = _borderColor;

            if (_hotTrack)
            {
                if (this.Focused || _isMouseOver)
                {
                    penColor = _hotColor;
                }
            }

            return new Pen(penColor, _borderWidth);
        }
        #endregion
    }

    /// <summary>
    /// 多行按钮控件，支持显示两行文本和左侧图标
    /// </summary>
    public class MultiLineButton : Button
    {
        #region 私有字段
        private string _line1 = "";
        private string _line2 = "";
        private Font _line1Font;
        private Font _line2Font;
        private Image _leftImage;
        private int _imagePadding = 5;
        private int _textPadding = 10;
        #endregion

        #region 构造函数
        /// <summary>
        /// 初始化MultiLineButton
        /// </summary>
        public MultiLineButton()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 初始化控件属性
        /// </summary>
        private void InitializeComponent()
        {
            // 设置默认字体
            _line1Font = new Font("Microsoft YaHei", 12f, FontStyle.Bold);
            _line2Font = new Font("Microsoft YaHei", 9f, FontStyle.Regular);

            // 设置按钮样式
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.BackColor = Color.FromArgb(240, 240, 240);
            this.Size = new Size(250, 80);
        }
        #endregion

        #region 公共属性
        /// <summary>
        /// 获取或设置第一行文本
        /// </summary>
        [Category("外观")]
        [Description("按钮的第一行文本")]
        [DefaultValue("")]
        public string Line1
        {
            get { return _line1; }
            set
            {
                _line1 = value ?? "";
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置第二行文本
        /// </summary>
        [Category("外观")]
        [Description("按钮的第二行文本")]
        [DefaultValue("")]
        public string Line2
        {
            get { return _line2; }
            set
            {
                _line2 = value ?? "";
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置第一行文本字体
        /// </summary>
        [Category("外观")]
        [Description("第一行文本的字体")]
        public Font Line1Font
        {
            get { return _line1Font; }
            set
            {
                _line1Font = value ?? new Font("Microsoft YaHei", 12f, FontStyle.Bold);
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置第二行文本字体
        /// </summary>
        [Category("外观")]
        [Description("第二行文本的字体")]
        public Font Line2Font
        {
            get { return _line2Font; }
            set
            {
                _line2Font = value ?? new Font("Microsoft YaHei", 9f, FontStyle.Regular);
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置左侧图片
        /// </summary>
        [Category("外观")]
        [Description("按钮左侧显示的图片")]
        [DefaultValue(null)]
        public Image LeftImage
        {
            get { return _leftImage; }
            set
            {
                _leftImage = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置图片内边距
        /// </summary>
        [Category("布局")]
        [Description("图片与边框的内边距")]
        [DefaultValue(10)]
        public int ImagePadding
        {
            get { return _imagePadding; }
            set
            {
                _imagePadding = Math.Max(0, value);
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置文本内边距
        /// </summary>
        [Category("布局")]
        [Description("文本与右侧边框的内边距")]
        [DefaultValue(15)]
        public int TextPadding
        {
            get { return _textPadding; }
            set
            {
                _textPadding = Math.Max(0, value);
                this.Invalidate();
            }
        }
        #endregion

        #region 重写的绘制方法
        /// <summary>
        /// 自定义绘制按钮
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            if (e == null) return;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            DrawBackground(e.Graphics);

            int contentStartX = CalculateContentStartX();
            DrawTextContent(e.Graphics, contentStartX);
            DrawBorder(e.Graphics);
            DrawFocusRectangle(e.Graphics);
        }

        /// <summary>
        /// 绘制按钮背景
        /// </summary>
        private void DrawBackground(Graphics g)
        {
            using (SolidBrush backBrush = new SolidBrush(this.BackColor))
            {
                g.FillRectangle(backBrush, this.ClientRectangle);
            }
        }

        /// <summary>
        /// 计算内容起始X坐标
        /// </summary>
        private int CalculateContentStartX()
        {
            int contentStartX = _imagePadding;

            if (_leftImage != null)
            {
                int imageSize = Math.Min(this.Height - _imagePadding * 2, 50);
                contentStartX += imageSize + _imagePadding;
            }

            return contentStartX;
        }

        /// <summary>
        /// 绘制文本内容
        /// </summary>
        private void DrawTextContent(Graphics g, int contentStartX)
        {
            if (_leftImage != null)
            {
                DrawLeftImage(g);
            }

            int textAreaX = contentStartX;
            int textAreaWidth = this.Width - textAreaX - _textPadding;

            DrawLineText(g, _line1, _line1Font, textAreaX, textAreaWidth, -10);
            DrawLineText(g, _line2, _line2Font, textAreaX, textAreaWidth, 10);
        }

        /// <summary>
        /// 绘制左侧图片
        /// </summary>
        private void DrawLeftImage(Graphics g)
        {
            int imageSize = Math.Min(this.Height - _imagePadding * 2, 50);
            int imageY = (this.Height - imageSize) / 2;
            g.DrawImage(_leftImage, _imagePadding, imageY, imageSize, imageSize);
        }

        /// <summary>
        /// 绘制单行文本
        /// </summary>
        private void DrawLineText(Graphics g, string text, Font font, int x, int maxWidth, int verticalOffset)
        {
            if (string.IsNullOrEmpty(text)) return;

            using (SolidBrush textBrush = new SolidBrush(this.ForeColor))
            {
                string displayText = TruncateText(g, text, font, maxWidth);
                SizeF textSize = g.MeasureString(displayText, font);

                float y = this.Height / 2 - textSize.Height / 2 + verticalOffset;
                g.DrawString(displayText, font, textBrush, x, y);
            }
        }

        /// <summary>
        /// 绘制边框
        /// </summary>
        private void DrawBorder(Graphics g)
        {
            using (Pen borderPen = new Pen(Color.FromArgb(200, 200, 200), 1))
            {
                g.DrawRectangle(borderPen, 0, 0, this.Width - 1, this.Height - 1);
            }
        }

        /// <summary>
        /// 绘制焦点矩形
        /// </summary>
        private void DrawFocusRectangle(Graphics g)
        {
            if (this.Focused)
            {
                ControlPaint.DrawFocusRectangle(g,
                    new Rectangle(2, 2, this.Width - 4, this.Height - 4));
            }
        }
        #endregion

        #region 文本处理
        /// <summary>
        /// 截断文本以适应指定宽度
        /// </summary>
        private string TruncateText(Graphics g, string text, Font font, int maxWidth)
        {
            const string ellipsis = "...";

            if (string.IsNullOrEmpty(text) || maxWidth <= 0)
                return text;

            SizeF textSize = g.MeasureString(text, font);
            if (textSize.Width <= maxWidth)
                return text;

            // 使用二分查找确定合适的截断位置
            int left = 0;
            int right = text.Length;
            string truncatedText;

            while (left <= right)
            {
                int mid = (left + right) / 2;
                truncatedText = text.Substring(0, mid) + ellipsis;
                textSize = g.MeasureString(truncatedText, font);

                if (textSize.Width <= maxWidth)
                {
                    left = mid + 1;
                }
                else
                {
                    right = mid - 1;
                }
            }

            return right > 0 ? text.Substring(0, right) + ellipsis : ellipsis;
        }
        #endregion

        #region 资源清理
        /// <summary>
        /// 清理使用的资源
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _line1Font?.Dispose();
                _line2Font?.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion
    }

    /// <summary>
    /// 自定义窗体基类，提供统一的窗口样式和标题栏自定义功能
    /// </summary>
    public class HiveBaseForm : Form
    {
        #region Windows API 声明

        /// <summary>
        /// 设置窗口样式
        /// </summary>
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        /// <summary>
        /// 获取窗口样式
        /// </summary>
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        /// <summary>
        /// 设置窗口位置和大小
        /// </summary>
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        /// <summary>
        /// 设置桌面窗口管理器属性
        /// </summary>
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        /// <summary>
        /// 获取桌面窗口管理器属性
        /// </summary>
        [DllImport("dwmapi.dll")]
        private static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out bool pvAttribute, int cbAttribute);

        // 新增：用于检测和设置 MessageBox 的 API
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetLastActivePopup(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        #endregion

        #region Windows 常量定义

        public bool bCommError = false;
        // 窗口样式常量
        private const int GWL_STYLE = -16;           // 窗口样式索引
        private const int GWL_EXSTYLE = -20;         // 扩展窗口样式索引
        private const int WS_BORDER = 0x00800000;    // 窗口边框样式
        private const int WS_EX_CLIENTEDGE = 0x00000200; // 客户端边缘样式

        // 窗口位置标志
        private const uint SWP_FRAMECHANGED = 0x0020; // 强制发送WM_NCCALCSIZE消息
        private const uint SWP_NOSIZE = 0x0001;      // 不调整窗口大小
        private const uint SWP_NOMOVE = 0x0002;      // 不移动窗口位置
        private const uint SWP_NOZORDER = 0x0004;    // 不改变Z序
        private const uint SWP_SHOWWINDOW = 0x0040;  // 显示窗口
        private const uint SWP_NOACTIVATE = 0x0010;  // 不激活窗口

        // 窗口Z序常量
        private static readonly IntPtr HWND_TOP = new IntPtr(0);        // 置于顶部
        private static readonly IntPtr HWND_BOTTOM = new IntPtr(1);     // 置于底部
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);   // 置顶窗口
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2); // 非置顶窗口

        // DWM窗口属性常量
        private const int DWMWCP_DONOTROUND = 1;     // 直角窗口角样式
        private const int DWMWCP_ROUND = 2;          // 圆角窗口角样式
        private const int DWMWCP_ROUNDSMALL = 3;     // 小圆角窗口角样式
        private const int DWMWA_BORDER_COLOR = 34;   // 窗口边框颜色属性
        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33; // 窗口角样式属性
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;  // 沉浸式深色模式属性
        private const int DWMWA_CAPTION_COLOR = 35;  // 标题栏颜色属性
        private const int DWMWA_TEXT_COLOR = 36;     // 标题文字颜色属性
        private const int DWMWA_MICA_EFFECT = 1029;  // Mica效果属性

        // Windows消息常量
        private const int WM_NCLBUTTONDOWN = 0x00A1; // 非客户区鼠标左键按下消息
        private const int HTCAPTION = 2;             // 标题栏区域代码

        // 窗口关系常量
        private const uint GW_OWNER = 4;

        #endregion

        #region 私有字段

        private Color _borderColor = Color.Black;   // 默认颜色
        private Color _titleBarColor = HiveUIColors.Gray;   // 默认标题栏背景颜色
        private Color _titleTextColor = Color.Black; // 默认标题栏文字颜色
        private bool _enableTitleBarDrag = true;     // 是否启用标题栏拖动
                                                     // 新增：MessageBox 检测相关字段
        private System.Windows.Forms.Timer messageBoxCheckTimer;
        private bool _isCheckingMessageBox = false;
        private HashSet<IntPtr> _processedMessageBoxes = new HashSet<IntPtr>();
        #endregion

        #region 公共属性

        /// <summary>
        /// 获取或设置边框颜色
        /// </summary>
        [Category("外观")]
        [Description("窗体的边框颜色")]
        [DefaultValue(typeof(Color), "Black")]
        public Color BorderColor
        {
            get { return _borderColor; }
            set
            {
                _borderColor = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置标题栏背景颜色
        /// </summary>
        [Category("外观")]
        [Description("标题栏的背景颜色")]
        [DefaultValue(typeof(Color), "240, 240, 240")]
        public Color TitleBarColor
        {
            get { return _titleBarColor; }
            set
            {
                _titleBarColor = value;
                UpdateTitleBarColor();
            }
        }

        /// <summary>
        /// 获取或设置标题栏文字颜色
        /// </summary>
        [Category("外观")]
        [Description("标题栏的文字颜色")]
        [DefaultValue(typeof(Color), "Black")]
        public Color TitleTextColor
        {
            get { return _titleTextColor; }
            set
            {
                _titleTextColor = value;
                UpdateTitleBarColor();
            }
        }

        /// <summary>
        /// 获取或设置是否启用标题栏拖动
        /// </summary>
        [Category("行为")]
        [Description("是否允许通过标题栏拖动窗口")]
        [DefaultValue(true)]
        public bool EnableTitleBarDrag
        {
            get { return _enableTitleBarDrag; }
            set { _enableTitleBarDrag = value; }
        }
        /// <summary>
        /// 获取或设置是否启用MessageBox检测
        /// </summary>
        [Category("行为")]
        [Description("是否启用MessageBox自动检测并置顶")]
        [DefaultValue(true)]
        public bool EnableMessageBoxDetection { get; set; } = true;
        #endregion

        #region 构造函数

        /// <summary>
        /// 初始化HiveBaseForm
        /// </summary>
        public HiveBaseForm()
        {

            // 设置默认窗体属性
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Microsoft YaHei", 9f);

            // 启用双缓冲以减少闪烁
            this.DoubleBuffered = true;

            // 初始化MessageBox检测定时器
            InitializeMessageBoxDetection();
        }

        #endregion
        #region MessageBox 检测功能

        /// <summary>
        /// 初始化MessageBox检测定时器
        /// </summary>
        private void InitializeMessageBoxDetection()
        {
            messageBoxCheckTimer = new System.Windows.Forms.Timer();
            messageBoxCheckTimer.Interval = 100; // 每100ms检查一次
            messageBoxCheckTimer.Tick += MessageBoxCheckTimer_Tick;
        }

        /// <summary>
        /// 开始检测MessageBox
        /// </summary>
        protected void StartMessageBoxDetection()
        {
            if (EnableMessageBoxDetection && !messageBoxCheckTimer.Enabled)
            {
                messageBoxCheckTimer.Start();
            }
        }

        /// <summary>
        /// 停止检测MessageBox
        /// </summary>
        protected void StopMessageBoxDetection()
        {
            if (messageBoxCheckTimer.Enabled)
            {
                messageBoxCheckTimer.Stop();
            }
            _processedMessageBoxes.Clear();
        }

        /// <summary>
        /// MessageBox检测定时器事件
        /// </summary>
        private void MessageBoxCheckTimer_Tick(object sender, EventArgs e)
        {
            if (_isCheckingMessageBox || !EnableMessageBoxDetection)
                return;

            _isCheckingMessageBox = true;

            try
            {
                CheckAndBringMessageBoxesToFront();
                CleanupProcessedMessageBoxes(); // 添加清理调用
            }
            catch (Exception ex)
            {
                // 记录错误但不影响程序运行
                System.Diagnostics.Debug.WriteLine($"MessageBox检测错误: {ex.Message}");
            }
            finally
            {
                _isCheckingMessageBox = false;
            }
        }

        /// <summary>
        /// 检查并置顶MessageBox
        /// </summary>
        private void CheckAndBringMessageBoxesToFront()
        {
            // 枚举所有顶层窗口
            EnumWindows(EnumWindowsCallback, IntPtr.Zero);
        }

        /// <summary>
        /// 枚举窗口回调函数
        /// </summary>
        private bool EnumWindowsCallback(IntPtr hWnd, IntPtr lParam)
        {
            // 检查窗口是否可见
            if (!IsWindowVisible(hWnd))
                return true;

            // 检查窗口类名是否为MessageBox
            if (IsMessageBoxWindow(hWnd))
            {
                // 检查是否已经处理过这个MessageBox
                if (!_processedMessageBoxes.Contains(hWnd))
                {
                    // 将MessageBox置顶
                    BringMessageBoxToFront(hWnd);
                    _processedMessageBoxes.Add(hWnd);
                }
            }

            return true; // 继续枚举
        }

        /// <summary>
        /// 判断窗口是否为MessageBox
        /// </summary>
        private bool IsMessageBoxWindow(IntPtr hWnd)
        {
            try
            {
                // 获取窗口类名
                StringBuilder className = new StringBuilder(256);
                if (GetClassName(hWnd, className, className.Capacity) == 0)
                    return false;

                string classNameStr = className.ToString();

                // MessageBox的类名通常是 "#32770"
                if (classNameStr == "#32770")
                {
                    // 进一步验证：检查窗口是否属于当前进程
                    uint processId;
                    GetWindowThreadProcessId(hWnd, out processId);
                    uint currentProcessId = (uint)System.Diagnostics.Process.GetCurrentProcess().Id;

                    if (processId == currentProcessId)
                    {
                        // 额外验证：检查窗口是否有标准MessageBox的特征
                        // 比如有OK、Cancel、Yes、No等按钮
                        return HasMessageBoxFeatures(hWnd);
                    }
                }
            }
            catch
            {
                // 忽略异常
            }

            return false;
        }
        /// <summary>
        /// 检查窗口是否具有MessageBox的特征
        /// </summary>
        private bool HasMessageBoxFeatures(IntPtr hWnd)
        {
            // 这里可以添加更精确的MessageBox特征检测
            // 例如检查是否有标准的按钮等

            // 暂时返回true，因为类名已经是#32770且属于当前进程
            return true;
        }
        /// <summary>
        /// 将MessageBox置顶
        /// </summary>
        private void BringMessageBoxToFront(IntPtr messageBoxHandle)
        {
            try
            {
                // 设置MessageBox为前台窗口
                SetForegroundWindow(messageBoxHandle);
                BringWindowToTop(messageBoxHandle);

                // 可选：将MessageBox设置为TopMost
                SetWindowPos(messageBoxHandle, HWND_TOPMOST, 0, 0, 0, 0,
                            SWP_NOSIZE | SWP_NOMOVE | SWP_SHOWWINDOW);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"置顶MessageBox失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 清理已处理的MessageBox记录
        /// </summary>
        private void CleanupProcessedMessageBoxes()
        {
            // 每30次检测清理一次（约3秒）
            if (DateTime.Now.Second % 3 == 0)
            {
                _processedMessageBoxes.RemoveWhere(handle =>
                {
                    // 检查窗口是否仍然存在且可见
                    return !IsWindow(handle) || !IsWindowVisible(handle);
                });
            }
        }

        [DllImport("user32.dll")]
        private static extern bool IsWindow(IntPtr hWnd);

        #endregion
        #region 重写方法

        /// <summary>
        /// 窗口句柄创建完成时调用
        /// </summary>
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            if (!DesignMode) // 设计模式下不执行
            {
                // 设置窗口边框样式为直角
                SetSquareBorderStyle();

                // 设置黑色边框颜色
                SetBlackBorderColor();

                // 开始检测MessageBox
                //StartMessageBoxDetection();

                // 延迟设置标题栏颜色，确保窗口完全初始化
                this.BeginInvoke(new Action(() =>
                {
                    UpdateTitleBarColor();
                }));
            }
        }
        /// <summary>
        /// 窗体激活时
        /// </summary>
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            // 窗体激活时检查是否有MessageBox需要置顶
            if (EnableMessageBoxDetection)
            {
                CheckAndBringMessageBoxesToFront();
            }
        }
        /// <summary>
        /// 窗体关闭时
        /// </summary>
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            // 停止MessageBox检测
            StopMessageBoxDetection();

            base.OnFormClosed(e);
        }

        /// <summary>
        /// 处理Windows消息
        /// </summary>
        protected override void WndProc(ref Message m)
        {
            // 拦截非客户区的鼠标左键按下消息（通常发生在标题栏）
            if (m.Msg == WM_NCLBUTTONDOWN && (int)m.WParam == HTCAPTION)
            {
                if (!_enableTitleBarDrag)
                {
                    // 直接返回，不调用基类处理，从而屏蔽标题栏拖动操作
                    return;
                }
            }

            // 其他消息交给默认流程处理
            base.WndProc(ref m);
        }

        /// <summary>
        /// 窗体加载时执行
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!DesignMode)
            {
                EnableTitleBarDrag = false;//FALSE时不允许拖动所有窗口，TRUE时可以拖动
                // 确保窗口样式正确应用
                UpdateTitleBarColor();
            }
        }

        #endregion

        #region 受保护的方法

        /// <summary>
        /// 更新窗口边框样式（子类可调用）
        /// </summary>
        protected void RefreshWindowStyle()
        {
            if (this.IsHandleCreated && !DesignMode)
            {
                SetSquareBorderStyle();
                SetBlackBorderColor();
                UpdateTitleBarColor();
            }
        }
        #endregion

        #region 私有方法

        /// <summary>
        /// 设置直角边框样式
        /// </summary>
        private void SetSquareBorderStyle()
        {
            try
            {
                // 获取当前窗口样式
                int style = GetWindowLong(this.Handle, GWL_STYLE);

                // 添加窗口边框样式
                SetWindowLong(this.Handle, GWL_STYLE, style | WS_BORDER);

                // 刷新窗口以应用样式更改
                SetWindowPos(this.Handle, IntPtr.Zero, 0, 0, 0, 0,
                    SWP_FRAMECHANGED | 0x0001 | 0x0002 | 0x0004 | 0x0008);
            }
            catch (Exception ex)
            {
                Console.WriteLine("设置直角边框样式失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 设置黑色边框颜色和直角角样式
        /// </summary>
        private void SetBlackBorderColor()
        {
            try
            {
                // 设置边框颜色为黑色
                int borderColor = ColorToInt(_borderColor);
                DwmSetWindowAttribute(this.Handle, DWMWA_BORDER_COLOR, ref borderColor, sizeof(int));

                // 设置窗口角样式为直角
                int cornerPreference = DWMWCP_DONOTROUND;
                DwmSetWindowAttribute(this.Handle, DWMWA_WINDOW_CORNER_PREFERENCE,
                                    ref cornerPreference, sizeof(int));
            }
            catch (Exception ex)
            {
                // DWM API可能在某些Windows版本上不可用
                Console.WriteLine("设置边框颜色失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 更新标题栏颜色设置
        /// </summary>
        private void UpdateTitleBarColor()
        {
            if (!this.IsHandleCreated || DesignMode) return;

            try
            {
                // 设置标题栏背景颜色 (Windows 11)
                int captionColor = ColorToInt(_titleBarColor);
                DwmSetWindowAttribute(this.Handle, DWMWA_CAPTION_COLOR, ref captionColor, sizeof(int));

                // 设置标题栏文字颜色 (Windows 11)
                int textColor = ColorToInt(_titleTextColor);
                DwmSetWindowAttribute(this.Handle, DWMWA_TEXT_COLOR, ref textColor, sizeof(int));

                // 禁用深色模式以确保颜色正确显示
                int darkMode = 0;
                DwmSetWindowAttribute(this.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int));

                // 刷新窗口显示
                this.Invalidate();
            }
            catch (Exception ex)
            {
                Console.WriteLine("设置标题栏颜色失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 将Color颜色值转换为Windows API使用的整数值
        /// </summary>
        /// <param name="color">要转换的颜色</param>
        /// <returns>Windows API格式的颜色整数值</returns>
        private int ColorToInt(Color color)
        {
            // Windows API使用BGR格式，而不是RGB
            return (color.B << 16) | (color.G << 8) | color.R;
        }

        #endregion
    }
    /// <summary>
    /// 自定义窗体基类，提供统一的窗口样式和标题栏自定义功能
    /// </summary>
    public class HiveBaseForm1 : Form
    {
        #region Windows API 声明

        /// <summary>
        /// 设置窗口样式
        /// </summary>
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        /// <summary>
        /// 获取窗口样式
        /// </summary>
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        /// <summary>
        /// 设置窗口位置和大小
        /// </summary>
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        /// <summary>
        /// 设置桌面窗口管理器属性
        /// </summary>
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        /// <summary>
        /// 获取桌面窗口管理器属性
        /// </summary>
        [DllImport("dwmapi.dll")]
        private static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out bool pvAttribute, int cbAttribute);

        #endregion

        #region Windows 常量定义

        public bool bCommError = false;
        // 窗口样式常量
        private const int GWL_STYLE = -16;           // 窗口样式索引
        private const int GWL_EXSTYLE = -20;         // 扩展窗口样式索引
        private const int WS_BORDER = 0x00800000;    // 窗口边框样式
        private const int WS_EX_CLIENTEDGE = 0x00000200; // 客户端边缘样式

        // 窗口位置标志
        private const uint SWP_FRAMECHANGED = 0x0020; // 强制发送WM_NCCALCSIZE消息

        // DWM窗口属性常量
        private const int DWMWCP_DONOTROUND = 1;     // 直角窗口角样式
        private const int DWMWCP_ROUND = 2;          // 圆角窗口角样式
        private const int DWMWCP_ROUNDSMALL = 3;     // 小圆角窗口角样式
        private const int DWMWA_BORDER_COLOR = 34;   // 窗口边框颜色属性
        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33; // 窗口角样式属性
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;  // 沉浸式深色模式属性
        private const int DWMWA_CAPTION_COLOR = 35;  // 标题栏颜色属性
        private const int DWMWA_TEXT_COLOR = 36;     // 标题文字颜色属性
        private const int DWMWA_MICA_EFFECT = 1029;  // Mica效果属性

        // Windows消息常量
        private const int WM_NCLBUTTONDOWN = 0x00A1; // 非客户区鼠标左键按下消息
        private const int HTCAPTION = 2;             // 标题栏区域代码

        #endregion

        #region 私有字段

        private Color _borderColor = Color.Black;   // 默认颜色
        private Color _titleBarColor = HiveUIColors.Gray;   // 默认标题栏背景颜色
        private Color _titleTextColor = Color.Black; // 默认标题栏文字颜色
        private bool _enableTitleBarDrag = true;     // 是否启用标题栏拖动

        #endregion

        #region 公共属性

        /// <summary>
        /// 获取或设置边框颜色
        /// </summary>
        [Category("外观")]
        [Description("窗体的边框颜色")]
        [DefaultValue(typeof(Color), "Black")]
        public Color BorderColor
        {
            get { return _borderColor; }
            set
            {
                _borderColor = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置标题栏背景颜色
        /// </summary>
        [Category("外观")]
        [Description("标题栏的背景颜色")]
        [DefaultValue(typeof(Color), "240, 240, 240")]
        public Color TitleBarColor
        {
            get { return _titleBarColor; }
            set
            {
                _titleBarColor = value;
                UpdateTitleBarColor();
            }
        }

        /// <summary>
        /// 获取或设置标题栏文字颜色
        /// </summary>
        [Category("外观")]
        [Description("标题栏的文字颜色")]
        [DefaultValue(typeof(Color), "Black")]
        public Color TitleTextColor
        {
            get { return _titleTextColor; }
            set
            {
                _titleTextColor = value;
                UpdateTitleBarColor();
            }
        }

        /// <summary>
        /// 获取或设置是否启用标题栏拖动
        /// </summary>
        [Category("行为")]
        [Description("是否允许通过标题栏拖动窗口")]
        [DefaultValue(true)]
        public bool EnableTitleBarDrag
        {
            get { return _enableTitleBarDrag; }
            set { _enableTitleBarDrag = value; }
        }

        #endregion

        #region 构造函数

        /// <summary>
        /// 初始化HiveBaseForm
        /// </summary>
        public HiveBaseForm1()
        {
            // 设置默认窗体属性
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Microsoft YaHei", 9f);

            // 启用双缓冲以减少闪烁
            this.DoubleBuffered = true;
        }

        #endregion

        #region 重写方法

        /// <summary>
        /// 窗口句柄创建完成时调用
        /// </summary>
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            if (!DesignMode) // 设计模式下不执行
            {
                // 设置窗口边框样式为直角
                SetSquareBorderStyle();

                // 设置黑色边框颜色
                SetBlackBorderColor();

                // 延迟设置标题栏颜色，确保窗口完全初始化
                this.BeginInvoke(new Action(() =>
                {
                    UpdateTitleBarColor();
                }));
            }
        }

        /// <summary>
        /// 处理Windows消息
        /// </summary>
        protected override void WndProc(ref Message m)
        {
            // 拦截非客户区的鼠标左键按下消息（通常发生在标题栏）
            if (m.Msg == WM_NCLBUTTONDOWN && (int)m.WParam == HTCAPTION)
            {
                if (!_enableTitleBarDrag)
                {
                    // 直接返回，不调用基类处理，从而屏蔽标题栏拖动操作
                    return;
                }
            }

            // 其他消息交给默认流程处理
            base.WndProc(ref m);
        }

        /// <summary>
        /// 窗体加载时执行
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!DesignMode)
            {
                // 确保窗口样式正确应用
                UpdateTitleBarColor();
            }
        }

        #endregion

        #region 受保护的方法

        /// <summary>
        /// 更新窗口边框样式（子类可调用）
        /// </summary>
        protected void RefreshWindowStyle()
        {
            if (this.IsHandleCreated && !DesignMode)
            {
                SetSquareBorderStyle();
                SetBlackBorderColor();
                UpdateTitleBarColor();
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 设置直角边框样式
        /// </summary>
        private void SetSquareBorderStyle()
        {
            try
            {
                // 获取当前窗口样式
                int style = GetWindowLong(this.Handle, GWL_STYLE);

                // 添加窗口边框样式
                SetWindowLong(this.Handle, GWL_STYLE, style | WS_BORDER);

                // 刷新窗口以应用样式更改
                SetWindowPos(this.Handle, IntPtr.Zero, 0, 0, 0, 0,
                    SWP_FRAMECHANGED | 0x0001 | 0x0002 | 0x0004 | 0x0008);
            }
            catch (Exception ex)
            {
                Console.WriteLine("设置直角边框样式失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 设置黑色边框颜色和直角角样式
        /// </summary>
        private void SetBlackBorderColor()
        {
            try
            {
                // 设置边框颜色为黑色
                int borderColor = ColorToInt(_borderColor);
                DwmSetWindowAttribute(this.Handle, DWMWA_BORDER_COLOR, ref borderColor, sizeof(int));

                // 设置窗口角样式为直角
                int cornerPreference = DWMWCP_DONOTROUND;
                DwmSetWindowAttribute(this.Handle, DWMWA_WINDOW_CORNER_PREFERENCE,
                                    ref cornerPreference, sizeof(int));
            }
            catch (Exception ex)
            {
                // DWM API可能在某些Windows版本上不可用
                Console.WriteLine("设置边框颜色失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 更新标题栏颜色设置
        /// </summary>
        private void UpdateTitleBarColor()
        {
            if (!this.IsHandleCreated || DesignMode) return;

            try
            {
                // 设置标题栏背景颜色 (Windows 11)
                int captionColor = ColorToInt(_titleBarColor);
                DwmSetWindowAttribute(this.Handle, DWMWA_CAPTION_COLOR, ref captionColor, sizeof(int));

                // 设置标题栏文字颜色 (Windows 11)
                int textColor = ColorToInt(_titleTextColor);
                DwmSetWindowAttribute(this.Handle, DWMWA_TEXT_COLOR, ref textColor, sizeof(int));

                // 禁用深色模式以确保颜色正确显示
                int darkMode = 0;
                DwmSetWindowAttribute(this.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int));

                // 刷新窗口显示
                this.Invalidate();
            }
            catch (Exception ex)
            {
                Console.WriteLine("设置标题栏颜色失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 将Color颜色值转换为Windows API使用的整数值
        /// </summary>
        /// <param name="color">要转换的颜色</param>
        /// <returns>Windows API格式的颜色整数值</returns>
        private int ColorToInt(Color color)
        {
            // Windows API使用BGR格式，而不是RGB
            return (color.B << 16) | (color.G << 8) | color.R;
        }

        #endregion
    }

    public class HiveBaseFormText : HiveBaseForm
    {
        #region Windows API 声明

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        #endregion

        #region Windows 常量定义

        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x00080000;
        private const int WS_MINIMIZEBOX = 0x00020000;
        private const int WS_MAXIMIZEBOX = 0x00010000;
        private const int WM_NCLBUTTONDOWN = 0x00A1;
        private const int HT_CAPTION = 0x2;

        #endregion

        #region 私有字段

        private Panel _titleBarPanel;
        private Label _titleLabel;
        private Button _closeButton;
        private bool _useCustomTitleBar = true;
        private Color _borderColor = Color.Black;
        private int _borderWidth = 1;
        private FormBorderStyle _originalFormBorderStyle;
        private bool _alwaysShowBorder = true;
        private bool _showCloseButton = true; // 改为默认显示
        private int _closeButtonSize = 25;

        #endregion

        #region 公共属性

        /// <summary>
        /// 获取或设置是否使用自定义标题栏
        /// </summary>
        [Category("外观")]
        [Description("是否使用自定义标题栏")]
        [DefaultValue(true)]
        public bool UseCustomTitleBar
        {
            get { return _useCustomTitleBar; }
            set
            {
                _useCustomTitleBar = value;
                UpdateTitleBar();
            }
        }

        /// <summary>
        /// 获取或设置边框颜色
        /// </summary>
        [Category("外观")]
        [Description("窗体的边框颜色")]
        [DefaultValue(typeof(Color), "Black")]
        public Color BorderColor
        {
            get { return _borderColor; }
            set
            {
                _borderColor = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置边框宽度
        /// </summary>
        [Category("外观")]
        [Description("窗体的边框宽度")]
        [DefaultValue(1)]
        public int BorderWidth
        {
            get { return _borderWidth; }
            set
            {
                _borderWidth = Math.Max(1, value);
                UpdateLayout();
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置是否始终显示边框
        /// </summary>
        [Category("外观")]
        [Description("是否在所有状态下都显示边框")]
        [DefaultValue(true)]
        public bool AlwaysShowBorder
        {
            get { return _alwaysShowBorder; }
            set
            {
                _alwaysShowBorder = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置是否显示关闭按钮
        /// </summary>
        [Category("外观")]
        [Description("是否在标题栏显示关闭按钮")]
        [DefaultValue(true)]
        public bool ShowCloseButton
        {
            get { return _showCloseButton; }
            set
            {
                _showCloseButton = value;
                UpdateCloseButton();
            }
        }

        /// <summary>
        /// 获取或设置关闭按钮大小
        /// </summary>
        [Category("外观")]
        [Description("关闭按钮的大小")]
        [DefaultValue(25)]
        public int CloseButtonSize
        {
            get { return _closeButtonSize; }
            set
            {
                _closeButtonSize = Math.Max(16, value);
                UpdateLayout();
            }
        }

        #endregion

        #region 构造函数

        public HiveBaseFormText()
        {
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Microsoft YaHei", 9f);
            this.DoubleBuffered = true;

            // 保存原始边框样式
            _originalFormBorderStyle = this.FormBorderStyle;

            // 设置窗体样式以支持自定义绘制
            this.SetStyle(ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);

            InitializeCustomTitleBar();
            UpdateLayout();
        }

        #endregion

        #region 自定义标题栏初始化

        private void InitializeCustomTitleBar()
        {
            // 创建标题栏面板
            _titleBarPanel = new Panel
            {
                Height = 25,
                BackColor = HiveUIColors.Gray
            };

            // 创建标题标签（居中显示）
            _titleLabel = new Label
            {
                Text = this.Text,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                ForeColor = Color.Black,
                Font = new Font("Microsoft YaHei", 9f, FontStyle.Regular),
                BackColor = Color.Transparent
            };

            // 创建关闭按钮
            _closeButton = new Button
            {
                Size = new Size(_closeButtonSize, _closeButtonSize),
                Text = "X",
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Microsoft YaHei", 12f, FontStyle.Regular),
                ForeColor = Color.Black,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Right | AnchorStyles.Top
            };

            // 设置关闭按钮样式 - 移除所有颜色切换效果
            _closeButton.FlatAppearance.BorderSize = 0;
            _closeButton.FlatAppearance.MouseOverBackColor = Color.Transparent; // 移除悬停背景色
            _closeButton.FlatAppearance.MouseDownBackColor = Color.Transparent; // 移除按下背景色

            // 关闭按钮点击事件
            _closeButton.Click += (s, e) => this.Close();

            // 添加到标题栏面板
            _titleBarPanel.Controls.Add(_closeButton);
            _titleBarPanel.Controls.Add(_titleLabel);

            // 添加鼠标事件以实现拖动
            _titleBarPanel.MouseDown += TitleBarMouseDown;
            _titleLabel.MouseDown += TitleBarMouseDown;

            // 添加到窗体
            this.Controls.Add(_titleBarPanel);

            // 确保关闭按钮在最前面
            _closeButton.BringToFront();
        }

        private void TitleBarMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        #endregion

        #region 重写方法

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            if (!DesignMode && _useCustomTitleBar)
            {
                // 移除系统标题栏
                RemoveSystemTitleBar();
            }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            if (_titleLabel != null)
            {
                _titleLabel.Text = this.Text;
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            UpdateLayout();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // 始终绘制边框
            if (_alwaysShowBorder && _borderWidth > 0)
            {
                DrawCustomBorder(e.Graphics);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.Invalidate(); // 确保边框在调整大小时重绘
        }

        #endregion

        #region 私有方法

        private void RemoveSystemTitleBar()
        {
            if (this.IsHandleCreated)
            {
                int style = GetWindowLong(this.Handle, GWL_STYLE);
                SetWindowLong(this.Handle, GWL_STYLE, style & ~WS_SYSMENU);
            }
        }

        private void UpdateTitleBar()
        {
            if (_titleBarPanel != null)
            {
                _titleBarPanel.Visible = _useCustomTitleBar;
                UpdateLayout();
            }
        }

        private void UpdateCloseButton()
        {
            if (_closeButton != null)
            {
                _closeButton.Visible = _showCloseButton;
                UpdateLayout();
            }
        }

        private void UpdateLayout()
        {
            if (_titleBarPanel != null)
            {
                // 设置标题栏位置和大小（在边框内部）
                _titleBarPanel.Location = new Point(_borderWidth, _borderWidth);
                _titleBarPanel.Size = new Size(
                    this.ClientSize.Width - (_borderWidth * 2),
                    25
                );

                // 设置关闭按钮位置
                if (_closeButton != null)
                {
                    _closeButton.Location = new Point(
                        _titleBarPanel.Width - _closeButtonSize - 2,
                        (_titleBarPanel.Height - _closeButtonSize) / 2
                    );

                    // 确保按钮大小正确
                    _closeButton.Size = new Size(_closeButtonSize, _closeButtonSize);
                }

                // 设置标题标签的边距（为关闭按钮留出空间）
                if (_titleLabel != null)
                {
                    int rightMargin = _showCloseButton ? _closeButtonSize + 5 : 0;
                    _titleLabel.Margin = new Padding(0, 0, rightMargin, 0);
                }

                // 设置窗体内容区域的内边距
                int topPadding = _useCustomTitleBar ? 25 + _borderWidth : _borderWidth;
                this.Padding = new Padding(_borderWidth, topPadding, _borderWidth, _borderWidth);
            }
        }

        /// <summary>
        /// 绘制自定义边框
        /// </summary>
        private void DrawCustomBorder(Graphics g)
        {
            if (_borderWidth <= 0) return;

            using (Pen borderPen = new Pen(_borderColor, _borderWidth))
            {
                // 绘制四周边框
                // 上边框
                g.DrawLine(borderPen, 0, 0, this.ClientSize.Width, 0);

                // 下边框
                g.DrawLine(borderPen, 0, this.ClientSize.Height - _borderWidth,
                          this.ClientSize.Width, this.ClientSize.Height - _borderWidth);

                // 左边框
                g.DrawLine(borderPen, 0, 0, 0, this.ClientSize.Height);

                // 右边框
                g.DrawLine(borderPen, this.ClientSize.Width - _borderWidth, 0,
                          this.ClientSize.Width - _borderWidth, this.ClientSize.Height);

                // 如果需要更粗的边框，绘制多条线
                for (int i = 1; i < _borderWidth; i++)
                {
                    // 上边框
                    g.DrawLine(borderPen, i, i, this.ClientSize.Width - i, i);

                    // 下边框
                    g.DrawLine(borderPen, i, this.ClientSize.Height - i - 1,
                              this.ClientSize.Width - i, this.ClientSize.Height - i - 1);

                    // 左边框
                    g.DrawLine(borderPen, i, i, i, this.ClientSize.Height - i);

                    // 右边框
                    g.DrawLine(borderPen, this.ClientSize.Width - i - 1, i,
                              this.ClientSize.Width - i - 1, this.ClientSize.Height - i);
                }
            }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 设置标题栏颜色
        /// </summary>
        public void SetTitleBarColor(Color backColor, Color foreColor)
        {
            if (_titleBarPanel != null)
            {
                _titleBarPanel.BackColor = backColor;
                _titleLabel.ForeColor = foreColor;
            }
        }

        /// <summary>
        /// 设置关闭按钮颜色
        /// </summary>
        /// <param name="normalColor">正常状态颜色</param>
        /// <param name="foreColor">文字颜色</param>
        public void SetCloseButtonColor(Color normalColor, Color foreColor)
        {
            if (_closeButton != null)
            {
                _closeButton.BackColor = normalColor;
                _closeButton.ForeColor = foreColor;
            }
        }

        /// <summary>
        /// 设置窗体边框样式，同时保持自定义边框功能
        /// </summary>
        public new FormBorderStyle FormBorderStyle
        {
            get { return base.FormBorderStyle; }
            set
            {
                base.FormBorderStyle = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 应用无边框样式但保持自定义边框
        /// </summary>
        public void ApplyBorderlessStyle()
        {
            this.FormBorderStyle = FormBorderStyle.None;
        }

        /// <summary>
        /// 恢复原始边框样式
        /// </summary>
        public void RestoreOriginalBorderStyle()
        {
            this.FormBorderStyle = _originalFormBorderStyle;
        }

        #endregion
    }
    /// <summary>
    /// 椭圆形标签控件，支持圆角背景和自定义颜色
    /// </summary>
    public class EllipseLabel : Label
    {
        #region 私有字段
        private Color _backgroundColor = HiveUIColors.Gray; // 默认背景颜色
        private int _cornerRadius = 30; // 默认圆角半径
        #endregion

        #region 构造函数
        /// <summary>
        /// 初始化EllipseLabel
        /// </summary>
        public EllipseLabel()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 初始化控件属性
        /// </summary>
        private void InitializeComponent()
        {
            this.BackColor = Color.Transparent;
            this.AutoSize = false;
            this.TextAlign = ContentAlignment.MiddleCenter;
        }
        #endregion

        #region 公共属性
        /// <summary>
        /// 获取或设置椭圆形背景颜色
        /// </summary>
        [Category("外观")]
        [Description("椭圆形标签的背景颜色")]
        [DefaultValue(typeof(Color), "240, 240, 240")]
        public Color BackgroundColor
        {
            get { return _backgroundColor; }
            set
            {
                _backgroundColor = value;
                this.Invalidate(); // 重绘控件
            }
        }

        /// <summary>
        /// 获取或设置圆角半径
        /// </summary>
        [Category("外观")]
        [Description("椭圆形标签的圆角半径")]
        [DefaultValue(20)]
        public int CornerRadius
        {
            get { return _cornerRadius; }
            set
            {
                if (value < 0) value = 0;
                _cornerRadius = value;
                this.Invalidate(); // 重绘控件
            }
        }
        #endregion

        #region 重写的绘制方法
        /// <summary>
        /// 自定义绘制椭圆形标签
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            if (e == null) return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // 绘制椭圆形背景
            DrawEllipseBackground(e.Graphics);

            // 绘制文本
            DrawText(e.Graphics);
        }

        /// <summary>
        /// 绘制椭圆形背景
        /// </summary>
        private void DrawEllipseBackground(Graphics g)
        {
            using (GraphicsPath path = CreateRoundedRectanglePath())
            using (SolidBrush backgroundBrush = new SolidBrush(_backgroundColor))
            {
                g.FillPath(backgroundBrush, path);
            }
        }

        /// <summary>
        /// 创建圆角矩形路径
        /// </summary>
        private GraphicsPath CreateRoundedRectanglePath()
        {
            GraphicsPath path = new GraphicsPath();

            // 确保圆角半径不超过控件尺寸的一半
            int radius = Math.Min(_cornerRadius, Math.Min(Width, Height) /*/ 2*/);

            // 创建圆角矩形路径
            path.AddArc(0, 0, radius, radius, 180, 90); // 左上角
            path.AddArc(Width - radius, 0, radius, radius, 270, 90); // 右上角
            path.AddArc(Width - radius, Height - radius, radius, radius, 0, 90); // 右下角
            path.AddArc(0, Height - radius, radius, radius, 90, 90); // 左下角
            path.CloseAllFigures();

            return path;
        }

        /// <summary>
        /// 绘制文本
        /// </summary>
        private void DrawText(Graphics g)
        {
            if (string.IsNullOrEmpty(this.Text)) return;

            using (SolidBrush textBrush = new SolidBrush(this.ForeColor))
            using (StringFormat textFormat = CreateTextFormat())
            {
                RectangleF textRect = new RectangleF(0, 0, Width, Height);
                g.DrawString(this.Text, this.Font, textBrush, textRect, textFormat);
            }
        }

        /// <summary>
        /// 创建文本格式
        /// </summary>
        private StringFormat CreateTextFormat()
        {
            StringFormat format = new StringFormat();

            // 根据TextAlign属性设置文本对齐方式
            switch (this.TextAlign)
            {
                case ContentAlignment.TopLeft:
                    format.Alignment = StringAlignment.Near;
                    format.LineAlignment = StringAlignment.Near;
                    break;
                case ContentAlignment.TopCenter:
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Near;
                    break;
                case ContentAlignment.TopRight:
                    format.Alignment = StringAlignment.Far;
                    format.LineAlignment = StringAlignment.Near;
                    break;
                case ContentAlignment.MiddleLeft:
                    format.Alignment = StringAlignment.Near;
                    format.LineAlignment = StringAlignment.Center;
                    break;
                case ContentAlignment.MiddleCenter:
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Center;
                    break;
                case ContentAlignment.MiddleRight:
                    format.Alignment = StringAlignment.Far;
                    format.LineAlignment = StringAlignment.Center;
                    break;
                case ContentAlignment.BottomLeft:
                    format.Alignment = StringAlignment.Near;
                    format.LineAlignment = StringAlignment.Far;
                    break;
                case ContentAlignment.BottomCenter:
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Far;
                    break;
                case ContentAlignment.BottomRight:
                    format.Alignment = StringAlignment.Far;
                    format.LineAlignment = StringAlignment.Far;
                    break;
                default:
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Center;
                    break;
            }

            return format;
        }
        #endregion

        #region 重写的方法
        /// <summary>
        /// 响应文本更改事件
        /// </summary>
        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            this.Invalidate(); // 文本改变时重绘
        }

        /// <summary>
        /// 响应字体更改事件
        /// </summary>
        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            this.Invalidate(); // 字体改变时重绘
        }

        /// <summary>
        /// 响应尺寸更改事件
        /// </summary>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            this.Invalidate(); // 尺寸改变时重绘
        }
        #endregion

        #region 资源清理
        /// <summary>
        /// 清理使用的资源
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            // 如果有需要释放的非托管资源，可以在这里添加
            base.Dispose(disposing);
        }
        #endregion
    }

    /// <summary>
    /// 自定义下拉框控件，支持边框自定义、提示文字颜色和自定义下拉箭头
    /// </summary>
    public class CustomComboBox : ComboBox
    {
        #region 私有字段
        private Color _borderColor = Color.Red;
        private int _borderWidth = 1;
        private Color _placeholderColor = Color.LightGray;
        private string _placeholderText = "";
        private bool _hasSelection = false;
        private Color _originalForeColor = Color.Black;
        private Color _arrowColor = Color.Black;
        private int _arrowWidth = 20;  // 箭头宽度
        private int _arrowHeight = 15; // 箭头高度
        private bool _isPainting = false;
        #endregion

        #region 构造函数
        public CustomComboBox()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            //_originalForeColor = this.ForeColor;

            // 设置双缓冲和重绘样式
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.ResizeRedraw |
                         ControlStyles.UserPaint, true);

            this.DrawMode = DrawMode.OwnerDrawFixed;
            this.DropDownStyle = ComboBoxStyle.DropDownList;
            this.FlatStyle = FlatStyle.Flat;

            this.Size = new Size(121, 21);
        }
        #endregion

        #region 公共属性
        [Category("外观")]
        [Description("控件的边框颜色")]
        [DefaultValue(typeof(Color), "Red")]
        public Color BorderColor
        {
            get { return _borderColor; }
            set
            {
                _borderColor = value;
                SafeInvalidate();
            }
        }

        [Category("外观")]
        [Description("控件的边框宽度")]
        [DefaultValue(1)]
        public int BorderWidth
        {
            get { return _borderWidth; }
            set
            {
                if (value < 1) value = 1;
                _borderWidth = value;
                SafeInvalidate();
            }
        }

        [Category("外观")]
        [Description("未选择任何选项时的提示文字颜色")]
        [DefaultValue(typeof(Color), "LightGray")]
        public Color PlaceholderColor
        {
            get { return _placeholderColor; }
            set
            {
                _placeholderColor = value;
                UpdateTextColor();
            }
        }
        [Category("外观")]
        [Description("选择选项时的文字颜色")]
        [DefaultValue(typeof(Color), "LightGray")]
        public Color OriginalForeColor
        {
            get { return _originalForeColor; }
            set
            {
                _originalForeColor = value;
                UpdateTextColor();
            }
        }
        [Category("外观")]
        [Description("未选择任何选项时显示的提示文字")]
        [DefaultValue("")]
        public string PlaceholderText
        {
            get { return _placeholderText; }
            set
            {
                _placeholderText = value;
                UpdateTextColor();
                SafeInvalidate();
            }
        }

        [Category("外观")]
        [Description("下拉箭头的颜色")]
        [DefaultValue(typeof(Color), "Black")]
        public Color ArrowColor
        {
            get { return _arrowColor; }
            set
            {
                _arrowColor = value;
                SafeInvalidate();
            }
        }

        [Category("外观")]
        [Description("下拉箭头的宽度")]
        [DefaultValue(20)]
        public int ArrowWidth
        {
            get { return _arrowWidth; }
            set
            {
                if (value < 4) value = 4;
                if (value > 50) value = 50;
                _arrowWidth = value;
                SafeInvalidate();
            }
        }

        [Category("外观")]
        [Description("下拉箭头的高度")]
        [DefaultValue(15)]
        public int ArrowHeight
        {
            get { return _arrowHeight; }
            set
            {
                if (value < 3) value = 3;
                if (value > 50) value = 50;
                _arrowHeight = value;
                SafeInvalidate();
            }
        }
        #endregion

        #region 安全方法
        /// <summary>
        /// 安全的刷新方法
        /// </summary>
        private void SafeInvalidate()
        {
            if (!this.IsHandleCreated || _isPainting)
                return;

            this.Invalidate();
        }
        #endregion

        #region 重写的绘制方法
        protected override void OnPaint(PaintEventArgs e)
        {
            if (_isPainting)
            {
                base.OnPaint(e);
                return;
            }

            try
            {
                _isPainting = true;

                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                // 绘制背景
                using (SolidBrush backBrush = new SolidBrush(this.BackColor))
                {
                    e.Graphics.FillRectangle(backBrush, this.ClientRectangle);
                }

                // 绘制边框
                DrawBorder(e.Graphics);

                // 绘制文本
                DrawText(e.Graphics);

                // 绘制下拉箭头
                DrawArrow(e.Graphics);
            }
            catch (Exception ex)
            {
                // 绘制出错时回退到默认绘制
                System.Diagnostics.Debug.WriteLine("CustomComboBox绘制错误: " + ex.Message);
                base.OnPaint(e);
            }
            finally
            {
                _isPainting = false;
            }
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            try
            {
                e.DrawBackground();

                string text = this.GetItemText(this.Items[e.Index]);

                // 修改：使用原始前景色而不是当前前景色，确保下拉项颜色与选中项一致
                Color textColor = (e.State & DrawItemState.Selected) == DrawItemState.Selected
                    ? SystemColors.HighlightText
                    : _originalForeColor;

                using (SolidBrush textBrush = new SolidBrush(textColor))
                {
                    e.Graphics.DrawString(text, this.Font, textBrush, e.Bounds);
                }

                e.DrawFocusRectangle();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("CustomComboBox绘制项错误: " + ex.Message);
            }
        }
        #endregion

        #region 绘制方法
        private void DrawBorder(Graphics g)
        {
            using (Pen borderPen = new Pen(_borderColor, _borderWidth))
            {
                Rectangle borderRect = new Rectangle(
                    _borderWidth / 2,
                    _borderWidth / 2,
                    this.Width - _borderWidth - 1,
                    this.Height - _borderWidth - 1);
                g.DrawRectangle(borderPen, borderRect);
            }
        }

        private void DrawArrow(Graphics g)
        {
            // 修改：动态计算箭头位置，确保完整显示
            int rightPadding = 5; // 箭头距离右侧的边距
            int arrowX = this.Width - rightPadding - _arrowWidth;

            // 确保箭头不会超出左边界
            if (arrowX < 0)
                arrowX = 0;

            int arrowY = (this.Height - _arrowHeight) / 2;

            // 使用独立的宽度和高度绘制三角形
            Point[] points = new Point[]
            {
                new Point(arrowX, arrowY), // 左上角
                new Point(arrowX + _arrowWidth, arrowY), // 右上角
                new Point(arrowX + (_arrowWidth / 2), arrowY + _arrowHeight) // 底部中点
            };

            using (SolidBrush arrowBrush = new SolidBrush(_arrowColor))
            {
                g.FillPolygon(arrowBrush, points);
            }
        }

        private void DrawText(Graphics g)
        {
            string displayText = this.Text;

            // 处理提示文字
            if (string.IsNullOrEmpty(displayText) && !string.IsNullOrEmpty(_placeholderText) && !_hasSelection)
            {
                displayText = _placeholderText;
            }

            if (string.IsNullOrEmpty(displayText)) return;

            // 修改：动态计算文本区域，考虑箭头宽度
            int leftPadding = 3 + _borderWidth;
            int rightPadding = 5 + _arrowWidth; // 考虑箭头宽度
            int textWidth = this.Width - leftPadding - rightPadding;

            // 确保文本宽度不为负
            if (textWidth < 0)
                textWidth = 0;

            Rectangle textRect = new Rectangle(leftPadding, _borderWidth, textWidth, this.Height - (_borderWidth * 2));

            // 根据状态选择文本颜色
            Color textColor;
            if (!this.Enabled)
            {
                textColor = SystemColors.GrayText;
            }
            else if (!_hasSelection && !string.IsNullOrEmpty(_placeholderText))
            {
                textColor = _placeholderColor;
            }
            else
            {
                // 修改：有选中项时使用原始前景色
                textColor = _originalForeColor;
            }

            using (SolidBrush textBrush = new SolidBrush(textColor))
            using (StringFormat textFormat = new StringFormat())
            {
                textFormat.LineAlignment = StringAlignment.Center;
                textFormat.Alignment = StringAlignment.Near;
                textFormat.Trimming = StringTrimming.EllipsisCharacter;

                g.DrawString(displayText, this.Font, textBrush, textRect, textFormat);
            }
        }

        private void UpdateTextColor()
        {
            if (!this.IsHandleCreated) return;

            _hasSelection = this.SelectedIndex >= 0 ||
                         (!string.IsNullOrEmpty(this.Text) && this.Text != _placeholderText);

            // 修改：只更新显示颜色，不影响下拉项颜色
            if (_hasSelection)
            {
                // 有选中项时使用原始颜色
                // 注意：这里不修改this.ForeColor，因为下拉项绘制使用_originalForeColor
                this.ForeColor = _originalForeColor;
            }
            else
            {
                // 没有选中项时使用提示文字颜色
                this.ForeColor = _placeholderColor;
            }

            SafeInvalidate();
        }
        #endregion

        #region 重写的事件方法
        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            UpdateTextColor();
            base.OnSelectedIndexChanged(e);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            UpdateTextColor();
            base.OnTextChanged(e);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            // 确保句柄已创建后更新颜色
            if (this.IsHandleCreated)
            {
                this.BeginInvoke(new Action(() =>
                {
                    UpdateTextColor();
                }));
            }
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            SafeInvalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            SafeInvalidate();
        }
        #endregion

        #region 重写属性
        public override string Text
        {
            get
            {
                if (base.Text == _placeholderText && !_hasSelection)
                    return "";
                return base.Text;
            }
            set
            {
                base.Text = value;
                UpdateTextColor();
            }
        }

        public override Color ForeColor
        {
            get { return base.ForeColor; }
            set
            {
                base.ForeColor = value;
                //_originalForeColor = value; // 保存原始颜色
                SafeInvalidate();
            }
        }
        #endregion

        #region 设计时支持
        /// <summary>
        /// 设计器支持 - 返回默认值
        /// </summary>
        private void ResetBorderColor() { BorderColor = Color.Red; }
        private bool ShouldSerializeBorderColor() { return BorderColor != Color.Red; }

        private void ResetPlaceholderColor() { PlaceholderColor = Color.LightGray; }
        private bool ShouldSerializePlaceholderColor() { return PlaceholderColor != Color.LightGray; }

        private void ResetArrowColor() { ArrowColor = Color.Black; }
        private bool ShouldSerializeArrowColor() { return ArrowColor != Color.Black; }

        private void ResetArrowWidth() { ArrowWidth = 20; }
        private bool ShouldSerializeArrowWidth() { return ArrowWidth != 20; }

        private void ResetArrowHeight() { ArrowHeight = 15; }
        private bool ShouldSerializeArrowHeight() { return ArrowHeight != 15; }
        #endregion
    }





    /// <summary>
    /// 自定义表格布局控件，支持设置外边框和内边框颜色，以及提示文本功能
    /// </summary>
    public class CustomTableLayoutPanel : TableLayoutPanel
    {
        #region 私有字段
        private Color _outerBorderColor = Color.Gray;
        private Color _innerBorderColor = Color.LightGray;
        private int _outerBorderWidth = 1;
        private int _innerBorderWidth = 1;
        private bool _showInnerBorders = true;
        private bool _showOuterBorder = true;

        // 提示文本相关字段
        private string _tipText = string.Empty;
        private Color _tipTextColor = Color.Gray;
        private Font _tipFont = SystemFonts.DefaultFont;
        private int _tipRow = 0;
        private int _tipColumn = 0;
        private int _tipRowSpan = 1;
        private int _tipColumnSpan = 1;
        private bool _tipVisible = false;
        private ContentAlignment _tipTextAlignment = ContentAlignment.MiddleCenter;

        // 提示文本相对位置和尺寸字段
        private int _tipOffsetX = 0;
        private int _tipOffsetY = 0;
        private int _tipWidth = -1;  // -1 表示自动宽度
        private int _tipHeight = -1; // -1 表示自动高度
        private bool _tipAutoSize = true;
        private Padding _tipMargin = new Padding(0);
        private Padding _tipPadding = new Padding(5);
        private Color _tipBackColor = Color.Transparent;
        private bool _tipShowBackground = false;
        #endregion

        #region 构造函数
        /// <summary>
        /// 初始化CustomTableLayoutPanel
        /// </summary>
        public CustomTableLayoutPanel()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 初始化控件属性
        /// </summary>
        private void InitializeComponent()
        {
            // 设置双缓冲和重绘样式
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.ResizeRedraw |
                         ControlStyles.UserPaint |
                         ControlStyles.AllPaintingInWmPaint, true);

            // 设置默认属性
            this.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
            this.BackColor = Color.White;
            this.Size = new Size(200, 200);
        }
        #endregion

        #region 公共属性 - 边框
        /// <summary>
        /// 获取或设置外边框颜色
        /// </summary>
        [Category("外观")]
        [Description("控件外边框的颜色")]
        [DefaultValue(typeof(Color), "Gray")]
        public Color OuterBorderColor
        {
            get { return _outerBorderColor; }
            set
            {
                _outerBorderColor = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置内边框颜色
        /// </summary>
        [Category("外观")]
        [Description("单元格之间内边框的颜色")]
        [DefaultValue(typeof(Color), "LightGray")]
        public Color InnerBorderColor
        {
            get { return _innerBorderColor; }
            set
            {
                _innerBorderColor = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置外边框宽度
        /// </summary>
        [Category("外观")]
        [Description("外边框的宽度")]
        [DefaultValue(1)]
        public int OuterBorderWidth
        {
            get { return _outerBorderWidth; }
            set
            {
                if (value < 0) value = 0;
                if (value > 10) value = 10;
                _outerBorderWidth = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置内边框宽度
        /// </summary>
        [Category("外观")]
        [Description("内边框的宽度")]
        [DefaultValue(1)]
        public int InnerBorderWidth
        {
            get { return _innerBorderWidth; }
            set
            {
                if (value < 0) value = 0;
                if (value > 5) value = 5;
                _innerBorderWidth = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置是否显示内边框
        /// </summary>
        [Category("外观")]
        [Description("是否显示单元格之间的内边框")]
        [DefaultValue(true)]
        public bool ShowInnerBorders
        {
            get { return _showInnerBorders; }
            set
            {
                _showInnerBorders = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置是否显示外边框
        /// </summary>
        [Category("外观")]
        [Description("是否显示控件的外边框")]
        [DefaultValue(true)]
        public bool ShowOuterBorder
        {
            get { return _showOuterBorder; }
            set
            {
                _showOuterBorder = value;
                this.Invalidate();
            }
        }
        #endregion

        #region 公共属性 - 提示文本基础
        /// <summary>
        /// 获取或设置提示文本内容
        /// </summary>
        [Category("提示文本")]
        [Description("显示的提示文本内容")]
        [DefaultValue("")]
        public string TipText
        {
            get { return _tipText; }
            set
            {
                _tipText = value ?? string.Empty;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置提示文本颜色
        /// </summary>
        [Category("提示文本")]
        [Description("提示文本的颜色")]
        [DefaultValue(typeof(Color), "Gray")]
        public Color TipTextColor
        {
            get { return _tipTextColor; }
            set
            {
                _tipTextColor = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置提示文本字体
        /// </summary>
        [Category("提示文本")]
        [Description("提示文本的字体")]
        public Font TipFont
        {
            get { return _tipFont; }
            set
            {
                _tipFont = value ?? SystemFonts.DefaultFont;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置提示文本所在行
        /// </summary>
        [Category("提示文本")]
        [Description("提示文本所在的行索引（从0开始）")]
        [DefaultValue(0)]
        public int TipRow
        {
            get { return _tipRow; }
            set
            {
                if (value < 0) value = 0;
                _tipRow = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置提示文本所在列
        /// </summary>
        [Category("提示文本")]
        [Description("提示文本所在的列索引（从0开始）")]
        [DefaultValue(0)]
        public int TipColumn
        {
            get { return _tipColumn; }
            set
            {
                if (value < 0) value = 0;
                _tipColumn = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置提示文本横跨的行数
        /// </summary>
        [Category("提示文本")]
        [Description("提示文本横跨的行数")]
        [DefaultValue(1)]
        public int TipRowSpan
        {
            get { return _tipRowSpan; }
            set
            {
                if (value < 1) value = 1;
                _tipRowSpan = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置提示文本横跨的列数
        /// </summary>
        [Category("提示文本")]
        [Description("提示文本横跨的列数")]
        [DefaultValue(1)]
        public int TipColumnSpan
        {
            get { return _tipColumnSpan; }
            set
            {
                if (value < 1) value = 1;
                _tipColumnSpan = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置提示文本是否可见
        /// </summary>
        [Category("提示文本")]
        [Description("是否显示提示文本")]
        [DefaultValue(false)]
        public bool TipVisible
        {
            get { return _tipVisible; }
            set
            {
                if (_tipVisible != value)
                {
                    _tipVisible = value;
                    this.Invalidate();
                    OnTipVisibleChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// 获取或设置提示文本对齐方式
        /// </summary>
        [Category("提示文本")]
        [Description("提示文本的对齐方式")]
        [DefaultValue(ContentAlignment.MiddleCenter)]
        public ContentAlignment TipTextAlignment
        {
            get { return _tipTextAlignment; }
            set
            {
                _tipTextAlignment = value;
                this.Invalidate();
            }
        }
        #endregion

        #region 公共属性 - 提示文本位置和尺寸
        /// <summary>
        /// 获取或设置提示文本的水平偏移量
        /// </summary>
        [Category("提示文本位置")]
        [Description("提示文本相对于单元格的水平偏移量（像素）")]
        [DefaultValue(0)]
        public int TipOffsetX
        {
            get { return _tipOffsetX; }
            set
            {
                _tipOffsetX = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置提示文本的垂直偏移量
        /// </summary>
        [Category("提示文本位置")]
        [Description("提示文本相对于单元格的垂直偏移量（像素）")]
        [DefaultValue(0)]
        public int TipOffsetY
        {
            get { return _tipOffsetY; }
            set
            {
                _tipOffsetY = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置提示文本的宽度（-1表示自动宽度）
        /// </summary>
        [Category("提示文本位置")]
        [Description("提示文本的宽度（-1表示自动宽度）")]
        [DefaultValue(-1)]
        public int TipWidth
        {
            get { return _tipWidth; }
            set
            {
                _tipWidth = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置提示文本的高度（-1表示自动高度）
        /// </summary>
        [Category("提示文本位置")]
        [Description("提示文本的高度（-1表示自动高度）")]
        [DefaultValue(-1)]
        public int TipHeight
        {
            get { return _tipHeight; }
            set
            {
                _tipHeight = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置提示文本是否自动调整大小
        /// </summary>
        [Category("提示文本位置")]
        [Description("提示文本是否自动根据内容调整大小")]
        [DefaultValue(true)]
        public bool TipAutoSize
        {
            get { return _tipAutoSize; }
            set
            {
                _tipAutoSize = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置提示文本的外边距
        /// </summary>
        [Category("提示文本位置")]
        [Description("提示文本与单元格边界的外边距")]
        public Padding TipMargin
        {
            get { return _tipMargin; }
            set
            {
                _tipMargin = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置提示文本的内边距
        /// </summary>
        [Category("提示文本位置")]
        [Description("提示文本内容与文本边界的内边距")]
        public Padding TipPadding
        {
            get { return _tipPadding; }
            set
            {
                _tipPadding = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置提示文本的背景色
        /// </summary>
        [Category("提示文本位置")]
        [Description("提示文本的背景颜色")]
        [DefaultValue(typeof(Color), "Transparent")]
        public Color TipBackColor
        {
            get { return _tipBackColor; }
            set
            {
                _tipBackColor = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置是否显示提示文本背景
        /// </summary>
        [Category("提示文本位置")]
        [Description("是否显示提示文本的背景")]
        [DefaultValue(false)]
        public bool TipShowBackground
        {
            get { return _tipShowBackground; }
            set
            {
                _tipShowBackground = value;
                this.Invalidate();
            }
        }
        #endregion

        #region 事件
        /// <summary>
        /// 提示文本可见性改变时触发的事件
        /// </summary>
        [Category("行为")]
        [Description("当提示文本的可见性改变时触发")]
        public event EventHandler TipVisibleChanged;
        #endregion

        #region 重写的绘制方法
        /// <summary>
        /// 自定义绘制表格布局控件
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            // 先调用基类绘制背景和子控件
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // 绘制内边框
            if (_showInnerBorders && _innerBorderWidth > 0)
            {
                DrawInnerBorders(e.Graphics);
            }

            // 绘制外边框
            if (_showOuterBorder && _outerBorderWidth > 0)
            {
                DrawOuterBorder(e.Graphics);
            }

            // 绘制提示文本（在最后绘制，确保在最前面）
            if (_tipVisible && !string.IsNullOrEmpty(_tipText))
            {
                DrawTipText(e.Graphics);
            }
        }
        #endregion

        #region 绘制方法
        /// <summary>
        /// 绘制外边框
        /// </summary>
        private void DrawOuterBorder(Graphics g)
        {
            using (Pen borderPen = new Pen(_outerBorderColor, _outerBorderWidth))
            {
                Rectangle borderRect = new Rectangle(
                    _outerBorderWidth / 2,
                    _outerBorderWidth / 2,
                    this.Width - _outerBorderWidth,
                    this.Height - _outerBorderWidth);

                g.DrawRectangle(borderPen, borderRect);
            }
        }

        /// <summary>
        /// 绘制内边框（单元格分隔线）
        /// </summary>
        private void DrawInnerBorders(Graphics g)
        {
            using (Pen innerPen = new Pen(_innerBorderColor, _innerBorderWidth))
            {
                // 绘制列分隔线
                DrawColumnBorders(g, innerPen);

                // 绘制行分隔线
                DrawRowBorders(g, innerPen);
            }
        }

        /// <summary>
        /// 绘制列分隔线
        /// </summary>
        private void DrawColumnBorders(Graphics g, Pen pen)
        {
            if (this.ColumnCount <= 1) return;

            int totalWidth = 0;
            for (int i = 0; i < this.ColumnCount - 1; i++)
            {
                // 计算列宽
                int columnWidth = GetColumnWidth(i);
                if (columnWidth <= 0) continue;

                totalWidth += columnWidth;

                // 绘制垂直线
                int x = totalWidth;
                g.DrawLine(pen, x, 0, x, this.Height);
            }
        }

        /// <summary>
        /// 绘制行分隔线
        /// </summary>
        private void DrawRowBorders(Graphics g, Pen pen)
        {
            if (this.RowCount <= 1) return;

            int totalHeight = 0;
            for (int i = 0; i < this.RowCount - 1; i++)
            {
                // 计算行高
                int rowHeight = GetRowHeight(i);
                if (rowHeight <= 0) continue;

                totalHeight += rowHeight;

                // 绘制水平线
                int y = totalHeight;
                g.DrawLine(pen, 0, y, this.Width, y);
            }
        }

        /// <summary>
        /// 绘制提示文本（按单行形式绘制多行内容）
        /// </summary>
        private void DrawTipText(Graphics g)
        {
            Rectangle tipRect = GetTipTextRectangle();
            if (tipRect.IsEmpty)
            {
                tipRect = new Rectangle(10, 10, this.Width - 20, 60);
            }

            // 应用外边距
            tipRect = ApplyMargin(tipRect, _tipMargin);

            // 应用偏移量
            tipRect.Offset(_tipOffsetX, _tipOffsetY);

            // 应用固定尺寸
            if (_tipWidth > 0)
            {
                tipRect.Width = _tipWidth;
            }
            if (_tipHeight > 0)
            {
                tipRect.Height = _tipHeight;
            }

            // 确保矩形有效
            if (tipRect.Width <= 0) tipRect.Width = 100;
            if (tipRect.Height <= 0) tipRect.Height = 60;

            // 绘制背景
            if (_tipShowBackground && _tipBackColor != Color.Transparent)
            {
                using (Brush backBrush = new SolidBrush(_tipBackColor))
                {
                    g.FillRectangle(backBrush, tipRect);
                }
            }

            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // 应用内边距得到文本区域
            Rectangle textRect = ApplyPadding(tipRect, _tipPadding);

            // 确保文本区域有效
            if (textRect.Width <= 0) textRect.Width = 90;
            if (textRect.Height <= 0) textRect.Height = 50;

            using (Brush textBrush = new SolidBrush(_tipTextColor))
            using (StringFormat format = CreateTipStringFormat())
            {
                // 修复：保留空行，不要移除空条目
                string[] lines = _tipText.Split(new[] { "@@" }, StringSplitOptions.RemoveEmptyEntries);

                if (lines.Length == 0) return;

                // 计算单行高度
                float lineHeight = _tipFont.GetHeight(g);
                float totalTextHeight = lines.Length * lineHeight;

                // 如果启用自动大小，测量文本尺寸
                if (_tipAutoSize && (_tipWidth <= 0 || _tipHeight <= 0))
                {
                    // 测量最宽的行
                    float maxLineWidth = 0;
                    foreach (string line in lines)
                    {
                        if (string.IsNullOrEmpty(line)) continue;
                        SizeF lineSize = g.MeasureString(line, _tipFont, textRect.Width, format);
                        maxLineWidth = Math.Max(maxLineWidth, lineSize.Width);
                    }

                    // 调整矩形以适应文本
                    if (_tipWidth <= 0)
                    {
                        textRect.Width = (int)Math.Ceiling(maxLineWidth);
                        tipRect.Width = textRect.Width + _tipPadding.Horizontal;
                    }
                    if (_tipHeight <= 0)
                    {
                        textRect.Height = (int)Math.Ceiling(totalTextHeight);
                        tipRect.Height = textRect.Height + _tipPadding.Vertical;
                    }

                    // 重新对齐文本矩形
                    textRect = AlignTextRectangle(tipRect, textRect, format);
                }

                // 计算起始Y位置（垂直对齐）
                float startY = textRect.Y;
                switch (format.LineAlignment)
                {
                    case StringAlignment.Center:
                        startY = textRect.Y + (textRect.Height - totalTextHeight) / 2;
                        break;
                    case StringAlignment.Far:
                        startY = textRect.Bottom - totalTextHeight;
                        break;
                    case StringAlignment.Near:
                        startY = textRect.Y;
                        break;
                }
                // 计算提示文本框顶部与第一行顶部的差值
                float tipTopToFirstLineDelta = startY - tipRect.Top;

                // 逐行绘制文本
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];

                    // 计算当前行的位置
                    float currentY = startY + i * lineHeight;

                    float currentLineDelta = currentY - tipRect.Top;

                    // 确保在当前文本区域内
                    if (currentY + lineHeight > textRect.Bottom)
                    {
                        // 如果超出底部边界，停止绘制
                        break;
                    }

                    // 绘制单行文本（即使是空行也保留位置）
                    RectangleF lineRect = new RectangleF(textRect.X, currentY, textRect.Width, totalTextHeight);

                    // 只有非空文本才实际绘制
                    if (!string.IsNullOrEmpty(line))
                    {
                        g.DrawString(line, _tipFont, textBrush, lineRect, format);
                    }
                }
                LastTipTopToLineDelta = tipTopToFirstLineDelta;
            }
        }

        /// <summary>
        /// 获取提示文本框顶部与文本第一行顶部的Y方向差值
        /// </summary>
        public float GetTipTopToFirstLineDelta()
        {
            try
            {
                using (Graphics g = this.CreateGraphics())
                {
                    Rectangle tipRect = GetTipTextRectangle();
                    if (tipRect.IsEmpty)
                    {
                        tipRect = new Rectangle(10, 10, this.Width - 20, 60);
                    }

                    // 应用外边距和偏移量
                    tipRect = ApplyMargin(tipRect, _tipMargin);
                    tipRect.Offset(_tipOffsetX, _tipOffsetY);

                    // 应用固定尺寸
                    if (_tipWidth > 0) tipRect.Width = _tipWidth;
                    if (_tipHeight > 0) tipRect.Height = _tipHeight;

                    // 应用内边距得到文本区域
                    Rectangle textRect = ApplyPadding(tipRect, _tipPadding);

                    using (StringFormat format = CreateTipStringFormat())
                    {
                        string[] lines = _tipText.Split(new[] { "@@" }, StringSplitOptions.RemoveEmptyEntries);
                        if (lines.Length == 0) return 0;

                        // 计算单行高度和总高度
                        float lineHeight = _tipFont.GetHeight(g);
                        float totalTextHeight = lines.Length * lineHeight;

                        // 计算起始Y位置（垂直对齐）
                        float startY = textRect.Y;
                        switch (format.LineAlignment)
                        {
                            case StringAlignment.Center:
                                startY = textRect.Y + (textRect.Height - totalTextHeight) / 2;
                                break;
                            case StringAlignment.Far:
                                startY = textRect.Bottom - totalTextHeight;
                                break;
                            case StringAlignment.Near:
                                startY = textRect.Y;
                                break;
                        }

                        // 返回提示文本框顶部与第一行顶部的差值
                        return startY - tipRect.Top;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetTipTopToFirstLineDelta出错: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// 获取提示文本框顶部与指定行顶部的Y方向差值
        /// </summary>
        /// <param name="lineIndex">行索引（从0开始）</param>
        public float GetTipTopToLineDelta(int lineIndex)
        {
            try
            {
                using (Graphics g = this.CreateGraphics())
                {
                    Rectangle tipRect = GetTipTextRectangle();
                    if (tipRect.IsEmpty)
                    {
                        tipRect = new Rectangle(10, 10, this.Width - 20, 60);
                    }

                    // 应用外边距和偏移量
                    tipRect = ApplyMargin(tipRect, _tipMargin);
                    tipRect.Offset(_tipOffsetX, _tipOffsetY);

                    // 应用固定尺寸
                    if (_tipWidth > 0) tipRect.Width = _tipWidth;
                    if (_tipHeight > 0) tipRect.Height = _tipHeight;

                    // 应用内边距得到文本区域
                    Rectangle textRect = ApplyPadding(tipRect, _tipPadding);

                    using (StringFormat format = CreateTipStringFormat())
                    {
                        string[] lines = _tipText.Split(new[] { "@@" }, StringSplitOptions.RemoveEmptyEntries);
                        if (lines.Length == 0 || lineIndex >= lines.Length) return 0;

                        // 计算单行高度和总高度
                        float lineHeight = _tipFont.GetHeight(g);
                        float totalTextHeight = lines.Length * lineHeight;

                        // 计算起始Y位置（垂直对齐）
                        float startY = textRect.Y;
                        switch (format.LineAlignment)
                        {
                            case StringAlignment.Center:
                                startY = textRect.Y + (textRect.Height - totalTextHeight) / 2;
                                break;
                            case StringAlignment.Far:
                                startY = textRect.Bottom - totalTextHeight;
                                break;
                            case StringAlignment.Near:
                                startY = textRect.Y;
                                break;
                        }

                        // 计算指定行的位置
                        float currentY = startY + lineIndex * lineHeight;

                        // 返回提示文本框顶部与指定行顶部的差值
                        return currentY - tipRect.Top;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetTipTopToLineDelta出错: {ex.Message}");
                return 0;
            }
        }

        // 添加一个公共属性来存储最后一次计算的差值
        public float LastTipTopToLineDelta { get; private set; }
        /// <summary>
        /// 根据对齐方式调整文本矩形位置
        /// </summary>
        private Rectangle AlignTextRectangle(Rectangle containerRect, Rectangle textRect, StringFormat format)
        {
            Rectangle result = textRect;

            // 水平对齐
            switch (format.Alignment)
            {
                case StringAlignment.Center:
                    result.X = containerRect.X + (containerRect.Width - textRect.Width) / 2;
                    break;
                case StringAlignment.Far:
                    result.X = containerRect.Right - textRect.Width - _tipPadding.Right;
                    break;
                case StringAlignment.Near:
                    result.X = containerRect.X + _tipPadding.Left;
                    break;
            }

            // 垂直对齐
            switch (format.LineAlignment)
            {
                case StringAlignment.Center:
                    result.Y = containerRect.Y + (containerRect.Height - textRect.Height) / 2;
                    break;
                case StringAlignment.Far:
                    result.Y = containerRect.Bottom - textRect.Height - _tipPadding.Bottom;
                    break;
                case StringAlignment.Near:
                    result.Y = containerRect.Y + _tipPadding.Top;
                    break;
            }

            return result;
        }

        /// <summary>
        /// 应用外边距
        /// </summary>
        private Rectangle ApplyMargin(Rectangle rect, Padding margin)
        {
            return new Rectangle(
                rect.X + margin.Left,
                rect.Y + margin.Top,
                rect.Width - margin.Horizontal,
                rect.Height - margin.Vertical);
        }

        /// <summary>
        /// 应用内边距
        /// </summary>
        private Rectangle ApplyPadding(Rectangle rect, Padding padding)
        {
            return new Rectangle(
                rect.X + padding.Left,
                rect.Y + padding.Top,
                rect.Width - padding.Horizontal,
                rect.Height - padding.Vertical);
        }

        /// <summary>
        /// 创建提示文本的格式
        /// </summary>
        private StringFormat CreateTipStringFormat()
        {
            StringFormat format = new StringFormat();

            switch (_tipTextAlignment)
            {
                case ContentAlignment.TopLeft:
                    format.Alignment = StringAlignment.Near;
                    format.LineAlignment = StringAlignment.Near;
                    break;
                case ContentAlignment.TopCenter:
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Near;
                    break;
                case ContentAlignment.TopRight:
                    format.Alignment = StringAlignment.Far;
                    format.LineAlignment = StringAlignment.Near;
                    break;
                case ContentAlignment.MiddleLeft:
                    format.Alignment = StringAlignment.Near;
                    format.LineAlignment = StringAlignment.Center;
                    break;
                case ContentAlignment.MiddleCenter:
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Center;
                    break;
                case ContentAlignment.MiddleRight:
                    format.Alignment = StringAlignment.Far;
                    format.LineAlignment = StringAlignment.Center;
                    break;
                case ContentAlignment.BottomLeft:
                    format.Alignment = StringAlignment.Near;
                    format.LineAlignment = StringAlignment.Far;
                    break;
                case ContentAlignment.BottomCenter:
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Far;
                    break;
                case ContentAlignment.BottomRight:
                    format.Alignment = StringAlignment.Far;
                    format.LineAlignment = StringAlignment.Far;
                    break;
            }

            format.Trimming = StringTrimming.EllipsisCharacter;
            format.FormatFlags = StringFormatFlags.LineLimit;
            // 移除了 NoWrap 标志，允许文本在单行内自动换行（如果需要）
            return format;
        }
        #endregion

        #region 辅助方法
        /// <summary>
        /// 获取提示文本的显示区域
        /// </summary>
        private Rectangle GetTipTextRectangle()
        {
            if (this.RowCount == 0 || this.ColumnCount == 0)
                return new Rectangle(10, 10, this.Width - 20, 60); // 返回默认矩形

            // 确保行列索引在有效范围内
            int row = Math.Min(_tipRow, this.RowCount - 1);
            int column = Math.Min(_tipColumn, this.ColumnCount - 1);
            int rowSpan = Math.Min(_tipRowSpan, this.RowCount - row);
            int columnSpan = Math.Min(_tipColumnSpan, this.ColumnCount - column);

            int x = 0;
            int y = 0;
            int width = 0;
            int height = 0;

            try
            {
                // 计算起始位置和宽度
                for (int i = 0; i < column; i++)
                {
                    x += GetColumnWidth(i);
                }
                for (int i = column; i < column + columnSpan; i++)
                {
                    width += GetColumnWidth(i);
                }

                // 计算起始位置和高度
                for (int i = 0; i < row; i++)
                {
                    y += GetRowHeight(i);
                }
                for (int i = row; i < row + rowSpan; i++)
                {
                    height += GetRowHeight(i);
                }

                // 确保尺寸有效
                if (width <= 0) width = 100;
                if (height <= 0) height = 60;

                return new Rectangle(x, y, width, height);
            }
            catch (Exception)
            {
                return new Rectangle(10, 10, this.Width - 20, 60); // 返回默认矩形
            }
        }

        /// <summary>
        /// 获取指定列的宽度
        /// </summary>
        private int GetColumnWidth(int columnIndex)
        {
            if (columnIndex < 0 || columnIndex >= this.ColumnCount)
                return 0;

            try
            {
                // 处理绝对大小的列
                if (this.ColumnStyles[columnIndex].SizeType == SizeType.Absolute)
                {
                    return (int)this.ColumnStyles[columnIndex].Width;
                }

                // 处理百分比大小的列 - 简化计算
                int totalAbsoluteWidth = 0;
                float totalPercentage = 0;

                for (int i = 0; i < this.ColumnCount; i++)
                {
                    if (this.ColumnStyles[i].SizeType == SizeType.Absolute)
                    {
                        totalAbsoluteWidth += (int)this.ColumnStyles[i].Width;
                    }
                    else if (this.ColumnStyles[i].SizeType == SizeType.Percent)
                    {
                        totalPercentage += this.ColumnStyles[i].Width;
                    }
                }

                int availableWidth = this.Width - totalAbsoluteWidth;
                if (this.ColumnStyles[columnIndex].SizeType == SizeType.Percent && totalPercentage > 0)
                {
                    return (int)(availableWidth * (this.ColumnStyles[columnIndex].Width / totalPercentage));
                }

                // 默认返回平均宽度
                return this.Width / Math.Max(1, this.ColumnCount);
            }
            catch (Exception)
            {
                return this.Width / Math.Max(1, this.ColumnCount);
            }
        }

        /// <summary>
        /// 获取指定行的高度
        /// </summary>
        private int GetRowHeight(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= this.RowCount)
                return 0;

            try
            {
                // 处理绝对大小的行
                if (this.RowStyles[rowIndex].SizeType == SizeType.Absolute)
                {
                    return (int)this.RowStyles[rowIndex].Height;
                }

                // 处理百分比大小的行 - 简化计算
                int totalAbsoluteHeight = 0;
                float totalPercentage = 0;

                for (int i = 0; i < this.RowCount; i++)
                {
                    if (this.RowStyles[i].SizeType == SizeType.Absolute)
                    {
                        totalAbsoluteHeight += (int)this.RowStyles[i].Height;
                    }
                    else if (this.RowStyles[i].SizeType == SizeType.Percent)
                    {
                        totalPercentage += this.RowStyles[i].Height;
                    }
                }

                int availableHeight = this.Height - totalAbsoluteHeight;
                if (this.RowStyles[rowIndex].SizeType == SizeType.Percent && totalPercentage > 0)
                {
                    return (int)(availableHeight * (this.RowStyles[rowIndex].Height / totalPercentage));
                }

                // 默认返回平均高度
                return this.Height / Math.Max(1, this.RowCount);
            }
            catch (Exception)
            {
                return this.Height / Math.Max(1, this.RowCount);
            }
        }
        #endregion

        #region 事件方法
        /// <summary>
        /// 触发提示文本可见性改变事件
        /// </summary>
        protected virtual void OnTipVisibleChanged(EventArgs e)
        {
            TipVisibleChanged?.Invoke(this, e);
        }

        /// <summary>
        /// 布局改变时触发
        /// </summary>
        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            this.Invalidate();
        }

        /// <summary>
        /// 尺寸改变时触发
        /// </summary>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.Invalidate();
        }
        #endregion

        #region 设计时支持
        /// <summary>
        /// 设计器支持 - 返回默认值
        /// </summary>
        private void ResetOuterBorderColor() { OuterBorderColor = Color.Gray; }
        private bool ShouldSerializeOuterBorderColor() { return OuterBorderColor != Color.Gray; }

        private void ResetInnerBorderColor() { InnerBorderColor = Color.LightGray; }
        private bool ShouldSerializeInnerBorderColor() { return InnerBorderColor != Color.LightGray; }

        private void ResetOuterBorderWidth() { OuterBorderWidth = 1; }
        private bool ShouldSerializeOuterBorderWidth() { return OuterBorderWidth != 1; }

        private void ResetInnerBorderWidth() { InnerBorderWidth = 1; }
        private bool ShouldSerializeInnerBorderWidth() { return InnerBorderWidth != 1; }

        private void ResetTipTextColor() { TipTextColor = Color.Gray; }
        private bool ShouldSerializeTipTextColor() { return TipTextColor != Color.Gray; }

        private void ResetTipFont() { TipFont = SystemFonts.DefaultFont; }
        private bool ShouldSerializeTipFont() { return TipFont != SystemFonts.DefaultFont; }

        private void ResetTipMargin() { TipMargin = new Padding(0); }
        private bool ShouldSerializeTipMargin() { return TipMargin != new Padding(0); }

        private void ResetTipPadding() { TipPadding = new Padding(5); }
        private bool ShouldSerializeTipPadding() { return TipPadding != new Padding(5); }

        private void ResetTipBackColor() { TipBackColor = Color.Transparent; }
        private bool ShouldSerializeTipBackColor() { return TipBackColor != Color.Transparent; }
        #endregion

        #region 公共方法
        /// <summary>
        /// 显示提示文本
        /// </summary>
        public void ShowTip()
        {
            TipVisible = true;
        }

        /// <summary>
        /// 隐藏提示文本
        /// </summary>
        public void HideTip()
        {
            TipVisible = false;
        }

        /// <summary>
        /// 切换提示文本的显示状态
        /// </summary>
        public void ToggleTip()
        {
            TipVisible = !TipVisible;
        }

        /// <summary>
        /// 设置提示文本位置和尺寸
        /// </summary>
        public void SetTipPosition(int offsetX, int offsetY, int width = -1, int height = -1)
        {
            _tipOffsetX = offsetX;
            _tipOffsetY = offsetY;
            _tipWidth = width;
            _tipHeight = height;
            this.Invalidate();
        }

        /// <summary>
        /// 设置提示文本边距
        /// </summary>
        public void SetTipPadding(Padding margin, Padding padding)
        {
            _tipMargin = margin;
            _tipPadding = padding;
            this.Invalidate();
        }

        /// <summary>
        /// 自动调整提示文本大小以适应内容（多行版本）
        /// </summary>
        public void AutoSizeTip()
        {
            try
            {
                using (Graphics g = this.CreateGraphics())
                {
                    Rectangle baseRect = GetTipTextRectangle();
                    Rectangle textRect = ApplyPadding(ApplyMargin(baseRect, _tipMargin), _tipPadding);

                    using (StringFormat format = CreateTipStringFormat())
                    {
                        // 将文本按行分割
                        string[] lines = _tipText.Split(new[] { "@@" }, StringSplitOptions.RemoveEmptyEntries);

                        if (lines.Length == 0) return;

                        // 计算单行高度和最宽行宽度
                        float lineHeight = _tipFont.GetHeight(g);
                        float maxLineWidth = 0;

                        foreach (string line in lines)
                        {
                            SizeF lineSize = g.MeasureString(line, _tipFont, textRect.Width, format);
                            maxLineWidth = Math.Max(maxLineWidth, lineSize.Width);
                        }

                        float totalTextHeight = lines.Length * lineHeight;

                        _tipWidth = (int)Math.Ceiling(maxLineWidth) + _tipPadding.Horizontal + _tipMargin.Horizontal;
                        _tipHeight = (int)Math.Ceiling(totalTextHeight) + _tipPadding.Vertical + _tipMargin.Vertical;
                    }
                }
                this.Invalidate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AutoSizeTip出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 调试方法：显示当前提示文本设置
        /// </summary>
        public void DebugTipInfo()
        {
            System.Diagnostics.Debug.WriteLine($"TipVisible: {_tipVisible}");
            System.Diagnostics.Debug.WriteLine($"TipText: '{_tipText}'");
            System.Diagnostics.Debug.WriteLine($"TipRect: {GetTipTextRectangle()}");
            System.Diagnostics.Debug.WriteLine($"TipFont: {_tipFont}");
            System.Diagnostics.Debug.WriteLine($"TipColor: {_tipTextColor}");
        }
        #endregion

        #region 资源清理
        /// <summary>
        /// 清理使用的资源
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _tipFont?.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion
    }


    /// <summary>
    /// 双行标签控件，支持分别控制两行文本的字体和大小
    /// </summary>
    public class DualLineLabel : Label
    {
        #region 私有字段
        private string _line1Text = "";
        private string _line2Text = "";
        private Font _line1Font;
        private Font _line2Font;
        private Color _line1Color = Color.Black;
        private Color _line2Color = Color.Black;
        private int _lineSpacing = 5; // 行间距
        private ContentAlignment _line1Alignment = ContentAlignment.MiddleCenter;
        private ContentAlignment _line2Alignment = ContentAlignment.MiddleCenter;
        private bool _autoAdjustHeight = true; // 自动调整高度
        #endregion

        #region 构造函数
        /// <summary>
        /// 初始化DualLineLabel
        /// </summary>
        public DualLineLabel()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 初始化控件属性
        /// </summary>
        private void InitializeComponent()
        {
            // 设置默认字体
            _line1Font = new Font("Microsoft YaHei", 12f, FontStyle.Bold);
            _line2Font = new Font("Microsoft YaHei", 9f, FontStyle.Regular);

            // 设置默认属性
            this.AutoSize = false;
            this.BackColor = Color.Transparent;
            this.Size = new Size(150, 60);

            // 设置双缓冲减少闪烁
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.ResizeRedraw |
                         ControlStyles.UserPaint, true);
        }
        #endregion

        #region 公共属性
        /// <summary>
        /// 获取或设置第一行文本
        /// </summary>
        [Category("外观")]
        [Description("第一行显示的文本")]
        [DefaultValue("")]
        public string Line1Text
        {
            get { return _line1Text; }
            set
            {
                _line1Text = value ?? "";
                AdjustControlSize();
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置第二行文本
        /// </summary>
        [Category("外观")]
        [Description("第二行显示的文本")]
        [DefaultValue("")]
        public string Line2Text
        {
            get { return _line2Text; }
            set
            {
                _line2Text = value ?? "";
                AdjustControlSize();
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置第一行文本字体
        /// </summary>
        [Category("外观")]
        [Description("第一行文本的字体")]
        public Font Line1Font
        {
            get { return _line1Font; }
            set
            {
                _line1Font = value ?? new Font("Microsoft YaHei", 12f, FontStyle.Bold);
                AdjustControlSize();
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置第二行文本字体
        /// </summary>
        [Category("外观")]
        [Description("第二行文本的字体")]
        public Font Line2Font
        {
            get { return _line2Font; }
            set
            {
                _line2Font = value ?? new Font("Microsoft YaHei", 9f, FontStyle.Regular);
                AdjustControlSize();
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置第一行文本颜色
        /// </summary>
        [Category("外观")]
        [Description("第一行文本的颜色")]
        [DefaultValue(typeof(Color), "Black")]
        public Color Line1Color
        {
            get { return _line1Color; }
            set
            {
                _line1Color = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置第二行文本颜色
        /// </summary>
        [Category("外观")]
        [Description("第二行文本的颜色")]
        [DefaultValue(typeof(Color), "Black")]
        public Color Line2Color
        {
            get { return _line2Color; }
            set
            {
                _line2Color = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置行间距
        /// </summary>
        [Category("布局")]
        [Description("两行文本之间的间距")]
        [DefaultValue(5)]
        public int LineSpacing
        {
            get { return _lineSpacing; }
            set
            {
                _lineSpacing = Math.Max(0, value);
                AdjustControlSize();
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置第一行文本对齐方式
        /// </summary>
        [Category("布局")]
        [Description("第一行文本的对齐方式")]
        [DefaultValue(ContentAlignment.MiddleCenter)]
        public ContentAlignment Line1Alignment
        {
            get { return _line1Alignment; }
            set
            {
                _line1Alignment = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置第二行文本对齐方式
        /// </summary>
        [Category("布局")]
        [Description("第二行文本的对齐方式")]
        [DefaultValue(ContentAlignment.MiddleCenter)]
        public ContentAlignment Line2Alignment
        {
            get { return _line2Alignment; }
            set
            {
                _line2Alignment = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置是否自动调整控件高度
        /// </summary>
        [Category("行为")]
        [Description("是否根据文本内容自动调整控件高度")]
        [DefaultValue(true)]
        public bool AutoAdjustHeight
        {
            get { return _autoAdjustHeight; }
            set
            {
                _autoAdjustHeight = value;
                if (value) AdjustControlSize();
                this.Invalidate();
            }
        }
        #endregion

        #region 重写的绘制方法
        /// <summary>
        /// 自定义绘制双行标签
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // 绘制背景
            using (SolidBrush backBrush = new SolidBrush(this.BackColor))
            {
                e.Graphics.FillRectangle(backBrush, this.ClientRectangle);
            }

            // 绘制第一行文本
            if (!string.IsNullOrEmpty(_line1Text))
            {
                DrawTextLine(e.Graphics, _line1Text, _line1Font, _line1Color, _line1Alignment, true);
            }

            // 绘制第二行文本
            if (!string.IsNullOrEmpty(_line2Text))
            {
                DrawTextLine(e.Graphics, _line2Text, _line2Font, _line2Color, _line2Alignment, false);
            }

            // 绘制边框（如果有）
            if (this.BorderStyle != BorderStyle.None)
            {
                ControlPaint.DrawBorder(e.Graphics, this.ClientRectangle, this.ForeColor, this.BorderStyle == BorderStyle.FixedSingle ? ButtonBorderStyle.Solid : ButtonBorderStyle.Inset);
            }
        }
        #endregion

        #region 绘制方法
        /// <summary>
        /// 绘制单行文本
        /// </summary>
        private void DrawTextLine(Graphics g, string text, Font font, Color color, ContentAlignment alignment, bool isFirstLine)
        {
            using (SolidBrush textBrush = new SolidBrush(color))
            using (StringFormat format = CreateStringFormat(alignment))
            {
                // 计算文本区域
                RectangleF textRect = CalculateTextRectangle(g, text, font, isFirstLine);

                // 如果文本太长，进行截断
                string displayText = TruncateText(g, text, font, textRect.Width);

                g.DrawString(displayText, font, textBrush, textRect, format);
            }
        }

        /// <summary>
        /// 计算文本绘制区域
        /// </summary>
        private RectangleF CalculateTextRectangle(Graphics g, string text, Font font, bool isFirstLine)
        {
            // 测量文本大小
            SizeF textSize = g.MeasureString(text, font);

            float lineHeight = textSize.Height;
            float totalHeight = 0f;

            // 计算总高度
            if (!string.IsNullOrEmpty(_line1Text))
            {
                totalHeight += g.MeasureString(_line1Text, _line1Font).Height;
            }
            if (!string.IsNullOrEmpty(_line2Text))
            {
                totalHeight += g.MeasureString(_line2Text, _line2Font).Height;
            }
            if (!string.IsNullOrEmpty(_line1Text) && !string.IsNullOrEmpty(_line2Text))
            {
                totalHeight += _lineSpacing;
            }

            // 计算起始Y坐标
            float startY = (this.Height - totalHeight) / 2;
            float currentY = startY;

            // 如果是第一行，使用当前Y坐标
            if (isFirstLine)
            {
                return new RectangleF(0, currentY, this.Width, lineHeight);
            }
            else
            {
                // 如果是第二行，计算第二行的Y坐标
                float firstLineHeight = g.MeasureString(_line1Text, _line1Font).Height;
                currentY = startY + firstLineHeight + _lineSpacing;
                return new RectangleF(0, currentY, this.Width, lineHeight);
            }
        }

        /// <summary>
        /// 创建文本格式
        /// </summary>
        private StringFormat CreateStringFormat(ContentAlignment alignment)
        {
            StringFormat format = new StringFormat();

            // 设置水平对齐
            switch (alignment)
            {
                case ContentAlignment.TopLeft:
                case ContentAlignment.MiddleLeft:
                case ContentAlignment.BottomLeft:
                    format.Alignment = StringAlignment.Near;
                    break;
                case ContentAlignment.TopCenter:
                case ContentAlignment.MiddleCenter:
                case ContentAlignment.BottomCenter:
                    format.Alignment = StringAlignment.Center;
                    break;
                case ContentAlignment.TopRight:
                case ContentAlignment.MiddleRight:
                case ContentAlignment.BottomRight:
                    format.Alignment = StringAlignment.Far;
                    break;
            }

            // 设置垂直对齐（在计算矩形时已经处理了垂直位置，这里设置为Near）
            format.LineAlignment = StringAlignment.Near;
            format.Trimming = StringTrimming.EllipsisCharacter;
            format.FormatFlags = StringFormatFlags.NoWrap;

            return format;
        }

        /// <summary>
        /// 截断文本以适应宽度
        /// </summary>
        private string TruncateText(Graphics g, string text, Font font, float maxWidth)
        {
            const string ellipsis = "...";

            if (string.IsNullOrEmpty(text) || maxWidth <= 0)
                return text;

            SizeF textSize = g.MeasureString(text, font);
            if (textSize.Width <= maxWidth)
                return text;

            // 二分查找找到合适的截断位置
            int min = 0;
            int max = text.Length;
            int mid = 0;
            string truncatedText = "";

            while (min <= max)
            {
                mid = (min + max) / 2;
                truncatedText = text.Substring(0, mid) + ellipsis;
                textSize = g.MeasureString(truncatedText, font);

                if (textSize.Width <= maxWidth)
                {
                    min = mid + 1;
                }
                else
                {
                    max = mid - 1;
                }
            }

            return max > 0 ? text.Substring(0, max) + ellipsis : ellipsis;
        }
        #endregion

        #region 辅助方法
        /// <summary>
        /// 调整控件尺寸
        /// </summary>
        private void AdjustControlSize()
        {
            if (!_autoAdjustHeight || !this.IsHandleCreated) return;

            using (Graphics g = this.CreateGraphics())
            {
                float totalHeight = 0f;

                // 计算第一行高度
                if (!string.IsNullOrEmpty(_line1Text))
                {
                    totalHeight += g.MeasureString(_line1Text, _line1Font).Height;
                }

                // 计算第二行高度
                if (!string.IsNullOrEmpty(_line2Text))
                {
                    totalHeight += g.MeasureString(_line2Text, _line2Font).Height;
                }

                // 添加行间距（如果两行都有文本）
                if (!string.IsNullOrEmpty(_line1Text) && !string.IsNullOrEmpty(_line2Text))
                {
                    totalHeight += _lineSpacing;
                }

                // 添加一些边距
                totalHeight += 4;

                // 设置新的高度
                int newHeight = (int)Math.Ceiling(totalHeight);
                if (newHeight != this.Height)
                {
                    this.Height = newHeight;
                }
            }
        }
        #endregion

        #region 重写的事件方法
        /// <summary>
        /// 文本改变时触发
        /// </summary>
        protected override void OnTextChanged(EventArgs e)
        {
            // 基类的Text属性用于存储第一行文本
            _line1Text = this.Text;
            AdjustControlSize();
            base.OnTextChanged(e);
        }

        /// <summary>
        /// 字体改变时触发
        /// </summary>
        protected override void OnFontChanged(EventArgs e)
        {
            // 基类的Font属性用作第一行字体
            _line1Font = this.Font;
            AdjustControlSize();
            base.OnFontChanged(e);
        }

        /// <summary>
        /// 尺寸改变时触发
        /// </summary>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.Invalidate();
        }
        #endregion

        #region 重写属性
        /// <summary>
        /// 获取或设置控件文本（作为第一行文本）
        /// </summary>
        public override string Text
        {
            get { return _line1Text; }
            set
            {
                _line1Text = value ?? "";
                base.Text = value;
                AdjustControlSize();
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置控件字体（作为第一行字体）
        /// </summary>
        public override Font Font
        {
            get { return _line1Font; }
            set
            {
                _line1Font = value ?? new Font("Microsoft YaHei", 12f, FontStyle.Bold);
                base.Font = value;
                AdjustControlSize();
                this.Invalidate();
            }
        }
        #endregion

        #region 设计时支持
        /// <summary>
        /// 设计器支持 - 返回默认值
        /// </summary>
        private void ResetLine1Font() { Line1Font = new Font("Microsoft YaHei", 12f, FontStyle.Bold); }
        private bool ShouldSerializeLine1Font() { return !Line1Font.Equals(new Font("Microsoft YaHei", 12f, FontStyle.Bold)); }

        private void ResetLine2Font() { Line2Font = new Font("Microsoft YaHei", 9f, FontStyle.Regular); }
        private bool ShouldSerializeLine2Font() { return !Line2Font.Equals(new Font("Microsoft YaHei", 9f, FontStyle.Regular)); }

        private void ResetLine1Color() { Line1Color = Color.Black; }
        private bool ShouldSerializeLine1Color() { return Line1Color != Color.Black; }

        private void ResetLine2Color() { Line2Color = Color.Black; }
        private bool ShouldSerializeLine2Color() { return Line2Color != Color.Black; }
        #endregion

        #region 资源清理
        /// <summary>
        /// 清理使用的资源
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _line1Font?.Dispose();
                _line2Font?.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion
    }


    public class StatusIndicatorLabel : Label
    {
        #region 私有字段
        private string _line1 = "";
        private string _line2 = "";
        private Font _line1Font;
        private Font _line2Font;
        private Image _leftImage;
        private int _imagePadding = 5;
        private int _textPadding = 10;
        private bool _isNormalStatus = true;
        private Color _normalTextColor = Color.Black;
        private Color _dimmedTextColor = Color.Gray;
        private float _dimmedImageOpacity = 0.6f;
        private int _lineSpacing = 5;
        private ContentAlignment _line1Alignment = ContentAlignment.MiddleLeft;
        private ContentAlignment _line2Alignment = ContentAlignment.MiddleLeft;
        private bool _autoAdjustHeight = true;
        #endregion

        #region 构造函数
        /// <summary>
        /// 初始化StatusIndicatorLabel
        /// </summary>
        public StatusIndicatorLabel()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 初始化控件属性
        /// </summary>
        private void InitializeComponent()
        {
            // 设置默认字体
            _line1Font = new Font("Microsoft YaHei", 12f, FontStyle.Bold);
            _line2Font = new Font("Microsoft YaHei", 9f, FontStyle.Regular);

            // 设置默认属性
            this.AutoSize = false;
            this.BackColor = Color.FromArgb(240, 240, 240);
            this.Size = new Size(250, 80);

            // 设置双缓冲减少闪烁
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.ResizeRedraw |
                         ControlStyles.UserPaint, true);
        }
        #endregion

        #region 公共属性
        /// <summary>
        /// 获取或设置第一行文本
        /// </summary>
        [Category("外观")]
        [Description("标签的第一行文本")]
        [DefaultValue("")]
        public string Line1
        {
            get { return _line1; }
            set
            {
                _line1 = value ?? "";
                AdjustControlSize();
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置第二行文本
        /// </summary>
        [Category("外观")]
        [Description("标签的第二行文本")]
        [DefaultValue("")]
        public string Line2
        {
            get { return _line2; }
            set
            {
                _line2 = value ?? "";
                AdjustControlSize();
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置第一行文本字体
        /// </summary>
        [Category("外观")]
        [Description("第一行文本的字体")]
        public Font Line1Font
        {
            get { return _line1Font; }
            set
            {
                _line1Font = value ?? new Font("Microsoft YaHei", 12f, FontStyle.Bold);
                AdjustControlSize();
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置第二行文本字体
        /// </summary>
        [Category("外观")]
        [Description("第二行文本的字体")]
        public Font Line2Font
        {
            get { return _line2Font; }
            set
            {
                _line2Font = value ?? new Font("Microsoft YaHei", 9f, FontStyle.Regular);
                AdjustControlSize();
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置左侧图片
        /// </summary>
        [Category("外观")]
        [Description("标签左侧显示的图片")]
        [DefaultValue(null)]
        public Image LeftImage
        {
            get { return _leftImage; }
            set
            {
                _leftImage = value;
                AdjustControlSize();
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置图片内边距
        /// </summary>
        [Category("布局")]
        [Description("图片与边框的内边距")]
        [DefaultValue(5)]
        public int ImagePadding
        {
            get { return _imagePadding; }
            set
            {
                _imagePadding = Math.Max(0, value);
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置文本内边距
        /// </summary>
        [Category("布局")]
        [Description("文本与右侧边框的内边距")]
        [DefaultValue(10)]
        public int TextPadding
        {
            get { return _textPadding; }
            set
            {
                _textPadding = Math.Max(0, value);
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置状态指示（true为正常状态，false为暗淡状态）
        /// </summary>
        [Category("行为")]
        [Description("指示标签的状态，true为正常状态，false为暗淡状态")]
        [DefaultValue(true)]
        public bool IsNormalStatus
        {
            get { return _isNormalStatus; }
            set
            {
                if (_isNormalStatus != value)
                {
                    _isNormalStatus = value;
                    this.Invalidate();
                }
            }
        }

        /// <summary>
        /// 获取或设置正常状态下的文字颜色
        /// </summary>
        [Category("外观")]
        [Description("正常状态下的文字颜色")]
        [DefaultValue(typeof(Color), "Black")]
        public Color NormalTextColor
        {
            get { return _normalTextColor; }
            set
            {
                _normalTextColor = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置暗淡状态下的文字颜色
        /// </summary>
        [Category("外观")]
        [Description("暗淡状态下的文字颜色")]
        [DefaultValue(typeof(Color), "Gray")]
        public Color DimmedTextColor
        {
            get { return _dimmedTextColor; }
            set
            {
                _dimmedTextColor = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置暗淡状态下图片的透明度（0.0-1.0）
        /// </summary>
        [Category("外观")]
        [Description("暗淡状态下图片的透明度（0.0-1.0）")]
        [DefaultValue(0.6f)]
        public float DimmedImageOpacity
        {
            get { return _dimmedImageOpacity; }
            set
            {
                _dimmedImageOpacity = Math.Max(0.0f, Math.Min(1.0f, value));
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置行间距
        /// </summary>
        [Category("布局")]
        [Description("两行文本之间的间距")]
        [DefaultValue(5)]
        public int LineSpacing
        {
            get { return _lineSpacing; }
            set
            {
                _lineSpacing = Math.Max(0, value);
                AdjustControlSize();
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置第一行文本对齐方式
        /// </summary>
        [Category("布局")]
        [Description("第一行文本的对齐方式")]
        [DefaultValue(ContentAlignment.MiddleLeft)]
        public ContentAlignment Line1Alignment
        {
            get { return _line1Alignment; }
            set
            {
                _line1Alignment = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置第二行文本对齐方式
        /// </summary>
        [Category("布局")]
        [Description("第二行文本的对齐方式")]
        [DefaultValue(ContentAlignment.MiddleLeft)]
        public ContentAlignment Line2Alignment
        {
            get { return _line2Alignment; }
            set
            {
                _line2Alignment = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置是否自动调整控件高度
        /// </summary>
        [Category("行为")]
        [Description("是否根据文本内容自动调整控件高度")]
        [DefaultValue(true)]
        public bool AutoAdjustHeight
        {
            get { return _autoAdjustHeight; }
            set
            {
                _autoAdjustHeight = value;
                if (value) AdjustControlSize();
                this.Invalidate();
            }
        }
        #endregion

        #region 重写的绘制方法
        /// <summary>
        /// 自定义绘制标签
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            if (e == null) return;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            DrawBackground(e.Graphics);

            int contentStartX = CalculateContentStartX();
            DrawTextContent(e.Graphics, contentStartX);
            DrawBorder(e.Graphics);
        }

        /// <summary>
        /// 绘制标签背景
        /// </summary>
        private void DrawBackground(Graphics g)
        {
            using (SolidBrush backBrush = new SolidBrush(this.BackColor))
            {
                g.FillRectangle(backBrush, this.ClientRectangle);
            }
        }

        /// <summary>
        /// 计算内容起始X坐标
        /// </summary>
        private int CalculateContentStartX()
        {
            int contentStartX = _imagePadding;

            if (_leftImage != null)
            {
                int imageSize = Math.Min(this.Height - _imagePadding * 2, 50);
                contentStartX += imageSize + _imagePadding;
            }

            return contentStartX;
        }

        /// <summary>
        /// 绘制文本内容
        /// </summary>
        private void DrawTextContent(Graphics g, int contentStartX)
        {
            if (_leftImage != null)
            {
                DrawLeftImage(g);
            }

            int textAreaX = contentStartX;
            int textAreaWidth = this.Width - textAreaX - _textPadding;

            DrawLineText(g, _line1, _line1Font, textAreaX, textAreaWidth, true);
            DrawLineText(g, _line2, _line2Font, textAreaX, textAreaWidth, false);
        }

        /// <summary>
        /// 绘制左侧图片
        /// </summary>
        private void DrawLeftImage(Graphics g)
        {
            int imageSize = Math.Min(this.Height - _imagePadding * 2, 50);
            int imageY = (this.Height - imageSize) / 2;

            if (_isNormalStatus)
            {
                // 正常状态：直接绘制图片
                g.DrawImage(_leftImage, _imagePadding, imageY, imageSize, imageSize);
            }
            else
            {
                // 暗淡状态：使用颜色矩阵调整图片透明度
                ColorMatrix colorMatrix = new ColorMatrix();
                colorMatrix.Matrix33 = _dimmedImageOpacity; // 设置透明度

                ImageAttributes imageAttributes = new ImageAttributes();
                imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                g.DrawImage(
                    _leftImage,
                    new Rectangle(_imagePadding, imageY, imageSize, imageSize),
                    0, 0, _leftImage.Width, _leftImage.Height,
                    GraphicsUnit.Pixel,
                    imageAttributes);
            }
        }

        /// <summary>
        /// 绘制单行文本
        /// </summary>
        private void DrawLineText(Graphics g, string text, Font font, int x, int maxWidth, bool isFirstLine)
        {
            if (string.IsNullOrEmpty(text)) return;

            // 根据状态选择文字颜色
            Color textColor = _isNormalStatus ? _normalTextColor : _dimmedTextColor;

            using (SolidBrush textBrush = new SolidBrush(textColor))
            using (StringFormat format = CreateStringFormat(isFirstLine ? _line1Alignment : _line2Alignment))
            {
                string displayText = TruncateText(g, text, font, maxWidth);
                RectangleF textRect = CalculateTextRectangle(g, displayText, font, isFirstLine);

                g.DrawString(displayText, font, textBrush, textRect, format);
            }
        }

        /// <summary>
        /// 计算文本绘制区域
        /// </summary>
        private RectangleF CalculateTextRectangle(Graphics g, string text, Font font, bool isFirstLine)
        {
            SizeF textSize = g.MeasureString(text, font);
            float lineHeight = textSize.Height;

            // 计算总文本高度
            float totalTextHeight = 0f;
            if (!string.IsNullOrEmpty(_line1))
                totalTextHeight += g.MeasureString(_line1, _line1Font).Height;
            if (!string.IsNullOrEmpty(_line2))
                totalTextHeight += g.MeasureString(_line2, _line2Font).Height;
            if (!string.IsNullOrEmpty(_line1) && !string.IsNullOrEmpty(_line2))
                totalTextHeight += _lineSpacing;

            // 计算起始Y坐标
            float startY = (this.Height - totalTextHeight) / 2;
            float currentY = startY;

            // 如果是第一行，使用当前Y坐标
            if (isFirstLine)
            {
                return new RectangleF(CalculateContentStartX(), currentY, this.Width - CalculateContentStartX() - _textPadding, lineHeight);
            }
            else
            {
                // 如果是第二行，计算第二行的Y坐标
                float firstLineHeight = !string.IsNullOrEmpty(_line1) ? g.MeasureString(_line1, _line1Font).Height : 0;
                currentY = startY + firstLineHeight + _lineSpacing;
                return new RectangleF(CalculateContentStartX(), currentY, this.Width - CalculateContentStartX() - _textPadding, lineHeight);
            }
        }

        /// <summary>
        /// 创建文本格式
        /// </summary>
        private StringFormat CreateStringFormat(ContentAlignment alignment)
        {
            StringFormat format = new StringFormat();

            // 设置水平对齐
            switch (alignment)
            {
                case ContentAlignment.TopLeft:
                case ContentAlignment.MiddleLeft:
                case ContentAlignment.BottomLeft:
                    format.Alignment = StringAlignment.Near;
                    break;
                case ContentAlignment.TopCenter:
                case ContentAlignment.MiddleCenter:
                case ContentAlignment.BottomCenter:
                    format.Alignment = StringAlignment.Center;
                    break;
                case ContentAlignment.TopRight:
                case ContentAlignment.MiddleRight:
                case ContentAlignment.BottomRight:
                    format.Alignment = StringAlignment.Far;
                    break;
            }

            format.LineAlignment = StringAlignment.Near;
            format.Trimming = StringTrimming.EllipsisCharacter;
            format.FormatFlags = StringFormatFlags.NoWrap;

            return format;
        }

        /// <summary>
        /// 绘制边框
        /// </summary>
        private void DrawBorder(Graphics g)
        {
            if (this.BorderStyle == BorderStyle.FixedSingle)
            {
                using (Pen borderPen = new Pen(Color.FromArgb(200, 200, 200), 1))
                {
                    g.DrawRectangle(borderPen, 0, 0, this.Width - 1, this.Height - 1);
                }
            }
        }
        #endregion

        #region 文本处理
        /// <summary>
        /// 截断文本以适应指定宽度
        /// </summary>
        private string TruncateText(Graphics g, string text, Font font, int maxWidth)
        {
            const string ellipsis = "...";

            if (string.IsNullOrEmpty(text) || maxWidth <= 0)
                return text;

            SizeF textSize = g.MeasureString(text, font);
            if (textSize.Width <= maxWidth)
                return text;

            // 使用二分查找确定合适的截断位置
            int left = 0;
            int right = text.Length;
            string truncatedText;

            while (left <= right)
            {
                int mid = (left + right) / 2;
                truncatedText = text.Substring(0, mid) + ellipsis;
                textSize = g.MeasureString(truncatedText, font);

                if (textSize.Width <= maxWidth)
                {
                    left = mid + 1;
                }
                else
                {
                    right = mid - 1;
                }
            }

            return right > 0 ? text.Substring(0, right) + ellipsis : ellipsis;
        }
        #endregion

        #region 辅助方法
        /// <summary>
        /// 调整控件尺寸
        /// </summary>
        private void AdjustControlSize()
        {
            if (!_autoAdjustHeight || !this.IsHandleCreated) return;

            using (Graphics g = this.CreateGraphics())
            {
                float totalHeight = 0f;

                // 计算第一行高度
                if (!string.IsNullOrEmpty(_line1))
                {
                    totalHeight += g.MeasureString(_line1, _line1Font).Height;
                }

                // 计算第二行高度
                if (!string.IsNullOrEmpty(_line2))
                {
                    totalHeight += g.MeasureString(_line2, _line2Font).Height;
                }

                // 添加行间距（如果两行都有文本）
                if (!string.IsNullOrEmpty(_line1) && !string.IsNullOrEmpty(_line2))
                {
                    totalHeight += _lineSpacing;
                }

                // 添加图片高度（如果存在图片）
                if (_leftImage != null)
                {
                    int imageSize = Math.Min((int)totalHeight - _imagePadding * 2, 50);
                    totalHeight = Math.Max(totalHeight, imageSize + _imagePadding * 2);
                }

                // 添加一些边距
                totalHeight += 4;

                // 设置新的高度
                int newHeight = (int)Math.Ceiling(totalHeight);
                if (newHeight != this.Height)
                {
                    this.Height = newHeight;
                }
            }
        }
        #endregion

        #region 重写的事件方法
        /// <summary>
        /// 文本改变时触发
        /// </summary>
        protected override void OnTextChanged(EventArgs e)
        {
            // 基类的Text属性用于存储第一行文本
            _line1 = this.Text;
            AdjustControlSize();
            base.OnTextChanged(e);
        }

        /// <summary>
        /// 字体改变时触发
        /// </summary>
        protected override void OnFontChanged(EventArgs e)
        {
            // 基类的Font属性用作第一行字体
            _line1Font = this.Font;
            AdjustControlSize();
            base.OnFontChanged(e);
        }

        /// <summary>
        /// 尺寸改变时触发
        /// </summary>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.Invalidate();
        }
        #endregion

        #region 重写属性
        /// <summary>
        /// 获取或设置控件文本（作为第一行文本）
        /// </summary>
        public override string Text
        {
            get { return _line1; }
            set
            {
                _line1 = value ?? "";
                base.Text = value;
                AdjustControlSize();
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置控件字体（作为第一行字体）
        /// </summary>
        public override Font Font
        {
            get { return _line1Font; }
            set
            {
                _line1Font = value ?? new Font("Microsoft YaHei", 12f, FontStyle.Bold);
                base.Font = value;
                AdjustControlSize();
                this.Invalidate();
            }
        }
        #endregion

        #region 设计时支持
        /// <summary>
        /// 设计器支持 - 返回默认值
        /// </summary>
        private void ResetLine1Font() { Line1Font = new Font("Microsoft YaHei", 12f, FontStyle.Bold); }
        private bool ShouldSerializeLine1Font() { return !Line1Font.Equals(new Font("Microsoft YaHei", 12f, FontStyle.Bold)); }

        private void ResetLine2Font() { Line2Font = new Font("Microsoft YaHei", 9f, FontStyle.Regular); }
        private bool ShouldSerializeLine2Font() { return !Line2Font.Equals(new Font("Microsoft YaHei", 9f, FontStyle.Regular)); }

        private void ResetNormalTextColor() { NormalTextColor = Color.Black; }
        private bool ShouldSerializeNormalTextColor() { return NormalTextColor != Color.Black; }

        private void ResetDimmedTextColor() { DimmedTextColor = Color.Gray; }
        private bool ShouldSerializeDimmedTextColor() { return DimmedTextColor != Color.Gray; }
        #endregion

        #region 资源清理
        /// <summary>
        /// 清理使用的资源
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _line1Font?.Dispose();
                _line2Font?.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion
    }


    public class DropDownTextBox : UserControl
    {
        // 控件成员
        private TextBox textBox;
        private Button dropDownButton;
        private DropDownComboBox comboBox;
        private Panel containerPanel;

        // 事件
        public event EventHandler SelectedValueChanged;
        public event EventHandler DropDownOpened;
        public event EventHandler DropDownClosed;

        // 新增的公共事件
        public event EventHandler ButtonClick;
        public event EventHandler TextBoxTextChanged;

        // 箭头绘制属性
        private Color _arrowColor = Color.Black;
        private int _arrowWidth = 12;  // 箭头宽度
        private int _arrowHeight = 8;  // 箭头高度
        private const int BUTTON_PADDING = 4; // 按钮内边距，确保箭头完整显示

        [Category("外观")]
        [Description("下拉箭头的颜色")]
        [DefaultValue(typeof(Color), "Black")]
        public Color ArrowColor
        {
            get { return _arrowColor; }
            set
            {
                _arrowColor = value;
                SafeInvalidate();
            }
        }

        [Category("外观")]
        [Description("下拉箭头的宽度")]
        [DefaultValue(12)]
        public int ArrowWidth
        {
            get { return _arrowWidth; }
            set
            {
                if (value < 4) value = 4;
                if (value > 50) value = 50;
                _arrowWidth = value;
                UpdateButtonSize(); // 更新按钮尺寸
                SafeInvalidate();
            }
        }

        [Category("外观")]
        [Description("下拉箭头的高度")]
        [DefaultValue(8)]
        public int ArrowHeight
        {
            get { return _arrowHeight; }
            set
            {
                if (value < 3) value = 3;
                if (value > 50) value = 50;
                _arrowHeight = value;
                UpdateButtonSize(); // 更新按钮尺寸
                SafeInvalidate();
            }
        }

        // 属性
        public override string Text
        {
            get => textBox.Text;
            set
            {
                textBox.Text = value;
                UpdateTextBoxScrollBars();
            }
        }

        public object SelectedItem
        {
            get => comboBox.SelectedItem;
            set => comboBox.SelectedItem = value;
        }

        public int SelectedIndex
        {
            get => comboBox.SelectedIndex;
            set => comboBox.SelectedIndex = value;
        }

        public ComboBox.ObjectCollection Items => comboBox.Items;

        public bool IsDropDownOpen => comboBox.Visible;

        // 滚动条相关属性
        public ScrollBars TextBoxScrollBars
        {
            get => textBox.ScrollBars;
            set => textBox.ScrollBars = value;
        }

        public int MaxDropDownItems { get; set; } = 30;
        private int originalComboBoxHeight;
        private bool isFirstClick = true;

        public DropDownTextBox()
        {
            InitializeComponents();
            SetupLayout();
            // 延迟订阅事件，确保控件已经完全初始化
            this.HandleCreated += (s, e) =>
            {
                SubscribeEvents();
            };
        }

        private void InitializeComponents()
        {
            // 创建容器面板 - 去除边框
            containerPanel = new Panel();
            containerPanel.BorderStyle = BorderStyle.None; // 改为无边框
            containerPanel.BackColor = Color.White;

            // 创建文本框 - 默认不显示滚动条
            textBox = new TextBox();
            textBox.Multiline = true;
            textBox.ScrollBars = ScrollBars.None; // 默认不显示滚动条
            textBox.BorderStyle = BorderStyle.None; // 无边框
            textBox.BackColor = Color.White;

            // 创建下拉按钮 - 优化鼠标悬停效果
            dropDownButton = new Button();
            dropDownButton.BackColor = Color.White;
            dropDownButton.FlatStyle = FlatStyle.Flat;
            dropDownButton.FlatAppearance.BorderSize = 0;
            // 移除鼠标悬停和按下时的背景色变化
            dropDownButton.FlatAppearance.MouseOverBackColor = Color.White;
            dropDownButton.FlatAppearance.MouseDownBackColor = Color.White;
            dropDownButton.Text = "";
            dropDownButton.TextAlign = ContentAlignment.MiddleCenter;
            dropDownButton.Paint += DropDownButton_Paint;

            // 创建自定义ComboBox（去掉原生箭头）
            comboBox = new DropDownComboBox();
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList; // 设置为下拉列表模式
            comboBox.Visible = false;
            comboBox.Font = this.Font;
            comboBox.IntegralHeight = false; // 允许设置任意高度
            comboBox.FlatStyle = FlatStyle.Flat; // 扁平样式
            comboBox.BackColor = Color.White;

            // 将控件添加到容器
            containerPanel.Controls.Add(textBox);
            containerPanel.Controls.Add(dropDownButton);
            containerPanel.Controls.Add(comboBox);

            // 将容器添加到UserControl
            this.Controls.Add(containerPanel);

            // 设置UserControl无边框
            this.BorderStyle = BorderStyle.None;
            this.BackColor = Color.White; // 设置背景色为白色
        }

        private void DropDownButton_Paint(object sender, PaintEventArgs e)
        {
            DrawArrow(e.Graphics);
        }

        private void DrawArrow(Graphics g)
        {
            // 计算按钮可用区域（考虑内边距）
            int availableWidth = dropDownButton.Width - (BUTTON_PADDING * 2);
            int availableHeight = dropDownButton.Height - (BUTTON_PADDING * 2);

            // 如果可用空间不足，自动调整箭头大小
            int actualArrowWidth = _arrowWidth;
            int actualArrowHeight = _arrowHeight;

            if (actualArrowWidth > availableWidth)
            {
                actualArrowWidth = availableWidth;
                // 保持宽高比例
                actualArrowHeight = (int)((float)_arrowHeight / _arrowWidth * actualArrowWidth);
            }

            if (actualArrowHeight > availableHeight)
            {
                actualArrowHeight = availableHeight;
                // 保持宽高比例
                actualArrowWidth = (int)((float)_arrowWidth / _arrowHeight * actualArrowHeight);
            }

            // 确保调整后的尺寸不会太小
            actualArrowWidth = Math.Max(actualArrowWidth, 4);
            actualArrowHeight = Math.Max(actualArrowHeight, 3);

            //// 计算箭头位置，确保在右上角区域并完整显示
            //int arrowX = dropDownButton.Width - BUTTON_PADDING - actualArrowWidth;
            //int arrowY = (dropDownButton.Height - actualArrowHeight) / 2; // 垂直居中


            // 修改这里：确保箭头始终在右上角
            int arrowX = dropDownButton.Width - BUTTON_PADDING - actualArrowWidth;
            int arrowY = BUTTON_PADDING; // 固定在上边距位置，确保在右上角
            // 确保箭头不会超出边界
            arrowX = Math.Max(arrowX, BUTTON_PADDING);
            arrowY = Math.Max(arrowY, BUTTON_PADDING);

            // 绘制三角形箭头
            Point[] points = new Point[]
            {
        new Point(arrowX, arrowY), // 左上角
        new Point(arrowX + actualArrowWidth, arrowY), // 右上角
        new Point(arrowX + (actualArrowWidth / 2), arrowY + actualArrowHeight) // 底部中点
            };

            using (SolidBrush arrowBrush = new SolidBrush(_arrowColor))
            {
                g.FillPolygon(arrowBrush, points);
            }
        }

        /// <summary>
        /// 公共方法：在指定的Graphics对象上绘制三角形箭头
        /// </summary>
        /// <param name="g">Graphics对象</param>
        /// <param name="bounds">绘制区域的边界</param>
        /// <param name="arrowColor">箭头颜色</param>
        /// <param name="arrowWidth">箭头宽度</param>
        /// <param name="arrowHeight">箭头高度</param>
        /// <param name="direction">箭头方向：0-向下，1-向上，2-向左，3-向右</param>
        public static void DrawTriangleArrow(Graphics g, Rectangle bounds, Color arrowColor,
            int arrowWidth = 12, int arrowHeight = 8, int direction = 0)
        {
            if (g == null) return;

            // 计算实际可用的箭头尺寸
            int actualArrowWidth = Math.Min(arrowWidth, bounds.Width);
            int actualArrowHeight = Math.Min(arrowHeight, bounds.Height);

            // 确保尺寸有效
            actualArrowWidth = Math.Max(actualArrowWidth, 4);
            actualArrowHeight = Math.Max(actualArrowHeight, 3);

            // 计算箭头在边界内的居中位置
            int arrowX = bounds.X + (bounds.Width - actualArrowWidth) / 2;
            int arrowY = bounds.Y + (bounds.Height - actualArrowHeight) / 2;

            Point[] points;

            switch (direction)
            {
                case 1: // 向上
                    points = new Point[]
                    {
                    new Point(arrowX, arrowY + actualArrowHeight), // 左下角
                    new Point(arrowX + actualArrowWidth, arrowY + actualArrowHeight), // 右下角
                    new Point(arrowX + (actualArrowWidth / 2), arrowY) // 顶部中点
                    };
                    break;
                case 2: // 向左
                    points = new Point[]
                    {
                    new Point(arrowX + actualArrowWidth, arrowY), // 右上角
                    new Point(arrowX + actualArrowWidth, arrowY + actualArrowHeight), // 右下角
                    new Point(arrowX, arrowY + (actualArrowHeight / 2)) // 左侧中点
                    };
                    break;
                case 3: // 向右
                    points = new Point[]
                    {
                    new Point(arrowX, arrowY), // 左上角
                    new Point(arrowX, arrowY + actualArrowHeight), // 左下角
                    new Point(arrowX + actualArrowWidth, arrowY + (actualArrowHeight / 2)) // 右侧中点
                    };
                    break;
                default: // 向下（默认）
                    points = new Point[]
                    {
                    new Point(arrowX, arrowY), // 左上角
                    new Point(arrowX + actualArrowWidth, arrowY), // 右上角
                    new Point(arrowX + (actualArrowWidth / 2), arrowY + actualArrowHeight) // 底部中点
                    };
                    break;
            }

            using (SolidBrush arrowBrush = new SolidBrush(arrowColor))
            {
                g.FillPolygon(arrowBrush, points);
            }
        }

        /// <summary>
        /// 公共方法：使用当前实例的箭头设置绘制三角形箭头
        /// </summary>
        /// <param name="g">Graphics对象</param>
        /// <param name="bounds">绘制区域的边界</param>
        /// <param name="direction">箭头方向：0-向下，1-向上，2-向左，3-向右</param>
        public void DrawTriangleArrow(Graphics g, Rectangle bounds, int direction = 0)
        {
            DrawTriangleArrow(g, bounds, _arrowColor, _arrowWidth, _arrowHeight, direction);
        }

        /// <summary>
        /// 公共方法：在指定位置绘制三角形箭头
        /// </summary>
        /// <param name="g">Graphics对象</param>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="arrowColor">箭头颜色</param>
        /// <param name="arrowWidth">箭头宽度</param>
        /// <param name="arrowHeight">箭头高度</param>
        /// <param name="direction">箭头方向：0-向下，1-向上，2-向左，3-向右</param>
        public static void DrawTriangleArrow(Graphics g, int x, int y, int width, int height,
            Color arrowColor, int arrowWidth = 12, int arrowHeight = 8, int direction = 0)
        {
            Rectangle bounds = new Rectangle(x, y, width, height);
            DrawTriangleArrow(g, bounds, arrowColor, arrowWidth, arrowHeight, direction);
        }

        private void SafeInvalidate2()
        {
            if (dropDownButton != null && !dropDownButton.IsDisposed)
            {
                dropDownButton.Invalidate();
            }
        }

        // 更新按钮尺寸以确保箭头完整显示
        private void UpdateButtonSize()
        {
            if (dropDownButton != null && containerPanel != null)
            {
                // 计算按钮所需的最小尺寸
                int minButtonWidth = _arrowWidth + (BUTTON_PADDING * 2);
                int minButtonHeight = _arrowHeight + (BUTTON_PADDING * 2);

                // 确保按钮足够大以完整显示箭头
                int buttonWidth = Math.Max(20, minButtonWidth);
                int buttonHeight = containerPanel.Height;

                // 更新按钮位置和大小
                dropDownButton.Location = new Point(containerPanel.Width - buttonWidth, 0);
                dropDownButton.Size = new Size(buttonWidth, buttonHeight);

                // 更新文本框大小以适应新的按钮大小
                UpdateTextBoxSize();
            }
        }

        // 更新文本框大小
        private void UpdateTextBoxSize()
        {
            if (textBox != null && dropDownButton != null && containerPanel != null)
            {
                textBox.Size = new Size(containerPanel.Width - dropDownButton.Width, containerPanel.Height);
            }
        }

        private void SetupLayout()
        {
            // 设置UserControl大小
            this.Size = new Size(200, 80);

            // 设置容器面板填充整个UserControl
            containerPanel.Dock = DockStyle.Fill;

            // 设置文本框位置 - 去除边距
            textBox.Location = new Point(0, 0);
            textBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // 保存原始ComboBox高度
            originalComboBoxHeight = this.Height;

            // 设置ComboBox位置和大小（与文本框相同区域）
            comboBox.Location = textBox.Location;
            comboBox.Width = textBox.Width;
            comboBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // 初始化按钮尺寸
            UpdateButtonSize();
        }
        // 在DropDownTextBox中添加
        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            UpdateButtonSize();
            SafeInvalidate();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            UpdateButtonSize();
            SafeInvalidate();
        }

        // 增强SafeInvalidate方法
        private void SafeInvalidate()
        {
            if (dropDownButton != null && !dropDownButton.IsDisposed)
            {
                dropDownButton.Invalidate();
                dropDownButton.Update();
            }
            if (containerPanel != null && !containerPanel.IsDisposed)
            {
                containerPanel.Invalidate();
                containerPanel.Update();
            }
            this.Invalidate();
            this.Update();
        }
        private void SubscribeEvents()
        {
            dropDownButton.MouseDown += DropDownButton_MouseDown;
            dropDownButton.Click += DropDownButton_Click; // 新增点击事件
            comboBox.SelectedIndexChanged += ComboBox_SelectedIndexChanged;
            comboBox.DropDownClosed += ComboBox_DropDownClosed;
            comboBox.KeyDown += ComboBox_KeyDown;
            textBox.TextChanged += TextBox_TextChanged;
            textBox.VisibleChanged += TextBox_VisibleChanged; // 新增事件订阅
            this.Resize += DropDownTextBox_Resize;
            this.FontChanged += DropDownTextBox_FontChanged;
            this.Layout += DropDownTextBox_Layout;
        }

        // 新增：按钮点击事件处理
        private void DropDownButton_Click(object sender, EventArgs e)
        {
            // 触发公共按钮点击事件
            ButtonClick?.Invoke(this, e);
        }

        private void DropDownButton_MouseDown(object sender, MouseEventArgs e)
        {
            // 如果是第一次点击，直接打开下拉框，不设置选中项
            if (isFirstClick)
            {
                // 在打开下拉框前先确保滚动条状态正确
                SafeBeginInvoke(() =>
                {
                    UpdateTextBoxScrollBars();
                    OpenDropDown(true); // 传递true表示是第一次点击
                });
                isFirstClick = false;
            }
            else
            {
                ToggleDropDown();
            }
        }

        public void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 只有在ComboBox可见时才处理选中项变化
            if (comboBox.SelectedItem != null /*&& comboBox.Visible*/)
            {
                textBox.Text = comboBox.SelectedItem.ToString();
                CloseDropDown();
                SelectedValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void ComboBox_DropDownClosed(object sender, EventArgs e)
        {
            // 当下拉列表关闭时，如果ComboBox仍然可见，则关闭它
            if (comboBox.Visible)
            {
                CloseDropDown();
            }
        }

        private void ComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    if (comboBox.SelectedItem != null)
                    {
                        textBox.Text = comboBox.SelectedItem.ToString();
                        CloseDropDown();
                        SelectedValueChanged?.Invoke(this, EventArgs.Empty);
                    }
                    e.Handled = true;
                    break;

                case Keys.Escape:
                    CloseDropDown();
                    e.Handled = true;
                    break;
            }
        }

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            UpdateTextBoxScrollBars();

            // 新增：触发公共文本框文本变化事件
            TextBoxTextChanged?.Invoke(this, e);
        }

        private void DropDownTextBox_Resize(object sender, EventArgs e)
        {
            // 延迟更新滚动条，确保布局已完成
            SafeBeginInvoke(() =>
            {
                UpdateTextBoxScrollBars();
            });
            UpdateButtonSize(); // 调整按钮大小
                                // 更新ComboBox宽度以匹配TextBox
            if (comboBox != null)
            {
                comboBox.Width = textBox.Width;
            }
        }
        // 添加文本框可见性变化的事件处理
        private void TextBox_VisibleChanged(object sender, EventArgs e)
        {
            if (textBox.Visible)
            {
                // 当文本框变为可见时，延迟更新滚动条
                SafeBeginInvoke(() =>
                {
                    UpdateTextBoxScrollBars();
                });
            }
        }
        // 安全地调用 BeginInvoke，检查控件句柄是否已创建
        private void SafeBeginInvoke(Action action)
        {
            if (action == null) return;

            // 检查控件是否已经创建了窗口句柄
            if (this.IsHandleCreated && !this.IsDisposed)
            {
                this.BeginInvoke(action);
            }
            else
            {
                // 如果句柄尚未创建，直接执行操作
                action();
            }
        }
        private void DropDownTextBox_FontChanged(object sender, EventArgs e)
        {
            textBox.Font = this.Font;
            comboBox.Font = this.Font;
            UpdateTextBoxScrollBars();
        }

        private void DropDownTextBox_Layout(object sender, LayoutEventArgs e)
        {
            // 确保ComboBox宽度正确
            comboBox.Width = textBox.Width;
            // 更新按钮和文本框布局
            UpdateButtonSize();
        }

        // 更新文本框滚动条显示状态
        private void UpdateTextBoxScrollBars()
        {
            // 添加更多安全检查
            if (textBox == null || textBox.IsDisposed || !textBox.IsHandleCreated) return;

            // 如果文本框不可见，不需要更新滚动条
            if (!textBox.Visible) return;

            if (string.IsNullOrEmpty(textBox.Text))
            {
                // 安全检查后再设置属性
                if (!textBox.IsDisposed && textBox.IsHandleCreated)
                {
                    textBox.ScrollBars = ScrollBars.None;
                }
                return;
            }

            try
            {
                // 再次检查控件状态
                if (textBox.IsDisposed || !textBox.IsHandleCreated) return;

                using (Graphics g = textBox.CreateGraphics())
                {
                    // 获取文本框的客户端区域（去除内边距）
                    Rectangle clientRect = textBox.ClientRectangle;

                    // 考虑内边距
                    int availableWidth = clientRect.Width - 4; // 减去一些边距

                    // 计算文本所需的高度
                    SizeF textSize = g.MeasureString(textBox.Text, textBox.Font, availableWidth);

                    // 获取单行文本的高度
                    float lineHeight = textBox.Font.Height;

                    // 计算可见行数（考虑内边距）
                    int visibleLines = (int)((clientRect.Height - 2) / lineHeight);

                    // 计算总行数（近似值）
                    int totalLines = (int)Math.Ceiling(textSize.Height / lineHeight);

                    // 只有当文本确实超出可见区域时才显示滚动条
                    ScrollBars newScrollBars = (totalLines > visibleLines && textSize.Height > clientRect.Height)
                        ? ScrollBars.Vertical
                        : ScrollBars.None;

                    // 只有在值发生变化时才设置，避免不必要的重绘
                    if (!textBox.IsDisposed && textBox.IsHandleCreated && textBox.ScrollBars != newScrollBars)
                    {
                        textBox.ScrollBars = newScrollBars;
                    }
                }
            }
            catch (Exception ex)
            {
                // 如果计算过程中出现异常，默认不显示滚动条
                if (!textBox.IsDisposed && textBox.IsHandleCreated)
                {
                    textBox.ScrollBars = ScrollBars.None;
                }
            }
        }

        // 计算合适的下拉列表高度
        private int CalculateDropDownHeight()
        {
            if (comboBox.Items.Count == 0)
                return Math.Max(120, originalComboBoxHeight);

            // 获取单行高度
            int itemHeight = comboBox.ItemHeight;
            if (itemHeight <= 0)
                itemHeight = 13; // 默认高度

            // 计算显示所有项所需的高度
            int itemsToShow = Math.Min(comboBox.Items.Count, MaxDropDownItems);
            int totalHeight = itemHeight * itemsToShow;

            // 加上边框高度
            totalHeight += SystemInformation.BorderSize.Height * 2;

            // 确保高度不会太小
            totalHeight = Math.Max(totalHeight, originalComboBoxHeight);

            // 限制最大高度（避免超出屏幕）
            int maxHeight = GetMaxAvailableHeight();
            return Math.Min(totalHeight, maxHeight);
        }

        // 获取可用的最大高度（避免超出屏幕）
        private int GetMaxAvailableHeight()
        {
            // 获取屏幕工作区域
            Rectangle workingArea = Screen.GetWorkingArea(this);

            // 计算控件在屏幕上的位置
            Point screenLocation = this.PointToScreen(Point.Empty);

            // 计算下方可用空间
            int availableHeightBelow = workingArea.Bottom - (screenLocation.Y + this.Height);

            // 计算上方可用空间（如果向下空间不够，可以向上展开）
            int availableHeightAbove = screenLocation.Y - workingArea.Top;

            // 返回最大可用高度，但至少为120像素
            int maxAvailable = Math.Max(availableHeightBelow, availableHeightAbove);
            return Math.Max(maxAvailable, 120);
        }

        // 公共方法
        // 在DropDownTextBox类中添加
        public void RefreshButton()
        {
            if (dropDownButton != null && !dropDownButton.IsDisposed)
            {
                dropDownButton.Invalidate();
                dropDownButton.Update();
            }
            this.Invalidate();
            this.Update();
        }

        public void ToggleDropDown()
        {
            if (comboBox.Visible)
            {
                CloseDropDown();
            }
            else
            {
                OpenDropDown(false); // 传递false表示不是第一次点击
            }
        }

        public void OpenDropDown(bool isFirstClick)
        {
            if (!comboBox.Visible && comboBox.Parent != null)
            {
                // 计算并设置合适的ComboBox高度
                int dropDownHeight = CalculateDropDownHeight();
                comboBox.Height = dropDownHeight;

                // 设置ComboBox下拉列表的最大显示项数
                comboBox.MaxDropDownItems = MaxDropDownItems;

                // 确保ComboBox宽度正确
                comboBox.Width = textBox.Width;

                // 检查是否需要向上展开（如果下方空间不够）
                Point screenLocation = this.PointToScreen(Point.Empty);
                Rectangle workingArea = Screen.GetWorkingArea(this);
                int availableHeightBelow = workingArea.Bottom - (screenLocation.Y + this.Height);

                if (dropDownHeight > availableHeightBelow && availableHeightBelow < originalComboBoxHeight)
                {
                    // 向上展开
                    comboBox.Top = textBox.Top - dropDownHeight + this.Height;
                }
                else
                {
                    // 向下展开（默认）
                    comboBox.Top = textBox.Top;
                }

                comboBox.Visible = true;
                textBox.Visible = false;
                comboBox.BringToFront();
                comboBox.Focus();

                // 只有在不是第一次点击时才自动选择第一项
                if (!isFirstClick && comboBox.Items.Count > 0 && comboBox.SelectedIndex == -1)
                {
                    comboBox.SelectedIndex = 0;
                }

                // 打开下拉列表
                comboBox.DroppedDown = true;

                DropDownOpened?.Invoke(this, EventArgs.Empty);
            }
        }

        public void CloseDropDown()
        {
            if (comboBox.Visible)
            {
                // 关闭下拉列表
                comboBox.DroppedDown = false;

                // 重置ComboBox位置
                comboBox.Top = textBox.Top;

                comboBox.Visible = false;
                textBox.Visible = true;
                textBox.Focus();
                // 延迟更新滚动条状态，确保布局已完成
                SafeBeginInvoke(() =>
                {
                    UpdateTextBoxScrollBars();
                });
                DropDownClosed?.Invoke(this, EventArgs.Empty);
            }
        }

        public void AddItem(string item)
        {
            comboBox.Items.Add(item);
        }

        public void AddItems(string[] items)
        {
            comboBox.Items.AddRange(items);
        }

        public void ClearItems()
        {
            comboBox.Items.Clear();
        }

        public void RemoveItem(string item)
        {
            comboBox.Items.Remove(item);
        }

        public void RemoveItemAt(int index)
        {
            if (index >= 0 && index < comboBox.Items.Count)
            {
                comboBox.Items.RemoveAt(index);
            }
        }

        // 设置下拉列表的最大显示项数
        public void SetMaxDropDownItems(int maxItems)
        {
            MaxDropDownItems = Math.Max(1, maxItems);
        }

        // 重写OnLostFocus以在失去焦点时关闭下拉框
        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);

            // 检查失去焦点的控件是否是当前控件或其子控件
            Control focusedControl = FindForm()?.ActiveControl;
            if (focusedControl != null &&
                !this.Contains(focusedControl) &&
                !comboBox.Contains(focusedControl))
            {
                CloseDropDown();
                // 重置第一次点击状态
                isFirstClick = true;
            }
        }

        // 辅助方法：检查控件是否包含另一个控件
        private bool Contains(Control control)
        {
            Control parent = control;
            while (parent != null)
            {
                if (parent == this)
                    return true;
                parent = parent.Parent;
            }
            return false;
        }

        // 自定义 ComboBox 类，去掉原生下拉箭头和边框
        private class DropDownComboBox : ComboBox
        {
            private const int WM_PAINT = 0x000F;
            private const int WM_NCPAINT = 0x0085;

            [DllImport("user32.dll")]
            private static extern IntPtr GetWindowDC(IntPtr hWnd);

            [DllImport("user32.dll")]
            private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

            public DropDownComboBox()
            {
                // 设置扁平样式以去除边框
                this.FlatStyle = FlatStyle.Flat;
                this.BackColor = Color.White;
            }

            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);

                if (m.Msg == WM_PAINT || m.Msg == WM_NCPAINT)
                {
                    // 移除原生下拉箭头和边框
                    RemoveNativeArrowAndBorder();
                }
            }

            private void RemoveNativeArrowAndBorder()
            {
                // 获取设备上下文
                IntPtr hDC = GetWindowDC(this.Handle);
                if (hDC != IntPtr.Zero)
                {
                    using (Graphics g = Graphics.FromHdc(hDC))
                    {
                        // 用背景色覆盖整个控件区域，去除边框
                        Rectangle clientRect = new Rectangle(0, 0, this.Width, this.Height);
                        using (SolidBrush brush = new SolidBrush(this.BackColor))
                        {
                            g.FillRectangle(brush, clientRect);
                        }

                        // 用背景色覆盖箭头区域
                        Rectangle arrowRect = new Rectangle(
                            this.Width - SystemInformation.VerticalScrollBarWidth - 2,
                            0,
                            SystemInformation.VerticalScrollBarWidth + 2,
                            this.Height);

                        using (SolidBrush brush = new SolidBrush(this.BackColor))
                        {
                            g.FillRectangle(brush, arrowRect);
                        }
                    }
                    ReleaseDC(this.Handle, hDC);
                }
            }

            protected override void OnDropDown(EventArgs e)
            {
                base.OnDropDown(e);
                // 确保在下拉时也移除箭头和边框
                this.Invalidate();
            }

            protected override void OnDropDownClosed(EventArgs e)
            {
                base.OnDropDownClosed(e);
                // 确保在下拉关闭时也移除箭头和边框
                this.Invalidate();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                // 确保在绘制时去除边框
                ControlPaint.DrawBorder(e.Graphics, this.ClientRectangle, this.BackColor, ButtonBorderStyle.None);
            }
        }
    }



    /// <summary>
    /// 自定义直线控件
    /// </summary>
    [DefaultProperty("LineColor")]
    [ToolboxBitmap(typeof(LineControl), "LineControl.bmp")]
    public class LineControl : Control
    {
        #region 私有字段

        private Color _lineColor = Color.Black;
        private int _lineThickness = 1;
        private LineDirection _direction = LineDirection.Horizontal;
        private int _lineLength = 100;
        private DashStyle _lineStyle = DashStyle.Solid;
        private bool _antiAlias = true;
        private Color _backgroundColor = SystemColors.Control;
        private Padding _margin = new Padding(3); // 默认边距

        #endregion

        #region 枚举

        /// <summary>
        /// 直线方向
        /// </summary>
        public enum LineDirection
        {
            /// <summary>
            /// 水平方向
            /// </summary>
            Horizontal,

            /// <summary>
            /// 垂直方向
            /// </summary>
            Vertical,

            /// <summary>
            /// 左上到右下对角线
            /// </summary>
            DiagonalDown,

            /// <summary>
            /// 左下到右上对角线
            /// </summary>
            DiagonalUp
        }

        #endregion

        #region 构造函数

        /// <summary>
        /// 初始化直线控件
        /// </summary>
        public LineControl()
        {
            // 设置默认属性
            this.Size = new Size(100, 2);
            this.BackColor = SystemColors.Control; // 使用系统控件颜色而不是透明

            // 启用双缓冲
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint |
                         ControlStyles.ResizeRedraw |
                         ControlStyles.OptimizedDoubleBuffer, true);

            // 移除透明背景支持，使用标准背景色
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, false);

            // 设置默认边距
            this.Margin = new Padding(3);
        }

        #endregion

        #region 公共属性

        /// <summary>
        /// 获取或设置直线颜色
        /// </summary>
        [Category("外观")]
        [Description("直线的颜色")]
        [DefaultValue(typeof(Color), "Black")]
        public Color LineColor
        {
            get { return _lineColor; }
            set
            {
                _lineColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置直线粗细
        /// </summary>
        [Category("外观")]
        [Description("直线的粗细（像素）")]
        [DefaultValue(1)]
        public int LineThickness
        {
            get { return _lineThickness; }
            set
            {
                _lineThickness = Math.Max(1, value);
                UpdateSize();
                Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置直线方向
        /// </summary>
        [Category("布局")]
        [Description("直线的方向")]
        [DefaultValue(LineDirection.Horizontal)]
        public LineDirection Direction
        {
            get { return _direction; }
            set
            {
                _direction = value;
                UpdateSize();
                Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置直线长度
        /// </summary>
        [Category("布局")]
        [Description("直线的长度（像素）")]
        [DefaultValue(100)]
        public int LineLength
        {
            get { return _lineLength; }
            set
            {
                _lineLength = Math.Max(1, value);
                UpdateSize();
                Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置直线样式
        /// </summary>
        [Category("外观")]
        [Description("直线的样式")]
        [DefaultValue(DashStyle.Solid)]
        public DashStyle LineStyle
        {
            get { return _lineStyle; }
            set
            {
                _lineStyle = value;
                Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置是否启用抗锯齿
        /// </summary>
        [Category("外观")]
        [Description("是否启用抗锯齿渲染")]
        [DefaultValue(true)]
        public bool AntiAlias
        {
            get { return _antiAlias; }
            set
            {
                _antiAlias = value;
                Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置控件背景颜色
        /// </summary>
        [Category("外观")]
        [Description("控件的背景颜色")]
        [DefaultValue(typeof(Color), "Control")]
        public new Color BackColor
        {
            get { return base.BackColor; }
            set { base.BackColor = value; }
        }

        /// <summary>
        /// 获取或设置控件与其他控件之间的间距
        /// </summary>
        [Category("布局")]
        [Description("指定此控件与另一控件的边距之间的距离")]
        [DefaultValue(typeof(Padding), "3,3,3,3")]
        public new Padding Margin
        {
            get { return base.Margin; }
            set
            {
                base.Margin = value;
                // 当边距改变时，可能需要调整父容器的布局
                if (this.Parent != null)
                    this.Parent.PerformLayout();
            }
        }

        /// <summary>
        /// 获取或设置控件的内边距
        /// </summary>
        [Category("布局")]
        [Description("控件的内边距")]
        [DefaultValue(typeof(Padding), "0,0,0,0")]
        public new Padding Padding
        {
            get { return base.Padding; }
            set
            {
                base.Padding = value;
                Invalidate(); // 内边距改变需要重绘
            }
        }

        /// <summary>
        /// 隐藏基类的Text属性
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
        }

        #endregion

        #region 重写方法

        /// <summary>
        /// 绘制控件
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;

            // 清除背景
            using (Brush bgBrush = new SolidBrush(this.BackColor))
            {
                g.FillRectangle(bgBrush, this.ClientRectangle);
            }

            // 设置抗锯齿
            if (_antiAlias)
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
            }

            // 创建画笔
            using (Pen pen = new Pen(_lineColor, _lineThickness))
            {
                pen.DashStyle = _lineStyle;

                // 根据方向绘制直线（考虑内边距）
                Point startPoint, endPoint;
                CalculateLinePoints(out startPoint, out endPoint);

                // 绘制直线
                g.DrawLine(pen, startPoint, endPoint);
            }
        }

        /// <summary>
        /// 控件大小改变时
        /// </summary>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            // 根据当前大小更新长度
            if (_direction == LineDirection.Horizontal)
            {
                _lineLength = this.Width - this.Padding.Horizontal;
            }
            else if (_direction == LineDirection.Vertical)
            {
                _lineLength = this.Height - this.Padding.Vertical;
            }
            else
            {
                // 对角线方向，长度取对角线长度（考虑内边距）
                int effectiveWidth = this.Width - this.Padding.Horizontal;
                int effectiveHeight = this.Height - this.Padding.Vertical;
                _lineLength = (int)Math.Sqrt(effectiveWidth * effectiveWidth + effectiveHeight * effectiveHeight);
            }
        }

        /// <summary>
        /// 当边距改变时
        /// </summary>
        protected override void OnMarginChanged(EventArgs e)
        {
            base.OnMarginChanged(e);
            // 边距改变可能需要父容器重新布局
            if (this.Parent != null)
                this.Parent.Invalidate();
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 计算直线的起点和终点（考虑内边距）
        /// </summary>
        private void CalculateLinePoints(out Point startPoint, out Point endPoint)
        {
            // 计算有效绘制区域（考虑内边距）
            int effectiveWidth = this.Width - this.Padding.Horizontal;
            int effectiveHeight = this.Height - this.Padding.Vertical;

            if (effectiveWidth <= 0) effectiveWidth = 1;
            if (effectiveHeight <= 0) effectiveHeight = 1;

            int centerX = this.Padding.Left + effectiveWidth / 2;
            int centerY = this.Padding.Top + effectiveHeight / 2;

            switch (_direction)
            {
                case LineDirection.Horizontal:
                    startPoint = new Point(this.Padding.Left, centerY);
                    endPoint = new Point(this.Padding.Left + effectiveWidth, centerY);
                    break;

                case LineDirection.Vertical:
                    startPoint = new Point(centerX, this.Padding.Top);
                    endPoint = new Point(centerX, this.Padding.Top + effectiveHeight);
                    break;

                case LineDirection.DiagonalDown:
                    startPoint = new Point(this.Padding.Left, this.Padding.Top);
                    endPoint = new Point(this.Padding.Left + effectiveWidth, this.Padding.Top + effectiveHeight);
                    break;

                case LineDirection.DiagonalUp:
                    startPoint = new Point(this.Padding.Left, this.Padding.Top + effectiveHeight);
                    endPoint = new Point(this.Padding.Left + effectiveWidth, this.Padding.Top);
                    break;

                default:
                    startPoint = new Point(this.Padding.Left, centerY);
                    endPoint = new Point(this.Padding.Left + effectiveWidth, centerY);
                    break;
            }
        }

        /// <summary>
        /// 根据属性更新控件大小
        /// </summary>
        private void UpdateSize()
        {
            // 计算总大小（包含内边距）
            int totalWidth, totalHeight;

            switch (_direction)
            {
                case LineDirection.Horizontal:
                    totalWidth = _lineLength + this.Padding.Horizontal;
                    totalHeight = Math.Max(2, _lineThickness + 2) + this.Padding.Vertical;
                    this.Size = new Size(totalWidth, totalHeight);
                    break;

                case LineDirection.Vertical:
                    totalWidth = Math.Max(2, _lineThickness + 2) + this.Padding.Horizontal;
                    totalHeight = _lineLength + this.Padding.Vertical;
                    this.Size = new Size(totalWidth, totalHeight);
                    break;

                case LineDirection.DiagonalDown:
                case LineDirection.DiagonalUp:
                    // 对角线方向，使用正方形区域
                    int size = (int)(_lineLength / Math.Sqrt(2));
                    totalWidth = size + this.Padding.Horizontal;
                    totalHeight = size + this.Padding.Vertical;
                    this.Size = new Size(totalWidth, totalHeight);
                    break;
            }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 设置直线位置和长度
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <param name="length">长度</param>
        public void SetLinePosition(int x, int y, int length)
        {
            this.Location = new Point(x, y);
            this.LineLength = length;
        }

        /// <summary>
        /// 设置直线样式
        /// </summary>
        /// <param name="color">颜色</param>
        /// <param name="thickness">粗细</param>
        /// <param name="style">样式</param>
        public void SetLineStyle(Color color, int thickness, DashStyle style)
        {
            this.LineColor = color;
            this.LineThickness = thickness;
            this.LineStyle = style;
        }

        /// <summary>
        /// 设置虚线样式
        /// </summary>
        /// <param name="dashPattern">虚线模式数组</param>
        public void SetDashPattern(float[] dashPattern)
        {
            using (Pen pen = new Pen(_lineColor, _lineThickness))
            {
                if (dashPattern != null && dashPattern.Length > 0)
                {
                    pen.DashPattern = dashPattern;
                    _lineStyle = DashStyle.Custom;
                }
                else
                {
                    _lineStyle = DashStyle.Solid;
                }
            }
            Invalidate();
        }

        /// <summary>
        /// 获取推荐的大小（包含边距）
        /// </summary>
        public Size GetPreferredSize()
        {
            Size size = this.Size;
            return new Size(size.Width + this.Margin.Horizontal, size.Height + this.Margin.Vertical);
        }

        #endregion

        #region 设计时支持

        /// <summary>
        /// 设计时属性默认值
        /// </summary>
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            if (DesignMode)
            {
                UpdateSize();
            }
        }

        #endregion
    }

    /// <summary>
    /// Hive状态显示控件
    /// </summary>
    [DefaultProperty("HiveStatus")]
    [ToolboxBitmap(typeof(HiveStatusControl), "HiveStatusControl.bmp")]
    public class HiveStatusControl : Control
    {
        #region 枚举定义

        /// <summary>
        /// Hive状态枚举
        /// </summary>
        public enum HiveStatus
        {
            /// <summary>
            /// 默认状态
            /// </summary>
            [Description("默认状态")]
            Default = 0,

            /// <summary>
            /// 运行状态 - 机器状态1
            /// </summary>
            [Description("运行状态")]
            RunningState1 = 1,

            /// <summary>
            /// 空闲状态 - 机器状态2
            /// </summary>
            [Description("空闲状态")]
            IdleState2 = 2,

            /// <summary>
            /// 停机状态 - 机器状态5
            /// </summary>
            [Description("停机状态")]
            DownState5 = 5,

            /// <summary>
            /// 通信错误
            /// </summary>
            [Description("通信错误")]
            CommunicationError = 7,

            /// <summary>
            /// 命令错误
            /// </summary>
            [Description("命令错误")]
            CommandError = 8,

            /// <summary>
            /// 站未注册
            /// </summary>
            [Description("站未注册")]
            Offboard = 9
        }

        #endregion

        #region 私有字段

        private HiveStatus _currentStatus = HiveStatus.IdleState2;
        private Color _communicationErrorColor = HiveUIColors.Purple;
        private Color _commandErrorColor = HiveUIColors.Purple;
        private Color _offboardColor = HiveUIColors.Gray;
        private Color _runningColor = HiveUIColors.Green;
        private Color _idleColor = HiveUIColors.Yellow;
        private Color _downColor = HiveUIColors.Red;
        private int _cornerRadius = 5;
        private Font _statusFont;
        private StringAlignment _textAlignment = StringAlignment.Center;
        private Color _textColor = Color.Black; // 固定文本颜色
        private bool _useCustomFont = false;

        // 新增Timer相关字段
        private System.Windows.Forms.Timer _refreshTimer;
        private int _refreshInterval = 1000; // 默认1秒
        private bool _timerEnabled = true;
        private DateTime _lastRefreshTime = DateTime.MinValue;
        private int _refreshCount = 0;
        private bool _autoRefresh = true;

        #endregion

        #region 构造函数

        /// <summary>
        /// 初始化Hive状态控件
        /// </summary>
        public HiveStatusControl()
        {
            // 设置默认属性
            this.Size = new Size(120, 30);
            this.BackColor = Color.White;
            this.ForeColor = Color.Black;

            // 设置状态字体
            _statusFont = new Font("Microsoft YaHei", 9f, FontStyle.Bold);

            // 启用双缓冲
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint |
                         ControlStyles.ResizeRedraw |
                         ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.SupportsTransparentBackColor, true);

            InitializeTimer();
        }

        #endregion

        #region 公共属性

        /// <summary>
        /// 获取或设置当前Hive状态
        /// </summary>
        [Category("状态")]
        [Description("当前的Hive状态")]
        [DefaultValue(HiveStatus.IdleState2)]
        public HiveStatus CurrentStatus
        {
            get { return _currentStatus; }
            set
            {
                if (_currentStatus != value)
                {
                    _currentStatus = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 获取或设置圆角半径
        /// </summary>
        [Category("外观")]
        [Description("控件圆角的半径")]
        [DefaultValue(5)]
        public int CornerRadius
        {
            get { return _cornerRadius; }
            set
            {
                _cornerRadius = Math.Max(0, value);
                Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置文本对齐方式
        /// </summary>
        [Category("外观")]
        [Description("状态文本的对齐方式")]
        [DefaultValue(StringAlignment.Center)]
        public StringAlignment TextAlignment
        {
            get { return _textAlignment; }
            set
            {
                _textAlignment = value;
                Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置文本颜色
        /// </summary>
        [Category("外观")]
        [Description("状态文本的颜色")]
        [DefaultValue(typeof(Color), "Black")]
        public Color TextColor
        {
            get { return _textColor; }
            set
            {
                _textColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置状态文本字体
        /// </summary>
        [Category("外观")]
        [Description("状态文本的字体")]
        public Font StatusFont
        {
            get { return _statusFont; }
            set
            {
                if (_statusFont != null && _statusFont != value)
                {
                    _statusFont.Dispose();
                }
                _statusFont = value;
                _useCustomFont = true;
                Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置是否使用粗体
        /// </summary>
        [Category("外观")]
        [Description("状态文本是否使用粗体")]
        [DefaultValue(true)]
        public bool UseBoldFont
        {
            get { return _statusFont.Bold; }
            set
            {
                if (_statusFont != null)
                {
                    FontStyle style = value ? FontStyle.Bold : FontStyle.Regular;
                    if (_statusFont.Style != style)
                    {
                        Font newFont = new Font(_statusFont.FontFamily, _statusFont.Size, style);
                        if (_statusFont != null)
                            _statusFont.Dispose();
                        _statusFont = newFont;
                        _useCustomFont = true;
                        Invalidate();
                    }
                }
            }
        }

        /// <summary>
        /// 获取或设置字体大小
        /// </summary>
        [Category("外观")]
        [Description("状态文本的字体大小")]
        [DefaultValue(9f)]
        public float FontSize
        {
            get { return _statusFont.Size; }
            set
            {
                if (_statusFont != null && Math.Abs(_statusFont.Size - value) > 0.01f)
                {
                    Font newFont = new Font(_statusFont.FontFamily, value, _statusFont.Style);
                    if (_statusFont != null)
                        _statusFont.Dispose();
                    _statusFont = newFont;
                    _useCustomFont = true;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 获取当前状态的显示文本
        /// </summary>
        [Browsable(false)]
        public string StatusText
        {
            get { return GetStatusText(_currentStatus); }
        }

        /// <summary>
        /// 获取当前状态的显示颜色
        /// </summary>
        [Browsable(false)]
        public Color StatusColor
        {
            get { return GetStatusColor(_currentStatus); }
        }

        /// <summary>
        /// 隐藏基类的Text属性
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
        }

        // 新增Timer相关属性
        /// <summary>
        /// 获取或设置定时器刷新间隔（毫秒）
        /// </summary>
        [Category("定时器")]
        [Description("定时器刷新间隔（毫秒）")]
        [DefaultValue(1000)]
        public int RefreshInterval
        {
            get { return _refreshInterval; }
            set
            {
                if (value < 100) value = 100; // 最小100毫秒
                _refreshInterval = value;
                if (_refreshTimer != null)
                {
                    _refreshTimer.Interval = value;
                }
            }
        }

        /// <summary>
        /// 获取或设置是否启用定时器
        /// </summary>
        [Category("定时器")]
        [Description("是否启用定时刷新")]
        [DefaultValue(true)]
        public bool TimerEnabled
        {
            get { return _timerEnabled; }
            set
            {
                if (_timerEnabled != value)
                {
                    _timerEnabled = value;
                    if (_refreshTimer != null)
                    {
                        _refreshTimer.Enabled = value;
                    }
                    Invalidate(); // 重绘以更新状态指示器
                }
            }
        }

        /// <summary>
        /// 获取或设置是否自动刷新状态
        /// </summary>
        [Category("定时器")]
        [Description("是否在定时器触发时自动刷新状态")]
        [DefaultValue(true)]
        public bool AutoRefresh
        {
            get { return _autoRefresh; }
            set { _autoRefresh = value; }
        }

        /// <summary>
        /// 获取最后一次刷新时间
        /// </summary>
        [Category("定时器")]
        [Description("最后一次刷新时间")]
        [Browsable(false)]
        public DateTime LastRefreshTime
        {
            get { return _lastRefreshTime; }
        }

        /// <summary>
        /// 获取刷新次数统计
        /// </summary>
        [Category("定时器")]
        [Description("刷新次数统计")]
        [Browsable(false)]
        public int RefreshCount
        {
            get { return _refreshCount; }
        }

        #endregion

        #region 事件

        /// <summary>
        /// 定时器刷新时触发的事件
        /// </summary>
        [Category("行为")]
        [Description("定时器刷新时触发")]
        public event EventHandler<TimerRefreshEventArgs> TimerRefresh;

        /// <summary>
        /// 状态改变时触发的事件
        /// </summary>
        [Category("行为")]
        [Description("状态改变时触发")]
        public event EventHandler<StatusChangedEventArgs> StatusChanged;

        #endregion

        #region 重写方法

        /// <summary>
        /// 绘制控件
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Color statusColor; string statusText;
            if (HiveTool.HiveIsConnected)
            {
                // 获取状态颜色和文本
                statusColor = GetStatusColor(_currentStatus);
                statusText = GetStatusText(_currentStatus);
            }
            else
            {
                statusColor = GetStatusColor(HiveStatus.CommunicationError);
                statusText = GetStatusText(HiveStatus.CommunicationError);
            }

            // 绘制圆角背景
            DrawRoundedRectangle(g, statusColor, this.ClientRectangle, _cornerRadius);

            // 绘制状态文本
            DrawStatusText(g, statusText);

            //// 如果定时器启用，在角落显示状态指示器
            //if (_timerEnabled && _refreshTimer != null && _refreshTimer.Enabled)
            //{
            //    using (Brush indicatorBrush = new SolidBrush(Color.LimeGreen))
            //    {
            //        e.Graphics.FillEllipse(indicatorBrush, this.Width - 12, 2, 10, 10);
            //    }
            //}
        }

        /// <summary>
        /// 控件大小改变时
        /// </summary>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }

        /// <summary>
        /// 字体改变时
        /// </summary>
        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            if (!_useCustomFont)
            {
                if (_statusFont != null)
                    _statusFont.Dispose();
                _statusFont = new Font(this.Font, FontStyle.Bold);
                Invalidate();
            }
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _refreshTimer?.Stop();
                _refreshTimer?.Dispose();

                if (_statusFont != null)
                {
                    _statusFont.Dispose();
                    _statusFont = null;
                }
            }
            base.Dispose(disposing);
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 初始化定时器
        /// </summary>
        private void InitializeTimer()
        {
            _refreshTimer = new System.Windows.Forms.Timer();
            _refreshTimer.Interval = _refreshInterval;
            _refreshTimer.Tick += RefreshTimer_Tick;
            _refreshTimer.Enabled = _timerEnabled;
        }

        /// <summary>
        /// 定时器事件处理
        /// </summary>
        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                _refreshCount++;
                _lastRefreshTime = DateTime.Now;

                // 触发刷新事件
                OnTimerRefresh(new TimerRefreshEventArgs(_refreshCount, _lastRefreshTime));

                // 自动刷新状态
                if (_autoRefresh)
                {
                    TimerRefreshStatus();
                }
            }
            catch (Exception ex)
            {
                // 记录错误但不中断定时器
                System.Diagnostics.Debug.WriteLine($"状态控件定时器刷新错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 触发定时器刷新事件
        /// </summary>
        protected virtual void OnTimerRefresh(TimerRefreshEventArgs e)
        {
            TimerRefresh?.Invoke(this, e);
        }

        /// <summary>
        /// 触发状态改变事件
        /// </summary>
        protected virtual void OnStatusChanged(StatusChangedEventArgs e)
        {
            StatusChanged?.Invoke(this, e);
        }

        /// <summary>
        /// 根据状态获取显示文本
        /// </summary>
        private string GetStatusText(HiveStatus status)
        {
            switch (status)
            {
                case HiveStatus.Default:
                    return "Default";
                case HiveStatus.CommunicationError:
                    return "Comm error";
                case HiveStatus.CommandError:
                    return "Comm error";
                case HiveStatus.Offboard:
                    return "Offboard";
                case HiveStatus.RunningState1:
                    return "Running";
                case HiveStatus.IdleState2:
                    return "Idle";
                case HiveStatus.DownState5:
                    return "Down";
                default:
                    return "Unknown";
            }
        }

        /// <summary>
        /// 根据状态获取显示颜色
        /// </summary>
        private Color GetStatusColor(HiveStatus status)
        {
            switch (status)
            {
                case HiveStatus.Default:
                    return HiveUIColors.Gray; // 为默认状态添加一个颜色
                case HiveStatus.CommunicationError:
                    return _communicationErrorColor;
                case HiveStatus.CommandError:
                    return _commandErrorColor;
                case HiveStatus.Offboard:
                    return _offboardColor;
                case HiveStatus.RunningState1:
                    return _runningColor;
                case HiveStatus.IdleState2:
                    return _idleColor;
                case HiveStatus.DownState5:
                    return _downColor;
                default:
                    return Color.Gray;
            }
        }

        /// <summary>
        /// 绘制圆角矩形
        /// </summary>
        private void DrawRoundedRectangle(Graphics g, Color fillColor, Rectangle rect, int radius)
        {
            using (GraphicsPath path = CreateRoundedRectanglePath(rect, radius))
            using (Brush brush = new SolidBrush(fillColor))
            {
                g.FillPath(brush, path);
            }
        }

        /// <summary>
        /// 创建圆角矩形路径
        /// </summary>
        private GraphicsPath CreateRoundedRectanglePath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();

            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(rect.Location, size);

            // 左上角
            path.AddArc(arc, 180, 90);

            // 右上角
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);

            // 右下角
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // 左下角
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        /// <summary>
        /// 绘制状态文本
        /// </summary>
        private void DrawStatusText(Graphics g, string text)
        {
            // 创建文本格式
            StringFormat format = new StringFormat();
            format.Alignment = _textAlignment;
            format.LineAlignment = StringAlignment.Center;

            // 计算文本区域
            Rectangle textRect = this.ClientRectangle;

            // 绘制文本
            using (Brush textBrush = new SolidBrush(_textColor))
            {
                g.DrawString(text, _statusFont, textBrush, textRect, format);
            }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 设置状态
        /// </summary>
        /// <param name="status">新的状态</param>
        public void SetStatus(HiveStatus status)
        {
            HiveStatus oldStatus = _currentStatus;
            this.CurrentStatus = status;

            // 触发状态改变事件
            if (oldStatus != status)
            {   /* if(!HiveMgr.Instance.IsFormOpenInNamespace(this.GetType().Namespace, "Form_OffboardMsg"))*/
                OnStatusChanged(new StatusChangedEventArgs(oldStatus, status));
            }
        }

        /// <summary>
        /// 设置所有状态颜色
        /// </summary>
        public void SetAllStatusColors(Color communicationError, Color commandError, Color offboard,
                                      Color running, Color idle, Color down)
        {
            _communicationErrorColor = communicationError;
            _commandErrorColor = commandError;
            _offboardColor = offboard;
            _runningColor = running;
            _idleColor = idle;
            _downColor = down;
            Invalidate();
        }

        /// <summary>
        /// 设置文本样式
        /// </summary>
        /// <param name="font">字体</param>
        /// <param name="color">颜色</param>
        public void SetTextStyle(Font font, Color color)
        {
            this.StatusFont = font;
            this.TextColor = color;
        }

        /// <summary>
        /// 重置字体为默认值
        /// </summary>
        public void ResetFont()
        {
            if (_statusFont != null)
                _statusFont.Dispose();

            _statusFont = new Font("Microsoft YaHei", 9f, FontStyle.Bold);
            _useCustomFont = false;
            Invalidate();
        }

        /// <summary>
        /// 获取状态描述
        /// </summary>
        public string GetStatusDescription()
        {
            switch (_currentStatus)
            {
                case HiveStatus.Default:
                    return "Default state";
                case HiveStatus.CommunicationError:
                    return "The station sends data to HIVE but does not get any response, or error received from the server";
                case HiveStatus.CommandError:
                    return "Command error";
                case HiveStatus.Offboard:
                    return "Offboard Station has not been onboarded";
                case HiveStatus.RunningState1:
                    return "Running Hive is in machine state 1";
                case HiveStatus.IdleState2:
                    return "Idle Hive is in machine state 2";
                case HiveStatus.DownState5:
                    return "Down Hive is in machine state 5";
                default:
                    return "Unknown status";
            }
        }

        // 新增Timer控制方法
        /// <summary>
        /// 启动定时器
        /// </summary>
        public void StartTimer()
        {
            TimerEnabled = true;
            this.Invalidate(); // 重绘以显示状态指示器
        }

        /// <summary>
        /// 停止定时器
        /// </summary>
        public void StopTimer()
        {
            TimerEnabled = false;
            this.Invalidate(); // 重绘以隐藏状态指示器
        }

        /// <summary>
        /// 重置刷新计数器
        /// </summary>
        public void ResetRefreshCount()
        {
            _refreshCount = 0;
            _lastRefreshTime = DateTime.MinValue;
        }

        /// <summary>
        /// 立即触发一次刷新
        /// </summary>
        public void RefreshNow()
        {
            RefreshTimer_Tick(this, EventArgs.Empty);
        }

        /// <summary>
        /// 定时器刷新状态（供外部重写）
        /// </summary>
        protected virtual void TimerRefreshStatus()
        {
            // 这里可以添加自动刷新状态的逻辑
            // 例如从外部系统获取状态更新
            // 默认不执行任何操作，由用户通过事件处理
            // 设计时不执行状态更新
            if (DesignMode)
                return;

            SetStatus((HiveStatus)HiveTool.CurrentHiveStatus);

        }

        #endregion

        #region 设计时支持

        /// <summary>
        /// 是否应该序列化StatusFont属性
        /// </summary>
        private bool ShouldSerializeStatusFont()
        {
            return _useCustomFont;
        }

        /// <summary>
        /// 重置StatusFont属性
        /// </summary>
        private void ResetStatusFont()
        {
            ResetFont();
        }

        #endregion
    }

    /// <summary>
    /// 状态改变事件参数
    /// </summary>
    public class StatusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 获取之前的状态
        /// </summary>
        public HiveStatusControl.HiveStatus OldStatus { get; }

        /// <summary>
        /// 获取新的状态
        /// </summary>
        public HiveStatusControl.HiveStatus NewStatus { get; }

        /// <summary>
        /// 初始化状态改变事件参数
        /// </summary>
        public StatusChangedEventArgs(HiveStatusControl.HiveStatus oldStatus, HiveStatusControl.HiveStatus newStatus)
        {
            OldStatus = oldStatus;
            NewStatus = newStatus;
        }
    }

    /// <summary>
    /// HIVE二维码显示控件
    /// </summary>
    [DefaultProperty("StationID")]
    [ToolboxBitmap(typeof(HiveQRCodeControl), "HiveQRCodeControl.bmp")]
    public class HiveQRCodeControl : Control
    {
        #region 私有字段

        private PictureBox _qrCodePictureBox;
        private string _stationID = HiveMgr.hiveStationInfo.stationID;//SystemMgr.GetInstance().GetSystemParamString("SFCStationID");
        private string _product = HiveMgr.hiveStationInfo.product;
        private string _vendor = HiveMgr.hiveStationInfo.vendor;
        private string _errorCode = "";
        private int _hiveState = 1;
        private float _displayPercentage = 1.0f; // 默认显示80%大小
        private Color _qrDarkColor = Color.Black;
        private Color _qrLightColor = Color.White;
        private int _pixelsPerModule = 16;
        private bool _autoGenerate = true;
        private ErrorCorrectionLevel _errorCorrectionLevel = ErrorCorrectionLevel.Q;
        private bool _showBorder = false;

        // 新增Timer相关字段
        private System.Windows.Forms.Timer _refreshTimer;
        private int _refreshInterval = 1000; // 默认1秒
        private bool _timerEnabled = true;
        private DateTime _lastRefreshTime = DateTime.MinValue;
        private int _refreshCount = 0;

        #endregion

        #region 公共属性

        // 原有的属性保持不变...

        /// <summary>
        /// 获取或设置错误代码
        /// </summary>
        [Category("HIVE数据")]
        [Description("错误代码")]
        [DefaultValue("")]
        public string ErrorCode
        {
            get { return _errorCode; }
            set
            {
                _errorCode = value;
                UpdateQRCodeVisibility();
                if (_autoGenerate) GenerateQRCode();
            }
        }

        /// <summary>
        /// 获取或设置HIVE状态
        /// </summary>
        [Category("HIVE数据")]
        [Description("HIVE机器状态 (1-5)")]
        [DefaultValue(1)]
        public int HiveState
        {
            get { return _hiveState; }
            set
            {
                _hiveState = Math.Max(1, Math.Min(5, value)); // 限制在1-5范围内
                UpdateQRCodeVisibility();
                if (_autoGenerate) GenerateQRCode();
            }
        }

        /// <summary>
        /// 获取或设置二维码显示百分比（相对于控件大小）
        /// </summary>
        [Category("外观")]
        [Description("二维码显示大小相对于控件的百分比 (0.1-1.0)")]
        [DefaultValue(1.0f)]
        public float DisplayPercentage
        {
            get { return _displayPercentage; }
            set
            {
                _displayPercentage = Math.Max(0.1f, Math.Min(1.0f, value)); // 限制在10%-100%范围内
                UpdateControlLayout();
                if (_autoGenerate) GenerateQRCode();
            }
        }

        /// <summary>
        /// 获取或设置二维码深色模块颜色
        /// </summary>
        [Category("外观")]
        [Description("二维码深色模块的颜色")]
        [DefaultValue(typeof(Color), "Black")]
        public Color QRCodeDarkColor
        {
            get { return _qrDarkColor; }
            set
            {
                _qrDarkColor = value;
                if (_autoGenerate) GenerateQRCode();
            }
        }

        /// <summary>
        /// 获取或设置二维码浅色模块颜色
        /// </summary>
        [Category("外观")]
        [Description("二维码浅色模块的颜色")]
        [DefaultValue(typeof(Color), "White")]
        public Color QRCodeLightColor
        {
            get { return _qrLightColor; }
            set
            {
                _qrLightColor = value;
                if (_autoGenerate) GenerateQRCode();
            }
        }

        /// <summary>
        /// 获取或设置每个模块的像素数
        /// </summary>
        [Category("外观")]
        [Description("每个QR码模块的像素数")]
        [DefaultValue(4)]
        public int PixelsPerModule
        {
            get { return _pixelsPerModule; }
            set
            {
                _pixelsPerModule = Math.Max(1, value);
                if (_autoGenerate) GenerateQRCode();
            }
        }

        /// <summary>
        /// 获取或设置是否显示边框
        /// </summary>
        [Category("外观")]
        [Description("是否显示控件边框")]
        [DefaultValue(false)]
        public bool ShowBorder
        {
            get { return _showBorder; }
            set
            {
                _showBorder = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置是否自动生成二维码
        /// </summary>
        [Category("行为")]
        [Description("是否在属性改变时自动生成二维码")]
        [DefaultValue(true)]
        public bool AutoGenerate
        {
            get { return _autoGenerate; }
            set { _autoGenerate = value; }
        }

        /// <summary>
        /// 获取或设置错误校正级别
        /// </summary>
        [Category("行为")]
        [Description("QR码的错误校正级别")]
        public ErrorCorrectionLevel ErrorCorrectionLevel
        {
            get { return _errorCorrectionLevel; }
            set
            {
                _errorCorrectionLevel = value;
                if (_autoGenerate) GenerateQRCode();
            }
        }

        // 新增Timer相关属性
        /// <summary>
        /// 获取或设置定时器刷新间隔（毫秒）
        /// </summary>
        [Category("定时器")]
        [Description("定时器刷新间隔（毫秒）")]
        [DefaultValue(1000)]
        public int RefreshInterval
        {
            get { return _refreshInterval; }
            set
            {
                if (value < 100) value = 100; // 最小100毫秒
                _refreshInterval = value;
                if (_refreshTimer != null)
                {
                    _refreshTimer.Interval = value;
                }
            }
        }

        /// <summary>
        /// 获取或设置是否启用定时器
        /// </summary>
        [Category("定时器")]
        [Description("是否启用定时刷新")]
        [DefaultValue(true)]
        public bool TimerEnabled
        {
            get { return _timerEnabled; }
            set
            {
                if (_timerEnabled != value)
                {
                    _timerEnabled = value;
                    if (_refreshTimer != null)
                    {
                        _refreshTimer.Enabled = value;
                    }
                }
            }
        }

        /// <summary>
        /// 获取最后一次刷新时间
        /// </summary>
        [Category("定时器")]
        [Description("最后一次刷新时间")]
        [Browsable(false)]
        public DateTime LastRefreshTime
        {
            get { return _lastRefreshTime; }
        }

        /// <summary>
        /// 获取刷新次数统计
        /// </summary>
        [Category("定时器")]
        [Description("刷新次数统计")]
        [Browsable(false)]
        public int RefreshCount
        {
            get { return _refreshCount; }
        }

        /// <summary>
        /// 获取当前QR码内容
        /// </summary>
        [Browsable(false)]
        public string CurrentQRContent
        {
            get { return GetQRContent(); }
        }

        /// <summary>
        /// 获取QR码是否可见
        /// </summary>
        [Browsable(false)]
        public bool IsQRCodeVisible
        {
            get { return _qrCodePictureBox != null && _qrCodePictureBox.Visible; }
        }

        #endregion

        #region 事件

        /// <summary>
        /// 定时器刷新时触发的事件
        /// </summary>
        [Category("行为")]
        [Description("定时器刷新时触发")]
        public event EventHandler<TimerRefreshEventArgs> TimerRefresh;

        #endregion

        #region 构造函数

        /// <summary>
        /// 初始化HIVE二维码控件
        /// </summary>
        public HiveQRCodeControl()
        {
            // 设置默认属性
            this.Size = new Size(140, 140);
            this.BackColor = SystemColors.Control;

            // 启用双缓冲
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint |
                         ControlStyles.ResizeRedraw |
                         ControlStyles.OptimizedDoubleBuffer, true);
            InitializeComponents();
            InitializeTimer();
        }

        #endregion

        #region 控件初始化

        private void InitializeComponents()
        {
            // 创建QR码图片框
            _qrCodePictureBox = new PictureBox();
            _qrCodePictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            _qrCodePictureBox.BorderStyle = BorderStyle.None; // 去除二维码边框
            _qrCodePictureBox.BackColor = Color.White;
            _qrCodePictureBox.Dock = DockStyle.Fill;
            // 添加到控件
            this.Controls.Add(_qrCodePictureBox);

            // 初始布局
            UpdateControlLayout();
            UpdateQRCodeVisibility();
        }

        private void InitializeTimer()
        {
            _refreshTimer = new System.Windows.Forms.Timer();
            _refreshTimer.Interval = _refreshInterval;
            _refreshTimer.Tick += RefreshTimer_Tick;
            _refreshTimer.Enabled = _timerEnabled;
        }

        private void UpdateControlLayout()
        {
            if (_qrCodePictureBox != null)
            {
                // 根据显示百分比计算二维码大小
                int qrSize = (int)(Math.Min(this.Width, this.Height) * _displayPercentage);

                // 确保最小尺寸
                qrSize = Math.Max(20, qrSize);

                // 计算居中位置
                int x = (this.Width - qrSize) / 2;
                int y = (this.Height - qrSize) / 2;

                _qrCodePictureBox.Location = new Point(x, y);
                _qrCodePictureBox.Size = new Size(qrSize, qrSize);
            }
        }

        #endregion

        #region Timer事件处理

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                _refreshCount++;
                _lastRefreshTime = DateTime.Now;

                // 触发刷新事件
                OnTimerRefresh(new TimerRefreshEventArgs(_refreshCount, _lastRefreshTime));

                TimerGenerateQRCode();
            }
            catch (Exception ex)
            {
                // 记录错误但不中断定时器
                System.Diagnostics.Debug.WriteLine($"定时器刷新错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 触发定时器刷新事件
        /// </summary>
        protected virtual void OnTimerRefresh(TimerRefreshEventArgs e)
        {
            TimerRefresh?.Invoke(this, e);
        }

        #endregion

        #region 重写方法

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateControlLayout();
            // 如果控件大小改变且需要显示二维码，重新生成以适应新大小
            if (_autoGenerate && IsQRCodeVisible)
            {
                GenerateQRCode();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // 绘制边框（如果启用）
            if (_showBorder)
            {
                using (Pen borderPen = new Pen(this.ForeColor, 1))
                {
                    e.Graphics.DrawRectangle(borderPen, 0, 0, this.Width - 1, this.Height - 1);
                }
            }

            // 如果没有显示QR码，绘制提示文本
            if (!_qrCodePictureBox.Visible)
            {
                // 原有的提示文本绘制代码...
            }

            //// 如果定时器启用，在角落显示状态指示器
            //if (_timerEnabled && _refreshTimer != null && _refreshTimer.Enabled)
            //{
            //    using (Brush indicatorBrush = new SolidBrush(Color.LimeGreen))
            //    {
            //        e.Graphics.FillEllipse(indicatorBrush, this.Width - 12, 2, 10, 10);
            //    }
            //}
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _refreshTimer?.Stop();
                _refreshTimer?.Dispose();

                if (_qrCodePictureBox?.Image != null)
                {
                    _qrCodePictureBox.Image.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 获取状态消息
        /// </summary>
        private string GetStatusMessage()
        {
            if (_hiveState == 1 || _hiveState == 2)
            {
                return $"状态 {_hiveState}\n无需显示二维码";
            }
            else if (_hiveState == 5 && string.IsNullOrEmpty(_errorCode))
            {
                return "状态 5\n等待错误代码";
            }
            else
            {
                return "状态 3/4\n无需显示二维码";
            }
        }

        /// <summary>
        /// 获取QR码内容
        /// </summary>
        private string GetQRContent()
        {
            return $"{_stationID}+{_product}+{_vendor}+{_errorCode}";
        }

        /// <summary>
        /// 更新QR码可见性
        /// </summary>
        private void UpdateQRCodeVisibility()
        {

            bool shouldShow = (_hiveState == 5 && !string.IsNullOrEmpty(_errorCode) /*&& !HiveMgr.Instance.IsFormOpenInNamespace(this.GetType().Namespace, "Form_Operation")*/);

            if (_qrCodePictureBox != null)
            {
                _qrCodePictureBox.Visible = shouldShow;
            }

            this.Invalidate(); // 触发重绘以显示状态消息
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 生成二维码
        /// </summary>
        public void GenerateQRCode()
        {
            // 只有在状态5且有错误代码时才生成
            if (_hiveState != 5 || string.IsNullOrEmpty(_errorCode))
            {
                UpdateQRCodeVisibility();
                return;
            }

            try
            {
                string qrContent = GetQRContent();

                // 配置二维码参数
                var barcodeWriter = new BarcodeWriter
                {
                    Format = BarcodeFormat.QR_CODE,
                    Options = new QrCodeEncodingOptions
                    {
                        ErrorCorrection = _errorCorrectionLevel,
                        CharacterSet = "UTF-8",
                        Width = CalculateQRSize(),
                        Height = CalculateQRSize(),
                        //Margin = 1 // 二维码边距
                    }
                };

                // 设置渲染器
                barcodeWriter.Renderer = new BitmapRenderer();

                // 生成二维码位图
                Bitmap qrCodeBitmap = barcodeWriter.Write(qrContent);

                // 应用自定义颜色
                if (_qrDarkColor != Color.Black || _qrLightColor != Color.White)
                {
                    qrCodeBitmap = ApplyColorToQRCode(qrCodeBitmap, _qrDarkColor, _qrLightColor);
                }

                // 缩放图像（如果需要）
                if (_pixelsPerModule != 4)
                {
                    qrCodeBitmap = ScaleQRCode(qrCodeBitmap, _pixelsPerModule);
                }

                // 更新PictureBox中的图像
                if (_qrCodePictureBox.Image != null)
                {
                    _qrCodePictureBox.Image.Dispose();
                }
                _qrCodePictureBox.Image = qrCodeBitmap;

                UpdateQRCodeVisibility();
            }
            catch (Exception ex)
            {
                // 在设计时显示错误，在运行时可以选择记录日志
                if (this.DesignMode)
                {
                    _qrCodePictureBox.Image = CreateErrorImage();
                }
                else
                {
                    throw new Exception($"生成QR码时出错: {ex.Message}", ex);
                }
            }
        }
        /// <summary>
        /// 计算二维码大小
        /// </summary>
        private int CalculateQRSize()
        {
            // 基础大小，根据显示百分比调整
            int baseSize = (int)(Math.Min(this.Width, this.Height) * _displayPercentage);
            // 确保是最小尺寸
            baseSize = Math.Max(100, baseSize);
            // 根据像素每模块调整
            return (baseSize / _pixelsPerModule) * _pixelsPerModule;
        }
        /// <summary>
        /// 缩放二维码
        /// </summary>
        private Bitmap ScaleQRCode(Bitmap original, int pixelsPerModule)
        {
            // 计算新的尺寸
            int newWidth = original.Width * pixelsPerModule / 4;
            int newHeight = original.Height * pixelsPerModule / 4;

            Bitmap scaledBitmap = new Bitmap(newWidth, newHeight);
            using (Graphics g = Graphics.FromImage(scaledBitmap))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                g.DrawImage(original, 0, 0, newWidth, newHeight);
            }
            original.Dispose();
            return scaledBitmap;
        }
        /// <summary>
        /// 将黑白二维码应用自定义颜色
        /// </summary>
        private Bitmap ApplyColorToQRCode(Bitmap original, Color darkColor, Color lightColor)
        {
            Bitmap coloredBitmap = new Bitmap(original.Width, original.Height);

            for (int y = 0; y < original.Height; y++)
            {
                for (int x = 0; x < original.Width; x++)
                {
                    Color originalColor = original.GetPixel(x, y);

                    // 根据亮度决定使用深色还是浅色
                    float brightness = originalColor.GetBrightness();
                    Color newColor = brightness < 0.5f ? darkColor : lightColor;

                    coloredBitmap.SetPixel(x, y, newColor);
                }
            }

            original.Dispose();
            return coloredBitmap;
        }
        /// <summary>
        /// 生成QR码
        /// </summary>
        public void TimerGenerateQRCode()
        {
            HiveState = Convert.ToInt32(HiveTool.CurrentMachineState);
            ErrorCode = HiveTool.CurrentErrorCode;
        }
        /// <summary>
        /// 创建错误图像
        /// </summary>
        private Bitmap CreateErrorImage()
        {
            int qrSize = _qrCodePictureBox.Width;
            Bitmap errorBmp = new Bitmap(qrSize, qrSize);
            using (Graphics g = Graphics.FromImage(errorBmp))
            using (Brush brush = new SolidBrush(Color.LightGray))
            using (Pen pen = new Pen(Color.Red, 2))
            using (Font font = new Font("Arial", Math.Max(8, qrSize / 15)))
            using (Brush textBrush = new SolidBrush(Color.Red))
            {
                g.FillRectangle(brush, 0, 0, qrSize, qrSize);
                g.DrawRectangle(pen, 0, 0, qrSize - 1, qrSize - 1);

                string errorText = "QR Error";
                SizeF textSize = g.MeasureString(errorText, font);
                PointF textPos = new PointF(
                    (qrSize - textSize.Width) / 2,
                    (qrSize - textSize.Height) / 2);

                g.DrawString(errorText, font, textBrush, textPos);
            }
            return errorBmp;
        }

        /// <summary>
        /// 更新HIVE状态和错误代码
        /// </summary>
        /// <param name="state">HIVE状态 (1-5)</param>
        /// <param name="errorCode">错误代码</param>
        public void UpdateHiveState(int state, string errorCode)
        {
            _hiveState = Math.Max(1, Math.Min(5, state));
            _errorCode = errorCode ?? "";

            UpdateQRCodeVisibility();
            if (_autoGenerate) GenerateQRCode();
        }

        /// <summary>
        /// 设置所有HIVE参数
        /// </summary>
        public void SetHiveParameters(string stationID, string product, string vendor, int state, string errorCode)
        {
            _stationID = stationID;
            _product = product;
            _vendor = vendor;
            _hiveState = Math.Max(1, Math.Min(5, state));
            _errorCode = errorCode ?? "";

            UpdateQRCodeVisibility();
            if (_autoGenerate) GenerateQRCode();
        }

        /// <summary>
        /// 清除QR码
        /// </summary>
        public void ClearQRCode()
        {
            if (_qrCodePictureBox.Image != null)
            {
                _qrCodePictureBox.Image.Dispose();
                _qrCodePictureBox.Image = null;
            }
            _qrCodePictureBox.Visible = false;
            this.Invalidate();
        }

        /// <summary>
        /// 保存QR码图像到文件
        /// </summary>
        /// <param name="filename">文件名</param>
        /// <param name="format">图像格式</param>
        public void SaveQRCodeToFile(string filename, System.Drawing.Imaging.ImageFormat format)
        {
            if (_qrCodePictureBox.Image != null && _qrCodePictureBox.Visible)
            {
                _qrCodePictureBox.Image.Save(filename, format);
            }
            else
            {
                throw new InvalidOperationException("没有可用的QR码图像可以保存");
            }
        }

        // 新增Timer控制方法
        /// <summary>
        /// 启动定时器
        /// </summary>
        public void StartTimer()
        {
            TimerEnabled = true;
            this.Invalidate(); // 重绘以显示状态指示器
        }

        /// <summary>
        /// 停止定时器
        /// </summary>
        public void StopTimer()
        {
            TimerEnabled = false;
            this.Invalidate(); // 重绘以隐藏状态指示器
        }

        /// <summary>
        /// 重置刷新计数器
        /// </summary>
        public void ResetRefreshCount()
        {
            _refreshCount = 0;
            _lastRefreshTime = DateTime.MinValue;
        }

        /// <summary>
        /// 立即触发一次刷新
        /// </summary>
        public void RefreshNow()
        {
            RefreshTimer_Tick(this, EventArgs.Empty);
        }

        #endregion

        #region 设计时支持

        /// <summary>
        /// 设计时属性默认值
        /// </summary>
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            if (DesignMode)
            {
                UpdateControlLayout();
                UpdateQRCodeVisibility();
            }
        }

        #endregion
    }

    /// <summary>
    /// 定时器刷新事件参数
    /// </summary>
    public class TimerRefreshEventArgs : EventArgs
    {
        /// <summary>
        /// 刷新次数
        /// </summary>
        public int RefreshCount { get; }

        /// <summary>
        /// 刷新时间
        /// </summary>
        public DateTime RefreshTime { get; }

        /// <summary>
        /// 初始化事件参数
        /// </summary>
        public TimerRefreshEventArgs(int refreshCount, DateTime refreshTime)
        {
            RefreshCount = refreshCount;
            RefreshTime = refreshTime;
        }
    }


    /// <summary>
    /// Hive消息显示控件
    /// </summary>
    [DefaultProperty("MessageText")]
    [ToolboxBitmap(typeof(HiveMessageControl), "HiveMessageControl.bmp")]
    public class HiveMessageControl : UserControl
    {
        #region 私有字段

        private Panel panel1;
        private LinkLabel linkLabel1;
        private Label label1;

        private string _messageText = "";
        private string _linkText = "";
        private Color _messageColor = Color.Black;
        private Color _linkColor = Color.Black;
        private Font _messageFont = new Font("微软雅黑", 9f);
        private Font _linkFont = new Font("微软雅黑", 9f);
        private bool _autoSizeControl = true;
        private int _verticalSpacing = 5;
        private int _horizontalSpacing = 3;
        #endregion

        #region 构造函数

        /// <summary>
        /// 初始化Hive消息控件
        /// </summary>
        public HiveMessageControl()
        {
            InitializeComponent();
            SetupControl();
        }

        #endregion

        #region 组件初始化

        private void InitializeComponent()
        {
            this.panel1 = new Panel();
            this.label1 = new Label();
            this.linkLabel1 = new LinkLabel();
            this.panel1.SuspendLayout();
            this.SuspendLayout();

            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.AutoSize = true;
            this.panel1.Controls.Add(this.linkLabel1);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = DockStyle.Fill;
            this.panel1.Location = new Point(0, 0);
            this.panel1.Margin = new Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new Size(726, 28);
            this.panel1.TabIndex = 0;

            // 
            // linkLabel1
            // 
            this.linkLabel1.ActiveLinkColor = Color.Black;
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.BackColor = Color.Transparent;
            this.linkLabel1.Font = new Font("微软雅黑", 9f);
            this.linkLabel1.LinkBehavior = LinkBehavior.NeverUnderline;
            this.linkLabel1.LinkColor = Color.Black;
            this.linkLabel1.Location = new Point(234, 0);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new Size(98, 24);
            this.linkLabel1.TabIndex = 1;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "linkLabel1";
            this.linkLabel1.TextAlign = ContentAlignment.MiddleLeft;
            this.linkLabel1.VisitedLinkColor = Color.Black;
            this.linkLabel1.LinkClicked += new LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);

            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = Color.Transparent;
            this.label1.Font = new Font("微软雅黑", 9f);
            this.label1.Location = new Point(3, 0);
            this.label1.Margin = new Padding(3, 15, 3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new Size(63, 24);
            this.label1.TabIndex = 0;
            this.label1.Text = "label1";
            this.label1.TextAlign = ContentAlignment.MiddleLeft;

            // 
            // HiveMessageControl
            // 
            this.AutoScaleDimensions = new SizeF(9F, 18F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.panel1);
            this.Name = "HiveMessageControl";
            this.Size = new Size(726, 28);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void SetupControl()
        {
            // 设置默认属性
            this.Size = new Size(726, 28);
            this.BackColor = Color.White;
            this.ForeColor = Color.Black;

            // 启用双缓冲
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint |
                         ControlStyles.ResizeRedraw |
                         ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.SupportsTransparentBackColor, true);
        }

        #endregion

        #region 公共属性

        /// <summary>
        /// 获取或设置消息文本
        /// </summary>
        [Category("消息内容")]
        [Description("显示的消息文本")]
        [DefaultValue("")]
        public string MessageText
        {
            get { return _messageText; }
            set
            {
                _messageText = value;
                label1.Text = value;
                UpdateLayout();
            }
        }

        /// <summary>
        /// 获取或设置链接文本
        /// </summary>
        [Category("消息内容")]
        [Description("显示的链接文本")]
        [DefaultValue("")]
        public string LinkText
        {
            get { return _linkText; }
            set
            {
                _linkText = value;
                linkLabel1.Text = value;
                linkLabel1.Visible = !string.IsNullOrEmpty(value);
                UpdateLayout();
            }
        }

        /// <summary>
        /// 获取或设置消息文本颜色
        /// </summary>
        [Category("外观")]
        [Description("消息文本的颜色")]
        [DefaultValue(typeof(Color), "Black")]
        public Color MessageColor
        {
            get { return _messageColor; }
            set
            {
                _messageColor = value;
                label1.ForeColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置链接文本颜色
        /// </summary>
        [Category("外观")]
        [Description("链接文本的颜色")]
        [DefaultValue(typeof(Color), "Black")]
        public Color LinkColor
        {
            get { return _linkColor; }
            set
            {
                _linkColor = value;
                linkLabel1.LinkColor = value;
                linkLabel1.ActiveLinkColor = value;
                linkLabel1.VisitedLinkColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// 获取或设置消息文本字体
        /// </summary>
        [Category("外观")]
        [Description("消息文本的字体")]
        public Font MessageFont
        {
            get { return _messageFont; }
            set
            {
                _messageFont = value;
                label1.Font = value;
                UpdateLayout();
            }
        }

        /// <summary>
        /// 获取或设置链接文本字体
        /// </summary>
        [Category("外观")]
        [Description("链接文本的字体")]
        public Font LinkFont
        {
            get { return _linkFont; }
            set
            {
                _linkFont = value;
                linkLabel1.Font = value;
                UpdateLayout();
            }
        }

        /// <summary>
        /// 获取或设置是否自动调整控件大小
        /// </summary>
        [Category("布局")]
        [Description("是否根据内容自动调整控件大小")]
        [DefaultValue(true)]
        public bool AutoSizeControl
        {
            get { return _autoSizeControl; }
            set
            {
                _autoSizeControl = value;
                this.AutoSize = value;
                panel1.AutoSize = value;
                UpdateLayout();
            }
        }
        /// <summary>
        /// 获取或设置垂直间距
        /// </summary>
        [Category("布局")]
        [Description("控件内部的垂直间距")]
        [DefaultValue(5)]
        public int VerticalSpacing
        {
            get { return _verticalSpacing; }
            set
            {
                _verticalSpacing = value;
                UpdateLayout();
            }
        }
        /// <summary>
        /// 获取或设置水平间距
        /// </summary>
        [Category("布局")]
        [Description("消息和链接之间的水平间距")]
        [DefaultValue(3)]
        public int HorizontalSpacing
        {
            get { return _horizontalSpacing; }
            set
            {
                _horizontalSpacing = value;
                UpdateLayout();
            }
        }
        /// <summary>
        /// 获取消息标签控件
        /// </summary>
        [Browsable(false)]
        public Label MessageLabel
        {
            get { return label1; }
        }

        /// <summary>
        /// 获取链接标签控件
        /// </summary>
        [Browsable(false)]
        public LinkLabel MessageLinkLabel
        {
            get { return linkLabel1; }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 设置消息内容
        /// </summary>
        /// <param name="message">消息文本</param>
        /// <param name="linkText">链接文本（可选）</param>
        public void SetValue(string message, string linkText = "")
        {
            _messageText = message;
            _linkText = linkText;

            label1.Text = message;

            if (!string.IsNullOrEmpty(linkText))
            {
                linkLabel1.Font = new Font(linkLabel1.Font, FontStyle.Underline);
                linkLabel1.Text = linkText;
                linkLabel1.Visible = true;
                linkLabel1.ForeColor = Color.Red;
            }
            else
            {
                linkLabel1.Visible = false;
            }

            UpdateLayout();
        }

        /// <summary>
        /// 设置消息样式
        /// </summary>
        /// <param name="messageFont">消息字体</param>
        /// <param name="messageColor">消息颜色</param>
        /// <param name="linkFont">链接字体</param>
        /// <param name="linkColor">链接颜色</param>
        public void SetMessageStyle(Font messageFont, Color messageColor, Font linkFont, Color linkColor)
        {
            MessageFont = messageFont;
            MessageColor = messageColor;
            LinkFont = linkFont;
            LinkColor = linkColor;
        }

        /// <summary>
        /// 清除消息内容
        /// </summary>
        public void Clear()
        {
            SetValue("", "");
        }

        #endregion

        #region 私有方法

        public void UpdateLayout()
        {
            SuspendLayout();

            try
            {
                // 重置位置到左上角
                label1.Location = new Point(0, 0);
                linkLabel1.Location = new Point(0, 0);

                // 强制更新布局以确保尺寸正确
                label1.PerformLayout();
                if (linkLabel1.Visible)
                {
                    linkLabel1.PerformLayout();
                }

                // 计算位置 - 使用更可靠的方法
                //if (linkLabel1.Visible && !string.IsNullOrEmpty(linkLabel1.Text))
                //{
                // 有链接文本的情况
                linkLabel1.Location = new Point(label1.Right + _horizontalSpacing, 0);

                // 确保链接标签与消息标签垂直对齐
                int verticalOffset = (label1.Height - linkLabel1.Height) / 2;
                if (verticalOffset > 0)
                {
                    linkLabel1.Top = verticalOffset;
                }
                //}

                // 计算所需高度 - 取两个控件中较高的一个
                int maxHeight = Math.Max(label1.Height, linkLabel1.Visible ? linkLabel1.Height : 0);

                // 设置控件高度，确保文本完全显示
                this.Height = maxHeight + _verticalSpacing;
                panel1.Height = this.Height;

                // 如果设置了自动调整大小，更新宽度
                if (_autoSizeControl)
                {
                    int totalWidth = label1.Width +
                                   (linkLabel1.Visible ? (linkLabel1.Width + _horizontalSpacing) : 0);
                    this.Width = totalWidth + 10; // 添加一些边距
                }

                // 强制重绘
                Invalidate();
            }
            finally
            {
                ResumeLayout(true);
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (linkLabel1.Text.Contains('<') && linkLabel1.Text.Contains('>'))
            {
                string Picturename = linkLabel1.Text.TrimStart('<').TrimEnd('>');
                string path = Path.Combine(Ini.GetStringValue(HiveCommandProcessor.m_strIniFile, "LocalData", "PicturePath", "D:\\hive\\Image"), Picturename);

                OpenImage(path);
                return;
            }
            else
            {
                string path = linkLabel1.Text;
                if (path.Contains("jpg"))
                {
                    path = "D:\\Hive\\SOP\\" + path;
                    OpenImage(path);
                }
                else
                {
                    OpenDirectory(path);
                }
            }
        }

        private void OpenImage(string imagePath)
        {
            if (!System.IO.File.Exists(imagePath))
            {
                MessageBox.Show("文件不存在");
                return;
            }
            Process.Start(new ProcessStartInfo(imagePath) { UseShellExecute = true });
        }

        private void OpenDirectory(string directoryPath)
        {
            if (!System.IO.Directory.Exists(directoryPath))
            {
                MessageBox.Show("文件夹不存在");
                return;
            }
            Process.Start(new ProcessStartInfo(directoryPath) { UseShellExecute = true });
        }

        #endregion

        #region 重写方法

        /// <summary>
        /// 控件大小改变时
        /// </summary>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (!_autoSizeControl)
            {
                UpdateLayout();
            }
        }

        /// <summary>
        /// 字体改变时
        /// </summary>
        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            UpdateLayout();
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_messageFont != null)
                {
                    _messageFont.Dispose();
                }
                if (_linkFont != null)
                {
                    _linkFont.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #endregion

        #region 设计时支持

        /// <summary>
        /// 是否应该序列化MessageFont属性
        /// </summary>
        private bool ShouldSerializeMessageFont()
        {
            return _messageFont != null && !_messageFont.Equals(new Font("微软雅黑", 9f));
        }

        /// <summary>
        /// 重置MessageFont属性
        /// </summary>
        private void ResetMessageFont()
        {
            MessageFont = new Font("微软雅黑", 9f);
        }

        /// <summary>
        /// 是否应该序列化LinkFont属性
        /// </summary>
        private bool ShouldSerializeLinkFont()
        {
            return _linkFont != null && !_linkFont.Equals(new Font("微软雅黑", 9f));
        }

        /// <summary>
        /// 重置LinkFont属性
        /// </summary>
        private void ResetLinkFont()
        {
            LinkFont = new Font("微软雅黑", 9f);
        }

        #endregion
    }

    public partial class MaintenanceTimerControl : UserControl
    {
        #region 私有字段

        private Label lblWeeklyLabel;
        private Label lblWeeklyCountdown;
        private Label lblMonthlyLabel;
        private Label lblMonthlyCountdown;
        private Timer maintenanceTimer;
        private int weeklyRemainingDays = 0;
        private int monthlyRemainingDays = 0;
        private TableLayoutPanel mainLayoutPanel;

        #endregion

        #region 构造函数

        public MaintenanceTimerControl()
        {
            InitializeComponent();
            SetupComponents();
            StartMaintenanceTimer();
        }

        #endregion

        #region 组件初始化

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // MaintenanceTimerControl
            // 
            this.AutoScaleDimensions = new SizeF(5F, 9F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Name = "MaintenanceTimerControl";
            this.Size = new Size(200, 80); // 增加默认宽度以适应内容
            this.ResumeLayout(false);
        }

        private void SetupComponents()
        {
            // 设置控件基本属性
            this.Size = new Size(200, 80);
            this.BackColor = Color.Transparent;
            this.MinimumSize = new Size(60, 30); // 设置最小尺寸

            // 创建主布局面板
            mainLayoutPanel = new TableLayoutPanel
            {
                Name = "mainLayoutPanel",
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                BackColor = Color.Transparent
            };

            // 设置列宽比例：标签列60%，数值列40%
            mainLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            mainLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));

            // 设置行高：每行50%
            mainLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            mainLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            // 创建周维护标签
            lblWeeklyLabel = new Label
            {
                Name = "lblWeeklyLabel",
                Text = "Weekly:",
                Font = new Font("Microsoft YaHei", 8F, FontStyle.Regular, GraphicsUnit.Point, 134),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                ForeColor = SystemColors.ControlText,
                Margin = new Padding(0)
            };

            // 创建周维护倒计时
            lblWeeklyCountdown = new Label
            {
                Name = "lblWeeklyCountdown",
                Text = "0D",
                Font = new Font("Microsoft YaHei", 8F, FontStyle.Regular, GraphicsUnit.Point, 134),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = HiveUIColors.Red,
                Margin = new Padding(0)
            };

            // 创建月维护标签
            lblMonthlyLabel = new Label
            {
                Name = "lblMonthlyLabel",
                Text = "Monthly:",
                Font = new Font("Microsoft YaHei", 8F, FontStyle.Regular, GraphicsUnit.Point, 134),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                ForeColor = SystemColors.ControlText,
                Margin = new Padding(0)
            };

            // 创建月维护倒计时
            lblMonthlyCountdown = new Label
            {
                Name = "lblMonthlyCountdown",
                Text = "0D",
                Font = new Font("Microsoft YaHei", 8F, FontStyle.Regular, GraphicsUnit.Point, 134),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = HiveUIColors.Red,
                Margin = new Padding(0)
            };

            // 将控件添加到布局面板
            mainLayoutPanel.Controls.Add(lblWeeklyLabel, 0, 0);
            mainLayoutPanel.Controls.Add(lblWeeklyCountdown, 1, 0);
            mainLayoutPanel.Controls.Add(lblMonthlyLabel, 0, 1);
            mainLayoutPanel.Controls.Add(lblMonthlyCountdown, 1, 1);

            // 创建维护定时器
            maintenanceTimer = new Timer
            {
                Interval = 1000 // 1秒更新一次
            };
            maintenanceTimer.Tick += MaintenanceTimer_Tick;

            // 添加布局面板到容器
            this.Controls.Add(mainLayoutPanel);
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 启动维护定时器
        /// </summary>
        public void StartMaintenanceTimer()
        {
            maintenanceTimer.Start();
        }

        /// <summary>
        /// 停止维护定时器
        /// </summary>
        public void StopMaintenanceTimer()
        {
            maintenanceTimer.Stop();
        }

        /// <summary>
        /// 立即更新维护时间显示
        /// </summary>
        public void RefreshMaintenanceTime()
        {
            UpdateMaintenanceTimeDisplay();
        }

        /// <summary>
        /// 设置定时器更新间隔
        /// </summary>
        /// <param name="interval">间隔时间（毫秒）</param>
        public void SetUpdateInterval(int interval)
        {
            maintenanceTimer.Interval = Math.Max(100, interval);
        }

        #endregion

        #region 私有方法

        private void MaintenanceTimer_Tick(object sender, EventArgs e)
        {
            UpdateMaintenanceTimeDisplay();
        }

        private void UpdateMaintenanceTimeDisplay()
        {
            try
            {
                // 从配置文件读取维护时间
                DateTime weekMaintenanceTime = Convert.ToDateTime(
                    Ini.GetStringValue(
                        HiveCommandProcessor.m_strMaintenanceIniFile,
                        "Weekly",
                        "LastMaintenanceDate",
                        DateTime.Now.ToString()));

                DateTime monthMaintenanceTime = Convert.ToDateTime(
                    Ini.GetStringValue(
                        HiveCommandProcessor.m_strMaintenanceIniFile,
                        "Monthly",
                        "LastMaintenanceDate",
                        DateTime.Now.ToString()));

                // 计算剩余天数
                weeklyRemainingDays = (int)(7 - (DateTime.Now - weekMaintenanceTime).TotalDays);
                monthlyRemainingDays = (int)(31 - (DateTime.Now - monthMaintenanceTime).TotalDays);

                // 更新UI（确保线程安全）
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(UpdateDisplayLabels));
                }
                else
                {
                    UpdateDisplayLabels();
                }
            }
            catch (Exception ex)
            {
                // 记录错误或显示错误信息
                System.Diagnostics.Debug.WriteLine($"更新维护时间显示时出错: {ex.Message}");
            }
        }

        private void UpdateDisplayLabels()
        {
            // 更新周维护显示
            lblWeeklyCountdown.Text = $"{weeklyRemainingDays}D";
            lblWeeklyCountdown.ForeColor = weeklyRemainingDays > 0 ? HiveUIColors.Green : HiveUIColors.Red;

            // 更新月维护显示
            lblMonthlyCountdown.Text = $"{monthlyRemainingDays}D";
            lblMonthlyCountdown.ForeColor = monthlyRemainingDays > 0 ? HiveUIColors.Green : HiveUIColors.Red;
        }

        #endregion

        #region 重写方法

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            AdjustFontSize();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            AdjustFontSize();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                maintenanceTimer?.Stop();
                maintenanceTimer?.Dispose();

                mainLayoutPanel?.Dispose();
                lblWeeklyLabel?.Dispose();
                lblWeeklyCountdown?.Dispose();
                lblMonthlyLabel?.Dispose();
                lblMonthlyCountdown?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        #region 布局优化方法

        /// <summary>
        /// 根据控件大小自动调整字体大小
        /// </summary>
        private void AdjustFontSize()
        {
            try
            {
                // 计算基于控件高度的字体大小
                int baseFontSize = Math.Max(8, this.Height / 8);

                // 创建新字体
                Font newFont = new Font("Microsoft YaHei", baseFontSize, FontStyle.Regular, GraphicsUnit.Point, 134);

                // 应用新字体到所有标签
                //if (lblWeeklyLabel != null) lblWeeklyLabel.Font = newFont;
                //if (lblWeeklyCountdown != null) lblWeeklyCountdown.Font = newFont;
                //if (lblMonthlyLabel != null) lblMonthlyLabel.Font = newFont;
                //if (lblMonthlyCountdown != null) lblMonthlyCountdown.Font = newFont;
                // 注意：这里没有释放旧字体，因为控件会管理自己的字体资源
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"调整字体大小时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 设置布局比例
        /// </summary>
        /// <param name="labelColumnPercentage">标签列宽度百分比</param>
        /// <param name="valueColumnPercentage">数值列宽度百分比</param>
        public void SetLayoutProportions(float labelColumnPercentage, float valueColumnPercentage)
        {
            if (mainLayoutPanel != null && mainLayoutPanel.ColumnStyles.Count >= 2)
            {
                float total = labelColumnPercentage + valueColumnPercentage;
                if (total > 0)
                {
                    // 归一化百分比
                    mainLayoutPanel.ColumnStyles[0].Width = labelColumnPercentage / total * 100;
                    mainLayoutPanel.ColumnStyles[1].Width = valueColumnPercentage / total * 100;
                }
            }
        }

        #endregion

        #region 公共属性

        /// <summary>
        /// 获取周维护剩余天数
        /// </summary>
        [Browsable(false)]
        public int WeeklyRemainingDays => weeklyRemainingDays;

        /// <summary>
        /// 获取月维护剩余天数
        /// </summary>
        [Browsable(false)]
        public int MonthlyRemainingDays => monthlyRemainingDays;

        /// <summary>
        /// 获取或设置周维护标签文本
        /// </summary>
        [Category("外观")]
        [Description("周维护标签显示文本")]
        [DefaultValue("Weekly:")]
        public string WeeklyLabelText
        {
            get => lblWeeklyLabel.Text;
            set => lblWeeklyLabel.Text = value;
        }

        /// <summary>
        /// 获取或设置月维护标签文本
        /// </summary>
        [Category("外观")]
        [Description("月维护标签显示文本")]
        [DefaultValue("Monthly:")]
        public string MonthlyLabelText
        {
            get => lblMonthlyLabel.Text;
            set => lblMonthlyLabel.Text = value;
        }

        /// <summary>
        /// 获取或设置定时器更新间隔（毫秒）
        /// </summary>
        [Category("行为")]
        [Description("定时器更新间隔（毫秒）")]
        [DefaultValue(1000)]
        public int TimerInterval
        {
            get => maintenanceTimer.Interval;
            set => maintenanceTimer.Interval = Math.Max(100, value);
        }

        /// <summary>
        /// 获取或设置是否启用自动字体调整
        /// </summary>
        [Category("行为")]
        [Description("是否根据控件大小自动调整字体")]
        [DefaultValue(true)]
        public bool AutoAdjustFont { get; set; } = true;

        #endregion
    }

    public class TextMultiLineButton : Button
    {
        private List<TextLine> _textLines = new List<TextLine>();

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<TextLine> TextLines
        {
            get => _textLines;
            set
            {
                _textLines = value ?? new List<TextLine>();
                Invalidate();
            }
        }

        [DefaultValue(5)]
        public int LineSpacing { get; set; } = 5;

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string Text
        {
            get => base.Text;
            set
            {
                // 如果设置了TextLines，则忽略Text属性的设置
                if (_textLines.Count == 0)
                {
                    base.Text = value;
                }
            }
        }

        public TextMultiLineButton()
        {
            SetStyle(ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw, true);

            // 默认使用TextLines而不是Text
            base.Text = string.Empty;
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            // 先调用基类绘制背景、边框等
            base.OnPaint(pevent);

            // 如果有TextLines，则绘制多行文本
            if (_textLines.Count > 0)
            {
                DrawMultiLineText(pevent.Graphics);
            }
        }

        private void DrawMultiLineText(Graphics g)
        {
            if (_textLines.Count == 0) return;

            using (StringFormat sf = new StringFormat())
            {
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                sf.Trimming = StringTrimming.EllipsisCharacter;
                sf.FormatFlags = StringFormatFlags.NoWrap;

                // 计算总高度
                float totalHeight = CalculateTotalTextHeight(g);

                // 起始Y坐标（垂直居中）
                float startY = (Height - totalHeight) / 2;

                for (int i = 0; i < _textLines.Count; i++)
                {
                    var line = _textLines[i];

                    // 创建字体
                    using (Font font = new Font(Font.FontFamily, line.FontSize, FontStyle.Regular))
                    {
                        // 计算当前行的位置
                        float y = startY + CalculateYPosition(i, g);

                        // 绘制文本 - 修正：使用计算出的y坐标
                        RectangleF textRect = new RectangleF(
                                             ClientRectangle.X,
                                             y,
                                             ClientRectangle.Width,
                                             line.FontSize + LineSpacing // 为每行预留足够的高度
                        );

                        g.DrawString(line.Text, font, new SolidBrush(ForeColor), textRect, sf);
                    }
                }
            }
        }

        private float CalculateYPosition(int lineIndex, Graphics g)
        {
            float currentY = 0;

            // 累加前面所有行的高度和间距
            for (int i = 0; i < lineIndex; i++)
            {
                var previousLine = _textLines[i];
                using (Font tempFont = new Font(Font.FontFamily, previousLine.FontSize, FontStyle.Regular))
                {
                    // 使用实际测量的高度而不是仅用FontSize
                    SizeF textSize = g.MeasureString(previousLine.Text, tempFont);
                    currentY += textSize.Height + LineSpacing;
                }
            }

            return currentY;
        }

        private float CalculateTotalTextHeight(Graphics g)
        {
            float totalHeight = 0;

            foreach (var line in _textLines)
            {
                using (Font font = new Font(Font.FontFamily, line.FontSize, FontStyle.Regular))
                {
                    SizeF textSize = g.MeasureString(line.Text, font);
                    totalHeight += textSize.Height;
                }

                // 添加行间距（最后一行不添加）
                if (line != _textLines.Last())
                {
                    totalHeight += LineSpacing;
                }
            }

            return totalHeight;
        }




        // 辅助方法：快速设置所有行使用相同字体大小
        public void SetAllLinesFontSize(float fontSize)
        {
            foreach (var line in _textLines)
            {
                line.FontSize = fontSize;
            }
            Invalidate();
        }

        // 辅助方法：添加新行
        public void AddTextLine(string text, float fontSize = 9f)
        {
            _textLines.Add(new TextLine { Text = text, FontSize = fontSize });
            Invalidate();
        }

        // 辅助方法：清除所有行
        public void ClearTextLines()
        {
            _textLines.Clear();
            Invalidate();
        }
    }

    // 文本行配置类
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class TextLine
    {
        [Localizable(true)]
        public string Text { get; set; } = string.Empty;

        [DefaultValue(9f)]
        public float FontSize { get; set; } = 9f;

        public TextLine() { }

        public TextLine(string text, float fontSize = 9f)
        {
            Text = text;
            FontSize = fontSize;
        }

        public override string ToString()
        {
            return $"{Text} ({FontSize}pt)";
        }
    }


}