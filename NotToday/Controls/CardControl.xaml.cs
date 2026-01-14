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

namespace NotToday.Controls
{
    public partial class CardControl : UserControl
    {
        public CardControl()
        {
            InitializeComponent();
        }
        public FrameworkElement Header
        {
            get => (FrameworkElement)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }
        public static readonly DependencyProperty HeaderProperty
            = DependencyProperty.Register(
                nameof(Header),
                typeof(FrameworkElement),
                typeof(CardControl),
                new PropertyMetadata(null, new PropertyChangedCallback(HeaderPropertyPropertyChanged)));

        private static void HeaderPropertyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as CardControl;
            control.HeaderPresenter.Content = e.NewValue as FrameworkElement;
        }

        public new FrameworkElement Content
        {
            get => (FrameworkElement)GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }
        public static new readonly DependencyProperty ContentProperty
            = DependencyProperty.Register(
                nameof(Content),
                typeof(FrameworkElement),
                typeof(CardControl),
                new PropertyMetadata(null, ContentPropertyPropertyChanged));

        private static void ContentPropertyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as CardControl;
            control.ContentPresenter.Content = e.NewValue as FrameworkElement;
        }

        public static readonly DependencyProperty FonterProperty
           = DependencyProperty.Register(
               nameof(Fonter),
               typeof(FrameworkElement),
               typeof(CardControl),
               new PropertyMetadata(null, new PropertyChangedCallback(FonterPropertyPropertyChanged)));

        public FrameworkElement Fonter
        {
            get => (FrameworkElement)GetValue(FonterProperty);
            set => SetValue(FonterProperty, value);
        }

        private static void FonterPropertyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as CardControl;
            control.FonterPresenter.Content = e.NewValue as FrameworkElement;
            if (e.NewValue != null)
            {
                control.ContentBorder.BorderThickness = new Thickness(1, 0, 1, 0);
                control.ContentBorder.CornerRadius = new CornerRadius(0);
                control.FonterLine.Visibility = Visibility.Visible;
                control.FonterBorder.Visibility = Visibility.Visible;
            }
            else
            {
                control.ContentBorder.CornerRadius = new CornerRadius(0, 0, 2, 2);
                control.FonterLine.Visibility = Visibility.Collapsed;
                control.FonterBorder.Visibility = Visibility.Collapsed;
            }
        }
    }
}
