using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

namespace NotToday.Models
{
    public class ProcessInfo : ObservableObject
    {
        public System.Diagnostics.Process Process { get; set; }
        public IntPtr MainWindowHandle { get; set; }
        public string WindowTitle { get; set; }
        public string ProcessName { get; set; }
        private bool running;
        public bool Running
        {
            get => running; set => SetProperty(ref running, value);
        }
        public string GUID
        {
            get => _guid;
        }
        public string GetCharacterName()
        {
            if (!string.IsNullOrEmpty(WindowTitle))
            {
                var array = WindowTitle.Split('-');
                if (array.Length == 2)
                {
                    string name = array[1].Trim();
                    return name == "{[character]player.name}" ? null : name;
                }
            }
            return null;
        }

        private string _guid = Guid.NewGuid().ToString();

        private string settingName;
        public string SettingName
        {
            get => settingName;
            set
            {
                SetProperty(ref settingName, value);
                ShowSettingName = !string.IsNullOrEmpty(value);
            }
        }

        private bool showSettingName;
        public bool ShowSettingName
        {
            get => showSettingName; set => SetProperty(ref showSettingName, value);
        }


        [JsonIgnore]
        public int Sort { get; set; } = int.MaxValue;

        [JsonIgnore]
        public ObservableCollection<LocalIntelItem> LocalIntelItems { get; set; }
    }
}
