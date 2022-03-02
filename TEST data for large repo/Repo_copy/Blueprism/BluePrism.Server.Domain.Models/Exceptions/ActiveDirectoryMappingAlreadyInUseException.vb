Imports System.Runtime.Serialization

Public Class ActiveDirectoryMappingAlreadyInUseException : Inherits AlreadyExistsException

    Public Sub New()
        MyBase.New()
    End Sub

    Public Sub New(ByVal formattedMsg As String, ByVal ParamArray args() As Object)
        MyBase.New(formattedMsg, args)
    End Sub

    Public Sub New(ByVal info As SerializationInfo, ByVal ctx As StreamingContext)
        MyBase.New(info, ctx)
    End Sub

End Class
