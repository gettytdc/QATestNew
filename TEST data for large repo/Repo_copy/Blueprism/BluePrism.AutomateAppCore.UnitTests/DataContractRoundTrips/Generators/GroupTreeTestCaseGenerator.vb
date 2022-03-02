
#If UNITTESTS Then
Imports BluePrism.AutomateAppCore.Groups

Namespace DataContractRoundTrips.Generators

    Public Class GroupTreeTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim tree = New GroupTree(GroupTreeType.Processes, 100, False, New TreeAttributes())
            tree.Root.Add(New Group(GroupTestCaseGenerator.CreateProvider(1, "Group 1")) With {.Permissions = New MemberPermissions(Nothing)})
            tree.Root.Add(New Group(GroupTestCaseGenerator.CreateProvider(2, "Group 1.1")) With {.Permissions = New MemberPermissions(Nothing)})
            ' Definition and getter triggers call to permissions which won't be initialised
            ' Also, it doesn't depend on serialized state so can be excluded
            Yield Create("Nested", tree,
                         Function(options) options.
                            IgnoringCyclicReferences().
                            Excluding(Function(gt) gt.Definition)
                         )

        End Function


    End Class

End Namespace
#End If
