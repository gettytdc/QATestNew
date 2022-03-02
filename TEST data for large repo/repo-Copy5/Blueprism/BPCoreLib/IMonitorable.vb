''' <summary>
''' The delegate used to handle the data changed event.
''' </summary>
''' <param name="sender">The source of the event</param>
''' <param name="args">The arguments which detail the data change.</param>
Public Delegate Sub DataChangeEventHandler( _
 ByVal sender As Object, ByVal args As DataChangeEventArgs)

''' <summary>
''' Interface describing an object which can be monitored for changes. An
''' abstract base class exists in the form of <see cref="clsDataMonitor"/>.
''' Extending this will make any class monitorable and it provides some helper
''' methods to make fulfilling the monitorable contract easier.
''' </summary>
Public Interface IMonitorable

    ''' <summary>
    ''' Event fired when the data in this class changes.
    ''' </summary>
    ''' <param name="sender">The source of the change</param>
    ''' <param name="args">The arguments defining the change</param>
    Event DataChanged As DataChangeEventHandler

    ''' <summary>
    ''' Checks to see if any data has been changed on this object since it was
    ''' retrieved from the database.
    ''' </summary>
    ''' <returns>True if the schedule has changed at all</returns>
    Function HasChanged() As Boolean

    ''' <summary>
    ''' Resets the changed data flag within this object. Immediately after
    ''' calling this method, <see cref="HasChanged"/> will return False.
    ''' </summary>
    Sub ResetChanged()

    ''' <summary>
    ''' Marks this object as having been changed. This allows external objects
    ''' to alter the changed data state of this object.
    ''' </summary>
    ''' <exception cref="InvalidOperationException">If the descendant class does
    ''' not allow external manipulation of the data changed state.</exception>
    Sub Mark()

End Interface
