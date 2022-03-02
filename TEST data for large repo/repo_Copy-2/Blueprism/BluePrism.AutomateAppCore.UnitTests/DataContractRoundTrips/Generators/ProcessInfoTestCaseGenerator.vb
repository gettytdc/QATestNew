#If UNITTESTS Then
Imports BluePrism.AutomateProcessCore.Processes

Namespace DataContractRoundTrips.Generators

    Public Class ProcessInfoTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)
            Dim process = New ProcessInfo() With {
                    .Id = New Guid(),
                    .Type = DiagramType.Object,
                    .Name = "Process 1",
                    .Description = "Test Process for WCF Serialization/De-serialization",
                    .CanViewDefinition = True}

            Yield Create("Standard", process)
            Yield Create("Empty", New ProcessInfo())

        End Function
    End Class

End Namespace
#End If
