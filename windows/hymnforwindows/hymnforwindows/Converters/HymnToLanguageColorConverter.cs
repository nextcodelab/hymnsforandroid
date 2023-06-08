using HymnLibrary;
using HymnLibrary.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace hymnforwindows.Converters
{
    public class HymnToLanguageColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush color = new SolidColorBrush(Color.FromRgb(0xB0, 0xBE, 0xC5));
            if (value == null)
                return "";
            if (value is string _id)
            {
                var code = HymnalManager.GetLetters(_id);
                var colorBytes = HymnalManager.GetColorBytes(code);
                color = new SolidColorBrush(Color.FromRgb(colorBytes.BackgroundColor.R, colorBytes.BackgroundColor.G, colorBytes.BackgroundColor.B));
            }
            return color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
