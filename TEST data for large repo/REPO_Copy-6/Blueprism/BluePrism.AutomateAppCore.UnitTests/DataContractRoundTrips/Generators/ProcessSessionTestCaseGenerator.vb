#If UNITTESTS Then
Imports BluePrism.AutomateProcessCore
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.Server.Domain.Models

Namespace DataContractRoundTrips.Generators

    Public Class ProcessSessionTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim mock1 As New DictionaryDataProvider(New Hashtable() From {
                                                       {"sessionnumber", 12},
                                                       {"sessionid", Guid.NewGuid()},
                                                       {"processid", Guid.NewGuid()},
                                                       {"statusid", SessionStatus.Pending},
                                                       {"processname", "process1"},
                                                       {"starterusername", "user1"},
                                                       {"runningresourcename", "resource1"},
                                                       {"runningresourceid", Guid.NewGuid()},
                                                       {"startdatetime", Now() - TimeSpan.FromDays(5)},
                                                       {"enddatetime", Now() + TimeSpan.FromDays(5)},
                                                       {"startparamsxml", ""},
                                                       {"queueid", 33},
                                                       {"laststage", "start1"},
                                                       {"lastupdated", Now()}
                                                       })

            Dim session1 As New clsProcessSession(mock1)
            Dim groupPerm1 = New GroupPermissions()
            session1.ProcessPermissions = New MemberPermissions(groupPerm1)
            session1.ResourcePermissions = New MemberPermissions(groupPerm1)

            Yield Create("No Arguments", session1)

            Dim args As New clsArgumentList
            For Each p In TestHelper.CreateProcessValueDictionary()
                args.Add(New clsArgument(p.Key, p.Value))
            Next

            Dim mock2 As New DictionaryDataProvider(New Hashtable() From {
                                                       {"sessionnumber", 12},
                                                       {"sessionid", Guid.NewGuid()},
                                                       {"processid", Guid.NewGuid()},
                                                       {"statusid", SessionStatus.Pending},
                                                       {"processname", "process1"},
                                                       {"starterusername", "user1"},
                                                       {"runningresourcename", "resource1"},
                                                       {"runningresourceid", Guid.NewGuid()},
                                                       {"startdatetime", Now() - TimeSpan.FromDays(5)},
                                                       {"enddatetime", Now() + TimeSpan.FromDays(5)},
                                                       {"startparamsxml", args.ArgumentsToXML(False)},
                                                       {"queueid", 33},
                                                       {"laststage", "start1"},
                                                       {"lastupdated", Now()}
                                                       })

            Dim session2 As New clsProcessSession(mock2)
            Dim groupPerm2 = New GroupPermissions()
            session2.ProcessPermissions = New MemberPermissions(groupPerm2)
            session2.ResourcePermissions = New MemberPermissions(groupPerm2)

            Yield Create("With Arguments", session2)

        End Function


    End Class

End Namespace
#End If
