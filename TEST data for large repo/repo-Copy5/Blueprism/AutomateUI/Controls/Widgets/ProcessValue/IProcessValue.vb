Imports BluePrism.AutomateProcessCore

''' Project  : Automate
''' Interface    : IProcessValue
''' 
''' <summary>
''' Class to define the interface that all the controls which deal with values
''' must implement. e.g. ctlProcessDate, ctlProcessNumber etc...
''' </summary>
Public Interface IProcessValue

    ''' <summary>
    ''' Raised when the controls clsprocessValue is changed by the user
    ''' </summary>
    Event Changed As EventHandler

    ''' <summary>
    ''' The control must implement a property that allows access to the
    ''' clsProcessValue.
    ''' </summary>
    Property Value() As clsProcessValue

    ''' <summary>
    ''' Selects the control referenced by this process value implementation
    ''' </summary>
    Sub SelectControl()

    ''' <summary>
    ''' Sets the value control readonly
    ''' </summary>
    Property [ReadOnly]() As Boolean

    ''' <summary>
    ''' Commit any changes in the interface to the underlying value and raise a
    ''' <see cref="Changed"/> event if appropriate.
    ''' </summary>
    Sub Commit()

End Interface
