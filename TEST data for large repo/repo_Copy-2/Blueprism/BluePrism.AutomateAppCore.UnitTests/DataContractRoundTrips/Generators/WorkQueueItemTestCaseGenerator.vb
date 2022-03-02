#If UNITTESTS Then
Imports System.Xml.Linq
Imports BluePrism.AutomateProcessCore

Namespace DataContractRoundTrips.Generators

    Public Class WorkQueueItemTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim rowEmpty As New clsCollectionRow()

            Dim item1 As New clsWorkQueueItem(Guid.NewGuid(), 200, "key")
            With item1
                .DataRow = rowEmpty
                .DataXml = Nothing
                .CurrentState = clsWorkQueueItem.State.Pending
                .Position = 12
                .Status = "Unknown"
                .Resource = "Resource1"
                .Attempt = 1
                .Loaded = Now()
                .Worktime = 12
                .AttemptWorkTime = 2
                .Priority = 2
                .AddTag("on")
                .RemoveTag("off")
            End With
            Yield Create("Simple", item1)

            Dim item2 As New clsWorkQueueItem(Guid.NewGuid(), 200, "key")
            With item2
                .DataRow = rowEmpty
                .CurrentState = clsWorkQueueItem.State.None
                .Locked = Now()
            End With
            Yield Create("Locked", item2)

            Dim item3 As New clsWorkQueueItem(Guid.NewGuid(), 200, "key")
            With item3
                .DataRow = rowEmpty
                .CurrentState = clsWorkQueueItem.State.None
                .ExceptionDate = Now()
                .ExceptionReason = "elephant"
            End With
            Yield Create("Exceptioned", item3)

            Dim item4 As New clsWorkQueueItem(Guid.NewGuid(), 200, "key")
            With item4
                .DataRow = rowEmpty
                .CurrentState = clsWorkQueueItem.State.None
                .CompletedDate = Now()
            End With
            Yield Create("Completed", item4)

            Dim item5 As New clsWorkQueueItem(Guid.NewGuid(), 200, "key")
            With item5
                .DataRow = rowEmpty
                .CurrentState = clsWorkQueueItem.State.None
                .Deferred = Now()
            End With
            Yield Create("Deferred", item5)

            Dim item6 As New clsWorkQueueItem(Guid.NewGuid(), 200, "key")
            With item6
                .DataRow = rowEmpty
                .CurrentState = clsWorkQueueItem.State.None
            End With
            Yield Create("Pending", item6)

            Dim dataXml As XElement =
                    <collection>
                        <row>
                            <field name="Fish" type="text" value="Haddock"/>
                            <field name="Desserts" type="number" value="0"/>
                            <field name="DOB" type="date" value="1974/12/14"/>
                            <field name="Lunchtime" type="datetime" value="2017-04-18 12:30:00Z"/>
                            <field name="Hungry" type="flag" value="False"/>
                        </row>
                    </collection>

            Dim item7 As New clsWorkQueueItem(Guid.NewGuid(), 200, "key")
            With item7
                .DataRow = rowEmpty
                .CurrentState = clsWorkQueueItem.State.None
                .DataXml = dataXml.ToString()
            End With

            Yield Create("With XML", item7,
                         Function(o) o.Excluding(Function(t) t.SelectedMemberInfo.Name _
                                                             = "Data"))


            Dim rowPopulated As New clsCollectionRow
            For Each p In TestHelper.CreateProcessValueDictionary()
                rowPopulated.Add(p.Key, p.Value)
            Next

            Dim item8 As New clsWorkQueueItem(Guid.NewGuid(), 200, "key")
            With item8
                .DataRow = rowPopulated
                .CurrentState = clsWorkQueueItem.State.None
            End With

            'Note that the Data property seems to mess up the fluent assertions 
            'comparison so this has been excluded from the check (as it is not a data 
            'member and is not needed for testing serialization/deserialization
            Yield Create("Populated Datarow", item8,
                         Function(options) options _
                            .Excluding(Function(s) s.Data))


        End Function
    End Class

End Namespace
#End If
