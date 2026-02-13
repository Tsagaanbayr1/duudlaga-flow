import Foundation

struct TranscriptionRecord: Codable, Identifiable {
    let id: UUID
    let text: String
    let timestamp: Date
    let durationSeconds: Double

    init(text: String, durationSeconds: Double) {
        self.id = UUID()
        self.text = text
        self.timestamp = Date()
        self.durationSeconds = durationSeconds
    }

    var formattedDuration: String {
        let seconds = Int(durationSeconds)
        if seconds < 60 {
            return "\(seconds)s"
        }
        let minutes = seconds / 60
        let remaining = seconds % 60
        return "\(minutes)m \(remaining)s"
    }

    var formattedTimestamp: String {
        let formatter = DateFormatter()
        formatter.dateStyle = .medium
        formatter.timeStyle = .short
        return formatter.string(from: timestamp)
    }
}
