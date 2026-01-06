using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public Tab currtab => this.DataContext as Tab;
        private bool Page_Loaded = false, httpsFailed = false;
        public TabContents()
        {
            InitializeComponent();
        }
        private void browser_AddressChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            currtab.Url = browser.Address;
            if (Page_Loaded) RecordHistory(currtab.Url);
            Page_Loaded = true;
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
                Page_Loaded = record;
                HomePage();
                currtab.Url = url;
                return url;
            }
            else
            {
                Page_Loaded = record;
                url = (isURL) ? https(url) : $"https://www.google.com/search?q={Uri.EscapeDataString(url)}";
                browser.Load(url);
                currtab.Url = url;
                return url;
            }
        }
        private string https(string url)
        {
            if (!httpsFailed && url.Length >= 8 && !url.Substring(0, 8).Equals("https://") && url.Length >= 7 && !url.Substring(0, 7).Equals("http://"))
            {
                return "https://" + url;
            }
            else if (httpsFailed && url.Length >= 7 && !url.Substring(0, 7).Equals("http://"))
            {
                httpsFailed = false;
                return "http://" + url.Substring(8);
            }
            return url;
        }
        private void browser_LoadError(object sender, LoadErrorEventArgs e)
        {
            if (e.Frame.IsMain)
            {
                if (e.ErrorText == "ERR_CONNECTION_CLOSED")
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
            if (currtab.HistoryIndex == currtab.History.Count())
            {
                currtab.History.Add(url);
                currtab.HistoryIndex = currtab.History.Count() - 1;
            }
            else if (currtab.HistoryIndex < currtab.History.Count())
            {
                for (int i = currtab.History.Count() - 1; i > currtab.HistoryIndex; i--)
                {
                    currtab.History.RemoveAt(i);
                }
                currtab.History.Add(url);
                currtab.HistoryIndex = currtab.History.Count() - 1;
            }
        }
        private void UrlInput(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            currtab.Url = URL.Text;
            if (currtab.Url == currtab.History[currtab.HistoryIndex])
            {
                if (currtab.Url.Length != 0) browser.Reload();
                return;
            }
            ProcessURL(currtab.Url, IsURL_Valid(currtab.Url));
        }
        private void back_Click(object sender, RoutedEventArgs e)
        {
            currtab.HistoryIndex--;
            string url = currtab.History[currtab.HistoryIndex];
            if (url.Length == 0)
            {
                HomePage();
            }
            else
            {
                ProcessURL(url, IsURL_Valid(url), false);
            }
            currtab.Url = url;
            EnableDisableButtons();
        }
        private void foward_Click(object sender, RoutedEventArgs e)
        {
            currtab.HistoryIndex++;
            string url = currtab.History[currtab.HistoryIndex];
            if (url.Length == 0)
            {
                HomePage();
            }
            else
            {
                ProcessURL(url, IsURL_Valid(url), false);
            }
            currtab.Url = url;
            EnableDisableButtons();
        }
        private void refresh_Click(object sender, RoutedEventArgs e)
        {
            if (browser != null)
            {
                browser.Reload();
            }
        }
        private void EnableDisableButtons()
        {
            foward.IsEnabled = CanShift(1);
            back.IsEnabled = CanShift(-1);
            refresh.IsEnabled = (browser == null) ? false : true;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            browser.Load(currtab.Url);
            EnableDisableButtons();
        }
        private bool CanShift(int dir)
        {
            try
            {
                var val = currtab.History[currtab.HistoryIndex + dir];
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
