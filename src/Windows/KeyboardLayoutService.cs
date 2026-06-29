using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using CadIME.Abstractions;

namespace CadIME.Windows
{
    public class KeyboardLayoutService : IKeyboardLayoutService
    {
        private readonly IntPtr _mainCadWindowHandle;

        public KeyboardLayoutService()
        {
            _mainCadWindowHandle = Process.GetCurrentProcess().MainWindowHandle;
        }

        public IntPtr GetCurrent()
        {
            IntPtr activeW = NativeMethods.GetForegroundWindow();
            if (activeW == IntPtr.Zero) activeW = _mainCadWindowHandle;
            uint threadId = NativeMethods.GetWindowThreadProcessId(activeW, out _);
            return NativeMethods.GetKeyboardLayout(threadId);
        }

        public IntPtr GetDefaultChinese()
        {
            IntPtr[] layouts = new IntPtr[32];
            int count = GetKeyboardLayoutList(layouts.Length, layouts);
            for (int i = 0; i < count; i++)
            {
                if (((uint)layouts[i] & 0x0000FFFF) == 0x0804) return layouts[i];
            }
            return IntPtr.Zero;
        }

        public bool SwitchTo(IntPtr keyboardLayoutHandle)
        {
            if (keyboardLayoutHandle == IntPtr.Zero) return false;
            IntPtr targetWindow = NativeMethods.GetForegroundWindow();
            if (targetWindow == IntPtr.Zero) targetWindow = _mainCadWindowHandle;
            
            return NativeMethods.PostMessage(
                targetWindow, 
                NativeMethods.WM_INPUTLANGCHANGEREQUEST, 
                (IntPtr)NativeMethods.KLF_ACTIVATE, 
                keyboardLayoutHandle
            );
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetKeyboardLayoutList(int nBuff, [Out] IntPtr[] lpList);
    }
}
