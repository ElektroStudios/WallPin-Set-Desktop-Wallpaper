# WallPin automated tests

This test project targets the existing **VB.NET / .NET Framework 4.8** application. It references the real `WallPin.vbproj` project and inspects its compiled assembly; it does not contain copied implementations of the production logic.

## Installation

Extract the ZIP into the root directory of the `WallPin-Set-Desktop-Wallpaper` repository. It adds `Source\WallPin.Tests` and does not overwrite any existing source files.

The folder contains a Visual Studio solution with both the application and its test project:

- `WallPin.Tests.sln`
- `WallPin.Tests.vbproj`

## Requirements

- Windows.
- .NET Framework 4.8 Developer Pack / targeting pack.
- A .NET SDK with MSBuild and Visual Basic support, or a compatible Visual Studio installation.
- Internet access on the first restore, to download the test packages from NuGet.

The project uses MSTest 4.5.2 and Microsoft.NET.Test.Sdk 18.11.0.

## Run the tests

From this directory, run:

```powershell
dotnet test .\WallPin.Tests.sln --configuration Release
```

Alternatively, double-click `run-tests.cmd`, open `WallPin.Tests.sln` in Visual Studio, or run the project directly:

```powershell
dotnet test .\WallPin.Tests.vbproj --configuration Release
```

## Current test coverage

The tests currently cover:

- Every numeric wallpaper style identifier and its text alias (`center`, `tile`, `stretch`, `fit`, `fill`, and `span`).
- The registry values and `IDesktopWallpaper` position value associated with each style.
- The `default` style behavior and case-insensitive style lookup.
- The exact set of supported image extensions and case-insensitive extension matching.
- The fallback position used by `WallpaperConfig` for an unrecognized style value.
- Win32 constants and the `SystemParametersInfoW` P/Invoke metadata.
- The `IDesktopWallpaper` COM identifier, interface type, method/vtable order, and relevant string/structure marshaling declarations.
- The `DesktopWallpaper` COM activation class identifier.

## Safety

These are offline contract/unit tests. They do **not** call `Program.Main`, create the COM desktop-wallpaper object, invoke `SystemParametersInfo`, write to the Windows Registry, copy files into the temporary directory, or change the desktop wallpaper.

The tests use reflection to inspect private implementation details so the production code does not need `InternalsVisibleTo` attributes or visibility changes. As a result, they intentionally fail if the tested private field or type names are renamed; update the tests when those implementation details change.

## Scope limitations

The current application performs its final work through Windows Registry, COM, and Win32 APIs. Those side effects are deliberately not exercised by this suite. They would require a separate Windows integration-test layer with explicit isolation and cleanup. The test project therefore validates the deterministic mappings and interop declarations, not the successful end-to-end wallpaper change on a live desktop.
