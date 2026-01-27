using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
namespace NotToday.Helpers
{
    public class WindowCapture
    {
        // Win32 API声明
        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hDC);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth, int nHeight);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest,
            int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(IntPtr hDC);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        [DllImport("user32.dll")]
        private static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, uint nFlags);

        // 常量定义
        private const int SRCCOPY = 0x00CC0020;
        private const uint PW_CLIENTONLY = 0x00000001;
        private const uint PW_RENDERFULLCONTENT = 0x00000002;

        // 结构体定义
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        /// <summary>
        /// 捕获整个屏幕
        /// </summary>
        public static Image CaptureScreen()
        {
            return CaptureFullWindow(GetDesktopWindow());
        }

        /// <summary>
        /// 捕获指定窗口
        /// </summary>
        /// <param name="handle">窗口句柄</param>
        /// <returns>捕获的图像</returns>
        public static Bitmap CaptureFullWindow(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
                throw new ArgumentException("无效的窗口句柄");

            IntPtr hdcSrc = IntPtr.Zero;
            IntPtr hdcDest = IntPtr.Zero;
            IntPtr hBitmap = IntPtr.Zero;
            IntPtr hOldBitmap = IntPtr.Zero;

            try
            {
                // 获取窗口尺寸
                RECT rect = new RECT();
                GetWindowRect(handle, ref rect);

                int width = rect.Right - rect.Left;
                int height = rect.Bottom - rect.Top;

                if (width <= 0 || height <= 0)
                    throw new InvalidOperationException("无效的窗口尺寸");

                // 获取源设备上下文
                hdcSrc = GetWindowDC(handle);

                // 创建目标设备上下文和位图
                hdcDest = CreateCompatibleDC(hdcSrc);
                hBitmap = CreateCompatibleBitmap(hdcSrc, width, height);
                hOldBitmap = SelectObject(hdcDest, hBitmap);

                PrintWindow(handle, hdcDest, PW_RENDERFULLCONTENT);

                // 将HBITMAP转换为System.Drawing.Image
                return Image.FromHbitmap(hBitmap);
            }
            finally
            {
                // 清理资源
                if (hOldBitmap != IntPtr.Zero)
                    SelectObject(hdcDest, hOldBitmap);

                if (hBitmap != IntPtr.Zero)
                    DeleteObject(hBitmap);

                if (hdcDest != IntPtr.Zero)
                    DeleteDC(hdcDest);

                if (hdcSrc != IntPtr.Zero)
                    ReleaseDC(handle, hdcSrc);
            }
        }

        /// <summary>
        /// 捕获窗口的客户区域（不包括边框和标题栏）
        /// </summary>
        public static Image CaptureWindowClientArea(IntPtr handle)
        {
            RECT rectW = new RECT();
            GetWindowRect(handle, ref rectW);
            int widthW = rectW.Right - rectW.Left;
            int heightW = rectW.Bottom - rectW.Top;
            // 获取客户区域尺寸
            RECT clientRect = new RECT();
            GetClientRect(handle, ref clientRect);

            // 转换客户区域坐标到屏幕坐标
            POINT point = new POINT { X = clientRect.Left, Y = clientRect.Top };
            ClientToScreen(handle, ref point);

            // 将客户区域矩形位置转换为屏幕位置
            int width = clientRect.Right - clientRect.Left;
            int height = clientRect.Bottom - clientRect.Top;
            clientRect.Left = point.X - rectW.Left;
            clientRect.Top = point.Y - rectW.Top;
            clientRect.Right = clientRect.Left + width;
            clientRect.Bottom = clientRect.Top + height;

            var fullWindow = CaptureFullWindow(handle);
            return CropRegion(fullWindow, clientRect);
        }

        /// <summary>
        /// 捕获窗口客户区域指定矩形区域
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="rect">相对于客户区域的位置</param>
        /// <returns></returns>
        public static Image CaptureWindowClientArea(IntPtr handle, RECT rect)
        {
            RECT rectW = new RECT();
            GetWindowRect(handle, ref rectW);
            int widthW = rectW.Right - rectW.Left;
            int heightW = rectW.Bottom - rectW.Top;

            // 获取客户区域尺寸
            RECT clientRect = new RECT();
            GetClientRect(handle, ref clientRect);

            // 转换客户区域坐标到屏幕坐标
            POINT point = new POINT { X = clientRect.Left, Y = clientRect.Top };
            ClientToScreen(handle, ref point);

            //使用屏幕坐标表示客户区域左上角坐标
            int clientRectLeft = point.X - rectW.Left;
            int clientRectTop = point.Y - rectW.Top;

            // 将相对于客户区域的位置转换为屏幕坐标
            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;
            clientRect.Left = clientRectLeft + rect.Left;
            clientRect.Top = clientRectTop + rect.Top;
            clientRect.Right = clientRect.Left + width;
            clientRect.Bottom = clientRect.Top + height;

            var fullWindow = CaptureFullWindow(handle);
            return CropRegion(fullWindow, clientRect);
        }

        /// <summary>
        /// 捕获指定矩形区域
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="rect">屏幕坐标</param>
        /// <returns></returns>
        public static Image CaptureWindow(IntPtr handle, RECT rect)
        {
            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;

            IntPtr hdcSrc = GetWindowDC(handle);

            // 创建目标设备上下文和位图
            IntPtr hdcDest = CreateCompatibleDC(hdcSrc);
            IntPtr hBitmap = CreateCompatibleBitmap(hdcSrc, width, height);
            IntPtr hOldBitmap = SelectObject(hdcDest, hBitmap);

            try
            {
                BitBlt(hdcDest, 0, 0, width, height, hdcSrc,
                       rect.Left, rect.Top, SRCCOPY);

                return Image.FromHbitmap(hBitmap);
            }
            finally
            {
                SelectObject(hdcDest, hOldBitmap);
                DeleteObject(hBitmap);
                DeleteDC(hdcDest);
                ReleaseDC(IntPtr.Zero, hdcSrc);
            }
        }

        private static Bitmap CropRegion(Bitmap source, RECT rect)
        {
            Rectangle region = new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
            // 验证区域
            if (region.Left < 0 || region.Top < 0 ||
                region.Right > source.Width ||
                region.Bottom > source.Height)
            {
                throw new ArgumentException("区域超出窗口范围");
            }

            return source.Clone(region, source.PixelFormat);
        }


        // 辅助API声明
        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hWnd, ref RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }
    }
}
