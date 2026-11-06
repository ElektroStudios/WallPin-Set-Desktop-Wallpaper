Option Explicit On
Option Strict On
Option Infer Off

Imports System
Imports System.Collections
Imports System.IO
Imports Microsoft.VisualStudio.TestTools.UnitTesting

''' <summary>
''' End-to-end tests of the command-line validation performed by <c>Program.Main</c>,
''' running the real WallPin.exe in a child process and checking its exit code and error message.
''' </summary>
'''
''' <remarks>
''' <b>Safety:</b> every test in this class is designed to stop at a validation error (exit codes 1 to 5),
''' which <c>Program.Main</c> reports before it writes to the Windows Registry or changes the wallpaper.
''' The image files are empty placeholders created in a private temporary directory.
''' <para></para>
''' WallPin.exe runs in headless mode (<c>WALLPIN_HEADLESS=1</c>, set only for the child process), so it writes
''' its error messages to the standard error stream instead of showing message boxes.
''' These tests require Windows; on other platforms they are reported as inconclusive.
''' Exclude them with: <c>dotnet test --filter "TestCategory!=Integration"</c>
''' </remarks>
<TestClass>
<TestCategory("Integration")>
Public NotInheritable Class ProgramCommandLineTests

#Region " Constants "

    ''' <summary>
    ''' A style identifier that is never valid. Used to stop the program at the last validation step
    ''' (exit code 5), which proves that every previous validation step passed.
    ''' </summary>
    Private Const InvalidStyle As String = "not-a-style"

    ''' <summary>
    ''' The extended-length path prefix.
    ''' </summary>
    Private Const LongPathPrefix As String = "\\?\"

#End Region

#Region " Fields "

    ''' <summary>
    ''' The private temporary directory of the current test.
    ''' </summary>
    Private testDirectory As String

#End Region

#Region " Setup "

    ''' <summary>
    ''' Skips the test when WallPin.exe cannot run, and creates the private temporary directory.
    ''' </summary>
    <TestInitialize>
    Public Sub TestInitialize()

        If Not WallPinProcessRunner.CanRun Then
            Assert.Inconclusive("These tests require Windows.")
        End If

        Me.testDirectory = Path.Combine(Path.GetTempPath(), "WallPin.Tests", Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(Me.testDirectory)
    End Sub

    ''' <summary>
    ''' Deletes the private temporary directory, including any extended-length path inside it.
    ''' </summary>
    <TestCleanup>
    Public Sub TestCleanup()

        If Me.testDirectory IsNot Nothing AndAlso Directory.Exists(Me.testDirectory) Then
            Directory.Delete($"{LongPathPrefix}{Me.testDirectory}", recursive:=True)
        End If
    End Sub

#End Region

#Region " Argument Count (Exit Codes 1 and 2) "

    ''' <summary>
    ''' Verifies that running without arguments exits with code 1 and reports the usage instructions.
    ''' </summary>
    <TestMethod>
    Public Sub NoArgumentsExitsWithCode1AndShowsUsage()

        Dim result As WallPinRunResult = Me.RunWallPin()

        Assert.AreEqual(1, result.ExitCode)
        Assert.IsFalse(String.IsNullOrEmpty(result.MessageText), "An error message was expected on the standard error stream.")
        StringAssert.StartsWith(result.MessageText, "ERROR: Missing required arguments. Received: 0")
        StringAssert.Contains(result.MessageText, "USAGE:")
    End Sub

    ''' <summary>
    ''' Verifies that the usage instructions document every accepted style identifier and every supported extension,
    ''' so the help text cannot drift from the actual mappings.
    ''' </summary>
    <TestMethod>
    Public Sub UsageDocumentsEveryStyleIdentifierAndSupportedExtension()

        Dim result As WallPinRunResult = Me.RunWallPin()

        For Each identifier As String In GetStyleMapping().Keys
            StringAssert.Contains(result.MessageText, $"""{identifier}""", $"The usage text does not document the style '{identifier}'.")
        Next

        For Each extension As String In GetSupportedFileExtensions()
            StringAssert.Contains(result.MessageText, extension, $"The usage text does not document the extension '{extension}'.")
        Next
    End Sub

    ''' <summary>
    ''' Verifies that more than two arguments exit with code 2. The argument count is validated
    ''' before the file, so the (missing) image path is irrelevant.
    ''' </summary>
    '''
    ''' <param name="argumentCount">
    ''' The number of arguments to pass.
    ''' </param>
    <TestMethod>
    <DataRow(3)>
    <DataRow(5)>
    Public Sub TooManyArgumentsExitsWithCode2(argumentCount As Integer)

        Dim arguments(argumentCount - 1) As String
        For index As Integer = 0 To argumentCount - 1
            arguments(index) = $"argument{index}"
        Next

        Dim result As WallPinRunResult = Me.RunWallPin(arguments)

        Assert.AreEqual(2, result.ExitCode)
        StringAssert.StartsWith(result.MessageText, "ERROR: Too much arguments.")
        StringAssert.Contains(result.MessageText, $"received: {argumentCount}")
        StringAssert.Contains(result.MessageText, "USAGE:")
    End Sub

#End Region

#Region " Image File (Exit Codes 3 and 4) "

    ''' <summary>
    ''' Verifies that a missing image file exits with code 3 and reports its full path.
    ''' </summary>
    <TestMethod>
    Public Sub MissingImageFileExitsWithCode3()

        Dim missingPath As String = Path.Combine(Me.testDirectory, "missing.jpg")
        Dim result As WallPinRunResult = Me.RunWallPin(missingPath, "fill")

        Assert.AreEqual(3, result.ExitCode)
        StringAssert.Contains(result.MessageText, $"""{missingPath}""")
    End Sub

    ''' <summary>
    ''' Verifies that a directory is not accepted as an image file, even if its name has an image extension.
    ''' </summary>
    <TestMethod>
    Public Sub DirectoryWithImageExtensionExitsWithCode3()

        Dim directoryPath As String = Path.Combine(Me.testDirectory, "folder.jpg")
        Directory.CreateDirectory(directoryPath)

        Dim result As WallPinRunResult = Me.RunWallPin(directoryPath, InvalidStyle)

        Assert.AreEqual(3, result.ExitCode)
    End Sub

    ''' <summary>
    ''' Verifies that the file existence is validated before the style identifier.
    ''' </summary>
    <TestMethod>
    Public Sub MissingImageFileIsReportedBeforeAnInvalidStyle()

        Dim result As WallPinRunResult = Me.RunWallPin(Path.Combine(Me.testDirectory, "missing.jpg"), InvalidStyle)

        Assert.AreEqual(3, result.ExitCode)
    End Sub

    ''' <summary>
    ''' Verifies that an existing file with an unsupported extension exits with code 4 and reports the extension.
    ''' </summary>
    '''
    ''' <param name="fileName">
    ''' The file name to create.
    ''' </param>
    '''
    ''' <param name="expectedExtension">
    ''' The extension that must be reported.
    ''' </param>
    <TestMethod>
    <DataRow("image.txt", ".txt")>
    <DataRow("image.webp", ".webp")>
    <DataRow("image.jpg.txt", ".txt")>
    <DataRow("image", "")>
    Public Sub UnsupportedExtensionExitsWithCode4(fileName As String, expectedExtension As String)

        Dim result As WallPinRunResult = Me.RunWallPin(Me.CreateImageFile(fileName), "fill")

        Assert.AreEqual(4, result.ExitCode)
        StringAssert.Contains(result.MessageText, $"""{expectedExtension}"" is not a supported file format.")
    End Sub

    ''' <summary>
    ''' Verifies that the file extension is validated before the style identifier.
    ''' </summary>
    <TestMethod>
    Public Sub UnsupportedExtensionIsReportedBeforeAnInvalidStyle()

        Dim result As WallPinRunResult = Me.RunWallPin(Me.CreateImageFile("image.txt"), InvalidStyle)

        Assert.AreEqual(4, result.ExitCode)
    End Sub

    ''' <summary>
    ''' Verifies that every supported extension, in any letter case, passes the file validation
    ''' (the run stops later, at the invalid style).
    ''' </summary>
    '''
    ''' <param name="extension">
    ''' The file extension to test.
    ''' </param>
    <TestMethod>
    <DataRow(".bmp")>
    <DataRow(".gif")>
    <DataRow(".heic")>
    <DataRow(".heif")>
    <DataRow(".jfif")>
    <DataRow(".jpg")>
    <DataRow(".jpeg")>
    <DataRow(".png")>
    <DataRow(".tif")>
    <DataRow(".tiff")>
    <DataRow(".JPG")>
    <DataRow(".Png")>
    Public Sub SupportedExtensionPassesFileValidation(extension As String)

        Dim result As WallPinRunResult = Me.RunWallPin(Me.CreateImageFile($"image{extension}"), InvalidStyle)

        Assert.AreEqual(5, result.ExitCode, $"Extension '{extension}' did not pass the file validation.")
    End Sub

    ''' <summary>
    ''' Verifies that a relative image path is resolved against the process working directory.
    ''' </summary>
    '''
    ''' <param name="relativePath">
    ''' The relative path passed to the program.
    ''' </param>
    <TestMethod>
    <DataRow("image.jpg")>
    <DataRow(".\image.jpg")>
    <DataRow("subfolder\..\image.jpg")>
    Public Sub RelativeImagePathIsResolvedAgainstTheWorkingDirectory(relativePath As String)

        Me.CreateImageFile("image.jpg")

        Dim result As WallPinRunResult = Me.RunWallPin(relativePath, InvalidStyle)

        Assert.AreEqual(5, result.ExitCode, $"The relative path '{relativePath}' was not resolved.")
    End Sub

    ''' <summary>
    ''' Verifies that an image path longer than <c>MAX_PATH</c> is found
    ''' (the program checks its existence through the extended-length <c>\\?\</c> prefix).
    ''' </summary>
    <TestMethod>
    Public Sub ImagePathLongerThanMaxPathPassesFileValidation()

        Dim longDirectory As String = Me.testDirectory
        While longDirectory.Length < 300
            longDirectory = Path.Combine(longDirectory, New String("d"c, 50))
        End While

        Directory.CreateDirectory($"{LongPathPrefix}{longDirectory}")
        Dim imagePath As String = Path.Combine(longDirectory, "image.jpg")
        File.WriteAllBytes($"{LongPathPrefix}{imagePath}", Array.Empty(Of Byte)())

        Dim result As WallPinRunResult = Me.RunWallPin(imagePath, InvalidStyle)

        Assert.IsTrue(imagePath.Length > 260, $"The test path is not long enough: {imagePath.Length} characters.")
        Assert.AreEqual(5, result.ExitCode, "The long image path did not pass the file validation.")
    End Sub

#End Region

#Region " Wallpaper Style (Exit Code 5) "

    ''' <summary>
    ''' Verifies that an invalid style identifier exits with code 5, reports the identifier, and shows the usage.
    ''' Identifiers are not trimmed and only exact registry values are accepted.
    ''' </summary>
    '''
    ''' <param name="style">
    ''' The invalid style identifier.
    ''' </param>
    <TestMethod>
    <DataRow("invalid")>
    <DataRow("3")>
    <DataRow("-1")>
    <DataRow(" fill")>
    <DataRow("fill ")>
    <DataRow("stretched")>
    Public Sub InvalidStyleExitsWithCode5(style As String)

        Dim result As WallPinRunResult = Me.RunWallPin(Me.CreateImageFile("image.jpg"), style)

        Assert.AreEqual(5, result.ExitCode)
        StringAssert.StartsWith(result.MessageText, $"ERROR: Invalid WallpaperStyle identifier: ""{style}""")
        StringAssert.Contains(result.MessageText, "USAGE:")
    End Sub

#End Region

#Region " Private Helpers "

    ''' <summary>
    ''' Runs WallPin.exe in the test directory.
    ''' </summary>
    '''
    ''' <param name="arguments">
    ''' The command-line arguments.
    ''' </param>
    '''
    ''' <returns>
    ''' The run result.
    ''' </returns>
    '''
    ''' <exception cref="AssertFailedException">
    ''' Thrown when the process exits with code 0: a validation step was skipped.
    ''' </exception>
    Private Function RunWallPin(ParamArray arguments As String()) As WallPinRunResult

        Dim result As WallPinRunResult = WallPinProcessRunner.Run(arguments, Me.testDirectory)

        Assert.AreNotEqual(0, result.ExitCode,
                           $"WallPin.exe unexpectedly succeeded (a validation step was skipped). Arguments: {WallPinProcessRunner.BuildCommandLine(arguments)}")
        Return result
    End Function

    ''' <summary>
    ''' Creates an empty placeholder file in the test directory.
    ''' </summary>
    '''
    ''' <param name="fileName">
    ''' The file name.
    ''' </param>
    '''
    ''' <returns>
    ''' The full path of the created file.
    ''' </returns>
    Private Function CreateImageFile(fileName As String) As String

        Dim filePath As String = Path.Combine(Me.testDirectory, fileName)
        File.WriteAllBytes(filePath, Array.Empty(Of Byte)())
        Return filePath
    End Function

#End Region

End Class
