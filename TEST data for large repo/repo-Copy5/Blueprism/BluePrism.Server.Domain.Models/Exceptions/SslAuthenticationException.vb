Imports System.Runtime.Serialization

''' <summary>
''' Exception thrown when failing to authenticate using SSL
''' </summary>
<Serializable>
Public Class SslAuthenticationException
    Inherits BluePrismException

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Creates a new exception with the given message and inner exception
    ''' </summary>
    ''' <param name="exception">The exception that is the cause of the 
    ''' current exception
    ''' </param>
    Public Sub New(exception As Exception)
        MyBase.New(exception, exception.Message)
    End Sub

    ''' <summary>
    ''' Creates a new exception from the given de-serializer
    ''' </summary>
    ''' <param name="info">The serialization info from which this exception
    ''' should draw its data.</param>
    ''' <param name="ctx">The context defining the context for the current
    ''' deserialization stream.</param>
    Public Sub New(ByVal info As SerializationInfo, ByVal ctx As StreamingContext)
        MyBase.New(info, ctx)
    End Sub

End Class

