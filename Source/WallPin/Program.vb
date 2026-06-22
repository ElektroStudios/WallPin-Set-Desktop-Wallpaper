Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.IO
Imports System.Linq
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Windows.Forms

Imports Microsoft.Win32

Public Module Program

    Private Const WALLPIN_TEMP_FILENAME_PREFIX As String = "wallpin_temp_longpath_wallpaper"

    Private ReadOnly SupportedFileExtensions As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
            ".bmp",
            ".gif",
            ".jpg", ".jpeg",
            ".png",
            ".tif", ".tiff"
    }

    Private ReadOnly StyleMapping As New Dictionary(Of String, WallpaperConfig)(StringComparer.OrdinalIgnoreCase) From {
            {"0", New WallpaperConfig("0", "0")}, {"center", New WallpaperConfig("0", "0")},
            {"1", New WallpaperConfig("0", "1")}, {"tile", New WallpaperConfig("0", "1")},
            {"2", New WallpaperConfig("2", "0")}, {"stretch", New WallpaperConfig("2", "0")},
            {"6", New WallpaperConfig("6", "0")}, {"fit", New WallpaperConfig("6", "0")},
            {"10", New WallpaperConfig("10", "0")}, {"fill", New WallpaperConfig("10", "0")},
            {"22", New WallpaperConfig("22", "0")}, {"span", New WallpaperConfig("22", "0")}
        }

    Public Sub Main(args As String())

        ' Validate arguments count.
        If args.Length < 1 Then
            Program.ShowUsage($"ERROR: Missing required arguments. Received: {args.Length}", exitcode:=1)
        ElseIf args.Length > 2 Then
            Program.ShowUsage($"ERROR: Too much arguments. Expected: 2, received: {args.Length}", exitcode:=2)
        End If

        ' Parse image filepath.
        Dim filePath As String = args(0)
        Dim fullPath As String = Path.GetFullPath(filePath)
        Dim longFullPath As String = If(fullPath.StartsWith("\\?\"), fullPath, $"\\?\{fullPath}")

        ' Always use the extended kernel path prefix to safely verify existence of any path length
        If Not File.Exists(longFullPath) Then
            Program.TerminateProcess($"ERROR: The specified image filepath was not found: ""{fullPath}""", 3)
        End If

        ' Validate file extension.
        Dim fileExtension As String = Path.GetExtension(longFullPath)
        If Not Program.SupportedFileExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase) Then
            Program.TerminateProcess($"ERROR: ""{fileExtension}"" is not a supported file format.", exitCode:=4)
        End If

        ' Parse and resolve WallpaperStyle.
        Dim inputStyle As String = If(args.Length = 2, args(1), Nothing)
        Dim config As WallpaperConfig = Nothing

        If Not String.IsNullOrEmpty(inputStyle) AndAlso Not Program.StyleMapping.TryGetValue(inputStyle, config) Then
            Program.ShowUsage($"ERROR: Invalid WallpaperStyle identifier: ""{inputStyle}""", exitcode:=5)
        End If

        ' Set Registry values. Safe across x86 and x64 inside HKCU.
        Try
            Using desktopKey As RegistryKey = Registry.CurrentUser.OpenSubKey("Control Panel\Desktop", True)
                If desktopKey IsNot Nothing Then
                    If config IsNot Nothing Then
                        desktopKey.SetValue("TileWallpaper", config.TileWallpaper)
                        desktopKey.SetValue("WallpaperStyle", config.WallpaperStyle)
                    End If
                Else
                    Program.TerminateProcess("ERROR: Unable to open registry path: 'HKEY_CURRENT_USER\Control Panel\Desktop'", exitCode:=6)
                End If
            End Using

        Catch ex As Exception
            Dim errorMessage As String =
                "ERROR: Unable to access registry path: 'HKEY_CURRENT_USER\Control Panel\Desktop'" & Environment.NewLine & Environment.NewLine &
                $"HRESULT: {ex.HResult}" & Environment.NewLine &
                $"Message: {ex.Message}"

            Program.TerminateProcess(errorMessage, exitCode:=ex.HResult)
        End Try

        ' Determine the target path for the User32 API. 
        Dim finalWallpaperPath As String = fullPath

        ' Perform a shadow copy if the path length exceeds Windows MAX_PATH.
        If fullPath.Length >= Win32_Constants.MAX_PATH Then

            Dim tempDir As String = Path.GetTempPath()
            Try
                If Not Directory.Exists(tempDir) Then
                    Directory.CreateDirectory(tempDir)
                End If

                Dim safeWallpaperPath As String =
                    Path.Combine(tempDir, $"{Program.WALLPIN_TEMP_FILENAME_PREFIX}{fileExtension}")

                File.Copy(longFullPath, safeWallpaperPath, overwrite:=True)
                finalWallpaperPath = safeWallpaperPath

            Catch ex As Exception
                Dim copyErrorMessage As String =
                    $"ERROR: The image filepath is too long and WallPin failed to create a temporary local copy to apply it." & Environment.NewLine & Environment.NewLine &
                    $"HRESULT: {ex.HResult}" & Environment.NewLine &
                    $"Message: {ex.Message}"

                Program.TerminateProcess(copyErrorMessage, ex.HResult)
            End Try
        End If

        ' Trigger system update to apply the wallpaper.
        Dim lastWin32Error As Integer
        Dim updateResult As Integer =
            NativeMethods.SystemParametersInfo(Win32_Constants.SPI_SETDESKWALLPAPER, 0, finalWallpaperPath,
                                               Win32_Constants.SPIF_UPDATEINIFILE Or Win32_Constants.SPIF_SENDCHANGE)

        lastWin32Error = Marshal.GetLastWin32Error()

        If updateResult = 0 Then
            Dim win32Exception As New Win32Exception(lastWin32Error)

            Dim errorMessage As String =
                $"ERROR: Function '{NameOf(NativeMethods.SystemParametersInfo)}' has failed" & Environment.NewLine & Environment.NewLine &
                $"Win32 Error Code: {lastWin32Error}" & Environment.NewLine &
                $"Message: {win32Exception.Message}"

            Program.TerminateProcess(errorMessage, lastWin32Error)
        End If

        Environment.Exit(0)
    End Sub

    Private Sub ShowUsage(errorMessage As String, exitcode As Integer)

        Dim exeName As String =
            Path.GetFileName(Application.ExecutablePath)

        Dim usageMessage As String =
$"{errorMessage}

USAGE:
    {exeName} <ImagePath> <WallpaperStyle>

EXAMPLES:
    {exeName} ""C:\Wallpapers\image.jpg"" fill
    {exeName} ""C:\Wallpapers\image.jpg"" center
    {exeName} ""C:\Wallpapers\image.jpg"" 10
    
WallpaperStyle accepted values (Name or ID):
     0  or  Center 
     1  or  Tile
     2  or  Stretch
     6  or  Fit
    10 or  Fill
    22 or  Span
    Or leave empty to use the system's default style.

Supported file extensions:
     {String.Join(", ", Program.SupportedFileExtensions)}"

        Program.TerminateProcess(usageMessage, exitcode)
    End Sub

    Private Sub TerminateProcess(message As String, exitCode As Integer)

        MessageBox.Show(Nothing, message, My.Application.Info.Title, MessageBoxButtons.OK, MessageBoxIcon.Error)
        Environment.Exit(exitCode)
    End Sub

End Module