using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using NAudio.Wave;

namespace DuudlagaFlow.Views.Settings;

public partial class AudioSettingsPage : UserControl
{
    private bool _isLoading = true;
    private WaveInEvent? _testWaveIn;
    private DispatcherTimer? _testTimer;

    public AudioSettingsPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            LoadDevices();
            _isLoading = false;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"AudioSettings load error:\n{ex.Message}\n{ex.StackTrace}",
                "Алдаа", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        StopMicTest();
    }

    private void LoadDevices()
    {
        _isLoading = true;
        MicrophoneCombo.Items.Clear();
        MicrophoneCombo.Items.Add("Системийн үндсэн");

        var devices = App.Instance.AudioRecorder.GetAvailableDevices();
        foreach (var (_, name) in devices)
        {
            MicrophoneCombo.Items.Add(name);
        }

        int selectedIndex = App.Instance.Settings.SelectedMicrophoneIndex;
        MicrophoneCombo.SelectedIndex = selectedIndex < 0 ? 0 : selectedIndex + 1;
        _isLoading = false;
    }

    private void OnMicrophoneChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isLoading) return;
        int index = MicrophoneCombo.SelectedIndex - 1; // -1 = system default
        App.Instance.Settings.SelectedMicrophoneIndex = index;
        App.Instance.SaveSettings();
    }

    private void OnTestMicClick(object sender, RoutedEventArgs e)
    {
        if (_testWaveIn != null)
        {
            StopMicTest();
            return;
        }

        int deviceIndex = MicrophoneCombo.SelectedIndex - 1;

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
                MicLevelBar.Value = currentLevel;
            };
            _testTimer.Start();

            TestMicButton.Content = "Зогсоох";
            MicTestStatus.Text = "Микрофон ажиллаж байна...";
        }
        catch (Exception ex)
        {
            MicTestStatus.Text = $"Алдаа: {ex.Message}";
            StopMicTest();
        }
    }

    private void StopMicTest()
    {
        _testTimer?.Stop();
        _testTimer = null;

        try { _testWaveIn?.StopRecording(); } catch { }
        _testWaveIn?.Dispose();
        _testWaveIn = null;

        MicLevelBar.Value = 0;
        TestMicButton.Content = "Шалгах";
        MicTestStatus.Text = "";
    }

    private void OnRefreshDevicesClick(object sender, RoutedEventArgs e)
    {
        StopMicTest();
        LoadDevices();
    }
}
