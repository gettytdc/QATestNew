Imports System.Runtime.Serialization

''' <summary>
''' The types of authentication under which the current user may have logged in.
''' </summary>
<DataContract([Namespace]:="bp")>
Public Enum AuthMode

    ''' <summary>
    ''' No authentication mode
    ''' </summary>
    <EnumMember> Unspecified = 0

    ''' <summary>
    ''' Native authentication - ie. it uses a Blue Prism username and password.
    ''' This is only available in a Native/External login database.
    ''' </summary>
    <EnumMember> Native = 1

    ''' <summary>
    ''' Active Directory authentication. For authorization, it uses the AD user's
    ''' membership of Blue Prism security groups in Active Directory. This is only
    ''' available in a Single Sign On database.
    ''' </summary>
    <EnumMember> ActiveDirectory = 2

    ''' <summary>
    ''' Anonymous authentication
    ''' </summary>
    <EnumMember> Anonymous = 3

    ''' <summary>
    ''' System
    ''' </summary>
    <EnumMember> System = 4

    ''' <summary>
    ''' External ID Provider - ie. using an access token returned by logging in
    ''' via the authentication gateway with credentials from an external ID provider.
    ''' This is only available in a Native/External login database.
    ''' </summary>
    <EnumMember> External = 5

    ''' <summary>
    ''' Active Directory authentication, but where the AD user needs to be mapped 
    ''' to a Blue Prism user. For authorization, it uses the roles assigned to 
    ''' the Blue Prism user. This is only available in a Native/External login 
    ''' database.
    ''' </summary>
    ''' 
    <EnumMember> MappedActiveDirectory = 6
    ''' <summary>
    ''' Authenticated via the Authentication server.
    ''' </summary>
    ''' 
    <EnumMember> AuthenticationServer = 7

    ''' <summary>
    ''' Authenticated via the Authentication server service account.
    ''' </summary>
    <EnumMember> AuthenticationServerServiceAccount = 8

End Enum
