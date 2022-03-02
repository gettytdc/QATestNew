Imports LocaleTools
Imports BluePrism.AutomateAppCore.Auth
Public Class RefreshADUserListMessageBuilder

    Public Shared Function Build(adUserInfo As RefreshADUserList) As String
        If adUserInfo IsNot Nothing Then

            If adUserInfo.RolesNotMapped.Any Then

                Dim rolesNotMappedTranslated As New List(Of String)
                For Each role As String In adUserInfo.RolesNotMapped
                    rolesNotMappedTranslated.Add(LTools.GetC(role, "roleperms", "role"))
                Next

                adUserInfo.GroupErrors &= String.Format(My.Resources.clsServer_OneOrMoreBluePrismRolesHaveNotBeenMappedToAnActiveDirectorySecurityGroup, String.Join(", ", rolesNotMappedTranslated)) & vbCrLf
            End If

            Dim sb As New StringBuilder
            sb.AppendLine(My.Resources.clsServer_RefreshUserListCompletedSuccessfully)
            sb.AppendLine(My.Resources.clsServer_TheMembersOfTheActiveDirectorySecurityGroupsAssociatedWithBluePrismSecurityRole)
            sb.AppendLine(String.Format(My.Resources.clsServer_0UsersHaveBeenAdded, adUserInfo.AddedUsers.Count))
            sb.AppendLine(String.Format(My.Resources.clsServer_0UsersHaveBeenMarkedAsActive, adUserInfo.ActivatedUsers.Count))
            sb.AppendLine(String.Format(My.Resources.clsServer_0UsersHaveBeenMarkedAsInactive, adUserInfo.DeactivatedUsers.Count))

            If adUserInfo.AddedUsers.Count > 0 Then
                sb.AppendLine().AppendLine(My.Resources.clsServer_UsersAdded)
                For Each u As User In adUserInfo.AddedUsers
                    sb.AppendLine(u.Name)
                Next
            End If

            If adUserInfo.ActivatedUsers.Count > 0 Then
                sb.AppendLine().AppendLine(My.Resources.clsServer_UsersMarkedAsActive)
                For Each u As User In adUserInfo.ActivatedUsers
                    sb.AppendLine(u.Name)
                Next
            End If

            If adUserInfo.DeactivatedUsers.Count > 0 Then
                sb.AppendLine().AppendLine(My.Resources.clsServer_UsersMarkedAsInactive)
                For Each u As User In adUserInfo.DeactivatedUsers
                    sb.AppendLine(u.Name)
                Next
            End If

            If adUserInfo.GroupErrors.Length <> 0 Then
                sb.AppendLine()
                sb.AppendLine(String.Format(My.Resources.clsServer_WarningTheFollowingConfigurationErrorsWereDetected0, adUserInfo.GroupErrors))
            End If

            Return sb.ToString()

        End If
        Return Nothing
    End Function

End Class
