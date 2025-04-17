# MultipleScreenPowersave

**Experimental** tool for automatically switching off unused monitors in a multiple monitor setup.
Monitors are considered USED when at least one of the conditions is met
- at least one window is being shown
- the mouse cursor is located on it

Requirements:
- Monitors supporting to be switched on and of via [DDC/CI](https://en.wikipedia.org/wiki/Display_Data_Channel#DDC/CI)
- Windows operating system
- Linux:
    - ddcutil
	- light
	- xdotool

## Configuration file appsettings.json

Configuration file specifying regex-based blacklist entries for
- physical monitors
- windows by processName or windowTitle

The default `appsettings.configuration` provided
- always blacklists the primary monitor
- blacklists some basic windows processes
    - explorer used to display the desktop
    - TextInputHost used for redirecting text input

## Build

You need to have .net sdk 8.x installed.

`dotnet build --configuration Release`

Executable files and their dependencies are being created in folder `MultipleScreenPowersave/bin/Release/net8.0-windows/`

## Problems

- Some windows are reported as being "shown" though they are only returned by the API. To prevent such a window from keeping a monitor turned on indefinitely those programs need to be blacklisted manually, i.e.:

    ```
    {
		"blacklist": {
			"windows": [
				{ "processName": "explorer", "windowTitle": "^(|Program Manager)$" },
				{ "processName": "ApplicationFrameHost", "windowTitle": "^Einstellungen$" },
				{ "processName": "LogiOverlay", "windowTitle": "^MainWindow$" },
				{ "processName": "ShellExperienceHost", "windowTitle": "^Host für die Windows Shell-Oberfläche$" },
				{ "processName": "SystemSettings", "windowTitle": "^CN=Microsoft Windows, O=Microsoft Corporation, L=Redmond, S=Washington, C=US$" },
				{ "processName": "TextInputHost" }
			]
		}
	}
	```

    You can take a look at the debug output to see processNames and windowNames of existing windows.
- To enable debug output change `restrictedToMinimumLevel` in `appsettings.json` to "Debug":

    ```
    "WriteTo": {
      "ConsoleLog": {
        "Name": "Console",
        "Args": {
          "restrictedToMinimumLevel": "Debug"
        }
    },
    ```
