#If UNITTESTS Then
Imports BluePrism.Scheduling.Calendar

Namespace DataContractRoundTrips.Generators

    Public Class PublicHolidayTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim schema As PublicHolidaySchema = New Builders.PublicHolidaySchemaBuilder().Build()
            'Test the scotland holidays as they cover all member fields
            For Each hol In schema.GetHolidays()
                Yield Create(hol.Name, hol)
            Next

        End Function
    End Class

End Namespace
#End If
