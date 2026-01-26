using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

namespace NotToday.Models
{
    public class LocalIntelItemConfig : ObservableObject
    {
        [JsonIgnore]
        public IntPtr HWnd { get; set; }

        [JsonIgnore]
        public string ProcessGUID {  get; set; }

        private string _windowTitle;
        /// <summary>
        /// 窗口名称
        /// EVE-角色名称
        /// </summary>
        public string WindowTitle
        {
            get => _windowTitle; set => SetProperty(ref _windowTitle, value);
        }

        private string _configName;
        /// <summary>
        /// 配置名称
        /// 一个窗口名称可对应多个配置
        /// </summary>
        public string ConfigName
        {
            get => _configName; set => SetProperty(ref _configName, value);
        }

        private LocalIntelRect _intelRect = new LocalIntelRect();
        public LocalIntelRect IntelRect
        {
            get => _intelRect; set => SetProperty(ref _intelRect, value);
        }

        private bool windowNotify = true;
        public bool WindowNotify
        {
            get => windowNotify;
            set => SetProperty(ref windowNotify, value);
        }

        private bool toastNotify = false;
        public bool ToastNotify
        {
            get => toastNotify;
            set => SetProperty(ref toastNotify, value);
        }

        private bool soundNotify = true;
        public bool SoundNotify
        {
            get => soundNotify;
            set => SetProperty(ref soundNotify, value);
        }

        private double colorMatchThreshold = 0.15;
        /// <summary>
        /// 声望RGB识别阈值百分比
        /// </summary>
        public double ColorMatchThreshold
        {
            get => colorMatchThreshold;
            set => SetProperty(ref colorMatchThreshold, value);
        }

        private int minMatchPixel = 10;
        /// <summary>
        /// 最小匹配像素个数
        /// </summary>
        public int MinMatchPixel
        {
            get => minMatchPixel;
            set => SetProperty(ref minMatchPixel, value);
        }

        private bool notifyDecrease = true;
        /// <summary>
        /// 是否提示减少的情况
        /// </summary>
        public bool NotifyDecrease
        {
            get => notifyDecrease;
            set => SetProperty(ref notifyDecrease, value);
        }

        private LocalInteColorDecreaseMode decreaseMode;
        public LocalInteColorDecreaseMode DecreaseMode
        {
            get => decreaseMode;
            set => SetProperty(ref decreaseMode, value);
        }

        private bool loop = true;
        public bool Loop
        {
            get => loop;
            set => SetProperty(ref loop, value);
        }

        private int volume = 100;
        public int Volume
        {
            get => volume;
            set => SetProperty(ref volume, value);
        }

        private int delay = 0;
        public int Delay
        {
            get => delay;
            set => SetProperty(ref delay, value);
        }
        public ObservableCollection<LocalIntelColor> Colors { get; set; } = new ObservableCollection<LocalIntelColor>();
    }
    public class LocalIntelColor : ObservableObject
    {
        private System.Windows.Media.Color color = System.Windows.Media.Colors.Red;
        public System.Windows.Media.Color Color
        {
            get => color;
            set 
            {
                if(SetProperty(ref color, value))
                {
                    ColorBrush = new SolidColorBrush(value);
                }
            } 
        }

        private SolidColorBrush _colorBrush;
        [JsonIgnore]
        public SolidColorBrush ColorBrush
        {
            get => _colorBrush;
            set => SetProperty(ref _colorBrush, value);
        }

        private string name;
        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        private string soundFile;
        public string SoundFile
        {
            get => soundFile;
            set => SetProperty(ref soundFile, value);
        }
    }

    public class LocalIntelRect : ObservableObject
    {
        private int x = 0;
        public int X
        {
            get => x; set => SetProperty(ref x, value);
        }
        private int y = 0;
        public int Y
        {
            get => y; set => SetProperty(ref y, value);
        }
        private int width = 0;
        public int Width
        {
            get => width; set => SetProperty(ref width, value);
        }
        private int height = 0;
        public int Height
        {
            get => height; set => SetProperty(ref height, value);
        }

        public bool IsValid()
        {
            return width > 0 && height > 0;
        }
    }

    public enum LocalInteColorDecreaseMode
    {
        None,
        Notify,
        StopNotify
    }
}
