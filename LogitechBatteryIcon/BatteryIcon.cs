using System.Drawing;
using System.Drawing.Drawing2D;
using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;
using Microsoft.Win32.SafeHandles;

namespace LogitechBatteryIcon;

/// <summary>
/// Represents a native handle to an <see cref="HICON"/> resource with the drawing of a battery.
/// </summary>
class BatteryIcon : SafeHandleMinusOneIsInvalid
{
    const string FONT_NAME = "Segoe UI";

    /// <summary>
    /// Creates an icon of a battery with unknown status.
    /// </summary>
    public BatteryIcon() : base(true)
    {
        using Bitmap bitmap = new(64, 64);
        using Graphics graphics = DrawBattery(bitmap);

        graphics.DrawString("?", new Font(FONT_NAME, 18), Brushes.White, 16, 4);

        handle = bitmap.GetHicon();
    }

    /// <summary>
    /// Creates  an icon of a battery with the specified charge level and status.
    /// </summary>
    /// <param name="percentage">The percentage of the battery charge.</param>
    /// <param name="charging">Whether the battery is currently being charged. If <see langword="true"/>, the icon will display a charging symbol.</param>
    public BatteryIcon(int percentage, bool charging) : base(true)
    {
        using Bitmap bitmap = new(64, 64);
        using Graphics graphics = DrawBattery(bitmap);

        DrawChargingLevel(graphics, percentage, charging);

        if (charging)
        {
            graphics.DrawString("⚡", new Font(FONT_NAME, 22, FontStyle.Bold), Brushes.White, 12, 2);
        }

        handle = bitmap.GetHicon();
    }

    /// <summary>
    /// Returns the native handle of the icon resource.
    /// </summary>
    public HICON Handle => (HICON)handle;

    protected override bool ReleaseHandle() => PInvoke.DestroyIcon((HICON)handle);

    static Graphics DrawBattery(Bitmap bitmap)
    {
        Graphics graphics = Graphics.FromImage(bitmap);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.Clear(Color.Transparent);

        // Battery body
        using Pen pen = new(Color.White, 4);
        RectangleF batteryRect = new(12, 6, 40, 56);
        graphics.DrawRoundedRectangle(pen, batteryRect, new SizeF(12, 12));

        // Battery terminal
        using SolidBrush terminalBrush = new(Color.White);
        RectangleF terminalRect = new(24, 0, 16, 6);
        graphics.FillRoundedRectangle(terminalBrush, terminalRect, new SizeF(6, 6));

        return graphics;
    }

    static void DrawChargingLevel(Graphics graphics, int percentage, bool charging)
    {
        Color color = charging
            ? Color.ForestGreen
            : percentage switch
            {
                < 20 => Color.Crimson,
                < 30 => Color.Orange,
                > 99 => Color.ForestGreen,
                _ => Color.White
            };

        using SolidBrush chargeBrush = new(color);

        float chargeHeight = 42 * percentage / 100f;
        float offset = 42 - chargeHeight;
        RectangleF chargeRect = new(17, 14 + offset, 30, chargeHeight);

        graphics.FillRoundedRectangle(chargeBrush, chargeRect, new SizeF(6, 6));
    }
}
