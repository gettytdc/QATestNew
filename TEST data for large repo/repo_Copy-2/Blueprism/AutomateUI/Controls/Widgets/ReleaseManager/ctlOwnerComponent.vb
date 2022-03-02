Imports AutomateControls
Imports AutomateUI.My.Resources
Imports BluePrism.AutomateAppCore
Imports BluePrism.Images

''' <summary>
''' Base class representing a component group.
''' By default, the second panel of the splitter contains nothing, and is
''' collapsed when the control is displayed. The
''' <see cref="ctlOwnerComponent.HasDetailPanel"/> property should be overridden to
''' return true in order for the splitter panel to be shown.
''' See <see cref="ctlPackageDetails"/> for an example of how this is handled.
''' </summary>
Public Class ctlOwnerComponent : Implements IEnvironmentColourManager

    ''' <summary>
    ''' Creates a new component group control
    ''' </summary>
    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        mSplitter.Panel2Collapsed = Not HasDetailPanel
        colContentsIcon.ImageList = ImageLists.Components_16x16

        If Me.HasComponentTree Then
            cTree.Visible = True
            gridContents.Visible = False
        End If
    End Sub

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
            Return lblTitle.BackColor
        End Get
        Set(value As Color)
            lblTitle.BackColor = value
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
            Return lblTitle.ForeColor
        End Get
        Set(value As Color)
            lblTitle.ForeColor = value
        End Set
    End Property

    ''' <summary>
    ''' Flag indicating whether the detail panel is used for this control.
    ''' False by default, indicating that the splitter pane's second panel
    ''' should be collapsed when this control is displayed.
    ''' </summary>
    Protected Overridable ReadOnly Property HasDetailPanel() As Boolean
        Get
            Return False
        End Get
    End Property

    ''' <summary>
    ''' Indicates components should be displayed in tree form (so currently package
    ''' contents, not release contents)
    ''' </summary>
    Protected Overridable ReadOnly Property HasComponentTree() As Boolean
        Get
            Return False
        End Get
    End Property

    ''' <summary>
    ''' The label indicating the user action on this component group - typically
    ''' either "Created" or "Imported" (for non-local releases)
    ''' </summary>
    Protected Property UserActionLabel() As String
        Get
            Return lblUserAction.Text
        End Get
        Set(ByVal value As String)
            lblUserAction.Text = value
        End Set
    End Property

    Private mShowWarningSymbol As Boolean = False
    Protected Property ShowWarningSymbol As Boolean
        Get
            Return mShowWarningSymbol
        End Get
        Set
            If Value Xor mShowWarningSymbol Then
                If Value Then
                    lblContents.Text &= My.Resources.AutomateUI_Controls.General_WarningSymbol
                    mToolTip.SetToolTip(lblContents, WarningMessage)
                Else
                    lblContents.Text = lblContents.Text.Substring(0, lblContents.Text.Length - My.Resources.AutomateUI_Controls.General_WarningSymbol.Length)
                    mToolTip.SetToolTip(lblContents, String.Empty)
                End If
                mShowWarningSymbol = Value
            End If
        End Set
    End Property

    Private mWarningMessage As String = String.Empty
    Protected Property WarningMessage As String
        Get
            Return mWarningMessage
        End Get
        Set
            mWarningMessage = value
            If mShowWarningSymbol Then mToolTip.SetToolTip(lblContents, WarningMessage)
        End Set
    End Property

    ''' <summary>
    ''' The component which is being modelled by this component group.
    ''' </summary>
    Protected Overridable Property Component() As OwnerComponent
        Get
            Return DirectCast(Me.Tag, OwnerComponent)
        End Get
        Set(ByVal value As OwnerComponent)
            Me.Tag = value
            If value Is Nothing Then
                txtName.Text = ""
                txtDescription.Text = ""
                lblCreatedDate.Text = ""
                lblUser.Text = ""
                If Not HasComponentTree Then
                    gridContents.Rows.Clear()
                Else
                    cTree.Nodes.Clear()
                End If
                Return
            End If
            txtName.Text = value.Name
            txtDescription.Text = value.Description
            lblCreatedDate.Text = value.CreatedDisplayFormat(Resources.DateTimeFormat_ctlOwnerComponent_DddDdMMMYyyyAtHHMmSs)
            lblUser.Text = value.UserName
            If Not HasComponentTree Then
                If TypeOf value Is clsPackage Then
                    colInfo.Visible = True
                Else
                    colInfo.Visible = False
                End If
                With gridContents.Rows
                    .Clear()
                    For Each comp As PackageComponent In value.Members
                        Dim index As Integer
                        If TypeOf value Is clsPackage Then
                            Dim sb As New StringBuilder()
                            For Each dep As PackageComponent In comp.GetDependents()
                                sb.Append(String.Format(My.Resources.x012, dep.Type.Label, dep.Name, vbCrLf))
                            Next
                            For Each grp As KeyValuePair(Of Guid, String) In comp.GetGroupInfo()
                                sb.Append(String.Format(My.Resources.Group01, grp.Value, vbCrLf))
                            Next
                            If sb.Length > 0 Then
                                index = .Add(comp.TypeKey, comp.Name, BluePrism.Images.ToolImages.Information_16x16)
                                .Item(index).Cells(2).Tag = sb.ToString()
                                .Item(index).Cells(2).ToolTipText = My.Resources.ViewAssociatedItems
                            Else
                                index = .Add(comp.TypeKey, comp.Name, New Bitmap(16, 16))
                            End If
                        Else
                            If TypeOf comp Is GroupComponent Then
                                index = .Add(PackageComponentType.Group.Key, comp.Name)
                            Else
                                index = .Add(comp.TypeKey, comp.Name)
                            End If
                        End If

                        .Item(index).Tag = comp
                        .Item(index).Cells(0).ToolTipText = PackageComponentType.GetLocalizedFriendlyName(comp.Type)
                    Next
                End With
                gridContents.Sort(colContentsIcon, ListSortDirection.Ascending)
                gridContents.ClearSelection()
            Else
                cTree.Nodes.Clear()
                Dim comps As New List(Of PackageComponent)
                For Each c As PackageComponent In value.Members
                    Dim groups As IDictionary(Of Guid, String) = c.GetGroupInfo()
                    If groups.Count > 0 Then
                        PackComponents(c, groups, comps)
                    Else
                        comps.Add(c)
                    End If
                Next
                cTree.AddComponents(comps)
            End If
        End Set
    End Property

    ''' <summary>
    ''' Pack groupable components into their group hiearchy
    ''' </summary>
    ''' <param name="comp">The component to pack</param>
    ''' <param name="groups">The groups that the component exists in</param>
    ''' <param name="comps">The resulting list of components</param>
    Private Sub PackComponents(comp As PackageComponent, groups As IDictionary(Of Guid, String), comps As ICollection(Of PackageComponent))
        For Each gpInfo As KeyValuePair(Of Guid, String) In groups
            Dim lastGrp As GroupComponent = Nothing
            Dim gps As String() = gpInfo.Value.Split(Convert.ToChar("/"))
            For i As Integer = gps.Count - 1 To 0 Step -1
                Dim path As String = Join(gps.Take(i + 1).ToArray(), "/")
                Dim gp As GroupComponent = New GroupComponent(Nothing, Nothing, path, comp.Type, False)
                If lastGrp Is Nothing Then
                    gp.Add(comp)
                Else
                    gp.Add(lastGrp)
                End If
                lastGrp = gp
            Next
            comps.Add(lastGrp)
        Next
    End Sub

    ''' <summary>
    ''' Handles the sorting of the icon column, ensuring that a secondary sort on
    ''' the name of the component is performed.
    ''' </summary>
    Private Sub HandleSort(ByVal sender As Object, ByVal e As DataGridViewSortCompareEventArgs) Handles gridContents.SortCompare

        ' We only handle icon sorting, so ignore anything that isn't an icon sort.
        If e.Column Is colContentsIcon Then

            ' No matter what happens from here on in, we're handling this sort
            e.Handled = True

            ' First test the actual icon key
            Dim ct1 As PackageComponentType = PackageComponentType.AllTypes(DirectCast(e.CellValue1, String))
            Dim ct2 As PackageComponentType = PackageComponentType.AllTypes(DirectCast(e.CellValue2, String))

            ' Get the difference between the natural orders of the icon keys.
            ' If they are different, then we have our sort result.
            Dim diff As Integer = PackageComponentType.CompareTypes(ct1, ct2)
            If diff <> 0 Then e.SortResult = diff : Return

            ' The icons are the same, so we must perform a secondary sort on the name.
            Dim name1 As String = DirectCast(
             gridContents.Rows(e.RowIndex1).Cells(colContentsName.Index).Value, String)
            Dim name2 As String = DirectCast(
             gridContents.Rows(e.RowIndex2).Cells(colContentsName.Index).Value, String)

            e.SortResult = String.Compare(name1, name2, True)

        End If
    End Sub

    ''' <summary>
    ''' Handles a mouse-click in a cell (specifically the info column)
    ''' </summary>
    Private Sub HandleCellContentClick(sender As Object, e As DataGridViewCellEventArgs) _
     Handles gridContents.CellContentClick
        If gridContents.Columns(e.ColumnIndex) IsNot colInfo Then Return

        Dim cell As DataGridViewCell = gridContents.Rows(e.RowIndex).Cells(e.ColumnIndex)
        If cell.Tag Is Nothing Then Return

        Dim cellRec As Rectangle = gridContents.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, True)
        UserMessage.ShowFloating(gridContents.Parent, ToolTipIcon.Info, My.Resources.TheseItemsWillAlsoBeExported,
                                 cell.Tag.ToString(), New Point(cellRec.X, gridContents.Top + cellRec.Y))
    End Sub

    ''' <summary>
    ''' Handles the expand/collapse menu options
    ''' </summary>
    Private Sub HandleExpandCollapse(sender As Object, e As EventArgs) _
     Handles mnuExpandAll.Click, mnuCollapseAll.Click
        If sender Is mnuExpandAll Then _
            cTree.ExpandAll() Else _
            cTree.CollapseAll()
    End Sub

End Class
