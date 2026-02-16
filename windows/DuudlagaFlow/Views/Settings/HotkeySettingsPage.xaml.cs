using System.Windows;
using System.Windows.Controls;
using DuudlagaFlow.Models;

namespace DuudlagaFlow.Views.Settings;

public partial class HotkeySettingsPage : UserControl
{
    private bool _isLoading = true;

    public HotkeySettingsPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var mode = App.Instance.Settings.DictationModeEnum;
            switch (mode)
            {
                case DictationMode.PushToTalk:
                    PushToTalkRadio.IsChecked = true;
                    break;
                case DictationMode.Toggle:
                    ToggleRadio.IsChecked = true;
                    break;
                case DictationMode.HandsFree:
                    HandsFreeRadio.IsChecked = true;
                    break;
            }
            _isLoading = false;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"HotkeySettings load error:\n{ex.Message}\n{ex.StackTrace}",
                "Алдаа", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OnDictationModeChanged(object sender, RoutedEventArgs e)
    {
        if (_isLoading) return;

        DictationMode mode;
        if (PushToTalkRadio.IsChecked == true)
            mode = DictationMode.PushToTalk;
        else if (ToggleRadio.IsChecked == true)
            mode = DictationMode.Toggle;
        else
            mode = DictationMode.HandsFree;

        App.Instance.Settings.DictationModeEnum = mode;
        App.Instance.SaveSettings();
        App.Instance.HotkeyService.DictationMode = mode;
    }
}
