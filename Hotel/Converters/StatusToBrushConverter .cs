using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Hotel.Converters
{
    public class StatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                switch (status.ToLower())
                {
                    case "свободен":
                        return new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Зеленый
                    case "занят":
                        return new SolidColorBrush(Color.FromRgb(244, 67, 54)); // Красный
                    case "бронирование":
                        return new SolidColorBrush(Color.FromRgb(255, 193, 7)); // Желтый
                    default:
                        return new SolidColorBrush(Color.FromRgb(158, 158, 158)); // Серый
                }
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
