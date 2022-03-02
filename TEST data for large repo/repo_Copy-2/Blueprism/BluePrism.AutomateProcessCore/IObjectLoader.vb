''' <summary>
''' Interface for an object loader which creates all the internal business objects
''' </summary>
Public Interface IObjectLoader
    ''' <summary>
    ''' Function to create all the internal business objects
    ''' </summary>
    ''' <param name="p">A reference to the process calling the object</param>
    ''' <param name="s">The session the object is running under</param>
    ''' <returns>A list of internal business objects</returns>
    Function CreateAll(p As clsProcess, s As clsSession) As IEnumerable(Of clsInternalBusinessObject)
End Interface
