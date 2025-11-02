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
        private ChromiumWebBrowser _Page;
        private List<string> URL_History = new List<string>() { "" };
        private int HistoryIndex;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CreateTab();
        }
        private void UrlInput(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            ProcessURL(URL.Text);
        }
        private void ProcessURL(string url, bool record = true)
        {
            if (url.Length == 0)
            {
                HomePage();
                URL.Text = url;
                if(record) RecordHistory(url);
                EnableDisableButtons();
                return;
            }
            if (_Page != null)
            {
                _Page.Load(url);
                URL.Text = url;
                if (record) RecordHistory(url);
            }
            else
            {
                _Page = new ChromiumWebBrowser(url)
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Margin = new Thickness(0, 10, 0, 0)
                };
                Grid.SetRow(_Page, 1);
                Grid.SetColumnSpan(_Page, 4);
                grid.Children.Add(_Page);
                URL.Text = url;
                if (record)RecordHistory(url);
            }
            EnableDisableButtons();
        }
        private void HomePage()
        {
            DropBrowserInstance();
        }
        #region window buttons
        private void colse_tab_Click(object sender, RoutedEventArgs e)
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
            Window.GetWindow((Button)sender).WindowState = WindowState.Maximized;
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
        #region navigation
        private void DropBrowserInstance()
        {
            if (_Page != null)
            {
                grid.Children.Remove(_Page);
                _Page.Dispose();
                _Page = null;
            }
        }
        private void RecordHistory(string url)
        {
            if (HistoryIndex == URL_History.Count())
            {
                URL_History.Add(url);
                HistoryIndex = URL_History.Count() - 1;
            }
            else if (HistoryIndex < URL_History.Count())
            {
                for (int i = URL_History.Count() - 1; i > HistoryIndex; i--)
                {
                    URL_History.RemoveAt(i);
                }
                URL_History.Add(url);
                HistoryIndex = URL_History.Count() - 1;
            }
        }
        private void back_Click(object sender, RoutedEventArgs e)
        {
            HistoryIndex--;
            string url = URL_History[HistoryIndex];
            if(url.Length == 0)
            {
                HomePage();
            }
            else
            {
                ProcessURL(url, false);
            }
            URL.Text = url;
            EnableDisableButtons();
        }
        private void foward_Click(object sender, RoutedEventArgs e)
        {
            HistoryIndex++;
            string url = URL_History[HistoryIndex];
            if (url.Length == 0)
            {
                HomePage();
            }
            else
            {
                ProcessURL(url, false);
            }
            URL.Text = url;
            EnableDisableButtons();
        }
        private void refresh_Click(object sender, RoutedEventArgs e)
        {
            if (_Page != null)
            {
                _Page.Reload();
            }
        }
        private void EnableDisableButtons()
        {
            foward.IsEnabled = CanShift(1);
            back.IsEnabled = CanShift(-1);
            refresh.IsEnabled = (_Page == null) ? false : true;
        }
        private bool CanShift(int dir)
        {
            try
            {
                var val = URL_History[HistoryIndex + dir];
                return true;
            }
            catch (ArgumentOutOfRangeException e)
            {
                return false;
            }
        }

        #endregion
        
        private void CreateTab()
        {
            TabItem NewTab = new TabItem()
            {
                HeaderTemplate = (DataTemplate)tabs.FindResource("tab_header_button_wrapper"),
            };
            tabs.Items.Add(NewTab);
        }

        
    }
}
