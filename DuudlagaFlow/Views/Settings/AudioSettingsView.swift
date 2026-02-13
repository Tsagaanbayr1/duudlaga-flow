import SwiftUI
import AVFoundation

struct AudioSettingsView: View {
    @Bindable var settings: AppSettings
    let pipeline: DictationPipeline
    let appState: AppState

    @State private var microphones: [AVCaptureDevice] = []
    @State private var testLevel: Float = 0
    @State private var isTesting = false

    var body: some View {
        Form {
            Section("Микрофон") {
                Picker("Микрофон сонгох", selection: $settings.selectedMicrophoneID) {
                    Text("Системийн өгөгдмөл")
                        .tag(nil as String?)

                    ForEach(microphones, id: \.uniqueID) { mic in
                        Text(mic.localizedName)
                            .tag(mic.uniqueID as String?)
                    }
                }

                Button(isTesting ? "Тест зогсоох" : "Микрофон тест") {
                    if isTesting {
                        pipeline.cancelRecording()
                        isTesting = false
                    } else {
                        pipeline.startRecording()
                        isTesting = true
                    }
                }
                .controlSize(.small)

                if isTesting {
                    ProgressView(value: Double(appState.audioLevel), total: 0.5)
                        .tint(.green)
                    Text("Ярьж байна уу? Дээрх шугам хөдлөх ёстой.")
                        .font(.caption)
                        .foregroundStyle(.secondary)
                }
            }
        }
        .formStyle(.grouped)
        .padding()
        .onAppear {
            loadMicrophones()
        }
    }

    private func loadMicrophones() {
        microphones = AVCaptureDevice.DiscoverySession(
            deviceTypes: [.microphone],
            mediaType: .audio,
            position: .unspecified
        ).devices
    }
}
