# Logitech battery icon

A simple Windows application that adds an icon to the system tray showing the battery level of a connected Logitech mouse.

## Requirements

Runtime: .NET 10.0 or later.

The application requires the `logi_nethidppio.dll` library, that is not publicly available. 
It can be extracted from the executable of the `OnboardMemoryManager` Logitech tool which can be downloaded from here:
https://support.logi.com/hc/en-us/articles/360059641133-Onboard-Memory-Manager.

> [!NOTE]
> The program has been tested on Windows 11 with a Logitech G502X mouse, using the `logi_nethidppio.dll` extracted from the `2.6.1749` version of `OnboardMemoryManager`.</br>
  It should work on older version of Windows and similar Logitech mice. It does not work with other wireless devices (e.g. keyboards) or multiple devices.

## Usage

1. Download the latest release from the [releases page](https://github.com/lorenzodepasquale/logitech-battery-icon/releases)
2. Extract the contents of the zip file
3. Put the `logi_nethidppio.dll` file in the same directory
4. Run `LogitechBatteryIcon.exe`

> [!TIP]
> You can automatically start the application on Windows startup by adding a shortcut to the application in the Startup directory:</br>
  `C:\Users\<username>\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup`

## Build

1. Put the `logi_nethidppio.dll` file in the project directory (same directory as the `.csproj` file)
2. Run `dotnet build` in the project directory.
