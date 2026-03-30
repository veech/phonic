# Phonic

A lightweight Windows desktop app for switching the system audio output device from a controller-friendly UI. Designed to be launched from Steam Big Picture.

## How to run

```
dotnet run
```

Or run the compiled executable directly from `bin/Debug/net10.0-windows/phonic.exe`.

## How to publish

Publish a framework-dependent build for Windows:

```
dotnet publish -c Release --self-contained false -o publish
```

The output will be in the `publish/` folder (~800KB total). Point Steam at `publish/Phonic.exe`.

Requires [.NET 10](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) to be installed on the target machine.

## How to add to Steam

1. Open Steam and go to **Library**
2. Click **Add a Game** → **Add a Non-Steam Game**
3. Click **Browse** and select `phonic.exe`
4. Click **Add Selected Programs**
5. Right-click the new entry in your library → **Properties**
6. Set **Launch Options** if needed (none required by default)

## Suggested Steam Input mapping

In Steam Big Picture, configure a controller layout with these mappings:

| Controller input | Keyboard output |
|---|---|
| D-pad Up / Left Stick Up | Arrow Up |
| D-pad Down / Left Stick Down | Arrow Down |
| A button | Enter |
| B button | Escape |

This gives full navigation: browse devices with the stick or D-pad, confirm with A, and exit with B.

## Audio API approach

**Device enumeration** uses [NAudio](https://github.com/naudio/NAudio) (`MMDeviceEnumerator`) which wraps the Windows Core Audio API (WASAPI). This provides reliable access to active render endpoints with friendly display names and default device detection.

**Setting the default device** uses the undocumented `IPolicyConfig` COM interface (`PolicyConfigClient`, CLSID `870af99c-171d-4f9e-af0d-e63df40c2bc9`). This is the standard approach used by tools like NirCmd, SoundSwitch, and AudioDeviceCmdlets since Windows Vista. There is no documented public Windows API for setting the default audio endpoint — this COM interface is the only reliable method available.

### Limitations

- Switching the default audio device requires the `IPolicyConfig` undocumented COM interface, which could theoretically break on a future Windows update. In practice it has been stable since Windows Vista.
- Microphone / input device switching is not supported.
- Per-application audio routing is not supported.
