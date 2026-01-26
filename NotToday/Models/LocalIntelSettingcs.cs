using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

namespace NotToday.Models
{
    public class LocalIntelSetting : ObservableObject
    {
        private string processName = "exefile";
        public string ProcessName
        {
            get => processName; set => SetProperty(ref processName, value);
        }
        
        public List<LocalIntelItemConfig> ItemConfigs { get; set; } = new List<LocalIntelItemConfig>();
    }

    public class LocalIntelNotify
    {
        public LocalIntelNotify(IntPtr hwnd, string name, string changedMsg, string remainMsg)
        {
            HWnd = hwnd;
            Name = name;
            ChangedMsg = changedMsg;
            RemainMsg = remainMsg;
            Time = DateTime.Now;
        }
        public IntPtr HWnd { get; set; }
        public string Name { get; set; }
        public string ChangedMsg { get; set; }
        public string RemainMsg { get; set; }
        public DateTime Time { get; set; }
    }
}
