import Foundation

enum Constants {
    static let apiURL = URL(string: "https://api.chimege.com/v1.2/transcribe")!
    static let sttURL = URL(string: "https://api.chimege.com/v1.2/transcribe")!
    static let apiConsoleURL = URL(string: "https://console.chimege.com")!

    enum Defaults {
        static let apiToken = "chimege_api_token"
        static let punctuate = "chimege_punctuate"
        static let dictationMode = "dictation_mode"
        static let selectedMicrophoneID = "selected_microphone_id"
        static let launchAtLogin = "launch_at_login"
        static let showFlowBar = "show_flow_bar"
        static let onboardingCompleted = "onboarding_completed"
        static let useStandardStt = "use_standard_stt"
    }

    enum Audio {
        static let sampleRate: Double = 16000
        static let channels: UInt32 = 1
        static let bitDepth: UInt32 = 16
    }

    enum FlowBar {
        static let width: CGFloat = 120
        static let height: CGFloat = 6
        static let expandedWidth: CGFloat = 180
        static let expandedHeight: CGFloat = 28
        static let errorWidth: CGFloat = 200
        static let bottomPadding: CGFloat = 8
        static let successDisplayDuration: TimeInterval = 0.5
        static let errorDisplayDuration: TimeInterval = 2.0
    }

    enum Clipboard {
        static let restoreDelay: TimeInterval = 0.15
    }
}
