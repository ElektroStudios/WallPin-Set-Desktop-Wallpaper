Imports System.Runtime.InteropServices
Imports System.Security

<SuppressUnmanagedCodeSecurity>
Friend Module NativeMethods

    ' https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-systemparametersinfow
    <DllImport("user32.dll", EntryPoint:="SystemParametersInfoW", CharSet:=CharSet.Unicode, SetLastError:=True)>
    Friend Function SystemParametersInfo(uAction As Integer,
                                         uParam As Integer,
       <MarshalAs(UnmanagedType.LPWStr)> lpvParam As String,
                                         fuWinIni As Integer
    ) As Integer
    End Function

End Module
