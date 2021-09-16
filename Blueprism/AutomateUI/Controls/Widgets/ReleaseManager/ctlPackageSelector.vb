Imports BluePrism.AutomateAppCore
Imports BluePrism.Images

''' <summary>
''' Simplistic control for selecting a package from the current list of
''' packages. Nice if you could select a number of different types of
''' things.
''' </summary>
Public Class ctlPackageSelector : Inherits ctlWizardStageControl

#Region " Class scope declarations "

    ''' <summary>
    ''' Event fired when a package is activated.
    ''' </summary>
    ''' <param name="sender">The control which fired the event.</param>
    ''' <param name="pkg">The package which has been activated.</param>
    Public Event PackageActivated(ByVal sender As Object, ByVal pkg As clsPackage)

#End Region

#Region " Member variables "

    ' The adhoc package, used if the user clicks 'Create Adhoc Package'
    Private mAdhocPkg As clsPackage

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new package selector control
    ''' </summary>
    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        lvPackages.SmallImageList = ImageLists.Components_16x16
        rbCreateExisting.Checked = True

    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The packages represented in this package selector control.
    ''' </summary>
    Public Property Packages() As ICollection(Of clsPackage)
        Get
            Return DirectCast(lvPackages.Tag, ICollection(Of clsPackage))
        End Get
        Set(ByVal value As ICollection(Of clsPackage))
            For Each pkg As clsPackage In value
                With lvPackages.Items.Add(pkg.Name, PackageComponentType.Package.Key)
                    .SubItems.Add(pkg.Description)
                    .Tag = pkg
                End With
                lvPackages.Tag = value
            Next
        End Set
    End Property

    ''' <summary>
    ''' Flag indicating whether the package selected is a pre-existing package or
    ''' an adhoc package.
    ''' </summary>
    Public Property IsPreconfigured() As Boolean
        Get
            Return rbCreateExisting.Checked
        End Get
        Set(ByVal value As Boolean)
            If value Then rbCreateExisting.Checked = True Else rbCreateAdhoc.Checked = True
        End Set
    End Property

    ''' <summary>
    ''' The currently selected package - null if no package is selected
    ''' </summary>
    Public Property SelectedPackage() As clsPackage
        Get
            If IsPreconfigured Then
                If lvPackages.SelectedItems.Count <> 1 Then Return Nothing
                Dim item As ListViewItem = lvPackages.SelectedItems(0)
                Return DirectCast(item.Tag, clsPackage)
            End If
            ' Otherwise, it's an adhoc package
            If mAdhocPkg Is Nothing Then mAdhocPkg = New clsPackage(True)
            Return mAdhocPkg
        End Get
        Set(ByVal value As clsPackage)
            If value Is Nothing Then Return
            Dim found As Boolean = False
            For Each item As ListViewItem In lvPackages.Items
                Dim isValue As Boolean = value.Equals(item.Tag)
                item.Selected = isValue
                found = found OrElse isValue
            Next
            If Not found Then
                mAdhocPkg = value
                rbCreateAdhoc.Checked = True
            Else
                mAdhocPkg = Nothing
                rbCreateExisting.Checked = True
            End If
        End Set
    End Property

#End Region

#Region " Event Handlers "

    ''' <summary>
    ''' Handles this control being loaded. This just ensures that the columns on the
    ''' listview are a reasonable size for their content.
    ''' </summary>
    Protected Overrides Sub OnResize(ByVal e As EventArgs)
        MyBase.OnResize(e)
        colName.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent)
        colDescription.Width = -2
    End Sub

    ''' <summary>
    ''' Handles a listview item being activated.
    ''' </summary>
    Private Sub HandleItemActivated(ByVal sender As Object, ByVal e As EventArgs) _
     Handles lvPackages.ItemActivate
        Dim pkg As clsPackage = SelectedPackage
        If pkg IsNot Nothing Then RaiseEvent PackageActivated(Me, SelectedPackage)
    End Sub

    ''' <summary>
    ''' Handles the type of package creation being changed - ie. from an adhoc
    ''' package to a preconfigured package or vice versa
    ''' </summary>
    Private Sub HandleCreateTypeChanged(ByVal sender As Object, ByVal e As EventArgs) _
     Handles rbCreateAdhoc.CheckedChanged, rbCreateExisting.CheckedChanged
        lvPackages.Enabled = rbCreateExisting.Checked
    End Sub

#End Region

End Class
