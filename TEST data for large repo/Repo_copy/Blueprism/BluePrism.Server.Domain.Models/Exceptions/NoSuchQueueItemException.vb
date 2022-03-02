Imports System.Runtime.Serialization


<Serializable()>
Public Class NoSuchQueueItemException : Inherits NoSuchElementException

    Public Sub New(workQueueItemId As Guid)
        MyBase.New(My.Resources.NoSuchQueueItemException, workQueueItemId)
    End Sub

    Protected Sub New(ByVal info As SerializationInfo, ByVal ctx As StreamingContext)
        MyBase.New(info, ctx)
    End Sub

    Public Sub New(ByVal formattedMsg As String, ByVal ParamArray args() As Object)
        MyBase.New(formattedMsg, args)
    End Sub

End Class