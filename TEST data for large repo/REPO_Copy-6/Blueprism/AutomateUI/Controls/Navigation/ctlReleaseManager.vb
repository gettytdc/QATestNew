Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports AutomateControls
Imports BluePrism.Server.Domain.Models

''' <summary>
''' Overarching control which manages packages and releases. This brings together
''' a package tree along with the overview and detail panels to represent the
''' packages and releases in that tree
''' It also manages the wizards which create and modify packages as well as those
''' to create new releases.
''' </summary>
Friend Class ctlReleaseManager
    Implements IPermission, IStubbornChild, IHelp, IEnvironmentColourManager

#Region " Member Variables "

    ' The package overview control
    Private WithEvents mPackageOverview As ctlPackageOverview
    ' The package details control - may be null before a package is selected
    Private WithEvents mPackageDetails As ctlPackageDetails
    ' The release details control - may be null before a release is selected
    Private WithEvents mReleaseDetails As ctlReleaseDetails

    ' The packages being managed by this control
    Private mPackages As ICollection(Of clsPackage)

    ' Flag indicating that an event is being handled
    Private mHandlingSelectionEvent As Boolean

    ' The last set environment back color in this control
    Private mSavedEnvironmentBackColor As Color =
        ColourScheme.Default.EnvironmentBackColor

    ' The last set environment fore color in this control
    Private mSavedEnvironmentForeColor As Color =
        ColourScheme.Default.EnvironmentForeColor

#End Region

#Region " Properties "

    ''' <summary>
    ''' Gets or sets the environment-specific back colour in use in this environment.
    ''' Only set to the database-held values after login.
    ''' </summary>
    ''' <remarks>Note that this only affects the UI owned directly by this form - ie.
    ''' setting the colour here will not update the database</remarks>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property EnvironmentBackColor As Color _
     Implements IEnvironmentColourManager.EnvironmentBackColor
        Get
            Return mSavedEnvironmentBackColor
        End Get
        Set(value As Color)
            mSavedEnvironmentBackColor = value
            Dim ctl As IEnvironmentColourManager =
                TryCast(DetailControl, IEnvironmentColourManager)
            If ctl IsNot Nothing Then ctl.EnvironmentBackColor = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the environment-specific back colour in use in this environment.
    ''' Only set to the database-held values after login.
    ''' </summary>
    ''' <remarks>Note that this only affects the UI owned directly by this form - ie.
    ''' setting the colour here will not update the database</remarks>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property EnvironmentForeColor As Color _
     Implements IEnvironmentColourManager.EnvironmentForeColor
        Get
            Return mSavedEnvironmentForeColor
        End Get
        Set(value As Color)
            mSavedEnvironmentForeColor = value
            Dim ctl As IEnvironmentColourManager =
                TryCast(DetailControl, IEnvironmentColourManager)
            If ctl IsNot Nothing Then ctl.EnvironmentForeColor = value
        End Set
    End Property

    ''' <summary>
    ''' The packages being modelled by this release manager.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property Packages() As ICollection(Of clsPackage)
        Get
            Return mPackages
        End Get
        Set(ByVal value As ICollection(Of clsPackage))
            mPackages = New List(Of clsPackage)(value)
            mPackageTree.Packages = mPackages
        End Set
    End Property

    ''' <summary>
    ''' Gets the currently selected package, if one is currently selected.
    ''' If the package details control is visible, this is the package which is
    ''' currently being displayed. If the release details control is visible, this is
    ''' the package from which the release was created. If the package overview
    ''' control is visible, this is the currently <em>focused</em> package.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Private ReadOnly Property CurrentPackage() As clsPackage
        Get
            Dim ctl As Control = DetailControl
            If ctl Is Nothing Then Return Nothing
            If ctl Is mPackageDetails Then Return mPackageDetails.Package
            If ctl Is mPackageOverview Then Return mPackageOverview.FocusedPackage
            If ctl Is mReleaseDetails Then Return mReleaseDetails.Package
            Return Nothing
        End Get
    End Property
#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new release manager using the packages registered on the database.
    ''' </summary>
    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        UpdatePackages()
        EnforcePermissionRestrictions()
    End Sub

    ''' <summary>
    ''' Refreshes the packages displayed in the tree
    ''' </summary>
    Private Sub UpdatePackages()

        Try
            Me.Packages = gSv.GetPackages(True)

        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlReleaseManager_FailedToRetrievePackageInformation0, ex.Message), ex)

        End Try

    End Sub

    Private Sub EnforcePermissionRestrictions()
        Dim hasPermissionToImportRelease = User.Current.HasPermission(Permission.ReleaseManager.ImportRelease)

        If Not hasPermissionToImportRelease Then ctxImportRelease.Enabled = False
    End Sub

#End Region

#Region " Detail Panel "

    ''' <summary>
    ''' The panel which hosts the detail control in this object
    ''' </summary>
    Protected ReadOnly Property DetailHostPanel() As Panel
        Get
            Return splitMain.Panel2
        End Get
    End Property

    ''' <summary>
    ''' The detail control in this component to the given control.
    ''' Setting this to null has the effect of removing any detail control currently
    ''' set in this component.
    ''' </summary>
    Public Property DetailControl() As Control
        Get
            Dim pan As Panel = DetailHostPanel
            If pan.Controls.Count = 0 Then Return Nothing
            If pan.Controls.Count > 1 Then Throw New BluePrismException(
             "More than one detail control found")
            Return pan.Controls(0)
        End Get
        Set(ByVal value As Control)
            If DetailControl Is value Then Return
            With DetailHostPanel.Controls
                .Clear()
                If value IsNot Nothing Then
                    value.Dock = DockStyle.Fill
                    Dim colMgr As IEnvironmentColourManager =
                        TryCast(value, IEnvironmentColourManager)
                    If colMgr IsNot Nothing Then
                        colMgr.SetEnvironmentColours(Me)
                    End If
                    .Add(value)
                End If
            End With
        End Set
    End Property

#End Region

#Region " Event Handlers "

    ''' <summary>
    ''' Handles context menu opening.
    ''' </summary>
    Private Sub HandleContextMenuOpening(sender As Object, e As CancelEventArgs) _
     Handles mPackageManagerContextMenu.Opening
        ctxDeletePackage.Enabled = Not mPackageTree.SelectedOverview
        ctxModifyPackage.Enabled = Not mPackageTree.SelectedOverview
    End Sub

    ''' <summary>
    ''' Handles the package overview being selected in the package tree
    ''' </summary>
    ''' <param name="sender">The source of the event</param>
    ''' <param name="pkgs">The collection of packages being displayed in the overview
    ''' </param>
    Private Sub HandleOverviewSelected(
     ByVal sender As Object, ByVal pkgs As ICollection(Of clsPackage)) _
     Handles mPackageTree.OverviewSelected

        If mHandlingSelectionEvent Then Return ' Already being handled, ignore it.
        mHandlingSelectionEvent = True
        Try
            If mPackageOverview Is Nothing Then mPackageOverview = New ctlPackageOverview()
            mPackageOverview.Packages = pkgs
            mPackageTree.SelectedOverview = True
            DetailControl = mPackageOverview
        Finally
            mHandlingSelectionEvent = False
        End Try

    End Sub

    ''' <summary>
    ''' Handles a package being chosen - effectively a request to view a package's
    ''' details. It listens for a package being selected on the package tree as well
    ''' as the package being activated in the package overview list.
    ''' </summary>
    ''' <param name="sender">The source of the event</param>
    ''' <param name="pkg">The package which has been selected</param>
    Private Sub HandlePackageActivated(ByVal sender As Object, ByVal pkg As clsPackage) _
     Handles mPackageTree.PackageSelected, mPackageOverview.PackageActivated
        If mHandlingSelectionEvent Then Return ' Already being handled, ignore it.
        mHandlingSelectionEvent = True
        Try
            If mPackageDetails Is Nothing Then mPackageDetails = New ctlPackageDetails()
            mPackageDetails.Package = pkg
            mPackageTree.SelectedPackage = pkg
            DetailControl = mPackageDetails
        Finally
            mHandlingSelectionEvent = False
        End Try

    End Sub

    ''' <summary>
    ''' Handles a release being chosen - effectively a request to view a release's
    ''' details. It listens for a release being selected in the package tree as well
    ''' as a release being activated in a package details control
    ''' </summary>
    ''' <param name="sender">The source of the event</param>
    ''' <param name="rel">The release which has been selected</param>
    Private Sub HandleReleaseActivated(ByVal sender As Object, ByVal rel As clsRelease) _
     Handles mPackageTree.ReleaseSelected, mPackageDetails.ReleaseActivated

        If mHandlingSelectionEvent Then Return ' Already being handled, ignore it.
        mHandlingSelectionEvent = True
        Try
            If mReleaseDetails Is Nothing Then mReleaseDetails = New ctlReleaseDetails()
            mReleaseDetails.Release = rel
            mPackageTree.SelectedRelease = rel
            DetailControl = mReleaseDetails
        Finally
            mHandlingSelectionEvent = False
        End Try

    End Sub

    ''' <summary>
    ''' Handles a request to create a new package
    ''' </summary>
    Private Sub HandleCreatePackageRequested(ByVal sender As Object, ByVal e As EventArgs) _
      Handles ctxNewPackage.Click
        HandleCreatePackageRequested(sender)
    End Sub


    ''' <summary>
    ''' Handles a request to create a new package.
    ''' This opens and processes a <see cref="frmPackageBuilderWizard"/> which is
    ''' responsible for creating a new package, and updates the child controls of
    ''' this release manager with the new package.
    ''' </summary>
    ''' <param name="sender">The source of the event.</param>
    Private Sub HandleCreatePackageRequested(ByVal sender As Object) _
     Handles mPackageOverview.PackageCreateRequested
        Dim f As New frmPackageBuilderWizard(mPackages)
        If mParent.StartForm(f, True) = DialogResult.OK Then
            Try
                Dim pkg As clsPackage = f.Package
                mPackages.Add(pkg)
                mPackageTree.UpdatePackage(mPackages, 0, pkg)
                mPackageOverview.Packages = mPackages

            Catch ex As Exception
                UserMessage.Show(String.Format("Failed to retrieve package information: {0}", ex.Message), ex)

            End Try
        End If
    End Sub

    ''' <summary>
    ''' Handles a request to modify the currently selected package. If not package is
    ''' selected, this has no effect.
    ''' </summary>
    ''' <param name="sender">The source of the event</param>
    ''' <param name="e">Not much</param>
    Private Sub HandleModifyPackageRequested(ByVal sender As Object, ByVal e As EventArgs) _
      Handles ctxModifyPackage.Click
        HandleModifyPackageRequested(sender, CurrentPackage)
    End Sub

    ''' <summary>
    ''' Handles a request to modify the specified package.
    ''' </summary>
    ''' <param name="sender">The source of the event.</param>
    ''' <param name="pkg">The package which should be modified.</param>
    Private Sub HandleModifyPackageRequested(ByVal sender As Object, ByVal pkg As clsPackage) _
     Handles mPackageDetails.PackageModifyRequested
        If pkg Is Nothing Then Return
        Dim wiz As New frmPackageBuilderWizard(DirectCast(pkg.Clone(), clsPackage), mPackages)
        If mParent.StartForm(wiz, True) = DialogResult.OK Then
            Dim id As Integer = pkg.IdAsInteger
            ' We need to replace the package in our collection with the new one.
            mPackages.Remove(pkg)
            mPackages.Add(wiz.Package)
            ' Now we let all the child controls know that the package has changed.
            ' Update on the tree, so it can keep hold of its existing packages (and
            ' maintain reference equality in line with this control)
            mPackageTree.UpdatePackage(mPackages, id, wiz.Package)
            ' Overview can just update its list en masse
            mPackageOverview.Packages = mPackages
            ' If the package details control is there and showing the affected package,
            ' ensure it is updated too
            If mPackageDetails IsNot Nothing AndAlso mPackageDetails.Package.IdAsInteger = id Then
                mPackageDetails.Package = wiz.Package
            End If
        End If
    End Sub

    ''' <summary>
    ''' Handles a new release being requested for the currently selected package
    ''' </summary>
    Private Sub HandleNewReleaseRequested(ByVal sender As Object, ByVal e As EventArgs) _
     Handles ctxNewRelease.Click
        ' This has come from the action menu - we need to figure out which package has
        ' been selected / is currently open.
        HandleNewReleaseRequested(sender, CurrentPackage)
    End Sub

    ''' <summary>
    ''' Handles a new release being requested for the specified package.
    ''' </summary>
    ''' <param name="sender">The source of the event</param>
    ''' <param name="pkg">The package for which a release is required. A null package
    ''' is ignored.</param>
    Private Sub HandleNewReleaseRequested(ByVal sender As Object, ByVal pkg As clsPackage) _
     Handles mPackageDetails.NewReleaseRequested
        If pkg Is Nothing Then Return
        Dim wiz As New frmCreateRelease(pkg)
        If mParent.StartForm(wiz, True) = DialogResult.OK Then
            ' Update the other controls
            mPackageTree.UpdatePackage(mPackages, pkg.IdAsInteger, pkg)
            If mPackageDetails IsNot Nothing AndAlso mPackageDetails.Package Is pkg Then
                mPackageDetails.Package = pkg
            End If
        End If
    End Sub

    ''' <summary>
    ''' Handles a request to delete the currently selected package
    ''' </summary>
    ''' <param name="sender"></param>
    Private Sub HandleDeletePackageRequested(ByVal sender As Object, ByVal e As EventArgs) _
     Handles ctxDeletePackage.Click
        HandleDeletePackageRequested(sender, CurrentPackage)
    End Sub

    ''' <summary>
    ''' Handles a request to delete the specified package.
    ''' </summary>
    Private Sub HandleDeletePackageRequested(ByVal sender As Object, ByVal pkg As clsPackage)
        If pkg Is Nothing Then Return
        ' Check permissions first
        If Not User.Current.HasPermission("Delete Package") Then
            UserMessage.Show(My.Resources.ctlReleaseManager_TheDeletPackagePermissionIsRequiredForThisAction)
            Return
        End If
        If UserMessage.YesNo(String.Format(My.Resources.ctlReleaseManager_AreYouSureYouWantToDeleteThePackage0,
                             pkg.Name)) = MsgBoxResult.Yes Then
            Try
                gSv.DeletePackage(pkg)
            Catch ex As Exception
                UserMessage.Show(String.Format(
                 String.Format(My.Resources.ctlReleaseManager_AnErrorOccurredTryingToDeleteThePackage01, pkg.Name, ex.Message)),
                 ex)
                Return
            End Try
            mPackages.Remove(pkg)
            mPackageTree.UpdatePackage(mPackages, pkg.IdAsInteger, Nothing)
            mPackageOverview.Packages = mPackages
            ' We don't need to update the package details - if the package is removed, the
            ' tree will move the selection to the next or previous node, thus overwriting
            ' the currently selected package
        End If
    End Sub

    Private Sub HandleImportCompleted(sender As Object, e As EventArgs) _
     Handles mParent.ImportCompleted
        BeginInvoke(New FunctionDelegate(AddressOf UpdatePackages))
    End Sub

    Private Delegate Sub FunctionDelegate()
    ''' <summary>
    ''' Handles an Import Release request being made somewhere in the UI
    ''' </summary>
    Private Sub HandleImportReleaseRequested(ByVal sender As Object, ByVal e As EventArgs) _
      Handles ctxImportRelease.Click
        Dim wiz As New frmImportRelease()
        Dim res As DialogResult = mParent.StartForm(wiz, True)
        ' We need to update the packages in the tree and detail panel
        If res = DialogResult.OK Then

            ' Refresh local packages and tree so we are in sync with changes made during
            ' import (e.g. components being skipped etc)
            UpdatePackages()

            ' Select imported release in tree. If a legacy release has been imported there 
            ' won't be a package or release record to select (id will be zero)
            If wiz.ImportedReleaseId <> 0 Then
                Dim selectedRelease = mPackages _
                        .SelectMany(Function(p) p.Releases) _
                        .FirstOrDefault(Function(r) r.IdAsInteger = wiz.ImportedReleaseId)
                If (selectedRelease IsNot Nothing) Then
                    mPackageTree.SelectedRelease = selectedRelease
                End If

            End If

        End If
    End Sub

    ''' <summary>
    ''' Handles a verify release request being made.
    ''' </summary>
    Private Sub HandleVerifyReleaseRequested(ByVal sender As Object, ByVal e As EventArgs) _
      Handles ctxVerifyRelease.Click
        mParent.StartForm(New frmVerifyRelease(), True)
    End Sub

#End Region

#Region " Interface implementations "

    ''' <summary>
    ''' Gets the help file for this control
    ''' </summary>
    ''' <returns>The filename for the help page which elucidates this control
    ''' </returns>
    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return "relman-ui.html"
    End Function

    Private WithEvents mParent As frmApplication
    Friend Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
        Get
            Return mParent
        End Get
        Set(value As frmApplication)
            mParent = value
        End Set
    End Property


    ''' <summary>
    ''' Checks if navigation can leave this control. It can
    ''' </summary>
    ''' <returns>True</returns>
    Public Function CanLeave() As Boolean Implements IStubbornChild.CanLeave
        Return True
    End Function

    ''' <summary>
    ''' Gets the permissions required for, er, viewing this release manager?
    ''' </summary>
    Public ReadOnly Property RequiredPermissions() As ICollection(Of Permission) Implements IPermission.RequiredPermissions
        Get
            Return Permission.ByName("View Release Manager")
        End Get
    End Property

#End Region

End Class
