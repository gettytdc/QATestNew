#If UNITTESTS Then
Imports BluePrism.AutomateProcessCore

Namespace DataContractRoundTrips.Generators

    Public Class ReportDataTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)
            Dim rd As New clsWorkQueuesBusinessObject.ReportData() With {
                    .Count = 2,
                    .LeastTime = New TimeSpan(100),
                    .MeanTime = New TimeSpan(120),
                    .MedianTime = New TimeSpan(125),
                    .MostTime = New TimeSpan(200),
                    .TotalTime = New TimeSpan(300)}
            With rd.Items
                .Add(Guid.NewGuid())
                .Add(Guid.NewGuid())
            End With

            Yield Create("Empty", New clsWorkQueuesBusinessObject.ReportData())

            Yield Create("With data", rd)

            Dim args As New clsArgumentList()
            With args
                .Add(New clsArgument("Queue Name", New clsProcessValue("Queue A")))
                .Add(New clsArgument("Finished Start Date", New clsProcessValue(DataType.datetime, Date.Now)))
                .Add(New clsArgument("Finished End Date", New clsProcessValue(DataType.datetime, Date.Now)))
                .Add(New clsArgument("Loaded Start Date", New clsProcessValue(DataType.datetime, Date.Now)))
                .Add(New clsArgument("Loaded End Date", New clsProcessValue(DataType.datetime, Date.Now)))
                .Add(New clsArgument("Resource Names", New clsProcessValue("Resource1;Resource2")))
                .Add(New clsArgument("Tags", New clsProcessValue("+READY")))
                .Add(New clsArgument("Include unworked items?", New clsProcessValue(True)))
                .Add(New clsArgument("Include deferred items?", New clsProcessValue(True)))
                .Add(New clsArgument("Include completed items?", New clsProcessValue(True)))
                .Add(New clsArgument("Include exception items?", New clsProcessValue(True)))
                .Add(New clsArgument("Treat each attempt separately?", New clsProcessValue(True)))
            End With

            Yield Create("Empty", New clsWorkQueuesBusinessObject.ReportParams())

            Yield Create("With data", New clsWorkQueuesBusinessObject.ReportParams(args))

        End Function

    End Class

End Namespace
#End If
