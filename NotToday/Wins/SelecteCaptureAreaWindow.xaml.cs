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
            UpdateScreenshot();
        }

        private void UpdateScreenshot()
        {
            var img = GetWindowImage();
            ScreenshotImage.Source = Helpers.ImageHelper.ConvertBitmapToWriteableBitmapDirect(Helpers.ImageHelper.ImageToBitmap(img));
        }

        private System.Drawing.Image GetWindowImage()
        {
            return WindowCapture.CaptureWindowClientArea(_sourceHWnd);
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
