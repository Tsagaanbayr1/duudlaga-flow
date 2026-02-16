import Cocoa

final class HotkeyManager {
    private var globalMonitor: Any?
    private var localMonitor: Any?
    private var isControlDown = false
    private var isOptionDown = false
    private var isActive = false
    private(set) var isRunning = false

    var onHotkeyDown: (() -> Void)?
    var onHotkeyUp: (() -> Void)?
    var onToggle: (() -> Void)?

    var dictationMode: DictationMode = .pushToTalk

    @discardableResult
    func start() -> Bool {
        stop()

        // NSEvent monitors work WITHOUT accessibility permission
        globalMonitor = NSEvent.addGlobalMonitorForEvents(matching: .flagsChanged) { [weak self] event in
            self?.handleFlags(event.modifierFlags)
        }

        localMonitor = NSEvent.addLocalMonitorForEvents(matching: .flagsChanged) { [weak self] event in
            self?.handleFlags(event.modifierFlags)
            return event
        }

        isRunning = true
        print("[HotkeyManager] NSEvent monitors started.")
        return true
    }

    func stop() {
        if let monitor = globalMonitor {
            NSEvent.removeMonitor(monitor)
            globalMonitor = nil
        }
        if let monitor = localMonitor {
            NSEvent.removeMonitor(monitor)
            localMonitor = nil
        }
        isControlDown = false
        isOptionDown = false
        isActive = false
        isRunning = false
    }

    private func handleFlags(_ flags: NSEvent.ModifierFlags) {
        let controlDown = flags.contains(.control)
        let optionDown = flags.contains(.option)

        let wasHotkeyDown = isControlDown && isOptionDown
        let isHotkeyDown = controlDown && optionDown

        isControlDown = controlDown
        isOptionDown = optionDown

        switch dictationMode {
        case .pushToTalk:
            if isHotkeyDown && !wasHotkeyDown {
                isActive = true
                onHotkeyDown?()
            } else if !isHotkeyDown && wasHotkeyDown && isActive {
                isActive = false
                onHotkeyUp?()
            }

        case .toggle, .handsFree:
            if isHotkeyDown && !wasHotkeyDown {
                onToggle?()
            }
        }
    }
}
