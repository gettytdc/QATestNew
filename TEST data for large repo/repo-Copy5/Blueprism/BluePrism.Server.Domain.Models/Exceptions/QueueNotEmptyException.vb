Imports System.Globalization
Imports System.Runtime.Serialization

''' <summary>
''' Exception thrown when an operation is attempted which requires an empty queue
''' and the specified queue was not empty (ie. it had some work items associated
''' with it)
''' </summary>
<Serializable()> _
Public Class QueueNotEmptyException : Inherits BluePrismException

    ''' <summary>
    ''' Creates a new exception indicating that the queue with the specified name
    ''' was not empty.
    ''' </summary>
    Public Sub New(ByVal queueName As String, locale As CultureInfo)
        MyBase.New(
            My.Resources.ResourceManager.GetString("QueueNotEmptyException_TheOperationCouldNotBeCompletedBecauseTheQueue0IsNotEmpty", locale), queueName)
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

