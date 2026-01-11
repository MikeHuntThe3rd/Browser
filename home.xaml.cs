using CefSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;


namespace Browser
{
    public partial class home : UserControl
    {
        public Tab currtab => this.DataContext as Tab;
        public home()
        {
            InitializeComponent();
            if(!GlobalData.DbConn) options.IsEnabled = false;
            if (GlobalData.LoggedIn) SwitchVisibility();
            engine_list.ItemsSource = GlobalData.engines;

            string topFile = "left.png";
            string bottomFile = "right.png";

            double targetWidth = 100;
            double targetHeight = 100;

            string topPath = Path.Combine(GlobalData.debugPath, topFile);
            if (File.Exists(topPath))
            {
                BitmapImage bitmap = new BitmapImage(new Uri(topPath));
                TopImage.Source = bitmap;
                TopImage.Width = targetWidth;
                TopImage.Height = targetHeight;

                Canvas.SetLeft(TopImage, 250);
                Canvas.SetTop(TopImage, -targetHeight);
            }

            string bottomPath = Path.Combine(GlobalData.debugPath, bottomFile);
            if (File.Exists(bottomPath))
            {
                BitmapImage bitmap = new BitmapImage(new Uri(bottomPath));
                BottomImage.Source = bitmap;
                BottomImage.Width = targetWidth;
                BottomImage.Height = targetHeight;

                Canvas.SetLeft(BottomImage, 350);
                Canvas.SetTop(BottomImage, MainCanvas.Height);
            }

            MainCanvas.Loaded += MainCanvas_Loaded;
        }
        private void TextBlock_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            GlobalData.SearchOverride = true;
            GlobalData.IsUrl = false;
            currtab.Url = search.Text;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            fav button = (fav)(sender as FrameworkElement).DataContext;
            GlobalData.SearchOverride = true;
            GlobalData.IsUrl = true;
            currtab.Url = button.Link;
        }
        private void MainCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            AnimateImagesLoop();
        }
        private void AnimateImagesLoop()
        {
            double centerY = (MainCanvas.ActualHeight - TopImage.Height) / 2;
            double centerX = (MainCanvas.ActualWidth - TopImage.Width) / 2;

            double topStartY = -TopImage.Height;
            double bottomStartY = MainCanvas.ActualHeight;

            Canvas.SetLeft(TopImage, centerX - 35);
            Canvas.SetLeft(BottomImage, centerX + 35);

            double topFinalY = MainCanvas.ActualHeight;
            double bottomFinalY = -BottomImage.Height;

            Storyboard sb = new Storyboard();

            IEasingFunction easeOut = new QuadraticEase { EasingMode = EasingMode.EaseOut }; // gyors → lassul
            IEasingFunction easeIn = new QuadraticEase { EasingMode = EasingMode.EaseIn };    // lassan indul → gyorsul

            double pauseTime = 2.0;
            double endPause = 1.0;

            // --- 1. Befelé mozgás ---
            DoubleAnimation topToCenter = new DoubleAnimation(topStartY, centerY, TimeSpan.FromSeconds(1))
            { EasingFunction = easeOut };
            Storyboard.SetTarget(topToCenter, TopImage);
            Storyboard.SetTargetProperty(topToCenter, new PropertyPath("(Canvas.Top)"));
            sb.Children.Add(topToCenter);

            DoubleAnimation bottomToCenter = new DoubleAnimation(bottomStartY, centerY, TimeSpan.FromSeconds(1))
            { EasingFunction = easeOut };
            Storyboard.SetTarget(bottomToCenter, BottomImage);
            Storyboard.SetTargetProperty(bottomToCenter, new PropertyPath("(Canvas.Top)"));
            sb.Children.Add(bottomToCenter);

            // --- 2. Közép 2 mp megállás + kifelé mozgás ---
            double moveOutBegin = 0.8 + pauseTime;

            DoubleAnimation topMoveOut = new DoubleAnimation(centerY, topFinalY, TimeSpan.FromSeconds(1))
            {
                BeginTime = TimeSpan.FromSeconds(moveOutBegin),
                EasingFunction = easeIn
            };
            Storyboard.SetTarget(topMoveOut, TopImage);
            Storyboard.SetTargetProperty(topMoveOut, new PropertyPath("(Canvas.Top)"));
            sb.Children.Add(topMoveOut);

            DoubleAnimation bottomMoveOut = new DoubleAnimation(centerY, bottomFinalY, TimeSpan.FromSeconds(1))
            {
                BeginTime = TimeSpan.FromSeconds(moveOutBegin),
                EasingFunction = easeIn
            };
            Storyboard.SetTarget(bottomMoveOut, BottomImage);
            Storyboard.SetTargetProperty(bottomMoveOut, new PropertyPath("(Canvas.Top)"));
            sb.Children.Add(bottomMoveOut);

            // --- 3. Loop újraindítás ---
            sb.Completed += (s, e) =>
            {
                // visszaállítás kezdőpozíciókra
                Canvas.SetTop(TopImage, topStartY);
                Canvas.SetTop(BottomImage, bottomStartY);

                // kis szünet a végén, majd újraindítás
                var delay = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(endPause)
                };
                delay.Tick += (s2, e2) =>
                {
                    delay.Stop();
                    AnimateImagesLoop(); // újraindítás
                };
                delay.Start();
            };

            sb.Begin();
        }
        private void options_Click(object sender, RoutedEventArgs e)
        {
            usr.Text = "username";
            pass.Text = "password";
            tab.Visibility = (tab.Visibility == Visibility.Visible) ? Visibility.Hidden : Visibility.Visible;
        }
        private void create_Click(object sender, RoutedEventArgs e)
        {
            foreach (var user in DB.GetUsers())
            {
                if(user.Item2 == usr.Text)
                {
                    MessageBox.Show("this user already exists");
                    return;
                }
            }
            DB.InsertUser(Tuple.Create(usr.Text, pass.Text, 1));
            MessageBox.Show("created new account");
        }
        private void SwitchVisibility()
        {
            opt_tab.Visibility = (opt_tab.Visibility == Visibility.Visible) ? Visibility.Hidden : Visibility.Visible ;
            tab_settings.Visibility = (tab_settings.Visibility == Visibility.Visible) ? Visibility.Hidden : Visibility.Visible;
        }
        private void login_Click(object sender, RoutedEventArgs e)
        {
            var data = DB.GetUsers();
            foreach (var user in data)
            {
                if (user.Item2 == usr.Text && user.Item3 == pass.Text)
                {
                    GlobalData.userid = user.Item1;
                    GlobalData.query = DB.GetEngines(user.Item4)[0].query;
                    GlobalData.favs = DB.GetUserData(GlobalData.userid);
                    GlobalData.engineid = user.Item4;
                    GlobalData.engines = DB.GetEngines();
                    GlobalData.LoggedIn = true;
                    App.FirstTab = true;
                    foreach (var eng in engine_list.Items)
                    {
                        var temp = (engine)(eng as FrameworkElement).DataContext;
                        if(temp.id == GlobalData.engineid)
                        {

                        }
                    }
                    SwitchVisibility();

                    var reopened = new MainWindow();
                    reopened.Show();

                    foreach (var win in Application.Current.Windows.Cast<Window>().ToList())
                    {
                        if(win != reopened) win.Close();
                    }
                    MessageBox.Show("logged in");
                    return;
                }
            }
            MessageBox.Show("incorrect username or password");
        }
        private void LogOut_Click(object sender, RoutedEventArgs e)
        {
            GlobalData.userid = -1;
            GlobalData.query = "https://www.google.com/search?q=";
            GlobalData.favs = new System.Collections.ObjectModel.ObservableCollection<fav>();
            GlobalData.engines = new System.Collections.ObjectModel.ObservableCollection<engine>();
            GlobalData.LoggedIn = false;
            App.FirstTab = true;
            SwitchVisibility();

            var reopened = new MainWindow();
            reopened.Show();

            foreach (var win in Application.Current.Windows.Cast<Window>().ToList())
            {
                if (win != reopened) win.Close();
            }
            MessageBox.Show("logged out");
        }
        private void eng_Click(object sender, RoutedEventArgs e)
        {
            engine eng = (engine)(sender as FrameworkElement).DataContext;
            GlobalData.query = eng.query;
            GlobalData.engineid = eng.id;
            DB.UpdateEngineId(GlobalData.userid, GlobalData.engineid);
            tab.Visibility = Visibility.Hidden;
        }
        private void Button_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            fav curr = (fav)(sender as FrameworkElement).DataContext;
            foreach (var fav in DB.GetUserData(GlobalData.userid))
            {
                if (fav.Link == curr.Link) DB.DeleteData(fav.Link);
            }
            GlobalData.favs.Remove(curr);
        }
    }
}
