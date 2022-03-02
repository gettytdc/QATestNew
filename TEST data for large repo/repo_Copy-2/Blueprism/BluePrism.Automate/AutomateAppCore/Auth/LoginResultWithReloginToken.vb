Imports System.Runtime.Serialization
Imports BluePrism.Common.Security

Namespace Auth

    <DataContract([Namespace]:="bp")>
    <Serializable>
    Public Class LoginResultWithReloginToken
        <DataMember>
        Private Readonly mLoginResult As LoginResult

        <DataMember>
        Private ReadOnly mReloginToken As SafeString

        Public Readonly Property LoginResult As LoginResult
            Get
                Return mLoginResult
            End Get
        End Property
	    
        Public ReadOnly Property ReloginToken As SafeString
            Get
                Return mReloginToken
            End Get
        End Property

        Public Sub New(loginResultCode As LoginResultCode)
            Me.New(New LoginResult(loginResultCode), Nothing)
        End Sub

        Public Sub New(loginResult As LoginResult, reloginToken As SafeString)
            mLoginResult = loginResult
            mReloginToken = reloginToken
        End Sub
    End Class
End Namespace
