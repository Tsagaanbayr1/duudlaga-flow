using System.Text.Json.Serialization;

namespace DuudlagaFlow.Models;

public class AppSettings
{
    [JsonPropertyName("apiToken")]
    public string ApiToken { get; set; } = "";

    [JsonPropertyName("useStandardStt")]
    public bool UseStandardStt { get; set; } = true;

    [JsonPropertyName("punctuate")]
    public bool Punctuate { get; set; } = true;

    [JsonPropertyName("dictationMode")]
    public string DictationMode { get; set; } = "PushToTalk";

    [JsonPropertyName("selectedMicrophoneIndex")]
    public int SelectedMicrophoneIndex { get; set; } = -1;

    [JsonPropertyName("launchAtLogin")]
    public bool LaunchAtLogin { get; set; }

    [JsonPropertyName("showFlowBar")]
    public bool ShowFlowBar { get; set; } = true;

    [JsonPropertyName("onboardingCompleted")]
    public bool OnboardingCompleted { get; set; }

    [JsonIgnore]
    public bool HasValidToken => !string.IsNullOrWhiteSpace(ApiToken);

    [JsonIgnore]
    public DictationMode DictationModeEnum
    {
        get => Enum.TryParse<DictationMode>(DictationMode, out var mode)
            ? mode : Models.DictationMode.PushToTalk;
        set => DictationMode = value.ToString();
    }
}
