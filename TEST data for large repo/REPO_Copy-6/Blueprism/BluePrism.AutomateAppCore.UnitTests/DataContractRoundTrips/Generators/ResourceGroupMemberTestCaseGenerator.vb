#If UNITTESTS Then
Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.Core.Resources

Namespace DataContractRoundTrips.Generators

    Public Class ResourceGroupMemberTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim group1 = BuildGroup(Guid.NewGuid, "Group 1")

            Dim res = BuildResource(Guid.NewGuid, "Resource 1")
            group1.Add(res)

            Yield Create("", res, Function(options) options.IgnoringCyclicReferences())

        End Function

        Private Function BuildGroup(id As Guid, name As String) As Group
            Dim g As New Group(CreateProvider(id, name)) With {
                    .Permissions = New MemberPermissions(Nothing)}
            Return g
        End Function

        Private Function BuildResource(id As Guid, name As String) As ResourceGroupMember
            Return New ResourceGroupMember(CreateProvider(id, name)) _
                With {
                    .Attributes = ResourceAttribute.Pool
                    }
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
