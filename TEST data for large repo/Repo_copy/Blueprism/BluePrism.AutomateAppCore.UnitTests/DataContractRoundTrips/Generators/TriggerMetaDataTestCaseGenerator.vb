#If UNITTESTS Then
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.Scheduling

Namespace DataContractRoundTrips.Generators

    Public Class TriggerMetaDataTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim prov As New DictionaryDataProvider(New Hashtable From {
                                                      {"unittype", IntervalType.Year},
                                                      {"mode", TriggerMode.Fire},
                                                      {"priority", 1},
                                                      {"startdate", Date.Today},
                                                      {"enddate", Date.Today.AddDays(5)},
                                                      {"period", 12},
                                                      {"startpoint", 60},
                                                      {"endpoint", 120},
                                                      {"dayset", 3},
                                                      {"calendarid", 14},
                                                      {"nthofmonth", NthOfMonth.First},
                                                      {"missingdatepolicy", NonExistentDatePolicy.FirstSupportedDayInNextMonth},
                                                      {"usertrigger", False}})

            Yield Create("Default", New TriggerMetaData())

            Yield Create("Populated", New TriggerMetaData(prov))

        End Function

    End Class

End Namespace
#End If
