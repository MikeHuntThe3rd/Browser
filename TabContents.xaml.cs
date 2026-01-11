using CefSharp;
using CefSharp.Wpf;
using HtmlAgilityPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
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



namespace Browser
{
    public partial class TabContents : UserControl
    {
        public Tab currtab => this.DataContext as Tab;
        private bool httpsFailed = false, Search = false;
        public TabContents()
        {
            InitializeComponent();
            browser.RequestHandler = new RedirectHandler();
        }
        private void browser_AddressChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            DisableButtons();
            if (GlobalData.RecordAllowed || Search)
            {
                currtab.Url = browser.Address;
                RecordHistory(currtab.Url);
            }
            Search = false;
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
                HomePage();
                currtab.Url = url;
                currtab.Title = "New Title";
                currtab.Icon = "";
                EnableButtons();
                return url;
            }
            else
            {
                LeftHomePage();
                url = (isURL) ? https(url) : GlobalData.query + Uri.EscapeDataString(url);
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
            Search = true;
            ProcessURL(currtab.Url, IsURL_Valid(currtab.Url));
        }
        private void URL_TextChanged(object sender, TextChangedEventArgs e)
        {
            string txt = ((TextBox)sender).Text;
            if (GlobalData.SearchOverride && txt != "")
            {
                GlobalData.SearchOverride = false;
                currtab.Url = txt;
                if (browser.Address == currtab.Url) currtab.HistoryIndex++;
                Search = true;
                ProcessURL(currtab.Url, GlobalData.IsUrl);
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
            MakeFavourtie.IsEnabled = true;
            refresh.IsEnabled = (browser.Address == "") ? false : true;
        }
        private void DisableButtons()
        {
            back.IsEnabled = false;
            foward.IsEnabled = false;
            refresh.IsEnabled = false;
            MakeFavourtie.IsEnabled = false;
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
            string link = Path.Combine(GlobalData.debugPath, "black-armory-forge-svgrepo-com.ico");
            try
            {
                var title = html.DocumentNode.SelectSingleNode("//title");
                currtab.Title = (title != null) ? title.InnerHtml : "New Tab";

                var icon_link = html.DocumentNode.SelectNodes("//link");
                link = (icon_link != null) ? icon_link.First(v => v.GetAttributeValue("rel", "").Equals("icon") || v.GetAttributeValue("rel", "").Equals("shortcut icon")).GetAttributeValue("href", "") : "";

                link = (!link.Substring(0, 6).Equals("https:")) ? currtab.Url + link : link;
                link = (currtab.Url.Contains("www.google.com") || 
                    currtab.Url.Contains("www.bing.com") ||
                    currtab.Url.Contains("duckduckgo.com") ||
                    currtab.Url.Contains("search.yahoo.com") ||
                    currtab.Url.Contains("www.qwant.com") ||
                    currtab.Url.Count() == 0) ? Path.Combine(GlobalData.debugPath, "black-armory-forge-svgrepo-com.ico") : link;
            }
            catch (Exception e) { }
            try
            {
                using (var client = new HttpClient())
                {
                    var request = new HttpRequestMessage(HttpMethod.Head, link);
                    var response = await client.SendAsync(request);

                    if (!response.IsSuccessStatusCode && !response.Content.Headers.ContentType.MediaType.StartsWith("image/"))
                        link = Path.Combine(GlobalData.debugPath, "black-armory-forge-svgrepo-com.ico");
                }
            }
            catch
            {
                link = Path.Combine(GlobalData.debugPath, "black-armory-forge-svgrepo-com.ico");
            }
            currtab.Icon = link;
        }
        private void MakeFavourtie_Click(object sender, RoutedEventArgs e)
        {
            if (home.Visibility == Visibility.Visible || !IsURL_Valid(currtab.Url)) return;
            if (GlobalData.DbConn && GlobalData.LoggedIn)
            {
                foreach (var curr in DB.GetUserData(GlobalData.userid))
                {
                    if (curr.Link == currtab.Url)
                    {
                        MessageBox.Show("a saved page already exists under this link");
                        return;
                    }
                }
            }
            else
            {
                foreach (var curr in GlobalData.favs)
                {
                    if (curr.Link == currtab.Url)
                    {
                        MessageBox.Show("a local page already exists under this link");
                        return;
                    }
                }
            }
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
