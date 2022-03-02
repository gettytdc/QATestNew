namespace BluePrism.Api.Domain
{
    public enum WorkQueueSortByProperty
    {
        NameAsc = 0,
        NameDesc = 1,
        RunningAsc = 2,
        RunningDesc = 3,
        KeyFieldAsc = 4,
        KeyFieldDesc = 5,
        MaxAttemptsAsc = 6,
        MaxAttemptsDesc = 7,
        EncryptIdAsc = 8,
        EncryptIdDesc = 9,
        TotalAsc = 10,
        TotalDesc = 11,
        CompletedAsc = 12,
        CompletedDesc = 13,
        PendingAsc = 14,
        PendingDesc = 15,
        ExceptionedAsc = 16,
        ExceptionedDesc = 17,
        TotalWorkTimeAsc = 18,
        TotalWorkTimeDesc = 19,
        AverageWorkedTimeAsc = 20,
        AverageWorkedTimeDesc = 21,
        LockedAsc = 22,
        LockedDesc = 23
    }
}
