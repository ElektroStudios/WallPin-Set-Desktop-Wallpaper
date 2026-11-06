# WallPin automated tests

MSTest project for the **VB.NET / .NET Framework 4.8** WallPin application. It references the real `WallPin.vbproj` and inspects or runs its compiled assembly; it does not contain copied implementations of the production logic.

## Requirements

- Windows.
- .NET Framework 4.8 Developer Pack / targeting pack.
- A .NET SDK with MSBuild and Visual Basic support, or a compatible Visual Studio installation.
- Internet access on the first restore, to download the test packages from NuGet.

The project uses MSTest 4.5.2 and Microsoft.NET.Test.Sdk 18.11.0.

## Run the tests

From this directory:

```powershell
dotnet test .\WallPin.Tests.vbproj --configuration Release
```

Only the offline unit/contract tests (skip the tests that launch `WallPin.exe`):

```powershell
dotnet test .\WallPin.Tests.vbproj --configuration Release --filter "TestCategory!=Integration"
```

Alternatively, double-click `run-tests.cmd`, or open `WallPin.slnx` (or `WallPin.Tests.sln`) in Visual Studio.

## Test classes

| Class | Kind | What it verifies |
|---|---|---|
| `ProgramContractTests` | Unit | `Program.SupportedFileExtensions` and `Program.StyleMapping`: exact contents, canonical form, case-insensitivity **of the production collections themselves**, rejection of unknown values, the `default` style, and the registry/COM values of every style identifier. |
| `WallpaperConfigTests` | Unit | `WallpaperConfig`: constructor, read-only fields, sealed class, and `ToIDesktopWallpaperPosition` for every registry `WallpaperStyle`/`TileWallpaper` combination, including fallbacks. |
| `Win32InteropContractTests` | Unit | Win32 constants, the `SystemParametersInfoW` P/Invoke signature, and the full `IDesktopWallpaper` vtable: member order, parameter types, `PreserveSig` flags, HRESULT convention, `LPWStr` marshaling and `<Out>` parameters, plus the IID/CLSID. |
| `ShellIntegrationContractTests` | Unit | The `Shell Integration\*.bat` scripts stay in sync with the program: registered extensions, style arguments passed to `WallPin.exe`, item count vs. the `FOR /L` loop, icons, executable name, single-selection model, and EN/ES consistency. The scripts are parsed, never executed. |
| `ProgramCommandLineTests` | Integration | Runs the real `WallPin.exe` in headless mode and checks the exit codes 1–5 and their error messages: argument count, missing file, directory instead of file, unsupported extension, every supported extension (any case), relative paths, paths longer than `MAX_PATH`, invalid styles, validation order, and that the usage text documents every style and extension. |

## Safety

- The unit tests do not call `Program.Main`, create the COM desktop-wallpaper object, invoke `SystemParametersInfo`, write to the Windows Registry, or change the desktop wallpaper.
- `ProgramCommandLineTests` launches `WallPin.exe`, but **every case is designed to stop at a validation error** (exit codes 1–5), which the program reports before it touches the Registry or the wallpaper. The image files are empty placeholders created in a private folder under `%TEMP%\WallPin.Tests`, deleted after each test. If a run ever ends with exit code 0, the test fails explicitly.
- `ProgramCommandLineTests` starts `WallPin.exe` with the environment variable `WALLPIN_HEADLESS=1` (set only for that child process). In this mode WallPin writes its error messages to the standard error stream instead of showing a message box; the exit codes are the same. No message box appears, and the tests also work on non-interactive sessions such as CI agents. If a message box ever appeared anyway, the run would hit a 20-second timeout and the test would fail instead of hanging.
- End users are not affected: without that environment variable (or with any value other than `1`), WallPin shows its message boxes exactly as before.

## Maintenance notes

The tests use reflection to inspect private implementation details, so the production code needs no `InternalsVisibleTo` attribute or visibility changes. As a consequence, they intentionally fail if the tested private member names (`SupportedFileExtensions`, `StyleMapping`, `WallpaperStyle`, `TileWallpaper`, `ToIDesktopWallpaperPosition`) are renamed; update `ReflectionTestSupport.vb` when that happens.

The successful path (writing the Registry values and applying the wallpaper through `IDesktopWallpaper` or `SystemParametersInfo`) is deliberately not exercised: it would change the desktop of the machine running the tests.
