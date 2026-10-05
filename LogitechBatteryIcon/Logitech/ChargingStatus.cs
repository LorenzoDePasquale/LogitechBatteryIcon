using static logi.hidppio.Feature1004;

namespace LogitechBatteryIcon.Logitech;

/// <summary>
/// Represents the charging status of the battery.
/// </summary>
enum ChargingStatus
{
    Discharging,
    Charging,
    ChargeError,
    Invalid,
}

static class ChargingStatusExtensions
{
    extension(CHARGING_STATUS status)
    {
        public ChargingStatus ToChargingStatus() =>
            status switch
            {
                CHARGING_STATUS.DISCHARING or CHARGING_STATUS.CHARGE_COMPLETE => ChargingStatus.Discharging,
                CHARGING_STATUS.CHARGING or CHARGING_STATUS.CHARGING_SLOW or CHARGING_STATUS.WIRELESS_CHARGING => ChargingStatus.Charging,
                CHARGING_STATUS.CHARGE_ERROR => ChargingStatus.ChargeError,
                _ => ChargingStatus.Invalid
            };
    }
}
