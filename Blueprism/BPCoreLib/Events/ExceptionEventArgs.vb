Public Delegate Sub ExceptionEventHandler(sender As Object, e As ExceptionEventArgs)

''' <summary>
''' Event args detailing an event around an exception. This is typically used to
''' report an error which occurred asynchronously - ie. it has no specific action
''' which caused an error reaction.
''' </summary>
Public Class ExceptionEventArgs : Inherits EventArgs

    ' The exception these args are based on
    Private mEx As Exception

    ''' <summary>
    ''' Creates a new event args around the given exception
    ''' </summary>
    ''' <param name="ex">The exception making up the event.</param>
    ''' <exception cref="ArgumentNullException">If <paramref name="ex"/> is null.
    ''' </exception>
    Public Sub New(ex As Exception)
        If ex Is Nothing Then Throw New ArgumentNullException(NameOf(ex))
        mEx = ex
    End Sub

    ''' <summary>
    ''' The error associated with these args.
    ''' </summary>
    Public ReadOnly Property Exception As Exception
        Get
            Return mEx
        End Get
    End Property

End Class
