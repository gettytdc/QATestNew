Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.clsServerPartialClasses.DataAccess
Imports BluePrism.AutomateProcessCore
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.Skills

Partial Public Class clsServer

    <SecuredMethod(True)>
    Public Function GetSkillsWithVersionLinkedToWebApi(webApiName As String, template As String) As IEnumerable(Of String) Implements IServer.GetSkillsWithVersionLinkedToWebApi
        CheckPermissions()

        Dim skills = GetSkills()
        If skills.Count = 0 Then Return Nothing

        Dim webApiId = GetWebApiId(webApiName)
        If webApiId = Guid.Empty Then Throw New InvalidOperationException(My.Resources.clsServer_UnableToFindSpecifiedWebAPI)

        Dim skillsWithVersions As New List(Of String)

        For Each skill In skills
            For Each version In skill.Versions.OfType(Of WebSkillVersion)
                If version.WebApiId = webApiId Then
                    skillsWithVersions.Add(String.Format(template, version.Name, version.VersionNumber))
                End If
            Next
        Next

        Return skillsWithVersions
    End Function

    <SecuredMethod(True)>
    Public Function GetSkillVersionsWithWebApi(id As Guid) As IEnumerable(Of Guid) Implements IServer.GetSkillVersionsWithWebApi
        CheckPermissions()

        Using con = GetConnection()
            Dim dataAccess As New SkillDataAccess(con)
            Return dataAccess.GetSkillVersionsWithWebApi(id)
        End Using
    End Function

    <SecuredMethod(Permission.Skills.ViewSkill, Permission.Skills.ManageSkill, Permission.ProcessStudio.GroupName, Permission.ObjectStudio.GroupName)>
    Public Function GetSkill(id As Guid) As Skill Implements IServer.GetSkill
        CheckPermissions()

        Using con = GetConnection()
            Dim dataAccess As New SkillDataAccess(con)
            Return dataAccess.GetSkill(id)
        End Using
    End Function

    <SecuredMethod(True)>
    Public Function GetSkills() As IEnumerable(Of Skill) Implements IServer.GetSkills
        CheckPermissions()

        Using con = GetConnection()
            Dim dataAccess As New SkillDataAccess(con)
            Return dataAccess.GetSkills().ToList()
        End Using
    End Function

    <SecuredMethod(True)>
    Public Function GetDetailsForAllSkills() As IEnumerable(Of SkillDetails) Implements IServer.GetDetailsForAllSkills
        CheckPermissions()

        Using connection = GetConnection()
            Dim dataAccess As New SkillDataAccess(connection)
            Return dataAccess.GetDetailsForSkills()
        End Using
    End Function

    <SecuredMethod(True)>
    Public Sub InsertSkill(skill As Skill) Implements IServer.InsertSkill
        CheckPermissions()

        Using con = GetConnection()
            Dim dataAccess As New SkillDataAccess(con)
            dataAccess.Insert(skill)
        End Using
    End Sub

    <SecuredMethod(Permission.Skills.ManageSkill)>
    Public Sub InsertSkillVersion(version As WebSkillVersion, id As Guid) Implements IServer.InsertSkillVersion
        CheckPermissions()

        Using con = GetConnection()
            Dim dataAccess As New SkillDataAccess(con)
            dataAccess.InsertOrUpdateVersion(id, version, mLoggedInUser.Id)
        End Using
    End Sub

    <SecuredMethod(Permission.Skills.ManageSkill)>
    Public Sub UpdateSkillEnabled(id As Guid, enabled As Boolean) Implements IServer.UpdateSkillEnabled
        CheckPermissions()

        Using con = GetConnection()

            con.BeginTransaction()

            Dim dataAccess As New SkillDataAccess(con)
            dataAccess.UpdateEnabled(id, enabled)
            AuditRecordSkillSettingsEvent(con,
                SkillSettingsEventCode.ModifySkillEnabledSetting,
                    String.Format(My.Resources.clsServer_SkillEnabledSettingChangedTo0ForSkill1,
                        If(enabled, My.Resources.AutomateAppCore_True, My.Resources.AutomateAppCore_False),
                        id))

            con.CommitTransaction()
        End Using
    End Sub

    <SecuredMethod(Permission.Skills.ManageSkill)>
    Public Sub DeleteSkill(id As Guid, name As String) Implements IServer.DeleteSkill
        CheckPermissions()
        Using con = GetConnection()
            Dim webDataAccess = New WebApiDataAccess(con)
            Dim skillDataAccess = New SkillDataAccess(con)
            Dim webApiNames = New List(Of String)
            Dim webApiIds = skillDataAccess.GetWebApisForSkill(id)

            con.BeginTransaction()
            skillDataAccess.DeleteSkill(id)

            ' Delete any associated Web APIs if they are not
            ' also associated with other skills
            For Each id In webApiIds
                Dim webApi = webDataAccess.GetWebApi(id)
                If skillDataAccess.GetSkillNamesForWebApi(id).Count = 0 Then
                    webApiNames.Add(webApi.Name)
                    webDataAccess.Delete(id)
                End If
            Next

            AuditRecordSkillSettingsEvent(con, SkillSettingsEventCode.DeleteSkill,
                String.Format(My.Resources.clsServer_SkillName0AssociatedWebAPIs1, name, CollectionUtil.Join(webApiNames, ",")))

            con.CommitTransaction()
        End Using
    End Sub
End Class

