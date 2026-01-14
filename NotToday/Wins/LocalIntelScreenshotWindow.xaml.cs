using System;
using System.Collections.Generic;
using System.Drawing;
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
using System.Windows.Shapes;
using NotToday.Helpers;
using NotToday.Models;

namespace NotToday.Wins
{
    public partial class LocalIntelScreenshotWindow : Window
    {
        /// <summary>
        /// 每个略缩图中间的间隔
        /// </summary>
        private static readonly int SPAN = 1;
        class LocalIntelWindowItem
        {
            public LocalIntelItem LocalIntelItem { get; set; }
            /// <summary>
            /// 当前略缩图在预警窗口中的显示位置
            /// </summary>
            public WindowCaptureHelper.Rect ThumbRect { get; set; }
            /// <summary>
            /// 注册显示返回句柄
            /// </summary>
            public IntPtr ThumbHWnd { get; set; }
        }
        private readonly Dictionary<string, LocalIntelWindowItem> _intelDics = new Dictionary<string, LocalIntelWindowItem>();
        private int _refreshSpan = 100;
        private IntPtr _windowHandle = IntPtr.Zero;
        public LocalIntelScreenshotWindow()
        {
            InitializeComponent();
            Loaded += LocalIntelScreenshotWindow_Loaded;
        }

        private void LocalIntelScreenshotWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= LocalIntelScreenshotWindow_Loaded;
            _windowHandle = WindowHelper.GetWindowHandle(this);
        }
        public bool Add(LocalIntelItem localIntelItem)
        {
            if (!_intelDics.ContainsKey(localIntelItem.GUID))
            {
                try
                {
                    Stop();
                    int fw = 0;//当前窗口宽度
                    if (_intelDics.Count != 0)
                    {
                        fw = _intelDics.LastOrDefault().Value.ThumbRect.Right + SPAN;
                    }
                    var thumbHWdn = WindowCaptureHelper.RegisterThumbnail(_windowHandle,localIntelItem.Config.HWnd);
                    if (thumbHWdn != IntPtr.Zero)
                    {
                        LocalIntelWindowItem newItem = new LocalIntelWindowItem()
                        {
                            LocalIntelItem = localIntelItem,
                            ThumbRect = new WindowCaptureHelper.Rect(fw, 0, fw + localIntelItem.Config.IntelRect.Width, localIntelItem.Config.IntelRect.Height),
                            ThumbHWnd = thumbHWdn
                        };
                        int widthMargin = WindowHelper.GetBorderWidth(localIntelItem.Config.HWnd);//去掉左边白边及右边显示完整
                        _intelDics.Add(localIntelItem.GUID, newItem);
                        Win32.SetWindowPos(_windowHandle, IntPtr.Zero, 0, 0, newItem.ThumbRect.Right, _intelDics.Max(p => p.Value.ThumbRect.Bottom), Win32.SWP_NOMOVE | Win32.SWP_NOZORDER | Win32.SWP_NOACTIVATE);
                        WindowCaptureHelper.UpdateThumbDestination(thumbHWdn, newItem.ThumbRect, new WindowCaptureHelper.Rect(localIntelItem.Config.IntelRect.X + widthMargin, localIntelItem.Config.IntelRect.Y, localIntelItem.Config.IntelRect.X + localIntelItem.Config.IntelRect.Width + widthMargin, localIntelItem.Config.IntelRect.Y + localIntelItem.Config.IntelRect.Height));
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("注册Thumbnail失败");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return false;
                }
                finally
                {
                    if(_intelDics.Count > 0)
                    {
                        Start();
                    }
                }
            }
            else
            {
                return false;
            }
        }

        public bool Remove(LocalIntelItem localIntelItem)
        {
            if (_intelDics.TryGetValue(localIntelItem?.GUID, out LocalIntelWindowItem newItem))
            {
                Stop();
                WindowCaptureHelper.HideThumb(newItem.ThumbHWnd);
                _intelDics.Remove(localIntelItem.GUID);
                if (_intelDics.Count > 0)
                {
                    int w = 0;
                    //重新调整剩余的略缩图
                    var items = _intelDics.Values.ToArray();
                    for (int i = 0; i < items.Length; i++)
                    {
                        var item = items[i];
                        item.ThumbRect = new WindowCaptureHelper.Rect()
                        {
                            Left = w,
                            Right = w + item.LocalIntelItem.Config.IntelRect.Width,
                            Top = item.ThumbRect.Top,
                            Bottom = item.ThumbRect.Bottom,
                        };
                        w = item.ThumbRect.Right + SPAN;
                        int widthMargin = WindowHelper.GetBorderWidth(item.LocalIntelItem.Config.HWnd);//去掉左边白边及右边显示完整
                        WindowCaptureHelper.UpdateThumbDestination(item.ThumbHWnd, item.ThumbRect, new WindowCaptureHelper.Rect(item.LocalIntelItem.Config.IntelRect.X + widthMargin, item.LocalIntelItem.Config.IntelRect.Y, item.LocalIntelItem.Config.IntelRect.X + item.LocalIntelItem.Config.IntelRect.Width + widthMargin, item.LocalIntelItem.Config.IntelRect.Y + item.LocalIntelItem.Config.IntelRect.Height));
                    }
                    Win32.SetWindowPos(_windowHandle, IntPtr.Zero, 0, 0, w - SPAN, _intelDics.Max(p => p.Value.ThumbRect.Bottom), Win32.SWP_NOMOVE | Win32.SWP_NOZORDER | Win32.SWP_NOACTIVATE);
                    Start();
                }
                else
                {
                    //没有监控则关闭窗口
                    Close();
                }
                return true;
            }
            else
            {
                MessageBox.Show($"尝试移除不存在的进程{localIntelItem?.Config.ConfigName}");
                return false;
            }
        }
        #region 定期截图
        private object Locker = new object();
        private bool _stopScreenshot = true;
        public void Start()
        {
            lock (Locker)
            {
                if (!_stopScreenshot)
                {
                    return;
                }
                _stopScreenshot = false;
            }
            Task.Run(() =>
            {
                while (true)
                {
                    lock (Locker)
                    {
                        if (_stopScreenshot)
                        {
                            return;
                        }
                    }

                    try
                    {
                        //System.Drawing.Point point = new System.Drawing.Point();
                        //Win32.ClientToScreen(_windowHandle, ref point);
                        System.Drawing.Rectangle windowRect = new System.Drawing.Rectangle();
                        Win32.GetClientRect(_windowHandle, ref windowRect);
                        //截取整个预警窗口图
                        var img = Helpers.WindowCaptureHelper.GetScreenshot(windowRect.X, windowRect.Y, windowRect.Width, windowRect.Height);
                        if (img != null)
                        {
                            //分割图像
                            foreach (var item in _intelDics.Values)
                            {
                                var cutBitmap = ImageHelper.ImageToBitmap(img, new System.Drawing.Rectangle(item.ThumbRect.Left, item.ThumbRect.Top, item.ThumbRect.Right - item.ThumbRect.Left, item.ThumbRect.Bottom - item.ThumbRect.Top));
                                item.LocalIntelItem.ChangeScreenshot(cutBitmap.Clone() as Bitmap);
                                cutBitmap.Dispose();
                            }
                            img.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    finally
                    {
                        Thread.Sleep(_refreshSpan);
                    }
                }
            });
        }
        public void Stop()
        {
            lock (Locker)
            {
                _stopScreenshot = true;
            }
        }
        #endregion

        private void FluentWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void FluentWindow_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            
        }
    }
}
