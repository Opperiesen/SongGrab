using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SongGrab.UI.Converters
{
    public class TrackStatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status == "Téléchargé" 
                    ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xE8, 0xF5, 0xE9)) // Vert très clair
                    : new SolidColorBrush(System.Windows.Media.Colors.Transparent);
            }
            return new SolidColorBrush(System.Windows.Media.Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 