import SwiftUI

struct SettingsView: View {
    let settings: AppSettings
    let historyStore: HistoryStore
    let permissionManager: PermissionManager
    let appState: AppState
    let pipeline: DictationPipeline
    let apiClient: ChimegeAPIClient

    var body: some View {
        TabView {
            GeneralSettingsView(settings: settings, permissionManager: permissionManager, apiClient: apiClient)
                .tabItem {
                    Label("Ерөнхий", systemImage: "gear")
                }

            HotkeySettingsView(settings: settings)
                .tabItem {
                    Label("Товчлуур", systemImage: "keyboard")
                }

            AudioSettingsView(settings: settings, pipeline: pipeline, appState: appState)
                .tabItem {
                    Label("Аудио", systemImage: "mic")
                }

            HistoryView(historyStore: historyStore)
                .tabItem {
                    Label("Түүх", systemImage: "clock")
                }
        }
        .frame(width: 500, height: 420)
    }
}
