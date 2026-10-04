# WallPin Change Log 📋

## v1.1
- Added a set of icons for the context menu sub-items.
- Added support for `.heic`, `.heif` and `.jfif` image files.
- Added support for the `default` wallpaper style argument, which applies the wallpaper keeping the system's current style.
- The wallpaper is now applied through the `IDesktopWallpaper` COM interface. The legacy `SystemParametersInfo` function is kept only as a fallback.
- Batch-script files: Reduced the frequency of the context menu items appearing greyed out (disabled) when changing wallpapers in very quick succession. The shell integration script now registers the `CommandStateSync` value, so Explorer resolves the state of the items synchronously instead of in a background thread.

## v1.0
Initial Release.