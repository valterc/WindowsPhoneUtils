using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WindowsPhoneUtils.Utils
{
    public class ScreenUtils
    {
        private ScreenUtils()
        {

        }

        public static Size ScaledScreenSize()
        {
            double s = (double)Application.Current.Host.Content.ScaleFactor / 100;
            return new Size((int)(Application.Current.Host.Content.ActualWidth * s + .5f), (int)(Application.Current.Host.Content.ActualHeight * s + .5f));
        }

        public static Size ScreenSize()
        {
            return new Size((int)Application.Current.Host.Content.ActualWidth, (int)Application.Current.Host.Content.ActualHeight);
        }

    }
}
