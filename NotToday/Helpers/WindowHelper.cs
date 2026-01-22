using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NotToday.Helpers
{
    public static class WindowHelper
    {
        public static IntPtr GetWindowHandle(Window window)
        {
            return (new System.Windows.Interop.WindowInteropHelper(window)).Handle;
        }
        /// <summary>
        /// 边框宽度
        /// </summary>
        /// <param name="hwnd"></param>
        /// <returns></returns>
        public static int GetBorderWidth(IntPtr hwnd)
        {
            var windowRect = new System.Drawing.Rectangle();
            Win32.GetWindowRect(hwnd, ref windowRect);
            System.Drawing.Point point = new System.Drawing.Point();
            Win32.ClientToScreen(hwnd, ref point);
            return point.X - windowRect.Left;
        }
        public static void SetForegroundWindow(IntPtr targetHandle)
        {
            Win32.keybd_event(0, 0, 0, 0);
            if (Win32.IsIconic(targetHandle))
            {
                Win32.ShowWindow(targetHandle, 4);
            }
            else
            {
                Win32.ShowWindow(targetHandle, 5);
            }
            Win32.SetForegroundWindow(targetHandle);
            Win32.keybd_event(0, 0, 0x0002, 0);
        }
    }
}
