#If UNITTESTS Then
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Processes
Imports BluePrism.AutomateProcessCore.Stages

Namespace DataContractRoundTrips.Generators

    Public Class ProcessSchemaTestCaseGenenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)
            Dim schema = New ProcessSchema(True, "1.2", DiagramType.Object) With
                    {
                    .Id = Guid.NewGuid(),
                    .Name = "test schema",
                    .Description = "This is my description for this schema",
                    .EndPoint = "This is the endpoint",
                    .CreatedBy = "Emma",
                    .CreatedDate = DateTime.Parse("01/01/2001"),
                    .ModifiedBy = "Emma",
                    .ModifiedDate = DateTime.Parse("01/01/2002")
                    }

            schema.Stages.Add(New clsCalculationStage(Nothing))
            schema.SubSheets.Add(New clsProcessSubSheet(Nothing))

            Create("Process Schema", schema)

        End Function

    End Class
End Namespace
#End If
