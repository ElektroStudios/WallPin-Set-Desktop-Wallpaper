Option Explicit On
Option Strict On
Option Infer Off

Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Text.RegularExpressions
Imports Microsoft.VisualStudio.TestTools.UnitTesting

''' <summary>
''' Verifies that the Explorer context-menu scripts in "Shell Integration" stay in sync with the program:
''' the registered file extensions, the style arguments passed to WallPin.exe, and the referenced icons.
''' </summary>
'''
''' <remarks>
''' The scripts are only parsed, never executed: these tests do not touch the Windows Registry.
''' The test project copies the scripts and icons to its output directory (see <c>WallPin.Tests.vbproj</c>).
''' </remarks>
<TestClass>
Public NotInheritable Class ShellIntegrationContractTests

#Region " Constants "

    ''' <summary>
    ''' The output subdirectory where the test project copies the "Shell Integration" files.
    ''' </summary>
    Private Const ShellIntegrationDirectoryName As String = "ShellIntegration"

    ''' <summary>
    ''' The file name of the core registration script.
    ''' </summary>
    Private Const CoreScriptName As String = "Add registry keys [core].bat"

    ''' <summary>
    ''' The file name of the script that removes the registry keys.
    ''' </summary>
    Private Const RestoreScriptName As String = "Restore Windows default registry keys.bat"

    ''' <summary>
    ''' The file name of the English registration script.
    ''' </summary>
    Private Const EnglishScriptName As String = "Add registry keys [english].bat"

    ''' <summary>
    ''' The file name of the Spanish registration script.
    ''' </summary>
    Private Const SpanishScriptName As String = "Add registry keys [spanish].bat"

#End Region

#Region " Nested Types "

    ''' <summary>
    ''' Represents one context sub-menu item, defined in a language script as
    ''' <c>SET "ItemN=RegistryKeyName|MenuDisplayText|IconFile|Arguments"</c>.
    ''' </summary>
    Private NotInheritable Class MenuItemDefinition

        ''' <summary>
        ''' Gets the item index (the N in <c>ItemN</c>).
        ''' </summary>
        Friend ReadOnly Index As Integer

        ''' <summary>
        ''' Gets the registry sub-key name.
        ''' </summary>
        Friend ReadOnly RegistryKeyName As String

        ''' <summary>
        ''' Gets the menu display text.
        ''' </summary>
        Friend ReadOnly DisplayText As String

        ''' <summary>
        ''' Gets the icon file name.
        ''' </summary>
        Friend ReadOnly IconFile As String

        ''' <summary>
        ''' Gets the argument passed to WallPin.exe (the wallpaper style identifier).
        ''' </summary>
        Friend ReadOnly StyleArgument As String

        ''' <summary>
        ''' Initializes a new instance of the <see cref="MenuItemDefinition"/> class.
        ''' </summary>
        '''
        ''' <param name="index">
        ''' The item index.
        ''' </param>
        '''
        ''' <param name="definition">
        ''' The pipe-separated item definition.
        ''' </param>
        '''
        ''' <exception cref="FormatException">
        ''' Thrown when the definition does not have exactly four fields.
        ''' </exception>
        Friend Sub New(index As Integer, definition As String)

            Dim fields As String() = definition.Split("|"c)
            If fields.Length <> 4 Then
                Throw New FormatException($"Item{index} must have 4 pipe-separated fields: ""{definition}""")
            End If

            Me.Index = index
            Me.RegistryKeyName = fields(0)
            Me.DisplayText = fields(1)
            Me.IconFile = fields(2)
            Me.StyleArgument = fields(3)
        End Sub

    End Class

#End Region

#Region " File Extensions "

    ''' <summary>
    ''' Verifies that the scripts register (and unregister) exactly the extensions supported by the program,
    ''' so the menu never appears for a file that WallPin would reject, and every supported file gets the menu.
    ''' </summary>
    '''
    ''' <param name="scriptName">
    ''' The script file name.
    ''' </param>
    <TestMethod>
    <DataRow(CoreScriptName)>
    <DataRow(RestoreScriptName)>
    Public Sub ScriptFileExtensionsMatchTheProgramSupportedExtensions(scriptName As String)

        Dim scriptExtensions As String() = ParseFileExtensions(ReadScript(scriptName))

        Assert.HasCount(scriptExtensions.Distinct(StringComparer.OrdinalIgnoreCase).Count(), scriptExtensions,
                        $"'{scriptName}' contains duplicate extensions.")
        Assert.IsTrue(GetSupportedFileExtensions().SetEquals(scriptExtensions),
                      $"'{scriptName}' extensions: {String.Join(", ", scriptExtensions)}")
    End Sub

#End Region

#Region " Core Script "

    ''' <summary>
    ''' Verifies that the core script launches the production executable from the parent folder.
    ''' </summary>
    <TestMethod>
    Public Sub CoreScriptTargetsTheProductionExecutable()

        Dim script As String = ReadScript(CoreScriptName)
        Dim exeMatch As Match = Regex.Match(script, "^\s*SET\s+""Exe=%~dp0\.\.\\(?<name>[^""]+)""", RegexOptions.Multiline Or RegexOptions.IgnoreCase)
        Dim productionExeName As String = $"{GetProductionAssembly().GetName().Name}.exe"

        Assert.IsTrue(exeMatch.Success, "The 'Exe' variable was not found in the core script.")
        Assert.AreEqual(productionExeName, exeMatch.Groups("name").Value, ignoreCase:=True)
    End Sub

    ''' <summary>
    ''' Verifies that the menu only applies to a single selected file, because WallPin accepts a single image path.
    ''' </summary>
    <TestMethod>
    Public Sub CoreScriptRestrictsTheMenuToSingleSelection()

        Dim script As String = ReadScript(CoreScriptName)

        Assert.IsTrue(Regex.IsMatch(script, """MultiSelectModel""\s+/T\s+""REG_SZ""\s+/D\s+""Single""", RegexOptions.IgnoreCase),
                      "MultiSelectModel must be 'Single'.")
    End Sub

    ''' <summary>
    ''' Verifies that the command line passes the selected file and the style as two quoted arguments,
    ''' matching the two arguments expected by <c>Program.Main</c>.
    ''' </summary>
    <TestMethod>
    Public Sub CoreScriptPassesTheFileAndTheStyleAsTwoQuotedArguments()

        Dim script As String = ReadScript(CoreScriptName)

        StringAssert.Contains(script, "/D ""\""%Exe%\"" \""%%%%1\"" \""%%D\""""",
                              "The command must be: ""<Exe>"" ""<file>"" ""<style>"".")
    End Sub

#End Region

#Region " Language Scripts "

    ''' <summary>
    ''' Verifies that every menu item passes a style identifier that the program accepts,
    ''' and that every named style (plus "default") is exposed exactly once.
    ''' </summary>
    '''
    ''' <param name="scriptName">
    ''' The language script file name.
    ''' </param>
    <TestMethod>
    <DataRow(EnglishScriptName)>
    <DataRow(SpanishScriptName)>
    Public Sub LanguageScriptPassesValidStyleArguments(scriptName As String)

        Dim items As MenuItemDefinition() = ParseMenuItems(ReadScript(scriptName))
        Dim arguments As String() = items.Select(Function(item As MenuItemDefinition) item.StyleArgument).ToArray()

        For Each argument As String In arguments
            Dim config As Object = Nothing
            Assert.IsTrue(TryGetStyle(argument, config), $"'{scriptName}' passes an invalid style: '{argument}'")
        Next

        Assert.AreSequenceEqual(New String() {"default", "center", "tile", "stretch", "fit", "fill", "span"},
                                arguments, SequenceOrder.InAnyOrder, $"'{scriptName}' style arguments")
    End Sub

    ''' <summary>
    ''' Verifies that the item indices are contiguous and match the <c>FOR /L</c> range of the core script,
    ''' otherwise items would be silently skipped or empty registry keys would be written.
    ''' </summary>
    '''
    ''' <param name="scriptName">
    ''' The language script file name.
    ''' </param>
    <TestMethod>
    <DataRow(EnglishScriptName)>
    <DataRow(SpanishScriptName)>
    Public Sub LanguageScriptItemsMatchTheCoreLoopRange(scriptName As String)

        Dim loopMatch As Match = Regex.Match(ReadScript(CoreScriptName),
                                             "FOR\s+/L\s+%%I\s+IN\s+\((?<start>\d+),(?<step>\d+),(?<end>\d+)\)",
                                             RegexOptions.IgnoreCase)
        Assert.IsTrue(loopMatch.Success, "The 'FOR /L' loop was not found in the core script.")

        Dim loopStart As Integer = Integer.Parse(loopMatch.Groups("start").Value)
        Dim loopStep As Integer = Integer.Parse(loopMatch.Groups("step").Value)
        Dim loopEnd As Integer = Integer.Parse(loopMatch.Groups("end").Value)
        Dim expectedIndices As Integer() = Enumerable.Range(loopStart, loopEnd - loopStart + 1).Where(
            Function(value As Integer) (value - loopStart) Mod loopStep = 0).ToArray()

        Dim actualIndices As Integer() = ParseMenuItems(ReadScript(scriptName)).Select(Function(item As MenuItemDefinition) item.Index).ToArray()

        Assert.AreSequenceEqual(expectedIndices, actualIndices, SequenceOrder.InAnyOrder)
    End Sub

    ''' <summary>
    ''' Verifies that every icon referenced by a menu item exists in the "Icons" folder.
    ''' </summary>
    '''
    ''' <param name="scriptName">
    ''' The language script file name.
    ''' </param>
    <TestMethod>
    <DataRow(EnglishScriptName)>
    <DataRow(SpanishScriptName)>
    Public Sub LanguageScriptIconsExist(scriptName As String)

        Dim iconsDirectory As String = Path.Combine(GetShellIntegrationDirectory(), "Icons")

        For Each item As MenuItemDefinition In ParseMenuItems(ReadScript(scriptName))
            Dim iconPath As String = Path.Combine(iconsDirectory, item.IconFile)
            Assert.IsTrue(File.Exists(iconPath), $"Item{item.Index} references a missing icon: '{item.IconFile}'")
        Next
    End Sub

    ''' <summary>
    ''' Verifies that the menu display texts are not empty and that every registry sub-key name is unique.
    ''' </summary>
    '''
    ''' <param name="scriptName">
    ''' The language script file name.
    ''' </param>
    <TestMethod>
    <DataRow(EnglishScriptName)>
    <DataRow(SpanishScriptName)>
    Public Sub LanguageScriptItemsHaveUniqueKeysAndDisplayTexts(scriptName As String)

        Dim items As MenuItemDefinition() = ParseMenuItems(ReadScript(scriptName))

        For Each item As MenuItemDefinition In items
            Assert.IsFalse(String.IsNullOrWhiteSpace(item.DisplayText), $"Item{item.Index} has an empty display text.")
            Assert.IsFalse(String.IsNullOrWhiteSpace(item.RegistryKeyName), $"Item{item.Index} has an empty registry key name.")
        Next

        Assert.HasCount(items.Length, items.Select(Function(item As MenuItemDefinition) item.RegistryKeyName).Distinct(StringComparer.OrdinalIgnoreCase),
                        "Registry sub-key names must be unique (registry keys are case-insensitive).")
    End Sub

    ''' <summary>
    ''' Verifies that the translations only differ in the display texts: the registry keys, icons, and style arguments
    ''' must be identical, so switching the language replaces the same keys.
    ''' </summary>
    <TestMethod>
    Public Sub LanguageScriptsOnlyDifferInTheirDisplayTexts()

        Dim englishItems As MenuItemDefinition() = ParseMenuItems(ReadScript(EnglishScriptName))
        Dim spanishItems As MenuItemDefinition() = ParseMenuItems(ReadScript(SpanishScriptName))

        Assert.HasCount(englishItems.Length, spanishItems)

        For index As Integer = 0 To englishItems.Length - 1
            Assert.AreEqual(englishItems(index).Index, spanishItems(index).Index)
            Assert.AreEqual(englishItems(index).RegistryKeyName, spanishItems(index).RegistryKeyName, $"Item{englishItems(index).Index} registry key")
            Assert.AreEqual(englishItems(index).IconFile, spanishItems(index).IconFile, $"Item{englishItems(index).Index} icon")
            Assert.AreEqual(englishItems(index).StyleArgument, spanishItems(index).StyleArgument, $"Item{englishItems(index).Index} style")
        Next
    End Sub

#End Region

#Region " Private Helpers "

    ''' <summary>
    ''' Gets the directory where the test project copies the "Shell Integration" files.
    ''' </summary>
    '''
    ''' <returns>
    ''' The full directory path.
    ''' </returns>
    '''
    ''' <exception cref="DirectoryNotFoundException">
    ''' Thrown when the files were not copied to the test output directory.
    ''' </exception>
    Private Shared Function GetShellIntegrationDirectory() As String

        Dim testDirectory As String = Path.GetDirectoryName(GetType(ShellIntegrationContractTests).Assembly.Location)
        Dim directoryPath As String = Path.Combine(testDirectory, ShellIntegrationDirectoryName)

        If Not Directory.Exists(directoryPath) Then
            Throw New DirectoryNotFoundException($"The Shell Integration files were not copied to the test output: ""{directoryPath}""")
        End If

        Return directoryPath
    End Function

    ''' <summary>
    ''' Reads a script from the "Shell Integration" test output directory.
    ''' </summary>
    '''
    ''' <param name="scriptName">
    ''' The script file name.
    ''' </param>
    '''
    ''' <returns>
    ''' The script contents.
    ''' </returns>
    '''
    ''' <exception cref="FileNotFoundException">
    ''' Thrown when the script does not exist.
    ''' </exception>
    Private Shared Function ReadScript(scriptName As String) As String

        Dim scriptPath As String = Path.Combine(GetShellIntegrationDirectory(), scriptName)
        If Not File.Exists(scriptPath) Then
            Throw New FileNotFoundException("The Shell Integration script was not found.", scriptPath)
        End If

        Return File.ReadAllText(scriptPath)
    End Function

    ''' <summary>
    ''' Parses the comma-separated <c>FileExtensions</c> variable of a script.
    ''' </summary>
    '''
    ''' <param name="script">
    ''' The script contents.
    ''' </param>
    '''
    ''' <returns>
    ''' The file extensions, trimmed.
    ''' </returns>
    '''
    ''' <exception cref="FormatException">
    ''' Thrown when the variable is not defined exactly once.
    ''' </exception>
    Private Shared Function ParseFileExtensions(script As String) As String()

        Dim matches As MatchCollection = Regex.Matches(script, "^\s*SET\s+""FileExtensions=(?<value>[^""]*)""", RegexOptions.Multiline Or RegexOptions.IgnoreCase)
        If matches.Count <> 1 Then
            Throw New FormatException($"Expected exactly one 'FileExtensions' definition, found {matches.Count}.")
        End If

        Return matches(0).Groups("value").Value.
            Split(","c).
            Select(Function(extension As String) extension.Trim()).
            ToArray()
    End Function

    ''' <summary>
    ''' Parses the <c>ItemN</c> menu item definitions of a language script, sorted by index.
    ''' </summary>
    '''
    ''' <param name="script">
    ''' The script contents.
    ''' </param>
    '''
    ''' <returns>
    ''' The menu item definitions.
    ''' </returns>
    '''
    ''' <exception cref="FormatException">
    ''' Thrown when no item is defined, or an index is defined more than once.
    ''' </exception>
    Private Shared Function ParseMenuItems(script As String) As MenuItemDefinition()

        Dim items As New List(Of MenuItemDefinition)()

        For Each itemMatch As Match In Regex.Matches(script, "^\s*SET\s+""Item(?<index>\d+)=(?<value>[^""]*)""", RegexOptions.Multiline Or RegexOptions.IgnoreCase)
            items.Add(New MenuItemDefinition(Integer.Parse(itemMatch.Groups("index").Value), itemMatch.Groups("value").Value))
        Next

        If items.Count = 0 Then
            Throw New FormatException("No 'ItemN' definitions were found.")
        End If

        If items.Select(Function(item As MenuItemDefinition) item.Index).Distinct().Count() <> items.Count Then
            Throw New FormatException("An 'ItemN' index is defined more than once.")
        End If

        Return items.OrderBy(Function(item As MenuItemDefinition) item.Index).ToArray()
    End Function

#End Region

End Class
