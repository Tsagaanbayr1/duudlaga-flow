using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DuudlagaFlow.Utilities;
using Microsoft.Win32;

namespace DuudlagaFlow.Views.Settings;

public partial class GeneralSettingsPage : UserControl
{
    private bool _isLoading = true;

    public GeneralSettingsPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var settings = App.Instance.Settings;
            TokenBox.Password = settings.ApiToken ?? "";
            PunctuateCheck.IsChecked = settings.Punctuate;
            LaunchAtLoginCheck.IsChecked = settings.LaunchAtLogin;
            _isLoading = false;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"GeneralSettings load error:\n{ex.Message}\n{ex.StackTrace}",
                "Алдаа", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OnTokenChanged(object sender, RoutedEventArgs e)
    {
        if (_isLoading) return;
        App.Instance.Settings.ApiToken = TokenBox.Password;
        App.Instance.SaveSettings();
        TokenStatus.Text = "";
    }

    private async void OnTestTokenClick(object sender, RoutedEventArgs e)
    {
        var token = TokenBox.Password;
        if (string.IsNullOrWhiteSpace(token))
        {
            TokenStatus.Text = "Token оруулна уу";
            TokenStatus.Foreground = new SolidColorBrush(Color.FromRgb(220, 50, 50));
            return;
        }

        TestTokenButton.IsEnabled = false;
        TokenStatus.Text = "Шалгаж байна...";
        TokenStatus.Foreground = new SolidColorBrush(Color.FromRgb(100, 100, 100));

        var (success, message) = await App.Instance.ApiClient.TestTokenAsync(token);

        TokenStatus.Text = message;
        TokenStatus.Foreground = success
            ? new SolidColorBrush(Color.FromRgb(50, 180, 80))
            : new SolidColorBrush(Color.FromRgb(220, 50, 50));
        TestTokenButton.IsEnabled = true;
    }

    private void OnPunctuateChanged(object sender, RoutedEventArgs e)
    {
        if (_isLoading) return;
        App.Instance.Settings.Punctuate = PunctuateCheck.IsChecked ?? true;
        App.Instance.SaveSettings();
    }

    private void OnLaunchAtLoginChanged(object sender, RoutedEventArgs e)
    {
        if (_isLoading) return;
        bool enable = LaunchAtLoginCheck.IsChecked ?? false;
        App.Instance.Settings.LaunchAtLogin = enable;
        App.Instance.SaveSettings();
        SetLaunchAtStartup(enable);
    }

    private void OnConsoleClick(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo(Constants.ApiConsoleUrl) { UseShellExecute = true });
    }

    private static void SetLaunchAtStartup(bool enable)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Run", writable: true);
            if (key == null) return;

            if (enable)
            {
                string exePath = Process.GetCurrentProcess().MainModule!.FileName!;
                key.SetValue("DuudlagaFlow", $"\"{exePath}\"");
            }
            else
            {
                key.DeleteValue("DuudlagaFlow", throwOnMissingValue: false);
            }
        }
        catch
        {
            // Ignore registry errors
        }
    }
}
