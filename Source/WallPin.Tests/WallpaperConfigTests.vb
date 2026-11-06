Option Explicit On
Option Strict On
Option Infer Off

Imports System
Imports System.Reflection
Imports Microsoft.VisualStudio.TestTools.UnitTesting

''' <summary>
''' Verifies the <c>WallpaperConfig</c> class: its construction, immutability,
''' and the registry-to-COM position conversion.
''' </summary>
<TestClass>
Public NotInheritable Class WallpaperConfigTests

#Region " Construction and Immutability "

    ''' <summary>
    ''' Verifies that the constructor stores both registry values verbatim.
    ''' </summary>
    '''
    ''' <param name="style">
    ''' The registry <c>WallpaperStyle</c> value.
    ''' </param>
    '''
    ''' <param name="tile">
    ''' The registry <c>TileWallpaper</c> value.
    ''' </param>
    <TestMethod>
    <DataRow("10", "0")>
    <DataRow("0", "1")>
    <DataRow("22", "1")>
    <DataRow("", "")>
    <DataRow(Nothing, Nothing)>
    Public Sub ConstructorStoresTheRegistryValuesVerbatim(style As String, tile As String)

        Dim config As Object = CreateWallpaperConfig(style, tile)

        Assert.AreEqual(style, GetWallpaperStyle(config))
        Assert.AreEqual(tile, GetTileWallpaper(config))
    End Sub

    ''' <summary>
    ''' Verifies that both registry value fields are read-only.
    ''' </summary>
    '''
    ''' <remarks>
    ''' <c>Program.StyleMapping</c> shares the same <c>WallpaperConfig</c> instances across the whole process,
    ''' so they must not be mutable.
    ''' </remarks>
    '''
    ''' <param name="fieldName">
    ''' The field name.
    ''' </param>
    <TestMethod>
    <DataRow("WallpaperStyle")>
    <DataRow("TileWallpaper")>
    Public Sub RegistryValueFieldsAreReadOnly(fieldName As String)

        Dim field As FieldInfo = GetConfigField(fieldName)

        Assert.IsTrue(field.IsInitOnly, $"Field '{fieldName}' must be declared ReadOnly.")
        Assert.AreEqual(GetType(String), field.FieldType, $"Field '{fieldName}' must be a String (REG_SZ value).")
    End Sub

    ''' <summary>
    ''' Verifies that the class cannot be inherited and cannot be created without registry values.
    ''' </summary>
    <TestMethod>
    Public Sub ClassIsSealedAndHasNoAccessibleParameterlessConstructor()

        Dim configType As Type = GetProductionType(WallpaperConfigTypeName)
        Dim parameterlessConstructor As ConstructorInfo = configType.GetConstructor(
            AnyInstance, binder:=Nothing, types:=Type.EmptyTypes, modifiers:=Nothing)

        Assert.IsTrue(configType.IsSealed, "WallpaperConfig must be NotInheritable.")
        Assert.IsNotNull(parameterlessConstructor, "The private parameterless constructor was not found.")
        Assert.IsTrue(parameterlessConstructor.IsPrivate, "The parameterless constructor must be private.")
    End Sub

#End Region

#Region " ToIDesktopWallpaperPosition "

    ''' <summary>
    ''' Verifies the conversion from registry values to the native <c>DESKTOP_WALLPAPER_POSITION</c> value
    ''' (0 = Center, 1 = Tile, 2 = Stretch, 3 = Fit, 4 = Fill, 5 = Span).
    ''' </summary>
    '''
    ''' <remarks>
    ''' This covers combinations that <c>Program.StyleMapping</c> never produces, such as the raw
    ''' legacy <c>WallpaperStyle</c> "1", or <c>TileWallpaper</c> "1" combined with a non-center style:
    ''' only <c>WallpaperStyle</c> "0" is turned into Tile by <c>TileWallpaper</c> "1".
    ''' </remarks>
    '''
    ''' <param name="style">
    ''' The registry <c>WallpaperStyle</c> value.
    ''' </param>
    '''
    ''' <param name="tile">
    ''' The registry <c>TileWallpaper</c> value.
    ''' </param>
    '''
    ''' <param name="expectedPosition">
    ''' The expected native position value.
    ''' </param>
    <TestMethod>
    <DataRow("0", "0", 0)>
    <DataRow("0", "1", 1)>
    <DataRow("1", "0", 1)>
    <DataRow("1", "1", 1)>
    <DataRow("2", "0", 2)>
    <DataRow("2", "1", 2)>
    <DataRow("6", "0", 3)>
    <DataRow("6", "1", 3)>
    <DataRow("10", "0", 4)>
    <DataRow("10", "1", 4)>
    <DataRow("22", "0", 5)>
    <DataRow("22", "1", 5)>
    Public Sub RegistryValuesConvertToTheExpectedComPosition(style As String, tile As String, expectedPosition As Integer)

        Dim config As Object = CreateWallpaperConfig(style, tile)

        Assert.AreEqual(expectedPosition, GetDesktopWallpaperPosition(config), $"WallpaperStyle='{style}', TileWallpaper='{tile}'")
    End Sub

    ''' <summary>
    ''' Verifies the documented fallback to Center (0) for unrecognized registry style values.
    ''' Names are not registry values, and comparisons are exact (no trimming, no numeric parsing).
    ''' </summary>
    '''
    ''' <param name="style">
    ''' The unrecognized registry <c>WallpaperStyle</c> value.
    ''' </param>
    '''
    ''' <param name="tile">
    ''' The registry <c>TileWallpaper</c> value.
    ''' </param>
    <TestMethod>
    <DataRow("999", "0")>
    <DataRow("3", "0")>
    <DataRow("-1", "0")>
    <DataRow("fill", "0")>
    <DataRow("span", "0")>
    <DataRow(" 2", "0")>
    <DataRow("02", "0")>
    <DataRow("", "0")>
    <DataRow(Nothing, "0")>
    <DataRow(Nothing, Nothing)>
    <DataRow("999", "1")>
    Public Sub UnrecognizedRegistryStyleFallsBackToCenter(style As String, tile As String)

        Dim config As Object = CreateWallpaperConfig(style, tile)

        Assert.AreEqual(0, GetDesktopWallpaperPosition(config), $"WallpaperStyle='{style}', TileWallpaper='{tile}'")
    End Sub

    ''' <summary>
    ''' Verifies that only the exact <c>TileWallpaper</c> value "1" enables tiling for the center style.
    ''' </summary>
    '''
    ''' <param name="tile">
    ''' A <c>TileWallpaper</c> value other than "1".
    ''' </param>
    <TestMethod>
    <DataRow("0")>
    <DataRow("2")>
    <DataRow(" 1")>
    <DataRow("true")>
    <DataRow("")>
    <DataRow(Nothing)>
    Public Sub CenterStyleIsOnlyTiledByTheExactTileValueOne(tile As String)

        Dim config As Object = CreateWallpaperConfig("0", tile)

        Assert.AreEqual(0, GetDesktopWallpaperPosition(config), $"TileWallpaper='{tile}'")
    End Sub

    ''' <summary>
    ''' Verifies that the conversion is a pure function: repeated calls return the same value
    ''' and do not alter the stored registry values.
    ''' </summary>
    <TestMethod>
    Public Sub ConversionIsSideEffectFree()

        Dim config As Object = CreateWallpaperConfig("0", "1")

        Assert.AreEqual(1, GetDesktopWallpaperPosition(config))
        Assert.AreEqual(1, GetDesktopWallpaperPosition(config))
        Assert.AreEqual("0", GetWallpaperStyle(config))
        Assert.AreEqual("1", GetTileWallpaper(config))
    End Sub

#End Region

End Class
