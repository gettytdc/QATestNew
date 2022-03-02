''' <summary>
''' Interface that log diagnostics
''' </summary>
Public Interface IDiagnosticEmitter
    ''' <summary>
    ''' Event signalling need for diagnostic logging of specified information
    ''' </summary>
    ''' <param name="message">The information to be logged</param>
    Event Diags(message As String)
End Interface
