Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.Data

Namespace clsServerPartialClasses.Caching

    Public Class CacheDataProvider : Implements ICacheDataProvider
        Private ReadOnly mDatabaseCommandFactory As Func(Of String, IDbCommand)

        Public Sub New(
                      databaseCommandFactory As Func(Of String, IDbCommand))
            mDatabaseCommandFactory = databaseCommandFactory
        End Sub

        Public Function GetAllGroupPermissions(connection As IDatabaseConnection) _
            As IReadOnlyDictionary(Of String, IGroupPermissions) _
            Implements ICacheDataProvider.GetAllGroupPermissions

            Dim results As New Dictionary(Of String, IGroupPermissions)

            Using command = mDatabaseCommandFactory(
                " select g.Id, g.isrestricted, gurp.userroleid, gurp.permid, r.name as name " &
                " from BPAGroup g " &
                " left join BPAGroupUserRolePerm gurp on gurp.Groupid = g.id " &
                " left join BPAUserRole r on r.id = gurp.userroleid " &
                " where isrestricted = 1 " &
                " order by g.id ")

                Using reader = connection.ExecuteReturnDataReader(command)
                    While reader.Read()

                        Dim id = reader.Get(Of Guid)("id")
                        Dim idString As String = id.ToString()

                        If Not results.ContainsKey(idString) Then
                            results.Add(idString, New GroupPermissions(id, PermissionState.Restricted))
                        End If

                        Dim groupPermissions As IGroupPermissions = results(idString)

                        If Not reader.IsDBNull(2) Then
                            Dim roleId = reader.Get(Of Integer)("userroleid")

                            Dim role = groupPermissions.FirstOrDefault(Function(g) g.Id = roleId)

                            If role Is Nothing Then
                                Dim roleName = reader.Get(Of String)("name")
                                role = New GroupLevelPermissions(roleId, roleName)
                                groupPermissions.Add(role)
                            End If

                            If Not reader.IsDBNull(3) Then
                                Dim permissionId = reader.Get(Of Integer)("permid")
                                role.Add(Permission.GetPermission(permissionId))
                            End If
                        End If
                    End While
                End Using
            End Using

            Return results
        End Function

        Private Const GetProcessGroupsSql = " select id, groupId from BPVGroupedProcessesObjects where groupId is not Null "
        Private Const GetResourceGroupsSql = " select id, groupId from bpvgroupedresources "

        Public Function GetProcessGroups(connection As IDatabaseConnection) As IReadOnlyDictionary(Of Guid, List(Of Guid)) _
            Implements ICacheDataProvider.GetProcessGroups
            Return GetGroupedData(connection, GetProcessGroupsSql)
        End Function
        Public Function GetResourceGroups(connection As IDatabaseConnection) As IReadOnlyDictionary(Of Guid, List(Of Guid)) _
            Implements ICacheDataProvider.GetResourceGroups
            Return GetGroupedData(connection, GetResourceGroupsSql)
        End Function

        Public Function IsMIReportingEnabled(connection As IDatabaseConnection) As Boolean _
            Implements ICacheDataProvider.IsMIReportingEnabled
            Using command = mDatabaseCommandFactory("SELECT mienabled FROM BPAMIControl WHERE id=1;")
                Return CBool(connection.ExecuteReturnScalar(command))
            End Using
        End Function

        Private Function GetGroupedData(connection As IDatabaseConnection, query As String) _
            As IReadOnlyDictionary(Of Guid, List(Of Guid))
            Dim results As New Dictionary(Of Guid, List(Of Guid))
            Using command = mDatabaseCommandFactory(query)
                Using reader = connection.ExecuteReturnDataReader(command)
                    While reader.Read()
                        Dim id As Guid = reader.Get(Of Guid)("id")
                        If Not results.ContainsKey(id) Then
                            results.Add(id, New List(Of Guid))
                        End If

                        ' objects may not belong to a group, but this is good to cache too.
                        Dim groupId As Guid = reader.Get(Of Guid)("groupId")
                        If groupId <> Guid.Empty Then
                            results(id).Add(groupId)
                        End If

                    End While
                End Using
            End Using
            Return results
        End Function

    End Class

End Namespace