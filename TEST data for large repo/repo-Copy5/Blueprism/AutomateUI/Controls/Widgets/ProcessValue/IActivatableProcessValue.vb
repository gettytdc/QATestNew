''' Project  : Automate
''' <summary>
''' Interface describing a process value control which can be activated.
''' The obvious example is a collection control - 'activation' of a collection
''' allows an arbitrary control (say the containing form) to open the collection
''' that was activated.
''' </summary>
Public Interface IActivatableProcessValue
    Inherits IProcessValue

    ''' <summary>
    ''' Event fired when a process value control is activated.
    ''' </summary>
    ''' <param name="sender">The process value control which fired this event.
    ''' This will usually be 'this' object.</param>
    ''' <param name="e">The event args detailing the event.</param>
    Event Activated(ByVal sender As IActivatableProcessValue, ByVal e As EventArgs)

End Interface
