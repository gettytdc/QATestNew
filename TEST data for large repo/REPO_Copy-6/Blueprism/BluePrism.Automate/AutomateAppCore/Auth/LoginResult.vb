Imports System.Runtime.Serialization

Namespace Auth

    ''' <summary>
    ''' Describes the result of a login attempt.
    ''' </summary>
    <DataContract([Namespace]:="bp")>
    <Serializable>
    <KnownType(GetType(User))>
    Public Class LoginResult

#Region " Member Variables "

        ''' <summary>
        ''' The result code.
        ''' </summary>
        <DataMember>
        Public Code As LoginResultCode

        ''' <summary>
        ''' The user loaded after a successful login
        ''' </summary>
        <DataMember>
        Private mUser As IUser

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Creates a new LoginResult
        ''' </summary>
        ''' <param name="result">The result code for the login</param>
        Public Sub New(ByVal result As LoginResultCode)
            Me.New(result, Nothing)
        End Sub

        ''' <summary>
        ''' Creates a new LoginResult
        ''' </summary>
        ''' <param name="result">The result code for the login</param>
        ''' <param name="user">The logged in user after a successful login</param>
        Public Sub New(ByVal result As LoginResultCode, ByVal user As IUser)
            Code = result
            mUser = user
        End Sub

#End Region

#Region " Properties "

        ''' <summary>
        ''' Get a description of the login result, for displaying to the user.
        ''' </summary>
        Public ReadOnly Property Description() As String
            Get
                Dim desc As String
                Select Case Code
                    Case LoginResultCode.Success
                        desc = My.Resources.LoginResult_Success
                    Case LoginResultCode.PasswordExpired
                        desc = My.Resources.LoginResult_PasswordExpired
                    Case LoginResultCode.Already
                        desc = My.Resources.LoginResult_YouAreAlreadyLoggedIn
                    Case LoginResultCode.AccountExpired
                        desc = My.Resources.LoginResult_ThisAccountHasExpiredPleaseContactYourSystemAdministrator
                    Case LoginResultCode.Internal
                        desc = My.Resources.LoginResult_AnInternalOrConnectionErrorOccurred
                    Case LoginResultCode.Deleted
                        desc = My.Resources.LoginResult_ThisAccountHasBeenTerminatedPleaseContactYourSystemAdministrator
                    Case LoginResultCode.TypeMismatch
                        desc = My.Resources.LoginResult_TheWrongLoginTypeWasUsedPleaseContactYourSystemAdministrator
                    Case LoginResultCode.BadCredentials
                        desc = My.Resources.IncorrectLoginException_TheUsernameOrPasswordIsIncorrectPleaseTryAgain
                    Case LoginResultCode.BadAccount
                        desc = My.Resources.LoginResult_LocalSystemAccountsGuestAccountsAndAnonymousUserCredentialsAreNotCompatibleWith
                    Case LoginResultCode.NoGroups
                        desc = My.Resources.LoginResult_YouAreNotInAnyGroupsThatAreAuthorisedToLogIn
                    Case LoginResultCode.NotAuthenticated
                        desc = My.Resources.LoginResult_YourWindowsLoginIsNotAuthenticated
                    Case LoginResultCode.AttemptsExceeded
                        desc = My.Resources.LoginResult_TheMaximumNumberOfLoginAttemptsHasBeenExceeded
                    Case LoginResultCode.MissingUPN
                        desc = My.Resources.LoginResult_ThereIsNoUPNAssociatedWithThisAccountInActiveDirectoryPleaseContactYourSystemAd
                    Case LoginResultCode.AnonymousDisabled
                        desc = My.Resources.LoginResult_AnonymousResourcepcLoginsAreDisabled
                    Case LoginResultCode.UnableToFindUser
                        desc = My.Resources.LoginResult_CouldNotFindTheUser
                    Case LoginResultCode.InvalidAccessToken
                        desc = My.Resources.LoginResult_InvalidAccessToken
                    Case LoginResultCode.UnableToValidateClientIdentity
                        desc = String.Format(My.Resources.LoginResult_UnableToValidateClientIdentity, 
                            My.Resources.ServerConnection_WCFSOAPWithMessageEncryptionWindowsAuthentication,
                            My.Resources.ServerConnection_WCFSOAPWithTransportEncryptionWindowsAuthentication,
                            My.Resources.ServerConnection_NETRemotingSecure)
                    Case LoginResultCode.NoMappedActiveDirectoryUser
                        desc = My.Resources.LoginResult_NoMappedActiveDirectoryUser
                    Case LoginResultCode.ComputerNameNotSet
                        desc = My.Resources.LoginResult_ComputerNameNotSet
                    Case LoginResultCode.MissingPassword
                        desc = My.Resources.LoginResult_MissingPassword
                    Case LoginResultCode.InvalidReloginToken
                        desc = My.Resources.LoginResult_InvalidReloginToken
                    Case Else
                        desc = My.Resources.LoginResult_UnknownFailure
                End Select
                Return desc
            End Get
        End Property

        ''' <summary>
        ''' The user from the login attempt
        ''' </summary>
        Public ReadOnly Property User() As IUser
            Get
                Return mUser
            End Get
        End Property

        ''' <summary>
        ''' The UserId of the logged in user, or Guid.Empty if the login was not a
        ''' <see cref="IsSuccess">success</see>
        ''' </summary>
        Public ReadOnly Property UserID() As Guid
            Get
                If mUser Is Nothing Then Return Guid.Empty Else Return mUser.Id
            End Get
        End Property

        ''' <summary>
        ''' Gets whether the login result represents a success or not.
        ''' </summary>
        Public ReadOnly Property IsSuccess() As Boolean
            Get
                Return Code = LoginResultCode.Success
            End Get
        End Property

        ''' <summary>
        '''  Gets whether the login result represents a state where a password update is allowed
        '''  which is when either the login worked with no problems, or the user's password has
        '''  expired.
        ''' </summary>
        Public ReadOnly Property IsPasswordUpdateAllowed As Boolean
            Get
                Return (Code = LoginResultCode.Success OrElse
                        Code = LoginResultCode.PasswordExpired)
            End Get
        End Property
#End Region

    End Class

End Namespace
