Public Enum WorkQueueEventCode
    <EventCode("Q001","EventCodeAttribute_TheUser0CreatedTheQueue1")>
    CreateQueue
    <EventCode("Q002","EventCodeAttribute_TheUser0ModifiedTheQueue1")>
    ModifyQueue
    <EventCode("Q003","EventCodeAttribute_TheUser0DeletedTheQueue1")>
    DeleteQueue
    <EventCode("Q004","EventCodeAttribute_TheUser0ManuallyMarkedAnItemWithAnExceptionFromTheQueue1")>
    ManualException
    <EventCode("Q005","EventCodeAttribute_TheUser0ManuallyUnlockedAnItemFromTheQueue1")>
    ManualUnlock
    <EventCode("Q006","EventCodeAttribute_TheUser0DeletedSomeProcessedItemsFromTheQueue1")>
    DeleteProcessFromQueue
    <EventCode("Q007","EventCodeAttribute_TheUser0ExportedSomeDataFromTheQueue1")>
    ExportFromQueue
    <EventCode("Q008","EventCodeAttribute_TheUser0ForcedSomeItemsToRetryInQueue1")>
    ForceRetry
    <EventCode("Q009","EventCodeAttribute_TheUser0ImportedTheQueue1")>
    Import
    <EventCode("Q010","EventCodeAttribute_TheUser0ManuallyDeferredTheDateAndTimeOfItemSInQueue1")>
    ManualDefer
    <EventCode("Q011","EventCodeAttribute_WorkQueueItemsAddedAPI")>
    WorkQueueItemsAddedAPI
End Enum
