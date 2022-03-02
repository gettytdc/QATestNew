#If UNITTESTS Then
Imports BluePrism.AutomateAppCore.clsServerPartialClasses.DataContracts

Namespace DataContractRoundTrips.Generators

    Public Class LockedProcessesResultTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim lockedProcessesResult = New LockedProcessesResult With
                    {
                    .LockedProcesses = New List(Of LockedProcess) From
                    {
                    New LockedProcess With
                    {
                    .Id = Guid.NewGuid(),
                    .Name = Guid.NewGuid().ToString(),
                    .LockDate = DateTime.Now,
                    .MachineName = Guid.NewGuid().ToString(),
                    .Username = Guid.NewGuid().ToString()
                    }
                    }
                    }

            Yield Create("Standard", lockedProcessesResult)

        End Function
    End Class

End Namespace
#End If
