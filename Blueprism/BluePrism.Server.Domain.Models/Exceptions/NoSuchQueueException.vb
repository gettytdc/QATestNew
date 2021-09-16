Imports System.Globalization
Imports System.Runtime.Serialization

''' <summary>
''' Exception thrown when a queue name is required and that which has been
''' specified does not exist.
''' </summary>
<Serializable()> _
Public Class NoSuchQueueException : Inherits NoSuchElementException

    ''' <summary>
    ''' Creates a new exception indicating that no queue with the specified name
    ''' was found.
    ''' </summary>
    Public Sub New(queueName As String, locale As CultureInfo)
        MyBase.New(My.Resources.ResourceManager.GetString("NoSuchQueueException_TheQueueWithName0WasNotFound", locale), queueName)
    End Sub

    ''' <summary>
    ''' Creates a new exception indicating that no queue with the specified identity
    ''' was found
    ''' </summary>
    ''' <param name="queueIdent">The identity of the queue which was not found.
    ''' </param>
    Public Sub New(queueIdent As Integer)
        MyBase.New("The queue with identity '{0}' was not found", queueIdent)
    End Sub

    Public Sub New(workQueueId As Guid)
        MyBase.New("The queue with id '{0}' was not found", workQueueId)
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
