Imports System.Runtime.CompilerServices
Imports BluePrism.AutomateAppCore.clsServerPartialClasses.AuthenticationServerUserMapping
Imports AutomateC.My.Resources

Public Module UserMappingResultCodeExtensions
    <Extension()>
    Public Function ToLocalizedDescription(code As UserMappingResultCode) As String
        Dim description = String.Empty

        Select Case code
            Case UserMappingResultCode.BluePrismUsersAuthTypeDoesNotSupportMapping
                description = MapAuthenticationServerUsers_BluePrismUsersAuthTypeDoesNotSupportMapping
            Case UserMappingResultCode.BluePrismUserNotFound
                description = MapAuthenticationServerUsers_BluePrismUserNotFound
            Case UserMappingResultCode.BluePrismUserDeleted
                description = MapAuthenticationServerUsers_BluePrismuserdeleted
            Case UserMappingResultCode.CannotMapSystemUser
                description = MapAuthenticationServerUsers_CannotMapSystemUser
            Case UserMappingResultCode.UnexpectedError
                description = MapAuthenticationServerUsers_Unexpectederror
            Case UserMappingResultCode.AuthenticationServerUserNotLoaded
                description = MapAuthenticationServerUsers_Authenticationserveruserrecordnotloaded
            Case UserMappingResultCode.ErrorCreatingAuthenticationServerUserRecord
                description = MapAuthenticationServerUsers_Errorcreatingauthenticationserveruserrecord
            Case UserMappingResultCode.MissingMappingRecordValues
                description = MapAuthenticationServerUsers_Missingmappingrecordvalues
            Case UserMappingResultCode.BluePrismUserHasAlreadyBeenMapped
                description = MapAuthenticationServerUsers_BluePrismUserHasAlreadyBeenMapped
            Case UserMappingResultCode.AuthenticationServerUserAlreadyMappedToAnotherUser
                description = MapAuthenticationServerUsers_AuthenticationServerUserHasAlreadyBeenMapped
        End Select

        Return description
    End Function
End Module
