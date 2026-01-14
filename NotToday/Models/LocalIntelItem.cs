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
    public class LocalIntelItem
    {
        private System.Windows.Media.MediaPlayer _mediaPlayer;
        private LocalIntelItemConfig _config;
        private object _locker = new object();
        private bool _running = false;
        public LocalIntelItemConfig Config { get => _config; }
        public string GUID { get; private set; } = Guid.NewGuid().ToString();
        public LocalIntelItem(LocalIntelItemConfig config)
        {
            _config = config;
        }

        public async Task<bool> Start()
        {
            if(await Services.LocalIntelScreenshotService.Current.Add(this))
            {
                lock (_locker)
                {
                    _running = true;
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
                return true;
            }
            else
            {
                return false;
            }
        }
        public void Stop()
        {
            lock (_locker)
            {
                _running = false;
            }
            Services.LocalIntelScreenshotService.Current.Remve(this);
            _mediaPlayer?.Stop();
        }


        private void MediaPlayer_MediaEnded(object sender, EventArgs e)
        {
            _mediaPlayer.Position = TimeSpan.Zero;
            _mediaPlayer.Play();
        }

        /// <summary>
        /// 每个颜色对应的匹配像素数量
        /// </summary>
        private long[] _lastSums;
        private void Analyse(System.Drawing.Bitmap img)
        {
            _lastSums ??= new long[_config.Colors.Count];
            var curSums = FindCurSums(img);
            List<int> actingStandingIndex = new List<int>();
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < _lastSums.Length; i++)
            {
                long diff = curSums[i] - _lastSums[i];
                if (Math.Abs(diff) >= _config.MinMatchPixel)
                {
                    stringBuilder.Append($"[{_config.ConfigName}] ");
                    if (diff > 0)
                    {
                        stringBuilder.Append(_config.Colors[i].Name);
                        stringBuilder.Append("++");
                        stringBuilder.Append($"({diff})");
                        stringBuilder.Append("  ");
                        actingStandingIndex.Add(i);
                    }
                    else if (_config.NotifyDecrease)
                    {
                        stringBuilder.Append(_config.Colors[i].Name);
                        stringBuilder.Append("--");
                        stringBuilder.Append($"({diff})");
                        stringBuilder.Append("  ");
                        actingStandingIndex.Add(i);
                    }
                }
            }
            _lastSums = curSums;
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
                            var setting = _config.Colors[actingStandingIndex[0]];
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
        }
        private long[] FindCurSums(System.Drawing.Bitmap img)
        {
            long[] curSums = new long[_config.Colors.Count];
            var sourceMat = IntelImageHelper.BitmapToMat(img);
            for (int i = 0; i < _config.Colors.Count; i++)
            {
                var setting = _config.Colors[i];
                var points = IntelImageHelper.GetMatchPoint(sourceMat, setting.Color.R, setting.Color.G, setting.Color.B, _config.ColorMatchThreshold);
                long sum = 0;//使用每个点xy总和来记录，可能存在小概率误报
                sum += points.Count;
                curSums[i] = sum;
            }
            sourceMat.Dispose();
            return curSums;
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
