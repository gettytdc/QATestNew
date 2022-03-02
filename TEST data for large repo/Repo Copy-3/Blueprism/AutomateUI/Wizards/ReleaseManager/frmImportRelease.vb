Imports System.IO
Imports AutomateControls.Forms

Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.BackgroundJobs
Imports BluePrism.AutomateAppCore.BackgroundJobs.Monitoring
Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.AutomateProcessCore

Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.Server.Domain.Models
Imports BluePrism.Skills

''' <summary>
''' Wizard class used for creating a release within a package.
''' </summary>
Friend Class frmImportRelease
    Inherits frmStagedWizard

#Region " Class-scope definitions "

    ''' <summary>
    ''' The preference name for the output directory to use for the release
    ''' </summary>
    ''' <remarks></remarks>
    Private Const ReleaseDirectoryPrefName As String = "release.dir"

    ''' <summary>
    ''' Creates the stages used in this wizard.
    ''' </summary>
    ''' <returns>The list of stages for this wizard.
    ''' </returns>   
    Private Shared Function CreateStages(importType As ImportType) As IList(Of WizardStage)
        Dim stages As New List(Of WizardStage)
        ' Include the 'choose package' stage if one is not chosen

        stages.Add(New InputFileStage(ReleaseDirectoryPrefName, importType = ImportType.ProcessObject))
        stages.Add(New BackgroundWorkerStage("parse.stage", My.Resources.frmImportRelease_ReadingFile, My.Resources.frmImportRelease_PleaseWait))
        stages.Add(New ImportConflictStage())
        stages.Add(New LogLevelStage())
        stages.Add(New BackgroundWorkerStage("load.stage", My.Resources.frmImportRelease_SavingToDatabase, My.Resources.frmImportRelease_PleaseWait))
        If importType = ImportType.ProcessObject Then
            stages.Add(New ProcessImportReportStage())
        End If
        Return stages

    End Function

#End Region

#Region "Public Properties"
    'The full file path of the file to be imported
    Public WriteOnly Property FilePath As String
        Set
            mFileChooser.FileName = Value
        End Set
    End Property

    Enum ImportType
        Release
        ProcessObject
    End Enum

    Private Shared Property ImporterType As ImportType = ImportType.Release

#End Region

#Region " Member variables "

    ' The file name from which the release should be imported
    Private mFileName As String

    'Collection of Importfiles when dealing with multiple files
    Private mImportFiles As List(Of ImportFile) = New List(Of ImportFile)
    'The full file path of the file to be imported
    Private mFilePath As String

    ' The release that has/is being imported
    Private mRelease As clsRelease

    ' The id of the release that has been imported
    Private mImportedReleaseId As Integer = 0

    ' The collision report
    Private mConflicts As ConflictSet

    ' Flag indicating if the next import should force unlock all processes/VBOs
    ' that are being modified (overwritten or renamed).
    Private mRetryWithForceUnlock As Boolean

    ' The stage which accepts an input file
    Private WithEvents mFileChooser As InputFileStage

    ' The stage which provides the framework for doing the work
    Private WithEvents mLoadAndParseStage As BackgroundWorkerStage

    ' The stage on which the collisions can be resolved
    Private WithEvents mConflictStage As ImportConflictStage

    ' The stage which shows the log level of process/objects being imported
    Private WithEvents mLogLevelStage As LogLevelStage

    ' The stage within which the components are actually loaded into the database
    Private WithEvents mLoadOntoDBStage As BackgroundWorkerStage

    ' The stage that generates a report on imported processes / objects
    Private WithEvents mProcessImportReportStage As ProcessImportReportStage

    ' Thread process monitor, recreated each run.
    Private mMonitor As clsProgressMonitor

#End Region

#Region " Constructors "
    Public Sub New()
        MyBase.New(My.Resources.frmImportRelease_ImportRelease, CreateStages(ImportType.Release))
        ImporterType = ImportType.Release
        mFileChooser = DirectCast(GetStage(InputFileStage.StageId), InputFileStage)
        mLoadAndParseStage = DirectCast(GetStage("parse.stage"), BackgroundWorkerStage)
        mConflictStage = DirectCast(GetStage(ImportConflictStage.StageId), ImportConflictStage)
        mLogLevelStage = DirectCast(GetStage(LogLevelStage.StageId), LogLevelStage)
        mLoadOntoDBStage = DirectCast(GetStage("load.stage"), BackgroundWorkerStage)
        ConfigureValidFileExtensions()

        ' This call is required by the designer.
        InitializeComponent()
    End Sub

    Public Sub New(importType As ImportType)
        MyBase.New(If(importType <> ImportType.ProcessObject, My.Resources.frmImportRelease_ImportRelease, My.Resources.frmImportRlease_ImportProcessOrObject), CreateStages(importType))
        ImporterType = importType
        mFileChooser = DirectCast(GetStage(InputFileStage.StageId), InputFileStage)
        mLoadAndParseStage = DirectCast(GetStage("parse.stage"), BackgroundWorkerStage)
        mConflictStage = DirectCast(GetStage(ImportConflictStage.StageId), ImportConflictStage)
        mLogLevelStage = DirectCast(GetStage(LogLevelStage.StageId), LogLevelStage)
        mLoadOntoDBStage = DirectCast(GetStage("load.stage"), BackgroundWorkerStage)
        If ImporterType = ImportType.ProcessObject Then
            mProcessImportReportStage = DirectCast(GetStage(ProcessImportReportStage.StageId), ProcessImportReportStage)
        End If
        ConfigureValidFileExtensions()

        ' This call is required by the designer.
        InitializeComponent()
    End Sub
#End Region

#Region " Properties "

    ''' <summary>
    ''' The id of the release that was created by this wizard
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property ImportedReleaseId() As Integer
        Get
            Return mImportedReleaseId
        End Get
    End Property

#End Region

#Region " Methods "
    ''' <summary>
    ''' To be used with IPC - Sets the file path and shows the dialog window
    ''' </summary>
    ''' <param name="filePath"></param>    
    Public Sub SetFileToImport(filePath As String)
        If Not User.Current.HasPermissionToImportFile(filePath) Then
            DisplayPermissionsError(filePath)
            Return
        End If
        If Not String.IsNullOrEmpty(filePath) Then
            mFileName = filePath
            mFileChooser.FileName = filePath
            mFileChooser.Control.Enabled = False
            StartPosition = FormStartPosition.CenterParent
            ShowDialog(gMainForm)
            BringToFront()
        End If
    End Sub

    Friend Sub DisplayPermissionsError(filePath As String)
        Dim descriptionText = String.Format(My.Resources.InsufficientPermissions_DescriptionFile0, filePath.GetBluePrismFileTypeName, filePath)
        Dim popUp = New PopupForm(My.Resources.ImportError, descriptionText, My.Resources.OK) With {
                .StartPosition = FormStartPosition.CenterParent
                }
        AddHandler popUp.OnBtnOKClick, AddressOf HandleOnBtnOKClick

        popUp.ShowInTaskbar = False
        popUp.BringToFront()
        popUp.ShowInTaskbar = False
        popUp.ShowDialog(Me)
    End Sub
    Private Sub HandleOnBtnOKClick(sender As Object, e As EventArgs)
        Dim popup = CType(sender, PopupForm)
        RemoveHandler popup.OnBtnOKClick, AddressOf HandleOnBtnOKClick
        popup.Close()
    End Sub
    Private Sub ConfigureValidFileExtensions()
        Dim extensions As String
        Dim extensionsName As String = ""

        If ImporterType = ImportType.Release Then
            If User.LoggedIn AndAlso User.Current.HasPermission(Permission.ReleaseManager.ImportRelease) Then
                extensions = $"{clsRelease.FileExtension};{Skill.FileExtension}"
                extensionsName = My.Resources.frmImportRelease_BluePrismRelease
            Else
                extensions = $"{Skill.FileExtension}"
                extensionsName = My.Resources.frmImportRelease_BluePrismRelease
            End If
            InputFileStage.MultiSelect = False
        Else
            InputFileStage.MultiSelect = True
            extensions = $"{clsProcess.ProcessFileExtension};{clsProcess.ObjectFileExtension};xml"
            extensionsName = My.Resources.frmImportRelease_BluePrismProcess
        End If

        mFileChooser.AddExtensionEntry(extensions, extensionsName)
    End Sub

    ''' <summary>
    ''' Initialises the given stage.
    ''' This just checks if the stage is a background worker stage, and disables the
    ''' next button if it is.
    ''' </summary>
    ''' <param name="stg">The stage which is to be initialised.</param>
    Protected Overrides Sub OnInitStage(ByVal stg As WizardStage)
        MyBase.OnInitStage(stg)
        If TypeOf stg Is BackgroundWorkerStage Then btnNext.Enabled = False
        If TypeOf stg Is BackgroundWorkerStage Then btnBack.Enabled = False

    End Sub

    ''' <summary>
    ''' Go back a page. This just ensures that the loadOntoDBStage titles are
    ''' correct.
    ''' </summary>
    Protected Overrides Sub BackPage()
        MyBase.BackPage()
        mLoadOntoDBStage.Title = My.Resources.frmImportRelease_SavingToDatabasePleaseWait
    End Sub

    Protected Overrides Sub OnClosing(e As CancelEventArgs)
        MyBase.OnClosing(e)
        mLoadAndParseStage.SignalCancel()
    End Sub


#End Region

#Region " Event Handlers "

    ''' <summary>
    ''' Handles the file being chosen in the file chooser stage.
    ''' </summary>
    Private Sub HandleFileChosen(ByVal sender As WizardStage, ByVal e As StageCommittedEventArgs) _
     Handles mFileChooser.Committed
        mFileName = mFileChooser.FileName
    End Sub

    ''' <summary>
    ''' Handles progress on the loading and parsing of the input file changing
    ''' </summary>
    ''' <param name="value">The percentage of progress to report.</param>
    ''' <param name="status">The status text for the change</param>
    Private Sub HandleProgressChangeOnLoadAndParse(ByVal value As Integer, ByVal status As Object)
        mLoadAndParseStage.ReportProgress(10 + (value \ 2), status)
    End Sub



    ''' <summary>
    ''' Handles the worker stage's "DoWork" event - this actually creates the
    ''' release, saves it to the database and exports it to the chosen file.
    ''' </summary>
    Private Sub HandleLoadAndParseWork(ByVal sender As Object, ByVal e As DoWorkEventArgs) _
     Handles mLoadAndParseStage.DoWork
        ' The package is just a package info - we need the real thing now...
        mLoadAndParseStage.ReportProgress(10, My.Resources.frmImportRelease_ImportingFile)
        mMonitor = New clsProgressMonitor()
        AddHandler mMonitor.ProgressChanged, AddressOf HandleProgressChangeOnLoadAndParse
        Try
            If ImporterType = ImportType.Release Then
                mRelease = clsRelease.Import(New FileInfo(mFileName), mMonitor, False)
            Else
                mImportFiles = New List(Of ImportFile)
                Dim fileNames = mFileName.Split(CType(",", Char())).ToList
                fileNames.ForEach(Sub(f) mImportFiles.Add(New ImportFile(f)))

                mRelease = clsRelease.ImportProcessesAsRelease(mImportFiles, mMonitor, False)
            End If
        Finally
            RemoveHandler mMonitor.ProgressChanged, AddressOf HandleProgressChangeOnLoadAndParse
        End Try

        mLoadAndParseStage.ReportProgress(75, My.Resources.frmImportRelease_CheckingPermissions)
        Dim missingPerms As ICollection(Of String) = mRelease.CheckImportPermissions()
        If missingPerms.Count > 0 Then e.Result = missingPerms : Return

        mLoadAndParseStage.ReportProgress(85, My.Resources.frmImportRelease_ValidatingRelease)
        mConflicts = mRelease.Conflicts

        mLoadAndParseStage.ReportProgress(100, My.Resources.frmImportRelease_Done)

    End Sub

    Private Sub HandleCancelWork(ByVal sender As Object, ByVal e As EventArgs) Handles mLoadAndParseStage.CancelWork

        If mMonitor IsNot Nothing Then
            mMonitor.RequestCancel()
        End If
    End Sub

    ''' <summary>
    ''' Handles the run worker being completed.
    ''' </summary>
    Private Sub HandleLoadAndParseCompleted(
     ByVal sender As Object, ByVal e As RunWorkerCompletedEventArgs) _
     Handles mLoadAndParseStage.RunWorkerCompleted

        ' If cancelling, we don't want to update any more of the gui
        If e.Cancelled Then
            Return
        End If

        Dim ex As Exception = e.Error
        If ex Is Nothing Then ' It worked... or did it

            ' If the 'result' is a collection of strings, these are permissions
            ' which are required to import this release which the current user
            ' does not have.
            Dim missingPerms As ICollection(Of String) = TryCast(e.Result, ICollection(Of String))
            ' I'd really prefer to do this as an exception but VS treats all 
            ' exceptions within background workers as unhandled exceptions and pauses
            ' debugging to tell you so. It's very annoying.
            If missingPerms IsNot Nothing Then
                UserMessage.Show(String.Format(
                 My.Resources.frmImportRelease_TheFollowingPermissionsAreRequiredToImportThisRelease01,
                 vbCrLf, CollectionUtil.Join(missingPerms, vbCrLf)))

                DialogResult = DialogResult.Abort
                Close()
                Return
            End If

            ' If we're here, then yes, it really did work
            ' Set any conflicts we've received into the next stage.
            mConflictStage.Conflicts = mConflicts

            If mRelease IsNot Nothing Then
                Dim componentsWithConflicts = mConflicts.AllConflicts().Select(Function(c) c.Component)
                Dim skippedComponentWithConflicts = mConflicts.AllConflicts().Where(Function(c) c.Resolution.ConflictOption.Choice = ConflictOption.UserChoice.Skip).Select(Function(c) c.Component)
                mLogLevelStage.ProcessComponents = mRelease.Where(
                    Function(c)
                        Return TypeOf c Is ProcessComponent AndAlso (Not componentsWithConflicts.Contains(c) OrElse Not skippedComponentWithConflicts.Contains(c))
                    End Function).Select(Function(c) CType(c, ProcessComponent))
            End If

            btnNext.Enabled = True
            btnNext.PerformClick()

        Else
            UserMessage.Err(ex,
             My.Resources.frmImportRelease_AnErrorOccurredWhileAttemptingToImportTheRelease01,
             vbCrLf, ex.Message)
            btnCancel.Enabled = True

        End If

    End Sub

    ''' <summary>
    ''' Handles stepping next into another stage. This ensures that the conflict
    ''' stage is not traversed to if there are no conflicts.
    ''' </summary>
    Protected Overrides Sub OnSteppingNext(ByVal e As WizardSteppingEventArgs)
        If e.Stage Is mConflictStage AndAlso mConflicts.IsEmpty Then e.Skip = True
        If e.Stage Is mLogLevelStage AndAlso mLogLevelStage?.ProcessComponents?.Any() <> True Then e.Skip = True
    End Sub

    ''' <summary>
    ''' Handles progress on the loading of the release into the database changing
    ''' </summary>
    ''' <param name="value">The percentage of progress to report.</param>
    ''' <param name="status">The status text for the change</param>
    Private Sub HandleProgressChangeOnLoadOntoDb(ByVal value As Integer, ByVal status As Object)
        mLoadOntoDBStage.ReportProgress(10 + ((value * 9) \ 10), status)
        If value = 100 Then
            'Lets Update mImportFiles with any conflict data from mConflicts
            If ImporterType = ImportType.ProcessObject Then
                For Each conflict In mConflicts.AllConflicts
                    With mImportFiles.FirstOrDefault(Function(x) x.BluePrismId = CType(conflict.Component, ProcessComponent).OriginalId)
                        .Conflicts.Add(conflict)
                    End With
                Next
            End If
        End If
    End Sub

    ''' <summary>
    ''' Handles the work of loading the release into the database
    ''' </summary>
    Private Sub HandleLoadOntoDBWork(ByVal sender As Object, ByVal e As DoWorkEventArgs) _
     Handles mLoadOntoDBStage.DoWork

        mLoadOntoDBStage.ReportProgress(5, My.Resources.frmImportRelease_ApplyingConflictResolutions)

        'Do any saving to file
        For Each con As KeyValuePair(Of PackageComponent, ICollection(Of Conflict)) In mConflicts.Conflicts
            For Each c As Conflict In con.Value
                If c.Resolution.ConflictOption.Choice = ConflictOption.UserChoice.SaveToFile Then
                    Dim comp As ISaveToFile = TryCast(con.Key, ISaveToFile)
                    If comp IsNot Nothing Then
                        Try
                            comp.SaveToFile()
                        Catch ex As Exception
                            Throw New BluePrismException(My.Resources.frmImportRelease_ErrorSaving01ToFile2,
                             PackageComponentType.GetLocalizedFriendlyName(con.Key.Type), con.Key.Name, ex.Message)
                        End Try
                    End If
                End If
            Next
        Next

        SaveRelease()

    End Sub

    ''' <summary>
    ''' Initiates saving of release on server (which starts a background job),
    ''' then waits for it to complete
    ''' </summary>
    Private Sub SaveRelease()

        If Not mRelease.Conflicts.IsResolved Then Throw New BluePrismException(
            My.Resources.frmImportRelease_UnresolvedConflictsRemainInThisRelease)
        Dim notifier As New BackgroundJobNotifier()
        Dim job = If(ImporterType = ImportType.Release, gSv.ImportRelease(mRelease, mRetryWithForceUnlock, notifier), gSv.ImportProcessOrObjectAsRelease(mRelease, mRetryWithForceUnlock, notifier))
        HandleProgressChangeOnLoadOntoDb(10, My.Resources.frmImportRelease_Saving)

        Dim jobResult = WaitForBackgroundJob(job, notifier)

        HandleBackgroundJobResult(jobResult)

    End Sub

    ''' <summary>
    ''' Waits for job to complete, updating information on the form as it progresses.
    ''' Note that this sub blocks until the job has completed or monitoring stops due to an
    ''' error.
    ''' </summary>
    ''' <remarks>This method is a little unintuitive. Monitoring is typically an asynchronous
    ''' process. However this operation is running on a BackgroundWorkerStage thread that
    ''' is designed to run until work is complete. Support for background jobs could be 
    ''' added to the wizard infrastructure but for now we're blocking until monitoring has 
    ''' finished so that we fit in with the existing mechanism.</remarks>
    ''' <param name="job">Background job information</param>
    ''' <param name="notifier">Signals when updates are available for the job
    ''' (when running in-process or server callbacks are supported)</param>
    Private Function WaitForBackgroundJob(job As BackgroundJob, notifier As BackgroundJobNotifier) _
        As BackgroundJobResult
        ' .Result blocks until async task has completed
        Return job.Wait(notifier, AddressOf HandleBackgroundJobUpdate).Result
    End Function

    ''' <summary>
    ''' Invoked when job is running and updated data is available about progress of the job
    ''' </summary>
    ''' <param name="data"></param>
    Private Sub HandleBackgroundJobUpdate(data As BackgroundJobData)
        HandleProgressChangeOnLoadOntoDb(data.PercentComplete, String.Format(My.Resources.frmImportRelease_Running0, data.Description))
    End Sub

    ''' <summary>
    ''' Processes outcome when we have finished monitoring progress of background job
    ''' </summary>
    ''' <param name="jobResult">The result of the background job</param>
    Private Sub HandleBackgroundJobResult(jobResult As BackgroundJobResult)

        Dim jobData = jobResult.Data
        Select Case jobResult.Status
            Case JobMonitoringStatus.Success
                Dim statusMessage As String
                If ImporterType = ImportType.Release Then
                    statusMessage = If(mRelease.Count = 0,
                    My.Resources.frmImportRelease_ReleaseNotImportedIntoDatabase, My.Resources.frmImportRelease_ReleaseImportedIntoDatabase)
                Else
                    statusMessage = If(mRelease.Count = 0 OrElse mImportFiles.Where(Function(x) x.Errors.Any).Any,
                                       My.Resources.frmImportRelease_ProcessNotImportedIntoDatabase, My.Resources.frmImportRelease_ProcessImportedIntoDatabase)
                End If
                HandleProgressChangeOnLoadOntoDb(100, statusMessage)
                Dim resultData = jobData.ResultData
                If TypeOf resultData Is Integer Then
                    mImportedReleaseId = CInt(resultData)
                End If
            Case JobMonitoringStatus.Failure
                Dim errorDescription = If(jobData.[Error] IsNot Nothing, jobData.[Error].Message, My.Resources.frmImportRelease_NoErrorInformationAvailable)
                Throw New BackgroundJobException(My.Resources.frmImportRelease_ImportFailed0, errorDescription)
            Case JobMonitoringStatus.Missing
                Throw New BackgroundJobException(My.Resources.frmImportRelease_UnableToGetStatusOfReleaseServerMayHaveStoppedRunning)
            Case JobMonitoringStatus.Timeout
                Throw New BackgroundJobException(My.Resources.frmImportRelease_NoUpdateReceivedWithinTheExpectedTimeTheImportMayHaveStalled)
            Case JobMonitoringStatus.MonitoringError
                Dim exception = jobResult.Exception
                Dim message = If(exception IsNot Nothing, exception.Message, My.Resources.frmImportRelease_NoErrorInformationAvailable)
                Throw New BackgroundJobException(My.Resources.frmImportRelease_AnUnexpectedErrorOccuredWhileWaitingForTheImportToComplete _
                    & message)

        End Select
    End Sub

    ''' <summary>
    ''' Handles the completion of loading the release into the database.
    ''' </summary>
    Private Sub HandleLoadOntoDBCompleted(
     ByVal sender As Object, ByVal e As RunWorkerCompletedEventArgs) _
     Handles mLoadOntoDBStage.RunWorkerCompleted

        Dim ex As Exception = e.Error
        If ex Is Nothing Then
            btnNext.Enabled = True
            mLoadOntoDBStage.Title = My.Resources.frmImportRelease_ImportReleaseComplete
            Dim tp As String = My.Resources.ReleaseL
            If mRelease.IsLegacy Then tp = My.Resources.frmImportRelease_File
            If mRelease.Count = 0 Then
                mLoadOntoDBStage.Title = String.Format(
                 My.Resources.frmImportRelease_The01WasNotImportedIntoTheDatabase,
                 tp, mRelease.Name)
            Else
                mLoadOntoDBStage.Title = String.Format(
                 My.Resources.frmImportRelease_The01HasBeenImportedIntoTheDatabase,
                 tp, mRelease.Name)
            End If
            Completed = True

            If ImporterType = ImportType.ProcessObject AndAlso mProcessImportReportStage IsNot Nothing Then
                mProcessImportReportStage.ImportFiles = mImportFiles
            End If

            ' If a process/VBO is locked and has not already been retried
        ElseIf TypeOf ex Is AlreadyLockedException _
         AndAlso Not mRetryWithForceUnlock Then
            Dim resp As DialogResult = MessageBox.Show(
             My.Resources.frmImportRelease_OneOrMoreOfTheProcessesVBOsToBeOverwrittenIsLocked &
             vbCrLf & ex.Message & vbCrLf & vbCrLf & My.Resources.frmImportRelease_UnlockTheProcesses,
             My.Resources.frmImportRelease_ProcessesObjectsLocked,
             MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation)

            ' If they want to retry reraise an InitStage event
            If resp = DialogResult.OK Then
                mRetryWithForceUnlock = True
                OnInitStage(mLoadOntoDBStage)
            Else
                btnBack.Enabled = True
                btnCancel.Enabled = True
            End If

        Else
            UserMessage.Err(ex,
             My.Resources.frmImportRelease_AnErrorOccurredWhileAttemptingToImportTheRelease01,
             vbCrLf, ex.Message)
            btnBack.Enabled = True
            btnCancel.Enabled = True

            ' Ensure that the retry flag isn't stuck on if the user goes back
            mRetryWithForceUnlock = False

        End If

    End Sub

#End Region

#Region " Help "

    Public Overrides Function GetHelpFile() As String
        Return "relman-import-release.html"
    End Function

#End Region

End Class
