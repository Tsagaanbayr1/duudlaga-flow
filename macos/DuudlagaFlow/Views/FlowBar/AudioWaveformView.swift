import SwiftUI

enum DotWaveformMode {
    case recording
    case processing
}

struct DotWaveformView: View {
    let levels: [Float]
    let mode: DotWaveformMode

    private let dotCount = 5
    private let dotSize: CGFloat = 4

    @State private var processingPhase: CGFloat = 0

    var body: some View {
        HStack(spacing: 6) {
            ForEach(0..<dotCount, id: \.self) { index in
                dotView(for: index)
            }
        }
        .onChange(of: mode) { _, newMode in
            if newMode == .processing {
                startProcessingAnimation()
            }
        }
        .onAppear {
            if mode == .processing {
                startProcessingAnimation()
            }
        }
    }

    @ViewBuilder
    private func dotView(for index: Int) -> some View {
        let scaleY: CGFloat = {
            switch mode {
            case .recording:
                return recordingScale(for: index)
            case .processing:
                return processingScale(for: index)
            }
        }()

        Circle()
            .fill(Color.white)
            .frame(width: dotSize, height: dotSize)
            .scaleEffect(x: 1, y: scaleY, anchor: .center)
            .clipShape(Capsule())
            .animation(
                .spring(response: 0.25, dampingFraction: 0.6),
                value: scaleY
            )
    }

    private func recordingScale(for index: Int) -> CGFloat {
        guard !levels.isEmpty else { return 0.2 }

        let level: Float
        if levels.count >= dotCount {
            let step = Float(levels.count) / Float(dotCount)
            let idx = min(Int(Float(index) * step), levels.count - 1)
            level = levels[idx]
        } else {
            level = index < levels.count ? levels[index] : 0.0
        }

        let maxLevel = levels.max() ?? 1.0
        let scale: Float = maxLevel > 0.01 ? 1.0 / maxLevel : 1.0
        let normalized = min(1.0, level * scale)

        return CGFloat(0.2 + normalized * 0.8)
    }

    private func processingScale(for index: Int) -> CGFloat {
        let offset = CGFloat(index) * 0.12
        let wave = sin((processingPhase - offset) * .pi * 2)
        let normalized = (wave + 1) / 2 // 0...1
        return 0.2 + normalized * 0.8
    }

    private func startProcessingAnimation() {
        processingPhase = 0
        withAnimation(
            .linear(duration: 0.8)
            .repeatForever(autoreverses: false)
        ) {
            processingPhase = 1
        }
    }
}
