Imports System.Runtime.Serialization

<Serializable()> _
Public Class InvalidUrlException : Inherits BluePrismException
    Public Sub New()
        MyBase.New()
    End Sub

    Public Sub New(ByVal msg As String)
        MyBase.New(msg)
    End Sub

    Public Sub New(ByVal formattedMsg As String, ByVal ParamArray args() As Object)
        MyBase.New(formattedMsg, args)
    End Sub

    Protected Sub New(ByVal info As SerializationInfo, ByVal ctx As StreamingContext)
        MyBase.New(info, ctx)
    End Sub
End Class
