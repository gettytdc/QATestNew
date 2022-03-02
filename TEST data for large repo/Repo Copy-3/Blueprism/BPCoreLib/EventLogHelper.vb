Imports System.Security
Imports BluePrism.Server.Domain.Models
Imports NLog

''' <summary>
''' Manages Windows event log setup. Logging is managed using NLog, but the application 
''' is responsible for creating and configuring event logs and sources for Windows event 
''' log targets that are present in the standard logging configuration.
''' </summary>
Public Class EventLogHelper

    Private Shared ReadOnly Log As Logger = LogManager.GetCurrentClassLogger()

    ''' <summary>
    ''' The default entry log name for Blue Prism logs.
    ''' </summary>
    Public Shared DefaultLogName As String = "Blue Prism"

    ''' <summary>
    ''' The event log name for analytics output (i.e. Published Dashboards)
    ''' </summary>
    Public Shared AnalyticsLogName As String = "BP Analytics"

    ''' <summary>
    ''' The pattern used to generate the event source name for analytics. This
    ''' contains a placeholder for the server configuration name to be inserted.
    ''' </summary>
    Public Shared AnalyticsSourcePattern As String = "BP Analytics Service - {0}"

    ''' <summary>
    ''' The default windows event source to use in the Blue Prism event log
    ''' </summary>
    Public Shared DefaultSource As String = "Blue Prism General"

    ''' <summary>
    ''' The pattern to use to generate a source name for a resource PC. This contains
    ''' a single placeholder for the port number that the resource listens on.
    ''' </summary>
    Public Shared ResourceSourcePattern As String = "Blue Prism Resource (Port {0})"

    ''' <summary>
    ''' Attempts to create the default Blue Prism event log and source. The
    ''' default max size and overflow settings are configured if the event log does 
    ''' not already exist and needs to be created. The configuration of an 
    ''' existing log will not be altered.
    ''' </summary>
    Public Shared Sub CreateDefaultLog()

        CreateLog(DefaultLogName, DefaultSource)

    End Sub

    ''' <summary>
    ''' Attempts to create the BP Analytics event log and default source. The
    ''' default max size and overflow settings are configured if the event log does 
    ''' not already exist and needs to be created. The configuration of an 
    ''' existing log will not be altered.
    ''' </summary>
    Public Shared Sub CreateAnalyticsLog()

        Dim source = GetAnalyticsSource("Default")
        CreateLog(AnalyticsLogName, source)

    End Sub

    Private Shared Sub CreateLog(logName As String, source As String)

        CreateSource(source, logName)
        Using eventLog = New EventLog(logName, ".", source)
            Try
                eventLog.ModifyOverflowPolicy(OverflowAction.OverwriteAsNeeded, 7)
                ' I took this value of 16MB as the max size from the serverfault
                ' answer at: http://serverfault.com/a/69270 which explains its
                ' reasoning quite convincingly.
                ' The MS article at http://support.microsoft.com/kb/957662
                ' suggests that maximums up to 300MB are valid for XP, but
                ' I think that's overkill - if we're overrunning 16MB, then we
                ' have some serious problems elsewhere, I suspect.
                ' The default is 512KB, so it's 32x that.
                eventLog.MaximumKilobytes = 16384
            Catch ex As Exception
                Log.Error(ex, "Failed to set up default event log")
                Throw New OperationFailedException(
                    My.Resources.EventLogger_CouldNotSetupWindowsEventLogErrorTemplate, DefaultLogName, ex)
            End Try
        End Using
    End Sub

    ''' <summary>
    ''' Generates event log source for analytics
    ''' </summary>
    ''' <param name="configurationName">Name of the active configuration</param>
    Public Shared Function GetAnalyticsSource(configurationName As String) As String
        Return String.Format(AnalyticsSourcePattern, configurationName)
    End Function

    ''' <summary>
    ''' Generates event log source for a Resource PC
    ''' </summary>
    ''' <param name="port">The port number for which the event log source is
    ''' required.</param>
    Public Shared Function GetResourcePcSource(port As Integer) As String
        Return String.Format(ResourceSourcePattern, port)
    End Function

    ''' <summary>
    ''' Attempts to create an event log source inside the event log with the
    ''' given name.
    ''' </summary>
    ''' <param name="src">The source to create</param>
    ''' <param name="logName">The name of the event log that the source is associated
    ''' with.</param>
    ''' <exception cref="OperationFailedException">If the source could not be created
    ''' for some reason.</exception>
    Public Shared Sub CreateSource(src As String, logName As String)

        ' If it's already there, then we have no work to do
        If SourceExists(src) Then Return

        Try
            EventLog.CreateEventSource(New EventSourceCreationData(src, logName))
            Log.Info("Created event source: {0}", src)
        Catch ex As Exception
            Log.Error(ex, "Failed creating event source: {0}", src)
            Throw New OperationFailedException(
                My.Resources.EventLogger_CouldNotCreateWindowsEventSource0Error1, src, ex)
        End Try

    End Sub

    ''' <summary>
    ''' Checks if an event log source with the given name exists. It is thought not
    ''' to exist if the <see cref="EventLog.SourceExists"/> method throws a security
    ''' exception, indicating that the "source was not found, but some or all of the
    ''' event logs could not be searched.
    ''' </summary>
    ''' <param name="sourceName">The name of the source to check</param>
    ''' <returns>True if the source exists; False if the source doesn't exist or if
    ''' it was not found and security restrictions meant that there were some logs
    ''' which could not be searched.</returns>
    Private Shared Function SourceExists(ByVal sourceName As String) As Boolean
        Try
            Return EventLog.SourceExists(sourceName)
        Catch se As SecurityException
            ' This is thrown if the log was not found but permissions did not allow
            ' all logs to be searched
        End Try
        Return False
    End Function
End Class
