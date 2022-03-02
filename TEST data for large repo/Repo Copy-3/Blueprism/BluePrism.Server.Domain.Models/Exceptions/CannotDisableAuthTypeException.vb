Imports System.Runtime.Serialization

<Serializable()> _
Public Class CannotDisableAuthTypeException : Inherits BluePrismException
        
    Public ReadOnly AuthType As AuthMode
    Public Sub New(authType As AuthMode)
        MyBase.New()
        Me.AuthType = authType
    End Sub

    Public Sub New(authType As AuthMode, msg As String)
        MyBase.New(msg)
        Me.AuthType = authType
    End Sub

    Public Sub New(authType As AuthMode, formattedMsg As String, ParamArray args() As Object)
        MyBase.New(formattedMsg, args)
        Me.AuthType = authType
    End Sub       

    Protected Sub New(info As SerializationInfo, ctx As StreamingContext)
        MyBase.New(info, ctx)
        Dim authType = info.GetInt32(NameOf(Me.AuthType))
        If Not [Enum].IsDefined(GetType(AuthMode), authType) Then Throw New InvalidArgumentException(NameOf(info))
        Me.AuthType = CType(authType, AuthMode)
    End Sub

    Public Overrides Sub GetObjectData(info As SerializationInfo, context As StreamingContext)
        If info Is Nothing Then Throw New ArgumentNullException(NameOf(info))
        
        info.AddValue(NameOf(AuthType), AuthType)
        MyBase.GetObjectData(info, context)
    End Sub
End Class
