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
        public string Url
        {
            get => _url;
            set { _url = value; OnPropertyChanged(); }
        }

        public string Title { get; set; } = "New Tab";

        public ObservableCollection<string> History { get; } = new ObservableCollection<string>();
        public int HistoryIndex { get; set; } = 0;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}
