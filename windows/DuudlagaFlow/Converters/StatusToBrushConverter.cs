using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using DuudlagaFlow.Models;

namespace DuudlagaFlow.Converters;

public class StatusToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DictationStatus status)
        {
            return status switch
            {
                DictationStatus.Idle => new SolidColorBrush(Color.FromRgb(100, 100, 100)),
                DictationStatus.Recording => new SolidColorBrush(Color.FromRgb(220, 50, 50)),
                DictationStatus.Processing => new SolidColorBrush(Color.FromRgb(50, 130, 220)),
                DictationStatus.Success => new SolidColorBrush(Color.FromRgb(50, 180, 80)),
                DictationStatus.Error => new SolidColorBrush(Color.FromRgb(220, 50, 50)),
                _ => new SolidColorBrush(Color.FromRgb(100, 100, 100))
            };
        }
        return new SolidColorBrush(Color.FromRgb(100, 100, 100));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
