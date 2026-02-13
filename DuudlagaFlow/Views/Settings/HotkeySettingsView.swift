import SwiftUI

struct HotkeySettingsView: View {
    @Bindable var settings: AppSettings

    var body: some View {
        Form {
            Section("Товчлуур") {
                HStack {
                    Text("Одоогийн товчлуур:")
                    Spacer()
                    Text("⌃ Control + ⌥ Option")
                        .padding(.horizontal, 12)
                        .padding(.vertical, 6)
                        .background(.quaternary)
                        .clipShape(RoundedRectangle(cornerRadius: 6))
                        .font(.system(size: 13, weight: .medium, design: .monospaced))
                }
            }

            Section("Горим") {
                Picker("Бичлэг горим", selection: $settings.dictationMode) {
                    ForEach(DictationMode.allCases) { mode in
                        VStack(alignment: .leading) {
                            Text(mode.displayName)
                        }
                        .tag(mode)
                    }
                }
                .pickerStyle(.radioGroup)

                Text(settings.dictationMode.description)
                    .font(.caption)
                    .foregroundStyle(.secondary)
            }
        }
        .formStyle(.grouped)
        .padding()
    }
}
