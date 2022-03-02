Imports AutomateControls
Imports BluePrism.AutomateAppCore
Imports BluePrism.Images

''' <summary>
''' A control which shows a list of packages
''' </summary>
Public Class ctlPackageOverview : Implements IEnvironmentColourManager

#Region " Class scope declarations "

    ''' <summary>
    ''' Event fired when a package has been selected in this overview.
    ''' </summary>
    ''' <param name="sender">The overview control on which the package was selected.
    ''' </param>
    ''' <param name="pkg">The package which was selected (or the package selection
    ''' was cleared)</param>
    Public Event PackageSelected(ByVal sender As Object, ByVal pkg As clsPackage)

    ''' <summary>
    ''' Event fired when a package has been activated in this overview.
    ''' Usually, this means double-clicking it, though selecting with cursor
    ''' keys and pressing enter will probably have the same effect.
    ''' </summary>
    ''' <param name="sender">The overview control on which the package was activated
    ''' </param>
    ''' <param name="pkg">The package which was activated.</param>
    Public Event PackageActivated(ByVal sender As Object, ByVal pkg As clsPackage)

    ''' <summary>
    ''' Event fired when a request to create a package has been made.
    ''' </summary>
    ''' <param name="sender">The source of the request</param>
    Public Event PackageCreateRequested(ByVal sender As Object)

#End Region

#Region " Constructor(s) "

    ''' <summary>
    ''' Creates a new package overview control
    ''' </summary>
    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Just some helpers for spying so that automation can pick up the controls
        Text = My.Resources.PackageOverviewControl
        lvPackages.Text = My.Resources.PackageOverviewList

        ' Add any initialization after the InitializeComponent() call.
        lvPackages.SmallImageList = ImageLists.Components_16x16

    End Sub

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
            Return lblPackageOverview.BackColor
        End Get
        Set(value As Color)
            lblPackageOverview.BackColor = value
            panHeader.BackColor = value
            mMenuButton.BackColor = value
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
            Return lblPackageOverview.ForeColor
        End Get
        Set(value As Color)
            lblPackageOverview.ForeColor = value
        End Set
    End Property

    ''' <summary>
    ''' The packages currently being held by this package overview.
    ''' Note that direct changes to this collection will <em>not</em> be reflected
    ''' in the control
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property Packages() As ICollection(Of clsPackage)
        Get
            Return DirectCast(Me.Tag, ICollection(Of clsPackage))
        End Get
        Set(ByVal value As ICollection(Of clsPackage))
            If value Is Nothing Then value = New clsPackage() {}

            lvPackages.BeginUpdate()
            Try
                lvPackages.Items.Clear()
                For Each pkg As clsPackage In value
                    With lvPackages.Items.Add(pkg.Name, pkg.TypeKey)
                        .Tag = pkg
                        With .SubItems ' created, created-by, last-release
                            .Add(pkg.CreatedListFormat)
                            .Add(pkg.UserName)
                            If pkg.Releases.Count > 0 Then .Add(pkg.LatestRelease.CreatedListFormat)
                        End With
                    End With
                Next
                colName.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent)
                colCreated.Width = 120
                colCreatedBy.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent)
                If colCreatedBy.Width < 100 Then colCreatedBy.Width = 100
                colLastRelease.Width = 120
                Me.Tag = value
            Finally
                lvPackages.EndUpdate()
            End Try
        End Set
    End Property

    ''' <summary>
    ''' The currently selected packages in this overview, or an empty collection if
    ''' none is currently selected.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property SelectedPackages() As ICollection(Of clsPackage)
        Get
            Dim pkgs As New List(Of clsPackage)
            For Each item As ListViewItem In lvPackages.SelectedItems
                pkgs.Add(DirectCast(item.Tag, clsPackage))
            Next
            Return pkgs
        End Get
    End Property

    ''' <summary>
    ''' The currently focused package in this overview, or null if no package is
    ''' currently selected.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property FocusedPackage() As clsPackage
        Get
            Dim item As ListViewItem = lvPackages.FocusedItem
            If item IsNot Nothing Then Return DirectCast(item.Tag, clsPackage)
            Return Nothing
        End Get
    End Property

#End Region

#Region " Event Handlers "

    ''' <summary>
    ''' Handles a list view item representing a package being activated.
    ''' </summary>
    Private Sub HandleItemActivated(ByVal sender As Object, ByVal e As EventArgs) _
     Handles lvPackages.ItemActivate
        If lvPackages.SelectedItems.Count <> 1 Then Return ' Abort..
        RaiseEvent PackageActivated(Me, DirectCast(lvPackages.SelectedItems(0).Tag, clsPackage))
    End Sub

    ''' <summary>
    ''' Handles a list view item representing a package being selected.
    ''' </summary>
    Private Sub HandleItemSelected(ByVal sender As Object, ByVal e As ListViewItemSelectionChangedEventArgs) Handles lvPackages.ItemSelectionChanged
        If lvPackages.SelectedItems.Count = 1 Then
            RaiseEvent PackageSelected(Me, DirectCast(lvPackages.SelectedItems(0).Tag, clsPackage))
        Else
            RaiseEvent PackageSelected(Me, Nothing)
        End If
    End Sub

    ''' <summary>
    ''' Handles the 'Create Package' button being clicked.
    ''' </summary>
    Private Sub HandleCreatePackageClicked(ByVal sender As Object, ByVal e As EventArgs) _
        Handles CreateToolStripMenuItem.Click
        RaiseEvent PackageCreateRequested(Me)
    End Sub

#End Region

End Class
