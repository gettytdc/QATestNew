Imports System.Runtime.Serialization

''' <summary>
''' Exception raised when an operation is disallowed due to a licensing Overlap
''' </summary>
<Serializable>
Public Class LicenseOverlapException : Inherits BluePrismException

    ''' <summary>
    ''' Creates a new exception
    ''' </summary>
    Public Sub New()
        Me.New(Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new exception with the given message wrapped into an
    ''' <see cref="Licensing.GetOperationDisallowedMessage">operation
    ''' disallowed</see> message.
    ''' </summary>
    ''' <param name="msg">The message detailing the reason that the operation was
    ''' disallowed.</param>
    Public Sub New(ByVal msg As String)
        MyBase.New(msg)
    End Sub

    ''' <summary>
    ''' Creates a new exception from the given de-serializer
    ''' </summary>
    ''' <param name="info">The serialization info from which this exception should
    ''' draw its data.</param>
    ''' <param name="ctx">The context defining the context for the current
    ''' deserialization stream.</param>
    Protected Sub New(ByVal info As SerializationInfo, ByVal ctx As StreamingContext)
        MyBase.New(info, ctx)
    End Sub

End Class
