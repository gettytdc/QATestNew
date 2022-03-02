Imports BluePrism.AutomateAppCore.Auth
Imports System.Runtime.Serialization

''' Project  : AutomateAppCore
''' Class    : clsAuthToken
''' 
''' <summary>
''' Represents a Blue Prism authorisation token
''' </summary>
''' <remarks>Authorisation tokens are registered in the database when a command
''' requiring authentication is sent. The receiver then validates the token
''' (checking it is present in the database and valid) before executing the
''' command. This replaces the scheme of passing a username and password.</remarks>
<Serializable>
<DataContract([Namespace]:="bp")>
Public Class clsAuthToken

    ''' <summary>
    ''' Creates a new authorisation token for the passed user.
    ''' </summary>
    Friend Sub New(owningUserId As Guid, token As Guid, expiryTime As DateTime, processId As Guid)
        mOwningUserID = owningUserId
        mToken = token
        mExpiryTime = expiryTime
        Me.ProcessId = processId
    End Sub

    ''' <summary>
    ''' Constructs an authorisation token object from the passed token value.
    ''' </summary>
    ''' <param name="tokenValue">The token value</param>
    Public Sub New(tokenValue As String)
        'The format of the token is "{UserID}_{InternalTokenValue}". Neither
        'UserID nor InternalTokenValue may contain spaces.
        If String.IsNullOrEmpty(tokenValue) Then
            Throw New ArgumentException(My.Resources.clsAuthToken_ValueCanNotBeNullOrHaveZeroLength, "TokenValue")
        End If
        Dim parts As String() = tokenValue.Split("_"c)
        If parts.Length <> 2 Then
            Throw New ArgumentException(My.Resources.clsAuthToken_TokenFormatMustBeUserID_InternalTokenValue)
        End If

        mOwningUserID = New Guid(parts(0))
        mToken = New Guid(parts(1))
    End Sub

    ''' <summary>
    ''' The moment in time at which the token will expire.
    ''' </summary>
    Public ReadOnly Property ExpiryTime() As DateTime
        Get
            Return mExpiryTime
        End Get
    End Property
    <DataMember>
    Private mExpiryTime As DateTime

    ''' <summary>
    ''' The ID of the user who owns this token
    ''' </summary>
    Public ReadOnly Property OwningUserID() As Guid
        Get
            Return mOwningUserID
        End Get
    End Property
    <DataMember>
    Private mOwningUserID As Guid

    <DataMember>
    Public Property ProcessId As Guid

    ''' <summary>
    ''' The time for which a token is valid by default, in seconds.
    ''' </summary>
    Public Const DefaultTokenExpiryInterval As Integer = 300

    ''' <summary>
    ''' The internal representation of the token's value.
    ''' </summary>
    ''' <remarks>Clients of this class should not need to know or care of the token's
    ''' internal representation, or value (except when de/serialising the token for
    ''' exchange over the network).</remarks>
    Public ReadOnly Property Token As Guid
        Get
            Return mToken
        End Get
    End Property
    <DataMember>
    Private mToken As Guid

    ''' <summary>
    ''' The length of the token which is 2 guid strings and an underscore
    ''' </summary>
    Friend Const TokenLength As Integer = 73

    ''' <summary>
    ''' Serialises the token to a string value
    ''' </summary>
    Public Overrides Function ToString() As String
        Return $"{mOwningUserID}_{Token}"
    End Function

    ''' <summary>
    ''' Determines whether the token is valid (eg checks expiry, etc)
    ''' </summary>
    ''' <param name="invalidReason">When invalid, carries back a reason.</param>
    ''' <returns>Returns True if the token is valid, False otherwise.</returns>
    Public Function Validate(ByRef invalidReason As String) As User
        Try
            Return gSv.ValidateAuthorisationToken(Me, invalidReason)
        Catch ex As Exception
            Throw New InvalidOperationException("Unable to validate token - " & ex.Message)
        End Try
    End Function

    ''' <summary>
    ''' Compares this token to another.
    ''' </summary>
    ''' <param name="obj">The token to which to compare this one.</param>
    ''' <returns>Returns true if the two tokens are exactly equal.</returns>
    Public Overrides Function Equals(ByVal obj As Object) As Boolean
        Dim otherToken As clsAuthToken = TryCast(obj, clsAuthToken)
        If otherToken IsNot Nothing Then
            Return otherToken.Token.Equals(Token)
        Else
            Return False
        End If
    End Function

End Class
