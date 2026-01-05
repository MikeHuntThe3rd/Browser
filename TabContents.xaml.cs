using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
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
    public partial class TabContents : UserControl
    {
        private bool httpsFailed = false, recordh = true;
        public Tab currTab => this.DataContext as Tab;
        public TabContents()
        {
            InitializeComponent();
        }
        private void _Page_AddressChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            currTab.Url = e.NewValue as string;
            if(recordh) RecordHistory(currTab.Address);
            EnableDisableButtons();
        }
        private bool IsURL_Valid(string url)
        {
            if (url.Contains("https://") ||
                url.Contains("http://") ||
                url.Contains("www.") ||
                url.Contains(".com") ||
                url.Contains(".net") ||
                url.Contains(".org") ||
                url.Contains(".io") ||
                url.Contains(".hu") ||
                url.Contains("localhost:")) return true;
            return false;
        }
        private string ProcessURL(string url, bool isURL, bool record = true)
        {
            if (url.Length == 0)
            {
                HomePage();
                currTab.Url = "";
                recordh = false;
                currTab.Address = "https://github.com";
                record = true;
                RecordHistory(currTab.Url);
                return url;
            }
            else
            {
                url = (isURL) ? https(url) : $"https://www.google.com/search?q={Uri.EscapeDataString(url)}";
                currTab.Url = url;
                currTab.Address = url;
                return url;
            }
        }
        private string https(string url)
        {
            if (!httpsFailed && url.Length >= 8 && !url.Substring(0, 8).Equals("https://") && url.Length >= 7 && !url.Substring(0, 7).Equals("http://"))
            {
                return "https://" + url;
            }
            else if(httpsFailed && url.Length >= 7 && !url.Substring(0, 7).Equals("http://"))
            {
                httpsFailed = false;
                return "http://" + url.Substring(8);
            }
            return url;
        }
        private void _Page_LoadError(object sender, LoadErrorEventArgs e)
        {
            if (e.Frame.IsMain)
            {
                if(e.ErrorText == "ERR_SSL_VERSION_OR_CIPHER_MISMATCH" || e.ErrorText == "ERR_SSL_PROTOCOL_ERROR")
                {
                    httpsFailed = true;
                    browser.Dispatcher.Invoke(() => { RecordHistory(ProcessURL(e.FailedUrl, true, false)); });
                }
                else
                {
                    browser.Dispatcher.Invoke(() => { RecordHistory(ProcessURL(e.FailedUrl.Replace("http://", ""), false, false)); });
                }
            }
        }
        #region TabControls
        private void HomePage()
        {
        }
        private void RecordHistory(string url)
        {
            if (currTab.HistoryIndex == currTab.History.Count())
            {
                currTab.History.Add(url);
                currTab.HistoryIndex = currTab.History.Count() - 1;
            }
            else if (currTab.HistoryIndex < currTab.History.Count())
            {
                for (int i = currTab.History.Count() - 1; i > currTab.HistoryIndex; i--)
                {
                    currTab.History.RemoveAt(i);
                }
                currTab.History.Add(url);
                currTab.HistoryIndex = currTab.History.Count() - 1;
            }
        }
        private void UrlInput(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            TextBox txt = (TextBox)sender;
            currTab.Url = txt.Text;
            if (currTab.Url == currTab.History[currTab.HistoryIndex])
            {
                if (currTab.Url.Length != 0) refresh_url();
                return;
            }
            ProcessURL(currTab.Url, IsURL_Valid(currTab.Url));
        }
        private void back_Click(object sender, RoutedEventArgs e)
        {
            currTab.HistoryIndex--;
            string url = currTab.History[currTab.HistoryIndex];
            if (url.Length == 0)
            {
                HomePage();
            }
            else
            {
                ProcessURL(url, IsURL_Valid(url), false);
            }
            currTab.Url = url;
            EnableDisableButtons();
        }
        private void foward_Click(object sender, RoutedEventArgs e)
        {
            currTab.HistoryIndex++;
            string url = currTab.History[currTab.HistoryIndex];
            if (url.Length == 0)
            {
                HomePage();
            }
            else
            {
                ProcessURL(url, IsURL_Valid(url), false);
            }
            currTab.Url = url;
            EnableDisableButtons();
        }
        private void refresh_Click(object sender, RoutedEventArgs e)
        {
            recordh = false;
            browser.Reload();
            recordh = true;
        }
        private void refresh_url()
        {
            recordh = false;
            string temp = currTab.Url;
            currTab.Url = string.Empty;
            currTab.Url = temp;
            recordh = true;
        }
        private void EnableDisableButtons()
        {
            foward.IsEnabled = CanShift(1);
            back.IsEnabled = CanShift(-1);
            refresh.IsEnabled = (currTab.Url == "") ? false : true;
        }
        private bool CanShift(int dir)
        {
            try
            {
                var val = currTab.History[currTab.HistoryIndex + dir];
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
