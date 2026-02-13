import SwiftUI

@main
struct DuudlagaFlowApp: App {
    @NSApplicationDelegateAdaptor(AppDelegate.self) var appDelegate

    var body: some Scene {
        MenuBarExtra {
            MenuBarView(appDelegate: appDelegate)
        } label: {
            Image(systemName: appDelegate.appState?.menuBarIcon ?? "mic.circle")
        }
    }
}

struct MenuBarView: View {
    let appDelegate: AppDelegate

    var body: some View {
        if let appState = appDelegate.appState {
            switch appState.status {
            case .idle:
                Text("Бэлэн - Ctrl+Option дарна уу")
                    .font(.caption)
            case .recording:
                Text("Бичиж байна...")
                    .font(.caption)
                    .foregroundStyle(.red)
            case .processing:
                Text("Боловсруулж байна...")
                    .font(.caption)
            case .success(let text):
                Text(text)
                    .font(.caption)
                    .lineLimit(1)
            case .error(let message):
                Text(message)
                    .font(.caption)
                    .foregroundStyle(.red)
            }
        }

        Divider()

        if let settings = appDelegate.settings {
            Toggle("Flow Bar харуулах", isOn: Binding(
                get: { settings.showFlowBar },
                set: { newValue in
                    settings.showFlowBar = newValue
                    if newValue {
                        appDelegate.showFlowBar()
                    } else {
                        appDelegate.hideFlowBar()
                    }
                }
            ))
        }

        Divider()

        Button("Түүх...") {
            appDelegate.showHistoryWindow()
        }

        Button("Тохиргоо...") {
            appDelegate.showSettingsWindow()
        }
        .keyboardShortcut(",")

        if let settings = appDelegate.settings, !settings.onboardingCompleted {
            Button("Тохируулга...") {
                appDelegate.showOnboardingWindow()
            }
        }

        Divider()

        if let hotkeyManager = appDelegate.hotkeyManager {
            if hotkeyManager.isRunning {
                Label("Товчлуур: идэвхтэй", systemImage: "checkmark.circle")
            } else {
                Button("Товчлуур дахин эхлүүлэх") {
                    appDelegate.restartHotkey()
                }
            }
        }

        Divider()

        Button("Гарах") {
            NSApp.terminate(nil)
        }
        .keyboardShortcut("q")
    }
}
