using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;

namespace AF.Upload.Hive
{
    /// <summary>
    /// 全局 MessageBox Hook - 自动置顶所有 MessageBox
    /// </summary>
    public static class GlobalMessageBoxHook
    {
        #region Windows API

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr SetActiveWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        // 常量定义
        private const int WH_CBT = 5;
        private const int WH_CALLWNDPROCRET = 12;
        private const int HCBT_ACTIVATE = 5;
        private const int WM_DESTROY = 0x0002;
        private const int WM_ACTIVATE = 0x0006;
        private const int WM_WINDOWPOSCHANGING = 0x0046;
        private const int WM_NCDESTROY = 0x0082;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_SHOWWINDOW = 0x0040;
        private const uint SWP_NOACTIVATE = 0x0010;
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const int SW_RESTORE = 9;

        #endregion

        #region Hook 管理

        private static HookProc _hookProc;
        private static HookProc _callWndProcRetHook;
        private static IntPtr _hookId = IntPtr.Zero;
        private static IntPtr _callWndProcRetHookId = IntPtr.Zero;
        private static bool _isHookInstalled = false;

        // 用于跟踪已处理的 MessageBox 窗口
        private static ConcurrentDictionary<IntPtr, bool> _messageBoxWindows = new ConcurrentDictionary<IntPtr, bool>();

        // 用于存储需要恢复 TopMost 状态的窗体
        private static ConcurrentDictionary<IntPtr, FormInfo> _formsToRestore = new ConcurrentDictionary<IntPtr, FormInfo>();

        // 跟踪当前活动的 MessageBox 数量
        private static int _activeMessageBoxCount = 0;

        /// <summary>
        /// 安装 MessageBox Hook（当前线程）
        /// </summary>
        public static void Install()
        {
            if (_isHookInstalled) return;

            try
            {
                _hookProc = new HookProc(MessageBoxHookProc);
                _callWndProcRetHook = new HookProc(CallWndProcRetHook);

                // 使用线程钩子而不是全局钩子
                uint currentThreadId = GetCurrentThreadId();
                _hookId = SetWindowsHookEx(WH_CBT, _hookProc, IntPtr.Zero, currentThreadId);
                _callWndProcRetHookId = SetWindowsHookEx(WH_CALLWNDPROCRET, _callWndProcRetHook, IntPtr.Zero, currentThreadId);

                if (_hookId != IntPtr.Zero && _callWndProcRetHookId != IntPtr.Zero)
                {
                    _isHookInstalled = true;
                    Debug.WriteLine("MessageBox Hook 安装成功，线程ID: " + currentThreadId);
                }
                else
                {
                    int error = Marshal.GetLastWin32Error();
                    Debug.WriteLine($"MessageBox Hook 安装失败，错误代码: {error}");

                    // 清理失败的钩子
                    if (_hookId != IntPtr.Zero)
                    {
                        UnhookWindowsHookEx(_hookId);
                        _hookId = IntPtr.Zero;
                    }
                    if (_callWndProcRetHookId != IntPtr.Zero)
                    {
                        UnhookWindowsHookEx(_callWndProcRetHookId);
                        _callWndProcRetHookId = IntPtr.Zero;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"安装 MessageBox Hook 异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 卸载 MessageBox Hook
        /// </summary>
        public static void Uninstall()
        {
            if (!_isHookInstalled) return;

            try
            {
                if (_hookId != IntPtr.Zero)
                {
                    UnhookWindowsHookEx(_hookId);
                    _hookId = IntPtr.Zero;
                }
                if (_callWndProcRetHookId != IntPtr.Zero)
                {
                    UnhookWindowsHookEx(_callWndProcRetHookId);
                    _callWndProcRetHookId = IntPtr.Zero;
                }
                _isHookInstalled = false;

                // 恢复所有窗体的 TopMost 状态
                //RestoreAllFormsTopMost();

                // 清空跟踪的窗口
                _messageBoxWindows.Clear();
                _formsToRestore.Clear();
                _activeMessageBoxCount = 0;

                Debug.WriteLine("MessageBox Hook 已卸载");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"卸载 MessageBox Hook 异常: {ex.Message}");
            }
        }

        private static IntPtr MessageBoxHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode == HCBT_ACTIVATE)
            {
                try
                {
                    IntPtr hwnd = wParam;

                    // 检查窗口类名是否为 MessageBox
                    if (IsMessageBoxWindow(hwnd))
                    {
                        // 如果是第一个 MessageBox，临时取消其他 TopMost 窗体的置顶
                        if (_activeMessageBoxCount == 0)
                        {
                            //TemporarilyDisableOtherTopMostForms();
                        }

                        // 增加活动 MessageBox 计数
                        _activeMessageBoxCount++;

                        // 将 MessageBox 设置为永久置顶
                        SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0,
                                    SWP_NOSIZE | SWP_NOMOVE | SWP_SHOWWINDOW);

                        // 记录已处理的 MessageBox 窗口
                        _messageBoxWindows.TryAdd(hwnd, true);

                        Debug.WriteLine($"检测到 MessageBox 并已置顶: {hwnd}, 活动 MessageBox 计数: {_activeMessageBoxCount}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"处理 MessageBox Hook 异常: {ex.Message}");
                }
            }

            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        private static IntPtr CallWndProcRetHook(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                try
                {
                    // 获取消息信息
                    CWPRETSTRUCT msg = (CWPRETSTRUCT)Marshal.PtrToStructure(lParam, typeof(CWPRETSTRUCT));

                    // 检查是否是激活消息或者窗口位置变化消息
                    if (msg.message == WM_ACTIVATE || msg.message == WM_WINDOWPOSCHANGING)
                    {
                        IntPtr hwnd = msg.hwnd;

                        // 如果这是一个我们跟踪的 MessageBox 窗口
                        if (_messageBoxWindows.ContainsKey(hwnd) && IsWindowVisible(hwnd))
                        {
                            // 确保它仍然置顶
                            SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0,
                                        SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);

                            // 如果窗口不在前台，将其带到前台
                            if (GetForegroundWindow() != hwnd)
                            {
                                BringWindowToTop(hwnd);
                                SetActiveWindow(hwnd);
                            }
                        }
                    }
                    // 检查是否是窗口销毁消息
                    else if (msg.message == WM_DESTROY || msg.message == WM_NCDESTROY)
                    {
                        IntPtr hwnd = msg.hwnd;

                        // 如果这是一个我们跟踪的 MessageBox 窗口被销毁
                        if (_messageBoxWindows.ContainsKey(hwnd))
                        {
                            // 移除跟踪
                            _messageBoxWindows.TryRemove(hwnd, out _);

                            // 减少活动 MessageBox 计数
                            _activeMessageBoxCount--;

                            // 如果这是最后一个 MessageBox，恢复其他窗体的 TopMost 状态
                            if (_activeMessageBoxCount == 0)
                            {
                                //RestoreAllFormsTopMost();
                            }

                            Debug.WriteLine($"MessageBox 已关闭: {hwnd}, 活动 MessageBox 计数: {_activeMessageBoxCount}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"处理 CallWndProcRet Hook 异常: {ex.Message}");
                }
            }

            return CallNextHookEx(_callWndProcRetHookId, nCode, wParam, lParam);
        }

        /// <summary>
        /// 临时取消其他 TopMost 窗体的置顶
        /// </summary>
        private static void TemporarilyDisableOtherTopMostForms()
        {
            try
            {
                // 使用锁确保线程安全
                lock (_formsToRestore)
                {
                    // 清空之前的记录
                    _formsToRestore.Clear();

                    // 获取所有可见的 TopMost 窗体
                    var topMostForms = Application.OpenForms.Cast<Form>()
                                             .Where(f => f.TopMost && f.Visible && f.WindowState != FormWindowState.Minimized)
                                             .ToList();

                    foreach (var form in topMostForms)
                    {
                        try
                        {
                            // 记录需要恢复的窗体信息
                            var formInfo = new FormInfo
                            {
                                Form = form,
                                Handle = form.Handle,
                                Text = form.Text
                            };

                            // 记录窗体
                            _formsToRestore.TryAdd(form.Handle, formInfo);

                            // 临时取消 TopMost
                            form.TopMost = false;

                            Debug.WriteLine($"临时取消窗体 TopMost: {form.Text} ({form.Handle})");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"取消窗体 TopMost 失败: {form?.Text}, 错误: {ex.Message}");
                        }
                    }

                    Debug.WriteLine($"临时取消了 {_formsToRestore.Count} 个窗体的 TopMost 状态");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"临时取消 TopMost 窗体异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 恢复所有窗体的 TopMost 状态
        /// </summary>
        private static void RestoreAllFormsTopMost()
        {
            try
            {
                // 使用锁确保线程安全
                lock (_formsToRestore)
                {
                    Debug.WriteLine($"准备恢复 {_formsToRestore.Count} 个窗体的 TopMost 状态");

                    // 创建副本以避免在枚举时修改集合
                    var formsToRestoreCopy = _formsToRestore.Values.ToList();

                    foreach (var formInfo in formsToRestoreCopy)
                    {
                        try
                        {
                            // 检查窗体是否仍然存在且可见
                            if (formInfo.Form != null && !formInfo.Form.IsDisposed &&
                                formInfo.Form.Visible && IsWindow(formInfo.Handle))
                            {
                                formInfo.Form.TopMost = true;
                                Debug.WriteLine($"恢复窗体 TopMost: {formInfo.Text} ({formInfo.Handle})");
                            }
                            else
                            {
                                Debug.WriteLine($"跳过已关闭或无效的窗体: {formInfo.Text} ({formInfo.Handle})");
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"恢复窗体 TopMost 失败: {formInfo?.Text}, 错误: {ex.Message}");
                        }
                    }

                    _formsToRestore.Clear();
                    Debug.WriteLine("已恢复所有窗体的 TopMost 状态");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"恢复 TopMost 窗体异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 判断窗口是否为 MessageBox
        /// </summary>
        private static bool IsMessageBoxWindow(IntPtr hWnd)
        {
            try
            {
                StringBuilder className = new StringBuilder(256);
                if (GetClassName(hWnd, className, className.Capacity) == 0)
                    return false;

                string classNameStr = className.ToString();

                // MessageBox 的类名是 "#32770"
                if (classNameStr == "#32770")
                {
                    // 进一步验证：检查窗口是否属于当前进程
                    GetWindowThreadProcessId(hWnd, out uint processId);
                    uint currentProcessId = (uint)Process.GetCurrentProcess().Id;

                    return processId == currentProcessId;
                }
            }
            catch
            {
                // 忽略异常
            }

            return false;
        }

        #endregion

        #region 结构体定义

        [StructLayout(LayoutKind.Sequential)]
        private struct CWPRETSTRUCT
        {
            public IntPtr lResult;
            public IntPtr lParam;
            public IntPtr wParam;
            public uint message;
            public IntPtr hwnd;
        }

        // 窗体信息类
        private class FormInfo
        {
            public Form Form { get; set; }
            public IntPtr Handle { get; set; }
            public string Text { get; set; }
        }

        #endregion
    }
}