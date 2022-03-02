#If UNITTESTS Then
Imports BluePrism.BPCoreLib.Data

Namespace DataContractRoundTrips.Generators

    Public Class LockInfoTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)
            Dim info = New clsLockInfo(New DictionaryDataProvider(New Hashtable() From {
                                                                     {"name", "Lock 1"},
                                                                     {"token", "Lock 1 Token"},
                                                                     {"sessionid", Guid.NewGuid()},
                                                                     {"resourceid", Guid.NewGuid()},
                                                                     {"resourcename", "Res 1"},
                                                                     {"username", "Frank"},
                                                                     {"processid", Guid.NewGuid()},
                                                                     {"processname", "P1"},
                                                                     {"locktime", New DateTime(2010, 1, 1, 11, 30, 29)},
                                                                     {"comment", "Test comment"}
                                                                     }))

            Yield Create("Standard", info)

        End Function
    End Class

End Namespace
#End If
