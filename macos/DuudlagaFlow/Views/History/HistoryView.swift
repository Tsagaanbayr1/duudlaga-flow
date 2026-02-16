import SwiftUI

struct HistoryView: View {
    let historyStore: HistoryStore
    @State private var searchText = ""
    @State private var records: [TranscriptionRecord] = []

    var body: some View {
        VStack(spacing: 0) {
            // Search bar
            HStack {
                Image(systemName: "magnifyingglass")
                    .foregroundStyle(.secondary)
                TextField("Хайх...", text: $searchText)
                    .textFieldStyle(.plain)
                if !searchText.isEmpty {
                    Button {
                        searchText = ""
                    } label: {
                        Image(systemName: "xmark.circle.fill")
                            .foregroundStyle(.secondary)
                    }
                    .buttonStyle(.plain)
                }
            }
            .padding(8)
            .background(.quaternary)
            .clipShape(RoundedRectangle(cornerRadius: 8))
            .padding(.horizontal)
            .padding(.top, 8)

            if filteredRecords.isEmpty {
                ContentUnavailableView {
                    Label("Түүх хоосон", systemImage: "clock")
                } description: {
                    if searchText.isEmpty {
                        Text("Та Ctrl+Option дарж ярьснаар энд хадгалагдана.")
                    } else {
                        Text("'\(searchText)' хайлтад тохирох үр дүн олдсонгүй.")
                    }
                }
            } else {
                List {
                    ForEach(filteredRecords) { record in
                        HistoryRowView(record: record)
                            .contextMenu {
                                Button("Хуулах") {
                                    NSPasteboard.general.clearContents()
                                    NSPasteboard.general.setString(record.text, forType: .string)
                                }
                                Divider()
                                Button("Устгах", role: .destructive) {
                                    historyStore.delete(record)
                                    loadRecords()
                                }
                            }
                    }
                }
                .listStyle(.inset)
            }

            // Footer
            HStack {
                Text("\(filteredRecords.count) бичлэг")
                    .font(.caption)
                    .foregroundStyle(.secondary)
                Spacer()
                if !records.isEmpty {
                    Button("Бүгдийг устгах", role: .destructive) {
                        historyStore.deleteAll()
                        loadRecords()
                    }
                    .controlSize(.small)
                    .font(.caption)
                }
            }
            .padding(.horizontal)
            .padding(.vertical, 8)
        }
        .onAppear {
            loadRecords()
        }
    }

    private var filteredRecords: [TranscriptionRecord] {
        if searchText.isEmpty {
            return records
        }
        return records.filter { $0.text.localizedCaseInsensitiveContains(searchText) }
    }

    private func loadRecords() {
        records = historyStore.allRecords
    }
}
