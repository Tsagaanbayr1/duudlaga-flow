import Foundation
import Observation

@Observable
final class AppSettings {
    var apiToken: String {
        didSet { UserDefaults.standard.set(apiToken, forKey: Constants.Defaults.apiToken) }
    }

    var punctuate: Bool {
        didSet { UserDefaults.standard.set(punctuate, forKey: Constants.Defaults.punctuate) }
    }

    var dictationMode: DictationMode {
        didSet { UserDefaults.standard.set(dictationMode.rawValue, forKey: Constants.Defaults.dictationMode) }
    }

    var selectedMicrophoneID: String? {
        didSet { UserDefaults.standard.set(selectedMicrophoneID, forKey: Constants.Defaults.selectedMicrophoneID) }
    }

    var launchAtLogin: Bool {
        didSet { UserDefaults.standard.set(launchAtLogin, forKey: Constants.Defaults.launchAtLogin) }
    }

    var showFlowBar: Bool {
        didSet { UserDefaults.standard.set(showFlowBar, forKey: Constants.Defaults.showFlowBar) }
    }

    var onboardingCompleted: Bool {
        didSet { UserDefaults.standard.set(onboardingCompleted, forKey: Constants.Defaults.onboardingCompleted) }
    }

    var hasValidToken: Bool {
        !apiToken.trimmingCharacters(in: .whitespacesAndNewlines).isEmpty
    }

    init() {
        let defaults = UserDefaults.standard
        self.apiToken = defaults.string(forKey: Constants.Defaults.apiToken) ?? ""
        self.punctuate = defaults.object(forKey: Constants.Defaults.punctuate) as? Bool ?? true
        let modeRaw = defaults.string(forKey: Constants.Defaults.dictationMode) ?? DictationMode.pushToTalk.rawValue
        self.dictationMode = DictationMode(rawValue: modeRaw) ?? .pushToTalk
        self.selectedMicrophoneID = defaults.string(forKey: Constants.Defaults.selectedMicrophoneID)
        self.launchAtLogin = defaults.bool(forKey: Constants.Defaults.launchAtLogin)
        self.showFlowBar = defaults.object(forKey: Constants.Defaults.showFlowBar) as? Bool ?? true
        self.onboardingCompleted = defaults.bool(forKey: Constants.Defaults.onboardingCompleted)
    }
}
