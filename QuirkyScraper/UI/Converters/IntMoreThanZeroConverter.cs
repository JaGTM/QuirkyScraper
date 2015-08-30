using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace QuirkyScraper.UI.Converters
{
    public class IntMoreThanZeroConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value.GetType() != typeof(int)) return Visibility.Collapsed;
            int intValue = (int)value;
            return intValue > 0 ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value.GetType() != typeof(Visibility)) return -1;
            Visibility vis = (Visibility)value;

            return vis == Visibility.Visible ? 1 : 0;
        }
    }
}
