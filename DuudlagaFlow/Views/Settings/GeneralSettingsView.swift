import SwiftUI

struct GeneralSettingsView: View {
    @Bindable var settings: AppSettings
    let permissionManager: PermissionManager
    let apiClient: ChimegeAPIClient

    @State private var showToken = false
    @State private var testStatus: TestStatus = .idle

    enum TestStatus: Equatable {
        case idle
        case testing
        case success(String)
        case failure(String)
    }

    var body: some View {
        Form {
            Section("API Тохиргоо") {
                HStack {
                    if showToken {
                        TextField("Chimege API Token", text: $settings.apiToken)
                            .textFieldStyle(.roundedBorder)
                    } else {
                        SecureField("Chimege API Token", text: $settings.apiToken)
                            .textFieldStyle(.roundedBorder)
                    }
                    Button {
                        showToken.toggle()
                    } label: {
                        Image(systemName: showToken ? "eye.slash" : "eye")
                    }
                    .buttonStyle(.borderless)
                }

                HStack {
                    Text("Token авахын тулд:")
                        .font(.caption)
                        .foregroundStyle(.secondary)
                    Link("console.chimege.com", destination: Constants.apiConsoleURL)
                        .font(.caption)
                }

                HStack {
                    Button("Token шалгах") {
                        testAPIToken()
                    }
                    .disabled(settings.apiToken.trimmingCharacters(in: .whitespacesAndNewlines).isEmpty || testStatus == .testing)

                    switch testStatus {
                    case .idle:
                        EmptyView()
                    case .testing:
                        ProgressView()
                            .controlSize(.small)
                        Text("Шалгаж байна...")
                            .font(.caption)
                            .foregroundStyle(.secondary)
                    case .success(let msg):
                        Image(systemName: "checkmark.circle.fill")
                            .foregroundStyle(.green)
                        Text(msg)
                            .font(.caption)
                            .foregroundStyle(.green)
                    case .failure(let msg):
                        Image(systemName: "xmark.circle.fill")
                            .foregroundStyle(.red)
                        Text(msg)
                            .font(.caption)
                            .foregroundStyle(.red)
                    }
                }

                Toggle("Цэг, таслал нэмэх", isOn: $settings.punctuate)
            }

            Section("Систем") {
                Toggle("Flow Bar харуулах", isOn: $settings.showFlowBar)
            }

            Section("Зөвшөөрөл") {
                HStack {
                    Image(systemName: permissionManager.isAccessibilityGranted ? "checkmark.circle.fill" : "xmark.circle.fill")
                        .foregroundStyle(permissionManager.isAccessibilityGranted ? .green : .red)
                    Text("Accessibility")
                    Spacer()
                    if !permissionManager.isAccessibilityGranted {
                        Button("Нээх (+ дарж нэмнэ үү)") {
                            permissionManager.openAccessibilitySettings()
                        }
                        .controlSize(.small)
                    }
                }

                HStack {
                    let micGranted = permissionManager.checkMicrophonePermission() == .authorized
                    Image(systemName: micGranted ? "checkmark.circle.fill" : "xmark.circle.fill")
                        .foregroundStyle(micGranted ? .green : .red)
                    Text("Микрофон")
                    Spacer()
                    if !micGranted {
                        Button("Зөвшөөрөх") {
                            permissionManager.openMicrophoneSettings()
                        }
                        .controlSize(.small)
                    }
                }
            }
        }
        .formStyle(.grouped)
        .padding()
    }

    private func testAPIToken() {
        testStatus = .testing
        Task {
            let result = await apiClient.testToken(settings.apiToken)
            switch result {
            case .success:
                testStatus = .success("Token зөв байна!")
            case .failure(let error):
                testStatus = .failure(error.localizedDescription)
            }
        }
    }
}
