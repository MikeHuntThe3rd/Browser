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
        public static bool SearchOverride = false, IsUrl = false;
        public static ObservableCollection<fav> favs = new ObservableCollection<fav>();
    }
}
