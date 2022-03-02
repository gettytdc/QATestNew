Imports System.Runtime.Serialization
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.Common.Security

<DataContract([Namespace]:="bp"), Serializable()>
Public Class NativeAdminUserModel

    <DataMember>
    Public User As User
    <DataMember>
    Public Password As SafeString

    Public Sub New(user As User, password As SafeString)
        Me.User = user
        Me.Password = password
    End Sub
End Class
