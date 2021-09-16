
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth

''' <summary>
''' Wizard used for 'building' a package - more specifically, this can be used to
''' create or modify a package.
''' </summary>
Friend Class frmPackageBuilderWizard
    Inherits frmStagedWizard
    Implements IPermission

#Region " Static Methods "

    ''' <summary>
    ''' Creates the stages required for this wizard.
    ''' </summary>
    ''' <param name="pkg">The package that is being built by this wizard.</param>
    ''' <returns>A list of wizard stages for use in this wizard.</returns>
    Private Shared Function CreateStages(ByVal pkg As clsPackage) As IList(Of WizardStage)
        Dim stages As New List(Of WizardStage)
        stages.Add(New NameStage("package"))
        stages.Add(New DescriptionStage("package"))
        stages.Add(New PackageBundlerStage())
        stages.Add(New BackgroundWorkerStage(My.Resources.frmPackageBuilderWizard_CreatingPackage, My.Resources.frmPackageBuilderWizard_PleaseWait))
        Return stages
    End Function

#End Region

#Region " Constants "

    ''' <summary>
    ''' Label to set in the work stage when the create wizard is doing the work
    ''' </summary>
    Private Shared CreateWizardWorking As String = "Creating Package"

    ''' <summary>
    ''' Label to set in the work stage when the modify wizard is doing the work
    ''' </summary>
    Private Shared ModifyWizardWorking As String = "Modifying Package"

    ''' <summary>
    ''' Title to set in the work stage when the create wizard has done the work
    ''' </summary>
    Private Shared CreateWizardDoneTitle As String = "Package Created"

    ''' <summary>
    ''' Subtitle to set in the work stage when the create wizard has done the work
    ''' </summary>
    Private Shared CreateWizardDoneSubTitle As String = "The package '{0}' has been created"

    ''' <summary>
    ''' Title to set in the work stage when the modify wizard has done the work
    ''' </summary>
    Private Shared ModifyWizardDoneTitle As String = My.Resources.frmPackageBuilderWizard_PackageModified

    ''' <summary>
    ''' Subtitle to set in the work stage when the modify wizard has done the work
    ''' </summary>
    Private Shared ModifyWizardDoneSubTitle As String = "The package '{0}' has been modified"

#End Region

#Region " Private Member Variables "

    ' Flag indicating that this wizard is creating a new package
    ' False indicates that it is modifying an existing package
    Private mNewPackage As Boolean

    ' The name stage for this wizard.
    Private WithEvents mNameStage As NameStage

    ' The description stage for this wizard
    Private WithEvents mDescStage As DescriptionStage

    ' The actual package bundling stage for this wizard
    Private WithEvents mPackageStage As PackageBundlerStage

    ' The worker stage which actually performs the exporting
    Private WithEvents mWorkerStage As BackgroundWorkerStage

    ' The collection of existing packages from the release manager
    Private mExistingPackages As ICollection(Of clsPackage)

    ' The package which is being built
    Private mPackage As clsPackage

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new package builder wizard, which will create a new package,
    ''' ensuring that its name does not clash with the given collection of packages.
    ''' </summary>
    ''' <param name="existingPackages">The collection of existing packages to ensure
    ''' do not clash with the new package</param>
    Public Sub New(ByVal existingPackages As ICollection(Of clsPackage))
        Me.New(Nothing, existingPackages)
    End Sub

    ''' <summary>
    ''' Creates a new package builder wizard for the given package, checking for
    ''' name collisions against the given collection of packages.
    ''' </summary>
    ''' <param name="pkg">The package to build - if this is null, a new package will
    ''' be created; otherwise, the given package will be modified.</param>
    ''' <param name="existingPackages">The collection of existing packages to check
    ''' for name collisions against.</param>
    Public Sub New(ByVal pkg As clsPackage, ByVal existingPackages As ICollection(Of clsPackage))
        MyBase.New(
         CStr(IIf(pkg Is Nothing, My.Resources.frmPackageBuilderWizard_CreatePackage, My.Resources.frmPackageBuilderWizard_ModifyPackage)), CreateStages(pkg))

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        CreateWizardWorking = My.Resources.frmPackageBuilderWizard_CreatingPackage
        ModifyWizardWorking = My.Resources.frmPackageBuilderWizard_ModifyingPackage
        CreateWizardDoneTitle = My.Resources.frmPackageBuilderWizard_PackageCreated
        CreateWizardDoneSubTitle = My.Resources.frmPackageBuilderWizard_ThePackage0HasBeenCreated
        ModifyWizardDoneTitle = My.Resources.frmPackageBuilderWizard_PackageModified
        ModifyWizardDoneSubTitle = My.Resources.frmPackageBuilderWizard_ThePackage0HasBeenModified

        ' Add any initialization after the InitializeComponent() call.
        mPackage = pkg
        mExistingPackages = existingPackages
        If mPackage Is Nothing Then
            mNewPackage = True
            mPackage = New clsPackage()
        End If

        mNameStage = DirectCast(GetStage(NameStage.StageId), NameStage)
        mDescStage = DirectCast(GetStage(DescriptionStage.StageId), DescriptionStage)
        mPackageStage = DirectCast(GetStage(PackageBundlerStage.StageId), PackageBundlerStage)
        mWorkerStage = DirectCast(GetStage(BackgroundWorkerStage.DefaultStageId), BackgroundWorkerStage)

        ' Set the working label to either "Creating" or "Modifying" as appropriate
        mWorkerStage.Title = Me.WizardWorkingLabel

    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' Gets the package which is being / has been built by this wizard
    ''' </summary>
    Public ReadOnly Property Package() As clsPackage
        Get
            Return mPackage
        End Get
    End Property

    ''' <summary>
    ''' Checks if this wizard is currently creating a new package or modifying an
    ''' existing one.
    ''' </summary>
    Private ReadOnly Property IsNewPackage() As Boolean
        Get
            Return mNewPackage
        End Get
    End Property

    ''' <summary>
    ''' The label indicating that the wizard is currently performing its work
    ''' </summary>
    Private ReadOnly Property WizardWorkingLabel() As String
        Get
            If IsNewPackage Then Return CreateWizardWorking Else Return ModifyWizardWorking
        End Get
    End Property

    ''' <summary>
    ''' The title indicating that the wizard has completed its work
    ''' </summary>
    Private ReadOnly Property WizardDoneTitle() As String
        Get
            If IsNewPackage Then Return CreateWizardDoneTitle Else Return ModifyWizardDoneTitle
        End Get
    End Property

#End Region

#Region " Interface Implementations "

    ''' <summary>
    ''' Gets the permissions required for opening this wizard
    ''' </summary>
    Public ReadOnly Property RequiredPermissions() As ICollection(Of Permission) _
     Implements IPermission.RequiredPermissions
        Get
            Return Permission.ByName("Create/Edit Package")
        End Get
    End Property

#End Region

#Region " Override Methods "

    ''' <summary>
    ''' Initialises the given stage.
    ''' </summary>
    ''' <param name="stg">The stage to initialise</param>
    Protected Overrides Sub OnInitStage(ByVal stg As WizardStage)
        MyBase.OnInitStage(stg)
        Select Case True
            Case stg Is mNameStage
                mNameStage.Name = mPackage.Name

            Case stg Is mDescStage
                mDescStage.Description = mPackage.Description

            Case stg Is mPackageStage
                mPackageStage.PackageComponents = mPackage.Members

        End Select
    End Sub

    ''' <summary>
    ''' Go back a page. This just ensures that the loadOntoDBStage titles are
    ''' correct.
    ''' </summary>
    Protected Overrides Sub BackPage()
        MyBase.BackPage()
        mWorkerStage.Title = String.Format(My.Resources.frmPackageBuilderWizard_0PleaseWait, Me.WizardWorkingLabel)
    End Sub

#End Region

#Region " Event Handling Methods "

    ''' <summary>
    ''' Handles the name stage committing - this checks for name collisions against
    ''' the existing packages.
    ''' </summary>
    Private Sub HandleNameCommitting(
     ByVal sender As WizardStage, ByVal e As StageCommittingEventArgs) Handles mNameStage.Committing
        Dim name As String = mNameStage.Name
        For Each pkg As clsPackage In mExistingPackages
            If pkg.Name = name AndAlso mPackage.IdAsInteger <> pkg.IdAsInteger Then
                UserMessage.Show(String.Format(
                 My.Resources.frmPackageBuilderWizard_ThePackage0AlreadyExistsPleaseEnterADifferentName, name))
                e.Cancel = True
                Return
            End If
        Next
    End Sub

    ''' <summary>
    ''' Handles the name being committed, saving it into the package being built
    ''' by this wizard.
    ''' </summary>
    Private Sub HandleNameCommitted(
     ByVal sender As WizardStage, ByVal e As StageCommittedEventArgs) Handles mNameStage.Committed
        mPackage.Name = mNameStage.Name
    End Sub

    ''' <summary>
    ''' Handles the description being committed, saving it into the package being
    ''' built by this wizard.
    ''' </summary>
    Private Sub HandleDescriptionCommitted(
     ByVal sender As WizardStage, ByVal e As StageCommittedEventArgs) Handles mDescStage.Committed
        mPackage.Description = mDescStage.Description
    End Sub

    ''' <summary>
    ''' Handles the package contents being committed, saving them into the package
    ''' being built by this wizard.
    ''' </summary>
    Private Sub HandlePackageContentsCommitted(
     ByVal sender As WizardStage, ByVal e As StageCommittedEventArgs) Handles mPackageStage.Committed
        mPackage.Members.Clear()
        mPackage.AddAll(mPackageStage.PackageComponents)
    End Sub

    ''' <summary>
    ''' Performs the actual work required by this wizard - in this case, the
    ''' saving of the package to the database.
    ''' </summary>
    Private Sub HandleDoWork(ByVal sender As Object, ByVal e As DoWorkEventArgs) Handles mWorkerStage.DoWork
        mWorkerStage.ReportProgress(25, My.Resources.frmPackageBuilderWizard_SavingPackage)
        mPackage.ContainsInaccessibleItems = False
        Dim pkg As clsPackage = gSv.SavePackage(mPackage)
        If pkg IsNot mPackage Then
            ' We need to ensure that our ID is up to date with that returned from
            ' the server
            mPackage.Id = pkg.Id
        End If
        mWorkerStage.ReportProgress(100, My.Resources.frmPackageBuilderWizard_Done)
    End Sub

    ''' <summary>
    ''' Handles the worker stage completing.
    ''' </summary>
    Private Sub HandleWorkerComplete( _
     ByVal sender As Object, ByVal e As RunWorkerCompletedEventArgs) Handles mWorkerStage.RunWorkerCompleted

        If e.Error Is Nothing Then
            mWorkerStage.Title = Me.WizardDoneTitle
            Completed = True
        Else
            btnBack.Enabled = True
            btnCancel.Enabled = True
            btnNext.Enabled = False
        End If

    End Sub

#End Region

#Region " Help "

    Public Overrides Function GetHelpFile() As String
        If Me.IsNewPackage _
            Then Return "relman-new-package.html" _
            Else Return "relman-modify-package.html"
    End Function


#End Region

End Class
