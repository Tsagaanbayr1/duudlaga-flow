import Foundation

enum DictationMode: String, Codable, CaseIterable, Identifiable {
    case pushToTalk
    case toggle
    case handsFree

    var id: String { rawValue }

    var displayName: String {
        switch self {
        case .pushToTalk: return "Hold to Talk"
        case .toggle: return "Press to Toggle"
        case .handsFree: return "Hands-Free"
        }
    }

    var description: String {
        switch self {
        case .pushToTalk: return "Hold Ctrl+Option to record, release to transcribe"
        case .toggle: return "Press Ctrl+Option to start/stop recording"
        case .handsFree: return "Press Ctrl+Option to start, auto-stops on silence"
        }
    }
}
