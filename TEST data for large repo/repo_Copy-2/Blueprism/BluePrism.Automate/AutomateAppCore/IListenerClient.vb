Imports System.Net
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateProcessCore
Imports BluePrism.Server.Domain.Models

Public Interface IListenerClient
    Property AuthenticatedUser As IUser
    ReadOnly Property DisconnectMessage As String
    ReadOnly Property HasControlResourcePermission As Boolean
    ReadOnly Property IsErrored As Boolean
    ReadOnly Property RemoteAddress As IPAddress
    ReadOnly Property RemoteAddressFriendlyString As String
    ReadOnly Property RemoteHostIdentity As String
    Property RemoveReason As String
    Property RequestedUserName As String
    Property RequestedAuthenticationMode As AuthMode
    ReadOnly Property UserId As Guid
    ReadOnly Property UserName As String
    Property UserRequested As Boolean
    ReadOnly Property UserSet As Boolean
    Property LastSessionCreated As Guid
    Property StartupParameters As clsArgumentList
    Property IsControllerConnection As Boolean
    Property StartupParametersXml As String
    Property SendSessionVariableUpdates As Boolean
    Property SendStatusUpdates As Boolean
    Property IsExpectingPing As Boolean
    Sub Close()
    Sub Send(buffer() As Byte)
    Function IsDisconnected() As Boolean
    Function HTTPPayloadRecieved() As Boolean
    Function IsConnected() As Boolean
    Function IsLocal() As Boolean
    Function ParseHTTPCommand(command As String, response As ListenerResponse) As Boolean
    Function ReadHTTPPayload() As String
    Function ReadLine() As String
End Interface
