using System;
using System.Diagnostics;
using System.Text;
using CadIME.Abstractions;

namespace CadIME.Windows
{
    public class WindowService : IWindowService
    {
        private readonly uint _currentPid;

        public WindowService()
        {
            _currentPid = (uint)Process.GetCurrentProcess().Id;
        }

        public bool IsAutoCADForeground()
        {
            IntPtr fgWin = NativeMethods.GetForegroundWindow();
            if (fgWin == IntPtr.Zero) return false;
            NativeMethods.GetWindowThreadProcessId(fgWin, out uint pid);
            return pid == _currentPid;
        }

        public bool IsTextEditorActive()
        {
            IntPtr focusHwnd = NativeMethods.GetFocus();
            if (focusHwnd == IntPtr.Zero) focusHwnd = NativeMethods.GetForegroundWindow();
            if (focusHwnd == IntPtr.Zero) return false;

            StringBuilder sb = new StringBuilder(256);
            if (NativeMethods.GetClassName(focusHwnd, sb, sb.Capacity) == 0) return false;
            string className = sb.ToString();

            // 【深度消杀】：排除 F2 历史控制台（TextConsole）和一切单纯的联想工具提示框
            if (className.Contains("TextConsole") || className.Contains("ToolTip")) return false;

            // 只有当窗口类名属于正牌的文本编辑器或表格多行输入控件（WPF 文本容器代理）时才返回 true
            return className.Contains("Editor") || className.Contains("HwndWrapper");
        }

        public bool IsModalDialogActive()
        {
            IntPtr fgWin = NativeMethods.GetForegroundWindow();
            if (fgWin == IntPtr.Zero) return false;

            NativeMethods.GetWindowThreadProcessId(fgWin, out uint pid);
            if (pid != _currentPid) return false;

            StringBuilder sb = new StringBuilder(256);
            if (NativeMethods.GetClassName(fgWin, sb, sb.Capacity) == 0) return false;
            
            // #32770 是标准 Windows 模态对话框（保存、查找替换等）的官方硬编码类名
            return sb.ToString() == "#32770";
        }
    }
}
