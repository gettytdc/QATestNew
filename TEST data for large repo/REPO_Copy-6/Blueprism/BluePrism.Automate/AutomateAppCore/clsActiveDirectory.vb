Option Strict On

Imports System.DirectoryServices
Imports System.DirectoryServices.ActiveDirectory
Imports System.DirectoryServices.AccountManagement
Imports System.Security.Principal
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.BPCoreLib
Imports System.Text.RegularExpressions
Imports LocaleTools
Imports BluePrism.Core.WindowsSecurity
Imports BluePrism.Core.ActiveDirectory
Imports BluePrism.Server.Domain.Models

''' Project  : AutomateAppCore
''' Class    : clsActiveDirectrory
''' 
''' <summary>
''' Class to provide shared utility methods for manipulation and querying of
''' Active Dirctory things.
''' </summary>
Public Class clsActiveDirectory

    Private Const ObjectSidToken As String = "objectSid"

#Region " Group and Member sub-classes "

    ''' <summary>
    ''' Summary information for an Active Directory User
    ''' </summary>
    Public Class ActiveDirectoryUser
        Public Sid As SecurityIdentifier
        Public FullName As String
        Public UserPrincipalName As String
        Public DistinguishedName As String
    End Class

    ''' <summary>
    ''' Summary information for an Active Directory Security Group
    ''' </summary>
    Public Class SecurityGroup
        Public Sid As SecurityIdentifier
        Public Name As String
        Public Scope As GroupScope?
        Public Path As String
        Public Members As IList(Of ActiveDirectoryUser)
        Public ConfigException As Exception

        Public ReadOnly Property GroupType() As String
            Get
                Dim type As String = ""
                If Scope = GroupScope.Universal Then
                    type = My.Resources.SecurityGroup_Universal
                ElseIf Scope = GroupScope.Global Then
                    type = My.Resources.SecurityGroup_Global
                ElseIf Scope = GroupScope.Local Then
                    type = My.Resources.SecurityGroup_Local
                End If
                Return String.Format(My.Resources.SecurityGroup_SecurityGroup0, type)
            End Get
        End Property

        ''' <summary>
        ''' Indicates whether or not the passed security group name or sid matches this
        ''' security group.
        ''' </summary>
        ''' <param name="gpName">The group name or SID to check</param>
        ''' <returns>True if the passed name or SID matches this group</returns>
        Public Function MatchesNameOrSid(gpName As String) As Boolean
            Dim gpSid = TryParseSID(gpName)
            If gpSid IsNot Nothing Then Return Sid.Equals(gpSid)
            Return Name.Equals(gpName)
        End Function
    End Class

#End Region

    ''' <summary>
    ''' Checks if a given host name is a valid DNS name or not. This mostly uses the
    ''' definition in http://www.ietf.org/rfc/rfc952.txt. Specifically, it ensures:
    ''' <list>
    ''' <item>the name is a text string drawn from the alphabet (A-Z), digits (0-9),
    ''' minus sign (-), and period (.)</item>
    ''' <item>periods are only allowed when they serve to delimit components of
    ''' "domain style names" (I've taken this to mean that 'domain style names' can
    ''' start and finish with a hyphen, but cannot consist only of hyphens. This may
    ''' be inaccurate, but if so, it falls on the side of too permissive, so it will
    ''' just not filter out invalid names before it tries to use them, rather than
    ''' incorrectly denying valid names).</item>
    ''' <item>No blank or space characters are permitted as part of a name.</item>
    ''' <item>No distinction is made between upper and lower case.</item>
    ''' <item>The first character must be an alpha character.</item>
    ''' <item>The last character must not be a minus sign. Note that rfc952 says that
    ''' the last character should also not be a period; however, rfc1034 (see
    ''' http://www.ietf.org/rfc/rfc1034.txt) makes it clear that a trailing "." char
    ''' in a domain name is valid, documenting that it details an 'absolute' domain
    ''' name over a 'relative' domain name. Thus, this method allows a trailing "."
    ''' char.</item>
    ''' <item>Single character names are not allowed.</item>
    ''' </list>
    ''' </summary>
    ''' <param name="hostName">The host name to test for validity</param>
    ''' <returns>True if the given host name is a valid DNS names according to the
    ''' rules described in this method; False otherwise. Note that <c>null</c>, or an
    ''' empty string are never considered a valid host name and that any whitespace
    ''' will also render a hostname invalid.</returns>
    ''' <remarks><para>Note that this omits some of the rules in rfc952, namely:
    ''' It does not ensure a maximum string length of 24 characters;
    ''' It cannot, and thus does not, check that only gateways or TACs contain the
    ''' -GATEWAY, -GW or -TAC parts in their names. By the same token, it does not
    ''' validate for 'nicknames'.</para>
    ''' <para>Another source for the rules used in this test is MSDN, specifically:
    ''' https://support.microsoft.com/en-gb/help/909264/
    ''' </para>
    ''' </remarks>
    Public Shared Function IsValidDnsName(hostName As String) As Boolean
        Return (
         hostName <> "" AndAlso
         Regex.IsMatch(hostName, "^[a-zA-Z][0-9a-zA-Z.-]*[0-9a-zA-Z.]$") AndAlso
         Not Regex.IsMatch(hostName, "\.[-.]*\.")
        )
    End Function

    ''' <summary>
    ''' Get the NetBIOS Domain Name for the given DNS Domain Name.
    ''' </summary>
    ''' <param name="dnsDomainName">A DNS Domain Name.</param>
    ''' <returns>The corresponding NetBIOS Domain Name, or Nothing if it was not
    ''' possible to find it.</returns>
    ''' <exception cref="InvalidFormatException">If <paramref name="dnsDomainName"/>
    ''' is in an invalid format, according to <see cref="IsValidDnsName"/>
    ''' </exception>
    Public Shared Function GetNetBIOSDomainName(ByVal dnsDomainName As String) As String
        If Not IsValidDnsName(dnsDomainName) Then Throw New InvalidFormatException(
            My.Resources.clsActiveDirectory_DNSDomainName0IsInvalid)

        Dim netbiosname As String = Nothing
        Using root As New DirectoryEntry(String.Format("LDAP://{0}/RootDSE", dnsDomainName))
            Dim cnc As String = root.Properties("configurationNamingContext")(0).ToString()
            Using searchRoot As New DirectoryEntry("LDAP://cn=Partitions," & cnc)
                Using searcher As New DirectorySearcher(searchRoot)
                    searcher.SearchScope = SearchScope.OneLevel
                    searcher.PropertiesToLoad.Add("netbiosname")
                    searcher.Filter = String.Format("(&(objectcategory=Crossref)(dnsRoot={0})(netBIOSName=*))", dnsDomainName)
                    Dim result As SearchResult = searcher.FindOne()
                    If result IsNot Nothing Then
                        netbiosname = result.Properties("netbiosname")(0).ToString()
                    End If
                End Using
            End Using
        End Using
        Return netbiosname

    End Function

    ''' <summary>
    ''' Attempts to get the user principal name (UPN) associated with the passed
    ''' user name. If the domain cannot be derived from the user name (i.e. it isn't
    ''' in the format {domain}\{user} or {user}@{domain}) then the passed default
    ''' domain is searched instead.
    ''' </summary>
    ''' <param name="username">The user name</param>
    ''' <param name="defaultDomain">The default domain</param>
    ''' <returns>The user's UPN, or an exception if the user doesn't exist</returns>
    Public Shared Function GetUserPrincipalName(username As String, defaultDomain As String) As String

        'We can accept the 'username' in various different formats...
        If username.Contains("\") Then
            Dim ud As String() = username.Split("\"c)
            Return ValidateUser(ud(0), ud(1))
        ElseIf username.Contains("@") Then
            Return ValidateUser(username)
        End If
        Return ValidateUser(defaultDomain, username)
    End Function

    ''' <summary>
    ''' Attempts to get the user principal name (UPN) associated with the passed
    ''' user SID.
    ''' </summary>
    ''' <param name="sid">The SID of the user to find</param>
    ''' <returns>The user's UPN, or an exception if the user doesn't exist</returns>
    Public Shared Function GetUserPrincipalName(sid As SecurityIdentifier) As String
        If sid Is Nothing Then Throw New InvalidArgumentException(My.Resources.clsActiveDirectory_InvalidUserSIDSpecified)

        Using gc = Forest.GetCurrentForest().FindGlobalCatalog()
            Using uSearcher = gc.GetDirectorySearcher()
                uSearcher.Filter = String.Format("(&(objectCategory=user)(objectSid={0}))", sid.ToString())
                uSearcher.PropertiesToLoad.Add("userPrincipalName")
                Dim res = uSearcher.FindOne()
                If res Is Nothing Then Throw New ActiveDirectoryObjectNotFoundException(
                    String.Format(My.Resources.clsActiveDirectory_UserSID0NotFoundInGlobalCatalog, sid.ToString()))

                If res.Properties("userPrincipalName").Count = 0 Then
                    Return String.Empty
                End If

                Return CStr(res.Properties("userPrincipalName")(0))
            End Using
        End Using
    End Function
    Public Shared Sub GetUsersSidsFromActiveDirectory(ByRef users As List(Of User))
        If users Is Nothing OrElse users.Count = 0 Then Throw New InvalidArgumentException(nameof(users))

        Using globalCatalog = Forest.GetCurrentForest().FindGlobalCatalog()
            Using catalogSearcher = globalCatalog.GetDirectorySearcher()

                catalogSearcher.PropertiesToLoad.Add(ObjectSidToken)

                For Each user In users
                    If user.Deleted Then
                        user.ExternalId = String.Empty
                        Continue For
                    End If

                    catalogSearcher.Filter = String.Format(
                        "(&(objectCategory=user)(userPrincipalName={0}))", LdapEscaper.EscapeSearchTerm(user.Name))

                    Dim searchResult = catalogSearcher.FindOne().GetDirectoryEntry()

                    If searchResult Is Nothing OrElse searchResult.Properties(ObjectSidToken).Count = 0 Then
                        user.ExternalId = String.Empty
                    Else
                        Dim binarySid As Byte() = CType(searchResult.Properties(ObjectSidToken).Value, Byte())
                        user.ExternalId = New SecurityIdentifier(binarySid, 0).Value
                    End If
                Next
            End Using
        End Using
    End Sub

    ''' <summary>
    ''' Check that the passed user principal name represents a valid user in the
    ''' current environment. e.g. fred.smith@eu.domain
    ''' </summary>
    ''' <param name="upn">The UPN of the user to find</param>
    ''' <returns>The user's UPN.</returns>
    ''' <exception cref="ActiveDirectoryObjectNotFoundException">If the user with
    ''' the given <paramref name="upn"/> value could not be found in the global
    ''' catalog of the current forest.</exception>
    Private Shared Function ValidateUser(upn As String) As String
        Using gc = Forest.GetCurrentForest().FindGlobalCatalog()
            Using uSearcher = gc.GetDirectorySearcher()
                uSearcher.Filter = String.Format(
                    "(&(objectCategory=user)(userPrincipalName={0}))",
                    LdapEscaper.EscapeSearchTerm(upn))
                uSearcher.PropertiesToLoad.Add("userPrincipalName")
                Dim res = uSearcher.FindOne()
                If res Is Nothing Then Throw New ActiveDirectoryObjectNotFoundException(
                    String.Format(My.Resources.clsActiveDirectory_UserUPN0NotFoundInGlobalCatalog, upn))
                Return upn
            End Using
        End Using
    End Function

    ''' <summary>
    ''' Check that the passed domain and account name represents a valid user in
    ''' the current environment. e.g. EU\fred.smith
    ''' </summary>
    ''' <param name="domain">The (Netbios) domain name</param>
    ''' <param name="sam">The account name</param>
    ''' <returns>The user's UPN, or an exception if the user doesn't exist</returns>
    Private Shared Function ValidateUser(domain As String, sam As String) As String
        Using gc = Forest.GetCurrentForest().FindGlobalCatalog()
            Using seeker = gc.GetDirectorySearcher()
                seeker.Filter = String.Format(
                    "(&(objectCategory=user)(sAMAccountName={0}))",
                    LdapEscaper.EscapeSearchTerm(sam))
                With seeker.PropertiesToLoad
                    .Add("userPrincipalName")
                    .Add("distinguishedName")
                End With
                For Each r As SearchResult In seeker.FindAll()
                    Dim fqdn As String = FQDNFromDN(CStr(r.Properties("distinguishedName")(0)))
                    If String.Equals(fqdn, domain, StringComparison.CurrentCultureIgnoreCase) OrElse
                      String.Equals(GetNetBIOSDomainName(fqdn), domain, StringComparison.CurrentCultureIgnoreCase) Then
                        Return CStr(r.Properties("userPrincipalName")(0))
                    End If
                Next
                Throw New InvalidOperationException(String.Format(My.Resources.clsActiveDirectory_User01NotFoundInGlobalCatalog, domain, sam))
            End Using
        End Using
    End Function




    ''' <summary>
    ''' Derives the fully qualified domain name from the passed distinguished name
    ''' </summary>
    ''' <param name="dn">The user's distinguished name</param>
    ''' <returns>The fully qualified domain name</returns>
    Public Shared Function FQDNFromDN(dn As String) As String
        Dim fqdn = String.Empty
        For Each part As String In dn.Split(","c)
            Dim pair() = part.Split("="c)
            If pair(0).ToLower = "dc" Then
                fqdn = String.Format("{0}{1}{2}", fqdn,
                    If(String.IsNullOrEmpty(fqdn), "", "."), pair(1))
            End If
        Next
        Return fqdn
    End Function

    ''' <summary>
    ''' Attempts to interpret the passed string as a security identifier, and either
    ''' returns it or returns nothing if the string is not in the correct format.
    ''' </summary>
    ''' <param name="s">The string to interpret</param>
    ''' <returns>This SID, or nothing</returns>
    Public Shared Function TryParseSID(s As String) As SecurityIdentifier
        Try
            Dim sid = New SecurityIdentifier(s)
            Return sid
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' Returns a RoleSet based on the Active Directory Security group membership for
    ''' the passed Windows Identity. An empty roleset indicates that the user has no
    ''' associated roles in BP.
    ''' </summary>
    ''' <param name="windowsIdentity">The Windows Identity</param>
    ''' <returns>The user's set of roles</returns>
    Public Shared Function GetRoles(windowsIdentity As IWindowsIdentity) As RoleSet

        'Get the WindowsPrincipal for passed identity
        Dim wp As New WindowsPrincipal(windowsIdentity.Identity)

        'Look for BP roles that have a security group assigned and check if
        'the current user is a member of them
        Dim rs As New RoleSet()
        For Each r In SystemRoleSet.Current
            If String.IsNullOrEmpty(r.ActiveDirectoryGroup) Then Continue For

            'Note: group could be stored as either a name or a sid
            Dim sid = TryParseSID(r.ActiveDirectoryGroup)
            If sid IsNot Nothing Then
                If wp.IsInRole(sid) Then rs.Add(r)
            Else
                If wp.IsInRole(r.ActiveDirectoryGroup) Then rs.Add(r)
            End If
        Next

        'Return the user's set of roles
        Return rs
    End Function

    ''' <summary>
    ''' Returns true if the given string is a valid sid.
    ''' </summary>
    ''' <param name="str">The string to check</param>
    Public Shared Function IsSid(str As String) As Boolean
        Try
            Dim result As New SecurityIdentifier(str)
            Return True
        Catch
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Returns the Active Directory security groups that are mapped to BP roles
    ''' along with (optionally) a list of users that are members of them.
    ''' </summary>
    ''' <param name="includeMembers">Set to True to include group member
    ''' information</param>
    ''' <param name="ctx">Carries back the server used to execute the query</param>
    ''' <param name="dom">Carries back the domain searched by the query</param>
    ''' <returns>A collection of Active Directory groups and (optionally) their
    ''' members</returns>
    Public Shared Function GetRoleMappedGroupMembership(includeMembers As Boolean,
      ByRef ctx As String, ByRef dom As String) As IList(Of SecurityGroup)
        'Get the BP domain context
        Dim domain = gSv.GetActiveDirectoryDomain()

        'Iterate through the groups assigned to BP roles
        Dim groupUsers As New List(Of SecurityGroup)
        Using searcher As New ADGroupSearcher()
            Dim gp As GroupPrincipal
            For Each r In SystemRoleSet.Current
                'Ignore if no AD group assigned to this role
                If String.IsNullOrEmpty(r.ActiveDirectoryGroup) Then Continue For

                'Find the GroupPrincipal for this group
                Dim sid = TryParseSID(r.ActiveDirectoryGroup)
                If sid IsNot Nothing Then
                    gp = searcher.GetADGroupBySid(domain, Nothing, sid)
                Else
                    gp = searcher.GetADGroupByName(domain, Nothing, r.ActiveDirectoryGroup)
                End If
                If gp Is Nothing Then Throw New ActiveDirectoryObjectNotFoundException(
                    String.Format(My.Resources.clsActiveDirectory_TheGroupWithNameOrSid0WasNotFoundInDomain1,
                        r.ActiveDirectoryGroup, domain))

                Dim secGroup As New SecurityGroup() With {
                    .Sid = gp.Sid, .Name = gp.Name, .Scope = gp.GroupScope,
                    .Path = gp.DistinguishedName, .Members = New List(Of ActiveDirectoryUser)}

                'Get all users that are a member of this group
                If includeMembers Then
                    Try
                        For Each u In GetValidGroupMembers(gp)
                            secGroup.Members.Add(New ActiveDirectoryUser() With {
                                    .Sid = u.Sid,
                                    .FullName = u.DisplayName,
                                    .DistinguishedName = u.DistinguishedName,
                                    .UserPrincipalName = u.UserPrincipalName})
                        Next
                    Catch ex As ActiveDirectoryConfigException
                        secGroup.ConfigException = ex
                    End Try
                End If

                'Add to collection
                groupUsers.Add(secGroup)
            Next
            ctx = searcher.ConnectedServer()
            dom = searcher.ConnectedDomain()
        End Using
        Return groupUsers
    End Function

    ''' <summary>
    ''' Returns a collection of security groups within the Blue Prism domain that the
    ''' passed user is a member of.
    ''' </summary>
    ''' <param name="upn">The user principal name to check</param>
    ''' <param name="skippedGroups">Carries back a list of groups which could not be
    ''' inspected (e.g. if they contain a foreign security principal and GetMembers()
    ''' fails)</param>
    ''' <returns>The collection of security groups</returns>
    Public Shared Function GetDomainGroupsForUser(upn As String, ByRef skippedGroups As String) As IList(Of SecurityGroup)
        'Get the BP domain context
        Dim domain = gSv.GetActiveDirectoryDomain()
        Dim skipped = 0

        'Find groups within the context of this domain and add them to the list if
        'the passed user is a member
        Dim groups As New List(Of SecurityGroup)
        Using searcher As New ADGroupSearcher(True)
            For Each p In searcher.GetAllADGroups(domain, Nothing)
                Dim gp = TryCast(p, GroupPrincipal)
                If gp Is Nothing Then Continue For
                Try
                    For Each u In GetValidGroupMembers(gp)
                        If u.UserPrincipalName = upn Then
                            groups.Add(New SecurityGroup() With {
                                .Sid = gp.Sid,
                                .Name = gp.Name,
                                .Scope = gp.GroupScope,
                                .Path = gp.DistinguishedName})
                            Exit For
                        End If
                    Next
                Catch ex As ActiveDirectoryConfigException
                    skipped += 1
                    If skipped <= 5 Then skippedGroups &= gp.Name & vbCrLf
                End Try
            Next
        End Using

        Dim others = skipped - 5
        If others > 0 Then
            skippedGroups = LTools.Format(My.Resources.clsActiveDirectory_SkippedGroupsCOUNTPluralOne1OtherOtherOthers, "SKIPPEDGROUPS", skippedGroups, "COUNT", others) '"{0}[+ {1} other{2}]"
        End If
        Return groups
    End Function

    ''' <summary>
    ''' Returns a list of valid users (i.e. must have a UPN) which are members of the
    ''' given Group Principal. This wraps the GroupPrincipal.GetMembers() function
    ''' because it can throw exceptions where foreign security principals or objects
    ''' with unresolved SIDs a re present.
    ''' </summary>
    ''' <param name="gp">The group to check</param>
    ''' <returns>List of valid users</returns>
    Public Shared Function GetValidGroupMembers(gp As GroupPrincipal) As IList(Of UserPrincipal)
        Try
            'Ignore any users without a UPN
            Return gp.GetMembers(True).OfType(Of UserPrincipal)().Where(
                Function(m) Not String.IsNullOrEmpty(m.UserPrincipalName)).ToList()
        Catch poe As PrincipalOperationException
            Throw New ActiveDirectoryConfigException(
                My.Resources.clsActiveDirectory_UnableToRetrieveTheMembersOfSecurityGroup0BecauseItContainsMembersWhichAreEithe,
                gp.Name)
        End Try
    End Function

    ''' <summary>
    ''' Indicates whether or not the given User Principal is enabled. This wraps the
    ''' UserPrincipal.Enabled property which can sometimes return False even for
    ''' enabled users when returned from GroupPrincipal.GetMembers() (e.g. for the
    ''' "Domain Users" group)
    ''' </summary>
    ''' <param name="u">The user principal to check</param>
    ''' <returns>True if the user is enabled, otherwise False</returns>
    Public Shared Function UserIsEnabled(u As UserPrincipal) As Boolean
        'If the Enabled property is set to True then return is
        If u.Enabled Then Return True

        'Otherwise it can't be relied upon so get the UserPrincipal directly
        Using ctx = New PrincipalContext(ContextType.Domain, FQDNFromDN(u.DistinguishedName))
            Dim tempUser = UserPrincipal.FindByIdentity(ctx, IdentityType.Sid, u.Sid.Value)
            If tempUser.Enabled Then Return True
        End Using

        'The user is definitely disabled
        Return False
    End Function

    ''' <summary>
    ''' Validate the current AD group configuration
    ''' </summary>
    ''' <param name="dom"></param>
    ''' <param name="groups"></param>
    ''' <param name="reason"></param>
    ''' <returns></returns>
    Public Shared Function ValidateADGroups(
     dom As String,
     groups As ICollection(Of String),
     ByRef reason As String) As Boolean
        Try
            Dim allSids As New HashSet(Of SecurityIdentifier)
            Dim allNames As New HashSet(Of String)

            Dim qb As New StringBuilder("(&(objectCategory=group)(|")
            ' Only look at the populated groups
            For Each gp In groups.Where(Function(s) s <> "")
                Dim sid As SecurityIdentifier = Nothing
                Dim gpName As String = Nothing
                Try
                    If gp.IsSid() Then sid = New SecurityIdentifier(gp)
                Catch ' Not a SID? Try a name instead
                End Try
                ' If we have a SID, use that; otherwise, try for the name
                If sid IsNot Nothing Then
                    allSids.Add(sid)
                    qb.AppendFormat("(objectSid={0})", sid.Value)
                Else
                    allNames.Add(gp)
                    qb.AppendFormat("(cn={0})", LdapEscaper.EscapeSearchTerm(gp))
                End If
            Next
            qb.Append("))")

            ' If we have any groups to process
            If allSids.Count + allNames.Count > 0 Then

                Dim query As String = qb.ToString()
                Dim context As New DirectoryContext(DirectoryContextType.Domain, dom)

                Using dmn As Domain = Domain.GetDomain(context)
                    'Ensure the search root is the domain containing the BP Security 
                    'Groups so clients on other domains can still find the groups.
                    Using searchRoot As DirectoryEntry = dmn.GetDirectoryEntry
                        Using ds As New DirectorySearcher(searchRoot, query) With {
                            .ReferralChasing = ReferralChasingOption.All
                        }
                            For Each res As SearchResult In ds.FindAll()
                                Dim prop As ResultPropertyValueCollection

                                prop = res.Properties(ObjectSidToken)
                                If prop.Count > 0 Then _
                                    allSids.Remove(New SecurityIdentifier(
                                            DirectCast(prop(0), Byte()), 0))

                                prop = res.Properties("cn")
                                If prop.Count > 0 Then allNames.Remove(CStr(prop(0)))
                            Next

                        End Using
                    End Using
                End Using
            End If

            Dim groupsValid = allSids.Count + allNames.Count = 0

            If Not groupsValid Then
                Dim sb As New StringBuilder()
                sb.Append(My.Resources.clsActiveDirectory_TheFollowingGroupsAreMissingOrMisconfigured)
                If allSids.Count > 0 Then sb.
                    AppendLine().
                    Append(My.Resources.clsActiveDirectory_GroupsWithTheSIDs).
                    Append(String.Join(", ", allSids.Select(Function(sid) sid.Value))).
                    Append("; ")
                If allNames.Count > 0 Then sb.
                    AppendLine().
                    Append(My.Resources.clsActiveDirectory_GroupsWithTheNames).
                    Append(String.Join(", ", allNames)).
                    Append("; ")

                reason = sb.ToString()

            End If

            Return True

        Catch ex As Exception
            Return False
        End Try
    End Function

End Class
