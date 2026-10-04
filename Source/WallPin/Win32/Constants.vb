Namespace Win32

    ''' <summary>
    ''' Provides native Win32 constants.
    ''' </summary>
    Friend Module Constants

        ''' <summary>
        ''' The maximum length, in characters, of a path in the legacy Win32 file APIs,
        ''' including the terminating null character.
        ''' </summary>
        ''' 
        ''' <remarks>
        ''' <para></para>
        ''' Official documentation:
        ''' <see href="https://learn.microsoft.com/en-us/windows/win32/fileio/maximum-file-path-limitation">
        ''' Maximum Path Length Limitation</see>
        ''' </remarks>
        Friend Const MAX_PATH As Integer = 260

        ''' <summary>
        ''' <c>SPI_SETDESKWALLPAPER</c> (0x0014).
        ''' <para></para>
        ''' Sets the desktop wallpaper.
        ''' The value of the <c>pvParam</c> parameter determines the new wallpaper.
        ''' </summary>
        ''' 
        ''' <remarks>
        ''' <para></para>
        ''' Official documentation:
        ''' <see href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-systemparametersinfow">
        ''' SystemParametersInfoW function (winuser.h)</see>
        ''' </remarks>
        Friend Const SPI_SETDESKWALLPAPER As Integer = 20

        ''' <summary>
        ''' <c>SPIF_UPDATEINIFILE</c> (0x01).
        ''' <para></para>
        ''' Writes the new system-wide parameter setting to the user profile.
        ''' </summary>
        ''' 
        ''' <remarks>
        ''' <para></para>
        ''' Official documentation:
        ''' <see href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-systemparametersinfow">
        ''' SystemParametersInfoW function (winuser.h)</see>
        ''' </remarks>
        Friend Const SPIF_UPDATEINIFILE As Integer = 1

        ''' <summary>
        ''' <c>SPIF_SENDCHANGE</c> (0x02), also known as <c>SPIF_SENDWININICHANGE</c>.
        ''' <para></para>
        ''' Broadcasts the <c>WM_SETTINGCHANGE</c> message after updating the user profile.
        ''' </summary>
        ''' 
        ''' <remarks>
        ''' <para></para>
        ''' Official documentation:
        ''' <see href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-systemparametersinfow">
        ''' SystemParametersInfoW function (winuser.h)</see>
        ''' </remarks>
        Friend Const SPIF_SENDCHANGE As Integer = 2

    End Module

End Namespace