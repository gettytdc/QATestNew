Imports BluePrism.AutomateAppCore.Auth

''' <summary>
''' Class to represent a Blue Prism environment
''' </summary>
Public Class BPEnvironment

    Private mDomain As String
    Private mRoles As RoleSet
    Private mUser As User

    Public Property Roles() As RoleSet
        Get
            Return mRoles

        End Get
        Set(ByVal value As RoleSet)
            mRoles = value
        End Set
    End Property

    Public ReadOnly Property IsActiveDirectory() As Boolean
        Get
            Return (mDomain IsNot Nothing)
        End Get
    End Property

    Public ReadOnly Property ActiveDirectoryDomain() As String
        Get
            Return mDomain
        End Get
    End Property

    Public ReadOnly Property IsLoggedIn() As Boolean
        Get
            Return (mUser IsNot Nothing)
        End Get
    End Property

    Public ReadOnly Property User() As User
        Get
            Return mUser
        End Get
    End Property


End Class
