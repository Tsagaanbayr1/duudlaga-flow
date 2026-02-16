import SwiftUI

struct OnboardingView: View {
    @Bindable var settings: AppSettings
    let permissionManager: PermissionManager
    let appState: AppState
    let pipeline: DictationPipeline
    var onComplete: (() -> Void)?

    @State private var currentStep = 0
    @State private var testResult: String?
    @State private var isTesting = false

    private let totalSteps = 4

    var body: some View {
        VStack(spacing: 0) {
            // Progress
            HStack(spacing: 4) {
                ForEach(0..<totalSteps, id: \.self) { step in
                    Capsule()
                        .fill(step <= currentStep ? Color.accentColor : Color.secondary.opacity(0.3))
                        .frame(height: 4)
                }
            }
            .padding(.horizontal, 32)
            .padding(.top, 20)

            Spacer()

            // Content
            Group {
                switch currentStep {
                case 0: welcomeStep
                case 1: apiTokenStep
                case 2: accessibilityStep
                case 3: microphoneStep
                default: completeStep
                }
            }
            .transition(.asymmetric(
                insertion: .move(edge: .trailing).combined(with: .opacity),
                removal: .move(edge: .leading).combined(with: .opacity)
            ))

            Spacer()

            // Navigation
            HStack {
                if currentStep > 0 {
                    Button("Буцах") {
                        withAnimation { currentStep -= 1 }
                    }
                    .controlSize(.large)
                }

                Spacer()

                if currentStep < totalSteps - 1 {
                    Button("Дараагийх") {
                        withAnimation { currentStep += 1 }
                    }
                    .controlSize(.large)
                    .buttonStyle(.borderedProminent)
                    .disabled(!canProceed)
                } else {
                    Button("Дуусгах") {
                        settings.onboardingCompleted = true
                        onComplete?()
                    }
                    .controlSize(.large)
                    .buttonStyle(.borderedProminent)
                }
            }
            .padding(32)
        }
        .frame(minWidth: 480, minHeight: 520)
    }

    private var canProceed: Bool {
        switch currentStep {
        case 1: return settings.hasValidToken
        default: return true
        }
    }

    // MARK: - Steps

    private var welcomeStep: some View {
        VStack(spacing: 16) {
            Image(systemName: "mic.circle.fill")
                .font(.system(size: 64))
                .foregroundStyle(.blue)

            Text("Дуудлага Flow")
                .font(.largeTitle.weight(.bold))

            Text("Монгол хэл дээрх яриаг текст болгон хөрвүүлэх\nмакОС програм")
                .multilineTextAlignment(.center)
                .foregroundStyle(.secondary)

            VStack(alignment: .leading, spacing: 12) {
                featureRow(icon: "keyboard", text: "Ctrl+Option дарж ярина уу")
                featureRow(icon: "text.cursor", text: "Текст автоматаар курсорын байрлалд бичигдэнэ")
                featureRow(icon: "clock", text: "Бүх бичлэгийн түүх хадгалагдана")
            }
            .padding(.top, 16)
        }
        .padding(32)
    }

    private var apiTokenStep: some View {
        VStack(spacing: 16) {
            Image(systemName: "key.fill")
                .font(.system(size: 48))
                .foregroundStyle(.orange)

            Text("API Token оруулна уу")
                .font(.title2.weight(.semibold))

            Text("Chimege API-г ашиглахын тулд token шаардлагатай.")
                .foregroundStyle(.secondary)
                .multilineTextAlignment(.center)

            SecureField("API Token", text: $settings.apiToken)
                .textFieldStyle(.roundedBorder)
                .frame(maxWidth: 360)

            Link("Token авах →", destination: Constants.apiConsoleURL)
                .font(.callout)

            if settings.hasValidToken {
                Label("Token оруулсан", systemImage: "checkmark.circle.fill")
                    .foregroundStyle(.green)
            }
        }
        .padding(32)
    }

    private var accessibilityStep: some View {
        VStack(spacing: 16) {
            Image(systemName: "hand.raised.fill")
                .font(.system(size: 48))
                .foregroundStyle(.purple)

            Text("Accessibility зөвшөөрөл")
                .font(.title2.weight(.semibold))

            Text("Товчлуур сонсох, текст оруулахын тулд Accessibility зөвшөөрөл шаардлагатай.")
                .foregroundStyle(.secondary)
                .multilineTextAlignment(.center)

            HStack {
                Image(systemName: permissionManager.isAccessibilityGranted ? "checkmark.circle.fill" : "xmark.circle.fill")
                    .foregroundStyle(permissionManager.isAccessibilityGranted ? .green : .red)
                Text(permissionManager.isAccessibilityGranted ? "Зөвшөөрөгдсөн" : "Зөвшөөрөгдөөгүй")
            }
            .font(.headline)

            if !permissionManager.isAccessibilityGranted {
                VStack(alignment: .leading, spacing: 8) {
                    Text("Дараах алхмуудыг дагана уу:")
                        .font(.callout.weight(.medium))
                    HStack(alignment: .top, spacing: 8) {
                        Text("1.")
                            .font(.callout.weight(.semibold))
                        Text("Доорх товчийг дарж Privacy & Security > Accessibility хэсгийг нээнэ үү")
                            .font(.callout)
                    }
                    HStack(alignment: .top, spacing: 8) {
                        Text("2.")
                            .font(.callout.weight(.semibold))
                        Text("Зүүн доод булангийн \"+\" товчийг дарна уу")
                            .font(.callout)
                    }
                    HStack(alignment: .top, spacing: 8) {
                        Text("3.")
                            .font(.callout.weight(.semibold))
                        Text("Applications хавтаснаас \"Duudlaga Flow\" програмыг сонгоно уу")
                            .font(.callout)
                    }
                    HStack(alignment: .top, spacing: 8) {
                        Text("4.")
                            .font(.callout.weight(.semibold))
                        Text("Toggle-ийг асаана уу")
                            .font(.callout)
                    }
                }
                .padding()
                .background(.quaternary.opacity(0.5))
                .clipShape(RoundedRectangle(cornerRadius: 8))

                Button("Accessibility тохиргоо нээх") {
                    permissionManager.openAccessibilitySettings()
                }
                .controlSize(.large)
                .buttonStyle(.borderedProminent)
            }
        }
        .padding(32)
    }

    private var microphoneStep: some View {
        VStack(spacing: 16) {
            Image(systemName: "mic.fill")
                .font(.system(size: 48))
                .foregroundStyle(.blue)

            Text("Микрофон зөвшөөрөл")
                .font(.title2.weight(.semibold))

            Text("Дуу хоолойг бичихийн тулд микрофоны зөвшөөрөл шаардлагатай.")
                .foregroundStyle(.secondary)
                .multilineTextAlignment(.center)

            let micStatus = permissionManager.checkMicrophonePermission()

            HStack {
                Image(systemName: micStatus == .authorized ? "checkmark.circle.fill" : "xmark.circle.fill")
                    .foregroundStyle(micStatus == .authorized ? .green : .red)
                Text(micStatus == .authorized ? "Зөвшөөрөгдсөн" : "Зөвшөөрөгдөөгүй")
            }
            .font(.headline)

            if micStatus != .authorized {
                Button("Микрофон зөвшөөрөх") {
                    Task {
                        _ = await permissionManager.requestMicrophonePermission()
                    }
                }
                .controlSize(.large)
                .buttonStyle(.bordered)
            }
        }
        .padding(32)
    }

    private var completeStep: some View {
        VStack(spacing: 16) {
            Image(systemName: "checkmark.circle.fill")
                .font(.system(size: 64))
                .foregroundStyle(.green)

            Text("Бэлэн боллоо!")
                .font(.largeTitle.weight(.bold))

            Text("Та одоо ямар нэгэн текст талбарт Ctrl+Option дарж монголоор ярьж болно.")
                .multilineTextAlignment(.center)
                .foregroundStyle(.secondary)
        }
        .padding(32)
    }

    // MARK: - Helpers

    private func featureRow(icon: String, text: String) -> some View {
        HStack(spacing: 12) {
            Image(systemName: icon)
                .frame(width: 24)
                .foregroundStyle(.blue)
            Text(text)
                .font(.callout)
        }
    }
}
