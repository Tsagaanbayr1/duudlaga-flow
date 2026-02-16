using System.Collections.ObjectModel;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using DuudlagaFlow.Models;

namespace DuudlagaFlow.ViewModels;

public partial class AppStateViewModel : ObservableObject
{
    [ObservableProperty]
    private DictationStatus _status = DictationStatus.Idle;

    [ObservableProperty]
    private float _audioLevel;

    [ObservableProperty]
    private string _statusText = "";

    [ObservableProperty]
    private double _recordingDuration;

    [ObservableProperty]
    private string _menuBarIcon = "mic_idle";

    [ObservableProperty]
    private string _trayTooltip = "Duudlaga Flow - Бэлэн";

    public ObservableCollection<float> AudioLevels { get; } = new();

    private DateTime _recordingStartTime;

    partial void OnStatusChanged(DictationStatus value)
    {
        UpdateMenuBarIcon();
        UpdateTrayTooltip();
    }

    public void StartRecording()
    {
        _recordingStartTime = DateTime.Now;
        RecordingDuration = 0;
        AudioLevels.Clear();
        Status = DictationStatus.Recording;
        StatusText = "Бичиж байна...";
    }

    public void SetProcessing()
    {
        Status = DictationStatus.Processing;
        StatusText = "Боловсруулж байна...";
    }

    public void SetSuccess(string text)
    {
        Status = DictationStatus.Success;
        StatusText = text;
    }

    public void SetError(string message)
    {
        Status = DictationStatus.Error;
        StatusText = message;
    }

    public void Reset()
    {
        Status = DictationStatus.Idle;
        StatusText = "";
        AudioLevel = 0;
        RecordingDuration = 0;
        AudioLevels.Clear();
    }

    public void UpdateRecordingDuration()
    {
        if (Status == DictationStatus.Recording)
        {
            RecordingDuration = (DateTime.Now - _recordingStartTime).TotalSeconds;
        }
    }

    public bool IsBusy => Status == DictationStatus.Recording || Status == DictationStatus.Processing;
    public bool IsRecording => Status == DictationStatus.Recording;

    private void UpdateMenuBarIcon()
    {
        MenuBarIcon = Status switch
        {
            DictationStatus.Idle => "mic_idle",
            DictationStatus.Recording => "mic_recording",
            DictationStatus.Processing => "mic_processing",
            DictationStatus.Success => "mic_success",
            DictationStatus.Error => "mic_error",
            _ => "mic_idle"
        };
    }

    private void UpdateTrayTooltip()
    {
        TrayTooltip = Status switch
        {
            DictationStatus.Idle => "Duudlaga Flow - Бэлэн",
            DictationStatus.Recording => "Duudlaga Flow - Бичиж байна...",
            DictationStatus.Processing => "Duudlaga Flow - Боловсруулж байна...",
            DictationStatus.Success => "Duudlaga Flow - Амжилттай",
            DictationStatus.Error => $"Duudlaga Flow - Алдаа",
            _ => "Duudlaga Flow"
        };
    }
}
