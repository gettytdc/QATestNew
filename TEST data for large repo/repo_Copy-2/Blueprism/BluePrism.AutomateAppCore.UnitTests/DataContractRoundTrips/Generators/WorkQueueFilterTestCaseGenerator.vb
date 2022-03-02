#If UNITTESTS Then
Namespace DataContractRoundTrips.Generators


    Public Class WorkQueueFilterTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Yield Create("Default", New WorkQueueFilter())

            Dim startDate = Date.UtcNow - TimeSpan.FromDays(5)
            Dim endDate = Date.UtcNow

            Dim workQueueFilter = New WorkQueueFilter()
            workQueueFilter.CompletedEndDate = endDate
            workQueueFilter.CompletedStartDate = startDate
            workQueueFilter.ExceptionEndDate = endDate
            workQueueFilter.ExceptionStartDate = startDate
            workQueueFilter.ItemId = Guid.NewGuid()
            workQueueFilter.ItemKey = "key1"
            workQueueFilter.LastUpdatedEndDate = endDate
            workQueueFilter.LastUpdatedStartDate = startDate
            workQueueFilter.LoadedEndDate = endDate
            workQueueFilter.LoadedStartDate = startDate
            workQueueFilter.MaxAttempt = 5
            workQueueFilter.MaxPriority = 2
            workQueueFilter.MaxRows = 8
            workQueueFilter.MaxWorkTime = 10
            workQueueFilter.MinAttempt = 2
            workQueueFilter.MinPriority = 1
            workQueueFilter.MinWorkTime = 4
            workQueueFilter.NextReviewEndDate = endDate
            workQueueFilter.NextReviewStartDate = startDate
            workQueueFilter.Resource = "resource"
            workQueueFilter.SortColumn = QueueSortColumn.ByItemKey
            workQueueFilter.SortOrder = QueueSortOrder.Descending
            workQueueFilter.StartIndex = 1
            workQueueFilter.Status = "Active"
            workQueueFilter.Tags = "+tag1 -tag2"

            workQueueFilter.SetStates(QueueItemState.Deferred)


            Yield Create("Standard", workQueueFilter)
        End Function
    End Class

End Namespace
#End If
