Option Explicit On
Option Strict On
Option Infer Off

Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Reflection
Imports System.Linq
Imports Microsoft.VisualStudio.TestTools.UnitTesting

''' <summary>
''' Verifies the command-line program's static style and file-format contracts.
''' </summary>
<TestClass>
Public NotInheritable Class ProgramContractTests

    Private Const ProgramTypeName As String = "Program"
    ''' <summary>
    ''' Verifies that every supported image extension is present exactly once.
    ''' </summary>
    <TestMethod>
    Public Sub SupportedFileExtensionsMatchTheExpectedSet()
        Dim extensionValues As Object = GetStaticFieldValue(ProgramTypeName, "SupportedFileExtensions")
        Dim extensionEnumerable As IEnumerable = DirectCast(extensionValues, IEnumerable)
        Dim actualExtensions As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

        For Each extensionValue As Object In extensionEnumerable
            Dim extension As String = CStr(extensionValue)
            Assert.DoesNotContain(extension, actualExtensions, actualExtensions.Comparer, $"Duplicate extension found: {extension}")
            actualExtensions.Add(extension)
        Next

        Dim expectedExtensions As String() = {
            ".bmp", ".gif", ".heic", ".heif", ".jfif",
            ".jpg", ".jpeg", ".png", ".tif", ".tiff"
        }

        Assert.AreSequenceEqual(expectedExtensions, actualExtensions.ToArray(), SequenceOrder.InAnyOrder)
        Assert.Contains(".JPG", actualExtensions, actualExtensions.Comparer, "The extension collection must be case-insensitive.")
        Assert.DoesNotContain(".webp", actualExtensions, actualExtensions.Comparer, "WEBP is not currently a supported format.")
    End Sub

    ''' <summary>
    ''' Verifies the supported wallpaper identifiers, their aliases, and default behavior.
    ''' </summary>
    <TestMethod>
    Public Sub StyleMappingContainsAllIdentifiersAndAliases()
        Dim styleMapping As IDictionary = DirectCast(
            GetStaticFieldValue(ProgramTypeName, "StyleMapping"),
            IDictionary)

        Dim expectedIdentifiers As String() = {
            "default",
            "0", "center",
            "1", "tile",
            "2", "stretch",
            "6", "fit",
            "10", "fill",
            "22", "span"
        }

        Assert.HasCount(expectedIdentifiers.Length, styleMapping.Keys)

        For Each identifier As String In expectedIdentifiers
            Assert.Contains(identifier, styleMapping.Keys, StringComparer.OrdinalIgnoreCase, $"Missing wallpaper style identifier: {identifier}")
        Next

        Assert.Contains("FILL", styleMapping.Keys, StringComparer.OrdinalIgnoreCase, "Style identifiers must be case-insensitive.")
        Assert.Contains("DeFaUlT", styleMapping.Keys, StringComparer.OrdinalIgnoreCase, "The default identifier must be case-insensitive.")
        Assert.DoesNotContain("invalid", styleMapping.Keys, StringComparer.OrdinalIgnoreCase, "Unknown identifiers must not be mapped.")
        Assert.IsNull(styleMapping("default"), "The default style must preserve the current Windows style.")
    End Sub

    ''' <summary>
    ''' Verifies registry values and COM position values for every named and numeric style alias.
    ''' </summary>
    <TestMethod>
    Public Sub EveryStyleAliasMapsToTheExpectedRegistryAndComValues()
        Dim styleMapping As IDictionary = DirectCast(
            GetStaticFieldValue(ProgramTypeName, "StyleMapping"),
            IDictionary)

        Dim identifiers As String() = {
            "0", "center",
            "1", "tile",
            "2", "stretch",
            "6", "fit",
            "10", "fill",
            "22", "span"
        }

        Dim expectedRegistryStyles As String() = {
            "0", "0",
            "0", "0",
            "2", "2",
            "6", "6",
            "10", "10",
            "22", "22"
        }

        Dim expectedTileValues As String() = {
            "0", "0",
            "1", "1",
            "0", "0",
            "0", "0",
            "0", "0",
            "0", "0"
        }

        Dim expectedComPositions As Integer() = {
            0, 0,
            1, 1,
            2, 2,
            3, 3,
            4, 4,
            5, 5
        }

        For index As Integer = 0 To identifiers.Length - 1
            Dim identifier As String = identifiers(index)
            Dim config As Object = styleMapping(identifier)
            Assert.IsNotNull(config, $"Style '{identifier}' must have a configuration.")

            Dim configType As Type = config.GetType()
            Dim instanceFlags As BindingFlags = BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.NonPublic
            Dim wallpaperStyleField As FieldInfo = configType.GetField("WallpaperStyle", instanceFlags)
            Dim tileWallpaperField As FieldInfo = configType.GetField("TileWallpaper", instanceFlags)
            Dim positionMethod As MethodInfo = configType.GetMethod("ToIDesktopWallpaperPosition", instanceFlags)

            Assert.IsNotNull(wallpaperStyleField, "WallpaperStyle field was not found.")
            Assert.IsNotNull(tileWallpaperField, "TileWallpaper field was not found.")
            Assert.IsNotNull(positionMethod, "ToIDesktopWallpaperPosition method was not found.")

            Assert.AreEqual(expectedRegistryStyles(index), CStr(wallpaperStyleField.GetValue(config)), identifier)
            Assert.AreEqual(expectedTileValues(index), CStr(tileWallpaperField.GetValue(config)), identifier)
            Assert.AreEqual(expectedComPositions(index), CInt(positionMethod.Invoke(config, Nothing)), identifier)
        Next
    End Sub

End Class
