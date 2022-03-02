Imports System.DirectoryServices.AccountManagement
Imports System.DirectoryServices
Imports System.Security.Principal

Imports BluePrism.BPCoreLib

Public Class ADGroupSearcher
    Implements IDisposable

    ''' <summary>
    ''' Flag indicating whether the searcher will only look for AD security groups
    ''' (and not Distribution groups)
    ''' </summary>
    Private mSecurityGroupsOnly As Boolean = False

    ''' <summary>
    ''' The context of the current search. This will be disposed when 
    ''' dispose is called on the ADGroupSearcher object.
    ''' </summary>
    Private mContext As PrincipalContext

    ''' <summary>
    ''' The group that is being currently searched for. This will be disposed when 
    ''' dispose is called on the ADGroupSearcher object.
    ''' </summary>
    Private mGroup As GroupPrincipal

    ''' <summary>
    ''' The current object used for searching. This will be disposed when 
    ''' dispose is called on the ADGroupSearcher object.
    ''' </summary>
    Private mSearcher As PrincipalSearcher

    ''' <summary>
    ''' Create a new instance of the AD Group Searcher that searches and
    ''' returns <em>security</em> groups from Active Directory based on search
    ''' criteria.
    ''' </summary>
    Public Sub New()
        Me.New(True)
    End Sub

    ''' <summary>
    ''' Create a new instance of the AD Group Searcher that searches and
    ''' returns groups from Active Directory based on search criteria
    ''' </summary>
    ''' <param name="SecurityGroupsOnly">Only search for Active Directory
    ''' Security Groups</param>
    Public Sub New(ByVal SecurityGroupsOnly As Boolean)
        mSecurityGroupsOnly = SecurityGroupsOnly
    End Sub

    ''' <summary>
    ''' Gets the server that this AD group searcher is currently connected to, or
    ''' null if it is not currently connected.
    ''' </summary>
    Public ReadOnly Property ConnectedServer As String
        Get
            If mContext IsNot Nothing Then Return mContext.ConnectedServer
            Return Nothing
        End Get
    End Property

    ''' <summary>
    ''' Gets the domain that this AD group searcher is currently connected to, or
    ''' null if it is not currently connected.
    ''' </summary>
    Public ReadOnly Property ConnectedDomain As String
        Get
            If mContext IsNot Nothing Then Return mContext.Name
            Return Nothing
        End Get
    End Property

    ''' <summary>
    ''' Returns all active directory groups from the specified domain within
    ''' a certain location
    ''' </summary>
    ''' <param name="Domain">Active Directory FQDN to search</param>
    ''' <param name="SearchLocation">Path to start search from. If left blank, 
    ''' then the whole domain is searched</param>
    Public Function GetAllADGroups(ByVal Domain As String, _
                                           ByVal SearchLocation As String) _
                            As PrincipalSearchResult(Of Principal)

        Dim result As PrincipalSearchResult(Of Principal)

        mContext = GetGroupSearchContext(Domain, SearchLocation)

        mGroup = New GroupPrincipal(mContext)

        If mSecurityGroupsOnly Then
            mGroup.IsSecurityGroup = True
        End If

        mSearcher = New PrincipalSearcher(mGroup)

        result = mSearcher.FindAll()

        Return result

    End Function

    ''' <summary>
    ''' Return the active directory group from the specified domain matching 
    ''' the specified Distinguished Name
    ''' </summary>
    ''' <param name="Domain">Active Directory FQDN to search</param>
    ''' <param name="SearchLocation">Location to start search from. If left blank, 
    ''' then the whole domain is searched</param>
    ''' <param name="DistinguishedName">DN to search for</param>
    Public Function GetADGroupByDN(ByVal Domain As String, _
                                    ByVal SearchLocation As String, _
                                    ByVal DistinguishedName As String) _
                            As GroupPrincipal

        Dim result As GroupPrincipal

        mContext = GetGroupSearchContext(Domain, SearchLocation)

        result = GroupPrincipal.FindByIdentity(mContext, IdentityType.DistinguishedName, _
                                               DistinguishedName.ToString)

        If mSecurityGroupsOnly AndAlso result IsNot Nothing _
            AndAlso Not result.IsSecurityGroup Then

            result.Dispose()
            Return Nothing
        End If

        Return result


    End Function

    ''' <summary>
    ''' Gets the AD Group from the currently configured AD Domain in the database,
    ''' which is represented by the given value, which may be a string representation
    ''' of a SecurityIdentifier or a name in legacy systems.
    ''' </summary>
    ''' <param name="nameOrSid">The SID or name of the required group</param>
    ''' <returns>The group corresponding with the given SID or name, or null if no
    ''' such group could be found.</returns>
    Public Function GetGroup(nameOrSid As String) As GroupPrincipal
        If nameOrSid = "" Then Return Nothing
        Dim dom As String = gSv.GetActiveDirectoryDomain()
        If dom = "" Then Return Nothing
        Try
            If nameOrSid.IsSid() Then _
             Return GetADGroupBySid(dom, Nothing, New SecurityIdentifier(nameOrSid))

        Catch
            ' The IsSid() function only checks text pattern; it may still be wrong
            ' in which case, it may still not be a proper SID, so fall through to
            ' checking for the group by name if any errors occur getting by SID
        End Try
        Return GetADGroupByName(dom, Nothing, nameOrSid)

    End Function

    ''' <summary>
    ''' Returns the active directory group from the specified domain based 
    ''' on the group name
    ''' </summary>
    ''' <param name="Domain">Active Directory FQDN to search</param>
    ''' <param name="SearchLocation">Location to start search from. If left blank, 
    ''' then the whole domain is searched</param>
    ''' <param name="GroupName">Name of group the searcher is trying to find.
    '''</param>
    Public Function GetADGroupByName(ByVal Domain As String, _
                                     ByVal SearchLocation As String, _
                                     ByVal GroupName As String) _
                                 As GroupPrincipal

        Dim result As GroupPrincipal

        mContext = GetGroupSearchContext(Domain, SearchLocation)

        result = GroupPrincipal.FindByIdentity(mContext, IdentityType.Name, _
                                               GroupName.ToString)

        If mSecurityGroupsOnly AndAlso result IsNot Nothing _
            AndAlso Not result.IsSecurityGroup Then

            result.Dispose()
            Return Nothing
        End If

        Return result


    End Function

    ''' <summary>
    ''' Returns the active directory groups that match the search string.
    ''' </summary>
    ''' <param name="Domain">Active Directory FQDN to search</param>
    ''' <param name="SearchLocation">Location to start search from. If left blank, 
    ''' then the whole domain is searched</param>
    ''' <param name="GroupNameSearch">Name of group the searcher is trying to find.
    ''' This can include wildcards</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetADGroupByWildcardSearch(ByVal Domain As String, _
                                               ByVal SearchLocation As String, _
                                               ByVal GroupNameSearch As String) _
                                 As PrincipalSearchResult(Of Principal)

        Dim result As PrincipalSearchResult(Of Principal)

        mContext = GetGroupSearchContext(Domain, SearchLocation)

        mGroup = New GroupPrincipal(mContext)

        If mSecurityGroupsOnly Then
            mGroup.IsSecurityGroup = True
        End If

        mGroup.Name = GroupNameSearch
        mSearcher = New PrincipalSearcher(mGroup)

        result = mSearcher.FindAll()

        Return result

    End Function

    ''' <summary>
    ''' Returns the active directory group from the specified domain based 
    ''' on the group's Active Directory SID
    ''' </summary>
    ''' <param name="Domain">Active Directory FQDN to search</param>
    '''<param name="SearchLocation">Location to start search from. If left blank, 
    ''' then the whole domain is searched</param>
    ''' <param name="GroupSid">SID of group that searcher is trying to find</param>
    Public Function GetADGroupBySid(ByVal Domain As String, _
                                     ByVal SearchLocation As String, _
                                     ByVal GroupSid As SecurityIdentifier) As GroupPrincipal

        mContext = GetGroupSearchContext(Domain, SearchLocation)
        Dim result = GroupPrincipal.FindByIdentity(mContext, IdentityType.Sid, _
                                               GroupSid.Value)

        If mSecurityGroupsOnly AndAlso result IsNot Nothing _
            AndAlso Not result.IsSecurityGroup Then

            result.Dispose()
            Return Nothing
        End If

        Return result

    End Function

    ''' <summary>
    ''' Returns the path of the specified group's 'Parent Folder' in Active Directory
    ''' </summary>
    ''' <param name="Domain">Active Directory FQDN to search</param>
    ''' <param name="GroupSid">SID of the group whose parent path you 
    ''' want to return </param>
    ''' <returns>The path of the group's parent or an empty string if the group or
    ''' its parent could not be found.</returns>
    Public Function GetParentPathForADGroup(ByVal Domain As String, _
                                              ByVal GroupSid As SecurityIdentifier) As String

        Dim path As String = ""

        Using group As GroupPrincipal = GetADGroupBySid(Domain, Nothing, GroupSid)
            If group Is Nothing Then Return ""

            Using entry = TryCast(group.GetUnderlyingObject(), DirectoryEntry)
                If entry Is Nothing Then Return ""

                If entry IsNot Nothing AndAlso entry.Parent IsNot Nothing Then
                    path = entry.Parent.Path
                    path = path.Replace("LDAP://", "")
                    path = path.Replace(Domain & "/", "")
                End If
            End Using
        End Using

        Return path

    End Function

    Private Function GetGroupSearchContext(Domain As String, SearchLocation As String) As PrincipalContext
        If String.IsNullOrEmpty(SearchLocation) Then
            Return New PrincipalContext(ContextType.Domain, Domain)
        Else
            Return New PrincipalContext(ContextType.Domain, Domain, SearchLocation)
        End If

    End Function


#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                If mContext IsNot Nothing Then mContext.Dispose()
                If mGroup IsNot Nothing Then mGroup.Dispose()
                If mSearcher IsNot Nothing Then mSearcher.Dispose()
            End If

        End If
        Me.disposedValue = True
    End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in 
        'Dispose (disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

End Class
