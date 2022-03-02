
''' <summary>
''' The states as held in the database for a given work queue item.
''' Note that 'Deferred' in this case means any item which has a referral
''' date, regardless of whether that date is in the future or the past.
''' </summary>
''' <remarks>The column 'state' which these refer to was added in the
''' DB script : db_upgradeR113.sql </remarks>
Public Enum QueueItemState
    Exceptioned = 5
    Completed = 4
    Deferred = 3
    Locked = 2
    Pending = 1
    None = 0
End Enum
