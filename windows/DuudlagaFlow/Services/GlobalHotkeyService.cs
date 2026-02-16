using System.Runtime.InteropServices;
using System.Windows;
using DuudlagaFlow.Models;
using DuudlagaFlow.Utilities;

namespace DuudlagaFlow.Services;

public class GlobalHotkeyService : IDisposable
{
    private IntPtr _hookId;
    private NativeMethods.LowLevelKeyboardProc? _hookProc;
    private bool _isCtrlDown;
    private bool _isAltDown;
    private bool _isHotkeyActive;

    public DictationMode DictationMode { get; set; } = DictationMode.PushToTalk;
    public bool IsRunning { get; private set; }

    public event Action? HotkeyDown;
    public event Action? HotkeyUp;
    public event Action? HotkeyToggle;

    public void Start()
    {
        if (IsRunning) return;

        _hookProc = HookCallback;
        _hookId = NativeMethods.SetWindowsHookEx(
            NativeMethods.WH_KEYBOARD_LL,
            _hookProc,
            NativeMethods.GetModuleHandle(null),
            0);

        if (_hookId == IntPtr.Zero)
        {
            System.Diagnostics.Debug.WriteLine("[Hotkey] Failed to install keyboard hook");
            return;
        }

        IsRunning = true;
        System.Diagnostics.Debug.WriteLine("[Hotkey] Keyboard hook installed");
    }

    public void Stop()
    {
        if (!IsRunning) return;

        if (_hookId != IntPtr.Zero)
        {
            NativeMethods.UnhookWindowsHookEx(_hookId);
            _hookId = IntPtr.Zero;
        }

        _hookProc = null;
        IsRunning = false;
        _isCtrlDown = false;
        _isAltDown = false;
        _isHotkeyActive = false;
        System.Diagnostics.Debug.WriteLine("[Hotkey] Keyboard hook removed");
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            int vkCode = Marshal.ReadInt32(lParam);
            int msg = wParam.ToInt32();

            bool isKeyDown = msg == NativeMethods.WM_KEYDOWN || msg == NativeMethods.WM_SYSKEYDOWN;
            bool isKeyUp = msg == NativeMethods.WM_KEYUP || msg == NativeMethods.WM_SYSKEYUP;

            bool stateChanged = false;

            switch (vkCode)
            {
                case NativeMethods.VK_LCONTROL:
                case NativeMethods.VK_RCONTROL:
                    if (isKeyDown && !_isCtrlDown) { _isCtrlDown = true; stateChanged = true; }
                    else if (isKeyUp) { _isCtrlDown = false; stateChanged = true; }
                    break;

                case NativeMethods.VK_LMENU:
                case NativeMethods.VK_RMENU:
                    if (isKeyDown && !_isAltDown) { _isAltDown = true; stateChanged = true; }
                    else if (isKeyUp) { _isAltDown = false; stateChanged = true; }
                    break;
            }

            if (stateChanged)
            {
                bool hotkeyNowActive = _isCtrlDown && _isAltDown;
                HandleHotkeyStateChange(hotkeyNowActive);
            }
        }

        return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    private void HandleHotkeyStateChange(bool hotkeyActive)
    {
        if (hotkeyActive == _isHotkeyActive) return;

        bool wasActive = _isHotkeyActive;
        _isHotkeyActive = hotkeyActive;

        // Dispatch to UI thread
        Application.Current?.Dispatcher.BeginInvoke(() =>
        {
            switch (DictationMode)
            {
                case DictationMode.PushToTalk:
                    if (hotkeyActive && !wasActive)
                        HotkeyDown?.Invoke();
                    else if (!hotkeyActive && wasActive)
                        HotkeyUp?.Invoke();
                    break;

                case DictationMode.Toggle:
                case DictationMode.HandsFree:
                    if (hotkeyActive && !wasActive)
                        HotkeyToggle?.Invoke();
                    break;
            }
        });
    }

    public void Dispose()
    {
        Stop();
    }
}
