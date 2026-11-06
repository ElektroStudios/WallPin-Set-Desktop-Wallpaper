Option Explicit On
Option Strict On
Option Infer Off

Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks

''' <summary>
''' Represents the observable outcome of a WallPin.exe process run.
''' </summary>
Friend NotInheritable Class WallPinRunResult

    ''' <summary>
    ''' Gets the process exit code.
    ''' </summary>
    Friend ReadOnly ExitCode As Integer

    ''' <summary>
    ''' Gets the error message written by the process to the standard error stream
    ''' (the text that a normal run would show in a message box), without the trailing line break.
    ''' </summary>
    Friend ReadOnly MessageText As String

    ''' <summary>
    ''' Initializes a new instance of the <see cref="WallPinRunResult"/> class.
    ''' </summary>
    '''
    ''' <param name="exitCode">
    ''' The process exit code.
    ''' </param>
    '''
    ''' <param name="messageText">
    ''' The error message.
    ''' </param>
    Friend Sub New(exitCode As Integer, messageText As String)

        Me.ExitCode = exitCode
        Me.MessageText = messageText
    End Sub

End Class

''' <summary>
''' Runs the real WallPin.exe in a child process, in headless mode, and returns its exit code and error message.
''' </summary>
'''
''' <remarks>
''' The child process gets the <c>WALLPIN_HEADLESS=1</c> environment variable, so WallPin writes its error
''' messages to the standard error stream instead of showing a message box. Only the child process is affected:
''' the variable is never set for the current user or the machine.
''' </remarks>
Friend Module WallPinProcessRunner

#Region " Constants "

    ''' <summary>
    ''' The environment variable that enables WallPin's headless error reporting mode.
    ''' Must match <c>Program.WALLPIN_HEADLESS_ENVIRONMENT_VARIABLE</c>.
    ''' </summary>
    Friend Const HeadlessEnvironmentVariable As String = "WALLPIN_HEADLESS"

    ''' <summary>
    ''' The maximum time to wait for the process to exit.
    ''' </summary>
    '''
    ''' <remarks>
    ''' If headless mode ever stops working, WallPin blocks on a message box; the timeout turns that
    ''' into a clear test failure instead of a hung test run.
    ''' </remarks>
    Private ReadOnly ProcessTimeout As TimeSpan = TimeSpan.FromSeconds(20)

#End Region

#Region " Public Members "

    ''' <summary>
    ''' Gets a value indicating whether WallPin.exe can be run in the current environment (Windows).
    ''' </summary>
    Friend ReadOnly Property CanRun As Boolean
        Get
            Return Environment.OSVersion.Platform = PlatformID.Win32NT
        End Get
    End Property

    ''' <summary>
    ''' Runs WallPin.exe in headless mode with the specified arguments.
    ''' </summary>
    '''
    ''' <param name="arguments">
    ''' The command-line arguments. Each one is quoted as needed.
    ''' </param>
    '''
    ''' <param name="workingDirectory">
    ''' The working directory of the process.
    ''' </param>
    '''
    ''' <returns>
    ''' The run result.
    ''' </returns>
    '''
    ''' <exception cref="TimeoutException">
    ''' Thrown when the process does not exit in time (for example, because it is showing a message box).
    ''' The process is killed.
    ''' </exception>
    Friend Function Run(arguments As IEnumerable(Of String), workingDirectory As String) As WallPinRunResult

        Dim startInfo As New ProcessStartInfo(GetProductionAssembly().Location) With {
            .Arguments = BuildCommandLine(arguments),
            .WorkingDirectory = workingDirectory,
            .UseShellExecute = False,
            .CreateNoWindow = True,
            .RedirectStandardError = True,
            .StandardErrorEncoding = Encoding.UTF8
        }
        startInfo.EnvironmentVariables(HeadlessEnvironmentVariable) = "1"

        Using wallPinProcess As Process = Process.Start(startInfo)
            ' Read asynchronously, so a full pipe buffer can never deadlock the child process.
            Dim standardErrorTask As Task(Of String) = wallPinProcess.StandardError.ReadToEndAsync()

            If Not wallPinProcess.WaitForExit(CInt(ProcessTimeout.TotalMilliseconds)) Then
                wallPinProcess.Kill()
                wallPinProcess.WaitForExit()
                Throw New TimeoutException(
                    $"WallPin.exe did not exit within {ProcessTimeout.TotalSeconds} seconds (is it showing a message box?). Arguments: {startInfo.Arguments}")
            End If

            wallPinProcess.WaitForExit() ' Ensures the redirected stream has been fully read.
            Dim messageText As String = standardErrorTask.GetAwaiter().GetResult().TrimEnd(ControlChars.Cr, ControlChars.Lf)

            Return New WallPinRunResult(wallPinProcess.ExitCode, messageText)
        End Using
    End Function

    ''' <summary>
    ''' Builds a Windows command line from separate arguments, following the
    ''' <c>CommandLineToArgvW</c> quoting rules, so each argument reaches <c>Main</c> unchanged.
    ''' </summary>
    '''
    ''' <param name="arguments">
    ''' The arguments.
    ''' </param>
    '''
    ''' <returns>
    ''' The command line.
    ''' </returns>
    Friend Function BuildCommandLine(arguments As IEnumerable(Of String)) As String

        Return String.Join(" ", arguments.Select(AddressOf QuoteArgument))
    End Function

#End Region

#Region " Private Methods "

    ''' <summary>
    ''' Quotes a single command-line argument.
    ''' </summary>
    '''
    ''' <param name="argument">
    ''' The argument.
    ''' </param>
    '''
    ''' <returns>
    ''' The quoted argument.
    ''' </returns>
    Private Function QuoteArgument(argument As String) As String

        If argument.Length > 0 AndAlso argument.IndexOfAny(New Char() {" "c, ControlChars.Tab, """"c}) = -1 Then
            Return argument
        End If

        Dim builder As New StringBuilder("""")
        Dim backslashCount As Integer = 0

        For Each character As Char In argument
            If character = "\"c Then
                backslashCount += 1
            ElseIf character = """"c Then
                builder.Append("\"c, (backslashCount * 2) + 1).Append(""""c)
                backslashCount = 0
            Else
                builder.Append("\"c, backslashCount).Append(character)
                backslashCount = 0
            End If
        Next

        Return builder.Append("\"c, backslashCount * 2).Append(""""c).ToString()
    End Function

#End Region

End Module
