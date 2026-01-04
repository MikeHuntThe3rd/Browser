using Dragablz;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Browser
{
    public static class TabDragManager
    {
        public static InterTabController CreateController()
        {
            return new InterTabController
            {
                Partition = "BrowserTabs"
            };
        }
    }
}
