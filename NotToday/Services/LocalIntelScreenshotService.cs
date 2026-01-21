using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NotToday.Models;
using NotToday.Wins;

namespace NotToday.Services
{
    public class LocalIntelScreenshotService
    {
        private static LocalIntelScreenshotService _current;
        public static LocalIntelScreenshotService Current
        {
            get
            {
                _current ??= new LocalIntelScreenshotService();
                return _current;
            }
        }

        private LocalIntelScreenshotWindow _window;

        public async Task<bool> Add(LocalIntelItem localIntelItem, bool autoStart)
        {
            if (_window == null)
            {
                _window = new LocalIntelScreenshotWindow();
                _window.Closed += Window_Closed;
            }
            _window.Show();
            await Task.Delay(500);
            return _window.Add(localIntelItem, autoStart);
        }
        public void Start()
        {
            _window?.Start();
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            _window = null;
        }

        public bool Remve(LocalIntelItem localIntelItem)
        {
            return _window?.Remove(localIntelItem) == true;
        }
    }
}
