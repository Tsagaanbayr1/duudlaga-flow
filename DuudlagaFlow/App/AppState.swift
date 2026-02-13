import Foundation
import Observation

enum DictationStatus: Equatable {
    case idle
    case recording(duration: TimeInterval)
    case processing
    case success(text: String)
    case error(message: String)

    var isRecording: Bool {
        if case .recording = self { return true }
        return false
    }

    var isProcessing: Bool {
        if case .processing = self { return true }
        return false
    }

    var isBusy: Bool {
        isRecording || isProcessing
    }
}

@Observable
final class AppState {
    var status: DictationStatus = .idle
    var audioLevel: Float = 0.0
    var audioLevels: [Float] = []
    var recordingStartTime: Date?

    var recordingDuration: TimeInterval {
        guard let start = recordingStartTime else { return 0 }
        return Date().timeIntervalSince(start)
    }

    var menuBarIcon: String {
        switch status {
        case .idle: return "mic.circle"
        case .recording: return "mic.circle.fill"
        case .processing: return "arrow.triangle.2.circlepath.circle"
        case .success: return "checkmark.circle.fill"
        case .error: return "exclamationmark.circle"
        }
    }

    func startRecording() {
        recordingStartTime = Date()
        audioLevels = []
        status = .recording(duration: 0)
    }

    func updateRecordingDuration() {
        guard recordingStartTime != nil else { return }
        status = .recording(duration: recordingDuration)
    }

    func setProcessing() {
        recordingStartTime = nil
        status = .processing
    }

    func setSuccess(text: String) {
        status = .success(text: text)
    }

    func setError(message: String) {
        status = .error(message: message)
    }

    func reset() {
        status = .idle
        audioLevel = 0.0
        recordingStartTime = nil
    }
}
