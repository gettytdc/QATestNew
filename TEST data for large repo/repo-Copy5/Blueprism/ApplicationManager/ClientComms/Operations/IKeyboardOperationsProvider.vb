Imports BluePrism.Server.Domain.Models

Namespace Operations
    ''' <summary>
    ''' Provides methods for interacting with keyboard
    ''' </summary>
    Public Interface IKeyboardOperationsProvider

        ''' <summary>
        ''' Calls <see cref="Windows.Forms.SendKeys.SendWait"/> sending the keys specified 
        ''' to the current foreground application. If an interval is specified in the query
        ''' the keys are split and sent independently. Note that control characters cannot 
        ''' be sent if a non-zero interval is provided.
        ''' </summary>
        ''' <param name="keys">A string containing the keys to send to the target application.</param>
        ''' <param name="interval">The time to wait between sending each key</param>
        ''' <exception cref="InvalidValueException">If a non-zero interval value was
        ''' provided, but the text to send contained control characters.</exception>
        Sub SendKeys(keys As String, interval As TimeSpan)

    End Interface
End Namespace
