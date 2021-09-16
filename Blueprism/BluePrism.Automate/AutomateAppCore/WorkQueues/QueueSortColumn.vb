
''' <summary>
''' Columns on which queue items can be sorted
''' </summary>
''' <seealso cref="WorkQueueFilter"/>
Public Enum QueueSortColumn
    DefaultOrdering
    ByPosition
    ByItemKey
    ByPriority
    ByLoadedDate
    ByExceptionDate
    ByNextReviewDate
    ByLastUpdatedDate
    ByWorkTime
    ByStatus
    ByTags
    ByAttempt
    ByResource
    ByCompleted
    ByExceptionReason
    ByState 'ie group by pending/completed/failed/etc
End Enum
