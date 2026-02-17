import Foundation

@MainActor
final class DictationPipeline {
    let appState: AppState
    let settings: AppSettings
    let audioRecorder: AudioRecorder
    let apiClient: ChimegeAPIClient
    let textInsertion: TextInsertionService
    let historyStore: HistoryStore

    private var recordingTimer: Timer?

    init(
        appState: AppState,
        settings: AppSettings,
        audioRecorder: AudioRecorder,
        apiClient: ChimegeAPIClient,
        textInsertion: TextInsertionService,
        historyStore: HistoryStore
    ) {
        self.appState = appState
        self.settings = settings
        self.audioRecorder = audioRecorder
        self.apiClient = apiClient
        self.textInsertion = textInsertion
        self.historyStore = historyStore
    }

    func startRecording() {
        guard !appState.status.isBusy else { return }
        guard settings.hasValidToken else {
            appState.setError(message: "API token тохируулаагүй байна")
            scheduleReset(after: Constants.FlowBar.errorDisplayDuration)
            return
        }

        // Remember which app is focused so we can paste into it later
        textInsertion.captureTargetApp()

        do {
            try audioRecorder.startRecording(deviceID: settings.selectedMicrophoneID)
            appState.startRecording()

            recordingTimer = Timer.scheduledTimer(withTimeInterval: 0.05, repeats: true) { [weak self] _ in
                Task { @MainActor in
                    guard let self = self else { return }
                    self.appState.audioLevel = self.audioRecorder.currentRMSLevel
                    self.appState.audioLevels.append(self.audioRecorder.currentRMSLevel)
                    if self.appState.audioLevels.count > 60 {
                        self.appState.audioLevels.removeFirst()
                    }
                    self.appState.updateRecordingDuration()
                }
            }
        } catch {
            appState.setError(message: "Бичлэг эхлэхэд алдаа: \(error.localizedDescription)")
            scheduleReset(after: Constants.FlowBar.errorDisplayDuration)
        }
    }

    func stopRecordingAndTranscribe() {
        guard appState.status.isRecording else { return }

        recordingTimer?.invalidate()
        recordingTimer = nil

        let duration = appState.recordingDuration

        guard let audioData = audioRecorder.stopRecording() else {
            appState.setError(message: "Аудио бичигдээгүй")
            scheduleReset(after: Constants.FlowBar.errorDisplayDuration)
            return
        }

        appState.setProcessing()

        Task {
            do {
                let text = try await apiClient.transcribe(
                    audioData: audioData,
                    token: settings.apiToken,
                    punctuate: settings.punctuate,
                    useStandardStt: settings.useStandardStt
                )

                let pasted = await textInsertion.insertText(text)

                let record = TranscriptionRecord(text: text, durationSeconds: duration)
                historyStore.add(record)

                if pasted {
                    appState.setSuccess(text: text)
                } else {
                    appState.setSuccess(text: "Clipboard-д хуулагдлаа. Cmd+V дарна уу")
                }
                scheduleReset(after: Constants.FlowBar.successDisplayDuration)

            } catch {
                appState.setError(message: error.localizedDescription)
                scheduleReset(after: Constants.FlowBar.errorDisplayDuration)
            }
        }
    }

    func toggleRecording() {
        if appState.status.isRecording {
            stopRecordingAndTranscribe()
        } else {
            startRecording()
        }
    }

    func cancelRecording() {
        recordingTimer?.invalidate()
        recordingTimer = nil
        _ = audioRecorder.stopRecording()
        appState.reset()
    }

    private func scheduleReset(after delay: TimeInterval) {
        Task {
            try? await Task.sleep(for: .seconds(delay))
            appState.reset()
        }
    }
}
