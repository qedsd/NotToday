using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using NotToday.Helpers;
using NotToday.Services;

namespace NotToday.Models
{
    public class LocalIntelItem : ObservableObject
    {
        private System.Windows.Media.MediaPlayer _mediaPlayer;
        private LocalIntelItemConfig _config;
        private object _locker = new object();
        private bool _running = false;
        private System.Timers.Timer _timer;
        public bool Running
        {
            get => _running;
            set => SetProperty(ref _running, value);
        }
        public LocalIntelItemConfig Config { get => _config; }
        public string GUID { get; private set; } = Guid.NewGuid().ToString();

        private int decreaseMode;
        public int DecreaseMode
        {
            get => decreaseMode;
            set
            {
                if(SetProperty(ref decreaseMode, value))
                {
                    Config.DecreaseMode = (LocalInteColorDecreaseMode)value;
                }
            }
        }

        private List<ColorMatch> _colorMatches;
        /// <summary>
        /// 每个颜色对应的匹配像素数量记录
        /// </summary>
        public List<ColorMatch> ColorMatches
        {
            get => _colorMatches;
            set
            {
                SetProperty(ref _colorMatches, value);
            }
        }

        public LocalIntelItem(LocalIntelItemConfig config)
        {
            _config = config;
            DecreaseMode = (int)config.DecreaseMode;
        }
        private nint _sourceHwnd;
        private WindowCapture.RECT _targetRect;

        public bool Start()
        {
            if (!Config.IntelRect.IsValid())
            {
                MessageBox.Show($"{Config.WindowTitle} {Config.ConfigName}: 请设置监控区域");
                return false;
            }
            if (Config.Colors.Count == 0)
            {
                MessageBox.Show($"{Config.WindowTitle} {Config.ConfigName}: 请设置监控颜色");
                return false;
            }
            try
            {
                _sourceHwnd = _config.HWnd;
                _targetRect = new WindowCapture.RECT()
                {
                    Left = _config.IntelRect.X,
                    Right = _config.IntelRect.X + _config.IntelRect.Width,
                    Top = _config.IntelRect.Y,
                    Bottom = _config.IntelRect.Y + _config.IntelRect.Height,
                };
                if (_timer == null)
                {
                    _timer = new System.Timers.Timer()
                    {
                        AutoReset = false,
                    };
                    _timer.Elapsed += Timer_Elapsed;
                }
                _timer.Interval = _config.RefreshSpan;
                ColorMatches = _config.Colors.Select(p=>new ColorMatch(p)).ToList();
                lock (_locker)
                {
                    Running = true;
                }
                if (_config.SoundNotify)
                {
                    _mediaPlayer ??= new System.Windows.Media.MediaPlayer();
                    _mediaPlayer.Volume = _config.Volume / 100.0;
                    if (_config.Loop)
                    {
                        _mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
                    }
                    else
                    {
                        _mediaPlayer.MediaEnded -= MediaPlayer_MediaEnded;
                    }
                }
                LocalIntelNotifyService.Current.OnHideNotifyWindow += OnHideNotifyWindow;
                _timer.Start();
                return true;
            }
            catch (Exception ex)
            {
                _timer?.Stop();
                MessageBox.Show($"{Config.WindowTitle} {Config.ConfigName}: {ex.ToString()}");
                return false;
            }
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                var img = WindowCapture.CaptureWindowClientAreaRectangle(_sourceHwnd, _targetRect);
                ChangeScreenshot(Helpers.ImageHelper.ImageToBitmap(img));
                img.Dispose();
                _timer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void OnHideNotifyWindow(object sender, EventArgs e)
        {
            _mediaPlayer?.Stop();
        }

        public void Stop()
        {
            lock (_locker)
            {
                Running = false;
            }
            _timer?.Stop();
            _mediaPlayer?.Stop();
        }


        private void MediaPlayer_MediaEnded(object sender, EventArgs e)
        {
            _mediaPlayer.Position = TimeSpan.Zero;
            _mediaPlayer.Play();
        }

        private void Analyse(System.Drawing.Bitmap img)
        {
            FindCurSums(img);
            List<ColorMatch> actingColorMatch = new List<ColorMatch>();
            StringBuilder stringBuilder = new StringBuilder();
            bool isDecreas = false;
            foreach(var colorMatch in _colorMatches)
            {
                long diff = colorMatch.Diff;
                if (Math.Abs(diff) >= _config.MinMatchPixel)
                {
                    if (diff > 0)
                    {
                        stringBuilder.Append($"[{_config.ConfigName}] ");
                        isDecreas = false;
                        stringBuilder.Append(colorMatch.Color.Name);
                        stringBuilder.Append("++");
                        stringBuilder.Append($"({diff})");
                        stringBuilder.Append("  ");
                        actingColorMatch.Add(colorMatch);
                    }
                    else
                    {
                        isDecreas = isDecreas || true;
                        if (_config.DecreaseMode == LocalInteColorDecreaseMode.Notify)
                        {
                            stringBuilder.Append($"[{_config.ConfigName}] ");
                            stringBuilder.Append(colorMatch.Color.Name);
                            stringBuilder.Append("--");
                            stringBuilder.Append($"({diff})");
                            stringBuilder.Append("  ");
                            actingColorMatch.Add(colorMatch);
                        }
                    }
                }
            }
            if (stringBuilder.Length != 0)
            {
                if (_config.WindowNotify)
                {
                    LocalIntelNotifyService.Current.NotifyByWindow(new LocalIntelNotify(_config.HWnd, _config.WindowTitle, stringBuilder.ToString(), string.Empty));
                }
                if (_config.SoundNotify)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            //使用第一个声望对应的音频文件
                            var setting = actingColorMatch[0].Color;
                            string filePath = string.IsNullOrEmpty(setting.SoundFile) ? System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Resources", "default.mp3") : setting.SoundFile;
                            _mediaPlayer.Stop();
                            _mediaPlayer.Open(new Uri(filePath, UriKind.RelativeOrAbsolute));
                            _mediaPlayer.Play();
                        }
                        catch (Exception ex)
                        {
                            System.Windows.MessageBox.Show($"无法播放文件: {ex.Message}");
                        }
                    });
                }
            }
            else if(isDecreas && _config.DecreaseMode == LocalInteColorDecreaseMode.StopNotify && _config.SoundNotify)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _mediaPlayer?.Stop();
                });
            }
        }
        private void FindCurSums(System.Drawing.Bitmap img)
        {
            var sourceMat = IntelImageHelper.BitmapToMat(img);
            foreach (var colorMatch in _colorMatches)
            {
                var setting = colorMatch.Color;
                //使用每个点xy总和来记录，可能存在小概率误报
                var points = IntelImageHelper.GetMatchPoint(sourceMat, setting.Color.R, setting.Color.G, setting.Color.B, _config.ColorMatchThreshold);
                colorMatch.SetNewSum(points.Count);
            }
            sourceMat.Dispose();
        }
        public void ChangeScreenshot(Bitmap img)
        {
            bool running = false;
            lock (_locker)
            {
                running = _running;
            }
            if (running)
            {
                Analyse(img);
                OnScreenshotChanged?.Invoke(this, img);
                img.Dispose();
            }
        }

        public delegate void ScreenshotChangedDelegate(LocalIntelItem sender, Bitmap img);
        public event ScreenshotChangedDelegate OnScreenshotChanged;
    }
}
