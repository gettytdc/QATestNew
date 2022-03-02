#If UNITTESTS Then
Imports BluePrism.AutomateProcessCore
Imports BluePrism.BPCoreLib.Data

Namespace DataContractRoundTrips.Generators

    Public Class SessionLogTestCaseGenerator
        Inherits TestCaseGenerator

        Public Class TestType
            Public Property SessionNumber As Object
            Public Property StageId As Object
        End Class

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim entryProv As New DictionaryDataProvider(New Hashtable() From {
                                                           {"SessionNumber", 842},
                                                           {"Logid", 1},
                                                           {"StageId", Guid.NewGuid()},
                                                           {"StageName", "Calc fields"},
                                                           {"StageType", StageTypes.Calculation},
                                                           {"ProcessName", "Test Process"},
                                                           {"PageName", "Page 1"},
                                                           {"ObjectName", "Test Object"},
                                                           {"ActionName", "DoTest"},
                                                           {"Result", "17"},
                                                           {"ResultType", DataType.number},
                                                           {"StartDateTime", Date.Now.AddMilliseconds(-456)},
                                                           {"EndDateTime", Date.Now},
                                                           {"AttributeXML", "<xml><innerxml></innerxml></xml>"},
                                                           {"automateworkingset", 1024L},
                                                           {"targetappname", "Calc"},
                                                           {"targetappworkingset", 1024L}})
            Dim logEntry As New clsSessionLogEntry(entryProv)

            Yield Create("Standard", logEntry)

            Yield CreateWithCustomState("Private fields", logEntry,
                                        Function(le) New TestType With {
                                           .SessionNumber = TestCaseGenerator.GetField(le, "mSessionNumber"),
                                           .StageId = TestCaseGenerator.GetField(le, "mStageId")
                                           }
                                        )

            Dim logProv As New DictionaryDataProvider(New Hashtable() From {
                                                         {"SessionId", Guid.NewGuid()},
                                                         {"SessionNumber", 842},
                                                         {"StartDateTime", Date.Now},
                                                         {"EndDateTime", Date.Now.AddMinutes(12.5)},
                                                         {"ProcessId", Guid.NewGuid()},
                                                         {"ProcessName", "Test Process"},
                                                         {"StarterResourceId", Guid.NewGuid()},
                                                         {"StarterUserId", Guid.NewGuid()},
                                                         {"RunningResourceId", Guid.NewGuid()},
                                                         {"RunningResourceName", "Resource1"},
                                                         {"RunningOSUserName", "Admin"},
                                                         {"StatusID", Server.Domain.Models.SessionStatus.Completed}})

            Yield Create("Completed (with entry)", New clsSessionLog(logProv, {logEntry}))

        End Function

    End Class

End Namespace
#End If
