Imports System.Runtime.CompilerServices
Imports BluePrism.Server.Domain.Models

Namespace Auth

    Public Module AuthModeExtensions

        <Extension()>
        Public Function ToLocalizedDisplayName(authenticationMode As AuthMode) As String
            Select Case authenticationMode
                Case AuthMode.External
                    Return My.Resources.AuthModeExternal_DisplayName
                Case AuthMode.Native
                    Return My.Resources.AuthModeNative_DisplayName
                Case AuthMode.ActiveDirectory, AuthMode.MappedActiveDirectory
                    Return My.Resources.AuthModeActiveDirectory_DisplayName
                Case AuthMode.AuthenticationServer
                    Return My.Resources.AuthModeAuthenticationServer_DisplayName
                Case AuthMode.AuthenticationServerServiceAccount
                    Return My.Resources.AuthModeAuthenticationServerServiceAccount_DisplayName
                Case Else
                    Throw New NotImplementedException($"No localized display name defined for AuthMode: {authenticationMode}")
            End Select
        End Function

    End Module

End Namespace

