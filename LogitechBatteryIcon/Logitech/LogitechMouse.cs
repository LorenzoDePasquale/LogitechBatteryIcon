using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using logi.hidppio;
using static logi.hidppio.Feature1004;

namespace LogitechBatteryIcon.Logitech;

/// <summary>
/// Represents a Logitech mouse device.
/// </summary>
class LogitechMouse : IDisposable
{
    readonly List<HidppDevice> _devices = [];
    readonly HidppDeviceProvider _deviceProviderInstance;
    readonly Action<BatteryStatus>? _batteryStatusBroadcastListener;
    readonly Lock _lock = new();
    HidppDevice? _currentDevice;
    Feature1004? _currentFeature;

    /// <summary>
    /// Raised when the mouse reports a battery update.
    /// </summary>
    public event EventHandler<BatteryUpdateEvent>? BatteryUpdated;

    /// <summary>
    /// Creates a new instance of <see cref="LogitechMouse"/> and starts listening to connected devices.
    /// </summary>
    public LogitechMouse()
    {
        _batteryStatusBroadcastListener = broadcast =>
        {
            BatteryUpdateEvent updateEvent = new(broadcast.state_of_charge, broadcast.charging_status.ToChargingStatus());
            BatteryUpdated?.Invoke(this, updateEvent);
        };

        _deviceProviderInstance = new HidppDeviceProvider();
        _deviceProviderInstance.DeviceAdded += dev => OnDeviceAdded((HidppDevice)dev);
        _deviceProviderInstance.DeviceRemoved += dev => OnDeviceRemoved((HidppDevice)dev);
        _deviceProviderInstance.Start();
    }

    void OnDeviceAdded(HidppDevice device)
    {
        lock (_lock)
        {
            _devices.Add(device);
        }

        device.ConnectionChanged += OnDeviceOnConnectionChanged;
        OnConnectionChanged();
    }

    void OnDeviceOnConnectionChanged(DeviceBase dev, bool b) => OnConnectionChanged();

    void OnDeviceRemoved(HidppDevice device)
    {
        device.ConnectionChanged -= OnDeviceOnConnectionChanged;
        int removedCount;

        lock (_lock)
        {
            removedCount = _devices.RemoveAll(d => d.Id == device.Id);
        }

        if (removedCount > 0)
        {
            OnConnectionChanged();
        }
    }

    void OnConnectionChanged()
    {
        const int RETRY_INTERVAL_MS = 100;
        const int TIMEOUT_MS = 5000;

        long deadline = Environment.TickCount64 + TIMEOUT_MS;
        bool reportedUnavailable = false;

        // Try connecting until success or timeout
        while (!TryConnect(ref reportedUnavailable))
        {
            long remainingMs = deadline - Environment.TickCount64;

            if (remainingMs <= 0)
            {
                // Too many connection attempts
                return;
            }

            Thread.Sleep((int)Math.Min(RETRY_INTERVAL_MS, remainingMs));
        }
    }

    // Returns true when no further retries are needed.
    bool TryConnect(ref bool reportedUnavailable)
    {
        lock (_lock)
        {
            if (IsSelectedMouseReady())
            {
                // Already connected
                return true;
            }

            if (!reportedUnavailable)
            {
                // Report that the mouse is unavailable
                reportedUnavailable = true;
                BatteryUpdateEvent batteryUpdateEvent = new(-1, CHARGING_STATUS.INVALID.ToChargingStatus());
                BatteryUpdated?.Invoke(this, batteryUpdateEvent);
            }

            // Only query DeviceType on connected devices.
            HidppDevice? newMouse = _devices.FirstOrDefault(m => m.IsConnected && m.DeviceType == BaseDeviceType.MOUSE);
            SetSelectedMouse(newMouse);

            return newMouse is null || IsSelectedMouseReady();
        }
    }

    bool IsSelectedMouseReady() => _currentDevice is not null &&
                                   _currentFeature is not null &&
                                   _currentDevice.IsConnected &&
                                   _devices.Any(m => m.Id == _currentDevice.Id);

    void SetSelectedMouse(HidppDevice? device)
    {
        UnsubscribeToBatteryEvents();

        try
        {
            _currentDevice?.Dispose();
        }
        catch
        {
            // Sometimes disposing the device throws an exception. (System.ObjectDisposedException: The CancellationTokenSource has been disposed)
            // It could be that we are disposing the same device multiple times, or a bug inside the Logitech library
        }

        _currentDevice = device;

        if (device is not null)
        {
            SubscribeToBatteryEvents(device);
        }
    }

    void SubscribeToBatteryEvents(HidppDevice device)
    {
        try
        {
            var feature = device.GetFeature<Feature1004>();
            feature.BatteryStatusBroadcast += _batteryStatusBroadcastListener;
            _currentFeature?.Dispose();
            _currentFeature = feature;

            BatteryStatus status = feature.GetStatus();
            BatteryUpdateEvent batteryUpdateEvent = new(status.state_of_charge, status.charging_status.ToChargingStatus());
            BatteryUpdated?.Invoke(this, batteryUpdateEvent);
        }
        catch (Exception)
        {
            // The device is not ready yet. The next ConnectionChanged event will retry
            UnsubscribeToBatteryEvents();
            _currentDevice = null;
        }
    }

    void UnsubscribeToBatteryEvents()
    {
        _currentFeature?.BatteryStatusBroadcast -= _batteryStatusBroadcastListener;
        _currentFeature = null;
    }

    public void Dispose() => _deviceProviderInstance.Dispose();
}
