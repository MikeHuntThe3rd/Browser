using CefSharp;
using CefSharp.Wpf;
using Dragablz;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
        public ObservableCollection<Tab> TabHandler { get; } = new ObservableCollection<Tab>(); 
        public MainWindow()
        {
            InitializeComponent();
            tabs.InterTabController = TabDragManager.CreateController();
            tabs.ItemsSource = TabHandler;
        }

        private void CreateTab()
        {
            var NewTab = new Tab();
            TabHandler.Add(NewTab);
        }
        public void Tab_Drag(object sender, MouseButtonEventArgs e) {
            Window CurrWin = Window.GetWindow((DependencyObject)sender);
            CurrWin.DragMove();
        }
        #region window buttons
        private void close_tab_Click(object sender, RoutedEventArgs e)
        {
            if (tabs.SelectedItem is Tab tab)
            {
                TabHandler.Remove(tab);
                if (TabHandler.Count == 0)
                {
                    Close();
                }
            }
        }
        private void Drag(object sender, MouseButtonEventArgs e)
        {
            Window wn = Window.GetWindow((DependencyObject)sender);
            if (wn.WindowState == WindowState.Maximized)
            {
                wn.WindowState = WindowState.Normal;
                wn.Top  = 0.0;
                wn.DragMove();
            }
            else
            {
                wn.DragMove();
            }
        }
        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow((DependencyObject)sender).WindowState = WindowState.Minimized;
        }
        private void Maximize_Click(object sender, RoutedEventArgs e)
        {

            if (Window.GetWindow((DependencyObject)sender).WindowState == WindowState.Maximized) Window.GetWindow((DependencyObject)sender).WindowState = WindowState.Normal;
            else Window.GetWindow((DependencyObject)sender).WindowState = WindowState.Maximized;

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
