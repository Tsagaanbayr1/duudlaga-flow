using System.Windows.Threading;
using DuudlagaFlow.Models;
using DuudlagaFlow.ViewModels;
using DuudlagaFlow.Utilities;

namespace DuudlagaFlow.Services;

public class DictationPipeline
{
    private readonly AppStateViewModel _appState;
    private readonly AppSettings _settings;
    private readonly AudioRecorderService _audioRecorder;
    private readonly ChimegeApiClient _apiClient;
    private readonly TextInsertionService _textInsertion;
    private readonly HistoryStore _historyStore;

    private DispatcherTimer? _recordingTimer;
    private CancellationTokenSource? _transcriptionCts;

    public DictationPipeline(
        AppStateViewModel appState,
        AppSettings settings,
        AudioRecorderService audioRecorder,
        ChimegeApiClient apiClient,
        TextInsertionService textInsertion,
        HistoryStore historyStore)
    {
        _appState = appState;
        _settings = settings;
        _audioRecorder = audioRecorder;
        _apiClient = apiClient;
        _textInsertion = textInsertion;
        _historyStore = historyStore;
    }

    public void StartRecording()
    {
        if (_appState.IsBusy) return;

        if (!_settings.HasValidToken)
        {
            _appState.SetError("API token тохируулаагүй байна");
            ScheduleReset(Constants.FlowBar.ErrorDisplaySeconds);
            return;
        }

        // Capture the target window before recording
        _textInsertion.CaptureTargetWindow();

        try
        {
            _audioRecorder.StartRecording(_settings.SelectedMicrophoneIndex);
            _appState.StartRecording();

            // Timer to update audio levels and duration
            _recordingTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(50)
            };
            _recordingTimer.Tick += (s, e) =>
            {
                _appState.AudioLevel = _audioRecorder.CurrentRmsLevel;
                _appState.AudioLevels.Add(_audioRecorder.CurrentRmsLevel);
                if (_appState.AudioLevels.Count > 60)
                {
                    _appState.AudioLevels.RemoveAt(0);
                }
                _appState.UpdateRecordingDuration();
            };
            _recordingTimer.Start();
        }
        catch (Exception ex)
        {
            _appState.SetError($"Бичлэг эхлэхэд алдаа: {ex.Message}");
            ScheduleReset(Constants.FlowBar.ErrorDisplaySeconds);
        }
    }

    public async void StopRecordingAndTranscribe()
    {
        if (!_appState.IsRecording) return;

        _recordingTimer?.Stop();
        _recordingTimer = null;

        var duration = _appState.RecordingDuration;

        var audioData = _audioRecorder.StopRecording();
        if (audioData == null || audioData.Length == 0)
        {
            _appState.SetError("Аудио бичигдээгүй");
            ScheduleReset(Constants.FlowBar.ErrorDisplaySeconds);
            return;
        }

        _appState.SetProcessing();

        _transcriptionCts?.Cancel();
        _transcriptionCts = new CancellationTokenSource();

        try
        {
            var text = await _apiClient.TranscribeAsync(
                audioData,
                _settings.ApiToken,
                _settings.Punctuate,
                _transcriptionCts.Token);

            var pasted = await _textInsertion.InsertTextAsync(text);

            var record = new TranscriptionRecord
            {
                Text = text,
                DurationSeconds = duration
            };
            _historyStore.Add(record);

            if (pasted)
            {
                _appState.SetSuccess(text);
            }
            else
            {
                _appState.SetSuccess("Clipboard-д хуулагдлаа. Ctrl+V дарна уу");
            }
            ScheduleReset(Constants.FlowBar.SuccessDisplaySeconds);
        }
        catch (Exception ex)
        {
            _appState.SetError(ex.Message);
            ScheduleReset(Constants.FlowBar.ErrorDisplaySeconds);
        }
    }

    public void ToggleRecording()
    {
        if (_appState.IsRecording)
        {
            StopRecordingAndTranscribe();
        }
        else
        {
            StartRecording();
        }
    }

    public void CancelRecording()
    {
        _recordingTimer?.Stop();
        _recordingTimer = null;
        _transcriptionCts?.Cancel();
        _ = _audioRecorder.StopRecording();
        _appState.Reset();
    }

    private void ScheduleReset(double delaySeconds)
    {
        Task.Delay(TimeSpan.FromSeconds(delaySeconds)).ContinueWith(_ =>
        {
            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                _appState.Reset();
            });
        });
    }
}
