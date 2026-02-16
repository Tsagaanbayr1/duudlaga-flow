using System.Globalization;
using System.Windows;
using System.Windows.Data;
using DuudlagaFlow.Models;

namespace DuudlagaFlow.Converters;

public class StatusToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string iconName)
        {
            var uri = new Uri($"pack://application:,,,/Resources/Icons/{iconName}.ico", UriKind.Absolute);
            try
            {
                return new System.Windows.Media.Imaging.BitmapImage(uri);
            }
            catch
            {
                return DependencyProperty.UnsetValue;
            }
        }
        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
