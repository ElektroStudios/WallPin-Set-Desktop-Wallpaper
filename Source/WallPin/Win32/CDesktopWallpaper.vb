Imports System.Runtime.InteropServices

Namespace Win32

    ''' <summary>
    ''' COM class (coclass) that implements the <see cref="IDesktopWallpaper"/> interface.
    ''' <para></para>
    ''' Corresponds to the native <c>CLSID_DesktopWallpaper</c> class identifier.
    ''' </summary>
    ''' 
    ''' <remarks>
    ''' <para></para>
    ''' This class is only a managed COM activation proxy. Create an instance and cast it
    ''' to <see cref="IDesktopWallpaper"/> to use it.
    ''' <para></para>
    ''' Official documentation:
    ''' <see href="https://learn.microsoft.com/en-us/windows/win32/api/shobjidl_core/nn-shobjidl_core-idesktopwallpaper">
    ''' IDesktopWallpaper interface (shobjidl_core.h)</see>
    ''' </remarks>
    <ComImport>
    <Guid("C2CF3110-460E-4FC1-B9D0-8A1C0C9CC4BD")> ' CLSID_DesktopWallpaper
    Friend Class CDesktopWallpaper
    End Class

End Namespace