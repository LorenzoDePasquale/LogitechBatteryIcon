using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using LogitechBatteryIcon.Logitech;

namespace LogitechBatteryIcon;

static class Program
{
    const nuint TRAY_EXIT_MENU_ID = 1001;

    static TrayIcon _trayIcon = null!;
    static BatteryIcon _batteryIcon = null!;
    static long _timeSinceLastNotification;

    static void Main()
    {
        HWND window = CreateWindow();
        _batteryIcon = new BatteryIcon();
        _trayIcon = new TrayIcon(window, "Waiting for a Logitech mouse to connect...", _batteryIcon.Handle);

        LogitechMouse mouse = new();
        mouse.BatteryUpdated += OnBatteryUpdated;

        RunMessagePump();

        mouse.Dispose();
        Dispose();
    }

    static void RunMessagePump()
    {
        while (PInvoke.GetMessage(out MSG msg, HWND.Null, 0, 0))
        {
            PInvoke.TranslateMessage(msg);
            PInvoke.DispatchMessage(msg);
        }
    }

    static void OnBatteryUpdated(object? _, BatteryUpdateEvent e)
    {
        BatteryIcon icon = e.Status == ChargingStatus.Invalid ? new BatteryIcon() : new BatteryIcon(e.Percentage, e.Status == ChargingStatus.Charging);
        _trayIcon.Update($"{e.Percentage}% - {e.Status}", icon.Handle);
        _batteryIcon.Dispose();
        _batteryIcon = icon;

        TimeSpan t = Stopwatch.GetElapsedTime(_timeSinceLastNotification);

        // Notify the user if the battery is low, no more than once per hour
        if (e.Status != ChargingStatus.Invalid && e.Percentage < 15 && t.TotalHours > 1)
        {
            _trayIcon.ShowNotification("Low mouse battery", $"Your Logitech mouse battery is low ({e.Percentage}%)");
            _timeSinceLastNotification = Stopwatch.GetTimestamp();
        }
    }

    static unsafe HWND CreateWindow()
    {
        fixed (char* classNamePtr = "Logitech battery icon")
        {
            PCWSTR className = new(classNamePtr);

            WNDCLASSEXW windowClass = new()
            {
                cbSize = (uint)Marshal.SizeOf<WNDCLASSEXW>(),
                hInstance = (HINSTANCE)Marshal.GetHINSTANCE(typeof(Program).Module),
                lpszClassName = className,
                lpfnWndProc = WindowProcedure
            };

            PInvoke.RegisterClassEx(windowClass);

            return PInvoke.CreateWindowEx(
                dwExStyle: WINDOW_EX_STYLE.WS_EX_LEFT,
                dwStyle: WINDOW_STYLE.WS_OVERLAPPED,
                lpClassName: className,
                X: 0,
                Y: 0,
                nWidth: 0,
                nHeight: 0);
        }
    }

    static LRESULT WindowProcedure(HWND hWnd, uint messageId, WPARAM wParam, LPARAM lParam)
    {
        switch (messageId)
        {
            // User right-clicked on the tray icon
            case TrayIcon.WindowMessageId when (uint)lParam.Value == PInvoke.WM_RBUTTONUP:
                SpawnPopupMenu(hWnd);
                return new LRESULT();

            // User clicked on the exit menu
            case PInvoke.WM_COMMAND when wParam.Value == TRAY_EXIT_MENU_ID:
                Dispose();
                Environment.Exit(0);
                return new LRESULT();

            // Windows removed the icon
            case PInvoke.WM_DESTROY:
                Dispose();
                return new LRESULT();

            default:
                return PInvoke.DefWindowProc(hWnd, messageId, wParam, lParam);
        }
    }

    static void SpawnPopupMenu(HWND hWnd)
    {
        PInvoke.SetForegroundWindow(hWnd);

        using MenuSafeHandle menu = PInvoke.CreatePopupMenu();
        PInvoke.AppendMenu(menu, MENU_ITEM_FLAGS.MF_STRING, TRAY_EXIT_MENU_ID, "Exit");
        PInvoke.GetCursorPos(out Point point);
        PInvoke.TrackPopupMenu(menu, TRACK_POPUP_MENU_FLAGS.TPM_RIGHTBUTTON, point.X, point.Y, hWnd);
    }

    static void Dispose()
    {
        _trayIcon.Dispose();
        _batteryIcon.Dispose();
    }
}
