#If UNITTESTS Then
Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.AutomateProcessCore
Imports BluePrism.BPCoreLib.Data

Namespace DataContractRoundTrips.Generators

    Public Class ProcessGroupMemberTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim group1 = BuildGroup(Guid.NewGuid, "Group 1")

            Dim process = BuildProcess(Guid.NewGuid, "Process 1")
            group1.Add(process)

            Yield Create("", process, Function(options) options.IgnoringCyclicReferences())

        End Function

        Private Function BuildGroup(id As Guid, name As String) As Group
            Dim g As New Group(CreateProvider(id, name)) With {
                    .Permissions = New MemberPermissions(Nothing)}
            Return g
        End Function

        Private Function BuildProcess(id As Guid, name As String) As ProcessGroupMember
            Dim p As New ProcessGroupMember(CreateProvider(id, name)) With {
                    .Attributes = ProcessAttributes.PublishedWS,
                    .Permissions = New MemberPermissions(Nothing)}
            Return p
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
