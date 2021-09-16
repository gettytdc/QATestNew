Imports System.IO

Imports BluePrism.AutomateAppCore.Auth
Imports System.Globalization
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.Server.Domain.Models
Imports NLog

''' Project  : AutomateAppCore
''' Class    : clsArchiver
''' <summary>
''' clsArchiver exposes methods to move session and log data from the automate
''' database to a file archive and back again.
''' </summary>
Public Class clsArchiver

    Private Shared ReadOnly Log As Logger = LogManager.GetCurrentClassLogger()

#Region " Constants and Enum / Class definitions "

    ''' <summary>
    ''' The name of the source used in the event log for log entries from this object
    ''' </summary>
    Public Const EventLogSource As String = "Blue Prism Archiver"

    ''' <summary>
    ''' See the documentation for the BPAAliveResources table. This is the interval
    ''' between timestamps in the table.
    ''' </summary>
    Private Const AliveResourceInterval As Integer = 2

    ''' <summary>
    ''' Enumeration for the run modes for this archiver.
    ''' </summary>
    Public Enum ArchiverMode
        Idle
        Archiving
        Restoring
        BackgroundArchiving
        BackgroundRestoring
    End Enum

    ''' <summary>
    ''' Workload class which encapsulates the payload for the background worker
    ''' for both the archive and restore operations.
    ''' </summary>
    Private Class WorkInput

        ' The mode that this workload should be processed in.
        Private mMode As ArchiverMode

        ' The input data for use by the processing code.
        ' This should be an ICollection of either FileInfo (for restoring) or
        ' clsSessionLog (for archiving)
        Private mPayload As Object

        ' Flag indicating that the given logs should be deleted rather than archived
        ' Only has meaning for archive operations
        Private mDeleteOnly As Boolean

        ''' <summary>
        ''' The mode of this workload - either <see cref="ArchiverMode.Archiving"/>
        ''' or <see cref="ArchiverMode.Restoring"/>.
        ''' </summary>
        Public ReadOnly Property Mode() As ArchiverMode
            Get
                Return mMode
            End Get
        End Property

        ''' <summary>
        ''' Gets the collection of FileInfo objects which represent logs to be
        ''' restored.
        ''' </summary>
        ''' <exception cref="InvalidCastException">If this workload is intended for
        ''' an <see cref="ArchiverMode.Archiving"/> operation. See <see cref="Mode"/>.
        ''' </exception>
        Public ReadOnly Property Files() As ICollection(Of FileInfo)
            Get
                Return DirectCast(mPayload, ICollection(Of FileInfo))
            End Get
        End Property

        ''' <summary>
        ''' Gets the collection of SessionLog objects which represent logs to be
        ''' archived.
        ''' </summary>
        ''' <exception cref="InvalidCastException">If this workload is intended for
        ''' an <see cref="ArchiverMode.Restoring"/> operation. See <see cref="Mode"/>.
        ''' </exception>
        Public ReadOnly Property Logs() As ICollection(Of clsSessionLog)
            Get
                Return DirectCast(mPayload, ICollection(Of clsSessionLog))
            End Get
        End Property

        ''' <summary>
        ''' Indicates whether this work input is set to delete the associated logs,
        ''' rather than archiving them first. Only has meaning for archiving
        ''' operations.
        ''' </summary>
        Public ReadOnly Property DeleteOnly() As Boolean
            Get
                Return mDeleteOnly
            End Get
        End Property

        ''' <summary>
        ''' Creates a new workload for restoring the given collection of files.
        ''' </summary>
        ''' <param name="files">The files representing the logs to be restored.
        ''' </param>
        Public Sub New(ByVal files As ICollection(Of FileInfo))
            mMode = ArchiverMode.Restoring
            mPayload = files
        End Sub

        ''' <summary>
        ''' Creates a new workload for archiving a collection of logs.
        ''' </summary>
        ''' <param name="logs">The logs to be archived.</param>
        Public Sub New(ByVal logs As ICollection(Of clsSessionLog))
            Me.New(logs, False)
        End Sub

        ''' <summary>
        ''' Creates a new workload for archiving or deleting a collection of logs.
        ''' </summary>
        ''' <param name="logs">The logs to be archived/deleted.</param>
        ''' <param name="deleteOnly">True to just delete the specified logs; False
        ''' to archive them before deleting</param>
        Public Sub New( _
         ByVal logs As ICollection(Of clsSessionLog), ByVal deleteOnly As Boolean)
            mMode = ArchiverMode.Archiving
            mPayload = logs
            mDeleteOnly = deleteOnly
        End Sub
    End Class

#End Region

#Region " Published Events "

    ''' <summary>
    ''' Event fired when an asynchronous archive operation has completed, either
    ''' finishing successfully, terminating with an error, or being cancelled.
    ''' </summary>
    ''' <param name="sender">The archiver instance which fired the event.</param>
    ''' <param name="e">The arguments detailing the event.</param>
    Public Event ArchiveCompleted( _
     ByVal sender As Object, ByVal e As OperationCompletedEventArgs)

    ''' <summary>
    ''' Event fired when an asynchronous restore operation has completed, either
    ''' finishing successfully, terminating with an error, or being cancelled.
    ''' </summary>
    ''' <param name="sender">The archiver instance which fired the event.</param>
    ''' <param name="e">The arguments detailing the event.</param>
    Public Event RestoreCompleted( _
     ByVal sender As Object, ByVal e As OperationCompletedEventArgs)

    ''' <summary>
    ''' Event fired when an asynchronous operation is reporting progress.
    ''' </summary>
    ''' <param name="sender">The archiver instance which fired the event.</param>
    ''' <param name="e">The arguments detailing the event.</param>
    Public Event ProgressChanged( _
     ByVal sender As Object, ByVal e As ProgressChangedEventArgs)

#End Region

#Region " Member Variables "

    ' The path to which the archiving is donw
    Private mArchivePath As String

    ' The mode that this archiver was initialised in
    Private mMode As ArchiverMode

    ' The percentage completeness of this archiver
    Public mPercentComplete As Integer

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates an instance of the archiver.
    ''' </summary>
    ''' <param name="archivePath">The directory to which archived files
    ''' will be stored.</param>
    Public Sub New(ByVal archivePath As String)

        mMode = ArchiverMode.Idle

        mArchivePath = archivePath

        mWorker = New BackgroundWorker()
        mWorker.WorkerReportsProgress = True
        mWorker.WorkerSupportsCancellation = True
        OperationAudit = (False, "", "")
        CleanupFailedDebugSessions()

    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' Returns who this archive was started by (essentially, who is logged in).
    ''' If there is no user logged in (eg. if this is running on a public resource
    ''' PC), this returns "automated archive"
    ''' </summary>
    Private ReadOnly Property StartedBy() As String
        Get
            If User.LoggedIn _
             Then Return User.Current.Name _
             Else Return "automated archive"
        End Get
    End Property

    ''' <summary>
    ''' The path of the folder in which we look for archive files.
    ''' </summary>
    Public Property ArchivePath() As String
        Get
            Return mArchivePath
        End Get
        Set(ByVal value As String)
            If mMode <> ArchiverMode.Idle Then
                Throw New InvalidOperationException(
                 My.Resources.clsArchiver_TheArchivePathsCannotBeChangedWhenAnOperationIsInProgress)
            End If

            mArchivePath = value
        End Set
    End Property

    ''' <summary>
    ''' Indicates the current run mode of the object.
    ''' </summary>
    Public ReadOnly Property Mode() As ArchiverMode
        Get
            Return mMode
        End Get
    End Property

    ''' <summary>
    ''' Used to do the archiving (and restoring) in System Manager 
    ''' so that Automate is not blocked by the archiving process 
    ''' and so that progress can be displayed on the front end.
    ''' </summary>
    Private WithEvents mWorker As BackgroundWorker

    ''' <summary>
    ''' The current percentage of work done by this archiver instance. Zero if it
    ''' is not active at the moment.
    ''' </summary>
    Public ReadOnly Property PercentageComplete() As Integer
        Get
            Return mPercentComplete
        End Get
    End Property

    Private Property OperationAudit() As (required As Boolean, narrative As String, comments As String)

#End Region

#Region " BackgroundWorker handling "

    ''' <summary>
    ''' Handles the background worker being executed.
    ''' </summary>
    ''' <param name="sender">The background worker which fired this event.</param>
    ''' <param name="e">The args detailing the event.</param>
    Private Sub HandleWorkerExecute(
     ByVal sender As Object, ByVal e As DoWorkEventArgs) Handles mWorker.DoWork

        Dim worker As BackgroundWorker = DirectCast(sender, BackgroundWorker)
        Dim load As WorkInput = DirectCast(e.Argument, WorkInput)
        If load.Mode = ArchiverMode.Archiving Then
            ArchiveSessions(load.Logs, load.DeleteOnly, worker, e)
            e.Result = ArchiverMode.Archiving

        ElseIf load.Mode = ArchiverMode.Restoring Then
            RestoreFiles(load.Files, worker, e)
            e.Result = ArchiverMode.Restoring

        Else
            Throw New InvalidOperationException(
             My.Resources.clsArchiver_InvalidModeForArchiverSWorkLoad & load.Mode.ToString())

        End If

    End Sub

    ''' <summary>
    ''' Handles the background worker thread being completed.
    ''' </summary>
    ''' <param name="sender">The background worker object which fired this event.
    ''' </param>
    ''' <param name="e">The args detailing the event.</param>
    Private Sub HandleWorkerCompleted(
     ByVal sender As Object, ByVal e As RunWorkerCompletedEventArgs) _
     Handles mWorker.RunWorkerCompleted

        Dim mode As ArchiverMode = ArchiverMode.Idle
        Select Case mMode
            Case ArchiverMode.Archiving, ArchiverMode.BackgroundArchiving
                mode = ArchiverMode.Archiving
            Case ArchiverMode.Restoring, ArchiverMode.BackgroundRestoring
                mode = ArchiverMode.Restoring
        End Select

        Dim op As String = GetOperationName(True)
        Dim args As OperationCompletedEventArgs = Nothing
        Try
            ReleaseArchiveLock()
            If e.Error IsNot Nothing Then
                Log.Error(e.Error, "{0} operation failed", op)
                args = New OperationCompletedEventArgs(e.Error)
            ElseIf e.Cancelled Then
                Log.Warn("{0} operation cancelled", op)
                args = New OperationCompletedEventArgs(True)
            Else
                Log.Info("{0} operation completed successfully", op)
                args = New OperationCompletedEventArgs(False)
            End If

        Finally
            mPercentComplete = 0
            mMode = ArchiverMode.Idle

        End Try

        If mode = ArchiverMode.Archiving Then
            RaiseEvent ArchiveCompleted(Me, args)

        Else
            RaiseEvent RestoreCompleted(Me, args)

        End If

    End Sub

    ''' <summary>
    ''' Handles the background worker thread reporing progress.
    ''' </summary>
    ''' <param name="sender">The background worker which fired this event.</param>
    ''' <param name="e">The args detailing the event.</param>
    Private Sub HandleWorkerProgress(ByVal sender As Object, ByVal e As ProgressChangedEventArgs) Handles mWorker.ProgressChanged
        RaiseEvent ProgressChanged(Me, e)
    End Sub

#End Region

#Region " Archive locking / unlocking "

    ''' <summary>
    ''' Gets a lock for this computer on the archiving process, or throws an
    ''' exception if it cannot do that.
    ''' </summary>
    Private Sub GetArchiveLock()
        'See if archiving is already running
        gSv.ArchiveCheckCanProceed(AliveResourceInterval)
        'Update the database to show that this machine is using the archive
        gSv.AcquireArchiveLock()
    End Sub

    ''' <summary>
    ''' Releases this computer's lock on the archiving process.
    ''' </summary>
    Private Sub ReleaseArchiveLock()
        gSv.ReleaseArchiveLock()
    End Sub

#End Region

#Region " Public Methods "

    ''' <summary>
    ''' Checks if this archiver is currently running a background operation, which
    ''' has no cancellation request pending.
    ''' </summary>
    ''' <returns>True if this archiver is currently running a background operation
    ''' which has no cancellation request pending on it.</returns>
    Public Function IsBackgroundOperationSetToContinue() As Boolean
        Return IsBackgroundOperationInProgress() AndAlso Not mWorker.CancellationPending
    End Function

    ''' <summary>
    ''' Checks if this archiver is currently running a background operation.
    ''' </summary>
    ''' <returns>True if this archiver is currently archiving or restoring in a
    ''' background thread.</returns>
    Public Function IsBackgroundOperationInProgress() As Boolean
        Return (mMode = ArchiverMode.BackgroundArchiving OrElse mMode = ArchiverMode.BackgroundRestoring)
    End Function

    ''' <summary>
    ''' Checks if this archiver is currently running an archive or restore operation.
    ''' </summary>
    ''' <returns>True if this archiver is currently archiving or restoring, either
    ''' synchronously or in a background thread.</returns>
    Public Function IsOperationInProgress() As Boolean
        Return (mMode <> ArchiverMode.Idle)
    End Function

    ''' <summary>
    ''' Requests that a background worker be cancelled.
    ''' This will just ignore the request if the worker is not currently busy, or
    ''' already has a cancellation pending.
    ''' </summary>
    Public Sub Cancel()
        If mWorker.IsBusy AndAlso Not mWorker.CancellationPending Then mWorker.CancelAsync()
    End Sub

    ''' <summary>
    ''' Gets the name of the operation that this archiver is currently executing.
    ''' This will be either "archive", "restore" or "", depending on the current
    ''' mode of the archiver.
    ''' </summary>
    ''' <param name="isEventLog">The type of caller.  If UI, then localise.
    ''' </param>
    ''' <returns><list>
    ''' <item>"archive" if this archiver's current mode is Archiving or
    ''' BackgroundArchiving;</item>
    ''' <item>"restore" if this archiver's current mode is Restoring or
    ''' BackgroundRestoring;</item>
    ''' <item>"" if this archiver's current mode is Idle</item>
    ''' </list></returns>
    Public Function GetOperationName(Optional ByVal isEventLog As Boolean = False) As String
        Select Case mMode
            Case ArchiverMode.Archiving, ArchiverMode.BackgroundArchiving
                Return CStr(IIf(isEventLog, "archive", My.Resources.clsArchiver_GetOperationName_archive))
            Case ArchiverMode.Restoring, ArchiverMode.BackgroundRestoring
                Return CStr(IIf(isEventLog, "restore", My.Resources.clsArchiver_GetOperationName_restore))
            Case Else
                Return ""
        End Select
    End Function

    ''' <summary>
    ''' Called from Main.vb to initiate archiving from the command line. Extracts
    ''' session data from the database and writes it into xml files.
    ''' </summary>
    ''' <param name="fromDate">The first date to include logs from</param>
    ''' <param name="toDate">The last date to include logs from</param>
    ''' <param name="justDelete">Set to True to delete without exporting anything!
    ''' </param>
    ''' <param name="process">The name of a process to archive the logs of, or
    ''' Nothing to archive everything.</param>
    ''' <param name="sErr">On failure, contains an error message.</param>
    ''' <returns>True if successful, False otherwise.</returns>
    Public Function CreateArchive(
     ByVal fromDate As DateTime, ByVal toDate As DateTime,
     ByVal justDelete As Boolean, ByVal process As String,
     includeAudit As Boolean, ByRef sErr As String) _
     As Boolean

        mMode = ArchiverMode.Archiving

        Log.Info(
         "Starting the {0} of {1} sessions which ended between {2} to {3}, " &
         "initiated by {4}",
         IIf(justDelete, "delete", "archive"),
         IIf(process Is Nothing, "all", "'" & process & "'"),
         fromDate, toDate, StartedBy)

        Try
            GetArchiveLock()
            Log.Debug("Archive Lock acquired")

            'If we've been asked to archive for a particular process only, find it...
            Dim procid As Guid = Guid.Empty
            If process IsNot Nothing Then
                procid = gSv.GetProcessIDByName(process, True)
                If procid = Guid.Empty Then
                    sErr = My.Resources.clsArchiver_RequestedProcessDoesNotExist
                    Return False
                End If
            End If

            'Read sessions from database
            Dim logs = gSv.GetSessionLogs(fromDate, toDate, procid)
            If includeAudit Then SetArchiveAudit(logs, GetEmpty.ICollection(Of clsSessionLog), False, process, justDelete)
            ArchiveSessions(logs, justDelete, Nothing, Nothing)
            ReleaseArchiveLock()
            Return True

        Catch alfe As ArchiveLockFailedException
            ' Lock failed - no point in attempting to release it
            Log.Error(alfe, "Archive process failed")
            sErr = alfe.Message
            Return False

        Catch ex As Exception
            Log.Error(ex, "Archive process failed")
            Try
                ReleaseArchiveLock()
            Catch rex As Exception
                Log.Error(rex, "Releasing lock failed")
            End Try
            sErr = ex.Message
            Return False

        Finally
            Log.Info("{0} finished", IIf(justDelete, "Delete", "Archive"))
            mMode = ArchiverMode.Idle

        End Try

    End Function

    ''' <summary>
    ''' Called from ctlArchivingInterface to initiate a BackgroundWorker. Extracts
    ''' data from the database and write it into xml files.
    ''' </summary>
    ''' <param name="ids">The ids of the session to archive</param>
    ''' <param name="justDelete">True to just delete the specified logs; False to
    ''' archive them to the currently configured directory</param>
    ''' <param name="sErr">The error message</param>
    ''' <returns>True if successful; False otherwise</returns>
    Public Function CreateArchive(
     ByVal ids As ICollection(Of Guid), ByVal justDelete As Boolean,
     ByRef sErr As String) As Boolean

        mMode = ArchiverMode.BackgroundArchiving
        Dim opType As String = CStr(IIf(justDelete, "Delete", "Archive"))

        Log.Info(
         "Starting the {0} of {1} session{2}, initiated by {3}",
         opType.ToLower(), ids.Count, IIf(ids.Count = 1, "", "s"), StartedBy)

        Try
            GetArchiveLock()
            Log.Debug("Archive Lock acquired")
            mWorker.RunWorkerAsync(New WorkInput(gSv.GetSessionLogs(ids), justDelete))
            Return True

        Catch alfe As ArchiveLockFailedException
            ' Lock failed - no point in attempting to release it
            Log.Error(alfe, "{0} operation failed", opType)
            sErr = alfe.Message

        Catch ex As Exception
            Log.Error(ex, "{0} operation failed", opType)
            Try
                ReleaseArchiveLock()
            Catch rex As Exception
                Log.Error(rex, "Releasing lock failed.")
            End Try
            sErr = ex.Message

        End Try

        ' Only reaches here after an exception.
        mMode = ArchiverMode.Idle
        Log.Info("{0} operation finished", opType)
        Return False

    End Function

    ''' <summary>
    ''' Called from Main.vb to initiate archive restoration from the command line. 
    ''' Extracts session data from xml files and writes it into the database.
    ''' </summary>
    ''' <param name="fromDate">The first date to include logs from</param>
    ''' <param name="toDate">The last date to include logs from</param>
    ''' <param name="sErr">The error message</param>
    ''' <returns>True if successful</returns>
    Public Function RestoreArchive(ByVal fromDate As Date, ByVal toDate As Date, ByRef sErr As String) As Boolean

        mMode = ArchiverMode.Restoring
        Dim dir As New DirectoryInfo(mArchivePath)

        Log.Info(
         "Beginning the restore of all sessions between {0} and {1} " &
         "from archive path: {2}, initiated by {3}",
         fromDate, toDate, dir.FullName, StartedBy)

        Try
            GetArchiveLock()
            Log.Debug("Archive Lock acquired")

            'Get all files from the required date.
            Dim files As ICollection(Of FileInfo) =
             clsSessionLog.FindLogFiles(dir, fromDate, toDate)

            SetRestoreAudit(files, GetEmpty.ICollection(Of FileInfo), GetEmpty.IDictionary(Of String, String))
            RestoreFiles(files, Nothing, Nothing)
            ReleaseArchiveLock()
            Return True

        Catch alfe As ArchiveLockFailedException
            ' Lock failed - no point in attempting to release it
            Log.Error(alfe, "Restore process failed")
            sErr = alfe.Message
            Return False

        Catch ex As Exception
            Log.Error(ex, "Restore process failed")
            Try
                ReleaseArchiveLock()
            Catch rex As Exception
                Log.Error(rex, "Releasing lock failed")
            End Try
            sErr = ex.Message
            Return False

        Finally
            Log.Info("End of restore process")
            mMode = ArchiverMode.Idle

        End Try

    End Function

    ''' <summary>
    ''' Called from ctlArchivingInterface to initiate a BackgroundWorker. Extracts 
    ''' session data from xml files and writes it into the database.
    ''' </summary>
    ''' <param name="aFiles">The FileInfo objects</param>
    ''' <param name="sErr">The error message</param>
    ''' <returns>True if successful</returns>
    Public Function RestoreArchive(
     ByVal aFiles As ICollection(Of FileInfo), ByRef sErr As String) As Boolean

        mMode = ArchiverMode.BackgroundRestoring

        Log.Info(
         "Starting the archive of {0} session{1}, initiated by {2}",
         aFiles.Count, IIf(aFiles.Count = 1, "", "s"), StartedBy)

        Try
            GetArchiveLock()
            Log.Debug("Archive Lock acquired")
            mWorker.RunWorkerAsync(New WorkInput(aFiles))
            Return True

        Catch alfe As ArchiveLockFailedException
            ' Lock failed - no point in attempting to release it
            Log.Error(alfe, "Restore process failed")
            sErr = alfe.Message

        Catch ex As Exception
            Log.Error(ex, "Restore process failed")
            Try
                ReleaseArchiveLock()
            Catch rex As Exception
                Log.Error(rex, "Releasing lock failed.")
            End Try
            sErr = ex.Message

        End Try

        mMode = ArchiverMode.Idle
        Log.Info("End of restore process")
        Return False

    End Function

#End Region

#Region " Internal Archive methods "

    ''' <summary>
    ''' Archives the given sessions to the output directory and deletes them from
    ''' the database.
    ''' </summary>
    ''' <param name="sessionLogs">The session logs to export and delete.</param>
    ''' <param name="justDelete">Set this to True to just delete the logs without
    ''' exporting them!</param>
    ''' <param name="worker">The background worker which this method is operating
    ''' on behalf of, or null if it is not from a background worker.</param>
    ''' <param name="e">The event args detailing the DoWork event from the given
    ''' background worker which instigated this method call. Null indicates that
    ''' this method call is not on behalf of a background worker.</param>
    Private Sub ArchiveSessions(
     ByVal sessionLogs As ICollection(Of clsSessionLog),
     ByVal justDelete As Boolean,
     ByVal worker As BackgroundWorker,
     ByVal e As DoWorkEventArgs)

        Dim total As Integer = sessionLogs.Count
        Dim count As Integer = 0
        Dim outputDir As DirectoryInfo = Nothing
        If Not justDelete Then outputDir = New DirectoryInfo(mArchivePath)

        Dim sessionLogMaxAttributeXmlLength = gSv.GetPref(PreferenceNames.XmlSettings.MaxAttributeXmlLength, clsSessionLog.AttributeXmlMaxLength)

        For Each sessionLog As clsSessionLog In sessionLogs
            If worker IsNot Nothing Then
                If worker.CancellationPending Then
                    e.Cancel = True
                    Return
                End If
                mPercentComplete = CInt(100.0 * count / total)
                worker.ReportProgress(mPercentComplete)
            End If

            If justDelete Then
                Dim entries As Integer = gSv.ArchiveSession(sessionLog.SessionNumber)
                Log.Info(
                 "Session {0,3} of {1}.{2}Deleted {3} log entries",
                 count, total, vbCrLf, entries)
            Else
                sessionLog.SessionLogMaxAttributeXmlLength = sessionLogMaxAttributeXmlLength

                ' Export the log, then delete it and log the information
                Dim file As FileInfo = sessionLog.ExportTo(outputDir)
                Dim entries As Integer = gSv.ArchiveSession(sessionLog.SessionNumber)
                Log.Info(
                 "Session {0,3} of {1}.{2}Archived {3} log entries.{2}Saved to file: {4}",
                 count, total, vbCrLf, entries, file.FullName)
            End If

            count += 1

        Next

        If OperationAudit.required Then
            gSv.AuditRecordArchiveEvent(If(justDelete,
                                            ArchiveOperationEventCode.Delete,
                                            ArchiveOperationEventCode.Archive),
                                        OperationAudit.narrative,
                                        OperationAudit.comments)
        End If

    End Sub

#End Region

#Region " Internal Restore methods "

    ''' <summary>
    ''' Restores the given files, on behalf of the given background worker, if one
    ''' exists for this operation.
    ''' </summary>
    ''' <param name="files">The (non-null) collection of files to restore.</param>
    ''' <param name="worker">The background worker to report progress to and check
    ''' for cancellation of the operation. Null indicates that this operation has
    ''' no background worker and should not report progress or check for cancels.
    ''' </param>
    ''' <param name="e">The event arguments from the background worker's DoWork
    ''' event - null if there is no background worker.</param>
    Private Sub RestoreFiles(ByVal files As ICollection(Of FileInfo),
     ByVal worker As BackgroundWorker, ByVal e As DoWorkEventArgs)

        Dim total As Integer = files.Count
        If total = 0 Then Return ' better to stop now than throw a DIVBYZERO reporting progress.

        Dim count As Integer = 0

        ' Go through each file and import it into the database.
        For Each file As FileInfo In files

            ' If we have background worker, check to see if it is pending a cancel.
            If worker IsNot Nothing Then
                If worker.CancellationPending Then
                    ' If so, set as much in the event args (which should be non-null
                    ' if we have a background worker) and exit.
                    e.Cancel = True
                    Return
                End If
                ' Not cancelling, let any listeners know where we're up to.
                mPercentComplete = CInt(100.0 * count / total)
                worker.ReportProgress(mPercentComplete)
            End If

            count += 1

            clsSessionLog.ImportFrom(file)
            DeleteFile(file)
            Log.Info(
              "Session {0,3} of {1} restored.{2}Deleted file: {3}",
              count, total, vbCrLf, file)
        Next

        If OperationAudit.required Then
            gSv.AuditRecordArchiveEvent(ArchiveOperationEventCode.Restore,
                                        OperationAudit.narrative,
                                        OperationAudit.comments)
        End If
    End Sub

    ''' <summary>
    ''' Deletes the given file and, where possible, any parent folders.
    ''' </summary>
    ''' <param name="child">The FileInfo object</param>
    Private Sub DeleteFile(ByVal child As FileInfo)
        Dim parent As DirectoryInfo

        parent = child.Directory
        child.Delete()

        If parent.GetFiles.Length = 0 Then
            'The parent folder is empty, so delete it.
            DeleteDirectory(parent)
        End If

    End Sub

    ''' <summary>
    ''' Deletes the given folder and, where possible, any parent folder up to the
    ''' archive root folder.
    ''' </summary>
    ''' <param name="child"></param>
    Private Sub DeleteDirectory(ByVal child As DirectoryInfo)
        Dim parent As DirectoryInfo

        parent = child.Parent
        If parent.GetDirectories.Length = 1 Then
            'The child is the only subfolder
            If parent.FullName = mArchivePath Then
                'The parent folder is the archive root folder, so just delete this subfolder and any contents
                child.Delete(True)
            Else
                'Recursively delete the parent folder
                DeleteDirectory(parent)
            End If
        Else
            'There are other subfolders, so just delete this subfolder and any contents
            child.Delete(True)
        End If
    End Sub

    Private Sub CleanupFailedDebugSessions()
        Try
            gSv.CleanupFailedDebugSessions()
        Catch ex As Exception
            Log.Error(ex, "Unable to cleanup failed debug sessions")
        End Try
    End Sub
#End Region

#Region " Audits "

    Public Sub SetArchiveAudit(
        selectedLogs As ICollection(Of clsSessionLog),
        unselectedLogs As ICollection(Of clsSessionLog),
        byProcess As Boolean, processName As String, deleteOnly As Boolean)

        Dim startDate = selectedLogs.Min(Function(s) s.StartDateTime)
        Dim endDate = selectedLogs.Max(Function(s) s.StartDateTime)

        Dim narrative = String.Format(My.Resources.clsArchiver_NumberOfRecordsProcessed0RecordDateRange12,
            selectedLogs.Count, FormatDate(startDate), FormatDate(endDate))

        Dim summary = New StringBuilder()
        If deleteOnly Then
            summary.Append(My.Resources.clsArchiver_DeletedLogsInclude)
        Else
            summary.AppendFormat(My.Resources.clsArchiver_ArchivePath0ArchivedLogsInclude, ArchivePath)
        End If
        If Not byProcess Then
            summary.Append(SelectedDates(processName, startDate, endDate, selectedLogs, unselectedLogs))
        Else
            summary.Append(SelectedProcesses(selectedLogs))
        End If

        OperationAudit = (True, narrative, summary.ToString())
    End Sub

    Private Function SelectedDates(processName As String, startDate As Date, endDate As Date, selected As ICollection(Of clsSessionLog), unselected As ICollection(Of clsSessionLog)) As String
        Dim dateRange = ""
        If unselected.Count = 0 OrElse
          Not unselected.Any(Function(s) s.StartDateTime >= startDate AndAlso s.StartDateTime <= endDate) Then
            dateRange = $"{FormatDate(startDate)} - {FormatDate(endDate)}"
        Else
            dateRange = CollectionUtil.Join(selected.Select(Function(s) FormatDate(s.StartDateTime)).Distinct, ", ")
        End If

        If String.IsNullOrEmpty(processName) Then
            Return String.Format(My.Resources.clsArchiver_AllLogsForAllProcessesForDates0Total1, dateRange, selected.Count)
        Else
            Return String.Format(My.Resources.clsArchiver_AllLogsForProcess0ForDates1Total2, processName, dateRange, selected.Count)
        End If
    End Function

    Private Function SelectedProcesses(selected As ICollection(Of clsSessionLog)) As String
        Dim processList As New List(Of String)

        For Each dt In selected.Select(Function(s) s.StartDateTime.Date).Distinct()
            Dim processesByDate = From session In selected
                                  Where session.StartDateTime.Date = dt
                                  Group By ProcessName = session.ProcessName
                                  Into Processes = Group, Count()
                                  Order By ProcessName

            Dim processesForDate As New List(Of String)
            For Each p In processesByDate
                processesForDate.Add(String.Format(My.Resources.clsArchiver_0Total1, p.ProcessName, p.Processes.Count))
            Next
            processList.Add($"{FormatDate(dt)}: {CollectionUtil.Join(processesForDate, ", ")}")
        Next

        Return CollectionUtil.Join(processList, "; ")
    End Function

    Public Sub SetRestoreAudit(selectedFiles As ICollection(Of FileInfo), unselectedFiles As ICollection(Of FileInfo), processMap As IDictionary(Of String, String))
        Dim startDate = selectedFiles.Min(Function(f) ExtractDate(f))
        Dim endDate = selectedFiles.Max(Function(f) ExtractDate(f))

        Dim narrative = String.Format(My.Resources.clsArchiver_NumberOfRecordsProcessed0RecordDateRange12,
            selectedFiles.Count, FormatDate(startDate), FormatDate(endDate))

        Dim summary = New StringBuilder()
        summary.AppendFormat(My.Resources.clsArchiver_ArchivePathRestoredFrom0RestoredLogsInclude, ArchivePath)

        If unselectedFiles.Count = 0 OrElse
                Not unselectedFiles.Any(Function(f)
                                            Dim fileDate = ExtractDate(f)
                                            Return fileDate >= startDate AndAlso fileDate <= endDate
                                        End Function) Then
            summary.AppendFormat(My.Resources.clsArchiver_AllLogsInTheSelectedLocationForDates01Total2,
                                 FormatDate(startDate),
                                 FormatDate(endDate),
                                 selectedFiles.Count)
        Else
            summary.Append(SelectedProcessesRestore(selectedFiles, processMap))
        End If

        OperationAudit = (True, narrative, summary.ToString())
    End Sub

    Private Function SelectedProcessesRestore(selected As ICollection(Of FileInfo), processMap As IDictionary(Of String, String)) As String
        Dim processList As New List(Of String)

        For Each dt In selected.Select(Function(f) ExtractDate(f)).Distinct()
            Dim processesByDate = From file In selected
                                  Where ExtractDate(file) = dt
                                  Group By ProcessName = processMap(file.Name)
                                  Into Processes = Group, Count()
                                  Order By ProcessName

            Dim processesForDate As New List(Of String)
            For Each p In processesByDate
                processesForDate.Add(String.Format(My.Resources.clsArchiver_0Total1, p.ProcessName, p.Processes.Count))
            Next
            processList.Add($"{FormatDate(dt)}: {CollectionUtil.Join(processesForDate, ", ")}")
        Next

        Return CollectionUtil.Join(processList, "; ")
    End Function


    Private Function FormatDate(dateToFormat As Date) As String
        Return dateToFormat.ToString("yyyy-MM-dd")
    End Function

    Private Function ExtractDate(file As FileInfo) As Date
        Return Date.ParseExact(file.Name.Substring(0, 8), "yyyyMMdd", CultureInfo.InvariantCulture)
    End Function

#End Region

End Class

