import SwiftUI

struct FlowBarView: View {
    let appState: AppState
    let pipeline: DictationPipeline

    @State private var isHovered = false

    var body: some View {
        ZStack {
            backgroundCapsule
            contentForState
        }
        .frame(width: barWidth, height: barHeight)
        .clipShape(Capsule())
        .shadow(color: shadowColor, radius: shadowRadius, y: 1)
        .onHover { isHovered = $0 }
        .onTapGesture {
            pipeline.toggleRecording()
        }
        .contextMenu {
            contextMenuItems
        }
        .animation(.spring(response: 0.3, dampingFraction: 0.75), value: appState.status)
    }

    // MARK: - Background

    @ViewBuilder
    private var backgroundCapsule: some View {
        switch appState.status {
        case .idle:
            Capsule()
                .fill(Color.black.opacity(0.35))
                .overlay(Capsule().stroke(Color.white.opacity(0.08), lineWidth: 0.5))

        case .recording:
            Capsule()
                .fill(
                    LinearGradient(
                        colors: [Color.blue, Color.indigo],
                        startPoint: .leading,
                        endPoint: .trailing
                    )
                )

        case .processing:
            Capsule()
                .fill(
                    LinearGradient(
                        colors: [Color.purple, Color.indigo],
                        startPoint: .leading,
                        endPoint: .trailing
                    )
                )

        case .success:
            Capsule()
                .fill(Color.green.opacity(0.9))

        case .error:
            Capsule()
                .fill(Color.red.opacity(0.85))
        }
    }

    // MARK: - Content

    @ViewBuilder
    private var contentForState: some View {
        switch appState.status {
        case .idle, .success:
            EmptyView()

        case .recording:
            DotWaveformView(levels: appState.audioLevels, mode: .recording)

        case .processing:
            DotWaveformView(levels: [], mode: .processing)

        case .error(let message):
            Text(message)
                .font(.system(size: 11, weight: .medium))
                .foregroundStyle(.white)
                .lineLimit(1)
                .truncationMode(.tail)
                .padding(.horizontal, 12)
        }
    }

    // MARK: - Context Menu

    @ViewBuilder
    private var contextMenuItems: some View {
        Button("Тохиргоо...") {
            if #available(macOS 14.0, *) {
                NSApp.activate()
            } else {
                NSApp.activate(ignoringOtherApps: true)
            }
            NSApp.sendAction(Selector(("showSettingsWindow:")), to: nil, from: nil)
        }
        Divider()
        Button("Гарах") {
            NSApp.terminate(nil)
        }
    }

    // MARK: - Computed Properties

    private var barWidth: CGFloat {
        switch appState.status {
        case .idle, .success:
            return Constants.FlowBar.width
        case .recording, .processing:
            return Constants.FlowBar.expandedWidth
        case .error:
            return Constants.FlowBar.errorWidth
        }
    }

    private var barHeight: CGFloat {
        switch appState.status {
        case .idle, .success:
            return Constants.FlowBar.height
        case .recording, .processing:
            return Constants.FlowBar.expandedHeight
        case .error:
            return 24
        }
    }

    private var shadowColor: Color {
        switch appState.status {
        case .recording: return .blue.opacity(0.3)
        case .processing: return .purple.opacity(0.3)
        case .success: return .green.opacity(0.4)
        case .error: return .red.opacity(0.3)
        default: return .clear
        }
    }

    private var shadowRadius: CGFloat {
        switch appState.status {
        case .idle: return 0
        default: return 6
        }
    }
}
