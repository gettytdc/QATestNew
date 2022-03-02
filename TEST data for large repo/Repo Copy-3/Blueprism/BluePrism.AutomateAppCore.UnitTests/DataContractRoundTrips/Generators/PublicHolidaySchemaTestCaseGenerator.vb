#If UNITTESTS Then
Imports BluePrism.Scheduling.Calendar
Imports BPScheduler.UnitTests

Namespace DataContractRoundTrips.Generators



    Public Class PublicHolidaySchemaTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)
            Dim schema As PublicHolidaySchema = New Builders.PublicHolidaySchemaBuilder().Build()
            Yield CreateWithCustomState("Public Holiday Schema", schema, Function(s) GetCompareData(s))

        End Function



        ''' <summary>
        ''' Get the data to compare before and after the round trip from the scheme we have
        ''' created
        ''' </summary>
        ''' <param name="schema">The tests scheme to get the data from</param>
        ''' <returns>The data as a CompareData object</returns>
        Private Function GetCompareData(schema As PublicHolidaySchema) As CompareData
            Dim data = New CompareData
            ' check that the groups are correct
            data.Groups = schema.GetGroups()
            ' and that the holidays are correct within them
            data.Group1 = schema.GetHolidays(DummyStore.Holiday.EnglandAndWales)
            data.Group2 = schema.GetHolidays(DummyStore.Holiday.RepublicOfIreland)
            Return data
        End Function

        ''' <summary>
        ''' Class to define the structure of the data we want to compare on
        ''' </summary>
        Public Class CompareData
            Public Property Group1 As ICollection(Of PublicHoliday)

            Public Property Group2 As ICollection(Of PublicHoliday)

            Public Property Groups As ICollection(Of String)

        End Class

    End Class

End Namespace
#End If
