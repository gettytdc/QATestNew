
Imports System.IO
Imports System.Xml
Imports System.Environment
Imports System.Drawing.Imaging
Imports BluePrism.CharMatching
Imports BluePrism.Core.Xml
Imports NLog

''' Project  : ApplicationManagerUtilities
''' Class    : clsConfig
''' <summary>
''' Handles Application Manager configuration. Configuration is done using an XML
''' file called "appman_config.xml" in the Application Data (current user!)
''' directory. The presence of this configuration file is completely optional -
''' normally the default values will suffice. A configuration file can be created
''' manually on a particular installation in order to override these default
''' settings - for example, when a consultant wants to enable low-level logging
''' for test/bug report purposes.
''' </summary>
Public Class clsConfig

    ''' Loggers are normally based on namespace + type. A recognisable "virtual" logger 
    ''' name is used here as the source of the logging that is configured via 
    ''' appman_config.xml.
    Private Shared ReadOnly Logger As Logger = LogManager.GetLogger("BluePrism.ClientComms.ApplicationManager")

    ''' <summary>
    ''' Controls whether logging is enabled.
    ''' </summary>
    Private Shared mLoggingEnabled As Boolean = False

    ''' <summary>
    ''' The path to a directory used to output images used when debugging
    ''' charmatching
    ''' </summary>
    Private Shared mLogDir As String = ""

    ''' <summary>
    ''' Arbitrary object used as a lock on the log file.
    ''' </summary>
    Private Shared mLogSync As New Object()

    ''' <summary>
    ''' True to include hi-res (fractional seconds) in the timestamp information at
    ''' the start of each log file line.
    ''' </summary>
    Private Shared mHiResTimes As Boolean = False

    ''' <summary>
    ''' True to log timing information. (Only relevant if logging is enabled)
    ''' </summary>
    Private Shared mLogTimings As Boolean = False

    ''' <summary>
    ''' True to log detailed Win32 information. (Only relevant if logging is
    ''' enabled)
    ''' </summary>
    Private Shared mLogWin32 As Boolean = False

    ''' <summary>
    ''' True to log detailed wait information - i.e. all subqueries and their
    ''' responses (Only relevant if logging is enabled)
    ''' </summary>
    Private Shared mLogWait As Boolean = False

    ''' <summary>
    ''' True to log detailed HTML information. (Only relevant if logging is
    ''' enabled)
    ''' </summary>
    Private Shared mLogHTML As Boolean = False

    ''' <summary>
    ''' True to log detailed JAB information. (Only relevant if logging is
    ''' enabled)
    ''' </summary>
    Public Shared ReadOnly Property JABLogging() As Boolean
        Get
            Return mLogJAB
        End Get
    End Property
    Private Shared mLogJAB As Boolean = False

    ''' <summary>
    ''' True to log detailed hooking information. (Only relevant if logging is
    ''' enabled)
    ''' </summary>
    Private Shared mLogHook As Boolean = False

    ''' <summary>
    ''' True to log exceptions (Only relevant if logging is
    ''' enabled)
    ''' </summary>
    Private Shared mLogExceptions As Boolean = False

    ''' <summary>
    ''' True to log element match retries (Only relevant if logging is
    ''' enabled)
    ''' </summary>
    Private Shared mLogRetries As Boolean = False

    ''' <summary>
    ''' True to log character matching information (Only relevant if logging is
    ''' enabled)
    ''' </summary>
    Public Shared ReadOnly Property LoggingCharMatching() As Boolean
        Get
            Return mLogCharMatching
        End Get
    End Property
    Private Shared mLogCharMatching As Boolean = False

    ''' <summary>
    ''' True to use a diagnostics version of the BPInjAgent DLL, which returns a lot
    ''' more diagnostics information, but inevitably slows down the target
    ''' application, significantly in some cases.
    ''' </summary>
    Public Shared ReadOnly Property AgentDiags() As Boolean
        Get
            Return mAgentDiags
        End Get
    End Property
    Private Shared mAgentDiags As Boolean = False


    ''' <summary>
    ''' True to use marshaled IHTMLDocument interfaces when hooking. This is an
    ''' experimental feature which has very poor performance.
    ''' </summary>
    ''' 'TODO investigate whether this code is used and if not remove
    Public Shared ReadOnly Property UseMarshaledHTML() As Boolean
        Get
            Return mUseMarshaledHTML
        End Get
    End Property
    Private Shared mUseMarshaledHTML As Boolean = False

    ''' <summary>
    ''' True to use an alternative method of retrieving HTML document interfaces,
    ''' via AA rather than the usual means.
    ''' </summary>
    Public Shared ReadOnly Property GetHTMLViaAA() As Boolean
        Get
            Return mGetHTMLViaAA
        End Get
    End Property
    Private Shared mGetHTMLViaAA As Boolean = False

    ''' <summary>
    ''' The filespec of the config file.
    ''' </summary>
    Public Shared ReadOnly Property ConfigFilespec() As String
        Get
            'Determine the expected location of the options file...
            Dim filespec As String = GetFolderPath(SpecialFolder.ApplicationData)
            filespec = Path.Combine(filespec, "appman_config.xml")
            Return filespec
        End Get
    End Property

    ''' <summary>
    ''' Initialise the configuration. Any application which makes use of
    ''' Application Manager should call this once on startup. Any error should
    ''' be dealt with by informing the user in an appropriate manner.
    ''' </summary>
    ''' <remarks>Applications that interface via AMI will call clsAMI.Init
    ''' which automatically calls this method. Direct calls to this method will
    ''' come from within ApplicationManager and associated projects.</remarks>
    Public Shared Function Init(ByRef sErr As String) As Boolean

        'See if the file exists - we silently do nothing if not, as that is the normal
        'state of affairs...
        If Not File.Exists(ConfigFilespec) Then Return True

        'Attempt to load the config...
        Try
            Dim x As New ReadableXmlDocument()
            x.Load(ConfigFilespec)
            'Verify that the root element is as expected...
            If x.DocumentElement.Name <> "config" Then
                sErr = My.Resources.TheConfigFileShouldHaveARootElementOfConfig
                Return False
            End If
            'Iterate through the child nodes...
            For Each e As XmlElement In x.DocumentElement
                Dim val As String = Nothing
                If e.FirstChild IsNot Nothing Then val = e.FirstChild.Value
                Select Case e.Name
                    Case "loggingenabled" : mLoggingEnabled = Boolean.Parse(val)
                    Case "logdir" : mLogDir = val?.TrimEnd("\"c)
                    Case "logtimings" : mLogTimings = Boolean.Parse(val)
                    Case "logfontrec" : mLogCharMatching = Boolean.Parse(val)
                    Case "logwin32" : mLogWin32 = Boolean.Parse(val)
                    Case "logwait" : mLogWait = Boolean.Parse(val)
                    Case "logjab" : mLogJAB = Boolean.Parse(val)
                    Case "loghtml" : mLogHTML = Boolean.Parse(val)
                    Case "loghook" : mLogHook = Boolean.Parse(val)
                    Case "logexceptions" : mLogExceptions = Boolean.Parse(val)
                    Case "logretries" : mLogRetries = Boolean.Parse(val)
                    Case "marshalhtml" : mUseMarshaledHTML = Boolean.Parse(val)
                    Case "gethtmlviaaa" : mGetHTMLViaAA = Boolean.Parse(val)
                    Case "userfontdirectory" : FontConfig.Directory = val
                    Case "agentdiags" : mAgentDiags = Boolean.Parse(val)
                End Select
            Next

            'If logging is enabled, we will log our startup time, also making the
            'start of a new entry clear in the log file...
            Log("Application Manager Initialised")
            Log("Address mode is {0}", CStr(IIf(IntPtr.Size = 4, "32 bit", "64 bit")))

            Return True

        Catch ex As Exception
            'Inform the user if there is a problem with the configuration file...
            sErr = String.Format(
             My.Resources.AnInvalidConfigFileExistsAt0PleaseEitherCorrectOrDeleteThisFileTheErrorWas1,
             ConfigFilespec, ex.Message)
            Return False

        End Try

    End Function

    Public Shared Sub Log(message As String)
        If mLoggingEnabled Then
            Logger.Debug(message)
        End If
    End Sub

    Public Shared Sub Log(messageFormat As String, ParamArray args() As Object)
        If mLoggingEnabled Then
            Logger.Debug(messageFormat, args)
        End If
    End Sub

    ''' <summary>
    ''' Log character matching diagnostics information.
    ''' </summary>
    ''' <param name="sText">The text to log</param>
    ''' <param name="b">A Bitmap containing diagnostics information</param>
    Public Shared Sub LogCharMatching(ByVal sText As String, ByVal b As System.Drawing.Bitmap)
        'This shouldn't be called if character matching logging is not enabled, since it
        'will have been expensive to generate the bitmap, but check anyway...
        If Not mLogCharMatching OrElse mLogDir = "" Then Exit Sub
        Dim sFileName As String = Path.GetRandomFileName() & ".png"
        Try
            If Not Directory.Exists(mLogDir) Then
                Directory.CreateDirectory(mLogDir)
            End If
            b.Save(mLogDir + "\" + sFileName, ImageFormat.Png)
            Log("Char Matching: Saw '{0}' from {1}", sText, sFileName)
        Catch ex As Exception
            Log($"Char Matching: Error: { ex.Message }.")
        End Try
    End Sub

    ''' <summary>
    ''' Helper function to log timing information to the log file, if logging and
    ''' logging of timings are enabled.
    ''' </summary>
    ''' <param name="sMessage">The text to log</param>
    Public Shared Sub LogTiming(ByVal sMessage As String)
        If mLogTimings Then Log(sMessage)
    End Sub

    ''' <summary>
    ''' Helper function to log timing information to the log file, if logging and
    ''' logging of timings are enabled.
    ''' </summary>
    ''' <param name="formatMsg">The text to log with placeholders</param>
    ''' <param name="args">The arguments to embed into the message</param>
    Public Shared Sub LogTiming(ByVal formatMsg As String, ParamArray args() As Object)
        If mLogTimings Then Log(formatMsg, args)
    End Sub

    ''' <summary>
    ''' Helper function to log win32 detail to the log file, if logging and
    ''' logging of win32 details are enabled.
    ''' </summary>
    ''' <param name="sMessage">The text to log</param>
    Public Shared Sub LogWin32(ByVal sMessage As String)
        If mLogWin32 Then Log(sMessage)
    End Sub

    ''' <summary>
    ''' Helper function to log wait detail to the log file, if logging and
    ''' logging of wait details are enabled.
    ''' </summary>
    ''' <param name="sMessage">The text to log</param>
    Public Shared Sub LogWait(ByVal sMessage As String)
        If mLogWait Then Log("WAIT SUBQUERY: {0}", sMessage)
    End Sub

    ''' <summary>
    ''' Helper function to log HTML detail to the log file, if logging and
    ''' logging of HTML details are enabled.
    ''' </summary>
    ''' <param name="sMessage">The text to log</param>
    Public Shared Sub LogHTML(ByVal sMessage As String)
        If mLogHTML Then Log(sMessage)
    End Sub

    ''' <summary>
    ''' Helper function to log JAB detail to the log file, if logging and
    ''' logging of JAB details are enabled.
    ''' </summary>
    ''' <param name="sMessage">The text to log</param>
    Public Shared Sub LogJAB(ByVal sMessage As String)
        If mLogJAB Then Log(sMessage)
    End Sub

    ''' <summary>
    ''' Helper function to log hooking detail to the log file, if logging and
    ''' logging of hooking are enabled.
    ''' </summary>
    ''' <param name="sMessage">The text to log</param>
    Public Shared Sub LogHook(ByVal sMessage As String)
        If mLogHook Then Log(sMessage)
    End Sub

    ''' <summary>
    ''' Helper function to log exception detail to the log file, if logging and
    ''' logging of exceptions is enabled.
    ''' </summary>
    ''' <param name="ex">The exception to log</param>
    Public Shared Sub LogException(ByVal ex As Exception)
        If mLogExceptions Then Log(ex.ToString)
    End Sub

    ''' <summary>
    ''' Helper function to log element match retries
    ''' </summary>
    ''' <param name="sMessage">The message to log</param>
    Public Shared Sub LogRetry(ByVal sMessage As String)
        If mLogRetries Then Log(sMessage)
    End Sub


End Class
