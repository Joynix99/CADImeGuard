using System;
using System.Runtime.InteropServices;
using System.Text;

namespace CadIME.Windows
{
    internal static class NativeMethods
    {
        private const string User32 = "user32.dll";

        public const uint WM_INPUTLANGCHANGEREQUEST = 0x0050;
        public const uint KLF_ACTIVATE = 0x00000001;
        public const uint EVENT_OBJECT_FOCUS = 0x8005; 
        public const uint WINEVENT_OUTOFCONTEXT = 0x0000; 

        [DllImport(User32, SetLastError = true)]
        public static extern IntPtr GetForegroundWindow();

        [DllImport(User32, SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport(User32, SetLastError = true)]
        public static extern IntPtr GetKeyboardLayout(uint idThread);

        [DllImport(User32, SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport(User32, SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport(User32, SetLastError = true)]
        public static extern IntPtr GetFocus();

        public delegate void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        [DllImport(User32, SetLastError = true)]
        public static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventProc lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        [DllImport(User32, SetLastError = true)]
        public static extern bool UnhookWinEvent(IntPtr hWinEventHook);
    }
}
