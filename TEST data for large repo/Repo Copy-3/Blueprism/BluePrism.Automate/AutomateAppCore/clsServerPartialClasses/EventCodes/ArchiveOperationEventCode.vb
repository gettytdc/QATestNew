Public Enum ArchiveOperationEventCode
    <EventCode("AR01","EventCodeAttribute_TheUser0ArchivedSessionLogs1")>
    Archive
    <EventCode("AR02","EventCodeAttribute_TheUser0DeletedSessionLogs1")>
    Delete
    <EventCode("AR03","EventCodeAttribute_TheUser0RestoredSessionLogs1")>
    Restore
    <EventCode("AR04","EventCodeAttribute_DebugSessionSThatWereUnableToCloseSuccessfullyHaveNowBeenClosed")>
    Clean
End Enum
