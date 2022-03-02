#If UNITTESTS Then
Imports FluentAssertions.Equivalency

Namespace DataContractRoundTrips.Generators

    Public Class ReleaseTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim release1 = New clsRelease(CreatePackage(), "Release 1", DateTime.UtcNow, "User 1", False)
            release1.AssociatedData = "Associated data"
            release1.Description = "A release"
            release1.FileName = "filename"
            release1.Id = Guid.NewGuid()
            release1.Local = True
            Dim p = release1.Package.Members(0)
            release1.Conflicts.Add(p, New ConflictDefinition("1", "Conflict 1", "Hint 1"))
            release1.Modifications.Add(clsRelease.ModificationType.OverwritingExisting, "name")
            release1.ReleaseNotes = "Release notes"

            Yield Create("Standard", release1, Function(options) options.IgnoringCyclicReferences() _
                            .Excluding(Function(s) ExcludeProperties(s)))

            Dim release2 = New clsRelease(New clsPackage(), String.Empty, False)
            Yield Create("Empty", release2, Function(options) options.IgnoringCyclicReferences() _
                            .Excluding(Function(s) ExcludeProperties(s)))

        End Function

        Private Function CreatePackage() As clsPackage
            Dim package = New clsPackage("package1", DateTime.UtcNow, "User 1")
            package.Add(New ScheduleComponent(package, 1, "Schedule component 1"))
            package.Add(New ProcessComponent(package, Guid.NewGuid(), "Process component 1"))
            Return package
        End Function

        Private Function ExcludeProperties(subject As ISubjectInfo) As Boolean
            ' PackageComponents have AssociatedData and more specific 
            ' AssociatedX properties that hit the db. UserId property also calls an IServer
            ' method and does not need to be included to test roundtripped state
            Dim name = subject.SelectedMemberInfo.Name
            Return name.StartsWith("Associated") Or name = "Conflicts" Or name = "UserId"
        End Function


    End Class

End Namespace
#End If
