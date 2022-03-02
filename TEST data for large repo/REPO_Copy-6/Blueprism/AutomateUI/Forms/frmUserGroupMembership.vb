Imports AutomateControls
Imports BluePrism.AutomateAppCore
Imports System.Security.Principal
Imports BluePrism.AutomateAppCore.Auth
Imports LocaleTools

''' <summary>
''' Form that shows the Blue Prism Role and AD Security Group membership for a
''' specified user.
''' </summary>
Friend Class frmUserGroupMembership
    Implements IEnvironmentColourManager, IHelp

    ''' <summary>
    ''' UserID of the user to display group membership of
    ''' </summary>
    Private mUserID As Guid

    ''' <summary>
    ''' UPN of the user to display group membership of
    ''' </summary>
    Private mUserName As String

    ''' <summary>
    ''' AD Security Groups the user is a member of (includes nested groups)
    ''' </summary>
    Private mSecurityGroups As IList(Of clsActiveDirectory.SecurityGroup)

    ''' <summary>
    ''' Create an instance of the user group membership form
    ''' </summary>
    ''' <param name="userID">UserID of the user to display group membership of</param>
    ''' <param name="userName">UPN of the user to display group membership of</param>
    Public Sub New(userID As Guid, userName As String)

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        mUserID = userID
        mUserName = userName
        Dim context As String = Nothing
        Dim domain As String = Nothing
        mSecurityGroups = clsActiveDirectory.GetRoleMappedGroupMembership(True, context, domain)
        SetDomainContextLabel(context)
        lblDomainGroups.Text = String.Format(
            My.Resources.SecurityGroupsInThe0DomainOfWhichTheUserIsAMember, domain)

        titleBar.Title = String.Format(My.Resources.GroupMembershipFor0, userName)

    End Sub

    ''' <summary>
    ''' Display data in the grids and display the domain context on the form load
    ''' </summary>
    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)

        DisplayRoles()

    End Sub

    ''' <summary>
    ''' Load and display Blue Prism Roles in the grid, and whether the user is a
    ''' member of the associated security group.
    ''' </summary>
    Private Sub DisplayRoles()

        'Clear any rows that may have been added
        dgvRoles.Rows.Clear()

        'Loop through each role, find out the associated AD Group Name and whether 
        'the user is a member of that security group, and add the role to the grid
        For Each role As Role In SystemRoleSet.Current

            Dim sid As SecurityIdentifier
            Dim isMemberOf As String = My.Resources.NA
            Dim ssoGroupName As String = ""

            If role.ActiveDirectoryGroup <> "" Then
                sid = clsActiveDirectory.TryParseSID(role.ActiveDirectoryGroup)
                Dim gp = (From g In mSecurityGroups
                          Where (sid IsNot Nothing AndAlso g.Sid = sid) OrElse g.Name = role.ActiveDirectoryGroup
                          Select g).FirstOrDefault
                ssoGroupName = gp.Name
                If gp.ConfigException IsNot Nothing Then
                    isMemberOf = My.Resources.Unknown
                Else
                    isMemberOf = If((From m In gp.Members
                                     Where m.UserPrincipalName = mUserName
                                     Select m).Count > 0, My.Resources.Yes, My.Resources.No)
                End If
            End If

            'Create the security group row and add to the grid
            Dim r As New DataGridViewRow
            r.CreateCells(dgvRoles,  LTools.GetC(role.Name, "roleperms", "role"), ssoGroupName, isMemberOf)
            If ssoGroupName = "" Then
                'Set row to be grey if no AD Security group associated with the role
                Dim style As DataGridViewCellStyle = r.DefaultCellStyle
                style.ForeColor = Color.Gray
                r.DefaultCellStyle = style
            End If
            dgvRoles.Rows.Add(r)

        Next


        'Sort the grid
        dgvRoles.Sort(colRole, ListSortDirection.Ascending)

        'Set no selected row by default, as the grid is used to display data
        dgvRoles.SelectedRow = Nothing

    End Sub

    ''' <summary>
    ''' Display the security groups the user is a member of in the grid
    ''' </summary>
    Private Sub DisplaySecurityGroups()

        'Clear any rows that may have been added
        dgvSecurityGroups.Rows.Clear()

        'Add the results of the AD search to the grid
        Dim skipped = ""
        For Each g As clsActiveDirectory.SecurityGroup In clsActiveDirectory.GetDomainGroupsForUser(mUserName, skipped)
            Dim r As New DataGridViewRow
            r.CreateCells(dgvSecurityGroups, g.Name, g.GroupType, g.Path)
            dgvSecurityGroups.Rows.Add(r)
        Next

        'Sort the grid
        dgvSecurityGroups.Sort(colName, ListSortDirection.Ascending)

        'Set no selected row by default, as the grid is used to display data
        dgvSecurityGroups.SelectedRow = Nothing

        If Not String.IsNullOrEmpty(skipped) Then
            UserMessage.Show(String.Format(
             My.Resources.TheFollowingSecurityGroupsCouldNotBeInspectedBecauseOneOrMoreOfTheirMembersIsAF, vbCrLf, skipped))
        End If
    End Sub

    ''' <summary>
    ''' Set a label describing the context used to query directory services
    ''' </summary>
    Private Sub SetDomainContextLabel(DomainContext As String)
        lblDomainContext.Text =
            String.Format(My.Resources.QueryPerformedOn0, DomainContext)

    End Sub

    ''' <summary>
    ''' Handles the query groups link being clicked
    ''' </summary>
    Private Sub QueryGroups(sender As Object, e As EventArgs) Handles llQueryGroups.Click
        dgvSecurityGroups.Enabled = True
        DisplaySecurityGroups()
        llQueryGroups.Enabled = False
    End Sub

#Region "IEnvironmentColourManager implementation"

    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property EnvironmentBackColor As Color _
     Implements IEnvironmentColourManager.EnvironmentBackColor
        Get
            Return titleBar.BackColor
        End Get
        Set(value As Color)
            titleBar.BackColor = value
        End Set
    End Property

    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property EnvironmentForeColor As Color _
     Implements IEnvironmentColourManager.EnvironmentForeColor
        Get
            Return titleBar.TitleColor
        End Get
        Set(value As Color)
            titleBar.TitleColor = value
        End Set
    End Property

#End Region


    Public Function IHelp_GetHelpFile() As String Implements IHelp.GetHelpFile
        Return "helpSingleSignonGroupsList.htm "
    End Function
End Class
