using System;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.WindowsAndMessaging;

namespace LogitechBatteryIcon;

/// <summary>
/// Shows an icon in the system tray.
/// </summary>
class TrayIcon : IDisposable
{
    /// <summary>
    /// The message ID used to send messages to the window.
    /// This message is sent to the window procedure of the window that was passed to the <see cref="TrayIcon"/> constructor.
    /// </summary>
    public const uint WindowMessageId = PInvoke.WM_USER + 1;

    NOTIFYICONDATAW _iconData;

    /// <summary>
    /// Creates a new instance of <see cref="TrayIcon"/>, and adds the icon to the system tray.
    /// </summary>
    /// <param name="window">A handle to the window that receives notifications associated with this icon.</param>
    /// <param name="tooltip">The tooltip to show when the mouse is over the icon.</param>
    /// <param name="icon"></param>
    public TrayIcon(HWND window, string tooltip, HICON icon)
    {
        _iconData = new NOTIFYICONDATAW
        {
            cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATAW>(),
            hIcon = icon,
            hWnd = window,
            szTip = tooltip,
            uCallbackMessage = WindowMessageId,
            uFlags = NOTIFY_ICON_DATA_FLAGS.NIF_GUID | NOTIFY_ICON_DATA_FLAGS.NIF_MESSAGE | NOTIFY_ICON_DATA_FLAGS.NIF_ICON | NOTIFY_ICON_DATA_FLAGS.NIF_TIP,
            guidItem = Guid.Parse("CFAF5D32-8663-4356-9903-7A89DFDB0175")
        };

        PInvoke.Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_ADD, _iconData);
    }

    /// <summary>
    /// Changes the tooltip and icon.
    /// </summary>
    /// <param name="tooltip">The new tooltip.</param>
    /// <param name="icon">A handle to the new icon. The caller should close the handle to the previous icon.</param>
    public void Update(string tooltip, HICON icon)
    {
        _iconData.szTip = tooltip;
        _iconData.hIcon = icon;
        PInvoke.Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_MODIFY, _iconData);
    }

    /// <summary>
    /// Shows a notification to the user.
    /// </summary>
    /// <param name="title">Title of the notification toast.</param>
    /// <param name="message">Text of the notification toast.</param>
    public void ShowNotification(string title, string message)
    {
        _iconData.uFlags |= NOTIFY_ICON_DATA_FLAGS.NIF_INFO;
        _iconData.szInfoTitle = title;
        _iconData.szInfo = message;
        PInvoke.Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_MODIFY, _iconData);
        _iconData.uFlags &= ~NOTIFY_ICON_DATA_FLAGS.NIF_INFO;
    }

    /// <summary>
    /// Removes the icon from the system tray.
    /// If this method is not called, the icon will persist after the application exits, until the user moves the mouse over the system tray.
    /// </summary>
    public void Dispose() => PInvoke.Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_DELETE, _iconData);
}
