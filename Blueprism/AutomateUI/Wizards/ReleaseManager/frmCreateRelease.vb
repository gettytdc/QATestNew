Imports System.IO

Imports BluePrism.Server.Domain.Models

Imports BluePrism.AutomateProcessCore

Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.BPCoreLib

Friend Class frmCreateRelease
    Inherits frmStagedWizard
    Implements IPermission

    ' Wizard class used for creating a release within a package.
#Region " Class-scope definitions "

    ''' <summary>
    ''' The preference name for the output directory to use for the release
    ''' </summary>
    ''' <remarks></remarks>
    Private Const ReleaseDirectoryPrefName As String = "release.dir"

    ''' <summary>
    ''' Creates the stages used in this wizard, based on the given package.
    ''' </summary>
    ''' <param name="pkg">The package from which a release is being created. If this
    ''' is a null, the first stage will allow the user to select a package.</param>
    ''' <returns>The list of stages for this wizard, given the specified package.
    ''' </returns>
    Private Shared Function CreateStages(ByVal pkg As clsPackage) As IList(Of WizardStage)
        Dim stages As New List(Of WizardStage)
        ' Include the 'choose package' stage if one is not chosen
        With stages
            If pkg Is Nothing Then
                stages.Add(New SelectPackageStage())
                stages.Add(New PackageBundlerStage())
            End If
            stages.Add(New NameStage("release"))
            stages.Add(New DescriptionStage(My.Resources.frmCreateRelease_ReleaseNotes,
             My.Resources.frmCreateRelease_PleaseEnterAnyReleaseNotesForThisRelease))
            stages.Add(New OutputFileStage(ReleaseDirectoryPrefName))
            stages.Add(New BackgroundWorkerStage(My.Resources.frmCreateRelease_ExportRelease, My.Resources.frmCreateRelease_ExportingTheRelease))
        End With
        Return stages
    End Function

#End Region

#Region " Member variables "

    ' The name of the release
    Private mName As String

    ' The description of the release
    Private mDescription As String

    ' The file name to which the release should be exported
    Private mFileName As String

    ' An adhoc package, created with the process that the create release was
    ' requested from. This will be non-null only if the wizard was initiated from a
    ' process without specifying a particular predefined package.
    Private mAdhocPackage As clsPackage

    ' The package, from which a release is being created.
    Private mPackage As clsPackage

    ' The release that has/is being created
    Private mRelease As clsRelease

    ' The stage which selects a package - may be null if the package was given at
    ' construction time
    Private WithEvents mPackageSelector As SelectPackageStage

    ' The stage with which an adhoc package can be created.
    Private WithEvents mPackageBundler As PackageBundlerStage

    ' The stage which accepts a release name
    Private WithEvents mNameStage As NameStage

    ' The stage which accepts a description
    Private WithEvents mDescriptionStage As DescriptionStage

    ' The stage which accepts an output file
    Private WithEvents mFileChooser As OutputFileStage

    ' The stage which provides the framework for doing the work
    Private WithEvents mWorkerStage As BackgroundWorkerStage

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new 'Create Release' wizard, which contains a stage on which
    ''' the user can choose the package that the release should be made from.
    ''' </summary>
    Public Sub New()
        Me.New(Nothing, Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new Create Release wizard for a new package, potentially an adhoc
    ''' package prepopulated with the given process.
    ''' </summary>
    ''' <param name="proc">The process for which a release is required.</param>
    Public Sub New(ByVal proc As clsProcess)
        Me.New(Nothing, proc)
    End Sub

    ''' <summary>
    ''' Creates a new Create Release wizard, based on the given package.
    ''' </summary>
    ''' <param name="pkg">The package to create the release from. If this is null,
    ''' the user will be shown a list of packages in the system to choose from.
    ''' </param>
    Public Sub New(ByVal pkg As clsPackage)
        Me.New(pkg, Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new 'Create Release' wizard, based on the given package or process
    ''' </summary>
    ''' <param name="pkg">The package to create the release from. If this is null,
    ''' the user will be shown a list of packages in the system to choose from.
    ''' </param>
    ''' <param name="proc">The process for which a release is required - the user
    ''' will be prompted for a package to create a release from, or to create an
    ''' adhoc package initialised with the given process.</param>
    Private Sub New(ByVal pkg As clsPackage, ByVal proc As clsProcess)
        MyBase.New(My.Resources.frmCreateRelease_CreateRelease, CreateStages(pkg))

        mPackageSelector = DirectCast(GetStage(SelectPackageStage.StageId), SelectPackageStage)
        mPackageBundler = DirectCast(GetStage(PackageBundlerStage.StageId), PackageBundlerStage)

        mFileChooser = DirectCast(GetStage(OutputFileStage.StageId), OutputFileStage)
        mNameStage = DirectCast(GetStage(NameStage.StageId), NameStage)
        mDescriptionStage = DirectCast(GetStage(DescriptionStage.StageId), DescriptionStage)
        mWorkerStage = DirectCast(GetStage(BackgroundWorkerStage.DefaultStageId), BackgroundWorkerStage)

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        mFileChooser.AddExtensionEntry(clsRelease.FileExtension, My.Resources.frmCreateRelease_BluePrismRelease)
        mDescriptionStage.Prompt = My.Resources.frmCreateRelease_ReleaseNotes

        ' If we have a process, create an adhoc package to hold it.
        If proc IsNot Nothing Then
            mAdhocPackage = New clsPackage(True)

            Dim pcomp As ProcessComponent = ProcessComponent.Create(mAdhocPackage, proc)
            mAdhocPackage.Add(pcomp)
        End If

        mPackage = pkg

    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' Gets the permissions required for opening this wizard
    ''' </summary>
    Public ReadOnly Property RequiredPermissions() As ICollection(Of Permission) _
     Implements IPermission.RequiredPermissions
        Get
            Return Permission.ByName("Create Release")
        End Get
    End Property

    ''' <summary>
    ''' The release that was created by this wizard.
    ''' </summary>
    Public ReadOnly Property Release() As clsRelease
        Get
            Return mRelease
        End Get
    End Property

#End Region

#Region " Methods "

    ''' <summary>
    ''' Initialises the given stage.
    ''' </summary>
    ''' <param name="stg">The stage which is to be initialised.</param>
    Protected Overrides Sub OnInitStage(ByVal stg As WizardStage)
        MyBase.OnInitStage(stg)
        Select Case True

            Case stg Is mPackageSelector
                mPackageSelector.SelectedPackage = mPackage

            Case stg Is mPackageBundler
                mPackageBundler.PackageComponents = mPackage.Members

            Case stg Is mNameStage
                mNameStage.Name = mName

            Case stg Is mDescriptionStage
                mDescriptionStage.Description = GetDefaultNotes()

            Case stg Is mFileChooser
                If mFileName Is Nothing Then mFileChooser.SuggestedName = mName

            Case stg Is mWorkerStage
                btnNext.Enabled = False

        End Select

    End Sub

    ''' <summary>
    ''' Handles this wizard stepping next, ensuring that the package bundler stage
    ''' is skipped if the user has chosen a preselected package.
    ''' </summary>
    Protected Overrides Sub OnSteppingNext(ByVal e As WizardSteppingEventArgs)
        ' Skip the package bundler if the user has chosen a predefined package
        If e.Stage Is mPackageBundler AndAlso mPackageSelector.IsPreconfigured Then
            e.Skip = True
        End If
    End Sub

    ''' <summary>
    ''' Pre-populates the release notes with any warnings (e.g. where child objects
    ''' are being exported without their parents).
    ''' </summary>
    ''' <returns>Default release notes</returns>
    Private Function GetDefaultNotes() As String
        Dim relNotes As String = String.Empty
        Dim objList As New Dictionary(Of Guid, String)

        'Get a list of objects contained in this release
        For Each c As PackageComponent In mPackage.Members
            If Not TypeOf c Is VBOComponent Then Continue For
            objList.Add(c.IdAsGuid, c.Name)
        Next
        If objList.Count = 0 Then Return String.Empty

        'Get any associated parent object names and create pre-populated release
        'notes if any parents are not included.
        Dim sb As New StringBuilder()
        Dim i As Integer = 0
        For Each o As KeyValuePair(Of Guid, String) In gSv.GetParentReferences(objList.Keys.ToList())
            If Not mPackage.Members.Any(Function(c) TypeOf c Is VBOComponent AndAlso c.Name = o.Value) Then
                i += 1
                sb.AppendLine(String.Format(My.Resources.frmCreateRelease_01ModelOwnedBy2, i, objList(o.Key), o.Value))
            End If
        Next
        If sb.Length > 0 Then
            relNotes = String.Format(My.Resources.frmCreateRelease_TheFollowingObjectsDependOnTheApplicationModelOfParentObjectsNotIncludedWithThi,
             Environment.NewLine, sb.ToString())
        End If

        Return relNotes
    End Function
#End Region

#Region " Event Handlers "

    ''' <summary>
    ''' Handles a package being selected in the package selector stage.
    ''' </summary>
    Private Sub HandlePackageSelected(ByVal sender As WizardStage,
     ByVal e As StageCommittedEventArgs) Handles mPackageSelector.Committed
        Dim pkg As clsPackage = mPackageSelector.SelectedPackage

        ' If it's changed then reset the file name.
        If pkg IsNot mPackage Then mFileName = Nothing

        ' If the user selected an adhoc package and we have already created a base
        ' for it, use that - it is prepopulated with the process it was created from.
        If pkg.IsAdHoc AndAlso mAdhocPackage IsNot Nothing Then
            ' We clone it, so that if the user adds components to it,
            ' then goes back to page 1 and selected a different package,
            ' then goes back to page 1 and selects ad-hoc again, it is
            ' started from scratch - ie. just containing the process.
            mPackage = DirectCast(mAdhocPackage.Clone(), clsPackage)

            ' If the user selected an adhoc package and we have not yet created one,
            ' we're creating an adhoc package from nothing; the given package from
            ' the package selector is empty, so we can just use that.
        ElseIf pkg.IsAdHoc Then

            ' Again, clone it so that it can be changed and discarded by choosing
            ' different selections on page 1.
            mPackage = DirectCast(pkg.Clone(), clsPackage)

        Else
            mPackage = pkg
        End If
    End Sub

    ''' <summary>
    ''' Handles an adhoc package's contents being chosen
    ''' </summary>
    Private Sub HandlePackageContentsChosen(
     ByVal sender As WizardStage, ByVal e As StageCommittedEventArgs) _
     Handles mPackageBundler.Committed
        mPackage.Members.Clear()
        mPackage.AddAll(mPackageBundler.PackageComponents)
    End Sub

    ''' <summary>
    ''' Handles the name stage committing - this just checks for duplicate names to
    ''' ensure that no two releases within a package have the same name.
    ''' </summary>
    Private Sub HandleNameCommitting(
     ByVal sender As WizardStage, ByVal e As StageCommittingEventArgs) Handles mNameStage.Committing
        Dim name As String = mNameStage.Name
        If mPackage.ContainsReleaseNamed(name) Then
            MessageBox.Show(String.Format(
             My.Resources.frmCreateRelease_ThisPackageAlreadyContainsAReleaseWithTheName01PleaseChooseAnother,
             name, vbCrLf), My.Resources.frmCreateRelease_DuplicateName, MessageBoxButtons.OK, MessageBoxIcon.Error)
            e.Cancel = True
        End If
    End Sub

    ''' <summary>
    ''' Handles the name stage being committed.
    ''' </summary>
    Private Sub HandleNameCommitted(ByVal sender As WizardStage, ByVal e As StageCommittedEventArgs) Handles mNameStage.Committed
        mName = mNameStage.Name
    End Sub

    ''' <summary>
    ''' Handles the description stage being committed.
    ''' </summary>
    Private Sub HandleDescriptionCommitted(
     ByVal sender As WizardStage, ByVal e As StageCommittedEventArgs) Handles mDescriptionStage.Committed
        mDescription = mDescriptionStage.Description
    End Sub

    ''' <summary>
    ''' Handles the file being chosen in the file chooser stage.
    ''' </summary>
    Private Sub HandleFileChosen(ByVal sender As WizardStage, ByVal e As StageCommittedEventArgs) _
     Handles mFileChooser.Committed
        mFileName = mFileChooser.FileName
    End Sub

    ''' <summary>
    ''' Handles the worker stage's "DoWork" event - this actually creates the
    ''' release, saves it to the database and exports it to the chosen file.
    ''' </summary>
    Private Sub HandleDoWork(ByVal sender As Object, ByVal e As DoWorkEventArgs) _
     Handles mWorkerStage.DoWork

        ' If package selector stage is present and not a preconfigured package, we
        ' are dealing with an adhoc package
        Dim adhoc As Boolean =
         (mPackageSelector IsNot Nothing AndAlso Not mPackageSelector.IsPreconfigured)

        mWorkerStage.ReportProgress(5, My.Resources.frmCreateRelease_CreatingRelease)

        ' The package is just a package info - we need the real thing now...
        mRelease = mPackage.CreateRelease(mName)
        Try
            mRelease.Description = mDescription
            mWorkerStage.ReportProgress(25, My.Resources.frmCreateRelease_SavingToFile)

            ' If the package exists on the database, save a release attached to it
            If Not adhoc AndAlso Not gSv.IsValidRelease(mRelease) Then
                Throw New NameAlreadyExistsException(
                 My.Resources.frmCreateRelease_AReleaseWithTheName0AlreadyExistsOnThePackage1,
                 mRelease.Name, mRelease.Package.Name)
            End If

            Dim monitor As New clsProgressMonitor()
            AddHandler monitor.ProgressChanged, AddressOf HandleProgressChangeOnExport

            mRelease.Export(New FileInfo(mFileChooser.FileName), monitor)

            If adhoc Then
                mWorkerStage.ReportProgress(75, My.Resources.frmCreateRelease_SkippedSavingAdhocReleaseToDatabase)
            Else
                mWorkerStage.ReportProgress(75, My.Resources.frmCreateRelease_SavingToDatabase)
                gSv.CreateRelease(mRelease)
            End If
            mWorkerStage.ReportProgress(100, String.Format(
             My.Resources.frmCreateRelease_Release0SavedToFile21, mRelease.Name, mFileChooser.FileName, vbCrLf))

        Catch
            ' We need to make sure that the package doesn't have the release assigned
            ' to it, since it failed to export correctly
            mPackage.Releases.Remove(mRelease)

            ' And rethrow to let the WorkerCompleted event deal with it
            Throw

        End Try

    End Sub

    ''' <summary>
    ''' Handles the progress changing in the export processing
    ''' </summary>
    ''' <param name="value">The progress percentage as an integer</param>
    ''' <param name="status">The status message to display</param>
    Private Sub HandleProgressChangeOnExport(ByVal value As Integer, ByVal status As Object)
        mWorkerStage.ReportProgress(25 + (value \ 2), status)
    End Sub

    ''' <summary>
    ''' Handles the run worker being completed.
    ''' </summary>
    Private Sub HandleRunWorkerCompleted(
     ByVal sender As Object, ByVal e As RunWorkerCompletedEventArgs) _
     Handles mWorkerStage.RunWorkerCompleted

        Dim ex As Exception = e.Error
        If ex Is Nothing Then ' It worked...
            btnNext.Enabled = True
            Me.Completed = True
            Return

        ElseIf TypeOf ex Is NameAlreadyExistsException Then
            ' No need to *fail* fail - just give an error and return them to the name stage
            MessageBox.Show(ex.Message, My.Resources.frmCreateRelease_Error, MessageBoxButtons.OK, MessageBoxIcon.Error)
            SetStage(mNameStage)

        Else
            UserMessage.Show(My.Resources.frmCreateRelease_AnErrorOccurredWhileAttemptingToCreateTheRelease & e.Error.Message, e.Error)
            btnBack.Enabled = True
            btnCancel.Enabled = True

        End If

    End Sub

#End Region

#Region " Help "

    Public Overrides Function GetHelpFile() As String
        Return "relman-new-release.html"
    End Function

#End Region

End Class
