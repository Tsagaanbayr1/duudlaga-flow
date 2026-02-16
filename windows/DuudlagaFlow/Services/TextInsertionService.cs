using System.Runtime.InteropServices;
using System.Windows;
using DuudlagaFlow.Utilities;

namespace DuudlagaFlow.Services;

public class TextInsertionService
{
    private IntPtr _targetWindowHandle;

    public void CaptureTargetWindow()
    {
        _targetWindowHandle = NativeMethods.GetForegroundWindow();
        System.Diagnostics.Debug.WriteLine($"[TextInsertion] Captured target window: 0x{_targetWindowHandle:X}");
    }

    public async Task<bool> InsertTextAsync(string text)
    {
        // Always copy to clipboard as fallback
        try
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Clipboard.SetText(text);
            });
        }
        catch
        {
            // Clipboard may fail if locked by another app
        }

        if (_targetWindowHandle == IntPtr.Zero)
        {
            System.Diagnostics.Debug.WriteLine("[TextInsertion] No target window captured");
            _targetWindowHandle = IntPtr.Zero;
            return false;
        }

        // Re-activate the target window
        bool activated = await ActivateTargetWindowAsync();
        if (!activated)
        {
            System.Diagnostics.Debug.WriteLine("[TextInsertion] Failed to activate target window");
            _targetWindowHandle = IntPtr.Zero;
            return false;
        }

        // Type the text using SendInput with KEYEVENTF_UNICODE
        try
        {
            TypeText(text);
            System.Diagnostics.Debug.WriteLine($"[TextInsertion] Typed {text.Length} characters");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[TextInsertion] TextEntry failed: {ex.Message}");
            _targetWindowHandle = IntPtr.Zero;
            return false;
        }

        _targetWindowHandle = IntPtr.Zero;
        return true;
    }

    private static void TypeText(string text)
    {
        foreach (char c in text)
        {
            var inputs = new NativeMethods.INPUT[2];

            // Key down
            inputs[0].type = NativeMethods.INPUT_KEYBOARD;
            inputs[0].ki.wVk = 0;
            inputs[0].ki.wScan = (ushort)c;
            inputs[0].ki.dwFlags = NativeMethods.KEYEVENTF_UNICODE;
            inputs[0].ki.time = 0;
            inputs[0].ki.dwExtraInfo = IntPtr.Zero;

            // Key up
            inputs[1].type = NativeMethods.INPUT_KEYBOARD;
            inputs[1].ki.wVk = 0;
            inputs[1].ki.wScan = (ushort)c;
            inputs[1].ki.dwFlags = NativeMethods.KEYEVENTF_UNICODE | NativeMethods.KEYEVENTF_KEYUP;
            inputs[1].ki.time = 0;
            inputs[1].ki.dwExtraInfo = IntPtr.Zero;

            NativeMethods.SendInput(2, inputs, Marshal.SizeOf<NativeMethods.INPUT>());
        }
    }

    private async Task<bool> ActivateTargetWindowAsync()
    {
        if (_targetWindowHandle == IntPtr.Zero) return false;

        // Try AttachThreadInput trick for reliable SetForegroundWindow
        uint targetThreadId = NativeMethods.GetWindowThreadProcessId(_targetWindowHandle, out _);
        uint currentThreadId = NativeMethods.GetCurrentThreadId();

        bool attached = false;
        if (targetThreadId != currentThreadId)
        {
            attached = NativeMethods.AttachThreadInput(currentThreadId, targetThreadId, true);
        }

        try
        {
            NativeMethods.SetForegroundWindow(_targetWindowHandle);

            // Wait for activation (up to 1.5s)
            for (int i = 0; i < 30; i++)
            {
                await Task.Delay(50);
                if (NativeMethods.GetForegroundWindow() == _targetWindowHandle)
                {
                    System.Diagnostics.Debug.WriteLine($"[TextInsertion] Activated in {(i + 1) * 50}ms");
                    await Task.Delay(100); // Extra settle time
                    return true;
                }
            }
        }
        finally
        {
            if (attached)
            {
                NativeMethods.AttachThreadInput(currentThreadId, targetThreadId, false);
            }
        }

        System.Diagnostics.Debug.WriteLine("[TextInsertion] Activation timed out");
        await Task.Delay(200);
        return NativeMethods.GetForegroundWindow() == _targetWindowHandle;
    }
}
