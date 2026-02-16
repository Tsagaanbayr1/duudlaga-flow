namespace DuudlagaFlow.Utilities;

public static class Constants
{
    public const string SttLongUrl = "https://api.chimege.com/v1.2/stt-long";
    public const string SttLongTranscriptUrl = "https://api.chimege.com/v1.2/stt-long-transcript";
    public const string ApiConsoleUrl = "https://console.chimege.com";

    public static class Audio
    {
        public const int SampleRate = 16000;
        public const int Channels = 1;
        public const int BitDepth = 16;
        public const int BufferMilliseconds = 50;
    }

    public static class FlowBar
    {
        public const double Width = 120;
        public const double Height = 6;
        public const double ExpandedWidth = 180;
        public const double ExpandedHeight = 28;
        public const double ErrorWidth = 200;
        public const double BottomPadding = 8;
        public const double SuccessDisplaySeconds = 0.5;
        public const double ErrorDisplaySeconds = 2.0;
    }

    public static class App
    {
        public const string Name = "Duudlaga Flow";
        public const string MutexName = "Global\\DuudlagaFlow_SingleInstance";
        public const string AppDataFolder = "DuudlagaFlow";
        public const string SettingsFile = "settings.json";
        public const string HistoryFile = "history.json";
    }
}
