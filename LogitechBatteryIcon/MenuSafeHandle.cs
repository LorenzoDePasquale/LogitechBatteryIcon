using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;
using Microsoft.Win32.SafeHandles;

namespace LogitechBatteryIcon;

/// <summary>
/// Represents a native handle to a <see cref="HMENU"/> resource.
/// </summary>
class MenuSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    /// <summary>
    /// Creates a new instance of the <see cref="MenuSafeHandle"/> class.
    /// </summary>
    /// <param name="menu">The native  handle to a <see cref="HMENU"/> resource.</param>
    /// <param name="ownsHandle"><inheritdoc/></param>
    MenuSafeHandle(HMENU menu, bool ownsHandle) : base(ownsHandle)
    {
        handle = menu;
    }

    public static implicit operator MenuSafeHandle(HMENU menu) => new(menu, true);

    protected override bool ReleaseHandle() => PInvoke.DestroyMenu((HMENU)handle);
}
