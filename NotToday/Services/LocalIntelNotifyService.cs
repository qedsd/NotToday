using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using NotToday.Models;

namespace NotToday.Services
{
    public class LocalIntelNotifyService
    {
        private static LocalIntelNotifyService current;
        public static LocalIntelNotifyService Current
        {
            get
            {
                current ??= new LocalIntelNotifyService();
                return current;
            }
        }

        private Wins.LocalIntelNotifyWindow _notifyWindow;
        public Wins.LocalIntelNotifyWindow NotifyWindow
        {
            get
            {
                _notifyWindow ??= new Wins.LocalIntelNotifyWindow();
                return _notifyWindow;
            }
        }

        public void NotifyByWindow(LocalIntelNotify notify)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                NotifyWindow.Show();
                NotifyWindow.Add(notify);
            });
        }

        public void ClearWindow()
        {
            _notifyWindow?.ClearMsg();
        }
        public void HideWindow()
        {
            _notifyWindow?.Hide();
        }
        public void AfterHideWindow()
        {
            OnHideNotifyWindow.Invoke(this, EventArgs.Empty);
        }

        public void CloseWindow()
        {
            _notifyWindow?.CloseWindow();
        }

        public event EventHandler OnHideNotifyWindow;
    }
}
