
#If UNITTESTS Then
Imports BluePrism.AutomateProcessCore
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.ClientServerResources.Core.Enums

Namespace DataContractRoundTrips.Generators

    Public Class ScheduledSessionTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)


            Dim session1 As New ScheduledSession(1, Guid.NewGuid,
                                                 "Resource 1",
                                                 Guid.NewGuid,
                                                 False, False, GetArgumentList)
            Yield Create("Arguments", session1,
                         Function(options) options _
                            .Excluding(Function(a) a.ProcessName) _
                            .Excluding(Function(a) a.ResourceId))


            Dim session2 As New ScheduledSession(2, Guid.NewGuid,
                                                 "Resource 2",
                                                 Guid.NewGuid,
                                                 False, False, New clsArgumentList)
            SetField(session2, "mResourceId", Guid.NewGuid)

            Yield Create("Resource", session2,
                         Function(options) options _
                            .Excluding(Function(a) a.ProcessName))




            Dim session3 As New ScheduledSession(3, Guid.NewGuid,
                                                 "Resource 3", Guid.NewGuid,
                                                 False, False, New clsArgumentList)
            Dim identifier =
                    Activator.CreateInstance(
                        "AutomateAppCore",
                        "BluePrism.AutomateAppCore.ScheduledSession+SessionIdentifier").Unwrap

            SetField(identifier, "sessionId", Guid.NewGuid)
            SetField(identifier, "sessionNo", 100)
            SetField(session3, "mSession", identifier)

            Yield Create("SessionIdentifier", session3,
                         Function(options) options _
                            .Excluding(Function(a) a.ProcessName) _
                            .Excluding(Function(a) a.ResourceId))





            Dim session4 As New ScheduledSession(4, Guid.NewGuid,
                                                 "Resource 4", Guid.NewGuid,
                                                 False, False, New clsArgumentList)
            session4.ErrorMessage = "Some error"

            Yield Create("Error", session4,
                         Function(options) options _
                            .Excluding(Function(a) a.ProcessName) _
                            .Excluding(Function(a) a.ResourceId))




            Dim session5 As New ScheduledSession(5, Guid.NewGuid,
                                                 "Resource 5",
                                                 Guid.NewGuid,
                                                 False, False, New clsArgumentList)

            Dim map As New clsOrderedDictionary(Of Guid, RunnerStatus)
            map(Guid.NewGuid) = RunnerStatus.IDLE
            map(Guid.NewGuid) = RunnerStatus.STARTFAILED

            session5.Data = map

            Yield Create("Data", session5,
                         Function(options) options _
                            .Excluding(Function(a) a.ProcessName) _
                            .Excluding(Function(a) a.ResourceId))

        End Function

        Private Function GetArgumentList() As clsArgumentList

            Dim args As New clsArgumentList

            For Each p In TestHelper.CreateProcessValueDictionary()
                args.Add(New clsArgument(p.Key, p.Value))
            Next

            Return args

        End Function

    End Class
End Namespace
#End If
