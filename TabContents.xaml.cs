using System;
using System.Collections;
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
using CefSharp;
using CefSharp.Wpf;
using static System.Net.Mime.MediaTypeNames;

namespace Browser
{
    /// <summary>
    /// Interaction logic for TabContetnts.xaml
    /// </summary>
    public partial class TabContents : UserControl
    {
        private ChromiumWebBrowser _Page;
        private List<string> URL_History = new List<string>() { "" };
        private int HistoryIndex;
        public TabContents()
        {
            InitializeComponent();
        }
        private void _Page_AddressChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            URL.Text = _Page.Address;
        }
        private void ProcessURL(string url, bool record = true)
        {
            if (url.Length == 0)
            {
                HomePage();
                URL.Text = url;
                if (record) RecordHistory(url);
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
                _Page = new ChromiumWebBrowser($"https://www.google.com/search?q={Uri.EscapeDataString(url)}")
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Margin = new Thickness(0, 10, 0, 0)
                };
                Grid.SetRow(_Page, 1);
                Grid.SetColumnSpan(_Page, 4);
                _Page.AddressChanged += _Page_AddressChanged;
                grid.Children.Add(_Page);
                URL.Text = url;
                if (record) RecordHistory(url);
            }
            EnableDisableButtons();
        }
        private void DropBrowserInstance()
        {
            if (_Page != null)
            {
                grid.Children.Remove(_Page);
                _Page.Dispose();
                _Page = null;
            }
        }
        #region TabControls
        private void HomePage()
        {
            DropBrowserInstance();
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
        private void UrlInput(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            if (URL.Text == URL_History[HistoryIndex])
            {
                if (URL.Text.Length != 0) _Page.Reload();
                return;
            }
            ProcessURL(URL.Text);
        }
        private void back_Click(object sender, RoutedEventArgs e)
        {
            HistoryIndex--;
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
    }
}
