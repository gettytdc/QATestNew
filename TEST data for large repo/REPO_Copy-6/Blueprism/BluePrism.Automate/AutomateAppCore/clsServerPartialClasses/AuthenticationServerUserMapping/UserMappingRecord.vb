Imports System.Runtime.Serialization

Namespace clsServerPartialClasses.AuthenticationServerUserMapping

    <Serializable, DataContract([Namespace]:="bp")>
    Public Class UserMappingRecord

        <DataMember>
        Private ReadOnly mBluePrismUsername As String
        <DataMember>
        Private ReadOnly mAuthenticationServerUserId As Guid?
        <DataMember>
        Private ReadOnly mFirstName As String
        <DataMember>
        Private ReadOnly mLastName As String
        <DataMember>
        Private ReadOnly mEmail As String

        Public Sub New(bluePrismUserName As String, authenticationServerUserId As Guid?,
                       firstName As String, lastName As String, email As String)

            mBluePrismUsername = bluePrismUserName
            mAuthenticationServerUserId = authenticationServerUserId
            mFirstName = firstName
            mLastName = lastName
            mEmail = email
        End Sub

        Public ReadOnly Property BluePrismUsername As String
            Get
                Return mBluePrismUsername
            End Get
        End Property

        Public ReadOnly Property AuthenticationServerUserId As Guid?
            Get
                Return mAuthenticationServerUserId
            End Get
        End Property

        Public ReadOnly Property FirstName As String
            Get
                Return mFirstName
            End Get
        End Property

        Public ReadOnly Property LastName As String
            Get
                Return mLastName
            End Get
        End Property

        Public ReadOnly Property Email As String
            Get
                Return mEmail
            End Get
        End Property

    End Class
End Namespace
