import AppKit
import SwiftUI

final class FlowBarPanel: NSPanel {
    init(appState: AppState, pipeline: DictationPipeline) {
        let contentRect = NSRect(
            x: 0, y: 0,
            width: Constants.FlowBar.errorWidth,
            height: Constants.FlowBar.expandedHeight
        )

        super.init(
            contentRect: contentRect,
            styleMask: [.borderless, .nonactivatingPanel],
            backing: .buffered,
            defer: false
        )

        self.level = .floating
        self.isOpaque = false
        self.backgroundColor = .clear
        self.hasShadow = false
        self.collectionBehavior = [.canJoinAllSpaces, .stationary, .fullScreenAuxiliary]
        self.hidesOnDeactivate = false
        self.isMovableByWindowBackground = false
        self.animationBehavior = .utilityWindow

        let hostingView = NSHostingView(rootView: FlowBarView(appState: appState, pipeline: pipeline))
        self.contentView = hostingView

        positionAtBottom()

        NotificationCenter.default.addObserver(
            self,
            selector: #selector(screenDidChange),
            name: NSApplication.didChangeScreenParametersNotification,
            object: nil
        )
    }

    func positionAtBottom() {
        guard let screen = NSScreen.main else { return }
        let screenFrame = screen.visibleFrame
        let x = screenFrame.midX - frame.width / 2
        let y = screenFrame.minY + Constants.FlowBar.bottomPadding
        setFrameOrigin(NSPoint(x: x, y: y))
    }

    @objc private func screenDidChange() {
        positionAtBottom()
    }

    override var canBecomeKey: Bool { false }
    override var canBecomeMain: Bool { false }
}
