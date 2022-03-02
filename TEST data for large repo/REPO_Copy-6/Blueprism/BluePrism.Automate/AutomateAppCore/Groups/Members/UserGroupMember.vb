Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.Images
Imports BluePrism.BPCoreLib
Imports System.Runtime.Serialization
Imports BluePrism.Server.Domain.Models

Namespace Groups
    ''' <summary>
    ''' Represents users within a group
    ''' </summary>
    <Serializable, DataContract([Namespace]:="bp")>
    Public Class UserGroupMember : Inherits GroupMember

#Region " Class-scope Declarations "

        ''' <summary>
        ''' Inner class to hold the data names for the properties in this class
        ''' </summary>
        Private Class DataNames
            Public Const ValidFrom As String = "ValidFrom"
            Public Const ValidTo As String = "ValidTo"
            Public Const PasswordExpiry As String = "PasswordExpiry"
            Public Const LastSignIn As String = "LastSignedIn"
            Public Const IsDeleted As String = "IsDeleted"
            Public Const MaxLoginAttempts As String = "MaxLoginAttempts"
            Public Const LoginAttempts As String = "LoginAttempts"
            Public Const RoleIds As String = "RoleIds"
            Public Const AuthType As String = "AuthType"
        End Class

        ''' <summary>
        ''' A predicate which allows only those resources which are true users (i.e.
        ''' excludes system user accounts) which are not deleted.
        ''' </summary>
        Public Shared ReadOnly Property ActiveNonSystem As Predicate(Of IGroupMember)
            Get
                Return _
                    Function(m)
                        Dim rm = TryCast(m, UserGroupMember)
                        If rm Is Nothing Then Return False
                        Return (Not rm.IsDeleted AndAlso rm.AuthType <> AuthMode.System AndAlso rm.AuthType <> AuthMode.Anonymous)
                    End Function
            End Get
        End Property

        ''' <summary>
        ''' A predicate which allows only those resources which are true users (i.e.
        ''' excludes system user accounts), but includes deleted users.
        ''' </summary>
        Public Shared ReadOnly Property AllNonSystem As Predicate(Of IGroupMember)
            Get
                Return _
                    Function(m)
                        Dim rm = TryCast(m, UserGroupMember)
                        If rm Is Nothing Then Return False
                        Return (rm.AuthType <> AuthMode.System AndAlso rm.AuthType <> AuthMode.Anonymous)
                    End Function
            End Get
        End Property
        Public ReadOnly Property AuthenticationServiceAccount As Boolean
            Get
                Return AuthType = AuthMode.AuthenticationServer OrElse
                       AuthType = AuthMode.AuthenticationServerServiceAccount
            End Get
        End Property
#End Region

#Region " Constructors "

        ''' <summary>
        ''' Creates a new user group member using data from a provider.
        ''' </summary>
        ''' <param name="prov">The provider of the data to initialise this group
        ''' member with - this expects: <list>
        ''' <item>id: Integer: The ID of the user</item>
        ''' <item>username: String: The user's name (or UPN for AD users)</item>
        ''' <item>validfrom</item>
        ''' <item>name: String: The user's name (or UPN for AD users)</item>
        ''' <item>validfrom</item>
        ''' <item>validfrom: DateTime: The date that the user is valid from.</item>
        ''' <item>validto: DateTime: The date that the user is valid to.</item>
        ''' <item>passwordexpiry: DateTime: The date that the password expires</item>
        ''' <item>lastsignedin: DateTime: The last time that the user successfully
        ''' signed into this environment</item>
        ''' <item>isdeleted: Boolean: True to indicate a deleted user; False to
        ''' indicate an active user</item>
        ''' <item>loginattempts: Integer: The number of failed login attempts for the
        ''' user.</item>
        ''' <item>maxloginattempts: Integer: The number of failed login attempts
        ''' allowed for a user in this environment before the user is locked.</item>
        ''' <item>roleid: Integer: The ID of a role associated with this user. This
        ''' is also used in the <see cref="AppendFrom"/> override in place in this
        ''' class.</item>
        ''' </list>
        ''' </param>
        Public Sub New(prov As IDataProvider)
            MyBase.New(prov)
            ValidFrom = prov.GetValue("validfrom", Date.MinValue)
            ValidTo = prov.GetValue("validto", Date.MaxValue)
            PasswordExpiry = prov.GetValue("passwordexpiry", Date.MaxValue)
            LastSignIn = prov.GetValue("lastsignedin", Date.MinValue)
            IsDeleted = prov.GetBool("isdeleted")
            LoginAttempts = prov.GetInt("loginattempts")
            MaxLoginAttempts = prov.GetInt("maxloginattempts", Integer.MaxValue)
            AuthType = prov.GetValue("authtype", AuthMode.Unspecified)
            AppendFrom(prov)
        End Sub

        ''' <summary>
        ''' Creates a new, empty user group member
        ''' </summary>
        Public Sub New()
            Me.New(NullDataProvider.Instance)
        End Sub

        ''' <summary>
        ''' Creates a new queue group member based on data from the given user
        ''' </summary>
        ''' <param name="u">The user which contains the data from which this
        ''' group member should draw its values</param>
        Public Sub New(u As User)
            Me.New(NullDataProvider.Instance)
            Id = u.Id
            Name = u.Name
            ValidFrom = u.Created
            ValidTo = u.Expiry
            PasswordExpiry = u.PasswordExpiry
            LastSignIn = u.LastSignedInAt
            IsDeleted = u.Deleted
            AuthType = u.AuthType
            ' We don't get this data from the user so we have to assume 'not locked'
            LoginAttempts = 0
            MaxLoginAttempts = Integer.MaxValue
        End Sub

#End Region

#Region " Properties "

#Region " Associated Data Properties "

        ''' <summary>
        ''' The date the user's account was created.
        ''' </summary>
        <DataMember>
        Public Property ValidFrom As Date
            Get
                Return GetData(DataNames.ValidFrom, Date.MinValue)
            End Get
            Set(value As Date)
                SetData(DataNames.ValidFrom, value)
            End Set
        End Property

        ''' <summary>
        ''' The date the user's account is valid until.
        ''' </summary>
        <DataMember>
        Public Property ValidTo As Date
            Get
                Return GetData(DataNames.ValidTo, Date.MaxValue)
            End Get
            Set(value As Date)
                SetData(DataNames.ValidTo, value)
            End Set
        End Property

        ''' <summary>
        ''' The date on which the user last signed in.
        ''' </summary>
        <DataMember>
        Public Property LastSignIn As Date
            Get
                Return GetData(DataNames.LastSignIn, Date.MinValue)
            End Get
            Set(value As Date)
                SetData(DataNames.LastSignIn, value)
            End Set
        End Property

        ''' <summary>
        ''' True to indicate that the user's account has been deleted from Blue Prism.
        ''' </summary>
        <DataMember>
        Public Property IsDeleted As Boolean
            Get
                Return GetData(DataNames.IsDeleted, False)
            End Get
            Set(value As Boolean)
                SetData(DataNames.IsDeleted, value)
            End Set
        End Property

        ''' <summary>
        ''' The date on which the user's password will expire
        ''' </summary>
        <DataMember>
        Public Property PasswordExpiry As Date
            Get
                Return GetData(DataNames.PasswordExpiry, Date.MaxValue)
            End Get
            Set(value As Date)
                SetData(DataNames.PasswordExpiry, value)
            End Set
        End Property

        ''' <summary>
        ''' The maximum number of login attempts allowed for this user. This should
        ''' be constant across all users at any point in time. If the allowed number
        ''' of login attempts changes in the lifetime of this group member, that
        ''' change will not be effected in this object until it is refreshed.
        ''' </summary>
        <DataMember>
        Public Property MaxLoginAttempts As Integer
            Get
                Return GetData(DataNames.MaxLoginAttempts, 3)
            End Get
            Set(value As Integer)
                SetData(DataNames.MaxLoginAttempts, value)
            End Set
        End Property

        ''' <summary>
        ''' The number of unsuccessful login attempts that this user has made .
        ''' </summary>
        <DataMember>
        Public Property LoginAttempts As Integer
            Get
                Return GetData(DataNames.LoginAttempts, 0)
            End Get
            Set(value As Integer)
                SetData(DataNames.LoginAttempts, value)
            End Set
        End Property

        ''' <summary>
        ''' The role IDs associated with this user in a collection. Note that this
        ''' collection is modifiable and settable. If set with a collection, the
        ''' data will be copied into a collection which used within this group member
        ''' only.
        ''' </summary>
        <DataMember>
        Public Property RoleIds As ICollection(Of Integer)
            Get
                Dim roleSet As SortedSet(Of Integer) = Nothing
                Dim curr = GetData(DataNames.RoleIds, roleSet)
                If curr Is Nothing Then
                    curr = New SortedSet(Of Integer)
                    SetData(DataNames.RoleIds, curr)
                End If
                Return curr
            End Get
            Set(value As ICollection(Of Integer))
                SetData(DataNames.RoleIds, New SortedSet(Of Integer)(value))
            End Set
        End Property

        <DataMember>
        Public Property AuthType As AuthMode
            Get
                Return GetData(DataNames.AuthType, AuthMode.Unspecified)
            End Get
            Set(value As AuthMode)
                SetData(DataNames.AuthType, value)
            End Set
        End Property

#End Region

        ''' <summary>
        ''' Gets the names of the user roles associated with this user.
        ''' </summary>
        Public ReadOnly Property RoleNames As IEnumerable(Of String)
            Get
                Return Roles.Select(Function(r) r.Name)
            End Get
        End Property

        ''' <summary>
        ''' Gets the names of the user roles associated with this user joined into a
        ''' single string.
        ''' </summary>
        Public ReadOnly Property RoleNamesJoined As String
            Get
                Return String.Join(", ", RoleNames)
            End Get
        End Property

        ''' <summary>
        ''' Gets the current set of roles associated with this user.
        ''' </summary>
        Public ReadOnly Property Roles As RoleSet
            Get
                Dim rs As New RoleSet()
                If ServerFactory.ServerAvailable Then rs.AddAll(
                    RoleIds.Select(Function(id) SystemRoleSet.Current(id))
                )
                Return rs
            End Get
        End Property

        ''' <summary>
        ''' Indicates whether this user is locked or not
        ''' </summary>
        Public Overrides ReadOnly Property IsLocked As Boolean
            Get
                Return LoginAttempts >= MaxLoginAttempts
            End Get
        End Property

        ''' <summary>
        ''' Gets the valid from date/time in local time as a display value.
        ''' </summary>
        Public ReadOnly Property ValidFromDisplay As String
            Get
                Return BPUtil.ConvertAndFormatUtcDateTime(ValidFrom)
            End Get
        End Property

        ''' <summary>
        ''' Gets the valid to date/time in local time as a display value.
        ''' </summary>
        ''' <remarks>ValidTo is apparently stored as local time.</remarks>
        Public ReadOnly Property ValidToDisplay As String
            Get
                Return If(ValidTo = Nothing, "", ValidTo.ToString("d"))
            End Get
        End Property

        ''' <summary>
        ''' Gets the last signed in date/time in local time as a display value.
        ''' Note that if the user has never signed in, this will return an empty
        ''' string.
        ''' </summary>
        Public ReadOnly Property LastSignInDisplay As String
            Get
                Return BPUtil.ConvertAndFormatUtcDateTime(LastSignIn)
            End Get
        End Property

        ''' <summary>
        ''' Gets the password expiry date/time in local time as a display value.
        ''' </summary>
        ''' <remarks>PasswordExpiry is apparently stored as local time.</remarks>
        Public ReadOnly Property PasswordExpiryDisplay As String
            Get
                Return If(PasswordExpiry = Nothing, "", PasswordExpiry.ToString("d"))
            End Get
        End Property

        ''' <summary>
        ''' The linking table between nodes of this type and groups. In this case,
        ''' the table is <c>BPAGroupUser</c>.
        ''' </summary>
        Public Overrides ReadOnly Property LinkTableName As String
            Get
                Return "BPAGroupUser"
            End Get
        End Property

        ''' <summary>
        ''' The image key to use for this object. Different images used for deleted
        ''' and locked users.
        ''' </summary>
        Public Overrides ReadOnly Property ImageKey As String
            Get
                If IsDeleted Then
                    Return If(AuthType = AuthMode.AuthenticationServerServiceAccount,
                              ImageLists.Keys.Component.ServiceAccount_Disabled,
                              ImageLists.Keys.Component.User_Deleted)
                End If

                If IsLocked Then
                    Return ImageLists.Keys.Component.User_Locked
                End If

                Return If(AuthType = AuthMode.AuthenticationServerServiceAccount,
                          ImageLists.Keys.Component.ServiceAccount,
                          ImageLists.Keys.Component.User)
            End Get
        End Property

        ''' <summary>
        ''' The group member type represented by this class
        ''' </summary>
        Public Overrides ReadOnly Property MemberType As GroupMemberType
            Get
                Return GroupMemberType.User
            End Get
        End Property

#End Region

#Region " Methods "

        ''' <summary>
        ''' Resets the locked state of this group member. Does nothing if this member
        ''' has no 'locked' concept or if it is not currently locked.
        ''' </summary>
        Public Overrides Sub ResetLock()
            LoginAttempts = 0
        End Sub

        ''' <summary>
        ''' Appends extra data relating to this user group member into this object.
        ''' In this case, a role ID of a role that the user is associated with.
        ''' </summary>
        ''' <param name="prov">The provider from which to draw the appending data.
        ''' The expected data in the provider is:
        ''' <list>
        ''' <item>roleid: Integer: The ID of a role associated with this user.</item>
        ''' </list></param>
        Public Overrides Sub AppendFrom(prov As IDataProvider)
            MyBase.AppendFrom(prov)
            RoleIds.Add(prov.GetInt("roleid"))
        End Sub

#End Region

    End Class

End Namespace
