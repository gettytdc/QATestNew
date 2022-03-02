Namespace clsServerPartialClasses.AuthenticationServerUserMapping
    Public Enum UserMappingResultCode
        None = 0
        BluePrismUsersAuthTypeDoesNotSupportMapping = 1
        BluePrismUserNotFound = 2
        BluePrismUserDeleted = 3
        CannotMapSystemUser = 4
        UnexpectedError = 5
        AuthenticationServerUserNotLoaded = 6
        ErrorCreatingAuthenticationServerUserRecord = 7
        MissingMappingRecordValues = 8
        BluePrismUserHasAlreadyBeenMapped = 9
        AuthenticationServerUserAlreadyMappedToAnotherUser = 10
    End Enum
End NameSpace
