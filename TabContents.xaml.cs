using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
using HtmlAgilityPack;

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
            DisableButtons();
            if (Page_Loaded)
            {
                currtab.Url = browser.Address;
                RecordHistory(currtab.Url);
            }
            Page_Loaded = true;
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
            DisableButtons();
            if (url.Length == 0)
            {
                Page_Loaded = record;
                HomePage();
                currtab.Url = url;
                EnableButtons();
                return url;
            }
            else
            {
                LeftHomePage();
                Page_Loaded = record;
                url = (isURL) ? https(url) : GlobalData.query + Uri.EscapeDataString(url);
                browser.Load(url);
                currtab.Url = url;
                EnableButtons();
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
            browser.Visibility = Visibility.Hidden;
            home.Visibility = Visibility.Visible;
        }
        private void LeftHomePage()
        {
            browser.Visibility = Visibility.Visible;
            home.Visibility = Visibility.Hidden;
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
            if (currtab.Url == "") return;
            if (currtab.Url == currtab.History[currtab.HistoryIndex])
            {
                if (currtab.Url.Length != 0) browser.Reload();
                return;
            }
            ProcessURL(currtab.Url, IsURL_Valid(currtab.Url));
        }
        private void URL_TextChanged(object sender, TextChangedEventArgs e)
        {
            string txt = ((TextBox)sender).Text;
            if (GlobalData.SearchOverride && txt != "")
            {
                GlobalData.SearchOverride = false;
                currtab.Url = txt;
                
                ProcessURL(currtab.Url, GlobalData.IsUrl);
                browser.Visibility = Visibility.Visible;
                home.Visibility = Visibility.Hidden;
            }
        }
        private void back_Click(object sender, RoutedEventArgs e)
        {
            DisableButtons();
            currtab.HistoryIndex--;
            string url = currtab.History[currtab.HistoryIndex];
            if (url.Length == 0)
            {
                ProcessURL(url, true, false);
            }
            else
            {
                ProcessURL(url, IsURL_Valid(url), false);
            }
            currtab.Url = url;
        }
        private void foward_Click(object sender, RoutedEventArgs e)
        {
            DisableButtons();
            currtab.HistoryIndex++;
            string url = currtab.History[currtab.HistoryIndex];
            ProcessURL(url, IsURL_Valid(url), false);
            currtab.Url = url;
        }
        private void refresh_Click(object sender, RoutedEventArgs e)
        {
            DisableButtons();
            browser.Reload();
            currtab.Url = currtab.History[currtab.HistoryIndex];
        }
        private void EnableButtons()
        {
            foward.IsEnabled = CanShift(1);
            back.IsEnabled = CanShift(-1);
            refresh.IsEnabled = (browser.Address == "") ? false : true;
        }
        private void DisableButtons()
        {
            back.IsEnabled = false;
            foward.IsEnabled = false;
            refresh.IsEnabled = false;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (currtab.Url == "") HomePage();
            else ProcessURL(currtab.Url, true, false);

        }
        private  void browser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (!e.IsLoading)
            {
                browser.Dispatcher.Invoke( () => EnableButtons());
                browser.Dispatcher.Invoke( () => UsePageSource());
            }
        }
        private async void UsePageSource()
        {
            string source = await browser.GetSourceAsync();
            var html = new HtmlDocument();
            html.LoadHtml(source);
            string link = "../../../black-armory-forge-svgrepo-com.ico";
            try
            {
                var title = html.DocumentNode.SelectSingleNode("//title");
                currtab.Title = (title != null) ? title.InnerHtml : "New Tab";

                var icon_link = html.DocumentNode.SelectNodes("//link");
                link = (icon_link != null) ? icon_link.First(v => v.GetAttributeValue("rel", "").Equals("icon") || v.GetAttributeValue("rel", "").Equals("shortcut icon")).GetAttributeValue("href", "") : "";

                link = (!link.Substring(0, 6).Equals("https:")) ? currtab.Url + link : link;
                link = (currtab.Url.Contains("www.google.com") || currtab.Url.Count() == 0) ? "../../../black-armory-forge-svgrepo-com.ico" : link;
            }
            catch (Exception e) { }
            currtab.Icon = link;
        }
        private void MakeFavourtie_Click(object sender, RoutedEventArgs e)
        {
            if (home.Visibility == Visibility.Visible && IsURL_Valid(currtab.Url)) return;
            var fav = new fav()
            {
                Title = currtab.Title,
                Icon = currtab.Icon,
                Link = currtab.Url
            };
            GlobalData.favs.Add(fav);
            if (GlobalData.LoggedIn) DB.InsertData(Tuple.Create(GlobalData.userid, currtab.Icon, currtab.Url, currtab.Title));
            var reopened = Window.GetWindow((DependencyObject)sender);
            foreach (var win in Application.Current.Windows.Cast<Window>().ToList())
            {
                if (win != reopened) win.Close();
            }
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
