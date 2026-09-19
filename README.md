# WakeLock

A lightweight Windows system tray utility that prevents your computer from sleeping and keeps the display awake using the Win32 `SetThreadExecutionState` API.

## Features

- **System Tray Integration**: Runs quietly in the notification area.
- **One-Click Toggle**: Double-click the tray icon or use the context menu to enable/disable sleep prevention.
- **Single Instance**: Prevents multiple instances from running concurrently.
- **Zero Dependencies**: Pure .NET Windows Forms / Win32 API with no external packages required.

## Download

A precompiled executable (`WakeLock.exe`) is available in the **Releases** section, built with:
- **Microsoft (R) Visual C# Compiler version 4.8.4084.0**

## Building from Source

You can compile `WakeLock.cs` using the .NET Framework C# compiler (`csc.exe`):

```cmd
csc /target:winexe /optimize+ /out:WakeLock.exe WakeLock.cs
```

## Contributing

Contributions, forks and pull requests are welcome.

## License

MIT License. See LICENSE for details.