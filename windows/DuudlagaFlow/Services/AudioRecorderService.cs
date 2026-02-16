using System.IO;
using NAudio.Wave;
using DuudlagaFlow.Utilities;

namespace DuudlagaFlow.Services;

public class AudioRecorderService : IDisposable
{
    private WaveInEvent? _waveIn;
    private MemoryStream? _buffer;
    private BinaryWriter? _writer;
    private readonly object _lock = new();

    public float CurrentRmsLevel { get; private set; }
    public bool IsRecording { get; private set; }

    public List<(int DeviceNumber, string Name)> GetAvailableDevices()
    {
        var devices = new List<(int, string)>();
        for (int i = 0; i < WaveIn.DeviceCount; i++)
        {
            var caps = WaveIn.GetCapabilities(i);
            devices.Add((i, caps.ProductName));
        }
        return devices;
    }

    public void StartRecording(int deviceNumber = -1)
    {
        if (IsRecording) return;

        _waveIn = new WaveInEvent
        {
            DeviceNumber = deviceNumber >= 0 ? deviceNumber : 0,
            WaveFormat = new WaveFormat(
                Constants.Audio.SampleRate,
                Constants.Audio.BitDepth,
                Constants.Audio.Channels),
            BufferMilliseconds = Constants.Audio.BufferMilliseconds
        };

        _buffer = new MemoryStream();
        _writer = new BinaryWriter(_buffer);

        _waveIn.DataAvailable += OnDataAvailable;
        _waveIn.RecordingStopped += OnRecordingStopped;

        try
        {
            _waveIn.StartRecording();
            IsRecording = true;
        }
        catch (Exception ex)
        {
            _waveIn.Dispose();
            _waveIn = null;
            _buffer.Dispose();
            _buffer = null;
            _writer?.Dispose();
            _writer = null;
            throw new InvalidOperationException($"Микрофон нээхэд алдаа: {ex.Message}", ex);
        }
    }

    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        lock (_lock)
        {
            _writer?.Write(e.Buffer, 0, e.BytesRecorded);
        }

        // Calculate RMS for waveform display
        if (e.BytesRecorded > 0)
        {
            float sumOfSquares = 0;
            int sampleCount = e.BytesRecorded / 2; // 16-bit samples
            for (int i = 0; i < e.BytesRecorded - 1; i += 2)
            {
                short sample = BitConverter.ToInt16(e.Buffer, i);
                float normalized = sample / 32768f;
                sumOfSquares += normalized * normalized;
            }
            CurrentRmsLevel = MathF.Sqrt(sumOfSquares / Math.Max(sampleCount, 1));
        }
    }

    private void OnRecordingStopped(object? sender, StoppedEventArgs e)
    {
        if (e.Exception != null)
        {
            System.Diagnostics.Debug.WriteLine($"[AudioRecorder] Recording stopped with error: {e.Exception.Message}");
        }
    }

    public byte[]? StopRecording()
    {
        if (_waveIn == null || !IsRecording) return null;

        IsRecording = false;
        CurrentRmsLevel = 0;

        try
        {
            _waveIn.StopRecording();
        }
        catch { /* ignore stop errors */ }

        _waveIn.DataAvailable -= OnDataAvailable;
        _waveIn.RecordingStopped -= OnRecordingStopped;
        _waveIn.Dispose();
        _waveIn = null;

        byte[] pcmData;
        lock (_lock)
        {
            pcmData = _buffer?.ToArray() ?? Array.Empty<byte>();
            _writer?.Dispose();
            _writer = null;
            _buffer?.Dispose();
            _buffer = null;
        }

        if (pcmData.Length == 0) return null;

        // Encode as WAV
        return WavEncoder.Encode(pcmData, Constants.Audio.SampleRate, Constants.Audio.Channels, Constants.Audio.BitDepth);
    }

    public void Dispose()
    {
        if (IsRecording)
        {
            StopRecording();
        }
        _waveIn?.Dispose();
    }
}
