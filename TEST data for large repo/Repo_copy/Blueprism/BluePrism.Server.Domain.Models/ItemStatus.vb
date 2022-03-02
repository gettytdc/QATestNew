''' Project  : was in BPCoreLib
''' Enum     : ItemStatus
''' <summary>
''' Enumeration which details the status of an arbitrary item - this is generic 
''' enough that it can be used and extended for anything with a status, and is used
''' to offer a unified way of displaying ItemStatus in a data grid view or list view.
''' </summary>
''' <remarks>The numbers chosen lean heavily towards existing numbers within the
''' clsProcess.SessionStatus enum for back-compatibility.</remarks>
Public Enum ItemStatus

    ''' <summary>
    ''' Used to indicate all statuses in filters - sort of equivalent to 'unknown'.
    ''' As a status, it's a horrible one to try and use to make any semantic sense,
    ''' so if clsProcessSession is ever worked into this, I propose renaming or, at
    ''' least, providing an equivalent name for this value.
    ''' </summary>
    All = -2

    ''' <summary>
    ''' The status is not known or the item has no status
    ''' </summary>
    Unknown = -1

    ''' <summary>
    ''' The item is pending execution. It has been created, and it is ready
    ''' to run, but it has not yet been started.
    ''' </summary>
    Pending = 0

    ''' <summary>
    ''' The item is running - ie. currently executing.
    ''' </summary>
    Running = 1

    ''' <summary>
    '''  The item has failed / exceptioned / terminated / fail-word of the month-ed.
    ''' </summary>
    Failed = 2

    ''' <summary>
    ''' The item has exceptioned / failed / terminated /etc.
    ''' Equivalent of Failed
    ''' </summary>
    Exceptioned = 2

    ''' <summary>
    ''' The item has exceptioned / failed / terminated /etc.
    ''' Equivalent of Failed and, er, Exceptioned
    ''' </summary>
    Terminated = 2

    ''' <summary>
    ''' The item has been stopped - this usually indicates some external force
    ''' has caused the item to stop rather than completion or an internal error
    ''' condition (eg. user stopped it using the GUI, scheduler stopped it due
    ''' to errors elsewhere).
    ''' </summary>
    Stopped = 3

    ''' <summary>
    ''' The item was completed successfully.
    ''' </summary>
    Completed = 4

    ''' <summary>
    ''' The item is being executed in DEBUG mode - ie. it is currently being
    ''' debugged by a user.
    ''' </summary>
    Debugging = 5

    ''' <summary>
    ''' The item has been deferred. ie. execution has been deferred until a
    ''' later date.
    ''' </summary>
    Deferred = 6

    ''' <summary>
    ''' The item is locked, meaning that no other user (real-life user or 
    ''' figurative - eg. an automated work queue manager)
    ''' </summary>
    Locked = 7

    ''' <summary>
    ''' Specific status indicating a query on this item. A variant on unknown which
    ''' specifically does *not* mean 'None'.
    ''' </summary>
    Queried = 8

    ''' <summary>
    ''' The item has sub items that have failed
    ''' Equivalent of Failed
    ''' </summary>
    PartExceptioned = 9

End Enum
