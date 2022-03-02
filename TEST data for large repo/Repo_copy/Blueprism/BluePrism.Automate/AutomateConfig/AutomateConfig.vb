
Imports System.Globalization
Imports System.Threading
Imports BluePrism.BPCoreLib
Imports BluePrism.AutomateAppCore
Imports BluePrism.StartUp
Imports BluePrism.Server.Domain.Models

''' <summary>
''' Main entry point for the automateconfig project.
''' Usage is basically :-
'''
''' automateconfig ?
''' automateconfig help
''' automateconfig /help
''' automateconfig /h
'''   : All write the usage for automateconfig to stdout (which, incidentally, isn't
'''   :   written to the console if run from a command prompt - it needs to be
'''   :   redirected. Kinda pointless, really. I may dump it.
''' 
''' automateconfig [connections]
'''   : Shows the database config (for all users) - this is the default operation
'''   :   if no argument is given to automateconfig
'''   :
'''   : A response code of 0 indicates that the user OK'ed the form (and the file
'''   :   was saved.
'''   : A response code of 101 indicates that the form return DialogResult.None as
'''   :   its dialog result.
'''   : Any other response code is the integer value of the dialog result returned
'''   :   by the connections config dialog.
'''   : If any exceptions were encountered, the exception detail is written to
'''   :   stdout and a value of DialogResult.Abort (3) is returned
''' 
''' automateconfig daemon {processid}
'''   : Starts a form which runs a daemon serving a .net remoting channel over
'''   :   named pipes, available to the calling process with the URI:
'''   :   ipc://automateconfig-server-{processid}/ConfigManager
'''   :   The processid parameter should represent the ID of the calling process.
'''   :   This provides a mechanism for loading and saving the local machine options
'''   :   over a secure channel - the type exposed by this service is the
'''   :   <see cref="ConfigManager"/> class.
'''   : The daemon is exited by calling Exit() or Dispose() on the ConfigManager
'''   :   object returned.
'''   : If the process returns immediately with exit code 1, this indicates a usage
'''   :   error (ie. invalid arguments). If it returns with exit code 2, that
'''   :   indicates that the given process ID was not found to be running.
''' 
''' </summary>
Public Class AutomateConfig

    ''' <summary>
    ''' Runs the AutomateConfig program.
    ''' This runs the connections dialog box and exits with the dialog result
    ''' from the dialog - ie. DialogResult.OK, DialogResult.Cancel.
    ''' If any errors occurred, this will return DialogResult.Abort
    ''' </summary>
    ''' <param name="args">The arguments for this config</param>
    ''' <returns>The exit code for this command - the dialog result from the
    ''' connections dialog</returns>
    <STAThread>
    Shared Function Main(ByVal args() As String) As Integer

        ' Initialise dependency injection container
        ContainerInitialiser.SetUpContainer()
        RegexTimeout.SetDefaultRegexTimeout()

        Try
            'Set the thread culture as we may be on a background worker
            Dim configOptions = Options.Instance
            configOptions.Init(ConfigLocator.Instance)
            Thread.CurrentThread.CurrentUICulture = New CultureInfo(configOptions.CurrentLocale)
            Thread.CurrentThread.CurrentCulture = New CultureInfo(configOptions.CurrentLocale)
        Catch
            'Move on with the current culture
        End Try

        If args.Length = 0 Then args = New String() {"connections"}
        Select Case args(0).ToLower()

            Case "?", "help", "/help", "/h" : Return HandleUsage()

            Case "connections" : Return HandleDatabaseConfig()

            Case "mksrc"
                If args.Length < 2 Then
                    Err(My.Resources.UsageAutomateConfigMksrcSourcename)
                    Return 1
                End If
                Try
                    EventLogHelper.CreateDefaultLog()
                    EventLogHelper.CreateSource(args(1), EventLogHelper.DefaultLogName)
                Catch ofe As OperationFailedException
                    Err(ofe.ToString())
                    Return 2
                End Try

            Case "daemon"
                Dim procId As Integer = 0
                If args.Length < 2 OrElse Not Integer.TryParse(args(1), procId) Then
                    Err(My.Resources.UsageAutomateConfigDaemonProcessId)
                    Return 1
                End If

                ' Test that the process is there and exists
                Try
                    Process.GetProcessById(procId)
                Catch ae As ArgumentException
                    Err(My.Resources.PID0DoesNotRepresentARunningProcess, procId)
                    Return 2
                End Try
                Return HandleDaemon(procId)

        End Select

    End Function

    ''' <summary>
    ''' Handles the usage being requested. Since this application is primarily aimed
    ''' at being called from other projects rather than directly by the user, it's
    ''' rather technical but useful for the developer rather than the user.
    ''' </summary>
    ''' <returns></returns>
    Private Shared Function HandleUsage() As Integer
        Out(My.Resources.usage_help)
        Return 0
    End Function

    ''' <summary>
    ''' Shows the connection config form, and returns the exit code from the form.
    ''' </summary>
    ''' <returns><list>
    ''' <item>0 if the user OK'ed the form</item>
    ''' <item>101 if the form returned with <see cref="DialogResult.None"/></item>
    ''' <item>Otherwise, the <see cref="DialogResult"/> returned by the form</item>
    ''' </list>
    ''' </returns>
    Private Shared Function HandleDatabaseConfig() As Integer
        Application.EnableVisualStyles()

        Try
            Options.Instance.Init(ConfigLocator.Instance)
        Catch ex As Exception
            MessageBox.Show(String.Format(My.Resources.ErrorLoadingConfigurationOptions0, ex.Message),
            My.Resources.xError, MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return DialogResult.Abort
        End Try

        Dim res As DialogResult
        Try
            Using f As New ConnectionConfigForm()
                f.ShowInTaskbar = False
                res = f.ShowDialog()
            End Using
        Catch ex As Exception
            MessageBox.Show(String.Format(My.Resources.ErrorInConnectionConfigurationForm0, ex.ToString()))
            res = DialogResult.Abort
        End Try
        If res = DialogResult.OK Then Return 0 ' Everything's fine...
        If res = DialogResult.None Then Return 101 ' It shouldn't return None...
        Return res

    End Function

    ''' <summary>
    ''' Handles the automate config daemon running
    ''' </summary>
    ''' <param name="procId">The process ID for which the daemon should operate
    ''' </param>
    ''' <returns>An exit code, typically zero.
    ''' Pretty much always zero actually.</returns>
    Private Shared Function HandleDaemon(ByVal procId As Integer) As Integer
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
        Application.Run(New ConfigWriterBackgroundForm(procId))
        Return 0
    End Function

#Region " Console helpers "

    ''' <summary>
    ''' Writes the formatted string to stdout
    ''' </summary>
    ''' <param name="str">The string to write to stdout</param>
    Private Shared Sub Out(ByVal str As String)
        Dim replacedStr = str.Replace("\n", Environment.NewLine)
        Console.WriteLine(replacedStr)
    End Sub

    ''' <summary>
    ''' Writes the formatted string to stderr
    ''' </summary>
    ''' <param name="str">The string to write to stderr</param>
    Private Shared Sub Err(ByVal str As String)
        Console.Error.WriteLine(str)
    End Sub

    ''' <summary>
    ''' Writes the formatted string to stdout
    ''' </summary>
    ''' <param name="str">The string with argument placeholders</param>
    ''' <param name="args">The arguments for the string</param>
    Private Shared Sub Out(ByVal str As String, ByVal ParamArray args() As Object)
        Dim replacedStr = str.Replace("\n", Environment.NewLine)
        Console.WriteLine(replacedStr, args)
    End Sub

    ''' <summary>
    ''' Writes the formatted string to stderr
    ''' </summary>
    ''' <param name="str">The string with argument placeholders</param>
    ''' <param name="args">The arguments for the string</param>
    Private Shared Sub Err(ByVal str As String, ByVal ParamArray args() As Object)
        Console.Error.WriteLine(str, args)
    End Sub

#End Region

End Class
