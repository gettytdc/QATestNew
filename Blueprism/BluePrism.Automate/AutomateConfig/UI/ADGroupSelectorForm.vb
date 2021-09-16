Imports System.DirectoryServices.AccountManagement
Imports System.ComponentModel
Imports System.Resources
Imports BluePrism.AutomateAppCore
Imports System.Security.Principal
Imports AutomateControls
Imports LocaleTools

Public Class ADGroupSelectorForm
    Inherits AutomateControls.Forms.AutomateForm
    Implements IEnvironmentColourManager

    ''' <summary>
    ''' ResourceManager for custom resources belonging to control
    ''' </summary>
    Private Shared ReadOnly CustomResources As ResourceManager = _
        My.Resources.ADGroupSelectorForm_Resources.ResourceManager

    ''' <summary>
    ''' The Blue Prism role that the AD Group is being selected for
    ''' </summary>
    Private mRole As String = Nothing

    ''' <summary>
    ''' The AD Domain in which the selector will search for groups
    ''' </summary>
    Private mDomain As String

    ''' <summary>
    ''' Only search for Security Groups (i.e. not Distribution Groups)
    ''' </summary>
    Private mSecurityGroupsOnly As Boolean = False

    ' The selected group name - not set on entry; only set by user interation on
    ' this form.
    Private mSelectedGroupName As String

    ''' <summary>
    ''' The Active Directory group currently selected on the form
    ''' </summary>
    Private mSelectedGroup As SecurityIdentifier
    Public Property SelectedGroup() As SecurityIdentifier
        Get
            Return mSelectedGroup
        End Get
        Set(ByVal value As SecurityIdentifier)
            mSelectedGroup = value
        End Set
    End Property

    ''' <summary>
    ''' Create a new selector for finding and selecting an active directory group
    ''' </summary>
    ''' <param name="Domain">The fully qualified domain name that the group
    '''  will be selected from
    ''' </param>
    ''' <param name="PreviouslySelectedGroup">The group that was previously 
    ''' selected
    ''' </param>
    ''' <param name="SecurityGroupOnly">Whether the search will only look
    ''' for security groups
    ''' </param>
    ''' <param name="Role">
    ''' The Blue Prism role that will be associated with the selected group
    ''' </param>
    Public Sub New(Domain As String, PreviouslySelectedGroup As SecurityIdentifier, _
                   SecurityGroupOnly As Boolean, Role As String)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        mRole = Role
        mDomain = Domain
        mSecurityGroupsOnly = SecurityGroupOnly
        mSelectedGroup = PreviouslySelectedGroup


    End Sub

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)

        txtDomain.Text = mDomain

        txtLocation.GuidanceText = My.Resources.EnterTheDistinguishedNameDNToSearchFromWhenLeftBlankTheWholeDomainIsSearched

        If mRole IsNot Nothing Then
            tBar.Title = String.Format(
                My.Resources.SelectTheGroupToBeAssignedToThe0Role, LTools.GetC(mRole, "roleperms", "role"))
        End If

        cboFilterType.BindToLocalisedEnumItems(Of FilterType)(CustomResources, "FilterType_{0}")
        cboFilterType.SelectedValue = FilterType.Name

        'If there was a previously selected group, then load all of the other groups
        'in the selected group's 'Parent Folder' and then select the selected group 
        'in the grid
        If mSelectedGroup IsNot Nothing Then

            Using searcher As New ADGroupSearcher(mSecurityGroupsOnly)
                Dim ParentPath As String = searcher.GetParentPathForADGroup(
                    mDomain, mSelectedGroup)

                txtLocation.Text = ParentPath

                Dim results As PrincipalSearchResult(Of Principal) _
                    = searcher.GetAllADGroups(mDomain, ParentPath)

                DisplayGroups(results, mSelectedGroup)

            End Using


        Else
            dgvGroup.Visible = False
            EnableDisableOKButton()
        End If

    End Sub

    ''' <summary>
    ''' The group name which was selected by the user in this form, or an empty
    ''' string if no group has been selected in this form.
    ''' </summary>
    Public ReadOnly Property SelectedGroupName As String
        Get
            Return If(mSelectedGroupName, "")
        End Get
    End Property

    ''' <summary>
    ''' Sets a tooltip that gives extra information about the currently selected
    ''' filter type.
    ''' </summary>
    Private Sub SetFilterTypeToolTip()
        Dim tooltipText = ""
        Dim filterType As FilterType = cboFilterType.GetSelectedValueOrDefault(Of FilterType)
        If filterType = FilterType.Name Then
            tooltipText = String.Format(My.Resources.YouCanUseTheWildcardOperatorWhenFilteringByName0ToRepresentAValueThatCouldBeEqu, vbCrLf)
        End If

        'Disable the button if no tooltip set
        If tooltipText = "" Then
            btnFilterTypeTooltip.Image = BluePrism.Images.ToolImages.Help_16x16_Disabled
        Else
            btnFilterTypeTooltip.Image = BluePrism.Images.ToolImages.Help_16x16
        End If

        'Set the text for the tooltip
        mToolTip.SetToolTip(btnFilterTypeTooltip, tooltipText)

    End Sub

    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
        DialogResult = DialogResult.OK
        Close()
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) _
        Handles btnCancel.Click
        DialogResult = DialogResult.Cancel
        Close()
    End Sub

    Private Sub btnFind_Click(sender As Object, e As EventArgs) Handles btnFind.Click
        Try
            Me.Cursor = Cursors.WaitCursor

            'Search for groups in the selected Active Directory domain, and display 
            'the results in the grid
            Using searcher As New ADGroupSearcher(mSecurityGroupsOnly)

                Dim Filter As String = txtFilter.Text
                Dim SearchLocation As String = txtLocation.Text

                'If filter text has been entered, then search active directory for
                'groups using the filter and filter type
                If Not String.IsNullOrEmpty(Filter) Then

                    Dim filterType As FilterType = cboFilterType.GetSelectedValueOrDefault(Of FilterType)
                    Select Case filterType

                        Case FilterType.Name
                            If Filter.Contains("*") Then
                                Dim results As PrincipalSearchResult(Of Principal) = _
                                    searcher.GetADGroupByWildcardSearch(mDomain, SearchLocation, Filter)
                                DisplayGroups(results, Nothing)
                            Else
                                Dim group As GroupPrincipal = _
                                    searcher.GetADGroupByName(mDomain, SearchLocation, Filter)
                                DisplaySingleGroup(group)
                            End If

                        Case FilterType.DistinguishedName
                            Dim group As GroupPrincipal = _
                                searcher.GetADGroupByDN(mDomain, SearchLocation, Filter)
                            DisplaySingleGroup(group)

                        Case FilterType.Sid
                            Try
                                Dim GroupSid As New SecurityIdentifier(Filter)
                                Dim group As GroupPrincipal = _
                                    searcher.GetADGroupBySid(mDomain, SearchLocation, GroupSid)
                                DisplaySingleGroup(group)
                            Catch ex As Exception
                                MessageBox.Show(My.Resources.FilterTextIsNotAValidSID)
                            End Try

                        Case Else
                            'No filter type, so just search the whole domain
                            Dim results As PrincipalSearchResult(Of Principal) =
                                searcher.GetAllADGroups(mDomain, SearchLocation)
                            DisplayGroups(results, Nothing)

                    End Select

                Else
                    'Search whole domain for groups if no additional filter applied
                    Dim results As PrincipalSearchResult(Of Principal) =
                     searcher.GetAllADGroups(mDomain, SearchLocation)

                    DisplayGroups(results, Nothing)
                End If

            End Using
        Catch ex As Exception
            MessageBox.Show(ex.Message & vbCrLf & ex.InnerException.ToString, My.Resources.ErrorFindingGroups)
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    ''' <summary>
    ''' Clear grid and display the single group
    ''' </summary>
    Private Sub DisplaySingleGroup(Group As GroupPrincipal)

        RemoveHandler dgvGroup.SelectionChanged, _
            AddressOf dgvGroup_SelectionChanged

        'Clear the previous search results
        dgvGroup.Rows.Clear()

        'Add the group from the search to the data grid view
        AddRowToGrid(Group, False)

        'Before the grid is made visible, sort the rows and ensure the columns are
        'resized to fit the search results
        dgvGroup.Sort(colName, ListSortDirection.Ascending)
        dgvGroup.Visible = True

        'Set the selected group value for the form
        SetSelectedGroup()
        'Set OK button enabled only when there is a selected group
        EnableDisableOKButton()

        AddHandler dgvGroup.SelectionChanged, _
                AddressOf dgvGroup_SelectionChanged

    End Sub
    ''' <summary>
    ''' Clear grid and display the groups
    ''' </summary>
    ''' <param name="Groups">Groups to display in the grid</param>
    ''' <param name="GroupToSelect">The group to select in the grid</param>
    Private Sub DisplayGroups(Groups As PrincipalSearchResult(Of Principal), _
                              GroupToSelect As SecurityIdentifier)

        RemoveHandler dgvGroup.SelectionChanged, _
            AddressOf dgvGroup_SelectionChanged

        'Clear the previous search results
        dgvGroup.Rows.Clear()

        'Add the rows from the search to the data grid view
        For Each g As Principal In Groups
            Dim Group As GroupPrincipal = TryCast(g, GroupPrincipal)

            'If the form was opened with a previously selected value, ensure this
            'value is initially selected in the data grid view
            Dim IsSelected As Boolean = CBool(GroupToSelect IsNot Nothing _
                AndAlso Group.Sid = GroupToSelect)

            AddRowToGrid(Group, IsSelected)
        Next

        'Before the grid is made visible, sort the rows and ensure the columns are
        'resized to fit the search results
        dgvGroup.Sort(colName, System.ComponentModel.ListSortDirection.Ascending)
        dgvGroup.Visible = True

        'No specific group has been selected so select the first row in the grid
        'after sorting
        If GroupToSelect Is Nothing AndAlso dgvGroup.Rows.Count > 0 Then
            dgvGroup.SelectedRow _
                = dgvGroup.Rows(dgvGroup.FirstDisplayedCell.RowIndex)
        End If

        'Scroll to the selected row
        If dgvGroup.SelectedRow IsNot Nothing Then _
            dgvGroup.FirstDisplayedScrollingRowIndex = dgvGroup.SelectedRow.Index

        'Set the selected group value for the form
        SetSelectedGroup()
        'Set OK button enabled only when there is a selected group
        EnableDisableOKButton()

        AddHandler dgvGroup.SelectionChanged, _
            AddressOf dgvGroup_SelectionChanged

    End Sub

    ''' <summary>
    ''' Add a new row to the data grid view
    ''' </summary>
    ''' <param name="IsSelected">Set new row as the selected row</param>
    Private Sub AddRowToGrid(Group As GroupPrincipal, IsSelected As Boolean)
        If Group IsNot Nothing Then
            Dim r As New DataGridViewRow

            r.Tag = Group.Sid
            r.CreateCells(dgvGroup, Group.Name, GetGroupTypeAndScope(Group), _
                          Group.DistinguishedName)
            dgvGroup.Rows.Add(r)

            If IsSelected Then dgvGroup.SelectedRow = r

        End If
    End Sub

    ''' <summary>
    ''' Get a formatted string containing a group's type and scope e.g.
    ''' 'Security Group (Local)'
    ''' </summary>
    Private Function GetGroupTypeAndScope(Group As GroupPrincipal) As String

        Dim GroupType As String _
            = CStr(IIf(Group.IsSecurityGroup.GetValueOrDefault = True,
                       My.Resources.SecurityGroup, My.Resources.DistributionGroup))

        Dim localisedGroupScope = GroupScopeStaticConverter.LocaliseGroupScope(Group.GroupScope)

        Return $"{GroupType} ({localisedGroupScope})"
    End Function

    Private Sub dgvGroup_SelectionChanged(sender As Object, e As EventArgs)
        'Set the form's selected group to be the one selected in the grid
        SetSelectedGroup()
        EnableDisableOKButton()
    End Sub

    ''' <summary>
    ''' Sets the selected group to be the selected row in the grid
    ''' </summary>
    Private Sub SetSelectedGroup()
        ' Get the selected row of the grid view
        Dim row = dgvGroup.SelectedRow
        If row IsNot Nothing Then
            mSelectedGroup = TryCast(row.Tag, SecurityIdentifier)
            mSelectedGroupName = CStr(row.Cells(0).Value)
        Else
            mSelectedGroup = Nothing
            mSelectedGroupName = Nothing
        End If
    End Sub

    ''' <summary>
    ''' Enables OK button if there is a selected group
    ''' </summary>
    Private Sub EnableDisableOKButton()
        btnOK.Enabled = mSelectedGroup IsNot Nothing
    End Sub

    ''' <summary>
    ''' Update the filter type tooltup when the filter type changes 
    ''' </summary>
    Private Sub cboFilterType_SelectedValueChanged(sender As Object, e As EventArgs) _
        Handles cboFilterType.SelectedValueChanged
        SetFilterTypeToolTip()
    End Sub

#Region "IEnvironmentColourManager implementation"

    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property EnvironmentBackColor As Color _
     Implements IEnvironmentColourManager.EnvironmentBackColor
        Get
            Return tBar.BackColor
        End Get
        Set(value As Color)
            tBar.BackColor = value
        End Set
    End Property

    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property EnvironmentForeColor As Color _
     Implements IEnvironmentColourManager.EnvironmentForeColor
        Get
            Return tBar.TitleColor
        End Get
        Set(value As Color)
            tBar.TitleColor = value
        End Set
    End Property

#End Region

    Private Sub dgvGroup_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvGroup.CellDoubleClick
        If e.RowIndex >= 0 Then btnOK_Click(sender, e)
    End Sub

    ''' <summary>
    ''' Type of filter to apply to listing
    ''' </summary>
    Protected Enum FilterType
        ''' <summary>
        ''' Filter by name (including wildcards)
        ''' </summary>
        Name
        ''' <summary>
        ''' Filter by distinguished name
        ''' </summary>
        DistinguishedName
        ''' <summary>
        ''' Filter by SID
        ''' </summary>
        Sid
    End Enum
End Class