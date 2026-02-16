import SwiftUI

struct HistoryRowView: View {
    let record: TranscriptionRecord

    var body: some View {
        VStack(alignment: .leading, spacing: 4) {
            HStack {
                Text(record.formattedTimestamp)
                    .font(.caption)
                    .foregroundStyle(.secondary)
                Spacer()
                Text(record.formattedDuration)
                    .font(.caption2)
                    .padding(.horizontal, 6)
                    .padding(.vertical, 2)
                    .background(.blue.opacity(0.15))
                    .foregroundStyle(.blue)
                    .clipShape(Capsule())
            }

            Text(record.text)
                .font(.body)
                .lineLimit(2)
                .truncationMode(.tail)
        }
        .padding(.vertical, 4)
    }
}
