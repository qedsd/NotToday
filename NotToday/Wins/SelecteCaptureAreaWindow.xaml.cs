using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Threading;
using NotToday.Helpers;

namespace NotToday.Wins
{
    public partial class SelecteCaptureAreaWindow : Wpf.Ui.Controls.FluentWindow
    {
        private IntPtr _sourceHWnd = IntPtr.Zero;
        private IntPtr _thumbHWnd = IntPtr.Zero;
        private IntPtr _windowHandle = IntPtr.Zero;

        public Rect CroppedRegion {  get; private set; }

        public SelecteCaptureAreaWindow(IntPtr sourceHWnd)
        {
            _sourceHWnd = sourceHWnd;
            InitializeComponent();
            Loaded += SelecteCaptureAreaWindow_Loaded;
        }

        public void Show(IntPtr sourceHWnd)
        {
            _sourceHWnd = sourceHWnd;
            UpdateScreenshot();
        }

        private void SelecteCaptureAreaWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= SelecteCaptureAreaWindow_Loaded;
            _windowHandle = WindowHelper.GetWindowHandle(this);
            UpdateScreenshot();
        }

        private async void UpdateScreenshot()
        {
            await Task.Delay(500);
            var img = await GetWindowImage();
            await Task.Delay(500);
            ScreenshotImage.Source = Helpers.ImageHelper.ConvertBitmapToWriteableBitmapDirect(Helpers.ImageHelper.ImageToBitmap(img));
        }
        private System.Drawing.Rectangle _windowClientRect = new System.Drawing.Rectangle();
        private System.Drawing.Rectangle _sourceClientRect = new System.Drawing.Rectangle();
        private async Task<System.Drawing.Image> GetWindowImage()
        {
            _thumbHWnd = WindowCaptureHelper.Show(_windowHandle, _sourceHWnd);
            if (_thumbHWnd != IntPtr.Zero)
            {
                try
                {
                    Win32.GetClientRect(_sourceHWnd, ref _sourceClientRect);//源窗口显示区域分辨率大小
                    Win32.SetWindowPos(_windowHandle, IntPtr.Zero, 0, 0, _sourceClientRect.Width, _sourceClientRect.Height, Win32.SWP_NOMOVE | Win32.SWP_NOZORDER | Win32.SWP_NOACTIVATE);
                    Win32.GetClientRect(_windowHandle, ref _windowClientRect);//软件窗口显示区域分辨率大小
                    //软件窗口目标显示区域
                    WindowCaptureHelper.Rect rcD = new WindowCaptureHelper.Rect(_windowClientRect.Left, _windowClientRect.Top, _windowClientRect.Right, _windowClientRect.Bottom);
                    WindowCaptureHelper.UpdateThumbDestination2(_thumbHWnd, rcD);
                    await Task.Delay(100);
                    System.Drawing.Point point = new System.Drawing.Point();
                    Win32.ClientToScreen(_windowHandle, ref point);
                    var img = Helpers.WindowCaptureHelper.GetScreenshot(point.X, point.Y, _windowClientRect.Width, _windowClientRect.Height);
                    WindowCaptureHelper.HideThumb(_thumbHWnd);
                    this.Width = 1000;
                    this.Height = 600;
                    return img;
                    //img.Save("temp.png", ImageFormat.Png);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            return null;
        }

        private void UpdateScreenshot_Click(object sender, RoutedEventArgs e)
        {
            UpdateScreenshot();
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            CroppedRegion = ScreenshotImage.CroppedRegion;
            DialogResult = true;
        }
    }
}
