using System.Windows;
using System.Windows.Interop;
using DuudlagaFlow.Models;
using DuudlagaFlow.Utilities;
using DuudlagaFlow.ViewModels;
using Microsoft.Win32;

namespace DuudlagaFlow.Views.FlowBar;

public partial class FlowBarWindow : Window
{
    private readonly AppStateViewModel _appState;

    public FlowBarWindow(AppStateViewModel appState)
    {
        InitializeComponent();
        _appState = appState;

        Waveform.AudioLevels = _appState.AudioLevels;

        _appState.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(AppStateViewModel.Status))
            {
                Dispatcher.Invoke(UpdateVisualState);
            }
        };

        Loaded += OnLoaded;
        SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Apply WS_EX_NOACTIVATE and WS_EX_TOOLWINDOW styles
        var hwnd = new WindowInteropHelper(this).Handle;
        int exStyle = NativeMethods.GetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE);
        NativeMethods.SetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE,
            exStyle | NativeMethods.WS_EX_NOACTIVATE | NativeMethods.WS_EX_TOOLWINDOW);

        PositionAtBottom();
        UpdateVisualState();
    }

    private void PositionAtBottom()
    {
        var workArea = SystemParameters.WorkArea;
        Left = workArea.Left + (workArea.Width - Width) / 2;
        Top = workArea.Bottom - Height - Constants.FlowBar.BottomPadding;
    }

    private void OnDisplaySettingsChanged(object? sender, EventArgs e)
    {
        Dispatcher.Invoke(PositionAtBottom);
    }

    private void UpdateVisualState()
    {
        // Hide all
        IdleBorder.Visibility = Visibility.Collapsed;
        RecordingBorder.Visibility = Visibility.Collapsed;
        ProcessingBorder.Visibility = Visibility.Collapsed;
        SuccessBorder.Visibility = Visibility.Collapsed;
        ErrorBorder.Visibility = Visibility.Collapsed;

        switch (_appState.Status)
        {
            case DictationStatus.Idle:
                IdleBorder.Visibility = Visibility.Visible;
                Width = Constants.FlowBar.Width;
                Height = Constants.FlowBar.ExpandedHeight;
                break;

            case DictationStatus.Recording:
                RecordingBorder.Visibility = Visibility.Visible;
                Width = Constants.FlowBar.ExpandedWidth;
                Height = Constants.FlowBar.ExpandedHeight;
                break;

            case DictationStatus.Processing:
                ProcessingBorder.Visibility = Visibility.Visible;
                Width = Constants.FlowBar.ExpandedWidth;
                Height = Constants.FlowBar.ExpandedHeight;
                break;

            case DictationStatus.Success:
                SuccessBorder.Visibility = Visibility.Visible;
                Width = Constants.FlowBar.Width;
                Height = Constants.FlowBar.ExpandedHeight;
                break;

            case DictationStatus.Error:
                ErrorBorder.Visibility = Visibility.Visible;
                ErrorText.Text = _appState.StatusText;
                Width = Constants.FlowBar.ErrorWidth;
                Height = Constants.FlowBar.ExpandedHeight;
                break;
        }

        PositionAtBottom();
    }

    private void OnSettingsClick(object sender, RoutedEventArgs e)
    {
        App.Instance.ShowSettingsWindow();
    }

    private void OnQuitClick(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    protected override void OnClosed(EventArgs e)
    {
        SystemEvents.DisplaySettingsChanged -= OnDisplaySettingsChanged;
        base.OnClosed(e);
    }
}
