Imports System.Threading.Tasks

Namespace clsServerPartialClasses.AuthenticationServerUserMapping.AuthenticationServer
    Public Interface IAuthenticationServerHttpRequester
        Function PostUser(record As UserMappingRecord, authenticationServerUrl As String, clientCredential As clsCredential) _
            As Task(Of AuthenticationServerUser)

        Function GetUser(authenticationServerUserId As Guid?, authenticationServerUrl As String, clientCredential As clsCredential) _
            As Task(Of AuthenticationServerUser)
    End Interface
End Namespace
