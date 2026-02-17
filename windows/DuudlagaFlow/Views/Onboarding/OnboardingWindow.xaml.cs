using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using DuudlagaFlow.Utilities;
using NAudio.Wave;

namespace DuudlagaFlow.Views.Onboarding;

public partial class OnboardingWindow : Window
{
    private int _currentStep;
    private const int TotalSteps = 4; // 0=Welcome, 1=Token, 2=Mic, 3=Complete
    private WaveInEvent? _testWaveIn;
    private DispatcherTimer? _testTimer;

    public OnboardingWindow()
    {
        InitializeComponent();
        try { Icon = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Resources/icon_32.png", UriKind.Absolute)); } catch { }
        UpdateStep();
    }

    private void UpdateStep()
    {
        StepLabel.Text = $"Алхам {_currentStep + 1}/{TotalSteps}";
        StepProgress.Value = _currentStep;

        WelcomeStep.Visibility = _currentStep == 0 ? Visibility.Visible : Visibility.Collapsed;
        TokenStep.Visibility = _currentStep == 1 ? Visibility.Visible : Visibility.Collapsed;
        MicStep.Visibility = _currentStep == 2 ? Visibility.Visible : Visibility.Collapsed;
        CompleteStep.Visibility = _currentStep == 3 ? Visibility.Visible : Visibility.Collapsed;

        BackButton.Visibility = _currentStep > 0 ? Visibility.Visible : Visibility.Collapsed;
        NextButton.Content = _currentStep == TotalSteps - 1 ? "Дуусгах" : "Дараах";

        if (_currentStep == 2)
        {
            LoadMicrophones();
        }
    }

    private void OnNextClick(object sender, RoutedEventArgs e)
    {
        if (_currentStep == 1)
        {
            // Save token before moving on
            var token = OnboardTokenBox.Password;
            if (!string.IsNullOrWhiteSpace(token))
            {
                App.Instance.Settings.ApiToken = token;
                App.Instance.SaveSettings();
            }
        }
        else if (_currentStep == 2)
        {
            StopMicTest();
            // Save microphone selection
            int micIndex = OnboardMicCombo.SelectedIndex - 1;
            App.Instance.Settings.SelectedMicrophoneIndex = micIndex;
            App.Instance.SaveSettings();
        }

        if (_currentStep >= TotalSteps - 1)
        {
            // Complete onboarding
            App.Instance.Settings.OnboardingCompleted = true;
            App.Instance.SaveSettings();
            Close();
            return;
        }

        _currentStep++;
        UpdateStep();
    }

    private void OnBackClick(object sender, RoutedEventArgs e)
    {
        StopMicTest();
        if (_currentStep > 0)
        {
            _currentStep--;
            UpdateStep();
        }
    }

    private async void OnTestOnboardToken(object sender, RoutedEventArgs e)
    {
        var token = OnboardTokenBox.Password;
        if (string.IsNullOrWhiteSpace(token))
        {
            OnboardTokenStatus.Text = "Token оруулна уу";
            OnboardTokenStatus.Foreground = new SolidColorBrush(Color.FromRgb(220, 50, 50));
            return;
        }

        OnboardTokenStatus.Text = "Шалгаж байна...";
        OnboardTokenStatus.Foreground = new SolidColorBrush(Color.FromRgb(100, 100, 100));

        var (success, message) = await App.Instance.ApiClient.TestTokenAsync(token, App.Instance.Settings.UseStandardStt);

        OnboardTokenStatus.Text = message;
        OnboardTokenStatus.Foreground = success
            ? new SolidColorBrush(Color.FromRgb(50, 180, 80))
            : new SolidColorBrush(Color.FromRgb(220, 50, 50));
    }

    private void OnOpenConsole(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo(Constants.ApiConsoleUrl) { UseShellExecute = true });
    }

    private void LoadMicrophones()
    {
        OnboardMicCombo.Items.Clear();
        OnboardMicCombo.Items.Add("Системийн үндсэн");

        var devices = App.Instance.AudioRecorder.GetAvailableDevices();
        foreach (var (_, name) in devices)
        {
            OnboardMicCombo.Items.Add(name);
        }

        int selectedIndex = App.Instance.Settings.SelectedMicrophoneIndex;
        OnboardMicCombo.SelectedIndex = selectedIndex < 0 ? 0 : selectedIndex + 1;
    }

    private void OnTestOnboardMic(object sender, RoutedEventArgs e)
    {
        if (_testWaveIn != null)
        {
            StopMicTest();
            return;
        }

        int deviceIndex = OnboardMicCombo.SelectedIndex - 1;

        try
        {
            _testWaveIn = new WaveInEvent
            {
                DeviceNumber = deviceIndex < 0 ? 0 : deviceIndex,
                WaveFormat = new WaveFormat(16000, 16, 1),
                BufferMilliseconds = 50
            };

            float currentLevel = 0;

            _testWaveIn.DataAvailable += (s, args) =>
            {
                if (args.BytesRecorded > 0)
                {
                    float sumOfSquares = 0;
                    int sampleCount = args.BytesRecorded / 2;
                    for (int i = 0; i < args.BytesRecorded - 1; i += 2)
                    {
                        short sample = BitConverter.ToInt16(args.Buffer, i);
                        float normalized = sample / 32768f;
                        sumOfSquares += normalized * normalized;
                    }
                    currentLevel = MathF.Sqrt(sumOfSquares / Math.Max(sampleCount, 1));
                }
            };

            _testWaveIn.StartRecording();

            _testTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
            _testTimer.Tick += (s, args) =>
            {
                OnboardMicLevel.Value = currentLevel;
            };
            _testTimer.Start();

            OnboardMicTestBtn.Content = "Зогсоох";
            OnboardMicStatus.Text = "Ярьж шалгана уу...";
        }
        catch (Exception ex)
        {
            OnboardMicStatus.Text = $"Алдаа: {ex.Message}";
        }
    }

    private void StopMicTest()
    {
        _testTimer?.Stop();
        _testTimer = null;
        try { _testWaveIn?.StopRecording(); } catch { }
        _testWaveIn?.Dispose();
        _testWaveIn = null;
        OnboardMicLevel.Value = 0;
        OnboardMicTestBtn.Content = "Микрофон шалгах";
        OnboardMicStatus.Text = "";
    }

    protected override void OnClosed(EventArgs e)
    {
        StopMicTest();
        base.OnClosed(e);
    }
}
