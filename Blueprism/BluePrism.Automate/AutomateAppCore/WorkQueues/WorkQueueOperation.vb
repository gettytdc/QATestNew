
Public Enum WorkQueueOperation As Integer

    ''' <summary>
    ''' Operation code indicating no operation - equivalent to an 'Empty' operation
    ''' </summary>
    ''' <remarks></remarks>
    None = 0

    ''' <summary>
    '''  Operation code indicating that a work queue item has been created
    ''' </summary>
    ItemCreated = 1

    ''' <summary>
    ''' Operation code indicating that a work queue item has been locked
    ''' </summary>
    ItemLocked = 2

    ''' <summary>
    ''' Operation code indicating that a work queue item has been deferred
    ''' </summary>
    ItemDeferred = 3

    ''' <summary>
    ''' Operation code indicating that a work queue item has been exceptioned
    ''' and that it will be retried.
    ''' </summary>
    ItemRetryInitiated = 4

    ''' <summary>
    ''' Operation code indicating that a work queue item has been marked as
    ''' having completed succesfully
    ''' </summary>
    ItemCompletedSuccessfully = 5

    ''' <summary>
    ''' Operation code indicating that a work queue item has been exceptioned
    ''' and that it <em>won't</em> be retried.
    ''' </summary>
    ItemCompletedWithException = 6

    ''' <summary>
    ''' Operation code indicating that a work queue item has been deleted
    ''' </summary>
    ItemDeleted = 7

    ''' <summary>
    ''' Operation code indicating that a work queue item has been 'force retried',
    ''' ie. retried after it has exceptioned with no retry.
    ''' </summary>
    ItemForceRetried = 8

End Enum
