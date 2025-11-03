using CefSharp;
using CefSharp.Wpf;
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

namespace Browser
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            var settings = new CefSettings();
            Cef.Initialize(settings);
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CreateTab();
        }
        private void CreateTab()
        {
            TabItem NewTab = new TabItem()
            {
                HeaderTemplate = (DataTemplate)tabs.FindResource("tab_header_button_wrapper"),
                Content = new TabContents()
            };
            Grid.SetRow(NewTab, 1);
            tabs.Items.Add(NewTab);
            tabs.SelectedItem = NewTab;
        }
        #region window buttons
        private void close_tab_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            var tabItem = FindVisualParent<TabItem>(button);
            if (tabItem != null && tabs.Items.Count - 1 == 0)
            {
                Window.GetWindow((Button)sender).Close();
                return;
            }
            if (tabItem != null) tabs.Items.Remove(tabItem);
        }
        private static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null && !(child is T)) child = VisualTreeHelper.GetParent(child);
            return child as T;
        }
        private void Drag(object sender, MouseButtonEventArgs e)
        {
            Window.GetWindow((DependencyObject)sender).DragMove();
        }
        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow((Button)sender).WindowState = WindowState.Minimized;
        }
        private void Maximize_Click(object sender, RoutedEventArgs e)
        {

            if (Window.GetWindow((Button)sender).WindowState == WindowState.Maximized) Window.GetWindow((Button)sender).WindowState = WindowState.Normal;
            else Window.GetWindow((Button)sender).WindowState = WindowState.Maximized;

        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow((Button)sender).Close();
        }
        private void NewTab_Click(object sender, RoutedEventArgs e)
        {
            CreateTab();
        }
        #endregion
    }
}
