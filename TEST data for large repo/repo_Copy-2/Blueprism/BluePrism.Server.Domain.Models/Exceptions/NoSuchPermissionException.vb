Imports System.Runtime.Serialization

''' <summary>
''' Exception thrown when a lock name is required and that which has been
''' specified does not exist.
''' </summary>
<Serializable()> _
Public Class NoSuchPermissionException : Inherits NoSuchElementException
    ''' <summary>
    ''' Creates a new exception with no message
    ''' </summary>
    Public Sub New(ByVal permName As String)
        MyBase.New(My.Resources.NoSuchPermissionException_ThePermission0DoesNotExist, permName)
    End Sub

    ''' <summary>
    ''' Creates a new exception from the given de-serializer
    ''' </summary>
    ''' <param name="info">The serialization info from which this exception
    ''' should draw its data.</param>
    ''' <param name="ctx">The context defining the context for the current
    ''' deserialization stream.</param>
    Protected Sub New(ByVal info As SerializationInfo, ByVal ctx As StreamingContext)
        MyBase.New(info, ctx)
    End Sub

End Class
