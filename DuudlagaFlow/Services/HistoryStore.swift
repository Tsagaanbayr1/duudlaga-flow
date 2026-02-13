import Foundation

final class HistoryStore {
    private let fileURL: URL
    private var records: [TranscriptionRecord] = []

    init() {
        let appSupport = FileManager.default.urls(for: .applicationSupportDirectory, in: .userDomainMask).first!
        let appDirectory = appSupport.appendingPathComponent("DuudlagaFlow", isDirectory: true)

        try? FileManager.default.createDirectory(at: appDirectory, withIntermediateDirectories: true)

        self.fileURL = appDirectory.appendingPathComponent("history.json")
        loadFromDisk()
    }

    var allRecords: [TranscriptionRecord] {
        records.sorted { $0.timestamp > $1.timestamp }
    }

    func add(_ record: TranscriptionRecord) {
        records.append(record)
        saveToDisk()
    }

    func delete(_ record: TranscriptionRecord) {
        records.removeAll { $0.id == record.id }
        saveToDisk()
    }

    func deleteAll() {
        records.removeAll()
        saveToDisk()
    }

    func search(query: String) -> [TranscriptionRecord] {
        guard !query.isEmpty else { return allRecords }
        return allRecords.filter { $0.text.localizedCaseInsensitiveContains(query) }
    }

    private func loadFromDisk() {
        guard FileManager.default.fileExists(atPath: fileURL.path) else { return }
        do {
            let data = try Data(contentsOf: fileURL)
            let decoder = JSONDecoder()
            decoder.dateDecodingStrategy = .iso8601
            records = try decoder.decode([TranscriptionRecord].self, from: data)
        } catch {
            print("Failed to load history: \(error)")
        }
    }

    private func saveToDisk() {
        do {
            let encoder = JSONEncoder()
            encoder.dateEncodingStrategy = .iso8601
            encoder.outputFormatting = .prettyPrinted
            let data = try encoder.encode(records)
            try data.write(to: fileURL, options: .atomic)
        } catch {
            print("Failed to save history: \(error)")
        }
    }
}
