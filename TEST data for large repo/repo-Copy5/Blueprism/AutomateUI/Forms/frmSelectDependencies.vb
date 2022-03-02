Imports AutomateControls
Imports BluePrism.AutomateAppCore
Imports BluePrism.BPCoreLib.Collections

Public Class frmSelectDependencies
    Implements IEnvironmentColourManager

#Region " Class scope declarations "

    ''' <summary>
    ''' Component grid sorter
    ''' </summary>
    Private Class ComponentGridSorter : Implements IComparer

        Public Function Compare(x As Object, y As Object) As Integer Implements IComparer.Compare
            Dim xRow = CType(x, DataGridViewRow)
            Dim yRow = CType(y, DataGridViewRow)

            Return String.Compare(CType(xRow.Cells(Columns.Item).Value, String),
                                  CType(yRow.Cells(Columns.Item).Value, String),
                                  StringComparison.InvariantCultureIgnoreCase)
        End Function
    End Class

    Private Enum Columns
        Image = 0
        Item = 1
        Included = 2
    End Enum
#End Region

#Region "Properties"

    'Dependent components selected
    Public Property SelectedComponents As clsSet(Of PackageComponent)
    'Dependent components not selected
    Public Property UnSelectedComponents As clsSet(Of PackageComponent)

    Private ReadOnly Property CurrentlySelectedComponents As clsSet(Of PackageComponent)
        Get
            Dim currentSelectedComponents = New clsSet(Of PackageComponent)
            For Each r As DataGridViewRow In ItemsDataGridView.Rows
                If CBool(CType(r.Cells(Columns.Included), DataGridViewCheckBoxCell).Value) Then
                    currentSelectedComponents.Add(CType(r.Tag, PackageComponent))
                End If
            Next
            Return currentSelectedComponents
        End Get
    End Property

    Private ReadOnly Property CurrentlyUnSelectedComponents As clsSet(Of PackageComponent)
        Get
            Dim currentUnSelectedComponents = New clsSet(Of PackageComponent)
            For Each r As DataGridViewRow In ItemsDataGridView.Rows
                If Not CBool(CType(r.Cells(Columns.Included), DataGridViewCheckBoxCell).Value) Then
                    currentUnSelectedComponents.Add(CType(r.Tag, PackageComponent))
                End If
            Next
            Return currentUnSelectedComponents
        End Get
    End Property

    Public ReadOnly Property UnsavedChanges As Boolean
        Get
            If SelectedComponents Is Nothing OrElse UnSelectedComponents Is Nothing OrElse
                CurrentlySelectedComponents Is Nothing OrElse CurrentlyUnSelectedComponents Is Nothing Then Return False

            Return Not (CurrentlySelectedComponents.OrderBy(Function(i) i).SequenceEqual(SelectedComponents.OrderBy(Function(i) i)) AndAlso
                        CurrentlyUnSelectedComponents.OrderBy(Function(i) i).SequenceEqual(UnSelectedComponents.OrderBy(Function(i) i)))
        End Get
    End Property

#End Region

    Private mCanLeave As Boolean

#Region "Constructor"

    Public Sub New(comp As PackageComponent, sel As clsSet(Of PackageComponent),
                   unSel As clsSet(Of PackageComponent))

        ' This call is required by the designer.
        InitializeComponent()
        TitleBar.Title = String.Format(My.Resources.DependenciesOf0, PackageComponentType.GetLocalizedFriendlyName(comp.Type))
        TitleBar.SubTitle = comp.Name
        SelectedComponents = sel
        UnSelectedComponents = unSel

        ' Add any initialization after the InitializeComponent() call.
        ImageList1 = BluePrism.Images.ImageLists.Components_16x16
        Dim typeKeys As List(Of String) = PackageComponentType.AllTypes.Keys.ToList()

        Dim allComponents = New clsSet(Of PackageComponent)
        allComponents.AddAll(SelectedComponents)
        allComponents.AddAll(UnSelectedComponents)

        'Build grid of dependencies (indicating which are already selected)
        For Each c As PackageComponent In allComponents
            Dim i As Integer = ItemsDataGridView.Rows.Add(ImageList1.Images(c.TypeKey), c.Name, False)
            ItemsDataGridView.Rows(i).Cells(Columns.Image).ToolTipText = PackageComponentType.GetLocalizedFriendlyName(c.Type)
            ItemsDataGridView.Rows(i).Cells(Columns.Image).Tag = typeKeys.IndexOf(c.TypeKey)
            ItemsDataGridView.Rows(i).Tag = c
            ItemsDataGridView.Rows(i).Cells(Columns.Included).Value = SelectedComponents.Contains(c)
        Next

        ItemsDataGridView.Columns(Columns.Included).HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter

        ItemsDataGridView.Sort(New ComponentGridSorter())

        ApplyChangesButton.Enabled = UnsavedChanges
    End Sub

#End Region

#Region "Event handlers"
    Private Sub SelectAll() Handles SelectAllCheckBox.Click
        For Each row As DataGridViewRow In ItemsDataGridView.Rows
            row.Cells(Columns.Included).Value = SelectAllCheckBox.Checked
        Next

        ItemsDataGridView.EndEdit()
    End Sub

    Private Sub ApplyChanges_Click() Handles ApplyChangesButton.Click
        mCanLeave = True
        If UnsavedChanges Then
            SelectedComponents = CurrentlySelectedComponents
            UnSelectedComponents = CurrentlyUnSelectedComponents

            DialogResult = DialogResult.OK
        Else
            DialogResult = DialogResult.Cancel
        End If

        Close()
    End Sub

    Private Sub ItemClick() Handles ItemsDataGridView.CellValueChanged
        ApplyChangesButton.Enabled = UnsavedChanges
    End Sub

    Private Sub ItemDirtyStateCHanged() Handles ItemsDataGridView.CurrentCellDirtyStateChanged
        If ItemsDataGridView.IsCurrentCellDirty Then ItemsDataGridView.CommitEdit(DataGridViewDataErrorContexts.Commit)
    End Sub

    Private Sub Cancel_Click(sender As Object, e As EventArgs) Handles CancelButtonControl.Click
        mCanLeave = True
        DialogResult = DialogResult.Cancel
        Close()
    End Sub

    Private Sub FormOnClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        If Not mCanLeave AndAlso UnsavedChanges Then
            Dim popup = New Forms.YesNoCancelPopupForm(My.Resources.PopupForm_UnsavedChanges, My.Resources.ThereAreUnsavedChangesToTheDependencies, String.Empty)
            Select Case popup.ShowDialog()
                Case DialogResult.No
                    Return
                Case DialogResult.Cancel
                    e.Cancel = True
                Case DialogResult.Yes
                    ApplyChanges_Click()
            End Select
        End If
    End Sub

    Private Sub SelectDependencies_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        PositionHeaderCheckbox()
    End Sub

    Private Sub SelectDependencies_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        PositionHeaderCheckbox()
    End Sub
#End Region

#Region "Help and Environment colour implementations"

    Public Property EnvironmentBackColor As Color Implements IEnvironmentColourManager.EnvironmentBackColor
        Get
            Return TitleBar.BackColor
        End Get
        Set(value As Color)
            TitleBar.BackColor = value
        End Set
    End Property

    Public Property EnvironmentForeColor As Color Implements IEnvironmentColourManager.EnvironmentForeColor
        Get
            Return TitleBar.TitleColor
        End Get
        Set(value As Color)
            TitleBar.TitleColor = value
            TitleBar.SubtitleColor = value
        End Set
    End Property

#End Region

    Private Sub PositionHeaderCheckbox()
        Dim rect = ItemsDataGridView.GetCellDisplayRectangle(2, -1, True)
        SelectAllCheckBox.Size = New Size(18, 18)
        rect.Y += ItemsDataGridView.Location.Y
        SelectAllCheckBox.Location = New Point(rect.Location.X + (rect.Width \ 2) - (SelectAllCheckBox.Width \ 2), rect.Location.Y + (rect.Height \ 2) - (SelectAllCheckBox.Height \ 2))
    End Sub

End Class
