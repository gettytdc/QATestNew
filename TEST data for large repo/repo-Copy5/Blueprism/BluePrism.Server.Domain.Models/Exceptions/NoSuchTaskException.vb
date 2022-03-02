Imports System.Runtime.Serialization

<Serializable>
Public Class NoSuchTaskException : Inherits NoSuchElementException
    Public Sub New(taskId As Integer)
        MyBase.New("The task with id '{0}' was not found", taskId)
    End Sub

    Protected Sub New(info As SerializationInfo, ctx As StreamingContext)
        MyBase.New(info, ctx)
    End Sub
End Class
