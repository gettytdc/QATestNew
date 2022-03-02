#If UNITTESTS Then
Imports BluePrism.AutomateProcessCore
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.Server.Domain.Models

Namespace DataContractRoundTrips.Generators

    Public Class WorkQueueTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            ' Note that any time-dependent properties are excluded from comparison. The state used
            ' to calculate EndTime and EndTimeUtc will still be validated.
            Dim q1 As New clsWorkQueue(Guid.NewGuid, "testqueue1", "key1", 20, True, 0)
            q1.Ident = 1
            q1.ProcessId = Guid.NewGuid
            q1.ResourceGroupId = Guid.NewGuid
            q1.TotalAttempts = 20
            q1.Completed = 1
            q1.Pending = 2
            q1.Deferred = 4
            q1.Exceptioned = 2
            q1.TotalWorkTime = New TimeSpan(100)
            q1.AverageWorkedTime = New TimeSpan(200)
            q1.TargetSessionCount = 200
            q1.Sessions = Nothing

            Yield Create("Empty Collections", q1, Function(o) o.Excluding(Function(wq) wq.Clock).
                            Excluding(Function(wq) wq.EndTime).
                            Excluding(Function(wq) wq.EndTimeUtc))

            Dim q2 As New clsWorkQueue(Guid.NewGuid, "testqueue2", "key2", 10, False, 0)
            Dim a = New Dictionary(Of Guid, Integer)
            a.Add(Guid.NewGuid, 3)
            a.Add(Guid.NewGuid, 5)
            a.Add(Guid.NewGuid, 7)

            Yield Create("With Assignments", q2, Function(o) o.
                            Excluding(Function(t) t.SelectedMemberInfo.Name = "ResourceGroup").
                            Excluding(Function(wq) wq.Clock).
                            Excluding(Function(wq) wq.EndTime).
                            Excluding(Function(wq) wq.EndTimeUtc))

            Dim q3 As New clsWorkQueue(Guid.NewGuid, "testqueue2", "key2", 10, False, 0)

            Dim args As New clsArgumentList
            For Each p In TestHelper.CreateProcessValueDictionary()
                args.Add(New clsArgument(p.Key, p.Value))
            Next

            Dim mock As New DictionaryDataProvider(New Hashtable From {
                                                      {"sessionnumber", 12},
                                                      {"sessionid", Guid.NewGuid()},
                                                      {"processid", Guid.NewGuid()},
                                                      {"statusid", SessionStatus.Pending},
                                                      {"processname", "process1"},
                                                      {"starterusername", "user1"},
                                                      {"runningresourcename", "resource1"},
                                                      {"startdatetime", Now() - TimeSpan.FromDays(5)},
                                                      {"enddatetime", Now() + TimeSpan.FromDays(5)},
                                                      {"startparamsxml", args.ArgumentsToXML(False)},
                                                      {"queueid", 33}
                                                      })

            Dim sessions As New List(Of clsProcessSession)
            sessions.Add(New clsProcessSession(mock))
            q3.Sessions = sessions

            Yield Create("With Sessions", q3, Function(o) o.
                            Excluding(Function(t) t.SelectedMemberInfo.Name = "ResourceGroup").
                            Excluding(Function(wq) wq.Clock).
                            Excluding(Function(wq) wq.EndTime).
                            Excluding(Function(wq) wq.EndTimeUtc))

        End Function
    End Class
End Namespace
#End If
