Imports System.Data.SqlClient
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.Data

Partial Public Class clsServer

    Private Function GetGroupIdParameterTable(params As IEnumerable(Of Guid)) As DataTable
        Dim table = New DataTable()
        table.Columns.Add("GroupId", GetType(Guid))

        For Each g In params
            Dim r = table.NewRow()
            r("GroupId") = g
            table.Rows.Add(r)
        Next
        Return table
    End Function

    Private Sub DeleteGroupExpandedStatesByUser(userId As Guid)
        Using connection = GetConnection()
            connection.BeginTransaction()
            Using command = New SqlCommand(
                "delete from BPAGroupUserPref where UserId = @userid")

                command.AddParameter("@userid", userId)
                connection.Execute(command)
            End Using
            connection.CommitTransaction()
        End Using
    End Sub

    Private Function GetExpandedGroups(treeType As Short) As HashSet(Of Guid)
        Dim groups As New HashSet(Of Guid)

        Using connection = GetConnection()
            Using command = New SqlCommand(
                "select GroupId from BPAGroupUserPref where UserId = @userid and TreeType = @treetype")

                command.AddParameter("@userid", mLoggedInUser.Id)
                command.AddParameter("@treetype", treeType)

                Using reader = connection.ExecuteReturnDataReader(command)
                    Dim prov = New ReaderDataProvider(reader)

                    While reader.Read()
                        groups.Add(prov.GetGuid("groupid"))
                    End While
                End Using
            End Using
        End Using
        Return groups
    End Function

    <SecuredMethod()>
    Public Sub SaveTreeNodeExpandedState(id As Guid, expanded As Boolean, treeType As GroupTreeType) Implements IServer.SaveTreeNodeExpandedState
        CheckPermissions()
        Using connection = GetConnection()
            If Not expanded Then
                Using deleteCommand = New SqlCommand("delete from BPAGroupUserPref where UserId = @userid and TreeType = @treetype and GroupId = @groupId")
                    deleteCommand.AddParameter("@treetype", treeType)
                    deleteCommand.AddParameter("@userid", mLoggedInUser.Id)
                    deleteCommand.AddParameter("@groupId", id)
                    connection.Execute(deleteCommand)
                End Using
            Else
                Using insertCommand = New SqlCommand("if not exists (select 1 from BPAGroupUserPref where UserId = @userid and GroupId = @groupId and TreeType = @treetype) " &
                                                     "insert into BPAGroupUserPref (UserId, GroupId, TreeType) values (@userid, @groupId, @treetype)")
                    With insertCommand.Parameters
                        .AddWithValue("@treetype", treeType)
                        .AddWithValue("@userid", mLoggedInUser.Id)
                        .AddWithValue("@groupId", id)
                    End With
                    connection.Execute(insertCommand)
                End Using
            End If
        End Using
    End Sub
End Class
