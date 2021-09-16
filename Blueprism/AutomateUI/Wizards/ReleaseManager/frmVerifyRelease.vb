Imports System.IO

Imports BluePrism.AutomateAppCore
Imports BluePrism.BPCoreLib
Imports BluePrism.Server.Domain.Models

''' <summary>
''' Wizard class used for creating a release within a package.
''' </summary>
Friend Class frmVerifyRelease
    Inherits frmStagedWizard

#Region " Class-scope definitions "

    ''' <summary>
    ''' The preference name for the directory to search for the release
    ''' </summary>
    ''' <remarks></remarks>
    Private Const ReleaseDirectoryPrefName As String = "release.dir"

    ''' <summary>
    ''' Creates the stages used in this wizard.
    ''' </summary>
    ''' <returns>The list of stages for this wizard.
    ''' </returns>
    Private Shared Function CreateStages() As IList(Of WizardStage)
        Dim stages As New List(Of WizardStage)
        ' Include the 'choose package' stage if one is not chosen

        stages.Add(New InputFileStage(ReleaseDirectoryPrefName))
        stages.Add(New BackgroundWorkerStage("parse.stage", My.Resources.frmVerifyRelease_ReadingFile, My.Resources.frmVerifyRelease_PleaseWait))
        stages.Add(New ReleaseDifferenceStage())
        stages.Add(New MessageStage(My.Resources.frmVerifyRelease_ReleaseVerified, Nothing, Nothing))
        Return stages

    End Function

#End Region

#Region " Member variables "

    ' The name of the file to get the release from
    Private mFilename As String

    ' The incoming release to test
    Private mRelease As clsRelease

    ' The package this release comes from.
    Private mPackage As clsPackage

    ' The differences mapped against their components
    Private mDifferences As IDictionary(Of PackageComponent, String)

    ' The stage which accepts an input file
    Private WithEvents mFileChooser As InputFileStage

    ' The stage which provides the framework for doing the work
    Private WithEvents mLoadAndParseStage As BackgroundWorkerStage

    ' The stage on which the collisions can be resolved
    Private WithEvents mDiffStage As ReleaseDifferenceStage

    ' The stage within which the components are actually loaded into the database
    Private WithEvents mMessageStage As MessageStage

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new 'Import Release' wizard, based on the given package.
    ''' </summary>
    Public Sub New()
        MyBase.New(My.Resources.frmVerifyRelease_VerifyRelease, CreateStages())

        mFileChooser = DirectCast(GetStage(InputFileStage.StageId), InputFileStage)
        mLoadAndParseStage = DirectCast(GetStage("parse.stage"), BackgroundWorkerStage)
        mDiffStage = DirectCast(GetStage(ReleaseDifferenceStage.StageId), ReleaseDifferenceStage)
        mMessageStage = DirectCast(GetStage(MessageStage.StageId), MessageStage)

        mFileChooser.Title = My.Resources.frmVerifyRelease_PleaseSelectTheReleaseFileYouWishToVerify
        mFileChooser.AddExtensionEntry(clsRelease.FileExtension, My.Resources.frmVerifyRelease_BluePrismRelease)

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

    End Sub

#End Region

#Region " Methods "

    ''' <summary>
    ''' Initialises the given stage.
    ''' </summary>
    ''' <param name="stg">The stage which is to be initialised.</param>
    Protected Overrides Sub OnInitStage(ByVal stg As WizardStage)
        MyBase.OnInitStage(stg)
        Select Case True

            Case stg Is mLoadAndParseStage
                btnNext.Enabled = False

            Case stg Is mDiffStage
                mDiffStage.Differences = mDifferences
                btnNext.Text = My.Resources.frmVerifyRelease_Finish

        End Select

    End Sub

#End Region

#Region " Event Handlers "

    ''' <summary>
    ''' Handles the file being chosen in the file chooser stage.
    ''' </summary>
    Private Sub HandleFileChosen(ByVal sender As WizardStage, ByVal e As StageCommittedEventArgs) _
     Handles mFileChooser.Committed
        mFilename = mFileChooser.FileName
    End Sub

    ''' <summary>
    ''' Handles progress on the loading and parsing of the input file changing
    ''' </summary>
    ''' <param name="value">The percentage of progress to report.</param>
    ''' <param name="status">The status text for the change</param>
    Private Sub HandleProgressChange(ByVal value As Integer, ByVal status As Object)
        ' If release is not set, assume we're loading/parsing the file at the mo;
        ' otherwise, assume we're comparing the release.
        If mRelease Is Nothing Then _
         mLoadAndParseStage.ReportProgress(10 + (value \ 2), status) _
        Else _
         mLoadAndParseStage.ReportProgress(90 + (value \ 10), status)
    End Sub

    ''' <summary>
    ''' Handles the worker stage's "DoWork" event - this actually creates the
    ''' release, saves it to the database and exports it to the chosen file.
    ''' </summary>
    ''' <exception cref="NoSuchElementException">If the package that the release was
    ''' created from is not found in this system.</exception>
    Private Sub HandleLoadAndParseWork(ByVal sender As Object, ByVal e As DoWorkEventArgs) _
     Handles mLoadAndParseStage.DoWork
        ' The package is just a package info - we need the real thing now...
        mLoadAndParseStage.ReportProgress(10, My.Resources.frmVerifyRelease_ImportingFile)
        Dim monitor As New clsProgressMonitor()
        AddHandler monitor.ProgressChanged, AddressOf HandleProgressChange
        Try
            mRelease = clsRelease.Import(New FileInfo(mFilename), monitor, False)
        Finally
            RemoveHandler monitor.ProgressChanged, AddressOf HandleProgressChange
        End Try

        mLoadAndParseStage.ReportProgress(75, My.Resources.frmVerifyRelease_OpeningPackage)
        Dim pkg As clsPackage = mRelease.Package
        mPackage = gSv.GetPackage(pkg.Name)
        If mPackage Is Nothing Then Throw New NoSuchElementException(
         My.Resources.frmVerifyRelease_ThePackageThatThisReleaseWasCreatedFrom0WasNotFoundOnThisSystemCannotVerifyRele, pkg.Name)

        mLoadAndParseStage.ReportProgress(90, My.Resources.frmVerifyRelease_ComparingRelease)
        Dim newRel As clsRelease = mPackage.CreateRelease()
        newRel.Name = My.Resources.frmVerifyRelease_CurrentEnvironment
        mDifferences = newRel.Diff(mRelease, monitor)

        mLoadAndParseStage.ReportProgress(100, My.Resources.frmVerifyRelease_Done)

    End Sub

    ''' <summary>
    ''' Handles the run worker being completed.
    ''' </summary>
    Private Sub HandleLoadAndParseCompleted(
     ByVal sender As Object, ByVal e As RunWorkerCompletedEventArgs) _
     Handles mLoadAndParseStage.RunWorkerCompleted
        Dim ex As Exception = e.Error
        If ex Is Nothing Then ' It worked...
            btnNext.Enabled = True
            btnNext.PerformClick()
            Return
        ElseIf TypeOf ex Is NoSuchElementException Then
            UserMessage.Show(
             My.Resources.frmVerifyRelease_OnlyReleasesCreatedFromPackagesInThisEnvironmentCanBeVerified)
            btnBack.Enabled = False
        Else
            UserMessage.Err(ex,
             My.Resources.frmVerifyRelease_AnErrorOccurredWhileAttemptingToLoadTheRelease01,
             vbCrLf, ex.Message)
            btnBack.Enabled = True
        End If
        btnCancel.Enabled = True
    End Sub

    ''' <summary>
    ''' Handles the stepping due to the next button, ensuring that the differences
    ''' stage is not displayed if there are no differences.
    ''' </summary>
    Protected Overrides Sub OnSteppingNext(ByVal e As WizardSteppingEventArgs)
        If e.Stage Is mDiffStage AndAlso mDifferences.Count = 0 Then
            mMessageStage.Text = String.Format(
             My.Resources.frmVerifyRelease_NoDifferencesFoundRelease0IsUpToDate, mRelease.Name)
            e.Skip = True
        End If
    End Sub

    ''' <summary>
    ''' Handles the differences being committed - if they pressed Next on the
    ''' differences stage, that means the wizard is done.
    ''' </summary>
    Private Sub HandleDifferencesCommitted(ByVal sender As WizardStage, _
     ByVal e As StageCommittedEventArgs) Handles mDiffStage.Committed
        Completed = True
        Close()
    End Sub

#End Region

#Region " Help "

    Public Overrides Function GetHelpFile() As String
        Return "relman-verify-release.html"
    End Function

#End Region

End Class
