Imports BluePrism.AutomateAppCore.Auth
Imports LocaleTools
Imports System.IO

''' <summary>
''' Generates user role reports.
''' </summary>
Public Class clsRoleReporter
    Inherits clsReporter

#Region " Private Members "
    ' The report name displayed in the UI
    Private mName As String

    ' The report description displayed in the UI
    Private mDescription As String
#End Region

    Public Sub New()
        mName = My.Resources.clsRoleReporter_RolesName
        mDescription = My.Resources.clsRoleReporter_AllRolesPermissionsAssignmentsAndUsersInTheSystem
    End Sub

    Public Overrides ReadOnly Property Name() As String
        Get
            Return mName
        End Get
    End Property

    Public Overrides ReadOnly Property Description() As String
        Get
            Return mDescription
        End Get
    End Property

    Public Overrides ReadOnly Property OutputFormat() As OutputFormats
        Get
            Return OutputFormats.WikiText
        End Get
    End Property

    Public Overrides Function HasPermission() As Boolean
        Return User.Current.HasPermission("Security - Users")
    End Function

    Public Overrides Function GetArguments() As List(Of ArgumentInfo)
        Return New List(Of ArgumentInfo)
    End Function

    'AD group membership information
    Private mADGroupMembership As IList(Of clsActiveDirectory.SecurityGroup)

    Protected Overrides Sub GenerateReport(ByVal args As List(Of Object), ByVal sw As StreamWriter)
        MyBase.GenerateReport(args, sw)

        Dim databaseType = gSv.DatabaseType()
        If databaseType = DatabaseType.SingleSignOn Then mADGroupMembership = clsActiveDirectory.GetRoleMappedGroupMembership(True, Nothing, Nothing)

        sw.WriteLine(My.Resources.clsRoleReporter_Roles)
        sw.WriteLine(My.Resources.clsRoleReporter_EachRoleDefinedIsDetailedInTheFollowingSections)
        For Each r As Role In SystemRoleSet.Current
            sw.Write("==")
            sw.Write(LTools.GetC(r.Name, "roleperms", "role"))
            sw.WriteLine("==")
            If databaseType = DatabaseType.SingleSignOn AndAlso r.ActiveDirectoryGroup <> "" Then
                sw.Write(String.Format(My.Resources.clsRoleReporter_ThisRoleMapsTo0InActiveDirectory, GetADGroupName(r.ActiveDirectoryGroup)))
                sw.WriteLine(My.Resources.clsRoleReporter_ItGrantsTheFollowingPermissions)
            Else
                sw.WriteLine(My.Resources.clsRoleReporter_ThisRoleGrantsTheFollowingPermissions)
            End If
            For Each perm As Permission In r
                sw.Write("*")
                sw.WriteLine(LTools.GetC(perm.Name, "roleperms", "perm"))
            Next
        Next

        sw.WriteLine(My.Resources.clsRoleReporter_Permissions)
        sw.WriteLine(My.Resources.clsRoleReporter_TheFollowingIsACompleteListOfAvailablePermissionsOrganisedByGroup)
        sw.WriteLine(My.Resources.clsRoleReporter_NoteThatItIsPossibleForAPermissionToAppearInMoreThanOneGroup)
        For Each pg As PermissionGroup In PermissionGroup.All
            sw.Write("==")
            sw.Write(LTools.GetC(pg.Name, "roleperms", "group"))
            sw.WriteLine("==")
            For Each p As Permission In pg
                sw.Write("*")
                sw.WriteLine(LTools.GetC(p.Name, "roleperms", "perm"))
            Next
        Next

        sw.WriteLine(My.Resources.clsRoleReporter_Users)
        sw.WriteLine(My.Resources.clsRoleReporter_TheFollowingUsersAreDefined)
        For Each u As User In gSv.GetAllUsers()
            If u.IsHidden OrElse u.Deleted Then Continue For
            If databaseType = DatabaseType.SingleSignOn Then LoadADRolesInto(u)

            sw.Write("==")
            sw.Write(u.Name)
            sw.WriteLine("==")
            sw.WriteLine(My.Resources.clsRoleReporter_TheUserHasTheFollowingRoles)
            For Each r As Role In u.Roles
                sw.Write("*"c)
                sw.WriteLine(LTools.GetC(r.Name, "roleperms", "role"))
            Next
            sw.WriteLine(My.Resources.clsRoleReporter_TheRolesAssignedToTheUserProvideTheFollowingEffectivePermissions)
            Dim perms As ICollection(Of Permission) = u.Roles.EffectivePermissions
            For Each pg As PermissionGroup In PermissionGroup.All
                If pg.ContainsAny(perms) Then
                    sw.Write("===")
                    sw.Write(LTools.GetC(pg.Name, "roleperms", "group"))
                    sw.WriteLine("===")
                End If
                For Each p As Permission In pg
                    If perms.Contains(p) Then
                        sw.Write("*")
                        sw.WriteLine(LTools.GetC(p.Name, "roleperms", "perm"))
                    End If
                Next
            Next
        Next

    End Sub

    ''' <summary>
    ''' Returns the name of the passed AD group (which could be a SID). If a SID is
    ''' passed but could not be resolved to a group name then a warning message is
    ''' returned.
    ''' </summary>
    ''' <param name="nameOrSid">The AD group name or SID</param>
    ''' <returns>The AD group name</returns>
    Private Function GetADGroupName(nameOrSid As String) As String
        For Each gp In mADGroupMembership
            If gp.MatchesNameOrSid(nameOrSid) Then Return gp.Name
        Next
        If Not clsActiveDirectory.IsSid(nameOrSid) Then Return nameOrSid
        Return String.Format(My.Resources.clsRoleReporter_WarningGroupNotFoundWithSID0, nameOrSid)
    End Function

    ''' <summary>
    ''' Loads roles for an AD user, using the group membership information
    ''' </summary>
    ''' <param name="u">Reference to the user</param>
    Private Sub LoadADRolesInto(u As User)
        u.Roles.Clear()
        For Each r In SystemRoleSet.Current
            If String.IsNullOrEmpty(r.ActiveDirectoryGroup) Then Continue For
            For Each gp In mADGroupMembership
                If gp.MatchesNameOrSid(r.ActiveDirectoryGroup) AndAlso
                    (From m In gp.Members
                     Where m.UserPrincipalName = u.Name
                     Select m).Count > 0 Then
                    u.Roles.Add(r)
                End If
            Next
        Next
    End Sub

End Class
