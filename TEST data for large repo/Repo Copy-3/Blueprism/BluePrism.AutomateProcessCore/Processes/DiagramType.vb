Namespace Processes

    ''' <summary>
    ''' The two types of process are distinguished using this enumeration, currently
    ''' these are Process and Object, Process is what we have in process studio
    ''' whereas object is what we have in Object studio.
    ''' </summary>
    Public Enum DiagramType As Integer
        [Unset] = -1
        [Process] = 0
        [Object] = 1
    End Enum

End Namespace