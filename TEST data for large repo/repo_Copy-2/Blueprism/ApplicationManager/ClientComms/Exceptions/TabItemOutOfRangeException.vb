''' <summary>
''' Exception thrown when a given index is out of the range of a list
''' </summary>
''' <seealso cref="System.ArgumentOutOfRangeException" />
Public Class ListItemOutOfRangeException : Inherits ArgumentOutOfRangeException

    ''' <summary>
    ''' Initializes a new instance of the <see cref="ListItemOutOfRangeException"/> class.
    ''' </summary>
    ''' <param name="paramName">The name of the parameter that causes this exception.</param>
    Public Sub New(paramName As String)
        MyBase.New(paramName)
    End Sub
End Class
