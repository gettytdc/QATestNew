Imports System.Data.SqlClient
Imports System.IO
Imports BluePrism.AutomateProcessCore
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.Core.Encryption
Imports BluePrism.Data
Imports BluePrism.Server.Domain.Models
Imports BluePrism.Skills

Namespace clsServerPartialClasses.DataAccess
    Friend Class SkillDataAccess

        Private ReadOnly mConnection As IDatabaseConnection

        Public Sub New(connection As IDatabaseConnection)
            mConnection = connection
        End Sub

        Friend Sub UpdateEnabled(skillId As Guid, enabled As Boolean)
            Dim command As New SqlCommand("
                            update 
                                BPASkill
                            set 
                                isenabled=@enabled
                            where 
                                id=@id")

            command.Parameters.AddWithValue("@id", skillId)
            command.Parameters.AddWithValue("@enabled", enabled)

            mConnection.ExecuteReturnScalar(command)
        End Sub

        Friend Function CheckExists(skillId As Guid) As Boolean
            Dim command = New SqlCommand("select 1 from BPASkill where id=@id")
            command.Parameters.AddWithValue("@id", skillId)

            Return clsServer.IfNull(mConnection.ExecuteReturnScalar(command), False)
        End Function

        Friend Function GetSkillVersionId(skillId As Guid, version As WebSkillVersion) As Guid
            Dim command = New SqlCommand("select id from BPASkillVersion where skillid=@skillid and versionnumber=@versionnumber")
            command.Parameters.AddWithValue("@skillid", skillId)
            command.Parameters.AddWithValue("@versionnumber", version.VersionNumber)
            Return clsServer.IfNull(mConnection.ExecuteReturnScalar(command), Guid.Empty)
        End Function

        Friend Sub Insert(skill As Skill)
            Dim command As New SqlCommand("
                                insert into
                                    BPASkill (id, provider, isenabled)
                                values
                                    (@id, @provider, @enabled)")

            command.Parameters.AddWithValue("@id", skill.Id)
            command.Parameters.AddWithValue("@provider", DatabaseDataEncryption.EncryptWithRowIdentifier(skill.Id.ToString(), skill.Provider))
            command.Parameters.AddWithValue("@enabled", skill.Enabled)

            mConnection.ExecuteReturnScalar(command)
        End Sub

        Friend Sub InsertOrUpdateVersion(skillId As Guid, version As WebSkillVersion, userId As Guid)

            Dim existingVersionId = GetSkillVersionId(skillId, version)

            If existingVersionId <> Guid.Empty Then
                UpdateVersion(skillId, existingVersionId, version, userId)
            Else
                InsertVersion(skillId, version, userId)
            End If
        End Sub

        Public Function GetSkill(id As Guid) As Skill
            Dim command As New SqlCommand("
                            select 
                                [provider],
                                isenabled
                            from BPASkill
                                where id = @id")

            command.Parameters.AddWithValue("@id", id)

            Dim skillProvider As String
            Dim enabled As Boolean

            Using reader = mConnection.ExecuteReturnDataReader(command)
                Dim provider As New ReaderDataProvider(reader)
                If Not reader.Read() Then _
                    Throw New NoSuchElementException(My.Resources.NoSkillFoundWithId0, id)

                skillProvider = DatabaseDataEncryption.DecryptAndVerifRowIdentifier(id.ToString(), provider.GetString("provider"))
                enabled = provider.GetBool("isenabled")
            End Using

            Dim versions = GetVersions(id).ToList()
            Return New Skill(id, skillProvider, enabled, versions)
        End Function

        Public Function GetSkills() As IEnumerable(Of Skill)
            Dim command As New SqlCommand("
                            select
                                id,
                                [provider] as provider,
                                isenabled
                            from BPASkill")

            Dim skillDataList As New List(Of (id As Guid,
                                        provider As String,
                                        enabled As Boolean))

            Using reader = mConnection.ExecuteReturnDataReader(command)
                While reader.Read()
                    Dim provider As New ReaderDataProvider(reader)
                    Dim skillId = provider.GetGuid("id")
                    skillDataList.Add((
                        skillId,
                        DatabaseDataEncryption.DecryptAndVerifRowIdentifier(skillId.ToString(), provider.GetString("provider")),
                        provider.GetBool("isenabled")))
                End While
            End Using

            Return skillDataList.Select(Function(s)
                                            Return New Skill(
                                                        s.id,
                                                        s.provider,
                                                        s.enabled,
                                                        GetVersions(s.id).ToList())
                                        End Function).ToList()
        End Function

        Private Function GetVersions(skillId As Guid) As IEnumerable(Of SkillVersion)
            Dim command As New SqlCommand("
                            select
                                id,
                                [name] as name,
                                versionnumber,
                                [description] as description,
                                category,
                                icon,
                                bpversioncreated,
                                bpversiontested,
                                importedat
                            from 
                                BPASkillVersion
                            where 
                                skillid=@skillid")

            command.Parameters.AddWithValue("@skillid", skillId)

            Dim versionDataList As New List(Of (id As Guid,
                                        name As String,
                                        category As SkillCategory,
                                        versionNumber As String,
                                        description As String,
                                        icon As Byte(),
                                        bpVersionCreated As String,
                                        bpVersionTested As String,
                                        importedAt As DateTime))

            Using reader = mConnection.ExecuteReturnDataReader(command)
                While reader.Read()
                    Dim prov As New ReaderDataProvider(reader)

                    Dim versionId = prov.GetGuid("id")

                    Dim iconString = DatabaseDataEncryption.DecryptAndVerifRowIdentifier(versionId.ToString(), prov.GetValue(Of String)("icon", Nothing))
                    Dim iconBytes = Convert.FromBase64String(iconString)



                    versionDataList.Add((
                            versionId,
                            DatabaseDataEncryption.DecryptAndVerifRowIdentifier(versionId.ToString(), prov.GetString("name")),
                            CType(CInt(DatabaseDataEncryption.DecryptAndVerifRowIdentifier(versionId.ToString(), prov.GetString("category"))), SkillCategory),
                            prov.GetString("versionnumber"),
                            DatabaseDataEncryption.DecryptAndVerifRowIdentifier(versionId.ToString(), prov.GetString("description")),
                            iconBytes,
                            prov.GetString("bpversioncreated"),
                            prov.GetString("bpversiontested"),
                            prov.GetValue("importedat", DateTime.MinValue)))



                End While
            End Using

            Dim webApiDataAccess As New WebApiDataAccess(mConnection)

            Return versionDataList.Select(Function(v)
                                              Dim webapi = webApiDataAccess.GetWebApi(GetSkillVersionWebApiId(v.id))
                                              Return New WebSkillVersion(
                                                 webapi.Id,
                                                 webapi.Name,
                                                 webapi.Enabled,
                                                 v.name,
                                                 v.category,
                                                 v.versionNumber,
                                                 v.description,
                                                 v.icon,
                                                 v.bpVersionCreated,
                                                 v.bpVersionTested,
                                                 v.importedAt,
                                                 webapi.Configuration.Actions.Select(Function(x) x.Name).ToList())
                                          End Function).ToList()

        End Function

        Private Function GetSkillVersionWebApiId(id As Guid) As Guid
            Dim command As New SqlCommand("
                            select
                                webserviceid
                            from 
                                BPAWebSkillVersion
                            where 
                                versionid=@id")

            command.Parameters.AddWithValue("@id", id)
            Return CType(mConnection.ExecuteReturnScalar(command), Guid)
        End Function

        Private Sub InsertVersion(skillId As Guid, version As WebSkillVersion, userId As Guid)
            Dim command As New SqlCommand("
                                insert into BPASkillVersion
                                (id, skillid, name, versionnumber, description, category, icon, bpversioncreated, bpversiontested, importedat, importedby)
                                output inserted.id
                                values (@id, @skillid, @name, @versionnumber, @description, @category, @icon, @bpversioncreated, @bpversiontested, getutcdate(), @importedby)")

            Using stream = New MemoryStream()

                Dim id = Guid.NewGuid()
                Dim iconString = Convert.ToBase64String(version.Icon)

                command.Parameters.AddWithValue("@id", id)
                command.Parameters.AddWithValue("@skillid", skillId)
                command.Parameters.AddWithValue("@name", DatabaseDataEncryption.EncryptWithRowIdentifier(id.ToString(), version.Name))
                command.Parameters.AddWithValue("@versionnumber", version.VersionNumber)
                command.Parameters.AddWithValue("@description", DatabaseDataEncryption.EncryptWithRowIdentifier(id.ToString(), version.Description))
                command.Parameters.AddWithValue("@category", DatabaseDataEncryption.EncryptWithRowIdentifier(id.ToString(), CInt(version.Category).ToString()))
                command.Parameters.AddWithValue("@icon", DatabaseDataEncryption.EncryptWithRowIdentifier(id.ToString(), iconString))
                command.Parameters.AddWithValue("@bpversioncreated", version.BpVersionCreatedIn)
                command.Parameters.AddWithValue("@bpversiontested", version.BpVersionTestedIn)
                command.Parameters.AddWithValue("@importedby", userId)
            End Using

            Dim versionId = CType(mConnection.ExecuteReturnScalar(command), Guid)
            If versionId = Guid.Empty Then Exit Sub

            Dim webCommand As New SqlCommand("
                            insert 
                                into BPAWebSkillVersion  (versionid, webserviceid)
                            values 
                                (@id, 
                                @webserviceid)")
            webCommand.Parameters.AddWithValue("@webserviceid", version.WebApiId)

            webCommand.Parameters.AddWithValue("@id", versionId)
            mConnection.ExecuteReturnScalar(webCommand)
        End Sub

        Private Sub UpdateVersion(skillId As Guid, versionId As Guid, version As WebSkillVersion, userId As Guid)

            Dim command As New SqlCommand("
                                update BPASkillVersion
                                set name=@name, description=@description, category=@category, icon=@icon, bpversioncreated=@bpversioncreated, bpversiontested=@bpversiontested, importedat=getutcdate(), importedby=@importedby
                                where skillid=@skillid and versionnumber=@versionnumber")

            Using stream = New MemoryStream()


                Dim iconString = Convert.ToBase64String(version.Icon)


                command.Parameters.AddWithValue("@skillid", skillId)
                command.Parameters.AddWithValue("@name", DatabaseDataEncryption.EncryptWithRowIdentifier(versionId.ToString(), version.Name))
                command.Parameters.AddWithValue("@versionnumber", version.VersionNumber)
                command.Parameters.AddWithValue("@description", DatabaseDataEncryption.EncryptWithRowIdentifier(versionId.ToString(), version.Description))
                command.Parameters.AddWithValue("@category", DatabaseDataEncryption.EncryptWithRowIdentifier(versionId.ToString(), CInt(version.Category).ToString()))
                command.Parameters.AddWithValue("@icon", DatabaseDataEncryption.EncryptWithRowIdentifier(versionId.ToString(), iconString))
                command.Parameters.AddWithValue("@bpversioncreated", version.BpVersionCreatedIn)
                command.Parameters.AddWithValue("@bpversiontested", version.BpVersionTestedIn)
                command.Parameters.AddWithValue("@importedby", userId)
            End Using

            mConnection.Execute(command)

            Dim webCommand As New SqlCommand("
                            update BPAWebSkillVersion
                            set webserviceid = @webserviceid where versionid = @id")
            webCommand.Parameters.AddWithValue("@webserviceid", version.WebApiId)
            webCommand.Parameters.AddWithValue("@id", versionId)
            mConnection.Execute(webCommand)
        End Sub

        Public Function GetSkillNamesForWebApi(serviceId As Guid) As IList(Of String)
            Dim skillNames As New List(Of String)
            Dim command = New SqlCommand("
                            select v.id as versionid, v.[name] as name from BPASkillVersion v
                                inner join BPAWebSkillVersion w on v.id = w.versionid
                                inner join (select skillid, MAX(importedat) importedat 
                                            from BPASkillVersion 
                                            group by skillid) b
                                on b.skillid = v.skillid AND v.importedat = b.importedat
                                where w.webserviceid = @id")

            command.Parameters.AddWithValue("@id", serviceId)
            Using reader = mConnection.ExecuteReturnDataReader(command)
                While reader.Read()
                    Dim provider As New ReaderDataProvider(reader)
                    Dim skillName = DatabaseDataEncryption.DecryptAndVerifRowIdentifier(
                                   provider.GetString("versionid"), provider.GetString("name"))
                    If Not skillNames.Contains(skillName) Then skillNames.Add(skillName)
                End While
            End Using

            Return skillNames
        End Function

        Public Function GetSkillVersionsWithWebApi(id As Guid) As IEnumerable(Of Guid)
            Dim command As New SqlCommand("
                            select
                                versionid
                            from 
                                BPAWebSkillVersion
                            where 
                                webserviceid=@id")

            command.Parameters.AddWithValue("@id", id)
            Dim versions = New List(Of Guid)

            Using reader = mConnection.ExecuteReturnDataReader(command)
                While reader.Read()
                    Dim provider As New ReaderDataProvider(reader)
                    versions.Add(provider.GetGuid("versionid"))
                End While
            End Using
            Return versions
        End Function

        Public Function GetDetailsForSkills() As IEnumerable(Of SkillDetails)
            Dim query = New StringBuilder()
            Dim results = New List(Of SkillDetails)

            query.Append("SELECT sv.skillid, MAX(versionnumber) as latestversion ")
            query.Append("INTO #TempSkillLatestVersion ")
            query.Append("FROM BPASkill s ")
            query.Append("INNER JOIN BPASkillVersion sv ON s.id = sv.skillid ")
            query.Append("GROUP BY skillid; ")

            query.Append("SELECT DISTINCT s.id, was.[name] as webapiname, sv.id as decryptionkey, sv.[name] as skillversionname, sv.icon, sv.category ")
            query.Append("FROM BPASkill s ")
            query.Append("INNER JOIN BPASkillVersion sv ON s.id = sv.skillid ")
            query.Append("INNER JOIN BPAWebSkillVersion wsv ON sv.id = wsv.versionid ")
            query.Append("INNER JOIN BPAWebApiService was ON wsv.webserviceid = was.serviceid ")
            query.Append("INNER JOIN #TempSkillLatestVersion slv ON sv.versionnumber = slv.latestversion ")
            query.Append("WHERE slv.skillid = s.id AND s.isenabled = 1;")

            Using command = New SqlCommand(query.ToString())
                Using reader = mConnection.ExecuteReturnDataReader(command)
                    While reader.Read()
                        Dim provider As New ReaderDataProvider(reader)

                        Dim skillId = provider.GetValue("id", Guid.Empty).ToString()
                        Dim webApiName = provider.GetString("webapiname")
                        Dim decryptionKey = provider.GetValue("decryptionkey", Guid.Empty).ToString()
                        Dim encryptedSkillName = provider.GetString("skillversionname")
                        Dim encryptedSkillIcon = provider.GetString("icon")
                        Dim encryptedSkillCategory = provider.GetString("category")

                        Dim decryptedSkillName = DatabaseDataEncryption.DecryptAndVerifRowIdentifier(decryptionKey, encryptedSkillName)
                        Dim decryptedSkillIcon = DatabaseDataEncryption.DecryptAndVerifRowIdentifier(decryptionKey, encryptedSkillIcon)
                        Dim decryptedSkillCategory = DatabaseDataEncryption.DecryptAndVerifRowIdentifier(decryptionKey, encryptedSkillCategory)

                        results.Add(New SkillDetails(skillId, webApiName, decryptedSkillName, decryptedSkillIcon, decryptedSkillCategory))
                    End While
                End Using
            End Using

            Return results
        End Function

        Public Function GetWebApisForSkill(skillId As Guid) As IList(Of Guid)
            Dim sql = "select distinct wsv.webserviceid
                                            from BPASkillVersion sv
                                                inner join BPAWebSkillVersion wsv on wsv.versionid=sv.id
                                            where sv.skillid=@skillId"

            Dim webApiIds = New List(Of Guid)
            Using command = New SqlCommand(sql)
                command.Parameters.AddWithValue("@skillId", skillId)
                Using reader = mConnection.ExecuteReturnDataReader(command)
                    Dim provider As New ReaderDataProvider(reader)
                    While reader.Read()
                        webApiIds.Add(provider.GetGuid("webserviceid"))
                    End While
                End Using
            End Using

            Return webApiIds
        End Function

        Public Sub DeleteSkill(skillId As Guid)
            Dim sql = "delete from BPASkill where id=@skillId"

            Using command = New SqlCommand(sql)
                command.Parameters.AddWithValue("@skillId", skillId)
                mConnection.Execute(command)
            End Using
        End Sub
    End Class
End Namespace
