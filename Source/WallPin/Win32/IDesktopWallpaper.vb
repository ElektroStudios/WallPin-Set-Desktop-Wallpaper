Imports System.Runtime.InteropServices

Namespace Win32

    ''' <summary>
    ''' Exposes methods that control the desktop wallpaper.
    ''' <para></para>
    ''' Available since Windows 8.
    ''' </summary>
    ''' 
    ''' <remarks>
    ''' <para></para>
    ''' The order of the members matches the native vtable layout and must not be changed.
    ''' <para></para>
    ''' Official documentation:
    ''' <see href="https://learn.microsoft.com/en-us/windows/win32/api/shobjidl_core/nn-shobjidl_core-idesktopwallpaper">
    ''' IDesktopWallpaper interface (shobjidl_core.h)</see>
    ''' </remarks>
    <ComImport>
    <Guid("B92B56A9-8B55-4E14-9A89-0199BBB6F93B")>
    <InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>
    Friend Interface IDesktopWallpaper

        ''' <summary>
        ''' Sets the desktop wallpaper.
        ''' </summary>
        ''' 
        ''' <remarks>
        ''' <para></para>
        ''' Official documentation:
        ''' <see href="https://learn.microsoft.com/en-us/windows/win32/api/shobjidl_core/nf-shobjidl_core-idesktopwallpaper-setwallpaper">
        ''' IDesktopWallpaper::SetWallpaper method</see>
        ''' </remarks>
        ''' 
        ''' <param name="monitorID">
        ''' The ID of the monitor, as returned by <see cref="IDesktopWallpaper.GetMonitorDevicePathAt"/>.
        ''' <para></para>
        ''' Pass <see langword="Nothing"/> to apply the wallpaper to all monitors.
        ''' </param>
        ''' 
        ''' <param name="wallpaper">
        ''' The full path of the wallpaper image file.
        ''' </param>
        ''' 
        ''' <exception cref="COMException">
        ''' Thrown when the underlying method returns a failure HRESULT.
        ''' </exception>
        Sub SetWallpaper(<MarshalAs(UnmanagedType.LPWStr)> monitorID As String,
                         <MarshalAs(UnmanagedType.LPWStr)> wallpaper As String)

        ''' <summary>
        ''' Gets the current desktop wallpaper.
        ''' </summary>
        ''' 
        ''' <remarks>
        ''' <para></para>
        ''' Official documentation:
        ''' <see href="https://learn.microsoft.com/en-us/windows/win32/api/shobjidl_core/nf-shobjidl_core-idesktopwallpaper-getwallpaper">
        ''' IDesktopWallpaper::GetWallpaper method</see>
        ''' </remarks>
        ''' 
        ''' <param name="monitorID">
        ''' The ID of the monitor, as returned by <see cref="IDesktopWallpaper.GetMonitorDevicePathAt"/>.
        ''' <para></para>
        ''' Pass <see langword="Nothing"/> to get the wallpaper shared by all monitors, if any.
        ''' </param>
        ''' 
        ''' <param name="refWallpaper">
        ''' When this method returns, contains the full path of the current wallpaper image file.
        ''' </param>
        ''' 
        ''' <returns>
        ''' An <c>HRESULT</c>. <c>S_OK</c> (0) on success.
        ''' </returns>
        <PreserveSig>
        Function GetWallpaper(<MarshalAs(UnmanagedType.LPWStr)> monitorID As String,
                   <Out, MarshalAs(UnmanagedType.LPWStr)> ByRef refWallpaper As String) As Integer

        ''' <summary>
        ''' Retrieves the unique ID of one of the system's monitors.
        ''' </summary>
        ''' 
        ''' <remarks>
        ''' <para></para>
        ''' Official documentation:
        ''' <see href="https://learn.microsoft.com/en-us/windows/win32/api/shobjidl_core/nf-shobjidl_core-idesktopwallpaper-getmonitordevicepathat">
        ''' IDesktopWallpaper::GetMonitorDevicePathAt method</see>
        ''' </remarks>
        ''' 
        ''' <param name="monitorIndex">
        ''' The zero-based index of the monitor.
        ''' <para></para>
        ''' The valid range is 0 to <c>count - 1</c>, where count is the value returned by
        ''' <see cref="IDesktopWallpaper.GetMonitorDevicePathCount"/>.
        ''' </param>
        ''' 
        ''' <param name="refMonitorID">
        ''' When this method returns, contains the monitor device path (monitor ID).
        ''' </param>
        ''' 
        ''' <exception cref="COMException">
        ''' Thrown when the underlying method returns a failure HRESULT.
        ''' </exception>
        Sub GetMonitorDevicePathAt(monitorIndex As UInteger,
                                   <Out, MarshalAs(UnmanagedType.LPWStr)>
                             ByRef refMonitorID As String)

        ''' <summary>
        ''' Retrieves the number of monitors that are associated with the system.
        ''' </summary>
        ''' 
        ''' <remarks>
        ''' <para></para>
        ''' Official documentation:
        ''' <see href="https://learn.microsoft.com/en-us/windows/win32/api/shobjidl_core/nf-shobjidl_core-idesktopwallpaper-getmonitordevicepathcount">
        ''' IDesktopWallpaper::GetMonitorDevicePathCount method</see>
        ''' </remarks>
        ''' 
        ''' <param name="refCount">
        ''' When this method returns, contains the number of monitors.
        ''' </param>
        ''' 
        ''' <returns>
        ''' An <c>HRESULT</c>. <c>S_OK</c> (0) on success.
        ''' </returns>
        <PreserveSig>
        Function GetMonitorDevicePathCount(ByRef refCount As UInteger) As Integer

        ''' <summary>
        ''' Retrieves the display rectangle of the specified monitor.
        ''' </summary>
        ''' 
        ''' <remarks>
        ''' <para></para>
        ''' Official documentation:
        ''' <see href="https://learn.microsoft.com/en-us/windows/win32/api/shobjidl_core/nf-shobjidl_core-idesktopwallpaper-getmonitorrect">
        ''' IDesktopWallpaper::GetMonitorRECT method</see>
        ''' <para></para>
        ''' The native parameter is a <c>RECT</c> structure (16 bytes).
        ''' This declaration is only a placeholder to preserve the vtable layout; do not call it as is.
        ''' </remarks>
        ''' 
        ''' <param name="monitorID">
        ''' The ID of the monitor, as returned by <see cref="IDesktopWallpaper.GetMonitorDevicePathAt"/>.
        ''' </param>
        ''' 
        ''' <param name="refDisplayRect">
        ''' When this method returns, contains the display rectangle of the monitor.
        ''' </param>
        ''' 
        ''' <returns>
        ''' An <c>HRESULT</c>. <c>S_OK</c> (0) on success.
        ''' </returns>
        <PreserveSig>
        Function GetMonitorRECT(<MarshalAs(UnmanagedType.LPWStr)> monitorID As String,
                                                      <Out> ByRef refDisplayRect As IntPtr) As Integer

        ''' <summary>
        ''' Sets the color that is visible on the desktop when no image is displayed
        ''' or when the desktop background is disabled.
        ''' </summary>
        ''' <remarks>
        ''' 
        ''' <para></para>
        ''' Official documentation:
        ''' <see href="https://learn.microsoft.com/en-us/windows/win32/api/shobjidl_core/nf-shobjidl_core-idesktopwallpaper-setbackgroundcolor">
        ''' IDesktopWallpaper::SetBackgroundColor method</see>
        ''' </remarks>
        ''' 
        ''' <param name="color">
        ''' The background color, as a <c>COLORREF</c> value (0x00BBGGRR).
        ''' </param>
        ''' 
        ''' <exception cref="COMException">
        ''' Thrown when the underlying method returns a failure HRESULT.
        ''' </exception>
        Sub SetBackgroundColor(color As UInteger)

        ''' <summary>
        ''' Retrieves the color that is visible on the desktop when no image is displayed
        ''' or when the desktop background is disabled.
        ''' </summary>
        ''' 
        ''' <remarks>
        ''' <para></para>
        ''' Official documentation:
        ''' <see href="https://learn.microsoft.com/en-us/windows/win32/api/shobjidl_core/nf-shobjidl_core-idesktopwallpaper-getbackgroundcolor">
        ''' IDesktopWallpaper::GetBackgroundColor method</see>
        ''' </remarks>
        ''' 
        ''' <param name="refColor">
        ''' When this method returns, contains the background color as a <c>COLORREF</c> value (0x00BBGGRR).
        ''' </param>
        ''' 
        ''' <returns>
        ''' An <c>HRESULT</c>. <c>S_OK</c> (0) on success.
        ''' </returns>
        <PreserveSig>
        Function GetBackgroundColor(ByRef refColor As UInteger) As Integer

        ''' <summary>
        ''' Sets the display option for the desktop wallpaper image (how it fits the screen).
        ''' </summary>
        ''' 
        ''' <remarks>
        ''' <para></para>
        ''' Official documentation:
        ''' <see href="https://learn.microsoft.com/en-us/windows/win32/api/shobjidl_core/nf-shobjidl_core-idesktopwallpaper-setposition">
        ''' IDesktopWallpaper::SetPosition method</see>
        ''' <para></para>
        ''' Native <c>DESKTOP_WALLPAPER_POSITION</c> values:
        ''' 0 = Center, 1 = Tile, 2 = Stretch, 3 = Fit, 4 = Fill, 5 = Span.
        ''' </remarks>
        ''' 
        ''' <param name="position">
        ''' A <c>DESKTOP_WALLPAPER_POSITION</c> value that specifies the wallpaper position.
        ''' </param>
        ''' 
        ''' <exception cref="COMException">
        ''' Thrown when the underlying method returns a failure HRESULT.
        ''' </exception>
        Sub SetPosition(position As Integer)

        ''' <summary>
        ''' Gets the current display option for the desktop wallpaper image.
        ''' </summary>
        ''' <remarks>
        ''' 
        ''' <para></para>
        ''' Official documentation:
        ''' <see href="https://learn.microsoft.com/en-us/windows/win32/api/shobjidl_core/nf-shobjidl_core-idesktopwallpaper-getposition">
        ''' IDesktopWallpaper::GetPosition method</see>
        ''' </remarks>
        ''' 
        ''' <param name="refPosition">
        ''' When this method returns, contains a <c>DESKTOP_WALLPAPER_POSITION</c> value.
        ''' </param>
        ''' 
        ''' <returns>
        ''' An <c>HRESULT</c>. <c>S_OK</c> (0) on success.
        ''' </returns>
        <PreserveSig>
        Function GetPosition(ByRef refPosition As Integer) As Integer

        ''' <summary>
        ''' Sets the images to display in the desktop slideshow.
        ''' </summary>
        ''' <remarks>
        ''' 
        ''' <para></para>
        ''' Official documentation:
        ''' <see href="https://learn.microsoft.com/en-us/windows/win32/api/shobjidl_core/nf-shobjidl_core-idesktopwallpaper-setslideshow">
        ''' IDesktopWallpaper::SetSlideshow method</see>
        ''' </remarks>
        ''' 
        ''' <param name="items">
        ''' A pointer to an <c>IShellItemArray</c> that contains the slideshow images.
        ''' </param>
        ''' 
        ''' <exception cref="COMException">
        ''' Thrown when the underlying method returns a failure HRESULT.
        ''' </exception>
        Sub SetSlideshow(items As IntPtr)

        ''' <summary>
        ''' Gets the images that are displayed in the desktop slideshow.
        ''' </summary>
        ''' 
        ''' <remarks>
        ''' <para></para>
        ''' Official documentation:
        ''' <see href="https://learn.microsoft.com/en-us/windows/win32/api/shobjidl_core/nf-shobjidl_core-idesktopwallpaper-getslideshow">
        ''' IDesktopWallpaper::GetSlideshow method</see>
        ''' </remarks>
        ''' 
        ''' <param name="refItems">
        ''' When this method returns, contains a pointer to an <c>IShellItemArray</c>
        ''' with the slideshow images. The caller must release it.
        ''' </param>
        ''' 
        ''' <returns>
        ''' An <c>HRESULT</c>. <c>S_OK</c> (0) on success.
        ''' </returns>
        <PreserveSig>
        Function GetSlideshow(ByRef refItems As IntPtr) As Integer

        ''' <summary>
        ''' Sets the desktop slideshow settings, including the slideshow shuffle setting
        ''' and the interval between slides.
        ''' </summary>
        ''' 
        ''' <remarks>
        ''' <para></para>
        ''' Official documentation:
        ''' <see href="https://learn.microsoft.com/en-us/windows/win32/api/shobjidl_core/nf-shobjidl_core-idesktopwallpaper-setslideshowoptions">
        ''' IDesktopWallpaper::SetSlideshowOptions method</see>
        ''' <para></para>
        ''' Native <c>DESKTOP_SLIDESHOW_OPTIONS</c> values:
        ''' 0x1 = <c>DSO_SHUFFLEIMAGES</c>.
        ''' </remarks>
        ''' 
        ''' <param name="options">
        ''' A <c>DESKTOP_SLIDESHOW_OPTIONS</c> value.
        ''' </param>
        ''' 
        ''' <param name="slideshowTick">
        ''' The interval between slides, in milliseconds.
        ''' </param>
        ''' 
        ''' <exception cref="COMException">
        ''' Thrown when the underlying method returns a failure HRESULT.
        ''' </exception>
        Sub SetSlideshowOptions(options As Integer,
                                slideshowTick As UInteger)

        ''' <summary>
        ''' Gets the current desktop slideshow settings.
        ''' </summary>
        ''' 
        ''' <remarks>
        ''' <para></para>
        ''' Official documentation:
        ''' <see href="https://learn.microsoft.com/en-us/windows/win32/api/shobjidl_core/nf-shobjidl_core-idesktopwallpaper-getslideshowoptions">
        ''' IDesktopWallpaper::GetSlideshowOptions method</see>
        ''' </remarks>
        ''' 
        ''' <param name="refOptions">
        ''' When this method returns, contains a <c>DESKTOP_SLIDESHOW_OPTIONS</c> value.
        ''' </param>
        ''' 
        ''' <param name="refSlideshowTick">
        ''' When this method returns, contains the interval between slides, in milliseconds.
        ''' </param>
        ''' 
        ''' <returns>
        ''' An <c>HRESULT</c>. <c>S_OK</c> (0) on success.
        ''' </returns>
        <PreserveSig>
        Function GetSlideshowOptions(ByRef refOptions As Integer,
                                     ByRef refSlideshowTick As UInteger) As Integer

        ''' <summary>
        ''' Switches the wallpaper on a specified monitor to the next image in the slideshow.
        ''' </summary>
        ''' 
        ''' <remarks>
        ''' <para></para>
        ''' Official documentation:
        ''' <see href="https://learn.microsoft.com/en-us/windows/win32/api/shobjidl_core/nf-shobjidl_core-idesktopwallpaper-advanceslideshow">
        ''' IDesktopWallpaper::AdvanceSlideshow method</see>
        ''' <para></para>
        ''' Native <c>DESKTOP_SLIDESHOW_DIRECTION</c> values:
        ''' 0 = <c>DSD_FORWARD</c>, 1 = <c>DSD_BACKWARD</c>.
        ''' </remarks>
        ''' 
        ''' <param name="monitorID">
        ''' The ID of the monitor, as returned by <see cref="IDesktopWallpaper.GetMonitorDevicePathAt"/>.
        ''' <para></para>
        ''' Pass <see langword="Nothing"/> to advance the slideshow on all monitors.
        ''' </param>
        ''' 
        ''' <param name="direction">
        ''' A <c>DESKTOP_SLIDESHOW_DIRECTION</c> value that specifies the direction.
        ''' </param>
        ''' 
        ''' <exception cref="COMException">
        ''' Thrown when the underlying method returns a failure HRESULT.
        ''' </exception>
        Sub AdvanceSlideshow(<MarshalAs(UnmanagedType.LPWStr)> monitorID As String,
                                                               direction As Integer)

        ''' <summary>
        ''' Gets the current status of the desktop slideshow.
        ''' </summary>
        ''' 
        ''' <remarks>
        ''' <para></para>
        ''' Official documentation:
        ''' <see href="https://learn.microsoft.com/en-us/windows/win32/api/shobjidl_core/nf-shobjidl_core-idesktopwallpaper-getstatus">
        ''' IDesktopWallpaper::GetStatus method</see>
        ''' <para></para>
        ''' Native <c>DESKTOP_SLIDESHOW_STATE</c> values (flags):
        ''' 0x1 = <c>DSS_ENABLED</c>, 0x2 = <c>DSS_SLIDESHOW</c>, 0x4 = <c>DSS_DISABLED_BY_REMOTE_SESSION</c>.
        ''' </remarks>
        ''' 
        ''' <param name="refState">
        ''' When this method returns, contains a <c>DESKTOP_SLIDESHOW_STATE</c> value.
        ''' </param>
        ''' 
        ''' <returns>
        ''' An <c>HRESULT</c>. <c>S_OK</c> (0) on success.
        ''' </returns>
        <PreserveSig>
        Function GetStatus(ByRef refState As Integer) As Integer

        ''' <summary>
        ''' Enables or disables the desktop background.
        ''' </summary>
        ''' 
        ''' <remarks>
        ''' <para></para>
        ''' Official documentation:
        ''' <see href="https://learn.microsoft.com/en-us/windows/win32/api/shobjidl_core/nf-shobjidl_core-idesktopwallpaper-enable">
        ''' IDesktopWallpaper::Enable method</see>
        ''' </remarks>
        ''' 
        ''' <param name="enabled">
        ''' <see langword="True"/> to enable the desktop background;
        ''' <see langword="False"/> to disable it.
        ''' </param>
        ''' 
        ''' <returns>
        ''' An <c>HRESULT</c>. <c>S_OK</c> (0) on success.
        ''' </returns>
        <PreserveSig>
        Function Enable(enabled As Boolean) As Integer

    End Interface

End Namespace