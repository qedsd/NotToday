using NotToday.Models;
using NotToday.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Wpf.Ui.Controls;

namespace NotToday.Wins
{
    public partial class LocalIntelNotifyWindow : FluentWindow
    {
        private int _maxCounts = 10;
        private readonly ObservableCollection<LocalIntelNotify> _localIntelNotifies = new ObservableCollection<LocalIntelNotify>();
        public LocalIntelNotifyWindow()
        {
            Closing += LocalIntelNotifyWindow_Closing;
            InitializeComponent();
            ListView.ItemsSource = _localIntelNotifies;
        }

        private void LocalIntelNotifyWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            LocalIntelNotifyService.Current.AfterHideWindow();
            e.Cancel = true;
            Hide();
        }

        public void Add(LocalIntelNotify localIntelNotify)
        {
            if (_localIntelNotifies.Count > _maxCounts)
            {
                _localIntelNotifies.RemoveAt(0);
            }
            _localIntelNotifies.Add(localIntelNotify);
            ListView.SelectedItem = localIntelNotify;
            ListView.ScrollIntoView(localIntelNotify);
        }
        public void ClearMsg()
        {
            _localIntelNotifies.Clear();
        }

        public void CloseWindow()
        {
            Closing -= LocalIntelNotifyWindow_Closing;
            Close();
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if((sender as ListBox).SelectedItem is LocalIntelNotify item)
            {
                Helpers.WindowHelper.SetForegroundWindow(item.HWnd);
            }
        }
    }
}
