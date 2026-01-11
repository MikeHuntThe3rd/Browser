using CefSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CefSharp.Wpf;
using CefSharp.DevTools.Audits;

namespace Browser
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static bool FirstTab = true;
        public App()
        {
            var settings = new CefSettings();
            Cef.Initialize(settings);
            if (DB.GetConn().State == ConnectionState.Open)
            {
                if(!DB.DBExists()) DB.DBCreate();
                DB.builder.Database = "users_db";
            }
            else
            {
                GlobalData.DbConn = false;
            }
        }
    }
}
