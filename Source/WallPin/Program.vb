Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.IO
Imports System.Linq
Imports System.Runtime.InteropServices
Imports System.Windows.Forms

Imports Microsoft.Win32

Imports Win32

''' <summary>
''' WallPin: a command-line tool that sets an image file as the active desktop wallpaper,
''' with support for all the Windows wallpaper layout styles.
''' </summary>
Public Module Program

    ''' <summary>
    ''' The file name prefix (without extension) of the temporary copy of the image
    ''' that is created when the original file path exceeds <see cref="Win32.Constants.MAX_PATH"/>.
    ''' </summary>
    Private Const WALLPIN_TEMP_FILENAME_PREFIX As String = "wallpin_temp_wallpaper"

    ''' <summary>
    ''' The image file extensions supported by this tool (case-insensitive).
    ''' </summary>
    Private ReadOnly SupportedFileExtensions As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
            ".bmp",
            ".gif",
            ".heic", ".heif",
            ".jfif",
            ".jpg", ".jpeg",
            ".png",
            ".tif", ".tiff"
    }

    ''' <summary>
    ''' Maps each accepted wallpaper style identifier (numeric ID or name, case-insensitive)
    ''' to its corresponding <see cref="WallpaperConfig"/>.
    ''' <para></para>
    ''' The numeric IDs match the <c>WallpaperStyle</c> registry value
    ''' under <c>HKEY_CURRENT_USER\Control Panel\Desktop</c>.
    ''' <para></para>
    ''' The "default" identifier maps to <see langword="Nothing"/> on purpose:
    ''' it means "do not modify the current system wallpaper style".
    ''' </summary>
    Private ReadOnly StyleMapping As New Dictionary(Of String, WallpaperConfig)(StringComparer.OrdinalIgnoreCase) From {
        {"default", Nothing},
        {"0", New WallpaperConfig("0", "0")}, {"center", New WallpaperConfig("0", "0")},
        {"1", New WallpaperConfig("0", "1")}, {"tile", New WallpaperConfig("0", "1")},
        {"2", New WallpaperConfig("2", "0")}, {"stretch", New WallpaperConfig("2", "0")},
        {"6", New WallpaperConfig("6", "0")}, {"fit", New WallpaperConfig("6", "0")},
        {"10", New WallpaperConfig("10", "0")}, {"fill", New WallpaperConfig("10", "0")},
        {"22", New WallpaperConfig("22", "0")}, {"span", New WallpaperConfig("22", "0")}
    }

    ''' <summary>
    ''' The main application entry point.
    ''' <para></para>
    ''' Validates the arguments, stores the wallpaper style in the registry, and applies the wallpaper.
    ''' </summary>
    ''' 
    ''' <remarks>
    ''' Process exit codes:
    ''' <list type="bullet">
    ''' <item><description>0: The wallpaper was set successfully.</description></item>
    ''' <item><description>1: Missing required arguments.</description></item>
    ''' <item><description>2: Too many arguments.</description></item>
    ''' <item><description>3: The image file was not found.</description></item>
    ''' <item><description>4: Unsupported file extension.</description></item>
    ''' <item><description>5: Invalid wallpaper style identifier.</description></item>
    ''' <item><description>6: Unable to open the registry path.</description></item>
    ''' <item><description>Other: An HRESULT or Win32 error code reported by the failing operation.</description></item>
    ''' </list>
    ''' 
    ''' </remarks>
    ''' <param name="args">
    ''' The command-line arguments:
    ''' <para></para>
    ''' 1. The image file path (required).
    ''' <para></para>
    ''' 2. The wallpaper style, as a name or numeric ID (optional).
    ''' </param>
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
        If fullPath.Length >= Win32.Constants.MAX_PATH Then

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

        ' Attempt to update wallpaper via IDesktopWallpaper COM interface, falling back to User32 SystemParametersInfo if unavailable.
        Try
            Dim desktopWallpaper As IDesktopWallpaper = Program.TryCreateDesktopWallpaper()
            If desktopWallpaper IsNot Nothing Then
                Program.SetWallpaperViaIDesktopWallpaper(desktopWallpaper, finalWallpaperPath, config)
            Else
                Program.SetWallpaperViaSystemParametersInfo(finalWallpaperPath)
            End If

        Catch ex As COMException
            Program.TerminateProcess(
                $"ERROR: Interface '{NameOf(IDesktopWallpaper)}' has failed" & Environment.NewLine & Environment.NewLine &
                $"HRESULT: 0x{ex.ErrorCode:X8}" & Environment.NewLine &
                $"Message: {ex.Message}", ex.ErrorCode)

        Catch ex As Win32Exception
            Program.TerminateProcess(
                $"ERROR: Function '{NameOf(Win32.NativeMethods.SystemParametersInfo)}' has failed" & Environment.NewLine & Environment.NewLine &
                $"Win32 Error Code: {ex.NativeErrorCode}" & Environment.NewLine &
                $"Message: {ex.Message}", ex.NativeErrorCode)

        End Try

        Environment.Exit(0)
    End Sub

    ''' <summary>
    ''' Tries to create the <see cref="IDesktopWallpaper"/> COM object.
    ''' <para></para>
    ''' Returns <see langword="Nothing"/> if the interface is not available (e.g. Windows 7 or older).
    ''' </summary>
    ''' 
    ''' <returns>
    ''' An <see cref="IDesktopWallpaper"/> instance, or <see langword="Nothing"/> if it is not available.
    ''' </returns>
    Private Function TryCreateDesktopWallpaper() As IDesktopWallpaper

        Try
            Return DirectCast(New CDesktopWallpaper(), IDesktopWallpaper)

        Catch ex As Exception When TypeOf ex Is COMException OrElse TypeOf ex Is InvalidCastException
            Return Nothing

        End Try
    End Function

    ''' <summary>
    ''' Sets a wallpaper through the <see cref="IDesktopWallpaper"/> interface.
    ''' <para></para>
    ''' Throws <see cref="COMException"/> (HRESULT) on failure.
    ''' </summary>
    ''' 
    ''' <remarks>
    ''' This method takes ownership of <paramref name="desktopWallpaper"/>:
    ''' it releases the COM object before returning, so the caller must not use it afterwards.
    ''' </remarks>
    ''' 
    ''' <param name="desktopWallpaper">
    ''' The <see cref="IDesktopWallpaper"/> instance to use.
    ''' </param>
    ''' 
    ''' <param name="wallpaperPath">
    ''' The full path of the image file to set as the wallpaper.
    ''' </param>
    ''' <param name="config">
    ''' The wallpaper style to apply, or <see langword="Nothing"/> to keep the current system style.
    ''' </param>
    ''' 
    ''' <exception cref="COMException">
    ''' Thrown when <see cref="IDesktopWallpaper.SetPosition"/> or
    ''' <see cref="IDesktopWallpaper.SetWallpaper"/> returns a failure HRESULT.
    ''' </exception>
    Private Sub SetWallpaperViaIDesktopWallpaper(desktopWallpaper As IDesktopWallpaper,
                                                 wallpaperPath As String,
                                                 config As WallpaperConfig)
        Try
            If config IsNot Nothing Then
                Dim iDesktopWallpaperPosition As Integer = config.ToIDesktopWallpaperPosition()
                desktopWallpaper.SetPosition(iDesktopWallpaperPosition)
            End If

            desktopWallpaper.SetWallpaper(Nothing, wallpaperPath)

        Finally
            Marshal.FinalReleaseComObject(desktopWallpaper)

        End Try
    End Sub

    ''' <summary>
    ''' Sets a wallpaper through the <see cref="Win32.NativeMethods.SystemParametersInfo"/> function.
    ''' <para></para>
    ''' Throws <see cref="Win32Exception"/> (Win32 error code) on failure.
    ''' </summary>
    ''' 
    ''' <remarks>
    ''' This legacy method does not apply the wallpaper style by itself;
    ''' the style is read by the system from the registry values written in <see cref="Main"/>.
    ''' </remarks>
    ''' 
    ''' <param name="wallpaperPath">
    ''' The full path of the image file to set as the wallpaper.
    ''' </param>
    ''' 
    ''' <exception cref="Win32Exception">
    ''' Thrown when <see cref="Win32.NativeMethods.SystemParametersInfo"/> fails.
    ''' </exception>
    Private Sub SetWallpaperViaSystemParametersInfo(wallpaperPath As String)

        Dim success As Integer =
            Win32.NativeMethods.SystemParametersInfo(Win32.Constants.SPI_SETDESKWALLPAPER, 0, wallpaperPath,
                                                     Win32.Constants.SPIF_UPDATEINIFILE Or Win32.Constants.SPIF_SENDCHANGE)

        Dim win32error As Integer = Marshal.GetLastWin32Error()

        If success = 0 Then
            Throw New Win32Exception(win32error)
        End If
    End Sub

    ''' <summary>
    ''' Shows the usage instructions, and terminates the process.
    ''' </summary>
    ''' 
    ''' <param name="errorMessage">
    ''' The error message to display above the usage instructions.
    ''' </param>
    ''' 
    ''' <param name="exitcode">
    ''' The process exit code.
    ''' </param>
    Private Sub ShowUsage(errorMessage As String, exitcode As Integer)

        Dim exeName As String =
            Path.GetFileName(Application.ExecutablePath)

        Dim usageMessage As String =
$"{errorMessage}

USAGE:
    {exeName} <ImagePath> <WallpaperStyle>

EXAMPLES:
    {exeName} ""C:\Wallpapers\image.jpg"" default
    {exeName} ""C:\Wallpapers\image.jpg"" center
    {exeName} ""C:\Wallpapers\image.jpg"" fill
    {exeName} ""C:\Wallpapers\image.jpg"" 10
    
WallpaperStyle accepted values (ID or Name):
     ""0"" or ""center""
     ""1"" or ""tile""
     ""2"" or ""stretch""
     ""6"" or ""fit""
    ""10"" or ""fill""
    ""22"" or ""span""
    ""default"" to use the system's current style.

Supported file extensions:
     {String.Join(", ", Program.SupportedFileExtensions)}"

        Program.TerminateProcess(usageMessage, exitcode)
    End Sub

    ''' <summary>
    ''' Shows an error message box and terminates the process with the specified exit code.
    ''' </summary>
    ''' 
    ''' <param name="message">
    ''' The error message to display.
    ''' </param>
    ''' 
    ''' <param name="exitCode">
    ''' The process exit code.
    ''' </param>
    Private Sub TerminateProcess(message As String, exitCode As Integer)

        MessageBox.Show(Nothing, message, My.Application.Info.Title, MessageBoxButtons.OK, MessageBoxIcon.Error)
        Environment.Exit(exitCode)
    End Sub

End Module