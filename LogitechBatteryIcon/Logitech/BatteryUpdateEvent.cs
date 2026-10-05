namespace LogitechBatteryIcon.Logitech;

/// <summary>
/// Event raised when the battery status of the mouse changes.
/// </summary>
/// <param name="Percentage">The percentage of the battery charge.</param>
/// <param name="Status">The status of the battery.</param>
readonly record struct BatteryUpdateEvent(int Percentage, ChargingStatus Status);
