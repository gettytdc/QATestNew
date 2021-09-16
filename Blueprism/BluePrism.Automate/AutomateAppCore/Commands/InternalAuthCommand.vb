Imports BluePrism.AutomateAppCore.Auth

Namespace Commands

    ''' <summary>
    ''' The command used to exchange an authentication token
    ''' </summary>
    Public Class InternalAuthCommand
        Inherits CommandBase

        Public Sub New(client As IListenerClient, listener As IListener, server As IServer, memberPermissionsFactory As Func(Of IGroupPermissions, IMemberPermissions))
            MyBase.New(client, listener, server, memberPermissionsFactory)
        End Sub

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "internalauth"
            End Get
        End Property

        Public Overrides ReadOnly Property Help() As String
            Get
                Return My.Resources.InternalAuthCommand_UseInternalauthTokenToPerformTokenBasedAuthenticationThisUsesASingleUseTokenGen
            End Get
        End Property

        Public Overrides ReadOnly Property CommandAuthenticationRequired() As CommandAuthenticationMode
            Get
                Return CommandAuthenticationMode.Any
            End Get
        End Property

        Public Overrides ReadOnly Property ValidRunStates As IEnumerable(Of ResourcePcRunState)
            Get
                Return AllRunStates
            End Get
        End Property

        Protected Overrides Function Exec(command As String) As String

            'serialised tokens have the format 'userid tokenvalue', each part being a guid
            Dim serialisedToken As String = command.Substring(Name.Length + 1, 2 * (Guid.NewGuid).ToString.Length + 1)

            'Check the token
            Dim token As New clsAuthToken(serialisedToken)
            Dim invalidReason As String = Nothing
            Dim u = token.Validate(invalidReason)
            If u Is Nothing Then _
                Return "INVALID TOKEN - " & invalidReason

            If Not Listener.ResourceOptions.IsPublic AndAlso
                gSv.TryGetUserAttrib(token.OwningUserID) = BluePrism.Server.Domain.Models.AuthMode.System Then
                Client.AuthenticatedUser = u
                Return "AUTH ACCEPTED" & vbCrLf
            End If

            'Check if the requested user is allowed access to the Resource PC. Either
            'it is open to anyone, or it's restricted to a specific user...
            If Not Listener.ResourceOptions.IsPublic AndAlso Listener.UserId <> token.OwningUserID Then _
            Return "RESTRICTED : " & Listener.UserName

            If Not CheckUserHasPermissionOnResource(u, Listener.ResourceId, Permission.Resources.ImpliedViewResource) Then
                Return "USER DOES NOT HAVE ACCESS TO THIS RESOURCE"
            End If

            Client.AuthenticatedUser = u
            Return "AUTH ACCEPTED" & vbCrLf

        End Function

    End Class
End NameSpace