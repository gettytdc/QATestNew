#If UNITTESTS Then
Imports BluePrism.Common.Security
Imports FluentAssertions.Equivalency

Namespace DataContractRoundTrips.Generators

    Public Class PackageTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)
            Dim package1 = New clsPackage()
            Yield Create("Empty", package1,
                         Function(options) options.IgnoringCyclicReferences() _
                            .Excluding(Function(p) Exclude(p)))
            Yield CreateWithCustomState("Properties (empty)", package1,
                                        Function(s) New With {s.Releases})

            Dim package2 = New clsPackage("Package 1", New Date(2011, 1, 1), "Frank")

            package2.Add(New ScheduleComponent(package2, 1, "Schedule 1"))
            Dim credential = New clsCredential()
            credential.Username = "Frank"
            credential.Password = New SafeString("password123")
            credential.Description = "Test credentials"
            credential.ExpiryDate = New DateTime(2025, 1, 1)
            package2.Add(New CredentialComponent(Nothing, credential))
            package2.Add(New ScheduleListComponent(Nothing, 1, "Schedule List 1"))
            ' Adds a local release
            package2.AddReleases(New clsRelease() {New clsRelease(package2, "Release 1", False)})

            Yield Create("With Components and Releases", package2,
                         Function(options) options.IgnoringCyclicReferences() _
                            .Excluding(Function(s) Exclude(s)))

        End Function

        Private Function Exclude(subject As ISubjectInfo) As Boolean
            ' Child components have AssociatedData and more specific 
            ' AssociatedX properties that hit the db. UserId property also calls an IServer
            ' method and does not need to be included to test roundtripped state
            Dim name = subject.SelectedMemberInfo.Name
            Return name.StartsWith("Associated") Or name = "Conflicts" Or name = "UserId"
        End Function
    End Class


End Namespace
#End If
