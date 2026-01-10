using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Browser
{
    public static class GlobalData
    {
        public static int userid = -1, engineid = -1;
        public static string query = "https://www.google.com/search?q=";
        public static bool SearchOverride = false, IsUrl = false, LoggedIn = false;
        public static ObservableCollection<fav> favs = new ObservableCollection<fav>();
        public static ObservableCollection<engine> engines = new ObservableCollection<engine>();
    }
}
