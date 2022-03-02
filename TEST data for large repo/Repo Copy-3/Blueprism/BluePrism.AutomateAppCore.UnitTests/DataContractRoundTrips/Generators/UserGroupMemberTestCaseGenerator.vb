#If UNITTESTS Then
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.Server.Domain.Models

Namespace DataContractRoundTrips.Generators

    Public Class UserGroupMemberTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim group1 = BuildGroup(Guid.NewGuid, "Group 1")

            Dim user = BuildUser(Guid.NewGuid(), "King Nothing")
            group1.Add(user)

            Yield Create("", user,
                         Function(options) options.
                            IgnoringCyclicReferences().
                            Excluding(Function(u) u.RoleNames).
                            Excluding(Function(u) u.Roles).
                            Excluding(Function(u) u.RoleNamesJoined)
                         )
        End Function

        Private Function BuildGroup(id As Guid, name As String) As Group
            Dim g As New Group(CreateProvider(id, name)) With {
                    .Permissions = New MemberPermissions(Nothing)}
            Return g
        End Function

        Private Function BuildUser(id As Guid, name As String) As UserGroupMember
            Dim u = New UserGroupMember(New DictionaryDataProvider(New Hashtable() From {
                                                                      {"id", id},
                                                                      {"name", name},
                                                                      {"validfrom", New Date(2015, 4, 16, 9, 15, 24)},
                                                                      {"validto", New Date(2018, 1, 1, 12, 30, 29)},
                                                                      {"passwordexpiry", Date.Now() + TimeSpan.FromDays(30)},
                                                                      {"lastsignedin", Date.Now() - TimeSpan.FromDays(3)},
                                                                      {"isdeleted", False},
                                                                      {"loginattempts", 2},
                                                                      {"maxloginattempts", 5},
                                                                      {"authtype", AuthMode.Native}
                                                                      }))
            u.AppendFrom(New SingletonDataProvider("roleid", 1))
            u.AppendFrom(New SingletonDataProvider("roleid", 2))
            u.AppendFrom(New SingletonDataProvider("roleid", 3))
            Return u

        End Function

        Friend Shared Function CreateProvider(Of TId)(id As TId, name As String) As IDataProvider
            Return New DictionaryDataProvider(New Hashtable() From {
                                                 {"id", id},
                                                 {"name", name},
                                                 {"description", name + " description"},
                                                 {"createddate", New Date(2018, 1, 1, 12, 30, 29)},
                                                 {"createdby", "Frank"},
                                                 {"lastmodifieddate", New Date(2018, 1, 3, 12, 30, 29)},
                                                 {"lastmodifiedby", "Alan"},
                                                 {"isdefault", True}
                                                 })

        End Function

    End Class

End Namespace
#End If
