import AppKit
import ServiceManagement
import SwiftUI

@MainActor
final class AppDelegate: NSObject, NSApplicationDelegate {
    var appState: AppState!
    var settings: AppSettings!
    var audioRecorder: AudioRecorder!
    var apiClient: ChimegeAPIClient!
    var textInsertion: TextInsertionService!
    var historyStore: HistoryStore!
    var hotkeyManager: HotkeyManager!
    var permissionManager: PermissionManager!
    var pipeline: DictationPipeline!

    var flowBarPanel: FlowBarPanel?
    private var settingsWindow: NSWindow?
    private var historyWindow: NSWindow?
    private var onboardingWindow: NSWindow?
    private var accessibilityTimer: Timer?

    func applicationDidFinishLaunching(_ notification: Notification) {
        appState = AppState()
        settings = AppSettings()
        audioRecorder = AudioRecorder()
        apiClient = ChimegeAPIClient()
        textInsertion = TextInsertionService()
        historyStore = HistoryStore()
        hotkeyManager = HotkeyManager()
        permissionManager = PermissionManager()

        pipeline = DictationPipeline(
            appState: appState,
            settings: settings,
            audioRecorder: audioRecorder,
            apiClient: apiClient,
            textInsertion: textInsertion,
            historyStore: historyStore
        )

        setupFlowBar()
        requestMicrophoneIfNeeded()
        startHotkeyWhenReady()

        if !settings.onboardingCompleted {
            DispatchQueue.main.asyncAfter(deadline: .now() + 0.5) {
                self.showOnboardingWindow()
            }
        }
    }

    // MARK: - Hotkey

    private func startHotkeyWhenReady() {
        hotkeyManager.dictationMode = settings.dictationMode

        hotkeyManager.onHotkeyDown = { [weak self] in
            Task { @MainActor in
                self?.pipeline.startRecording()
            }
        }

        hotkeyManager.onHotkeyUp = { [weak self] in
            Task { @MainActor in
                self?.pipeline.stopRecordingAndTranscribe()
            }
        }

        hotkeyManager.onToggle = { [weak self] in
            Task { @MainActor in
                self?.pipeline.toggleRecording()
            }
        }

        if hotkeyManager.start() {
            print("[AppDelegate] Hotkey started on launch.")
        } else {
            print("[AppDelegate] Hotkey failed, polling for accessibility...")
            // Poll for accessibility permission
            accessibilityTimer = Timer.scheduledTimer(withTimeInterval: 2.0, repeats: true) { [weak self] _ in
                Task { @MainActor in
                    guard let self = self else { return }
                    if self.hotkeyManager.start() {
                        self.accessibilityTimer?.invalidate()
                        self.accessibilityTimer = nil
                        print("[AppDelegate] Hotkey started after accessibility granted.")
                    }
                }
            }
        }
    }

    func restartHotkey() {
        hotkeyManager.stop()
        hotkeyManager.start()
    }

    // MARK: - Flow Bar

    func setupFlowBar() {
        guard settings.showFlowBar else { return }
        let panel = FlowBarPanel(appState: appState, pipeline: pipeline)
        panel.orderFront(nil)
        self.flowBarPanel = panel
    }

    func showFlowBar() {
        if flowBarPanel == nil {
            let panel = FlowBarPanel(appState: appState, pipeline: pipeline)
            self.flowBarPanel = panel
        }
        flowBarPanel?.orderFront(nil)
    }

    func hideFlowBar() {
        flowBarPanel?.orderOut(nil)
    }

    // MARK: - Settings Window

    func showSettingsWindow() {
        if let window = settingsWindow {
            window.makeKeyAndOrderFront(nil)
            NSApp.activate(ignoringOtherApps: true)
            return
        }

        let settingsView = SettingsView(
            settings: settings,
            historyStore: historyStore,
            permissionManager: permissionManager,
            appState: appState,
            pipeline: pipeline,
            apiClient: apiClient
        )
        let hostingController = NSHostingController(rootView: settingsView)

        let window = NSWindow(contentViewController: hostingController)
        window.title = "Тохиргоо"
        window.setContentSize(NSSize(width: 500, height: 420))
        window.styleMask = [.titled, .closable]
        window.center()
        window.delegate = self
        window.isReleasedWhenClosed = false

        self.settingsWindow = window
        window.makeKeyAndOrderFront(nil)
        NSApp.activate(ignoringOtherApps: true)
    }

    // MARK: - History Window

    func showHistoryWindow() {
        if let window = historyWindow {
            window.makeKeyAndOrderFront(nil)
            NSApp.activate(ignoringOtherApps: true)
            return
        }

        let historyView = HistoryView(historyStore: historyStore)
        let hostingController = NSHostingController(rootView: historyView)

        let window = NSWindow(contentViewController: hostingController)
        window.title = "Түүх"
        window.setContentSize(NSSize(width: 480, height: 500))
        window.styleMask = [.titled, .closable, .resizable, .miniaturizable]
        window.center()
        window.delegate = self
        window.isReleasedWhenClosed = false

        self.historyWindow = window
        window.makeKeyAndOrderFront(nil)
        NSApp.activate(ignoringOtherApps: true)
    }

    // MARK: - Onboarding Window

    func showOnboardingWindow() {
        if let window = onboardingWindow {
            window.makeKeyAndOrderFront(nil)
            NSApp.activate(ignoringOtherApps: true)
            return
        }

        let onboardingView = OnboardingView(
            settings: settings,
            permissionManager: permissionManager,
            appState: appState,
            pipeline: pipeline,
            onComplete: { [weak self] in
                self?.onboardingWindow?.close()
                self?.onboardingWindow = nil
            }
        )
        let hostingController = NSHostingController(rootView: onboardingView)

        let window = NSWindow(contentViewController: hostingController)
        window.title = "Дуудлага Flow - Тохируулга"
        window.setContentSize(NSSize(width: 520, height: 600))
        window.styleMask = [.titled, .closable]
        window.center()
        window.delegate = self
        window.isReleasedWhenClosed = false

        self.onboardingWindow = window
        window.makeKeyAndOrderFront(nil)
        NSApp.activate(ignoringOtherApps: true)
    }

    // MARK: - Permissions

    private func requestMicrophoneIfNeeded() {
        Task {
            let micStatus = permissionManager.checkMicrophonePermission()
            if micStatus == .notDetermined {
                _ = await permissionManager.requestMicrophonePermission()
            }
        }
    }

    func applicationWillTerminate(_ notification: Notification) {
        accessibilityTimer?.invalidate()
        hotkeyManager.stop()
    }
}

extension AppDelegate: NSWindowDelegate {
    func windowWillClose(_ notification: Notification) {
        guard let window = notification.object as? NSWindow else { return }
        if window === settingsWindow {
            settingsWindow = nil
        } else if window === historyWindow {
            historyWindow = nil
        } else if window === onboardingWindow {
            onboardingWindow = nil
        }
    }
}
