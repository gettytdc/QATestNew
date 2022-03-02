Imports BluePrism.BPCoreLib
Imports System.Runtime.Serialization

Namespace Auth

    ''' <summary>
    ''' Values dictating the mode in which the username should be auto-completed
    ''' for an environment
    ''' </summary>
    Public Enum AutoPopulateMode

        ' Don't autocomplete the username
        <FriendlyName("None")>
        None = 0

        ' Autocomplete the username with the logged in windows username
        <FriendlyName("Windows user")>
        SystemUser = 1

        ' Autocomplete the username with the username that the current (windows) user
        ' last used to log into the connection.
        <FriendlyName("Last user")>
        LastUser = 2

    End Enum

    <Serializable()>
    <DataContract([Namespace]:="bp")>
    Public Class LogonOptions
        <DataMember>
        Public AutoPopulate As AutoPopulateMode
        <DataMember>
        Public ShowUserList As Boolean
        <DataMember>
        Public SingleSignon As Boolean
        <DataMember>
        Public AuthenticationGatewayUrl As String
        <DataMember>
        Public MappedActiveDirectoryAuthenticationEnabled As Boolean
        <DataMember>
        Public ExternalAuthenticationEnabled As Boolean
        <DataMember>
        Public AuthenticationServerUrl As String
        <DataMember>
        Public AuthenticationServerAuthenticationEnabled As Boolean
        <DataMember>
        Public AuthenticationServerApiCredentialId As Guid?

        Public Function ShallowCopy() As LogonOptions
            Return CType(Me.MemberwiseClone(), LogonOptions)
        End Function

        Public Overloads Function Equals(other As LogonOptions) As Boolean
            If other IsNot Nothing Then
                Return other.AutoPopulate = Me.AutoPopulate AndAlso
                       other.ShowUserList = Me.ShowUserList AndAlso
                       other.SingleSignon = Me.SingleSignon AndAlso
                       other.AuthenticationGatewayUrl = Me.AuthenticationGatewayUrl AndAlso
                       other.MappedActiveDirectoryAuthenticationEnabled = Me.MappedActiveDirectoryAuthenticationEnabled AndAlso
                       other.ExternalAuthenticationEnabled = Me.ExternalAuthenticationEnabled AndAlso
                       other.AuthenticationServerUrl = Me.AuthenticationServerUrl AndAlso
                       Nullable.Equals(AuthenticationServerApiCredentialId, AuthenticationServerApiCredentialId)
            End If
            Return False
        End Function

    End Class

End Namespace
