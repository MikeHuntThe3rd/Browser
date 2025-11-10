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
        private int HistoryIndex = 0;
        private bool Page_Loaded = false;
        public TabContents()
        {
            InitializeComponent();
        }
        private void _Page_AddressChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            URL.Text = _Page.Address;
            if(Page_Loaded) RecordHistory(URL.Text);
            Page_Loaded = true;
            EnableDisableButtons();
            //RecordHistory(URL.Text);
        }
        private bool IsURL_Valid(string url)
        {
            if (url.Contains("http://") ||
                url.Contains("https://") ||
                url.Contains("www.") ||
                url.Contains(".com") ||
                url.Contains(".net") ||
                url.Contains(".org") ||
                url.Contains(".io") ||
                url.Contains("localhost:")) return true;
            return false;
        }
        private void ProcessURL(string url, bool isURL, bool record = true)
        {
            if (url.Length == 0)
            {
                Page_Loaded = record;
                HomePage();
                URL.Text = url;
                return;
            }
            if (_Page != null)
            {
                Page_Loaded = record;
                url = (isURL) ? url : $"https://www.google.com/search?q={Uri.EscapeDataString(url)}";
                _Page.Load(url);
                URL.Text = url;
            }
            else
            {
                Page_Loaded = record;
                url = (isURL) ? url: $"https://www.google.com/search?q={Uri.EscapeDataString(url)}";
                _Page = new ChromiumWebBrowser(url)
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };
                Grid.SetRow(_Page, 1);
                Grid.SetColumnSpan(_Page, 4);
                _Page.AddressChanged += _Page_AddressChanged;
                _Page.LoadError += _Page_LoadError;
                grid.Children.Add(_Page);
                URL.Text = url;
            }
        }
        private void _Page_LoadError(object sender, LoadErrorEventArgs e)
        {
            if (e.Frame.IsMain && e.ErrorCode == CefErrorCode.ConnectionFailed)
            {
                _Page.Dispatcher.Invoke(()=> { ProcessURL(e.FailedUrl, false, false); });
            }
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
            ProcessURL(URL.Text, IsURL_Valid(URL.Text));
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
                ProcessURL(url, IsURL_Valid(url), false);
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
                ProcessURL(url, IsURL_Valid(url), false);
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
