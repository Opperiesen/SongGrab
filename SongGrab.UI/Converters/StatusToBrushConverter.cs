using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using WPF = System.Windows;

namespace SongGrab.UI.Converters
{
    public class StatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status.ToLower() switch
                {
                    "terminé" => WPF.Application.Current.Resources["SuccessBrush"],
                    "erreur" => WPF.Application.Current.Resources["ErrorBrush"],
                    "annulé" => WPF.Application.Current.Resources["PausedBrush"],
                    _ => WPF.Application.Current.Resources["ProgressBrush"]
                };
            }
            return System.Windows.Media.Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 