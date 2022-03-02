#If UNITTESTS Then
Imports System.Drawing
Imports BluePrism.AutomateAppCore.Resources
Imports BluePrism.Core.Resources

Namespace DataContractRoundTrips.Generators

    Public Class ResourceInfoTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim opt As New ResourceInfo() With {
                    .Name = "My test resource ä",
                    .ActiveSessions = 82,
                    .Attributes = ResourceAttribute.LoginAgent,
                    .DisplayStatus = ResourceStatus.LoggedOut,
                    .ID = New Guid(),
                    .InfoColour = Color.Pink.ToArgb,
                    .Information = "",
                    .LastUpdated = DateTime.UtcNow.AddDays(-10),
                    .PendingSessions = 100,
                    .Pool = New Guid(),
                    .Status = ResourceMachine.ResourceDBStatus.Ready,
                    .WarningSessions = 250
                    }

            Yield Create("Null", New ResourceInfo())

            Yield Create("Populated", opt)

        End Function

    End Class

End Namespace
#End If
