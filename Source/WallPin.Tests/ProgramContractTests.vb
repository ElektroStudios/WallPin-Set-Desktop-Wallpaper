Option Explicit On
Option Strict On
Option Infer Off

Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Linq
Imports Microsoft.VisualStudio.TestTools.UnitTesting

''' <summary>
''' Verifies the command-line program's static style and file-format contracts.
''' </summary>
'''
''' <remarks>
''' Every lookup in this class goes through the production collection's own comparer
''' (<see cref="HashSet(Of T).Contains"/>, <see cref="IDictionary.Contains"/>, <c>TryGetValue</c>),
''' so a regression in the production comparer is actually detected.
''' </remarks>
<TestClass>
Public NotInheritable Class ProgramContractTests

#Region " Test Data "

    ''' <summary>
    ''' The image file extensions that WallPin must accept.
    ''' </summary>
    Private Shared ReadOnly ExpectedExtensions As String() = {
        ".bmp", ".gif", ".heic", ".heif", ".jfif",
        ".jpg", ".jpeg", ".png", ".tif", ".tiff"
    }

    ''' <summary>
    ''' Every accepted wallpaper style identifier.
    ''' </summary>
    Private Shared ReadOnly ExpectedStyleIdentifiers As String() = {
        "default",
        "0", "center",
        "1", "tile",
        "2", "stretch",
        "6", "fit",
        "10", "fill",
        "22", "span"
    }

#End Region

#Region " Supported File Extensions "

    ''' <summary>
    ''' Verifies that the production collection contains exactly the expected extensions.
    ''' </summary>
    <TestMethod>
    Public Sub SupportedFileExtensionsMatchTheExpectedSet()

        Dim actualExtensions As HashSet(Of String) = GetSupportedFileExtensions()

        Assert.IsTrue(actualExtensions.SetEquals(ExpectedExtensions),
                      $"Actual extensions: {String.Join(", ", actualExtensions)}")
        Assert.HasCount(ExpectedExtensions.Length, actualExtensions)
    End Sub

    ''' <summary>
    ''' Verifies that every extension is stored in its canonical form: lowercase and with a leading dot,
    ''' because <c>Path.GetExtension</c> returns the dot and the usage message prints these values.
    ''' </summary>
    <TestMethod>
    Public Sub SupportedFileExtensionsAreLowercaseAndDotPrefixed()

        For Each extension As String In GetSupportedFileExtensions()
            Assert.IsTrue(extension.StartsWith(".", StringComparison.Ordinal), $"Missing leading dot: '{extension}'")
            Assert.AreEqual(extension.ToLowerInvariant(), extension, $"Extension is not lowercase: '{extension}'")
            Assert.AreEqual(extension.Trim(), extension, $"Extension has surrounding whitespace: '{extension}'")
        Next
    End Sub

    ''' <summary>
    ''' Verifies that the production collection itself (not a copy) matches extensions case-insensitively.
    ''' </summary>
    '''
    ''' <param name="extension">
    ''' An extension written with non-canonical casing.
    ''' </param>
    <TestMethod>
    <DataRow(".JPG")>
    <DataRow(".Jpeg")>
    <DataRow(".PNG")>
    <DataRow(".TiFf")>
    <DataRow(".HEIC")>
    Public Sub SupportedFileExtensionsAreCaseInsensitive(extension As String)

        Assert.IsTrue(GetSupportedFileExtensions().Contains(extension),
                      $"The production extension set must match '{extension}' case-insensitively.")
    End Sub

    ''' <summary>
    ''' Verifies that common but unsupported formats, and malformed extensions, are rejected.
    ''' </summary>
    '''
    ''' <param name="extension">
    ''' An extension that must not be accepted.
    ''' </param>
    <TestMethod>
    <DataRow(".webp")>
    <DataRow(".avif")>
    <DataRow(".ico")>
    <DataRow(".svg")>
    <DataRow(".txt")>
    <DataRow("jpg")>
    <DataRow(".jpg ")>
    <DataRow("")>
    Public Sub UnsupportedFileExtensionsAreRejected(extension As String)

        Assert.IsFalse(GetSupportedFileExtensions().Contains(extension),
                       $"The extension '{extension}' must not be supported.")
    End Sub

#End Region

#Region " Headless Mode "

    ''' <summary>
    ''' Verifies that the headless-mode environment variable used by the test runner
    ''' matches the one read by the program, so the command-line tests never fall back to message boxes.
    ''' </summary>
    <TestMethod>
    Public Sub HeadlessEnvironmentVariableMatchesTheTestRunner()

        Dim variableName As String = CStr(GetStaticFieldValue(ProgramTypeName, "WALLPIN_HEADLESS_ENVIRONMENT_VARIABLE"))

        Assert.AreEqual(WallPinProcessRunner.HeadlessEnvironmentVariable, variableName)
    End Sub

#End Region

#Region " Style Mapping "

    ''' <summary>
    ''' Verifies that the style mapping contains exactly the expected identifiers.
    ''' </summary>
    <TestMethod>
    Public Sub StyleMappingContainsExactlyTheExpectedIdentifiers()

        Dim styleMapping As IDictionary = GetStyleMapping()
        Dim actualIdentifiers As String() = styleMapping.Keys.Cast(Of String)().ToArray()

        Assert.AreSequenceEqual(ExpectedStyleIdentifiers, actualIdentifiers, SequenceOrder.InAnyOrder)
    End Sub

    ''' <summary>
    ''' Verifies that the production dictionary itself resolves identifiers case-insensitively.
    ''' </summary>
    '''
    ''' <param name="identifier">
    ''' An identifier written with non-canonical casing.
    ''' </param>
    <TestMethod>
    <DataRow("DEFAULT")>
    <DataRow("DeFaUlT")>
    <DataRow("Center")>
    <DataRow("TILE")>
    <DataRow("Stretch")>
    <DataRow("FIT")>
    <DataRow("FiLl")>
    <DataRow("SPAN")>
    Public Sub StyleMappingIsCaseInsensitive(identifier As String)

        Dim config As Object = Nothing

        Assert.IsTrue(GetStyleMapping().Contains(identifier),
                      $"The production style mapping must resolve '{identifier}' case-insensitively.")
        Assert.IsTrue(TryGetStyle(identifier, config))
    End Sub

    ''' <summary>
    ''' Verifies that unknown identifiers are rejected, including unmapped registry values,
    ''' whitespace-padded names, and combined values. <c>Program.Main</c> does not trim its arguments.
    ''' </summary>
    '''
    ''' <param name="identifier">
    ''' An identifier that must not be accepted.
    ''' </param>
    <TestMethod>
    <DataRow("invalid")>
    <DataRow("3")>
    <DataRow("4")>
    <DataRow("5")>
    <DataRow("7")>
    <DataRow("-1")>
    <DataRow("010")>
    <DataRow(" fill")>
    <DataRow("fill ")>
    <DataRow("center,tile")>
    <DataRow("stretched")>
    <DataRow("")>
    Public Sub StyleMappingRejectsUnknownIdentifiers(identifier As String)

        Dim config As Object = Nothing

        Assert.IsFalse(TryGetStyle(identifier, config), $"The identifier '{identifier}' must not be mapped.")
        Assert.IsNull(config)
    End Sub

    ''' <summary>
    ''' Verifies that the "default" identifier is present and maps to <see langword="Nothing"/>,
    ''' which means "keep the current system style".
    ''' </summary>
    '''
    ''' <remarks>
    ''' A non-generic <see cref="IDictionary"/> indexer returns <see langword="Nothing"/> for a missing key,
    ''' so presence must be asserted separately from the value.
    ''' </remarks>
    <TestMethod>
    Public Sub DefaultStyleIsPresentAndPreservesTheCurrentSystemStyle()

        Dim config As Object = New Object()

        Assert.IsTrue(TryGetStyle("default", config), "The 'default' identifier must be mapped.")
        Assert.IsNull(config, "The 'default' identifier must map to Nothing (keep the current Windows style).")
    End Sub

    ''' <summary>
    ''' Verifies the registry values and COM position value for every named and numeric style identifier.
    ''' </summary>
    '''
    ''' <param name="identifier">
    ''' The style identifier.
    ''' </param>
    '''
    ''' <param name="expectedStyle">
    ''' The expected registry <c>WallpaperStyle</c> value.
    ''' </param>
    '''
    ''' <param name="expectedTile">
    ''' The expected registry <c>TileWallpaper</c> value.
    ''' </param>
    '''
    ''' <param name="expectedPosition">
    ''' The expected native <c>DESKTOP_WALLPAPER_POSITION</c> value.
    ''' </param>
    <TestMethod>
    <DataRow("0", "0", "0", 0)>
    <DataRow("center", "0", "0", 0)>
    <DataRow("1", "0", "1", 1)>
    <DataRow("tile", "0", "1", 1)>
    <DataRow("2", "2", "0", 2)>
    <DataRow("stretch", "2", "0", 2)>
    <DataRow("6", "6", "0", 3)>
    <DataRow("fit", "6", "0", 3)>
    <DataRow("10", "10", "0", 4)>
    <DataRow("fill", "10", "0", 4)>
    <DataRow("22", "22", "0", 5)>
    <DataRow("span", "22", "0", 5)>
    Public Sub StyleIdentifierMapsToTheExpectedRegistryAndComValues(identifier As String,
                                                                     expectedStyle As String,
                                                                     expectedTile As String,
                                                                     expectedPosition As Integer)
        Dim config As Object = Nothing

        Assert.IsTrue(TryGetStyle(identifier, config), $"Missing style identifier: '{identifier}'")
        Assert.IsNotNull(config, $"Style '{identifier}' must have a configuration.")
        Assert.AreEqual(expectedStyle, GetWallpaperStyle(config), $"WallpaperStyle of '{identifier}'")
        Assert.AreEqual(expectedTile, GetTileWallpaper(config), $"TileWallpaper of '{identifier}'")
        Assert.AreEqual(expectedPosition, GetDesktopWallpaperPosition(config), $"COM position of '{identifier}'")
    End Sub

    ''' <summary>
    ''' Verifies that each numeric identifier and its name alias resolve to equivalent configurations.
    ''' </summary>
    '''
    ''' <param name="numericIdentifier">
    ''' The numeric identifier.
    ''' </param>
    '''
    ''' <param name="nameIdentifier">
    ''' The name alias.
    ''' </param>
    <TestMethod>
    <DataRow("0", "center")>
    <DataRow("1", "tile")>
    <DataRow("2", "stretch")>
    <DataRow("6", "fit")>
    <DataRow("10", "fill")>
    <DataRow("22", "span")>
    Public Sub NumericIdentifierAndNameAliasAreEquivalent(numericIdentifier As String, nameIdentifier As String)

        Dim numericConfig As Object = Nothing
        Dim nameConfig As Object = Nothing

        Assert.IsTrue(TryGetStyle(numericIdentifier, numericConfig))
        Assert.IsTrue(TryGetStyle(nameIdentifier, nameConfig))
        Assert.AreEqual(GetWallpaperStyle(numericConfig), GetWallpaperStyle(nameConfig))
        Assert.AreEqual(GetTileWallpaper(numericConfig), GetTileWallpaper(nameConfig))
        Assert.AreEqual(GetDesktopWallpaperPosition(numericConfig), GetDesktopWallpaperPosition(nameConfig))
    End Sub

    ''' <summary>
    ''' Verifies that the six non-default styles produce six distinct COM positions,
    ''' so no two styles silently collapse into the same layout.
    ''' </summary>
    <TestMethod>
    Public Sub NonDefaultStylesMapToDistinctComPositions()

        Dim positions As New HashSet(Of Integer)()

        For Each entry As DictionaryEntry In GetStyleMapping()
            If entry.Value IsNot Nothing Then
                positions.Add(GetDesktopWallpaperPosition(entry.Value))
            End If
        Next

        Assert.AreSequenceEqual(New Integer() {0, 1, 2, 3, 4, 5}, positions.ToArray(), SequenceOrder.InAnyOrder)
    End Sub

#End Region

End Class
