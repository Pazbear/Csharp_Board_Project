using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace project
{
    class WindowUtils
    {
        public static bool IsOpen(Window win)
        {
            return Application.Current.Windows.Cast<Window>().Contains(win);
        }
    }
}
