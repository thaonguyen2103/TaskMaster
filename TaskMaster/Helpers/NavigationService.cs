using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMaster.Helpers
{
    public static class NavigationService
    {
        public static Frame Frame { get; set; }

        public static void Navigate(Type pageType, object parameter = null)
        {
            Frame?.Navigate(pageType, parameter);
        }
    }
}
