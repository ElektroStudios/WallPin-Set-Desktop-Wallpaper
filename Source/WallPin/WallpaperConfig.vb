''' <summary>
''' Represents the configuration settings for desktop wallpaper positioning and registry styles.
''' </summary>
Friend NotInheritable Class WallpaperConfig

    ''' <summary>
    ''' Gets the registry string value for the wallpaper style (eg., "2" for stretch style).
    ''' </summary>
    Friend ReadOnly WallpaperStyle As String

    ''' <summary>
    ''' Gets the registry string value indicating whether the wallpaper is tiled ("0"=False, "1"=Tue).
    ''' </summary>
    Friend ReadOnly TileWallpaper As String

    ''' <summary>
    ''' Prevents a default instance of the <see cref="WallpaperConfig"/> class from being created.
    ''' </summary>
    Private Sub New()
    End Sub

    ''' <summary>
    ''' Initializes a new instance of the <see cref="WallpaperConfig"/> class using the specified style and tile parameters.
    ''' </summary>
    ''' 
    ''' <param name="style">
    ''' The registry string value for the wallpaper style (eg., "2" for stretch style).
    ''' </param>
    ''' 
    ''' <param name="tile">
    ''' The registry string value indicating whether the wallpaper is tiled ("0"=False, "1"=Tue).
    ''' </param>
    Friend Sub New(style As String,
                   tile As String)

        Me.WallpaperStyle = style
        Me.TileWallpaper = tile
    End Sub

    ''' <summary>
    ''' Converts the current registry wallpaper style string into its corresponding integer value 
    ''' required by the <see cref="Win32.IDesktopWallpaper"/> COM interface position enumeration.
    ''' </summary>
    ''' 
    ''' <returns>
    ''' An integer representing the desktop wallpaper position style.
    ''' </returns>
    Friend Function ToIDesktopWallpaperPosition() As Integer

        Select Case Me.WallpaperStyle

            Case "0"
                Return 0 ' Center

            Case "1"
                Return 1 ' Tile

            Case "2"
                Return 2 ' Stretch

            Case "6"
                Return 3 ' Fit

            Case "10"
                Return 4 ' Fill

            Case "22"
                Return 5 ' Span

            Case Else
                Return 0 ' Center (Default fallback for unrecognized styles)

        End Select
    End Function

End Class