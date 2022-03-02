Imports BluePrism.Server.Domain.Models

''' <summary>
''' Exception thrown when custom code cannot be executed due to a compilation
''' error
''' </summary>
<Serializable>
Public Class InvalidCodeException : Inherits BluePrismException

    ''' <summary>
    ''' Creates a new <see cref="InvalidCodeException"/> with the given message.
    ''' </summary>
    ''' <param name="msg">The message explaining the exception</param>
    Public Sub New(ByVal msg As String)
        MyBase.New(msg)
    End Sub
    
End Class
