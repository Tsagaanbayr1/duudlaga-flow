import AppKit
import ApplicationServices

@MainActor
final class TextInsertionService {
    private var targetApp: NSRunningApplication?
    private var targetPID: pid_t?

    func captureTargetApp() {
        let app = NSWorkspace.shared.frontmostApplication
        targetApp = app
        targetPID = app?.processIdentifier
        print("[TextInsertion] Captured target: \(app?.localizedName ?? "nil") pid=\(app?.processIdentifier ?? 0)")
    }

    func insertText(_ text: String) async -> Bool {
        // Also put on clipboard as backup
        let pb = NSPasteboard.general
        pb.clearContents()
        pb.setString(text, forType: .string)

        let trusted = AXIsProcessTrusted()
        print("[TextInsertion] AXIsProcessTrusted = \(trusted)")

        guard trusted else {
            print("[TextInsertion] NOT TRUSTED")
            targetApp = nil
            targetPID = nil
            return false
        }

        // Activate the target app
        await activateTargetApp()

        // Type the text character by character using CGEvent unicode
        typeText(text)

        targetApp = nil
        targetPID = nil
        return true
    }

    // MARK: - Activate Target

    private func activateTargetApp() async {
        guard let app = targetApp, !app.isTerminated else { return }

        app.activate()
        for i in 1...30 {
            try? await Task.sleep(for: .milliseconds(50))
            if app.isActive {
                print("[TextInsertion] Activated in \(i * 50)ms")
                try? await Task.sleep(for: .milliseconds(100))
                return
            }
        }
        print("[TextInsertion] Activation timed out")
        try? await Task.sleep(for: .milliseconds(200))
    }

    // MARK: - Type text via CGEvent Unicode

    private func typeText(_ text: String) {
        let utf16 = Array(text.utf16)
        let chunkSize = 8  // Characters per keystroke event
        var offset = 0

        while offset < utf16.count {
            let end = min(offset + chunkSize, utf16.count)
            var chunk = Array(utf16[offset..<end])
            let length = chunk.count

            // Key down with unicode string
            guard let keyDown = CGEvent(keyboardEventSource: nil, virtualKey: 0, keyDown: true) else {
                print("[TextInsertion] Failed to create keyDown event")
                return
            }
            keyDown.keyboardSetUnicodeString(stringLength: length, unicodeString: &chunk)
            keyDown.post(tap: .cghidEventTap)

            // Key up
            if let keyUp = CGEvent(keyboardEventSource: nil, virtualKey: 0, keyDown: false) {
                keyUp.post(tap: .cghidEventTap)
            }

            offset = end

            // Small delay between chunks so apps can process
            if offset < utf16.count {
                usleep(5000) // 5ms
            }
        }

        print("[TextInsertion] Typed \(utf16.count) chars in \((utf16.count + chunkSize - 1) / chunkSize) chunks")
    }
}
