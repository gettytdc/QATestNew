Imports AutomateUI.Classes
Imports BluePrism.AutomateAppCore
Imports BluePrism.Images

''' <summary>
''' Control to handle the package details.
''' </summary>
Public NotInheritable Class ctlPackageDetails : Inherits ctlOwnerComponent

    ''' <summary>
    ''' Event fired when a release is double clicked in the list of releases.
    ''' </summary>
    ''' <param name="sender">The control which fired the event.</param>
    ''' <param name="rel">The release which has been activated</param>
    Public Event ReleaseActivated(ByVal sender As Object, ByVal rel As clsRelease)

    ''' <summary>
    ''' Event fired when a release is added to a package within this control
    ''' </summary>
    ''' <param name="sender">The control which is firing the event</param>
    ''' <param name="pkg">The package on which a release has been added.</param>
    ''' <param name="rel">The release which has been added.</param>
    Public Event ReleaseAdded(ByVal sender As Object, _
     ByVal pkg As clsPackage, ByVal rel As clsRelease)

    ''' <summary>
    ''' Event fired when a package is changed by this control. That is that a new
    ''' instance of the package is created, not that the value of the instance has
    ''' changed - that has no corresponding event.
    ''' </summary>
    ''' <param name="sender">The control which fired the event.</param>
    ''' <param name="oldPackage">The old package. That which has been superseded.
    ''' </param>
    ''' <param name="newPackage">The new package. That which has superseded the old
    ''' package.</param>
    Public Event PackageChanged(ByVal sender As Object, _
     ByVal oldPackage As clsPackage, ByVal newPackage As clsPackage)

    ''' <summary>
    ''' Event fired when a request to modify a package is made.
    ''' </summary>
    ''' <param name="sender">The source of the event</param>
    ''' <param name="package">The package which is to be modified.</param>
    Public Event PackageModifyRequested(ByVal sender As Object, ByVal package As clsPackage)

    ''' <summary>
    ''' Event fired when a request to create a new release is made
    ''' </summary>
    ''' <param name="sender">The source of the event.</param>
    ''' <param name="package">The package for which a new release is required</param>
    Public Event NewReleaseRequested(ByVal sender As Object, ByVal package As clsPackage)

    Private ReadOnly mUserMessage As IUserMessage

    ''' <summary>
    ''' Creates a new Package Details control
    ''' </summary>
    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        mUserMessage = New UserMessageWrapper()

        ' Add any initialization after the InitializeComponent() call.
        colReleaseIcon.ImageList = ImageLists.Components_16x16

    End Sub

    ''' <summary>
    ''' Flag indicating that this package details control has a detail panel.
    ''' </summary>
    Protected Overrides ReadOnly Property HasDetailPanel() As Boolean
        Get
            Return True
        End Get
    End Property

    ''' <summary>
    ''' Flag indicating that package components should be displayed in a treeview.
    ''' </summary>
    Protected Overrides ReadOnly Property HasComponentTree As Boolean
        Get
            Return True
        End Get
    End Property

    ''' <summary>
    ''' The component being viewed by this control.
    ''' </summary>
    Protected Overrides Property Component() As OwnerComponent
        Get
            Return MyBase.Component
        End Get
        Set(ByVal value As OwnerComponent)
            MyBase.Component = value
            With gridReleaseHistory.Rows
                .Clear()
                If TypeOf value Is clsPackage Then
                    Dim pkg As clsPackage = DirectCast(value, clsPackage)
                    For Each rel As clsRelease In pkg.Releases
                        Dim index As Integer = _
                         .Add(rel.TypeKey, rel.Name, rel.Created.ToLocalTime, rel.UserName)
                        .Item(index).Tag = rel
                    Next
                End If
            End With
            Me.Tag = value
            ' Sort the releases into date order
            gridReleaseHistory.Sort(colReleaseDate, ListSortDirection.Ascending)

            ' And scroll to the last row (if there is one there)
            Dim i As Integer = gridReleaseHistory.Rows.GetLastRow(DataGridViewElementStates.None)
            If i >= 0 Then gridReleaseHistory.CurrentCell = gridReleaseHistory.Rows(i).Cells(0)

            ' Then clear the selection
            gridReleaseHistory.ClearSelection()

        End Set
    End Property

    ''' <summary>
    ''' The package being viewed by this control
    ''' </summary>
    Public Property Package As clsPackage
        Get
            Return DirectCast(Component, clsPackage)
        End Get
        Set
            Dim oldPackage = Package
            Component = value
            OnPackageChanged(oldPackage, value)
            ShowWarningSymbol = value.ContainsInaccessibleItems
            WarningMessage = My.Resources.AutomateUI_Controls.ctlPackageDetails_Package_InaccessibleItemsWarning
        End Set
    End Property

    ''' <summary>
    ''' Handles a grid cell being double clicked, translating it into a 'Release
    ''' Activated' event.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub HandleReleaseActivated( _
     ByVal sender As Object, ByVal e As DataGridViewCellMouseEventArgs) _
     Handles gridReleaseHistory.CellMouseDoubleClick

        If e.RowIndex < 0 OrElse e.ColumnIndex < 0 Then Return
        Dim rel As clsRelease = DirectCast(gridReleaseHistory.Rows(e.RowIndex).Tag, clsRelease)
        RaiseEvent ReleaseActivated(Me, rel)

    End Sub

    ''' <summary>
    ''' Handles the "New Release" button being clicked.
    ''' </summary>
    Private Sub HandleNewReleaseClicked(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnNewRelease.Click
        RaiseEvent NewReleaseRequested(Me, Me.Package)
    End Sub

    ''' <summary>
    ''' Handler for a release being added via this control
    ''' </summary>
    ''' <param name="pkg">The package which is being added to.</param>
    ''' <param name="rel">The release that has been added to the given package.
    ''' </param>
    Protected Sub OnReleaseAdded(ByVal pkg As clsPackage, ByVal rel As clsRelease)
        Me.Component = pkg
        RaiseEvent ReleaseAdded(Me, pkg, rel)
    End Sub

    ''' <summary>
    ''' Handles the 'Edit Package' button being clicked.
    ''' </summary>
    Private Sub HandleEditPackageClicked(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnEditPackage.Click

        If Package.ContainsInaccessibleItems
            If Not mUserMessage.ShowYesNo(My.Resources.AutomateUI_Controls.ctlPackageDetails_HandleEditPackageClicked_WarningMessage)
                Return
            End If
        End If

        RaiseEvent PackageModifyRequested(Me, Package)
    End Sub

    ''' <summary>
    ''' Handler for a package being changed via this control
    ''' </summary>
    ''' <param name="oldPackage">The old package</param>
    ''' <param name="newPackage">The new package</param>
    Private Sub OnPackageChanged(oldPackage As clsPackage, newPackage As clsPackage)
        ShowWarningSymbol = newPackage.ContainsInaccessibleItems
        RaiseEvent PackageChanged(Me, oldPackage, newPackage)
    End Sub

End Class
