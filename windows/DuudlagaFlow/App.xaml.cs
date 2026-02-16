using System.IO;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using DuudlagaFlow.Models;
using DuudlagaFlow.Services;
using DuudlagaFlow.ViewModels;
using DuudlagaFlow.Views.FlowBar;
using DuudlagaFlow.Views.Settings;
using DuudlagaFlow.Views.History;
using DuudlagaFlow.Views.Onboarding;
using DuudlagaFlow.Utilities;

namespace DuudlagaFlow;

public partial class App : Application
{
    private Mutex? _mutex;
    private TaskbarIcon? _trayIcon;

    // Services
    public SettingsStore SettingsStore { get; private set; } = null!;
    public AppSettings Settings { get; private set; } = null!;
    public AppStateViewModel AppState { get; private set; } = null!;
    public AudioRecorderService AudioRecorder { get; private set; } = null!;
    public ChimegeApiClient ApiClient { get; private set; } = null!;
    public TextInsertionService TextInsertion { get; private set; } = null!;
    public HistoryStore HistoryStore { get; private set; } = null!;
    public GlobalHotkeyService HotkeyService { get; private set; } = null!;
    public DictationPipeline Pipeline { get; private set; } = null!;

    // Windows
    private FlowBarWindow? _flowBarWindow;
    private SettingsWindow? _settingsWindow;
    private HistoryWindow? _historyWindow;
    private OnboardingWindow? _onboardingWindow;

    public static App Instance => (App)Current;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Global exception handler to prevent silent crashes
        DispatcherUnhandledException += (s, args) =>
        {
            MessageBox.Show(
                $"Алдаа гарлаа:\n\n{args.Exception.Message}\n\n{args.Exception.StackTrace}",
                "DuudlagaFlow - Алдаа",
                MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };

        AppDomain.CurrentDomain.UnhandledException += (s, args) =>
        {
            if (args.ExceptionObject is Exception ex)
            {
                MessageBox.Show(
                    $"Ноцтой алдаа:\n\n{ex.Message}\n\n{ex.StackTrace}",
                    "DuudlagaFlow - Алдаа",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        };

        // Single instance check
        _mutex = new Mutex(true, Constants.App.MutexName, out bool createdNew);
        if (!createdNew)
        {
            MessageBox.Show("Duudlaga Flow аль хэдийн ажиллаж байна.", Constants.App.Name,
                MessageBoxButton.OK, MessageBoxImage.Information);
            Shutdown();
            return;
        }

        InitializeServices();
        InitializeTrayIcon();
        InitializeHotkey();

        // Show flow bar if enabled
        if (Settings.ShowFlowBar)
        {
            ShowFlowBar();
        }

        // Show onboarding if first run
        if (!Settings.OnboardingCompleted)
        {
            ShowOnboardingWindow();
        }
    }

    private void InitializeServices()
    {
        SettingsStore = new SettingsStore();
        Settings = SettingsStore.Load();
        AppState = new AppStateViewModel();
        AudioRecorder = new AudioRecorderService();
        ApiClient = new ChimegeApiClient();
        TextInsertion = new TextInsertionService();
        HistoryStore = new HistoryStore();
        HotkeyService = new GlobalHotkeyService();
        Pipeline = new DictationPipeline(AppState, Settings, AudioRecorder, ApiClient, TextInsertion, HistoryStore);
    }

    private void InitializeTrayIcon()
    {
        _trayIcon = new TaskbarIcon
        {
            ToolTipText = "Duudlaga Flow - Бэлэн",
        };

        // Set icon programmatically
        UpdateTrayIcon();

        // Create context menu
        var contextMenu = new System.Windows.Controls.ContextMenu();

        var statusItem = new System.Windows.Controls.MenuItem { Header = "Бэлэн - Ctrl+Alt дарна уу", IsEnabled = false };
        contextMenu.Items.Add(statusItem);
        contextMenu.Items.Add(new System.Windows.Controls.Separator());

        var flowBarItem = new System.Windows.Controls.MenuItem
        {
            Header = "Flow Bar харуулах",
            IsCheckable = true,
            IsChecked = Settings.ShowFlowBar
        };
        flowBarItem.Click += (s, e) =>
        {
            Settings.ShowFlowBar = flowBarItem.IsChecked;
            SettingsStore.Save(Settings);
            if (Settings.ShowFlowBar) ShowFlowBar();
            else HideFlowBar();
        };
        contextMenu.Items.Add(flowBarItem);
        contextMenu.Items.Add(new System.Windows.Controls.Separator());

        var historyItem = new System.Windows.Controls.MenuItem { Header = "Түүх..." };
        historyItem.Click += (s, e) => ShowHistoryWindow();
        contextMenu.Items.Add(historyItem);

        var settingsItem = new System.Windows.Controls.MenuItem { Header = "Тохиргоо..." };
        settingsItem.Click += (s, e) => ShowSettingsWindow();
        contextMenu.Items.Add(settingsItem);

        contextMenu.Items.Add(new System.Windows.Controls.Separator());

        var hotkeyStatusItem = new System.Windows.Controls.MenuItem
        {
            Header = HotkeyService.IsRunning ? "Товчлуур: идэвхтэй" : "Товчлуур дахин эхлүүлэх",
            IsEnabled = !HotkeyService.IsRunning
        };
        hotkeyStatusItem.Click += (s, e) =>
        {
            HotkeyService.Start();
            hotkeyStatusItem.Header = "Товчлуур: идэвхтэй";
            hotkeyStatusItem.IsEnabled = false;
        };
        contextMenu.Items.Add(hotkeyStatusItem);

        contextMenu.Items.Add(new System.Windows.Controls.Separator());

        var quitItem = new System.Windows.Controls.MenuItem { Header = "Гарах" };
        quitItem.Click += (s, e) => ExitApplication();
        contextMenu.Items.Add(quitItem);

        _trayIcon.ContextMenu = contextMenu;

        // Update tray on state changes
        AppState.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(AppStateViewModel.Status))
            {
                Dispatcher.Invoke(() =>
                {
                    UpdateTrayIcon();
                    _trayIcon.ToolTipText = AppState.TrayTooltip;
                    statusItem.Header = AppState.Status switch
                    {
                        DictationStatus.Idle => "Бэлэн - Ctrl+Alt дарна уу",
                        DictationStatus.Recording => "Бичиж байна...",
                        DictationStatus.Processing => "Боловсруулж байна...",
                        DictationStatus.Success => AppState.StatusText.Length > 40
                            ? AppState.StatusText[..40] + "..."
                            : AppState.StatusText,
                        DictationStatus.Error => AppState.StatusText,
                        _ => "Duudlaga Flow"
                    };
                });
            }
        };
    }

    private System.Drawing.Icon? _appIcon;

    private void UpdateTrayIcon()
    {
        if (_trayIcon == null) return;

        // Load the app icon from embedded resource (only once)
        if (_appIcon == null)
        {
            try
            {
                var resourceUri = new Uri("pack://application:,,,/Resources/app.ico", UriKind.Absolute);
                var streamInfo = GetResourceStream(resourceUri);
                if (streamInfo != null)
                {
                    _appIcon = new System.Drawing.Icon(streamInfo.Stream, 32, 32);
                }
            }
            catch
            {
                // Fallback: try loading from file next to exe
            }

            if (_appIcon == null)
            {
                var exeDir = AppDomain.CurrentDomain.BaseDirectory;
                var icoPath = Path.Combine(exeDir, "Resources", "app.ico");
                if (File.Exists(icoPath))
                {
                    _appIcon = new System.Drawing.Icon(icoPath, 32, 32);
                }
            }
        }

        if (_appIcon != null)
        {
            _trayIcon.Icon = _appIcon;
        }
    }

    private void InitializeHotkey()
    {
        HotkeyService.DictationMode = Settings.DictationModeEnum;

        HotkeyService.HotkeyDown += () =>
        {
            Dispatcher.Invoke(() => Pipeline.StartRecording());
        };

        HotkeyService.HotkeyUp += () =>
        {
            Dispatcher.Invoke(() => Pipeline.StopRecordingAndTranscribe());
        };

        HotkeyService.HotkeyToggle += () =>
        {
            Dispatcher.Invoke(() => Pipeline.ToggleRecording());
        };

        HotkeyService.Start();
    }

    public void ShowFlowBar()
    {
        if (_flowBarWindow == null)
        {
            _flowBarWindow = new FlowBarWindow(AppState);
            _flowBarWindow.Closed += (s, e) => _flowBarWindow = null;
        }
        _flowBarWindow.Show();
    }

    public void HideFlowBar()
    {
        _flowBarWindow?.Close();
        _flowBarWindow = null;
    }

    public void ShowSettingsWindow()
    {
        try
        {
            if (_settingsWindow == null)
            {
                _settingsWindow = new SettingsWindow();
                _settingsWindow.Closed += (s, e) => _settingsWindow = null;
            }
            _settingsWindow.Show();
            _settingsWindow.Activate();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Тохиргоо нээхэд алдаа:\n{ex.Message}\n{ex.StackTrace}",
                "Алдаа", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void ShowHistoryWindow()
    {
        try
        {
            if (_historyWindow == null)
            {
                _historyWindow = new HistoryWindow();
                _historyWindow.Closed += (s, e) => _historyWindow = null;
            }
            _historyWindow.Show();
            _historyWindow.Activate();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Түүх нээхэд алдаа:\n{ex.Message}\n{ex.StackTrace}",
                "Алдаа", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void ShowOnboardingWindow()
    {
        try
        {
            if (_onboardingWindow == null)
            {
                _onboardingWindow = new OnboardingWindow();
                _onboardingWindow.Closed += (s, e) => _onboardingWindow = null;
            }
            _onboardingWindow.Show();
            _onboardingWindow.Activate();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Тохируулга нээхэд алдаа:\n{ex.Message}\n{ex.StackTrace}",
                "Алдаа", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void SaveSettings()
    {
        SettingsStore.Save(Settings);
    }

    private void ExitApplication()
    {
        HotkeyService.Dispose();
        AudioRecorder.Dispose();
        _flowBarWindow?.Close();
        _settingsWindow?.Close();
        _historyWindow?.Close();
        _onboardingWindow?.Close();
        _trayIcon?.Dispose();
        _mutex?.ReleaseMutex();
        _mutex?.Dispose();
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayIcon?.Dispose();
        base.OnExit(e);
    }
}
