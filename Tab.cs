using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Browser
{
    public class Tab : INotifyPropertyChanged
    {
        private string _url = "";
        private string _title = "New Tab";
        private string _icon = "";
        public string Url
        {
            get => _url;
            set { _url = value; OnPropertyChanged(); }
        }
        public string Title 
        { 
            get => _title;
            set { _title = value; OnPropertyChanged(); }
        }
        public string Icon
        {
            get => _icon;
            set { _icon = value; OnPropertyChanged(); }
        }

        public List<string> History { get; } = new List<string>() { "" };
        public int HistoryIndex { get; set; } = 0;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}
