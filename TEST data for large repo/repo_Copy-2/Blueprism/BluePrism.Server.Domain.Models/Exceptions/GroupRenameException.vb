Imports System.Runtime.Serialization

<Serializable()> _
Public Class GroupRenameException : Inherits BluePrismException

    Public Property GroupName As String

    Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)
        MyBase.New(info, context)
        GroupName = info.getstring("GroupName")
    End Sub

    Public Sub New(message As String, groupName As String)
        MyBase.New(message)
        Me.GroupName = groupName 
    End Sub

    Public Overrides Sub GetObjectData(info As SerializationInfo, ctx As StreamingContext)
        MyBase.GetObjectData(info, ctx)
        info.AddValue("GroupName",GroupName)
    End Sub
End Class
