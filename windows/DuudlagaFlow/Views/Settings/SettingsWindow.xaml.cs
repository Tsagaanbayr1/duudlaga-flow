using System.Windows;
using System.Windows.Media.Imaging;

namespace DuudlagaFlow.Views.Settings;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        try
        {
            InitializeComponent();
            SetWindowIcon();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"SettingsWindow init error:\n{ex.Message}\n\n{ex.InnerException?.Message}\n\n{ex.StackTrace}",
                "Алдаа", MessageBoxButton.OK, MessageBoxImage.Error);
            throw;
        }
    }

    private void SetWindowIcon()
    {
        try
        {
            Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/icon_32.png", UriKind.Absolute));
        }
        catch { }
    }
}
