Imports System.Runtime.InteropServices
Imports System.Security

Namespace Win32

    ''' <summary>
    ''' Provides platform invoke (P/Invoke) declarations for native Win32 functions.
    ''' </summary>
    <SuppressUnmanagedCodeSecurity>
    Friend Module NativeMethods

        ''' <summary>
        ''' Retrieves or sets the value of one of the system-wide parameters.
        ''' <para></para>
        ''' This function can also update the user profile while setting a parameter.
        ''' </summary>
        ''' 
        ''' <remarks>
        ''' <para></para>
        ''' Official documentation:
        ''' <see href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-systemparametersinfow">
        ''' SystemParametersInfoW function (winuser.h)</see>
        ''' </remarks>
        ''' 
        ''' <param name="uAction">
        ''' The system-wide parameter to be retrieved or set.
        ''' <para></para>
        ''' For example, <see cref="Constants.SPI_SETDESKWALLPAPER"/>.
        ''' </param>
        ''' 
        ''' <param name="uParam">
        ''' A parameter whose usage and format depends on the system parameter being queried or set.
        ''' <para></para>
        ''' Must be 0 when <paramref name="uAction"/> is <see cref="Constants.SPI_SETDESKWALLPAPER"/>.
        ''' </param>
        ''' 
        ''' <param name="lpvParam">
        ''' A parameter whose usage and format depends on the system parameter being queried or set.
        ''' <para></para>
        ''' When <paramref name="uAction"/> is <see cref="Constants.SPI_SETDESKWALLPAPER"/>,
        ''' this is the full path of the image file to set as the desktop wallpaper.
        ''' </param>
        ''' 
        ''' <param name="fuWinIni">
        ''' Specifies whether the user profile is to be updated, and if so,
        ''' whether the <c>WM_SETTINGCHANGE</c> message is to be broadcast to all top-level windows.
        ''' <para></para>
        ''' This can be a combination of <see cref="Constants.SPIF_UPDATEINIFILE"/>
        ''' and <see cref="Constants.SPIF_SENDCHANGE"/>.
        ''' </param>
        ''' 
        ''' <returns>
        ''' If the function succeeds, the return value is a nonzero value.
        ''' <para></para>
        ''' If the function fails, the return value is zero.
        ''' <para></para>
        ''' To get extended error information, call <see cref="Marshal.GetLastWin32Error"/>.
        ''' </returns>
        <DllImport("user32.dll", EntryPoint:="SystemParametersInfoW", CharSet:=CharSet.Unicode, SetLastError:=True)>
        Friend Function SystemParametersInfo(uAction As Integer,
                                             uParam As Integer,
           <MarshalAs(UnmanagedType.LPWStr)> lpvParam As String,
                                             fuWinIni As Integer
        ) As Integer
        End Function

    End Module

End Namespace
