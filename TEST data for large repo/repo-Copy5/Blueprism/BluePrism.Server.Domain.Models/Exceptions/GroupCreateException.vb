Imports System.Runtime.Serialization

<Serializable()>
Public Class GroupCreateException : Inherits BluePrismException

    Public Property NewName As String
    Public Property Owner As Guid

    Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)
        MyBase.New(info, context)
        NewName = info.GetString("NewName")
        Owner = Guid.Parse(info.GetString("Owner"))
    End Sub

    Public Sub New (message As String)
        MyBase.New(message)
    End Sub

    Public Sub New(message As String, newname As String, owner As Guid)
        MyBase.New(message)
        Me.NewName = newname
        Me.Owner = owner
    End Sub

    Public Overrides Sub GetObjectData(info As SerializationInfo, ctx As StreamingContext)
        MyBase.GetObjectData(info, ctx)
        info.AddValue("NewName", NewName.ToString)
        info.AddValue("Owner", Owner.ToString)
    End Sub
End Class
