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
    public class HymnToLanguageBorderColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush color = new SolidColorBrush(Color.FromRgb(0x3D, 0x57, 0x7A));
            if (value == null)
                return "";
            if (value is string _id)
            {
                var code = HymnalManager.GetLetters(_id);
                var colorBytes = HymnalManager.GetColorBytes(code);
                color = new SolidColorBrush(Color.FromRgb(colorBytes.BorderColor.R, colorBytes.BorderColor.G, colorBytes.BorderColor.B));
            }
            return color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
