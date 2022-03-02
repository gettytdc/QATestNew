''' <summary>
''' Possible result codes. Each entry here should also be given a friendly
''' description in GetDescription.
''' </summary>
Public Enum LoginResultCode
    Success                         'Login succeeded
    PasswordExpired                 'Password has expired
    TypeMismatch                    'Wrong login type attempted
    BadCredentials                  'Bad credentials were given
    Already                         'Already logged in
    Internal                        'An internal error occurred
    Deleted                         'User deleted
    AccountExpired                  'The account has expired
    BadAccount                      'Bad account type
    NoGroups                        'No relevant group membership
    NotAuthenticated                'Windows login not authenticated
    AttemptsExceeded                'The maximum number of login attempts has been exceeded
    MissingUPN                      'There is no User Principal Name associated with this account in AD'
    AnonymousDisabled               'Anonymous resourcepc logins are disabled
    InvalidAccessToken              'The JWT was not valid, or did not contain a user Id
    UnableToFindUser                'Could not find the user in the database
    UnableToFindServiceAccountUser                'Could not find the user in the database
    UnableToValidateClientIdentity  'Using a client/server connection that does not validate client identity
    NoMappedActiveDirectoryUser     'No Blue Prism user mapped to the client's Active Directory user
    ComputerNameNotSet
    MissingPassword
    InvalidReloginToken             'The Relogin token was not valid
End Enum
