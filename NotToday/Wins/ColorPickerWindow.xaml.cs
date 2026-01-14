using System;
using System.Collections.Generic;
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
    public partial class ColorPickerWindow : FluentWindow
    {
        public Color SelectedColor;
        public ColorPickerWindow(Color color)
        {
            InitializeComponent();
            //ColorPicker.SelectedColor = color;
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            //SelectedColor = ColorPicker.SelectedColor;
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
