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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using NotToday.Models;
using NotToday.Wins;

namespace NotToday.Views
{
    /// <summary>
    /// LocalIntelPage.xaml 的交互逻辑
    /// </summary>
    public partial class LocalIntelPage : Page
    {
        public LocalIntelPage()
        {
            InitializeComponent();
        }

        private void SelectColorButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var cell = FindParent<DataGridCell>(button);
            if (cell != null)
            {
                cell.IsEditing = true;
            }
            //var color = (sender as Button).DataContext as LocalIntelColor;
            //if (color != null)
            //{
            //    ColorPickerWindow colorPickerWindow = new ColorPickerWindow(Color.FromArgb(color.Color.A, color.Color.R, color.Color.G, color.Color.B));
            //    if (colorPickerWindow.ShowDialog() == true)
            //    {
            //        //color.Color = System.Drawing.Color.FromArgb(colorPickerWindow.SelectedColor.A, colorPickerWindow.SelectedColor.R, colorPickerWindow.SelectedColor.G, colorPickerWindow.SelectedColor.B);
            //    }
            //}
        }
        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            // 获取父元素
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            // 如果没有父元素，返回null
            if (parentObject == null) return null;

            // 如果父元素是目标类型，返回
            if (parentObject is T parent)
                return parent;

            // 递归查找
            return FindParent<T>(parentObject);
        }

        private TextBox _editingTextBox;
        private void ColorTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            var box = sender as TextBox;
            _editingTextBox = box;
            var color = box.DataContext as LocalIntelColor;
            if (color != null)
            {
                box.Text = color.Color.ToString();
            }
        }

        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if(_editingTextBox != null)
            {
                var color = _editingTextBox.DataContext as LocalIntelColor;
                if (color != null)
                {
                    color.Color = (Color)ColorConverter.ConvertFromString(_editingTextBox.Text);
                }
            }
        }

        private void PickSoundFileButton_Click(object sender, RoutedEventArgs e)
        {
            var color = (sender as FrameworkElement).DataContext as LocalIntelColor;
            if (color != null)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();

                // 基本设置
                openFileDialog.Title = "选择文件";
                openFileDialog.Filter = "所有文件 (*.*)|*.*|音频文件 (*.mp3)|*.mp3";
                openFileDialog.FilterIndex = 1;
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                openFileDialog.Multiselect = false;

                // 显示对话框
                bool? result = openFileDialog.ShowDialog();

                if (result == true)
                {
                    color.SoundFile = openFileDialog.FileName;
                }
            }
        }
    }
}
