Imports System.Data.SqlClient
Imports System.Drawing
Imports System.Globalization
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.Logging
Imports BluePrism.AutomateAppCore.Resources.ResourceMachine
Imports BluePrism.AutomateAppCore.Resources
Imports BluePrism.AutomateAppCore.Sessions
Imports BluePrism.AutomateProcessCore
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.Core.Compression
Imports BluePrism.Core.Extensions
Imports BluePrism.Core.Resources
Imports BluePrism.Data
Imports BluePrism.Server.Domain.Models

Partial Public Class clsServer

    ''' <summary>
    ''' Sets the configXML for a paticular resource (business object) in the database.
    ''' </summary>
    ''' <param name="sResourceName">The name of the business object,
    ''' e.g "CommonAutomation.clsWord"</param>
    ''' <param name="sConfigXML">The XML containing the config settings</param>
    <SecuredMethod(True)>
    Public Sub SetResourceConfig(ByVal sResourceName As String, ByVal sConfigXML As String) Implements IServer.SetResourceConfig
        CheckPermissions()

        Using con = GetConnection()
            'See if a resource already exists with this name - if so
            'we will just update the record...
            Dim bExists As Boolean = False
            Dim cmd As New SqlCommand("SELECT name FROM BPAResourceConfig where name = @ResourceName")
            With cmd.Parameters
                .AddWithValue("@ResourceName", sResourceName)
            End With

            Dim reader = con.ExecuteReturnDataReader(cmd)
            Do While reader.Read()
                bExists = True
            Loop
            reader.Close()

            'Now use a SQL command to either update or insert a new
            'record...
            Dim updatecmd As SqlCommand
            If bExists Then
                updatecmd = New SqlCommand("UPDATE BPAResourceConfig SET config = @ConfigXML WHERE name=@ResourceName")
            Else
                updatecmd = New SqlCommand("INSERT INTO BPAResourceConfig VALUES (@ResourceName,@ConfigXML)")
            End If

            With updatecmd.Parameters
                .AddWithValue("@ConfigXML", sConfigXML)
                .AddWithValue("@ResourceName", sResourceName)
            End With

            con.Execute(updatecmd)

            AuditRecordObjectEvent(con, ObjectEventCode.ConfigureObject, sResourceName, My.Resources.NewConfigXmlIsAsFollows & sConfigXML)
        End Using

    End Sub

    ''' <summary>
    ''' Reads a resource's config details from the database.
    ''' </summary>
    ''' <param name="sResourceName">The resource name</param>
    ''' <returns>The config details</returns>
    <SecuredMethod(True)>
    Public Function GetResourceConfig(ByVal sResourceName As String) As String Implements IServer.GetResourceConfig
        CheckPermissions()
        Dim sResult As String = String.Empty

        Dim cmd As New SqlCommand("select config from BPAResourceConfig where name = @ResourceName")
        With cmd.Parameters
            .AddWithValue("@ResourceName", sResourceName)
        End With

        Using con = GetConnection(), reader = con.ExecuteReturnDataReader(cmd)
            If reader.Read() Then sResult = CStr(reader("config"))
            Return sResult
        End Using
    End Function

    <SecuredMethod(True)>
    Public Function GetResourcesPoolInfo() As IEnumerable(Of ResourcePoolInfo) Implements IServer.GetResourcesPoolInfo
        CheckPermissions()
        Using connection = GetConnection()
            Return GetResourcesPoolInfo(connection)
        End Using
    End Function

    Private Function GetResourcesPoolInfo(connection As IDatabaseConnection) As IEnumerable(Of ResourcePoolInfo)
        Dim resourcePoolInfo = New List(Of ResourcePoolInfo)
        Using command As New SqlCommand("select a.resourceid as resourceid, a.name as resourceName, " &
                                        "b.name as poolName, b.controller as controllerid, b.resourceid as poolid from bparesource a " &
                                        "left join bparesource b on a.pool = b.resourceid " &
                                        "where a.attributeID & @flags = 0")
            command.Parameters.AddWithValue("@flags", CInt(ResourceAttribute.Retired Or ResourceAttribute.Debug Or ResourceAttribute.Pool))

            Using reader = connection.ExecuteReturnDataReader(command)
                Dim prov As New ReaderDataProvider(reader)
                While reader.Read()
                    resourcePoolInfo.Add(New ResourcePoolInfo(prov.GetGuid("resourceid"),
                                                              prov.GetString("resourceName"),
                                                              prov.GetString("poolName"),
                                                              prov.GetGuid("controllerid"),
                                                              prov.GetGuid("poolid")))
                End While
            End Using
            Return resourcePoolInfo
        End Using
    End Function


    ''' <summary>
    ''' Reads resource details from the database.
    ''' </summary>
    ''' <param name="requiredAttributes">Attributes which returned rows must
    ''' possess. Set to none to be fully inclusive.</param>
    ''' <param name="unacceptableAttributes">Attributes which returned rows must
    ''' not possess. Set to none to be fully inclusive.</param>
    ''' <returns>A datatable with columns name, status, resourceid, attributeid and
    ''' pool if no error occurs, or Nothing if an error does occur.</returns>
    <SecuredMethod(True)>
    Public Function GetResources(requiredAttributes As ResourceAttribute,
                                 unacceptableAttributes As ResourceAttribute,
                                 robotName As String) As DataTable Implements IServer.GetResources
        CheckPermissions()
        Try
            Using con = GetConnection()
                Return GetResources(con, requiredAttributes, unacceptableAttributes, robotName)
            End Using
        Catch ex As Exception
            Log.Error(ex, "Failed to get resources from server")
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Returns resources registered on a specific machine.
    ''' </summary>
    ''' <param name="requiredAttributes">Attributes which returned rows must
    ''' possess. Set to none to be fully inclusive.</param>
    ''' <param name="unacceptableAttributes">Attributes which returned rows must
    ''' not possess. Set to none to be fully inclusive.</param>
    ''' <param name="hostName">The machine host name to search on</param>
    ''' <returns>A datatable with columns name, status, resourceid, attributeid and
    ''' pool if no error occurs, or Nothing if an error does occur.</returns>
    <SecuredMethod(True)>
    Public Function GetResourcesForHost(requiredAttributes As ResourceAttribute,
     unacceptableAttributes As ResourceAttribute, hostName As String) As DataTable Implements IServer.GetResourcesForHost
        CheckPermissions()
        Using con = GetConnection()
            Return GetResources(con, requiredAttributes, unacceptableAttributes, hostName)
        End Using
    End Function

    ''' <summary>
    ''' Reads resource details from the database.
    ''' </summary>
    ''' <param name="requiredAttributes">Attributes which returned rows must
    ''' possess. Set to none to be fully inclusive.</param>
    ''' <param name="unacceptableAttributes">Attributes which returned rows must
    ''' not possess. Set to none to be fully inclusive.</param>
    ''' <param name="hostName">Optional machine name prefix</param>
    ''' <returns>A datatable with columns name, status, resourceid, attributeid and
    ''' pool if no error occurs, or Nothing if an error does occur.</returns>
    Friend Function GetResources(con As IDatabaseConnection,
     requiredAttributes As ResourceAttribute, unacceptableAttributes As ResourceAttribute,
     Optional hostName As String = Nothing) As DataTable

        Dim sb As New StringBuilder(
         " SELECT name,fqdn,statusid,resourceid,attributeid,pool,ssl" &
         " FROM BPAResource" &
         " WHERE (@reqd=0 OR (attributeid & @reqd) <> 0)" &
         "   AND (attributeid & @denied) = 0")
        If hostName IsNot Nothing Then
            sb.Append(" AND (name=@exactName or name like @likeName)")
        End If
        sb.Append(" ORDER BY name")

        Dim cmd As New SqlCommand(sb.ToString())
        With cmd.Parameters
            .AddWithValue("@reqd", requiredAttributes)
            .AddWithValue("@denied", unacceptableAttributes)
            If hostName IsNot Nothing Then
                .AddWithValue("@exactName", hostName)
                .AddWithValue("@likeName", hostName & ":%")
            End If
        End With

        Return con.ExecuteReturnDataTable(cmd)
    End Function

    ''' <summary>
    ''' Retrieves a list of known resources and applies given filters.
    ''' </summary>
    ''' <param name="requiredAttributes">Attributes which returned rows must
    ''' possess. Set to none to be fully inclusive.</param>
    ''' <param name="unacceptableAttributes">Attributes which returned rows must
    ''' not possess. Set to none to be fully inclusive.</param>
    ''' <param name="hostName">Optional machine name prefix</param>
    ''' <returns>A List of ResourceInfo objects</returns>
    <SecuredMethod(True)>
    Public Function GetResourceStatus(requiredAttributes As ResourceAttribute,
     unacceptableAttributes As ResourceAttribute, Optional hostName As String = Nothing) _
     As ICollection(Of ResourceInfo) Implements IServer.GetResourceInfo

        CheckPermissions()

        ' Note - In the (near) future we may want to read this from a periodically
        ' refreshing cache rather than database every time.
        Dim resources = GetLatestResourceStatus(unacceptableAttributes,
                                                requiredAttributes, hostName)

        Return resources
    End Function

    <SecuredMethod(True)>
    Public Function GetResourceInfoCompressed(requiredAttributes As ResourceAttribute,
                             unacceptableAttributes As ResourceAttribute) As Byte() Implements IServer.GetResourceInfoCompressed
        CheckPermissions()
        Dim resources = GetResourceStatus(requiredAttributes, unacceptableAttributes, Nothing)
        Return GZipCompression.SerializeAndCompress(Of ICollection(Of ResourceInfo))(resources)
    End Function

    <SecuredMethod(True)>
    Public Function GetResourcesData(resourceParameters As ResourceParameters) As IReadOnlyCollection(Of ResourceInfo) Implements IServer.GetResourcesData
        CheckPermissions()

        ' Note - In the (near) future we may want to read this from a periodically
        ' refreshing cache rather than database every time.
        Dim resources = GetLatestResourcesData(resourceParameters)

        Return resources
    End Function

    <SecuredMethod(True)>
    Public Function GetResourceData(resourceId As Guid) As ResourceInfo Implements IServer.GetResourceData
        CheckPermissions()

        ' Note - In the (near) future we may want to read this from a periodically
        ' refreshing cache rather than database every time.
        Dim resource = GetLatestResourceData(resourceId)

        Return resource
    End Function

    ''' <summary>
    ''' Retrieves a list of known resources cultures
    ''' </summary>
    ''' <returns>A List of ResourceCulture objects</returns>
    <SecuredMethod(True)>
    Public Function GetResourcesCulture() _
     As ICollection(Of ResourceCulture) Implements IServer.GetResourcesCulture

        CheckPermissions()

        Dim resourceCultures As New List(Of ResourceCulture)
        Dim controllers As New HashSet(Of Guid)

        Using con = GetConnection()
            Using cmd As New SqlCommand("select resourceid, currentculture from BPAResource where attributeID & @retiredFlag  = 0")
                With cmd.Parameters
                    .AddWithValue("@retiredFlag", CInt((ResourceAttribute.Retired Or ResourceAttribute.Debug)))
                End With

                Using dt As DataTable = con.ExecuteReturnDataTable(cmd)
                    For Each dr As DataRow In dt.Rows
                        Dim resource = New ResourceCulture With {
                            .ID = CType(dr("resourceid"), Guid),
                            .CurrentCulture = If(dr("currentculture") Is DBNull.Value, "", CStr(dr("currentculture")))
                            }
                        resourceCultures.Add(resource)
                    Next
                End Using
            End Using
        End Using
        Return resourceCultures
    End Function

    ''' <summary>
    ''' Get the resource information from the database and coerce it into POCO
    ''' data objects.
    ''' </summary>
    ''' <returns>A List of ResourceInfo objects representing all known resources
    ''' </returns>
    Private Function GetLatestResourceStatus(excludedAttributes As ResourceAttribute, requiredAttributes As ResourceAttribute, hostname As String) As List(Of ResourceInfo)

        Dim resourceInfoList As New List(Of ResourceInfo)
        Dim controllers As New HashSet(Of Guid)
        Dim resourceConnectionStatistics = GetASCRConnectionManager()?.GetAllResourceConnectionStatistics()

        Using con = GetConnection()
            Using dt As DataTable = GetResourcesStatus(con, excludedAttributes, requiredAttributes, hostname)
                ' Convert the data into a re-usable format
                For Each dr As DataRow In dt.Rows
                    Dim resource = New ResourceInfo With {
                        .ID = CType(dr("resourceid"), Guid),
                        .Pool = If(dr("pool") Is DBNull.Value, Guid.Empty, CType(dr("pool"), Guid)),
                        .Attributes = If(dr("attributeid") Is DBNull.Value, ResourceAttribute.None, CType(dr("attributeid"), ResourceAttribute)),
                        .Name = If(dr("name") Is DBNull.Value, "", CStr(dr("name"))),
                        .LastUpdated = If(dr("lastupdated") Is DBNull.Value, Date.MinValue, CDate(dr("lastupdated"))),
                        .ActiveSessions = If(dr("actionsrunning") Is DBNull.Value, 0, CInt(dr("actionsrunning"))),
                        .WarningSessions = If(dr("warningsessions") Is DBNull.Value, 0, CInt(dr("warningsessions"))),
                        .PendingSessions = If(dr("pendingsessions") Is DBNull.Value, 0, CInt(dr("pendingsessions"))),
                        .UserID = If(dr("userid") Is DBNull.Value, Guid.Empty, CType(dr("userid"), Guid))
                        }

                    Dim result = New ResourceConnectionStatistic()
                    resource.LastConnectionStatistics = If(resourceConnectionStatistics?.TryGetValue(resource.ID, result),
                                                            result, Nothing)

                    ' Check current user has access to this resource
                    Dim member = New Groups.ResourceGroupMember(resource.ID)
                    member.Permissions = GetEffectiveMemberPermissions(con, member)
                    If Not member.HasViewPermission(mLoggedInUser) Then Continue For

                    Dim controller = If(dr("controller") Is DBNull.Value, Guid.Empty, CType(dr("controller"), Guid))

                    ' Add to the list of pool controllers if it's not there already.
                    If Not controller = Guid.Empty Then
                        If Not controllers.Contains(controller) Then controllers.Add(controller)
                    End If

                    resource.Status = CType(dr("statusid"), ResourceDBStatus)

                    resourceInfoList.Add(resource)
                Next
            End Using
        End Using

        ' Set the flag on the controllers and update the status, process the pools last so the display status is accurate 
        For Each resourceInfo As ResourceInfo In resourceInfoList.OrderBy(Function(x) If(x.Attributes.HasFlag(ResourceAttribute.Pool), 1, 0))
            resourceInfo.Controller = controllers.Contains(resourceInfo.ID)
            Dim anyOnlineInPool = resourceInfoList.Any(Function(f) f.Pool = resourceInfo.ID AndAlso
                                                     f.DisplayStatus <> ResourceStatus.Offline AndAlso
                                                     f.DisplayStatus <> ResourceStatus.Missing)
            UpdateResourceStatusInfo(resourceInfo, Function(userId) GetUserName(userId), mLoggedInUser.Id, anyOnlineInPool)
        Next
        Return resourceInfoList

    End Function

    Private Function GetLatestResourcesData(resourceParameters As ResourceParameters) As IReadOnlyCollection(Of ResourceInfo)

        Using connection = GetConnection()
            Dim stageWarningThreshold = GetStageWarningThreshold(connection)
            Using command As IDataAccessCommand = New GetResourcesDataCommand(mLoggedInUser, stageWarningThreshold, resourceParameters)
                Return CType(command.Execute(connection), IReadOnlyCollection(Of ResourceInfo))
            End Using
        End Using

    End Function

    Private Function GetLatestResourceData(resourceId As Guid) As ResourceInfo

        Using con = GetConnection()
            Using dt As DataTable = GetResourceDataFromDatabase(con, resourceId)
                If dt.Rows.Count = 0 Then Return Nothing

                Dim dr = dt.Rows(0)

                Dim resource = New ResourceInfo With {
                    .ID = CType(dr("resourceid"), Guid),
                    .Pool = If(dr("pool") Is DBNull.Value, Guid.Empty, CType(dr("pool"), Guid)),
                    .Name = If(dr("name") Is DBNull.Value, "", CStr(dr("name"))),
                    .Attributes = If(dr("attributeid") Is DBNull.Value, ResourceAttribute.None, CType(dr("attributeid"), ResourceAttribute)),
                    .UserID = If(dr("userid") Is DBNull.Value, Guid.Empty, CType(dr("userid"), Guid))
                }

                Dim member = New Groups.ResourceGroupMember(resource.ID)
                member.Permissions = GetEffectiveMemberPermissions(con, member)
                If Not member.HasViewPermission(mLoggedInUser) Then Return Nothing

                Return resource
            End Using
        End Using

    End Function

    ''' <summary>
    ''' Forms a status message for the given ResourceInfo object
    ''' data objects. Set as friend so we can unit test
    ''' </summary>
    ''' <param name="r">Current resource information</param>
    ''' <param name="loggedInUserId">The id of the currently logged in user</param>
    ''' <param name="anyOnlineInPool">True if there are any online resources in pool</param>
    Friend Sub UpdateResourceStatusInfo(r As ResourceInfo, getUserName As Func(Of Guid, String), loggedInUserId As Guid, Optional anyOnlineInPool As Boolean = False)
        Dim timeSinceLastContact As Integer = 0

        If r.Status = ResourceDBStatus.Offline Then
            r.DisplayStatus = ResourceStatus.Offline
            r.InfoColour = Color.Gray.ToArgb
            Return
        End If

        If r.LastUpdated > DateSerial(2017, 1, 1) Then
            ' not too out of date - if upgraded, this could be null
            timeSinceLastContact = CInt(DateTime.UtcNow.Subtract(r.LastUpdated).TotalSeconds)
        Else
            ' Arbitary number to make code go into connection lost
            timeSinceLastContact = 1000
        End If

        ' Set sensible defaults the colour
        r.InfoColour = Color.Black.ToArgb
        r.Information = ""
        r.DisplayStatus = ResourceStatus.Idle

        ' If this is a resource pool with resources, set status to pool, if no resources, set offline and return. No further action needed.
        If r.Attributes.HasFlag(ResourceAttribute.Pool) Then
            r.DisplayStatus = If(anyOnlineInPool, ResourceStatus.Pool, ResourceStatus.Offline)
            Return
        End If

        ' show a special state if it is a active login agent
        If r.Attributes.HasFlag(ResourceAttribute.LoginAgent) Then
            r.DisplayStatus = ResourceStatus.LoggedOut
        End If

        ' Normal resource - set status based on state of active sessions.
        If Not r.Attributes.HasAnyFlag(ResourceAttribute.LoginAgent Or ResourceAttribute.Pool) Then

            If r.Attributes.HasFlag(ResourceAttribute.Private) AndAlso loggedInUserId <> r.UserID Then
                ' active, but private and not the current user's.
                Dim userName = getUserName(r.UserID)
                r.DisplayStatus = ResourceStatus.Private
                r.Information = String.Format(My.Resources.clsServer_OwnedBy0, userName)
                r.InfoColour = Color.Gray.ToArgb
            Else
                If r.ActiveSessions > 0 Then
                    r.DisplayStatus = ResourceStatus.Working
                    r.InfoColour = Color.Green.ToArgb
                    r.Information = String.Format(My.Resources.clsServer_UpdateResourceStatusInfo_0Active, r.ActiveSessions)

                    If r.WarningSessions > 0 Then
                        r.DisplayStatus = ResourceStatus.Warning
                        r.Information &= String.Format(My.Resources.clsServer_UpdateResourceStatusInfo_0Warning, r.WarningSessions)
                        r.InfoColour = Color.Purple.ToArgb
                    End If

                ElseIf r.PendingSessions = 0 Then
                    'No active and no pending sessions.
                    r.DisplayStatus = ResourceStatus.Idle
                    r.InfoColour = Color.Black.ToArgb
                    r.Information = My.Resources.clsServer_NoSessions
                End If
                If r.PendingSessions > 0 Then
                    r.Information &= String.Format(If(String.IsNullOrEmpty(r.Information), My.Resources.UpdateResourceStatusInfo_0Pending, My.Resources.clsServer_UpdateResourceStatusInfo_0Pending), r.PendingSessions)
                End If
            End If
        End If

        ' Mark a pool controller as such
        If r.Pool <> Guid.Empty AndAlso r.Controller Then
            r.Information = "* " & r.Information
        End If

        ' If time since last contact is acceptable, our work here is done.
        If timeSinceLastContact < 60 Then Return

        ' Otherwise we have lost connection to this resource. Update information and status to
        ' indicate this.
        r.DisplayStatus = ResourceStatus.Missing
        r.InfoColour = Color.Red.ToArgb

        If timeSinceLastContact < 300 Then ' config needed in future US
            r.Information = String.Format(My.Resources.clsServer_ConnectionLost0Seconds, timeSinceLastContact)
        Else
            r.Information = My.Resources.clsServer_ConnectionLost
        End If

    End Sub

    ''' <summary>
    ''' Gets a select of all Resources that meet the given criteria of excludedAttributes, requiredAttributes and hostname
    ''' </summary>
    ''' <param name="excludedAttribute">Resource will meet this criteria. None to not check</param>
    ''' <param name="requiredAttribute">Resource will not have criteria. None to not check</param>
    ''' <param name="hostName">Sort by host name. Null to not check</param>
    ''' <returns></returns>
    Private Function GetResourcesStatus(con As IDatabaseConnection, excludedAttribute As ResourceAttribute, requiredAttribute As ResourceAttribute, Optional hostName As String = Nothing) As DataTable
        ' don't check for warnings if they are disabled
        If GetStageWarningThreshold(con) = 0 Then Return GetResourceStatusWithoutWarnings(con)

        Dim excludedAttributeList = excludedAttribute.ToIntList()
        Dim requiredAttributeList = requiredAttribute.ToIntList()

        Const cmdtxt =
                    "with sessions as (
                        select runningresourceid as id, count(*) as warningSessions
                        from BPASession
                        where statusid = 1 and warningthreshold > 0
                            and dateadd(""s"", lastupdatedtimezoneoffset, getutcdate()) > dateadd(""s"", warningthreshold, lastupdated) group by runningresourceid
                    )
                    select resourceid, name, fqdn, statusid, resourceid, attributeid, pool, (processesrunning - actionsrunning) As pendingsessions,
                    actionsrunning, lastupdated, isnull(warningSessions,0) As warningSessions, controller, userid
                    from BPAResource r
                    left join sessions On r.resourceid = sessions.id
                    where (@hostname is null or r.name = @hostname or r.name like @hostname + ':%')"
        Using cmd As New SqlCommand(cmdtxt)
            With cmd.Parameters
                .AddWithValue("@excludedAttribute", excludedAttribute)
                .AddWithValue("@requiredAttribute", requiredAttribute)
                .AddNullableWithValue("@hostname", hostName)
            End With

            ' build excludedAttribute conditional
            If excludedAttribute <> 0 Then
                ' and clause check the logical and of the enum values are not in a compound list
                'and r... & @excludedAttribute... not in (1,2,4,8...)
                cmd.CommandText &= $" and r.AttributeID & @excludedAttribute not in ({String.Join(",", excludedAttributeList.Select(Function(a, index) $"@excludedAttribute{index}"))})"
                cmd.AddEnumerable("@excludedAttribute", excludedAttributeList)
            End If
            ' build requiredAttribute conditional
            If requiredAttribute <> 0 Then
                cmd.CommandText &= $" and r.AttributeID & @requiredAttribute in ({String.Join(",", requiredAttributeList.Select(Function(a, index) $"@requiredAttribute{index}"))})"
                cmd.AddEnumerable("@requiredAttribute", requiredAttributeList)
            End If
            Return con.ExecuteReturnDataTable(cmd)
        End Using
    End Function

    Private Function GetResourceDataFromDatabase(con As IDatabaseConnection, resourceId As Guid) As DataTable
        Using command = mDatabaseCommandFactory("")
            command.CommandText =
                        "select top 1 resourceid, name, attributeid, pool, userid
                        from BPAResource
                        where resourceId = @resourceId
                            and attributeid not in (select AttributeID from BPAResourceAttribute where AttributeName in ('Pool', 'Debug'))"

            command.AddParameter("@resourceId", resourceId)

            Return con.ExecuteReturnDataTable(command)
        End Using
    End Function

    Private Function GetResourceStatusWithoutWarnings(con As IDatabaseConnection) As DataTable
        Const cmdTxt =
            "select resourceid, name,fqdn,statusid,resourceid,attributeid,pool,
            (processesrunning - actionsrunning) As pendingsessions,
            actionsrunning, lastupdated, 0 As warningSessions, controller, userid
            from BPAResource"
        Using cmd = New SqlCommand(cmdTxt)
            Return con.ExecuteReturnDataTable(cmd)
        End Using
    End Function

    Private Sub RemoveClonedResources(con As IDatabaseConnection, resourceId As Guid)

        Dim cmd As New SqlCommand("delete from BPAGroupResource where memberid = @memberid")
        With cmd.Parameters
            .AddWithValue("@memberid", resourceId)
        End With

        con.Execute(cmd)
    End Sub

    Private Sub AddToResourceAttributes(con As IDatabaseConnection, resourceId As Guid, attributesToAdd As ResourceAttribute)
        Dim cmd As New SqlCommand("
                update BPAResource
                set AttributeID = AttributeID | @attributesToAdd
                where resourceid = @resid")

        With cmd.Parameters
            .AddWithValue("@resid", resourceId)
            .AddWithValue("@attributesToAdd", attributesToAdd)
        End With
        con.Execute(cmd)
    End Sub

    ''' <summary>
    ''' Indicates whether a the resource PC with the specified ID is retired.
    ''' </summary>
    ''' <param name="id">The ID of the resource of interest.</param>
    ''' <returns>True if the resource PC is retired, False otherwise.</returns>
    Private Function IsResourcePCRetired(ByVal con As IDatabaseConnection, ByVal id As Guid) As Boolean
        Dim cmd As New SqlCommand(
         "select attributeid from BPAResource where resourceid = @resourceid")
        cmd.Parameters.AddWithValue("@ResourceID", id)
        Return (BPUtil.IfNull(con.ExecuteReturnScalar(cmd), ResourceAttribute.None) _
         And ResourceAttribute.Retired) <> 0
    End Function

    Private Sub SubtractFromResourceAttributes(con As IDatabaseConnection, resourceId As Guid, attributesToSubtract As ResourceAttribute)
        Dim cmd As New SqlCommand("
                update BPAResource
                set AttributeID = AttributeID & @attributesToSubtract
                where resourceid = @resid")

        With cmd.Parameters
            .AddWithValue("@resid", resourceId)
            .AddWithValue("@attributesToSubtract", CInt(Integer.MaxValue Xor CInt(attributesToSubtract)).ToString)
        End With
        con.Execute(cmd)
    End Sub

    ''' <summary>
    ''' Mark the specified resource as retired
    ''' </summary>
    ''' <param name="resourceId">The ID of the resource to retire</param>
    <SecuredMethod(Permission.Resources.ConfigureResource)>
    Public Sub RetireResource(resourceId As Guid) Implements IServer.RetireResource

        ' Check role-based permissions
        CheckPermissions()
        Using con = GetConnection()
            'Check group-based permissions
            If Not HasPermissionOnResource(con, resourceId, Permission.Resources.ConfigureResource) Then
                Throw New PermissionException(GetLocalisedResourceString("clsServer_TheCurrentUserDoesNotHavePermissionToRetireThisResource"))
            End If

            If Not CanRetireResource(con, resourceId) Then Throw New InvalidStateException(GetLocalisedResourceString("clsServer_ThisResourceIsCurrentlyOnlineAndCannotBeRetired"))
            con.BeginTransaction()

            RemoveClonedResources(con, resourceId)
            AddToResourceAttributes(con, resourceId, ResourceAttribute.Retired)

            ' Audit retire actions
            AuditRecordResourceEvent(con, ResourceEventCode.ChangedAttributes,
                    GetResourceName(con, resourceId), "", My.Resources.clsServer_TheRetiredAttributeWasSet)
            con.CommitTransaction()
        End Using
    End Sub

    ''' <summary>
    ''' Mark the specified resource as unretired
    ''' </summary>
    ''' <param name="resourceId">The ID of the resource to unretire</param>
    <SecuredMethod(Permission.Resources.ConfigureResource)>
    Public Sub UnretireResource(resourceId As Guid) Implements IServer.UnretireResource

        ' Check role-based permissions. No need to check group-based permissions
        ' since resources are removed from their groups when retired
        CheckPermissions()
        Using con = GetConnection()

            con.BeginTransaction()

            Dim reason = ""
            If Not CanActivateResource(con, resourceId, GetResourceAttributes(con, resourceId), reason) Then
                Throw New LicenseRestrictionException(reason)
            End If

            SubtractFromResourceAttributes(con, resourceId, ResourceAttribute.Retired)

            ' Audit unretire actions
            AuditRecordResourceEvent(con, ResourceEventCode.ChangedAttributes,
                GetResourceName(con, resourceId), "", My.Resources.clsServer_TheRetiredAttributeWasCleared)

            con.CommitTransaction()
        End Using
    End Sub

    ''' <summary>
    ''' Enables or disables the windows event logging on the specified resource.
    ''' </summary>
    ''' <param name="resourceId">The ID of the resource for which the event
    ''' logging should be enabled/disabled.</param>
    ''' <param name="enable">True to enable logging on the specified resource, false
    ''' to disable it. Note that if disabled, logging will still be written to the
    ''' textarea within the resource PCs' windows</param>
    <SecuredMethod(Permission.Resources.ConfigureResource)>
    Public Sub SetResourceEventLogging(resourceId As Guid, enable As Boolean) Implements IServer.SetResourceEventLogging

        ' Check role-based permissions
        CheckPermissions()
        Using con = GetConnection()
            ' Check group-based permissions
            If Not HasPermissionOnResource(con, resourceId, Permission.Resources.ConfigureResource) Then
                Throw New PermissionException(
                    My.Resources.clsServer_TheCurrentUserDoesNotHavePermissionToConfigureThisResource)
            End If

            Dim cmd As New SqlCommand("
                update BPAResource
                set logtoeventlog = @enable
                where resourceid = @resid")

            With cmd.Parameters
                .AddWithValue("@resid", resourceId)
                .AddWithValue("@enable", enable)
            End With
            con.Execute(cmd)
        End Using

    End Sub

    ''' <summary>
    ''' Checks the event logging status of a specified resource.
    ''' </summary>
    ''' <param name="resourceId">The ID of the resource</param>
    ''' <returns>True if logging to windows event log is enabled for the specified
    ''' resource; False if it is disabled.</returns>
    ''' <exception cref="NoSuchElementException">If no resource was found with the
    ''' given resource ID.</exception>
    <SecuredMethod(True)>
    Public Function IsResourceEventLoggingEnabled(ByVal resourceId As Guid) As Boolean Implements IServer.IsResourceEventLoggingEnabled
        CheckPermissions()
        Using con = GetConnection()
            Dim map As IDictionary(Of Guid, Boolean) =
             GetResourceEventLoggingStates(con, GetSingleton.ICollection(resourceId))
            If map.Count = 0 Then Throw New NoSuchElementException(
             My.Resources.clsServer_NoResourceWasFoundWithTheID0, resourceId)
            Return map(resourceId)
        End Using
    End Function

    ''' <summary>
    ''' Gets the state of logging to windows event log for the specified resources,
    ''' or all resources if no IDs were specified.
    ''' </summary>
    ''' <param name="con">The connection over which to check the logging states.
    ''' </param>
    ''' <param name="resourceIds">The IDs for which the event logging states are
    ''' required.</param>
    ''' <returns>A map of event logging states keyed against their respective IDs.
    ''' True indicates event logging to windows event log is enabled; False indicates
    ''' that it is disabled.</returns>
    Private Function GetResourceEventLoggingStates(ByVal con As IDatabaseConnection,
      ByVal resourceIds As ICollection(Of Guid)) As IDictionary(Of Guid, Boolean)
        Dim cmd As New SqlCommand()

        ' If we were passed a null/empty collection, treat that as 'get all resources'
        Dim sb As New StringBuilder("select resourceid, logtoeventlog from BPAResource")

        ' If we do have some resource IDs, add constraints to focus on them
        If Not CollectionUtil.IsNullOrEmpty(resourceIds) Then
            sb.Append(" where resourceid in (")
            Dim i As Integer = 0
            For Each id As Guid In resourceIds
                If i > 0 Then sb.Append(","c)
                i += 1
                cmd.Parameters.AddWithValue("@id" & i, id)
                sb.AppendFormat("@id{0}", i)
            Next
            sb.Append(")")
        End If

        cmd.CommandText = sb.ToString()

        ' Read them in and return the map of enabled statuses to IDs
        Using reader = con.ExecuteReturnDataReader(cmd)
            Dim prov As New ReaderDataProvider(reader)
            Dim map As New Dictionary(Of Guid, Boolean)
            While reader.Read()
                map(prov.GetValue("resourceid", Guid.Empty)) = prov.GetValue("logtoeventlog", True)
            End While
            Return map
        End Using

    End Function

    ''' <summary>
    ''' Register a resource PC with the database. Any existing registration with the
    ''' same name is overridden, unless the 'update' parameter is False.
    ''' </summary>
    ''' <param name="name">The name address of the PC, which uniquely identifies
    ''' it and depending on configuration may also be used as the address.</param>
    ''' <param name="domainName">The fully-qualified domain name of the PC</param>
    ''' <param name="status">The initial status code</param>
    ''' <param name="attributes">The attributes for the resource</param>
    ''' <param name="requiresSsl">True if the Resource requires ssl connections.</param>
    ''' <param name="updateResourceRecord">True to update the Resource record if a resource with
    ''' the given name already exists. Otherwise, giving the name of an existing
    ''' Resource will result in an error being returned.</param>
    ''' <param name="userId">The id of the user the resource was started as.
    ''' This will be guid.empty if it is public</param>
    <SecuredMethod(Permission.Resources.AuthenticateAsResource)>
    Public Sub RegisterResourcePC(
            name As String,
            domainName As String,
            status As ResourceDBStatus,
            requiresSsl As Boolean,
            attributes As ResourceAttribute,
            updateResourceRecord As Boolean,
            userId As Guid,
            currentculture As String) _
            Implements IServer.RegisterResourcePC

        If Not attributes.HasAnyFlag(ResourceAttribute.Debug Or ResourceAttribute.Private) Then _
            CheckPermissions()

        Using con = GetConnection()
            con.BeginTransaction()
            RegisterResourcePC(con, name, domainName, status, requiresSsl, attributes, updateResourceRecord, userId, currentculture)
            con.CommitTransaction()
        End Using

        ' We only want to flush the cache if it's a newly registered resource.
        If Not updateResourceRecord Then InvalidateCaches()
    End Sub

    ''' <summary>
    ''' Registers the specified resource PC with the given status and attributes
    ''' </summary>
    ''' <param name="con">The connection to the database to use.</param>
    ''' <param name="name">The name address of the PC, which uniquely identifies
    ''' it and depending on configuration may also be used as the address.</param>
    ''' <param name="status">The status to set in the resource</param>
    ''' <param name="attribs">The resource attributes to set in the resource</param>
    ''' <param name="ssl">True if the Resource requires ssl connections.</param>
    ''' <param name="update">True to update an existing record, False to create a new
    ''' record</param>
    ''' <param name="userID">The id of the user the resource was started as.
    ''' This will be guid.empty if it is public</param>
    Private Sub RegisterResourcePC(con As IDatabaseConnection,
     name As String, fqdn As String, status As ResourceDBStatus,
     ssl As Boolean, attribs As ResourceAttribute,
     update As Boolean, userID As Guid, currentculture As String)

        Dim isPool = (attribs And ResourceAttribute.Pool) <> 0
        If isPool AndAlso Not Licensing.License.CanUse(LicenseUse.ResourcePools) Then
            Throw New BluePrismException(My.Resources.clsServer_ResourcePoolsCannotBeCreatedInTheNHSEdition)
        End If

        'Check we've been given a valid name before proceeding... (see bug #1831)
        If name.Length > 128 Then Throw New ArgumentException(
         String.Format(My.Resources.clsServer_ResourceName0TooLongMax128Characters, name))
        If name.Length = 0 Then Throw New ArgumentException(
         My.Resources.clsServer_ResourceNameCannotBeEmpty)

        'Only check validity for a pool. A normal resource PC must have a valid
        'name because it's the machine's name (and it *can* contain a : , for
        'the port).
        If isPool Then
            Dim ind As Integer = name.IndexOfAny("/\[]"":;|<>+=,?* _".ToCharArray())
            If ind <> -1 Then Throw New ArgumentException(
             String.Format(My.Resources.clsServer_ResourceNameInvalidCannotContain0, name(ind)))
        End If

        'See if a resource already exists with this name - if so
        'we will just update the record. Otherwise it's new, and we make sure
        'we're allowed to register a new one.
        Dim exfqdn As String = Nothing
        Dim id As Guid = GetResourceIdAndFQDN(con, name, exfqdn)

        Dim isNewResource = id = Guid.Empty

        If Not isNewResource Then
            If Not update Then Throw New AlreadyExistsException(
                My.Resources.clsServer_AResourceWithTheName0AlreadyExists, name)
        ElseIf Not isPool Then
            'Always allow pools to be registered
            Dim cmdv As New SqlCommand("SELECT PreventResourceRegistration FROM BPASysConfig")
            Dim value As Integer = CInt(con.ExecuteReturnScalar(cmdv))
            If value <> 0 Then Throw New BluePrismException(
                My.Resources.clsServer_RegistrationOfNewResourcesIsDisabled)
        End If

        If Not isPool AndAlso Not attribs.HasFlag(ResourceAttribute.DefaultInstance) Then
            ' apply the ASCR resource cap to resources
            EnsureResourceCapNotExceeded(name)
        End If

        'First check that this Resource is not retired
        If IsResourcePCRetired(con, id) Then Throw New BluePrismException(
            My.Resources.clsServer_CannotRegisterResourcePC0BecauseItHasBeenRetired, name)


        'Validate the FQDN.
        If isPool Then
            If fqdn IsNot Nothing Then
                Throw New BluePrismException(My.Resources.clsServer_FQDNShouldNotBeSpecifiedForAResourcePool)
            End If
        ElseIf Not isNewResource Then
            'The FQDN can't be changed, unless we're in MachineMachine mode, which is
            'supposed to be backwards-compatible.
            Dim tcmd As New SqlCommand("SELECT ResourceRegistrationMode FROM BPASysconfig")
            Dim mode As ResourceRegistrationMode = CType(con.ExecuteReturnScalar(tcmd), ResourceRegistrationMode)
            If mode <> ResourceRegistrationMode.MachineMachine Then
                If exfqdn IsNot Nothing AndAlso fqdn <> exfqdn Then
                    Throw New BluePrismException(
                     My.Resources.clsServer_CannotRegisterResourceDueToFQDNMismatchNew0Existing1,
                    fqdn, exfqdn)
                End If
            End If
            If fqdn Is Nothing Then Throw New BluePrismException(My.Resources.clsServer_FQDNMustBeSpecified)

        End If

        'If we're adding a new resource PC then check that the license permits another
        'resource to be added...
        If isNewResource Then
            Dim sErr As String = Nothing
            If Not CanActivateResource(con, Guid.Empty, attribs, sErr) Then
                If sErr = Licensing.MaxResourcesLimitReachedMessage Then _
                 sErr = Licensing.GetOperationDisallowedMessage(sErr)
                Throw New InvalidOperationException(sErr)
            End If
        End If



        'Now use a SQL command to either update or insert a new
        'record...
        Dim cmd As New SqlCommand()
        If Not isNewResource Then
            cmd.CommandText =
             " update bparesource set" &
             "   lastupdated=@lastupdated," &
             "   processesrunning=0," &
             "   actionsrunning=0," &
             "   unitsallocated=0," &
             "   statusid=@statusid, " &
             "   fqdn=@fqdn, " &
             "   ssl=@ssl, " &
             "   attributeid=@attributeid, " &
             "   userID=@userID, " &
             "   currentculture=@currentculture " &
             " where resourceid=@resourceid"
        Else
            id = Guid.NewGuid()
            cmd.CommandText =
             " insert into bparesource " &
             "   (resourceid," &
             "    name," &
             "    fqdn," &
             "    statusid," &
             "    ssl," &
             "    processesrunning," &
             "    actionsrunning," &
             "    unitsallocated," &
             "    lastupdated," &
             "    attributeid, " &
             "    userID," &
             "    currentculture)" &
             "   values " &
             "    (@resourceid,@name,@fqdn,@statusid,@ssl,0,0,0,@lastupdated,@attributeid,@userID,@currentculture);"


        End If

        With cmd.Parameters
            .AddWithValue("@name", name)
            .AddWithValue("@fqdn", IIf(fqdn Is Nothing, DBNull.Value, fqdn))
            .AddWithValue("@lastupdated", DateTime.UtcNow)
            .AddWithValue("@statusid", status)
            .AddWithValue("@ssl", IIf(ssl, 1, 0))
            .AddWithValue("@resourceid", id)
            .AddWithValue("@attributeid", attribs)
            .AddWithValue("@userID", IIf(userID = Guid.Empty, DBNull.Value, userID))
            .AddWithValue("@currentculture", currentculture)
        End With

        con.Execute(cmd)

        If isNewResource AndAlso HasDefaultGroup(con, Groups.GroupTreeType.Resources) Then
            Dim defaultGroupId = GetDefaultGroupId(con, Groups.GroupTreeType.Resources)
            Dim addToDefaultGroupCommand As New SqlCommand()
            addToDefaultGroupCommand.CommandText = "insert into bpagroupresource (groupid, memberid) values(@groupid, @resourceid);"
            addToDefaultGroupCommand.Parameters.AddWithValue("@groupid", defaultGroupId)
            addToDefaultGroupCommand.Parameters.AddWithValue("@resourceid", id)
            con.Execute(addToDefaultGroupCommand)
        End If
    End Sub

    ''' <summary>
    ''' Get the refresh interval for refreshing runtime resource information
    ''' to the database.
    ''' This is used for both session state and runtime resource state.
    ''' </summary>
    ''' <returns>The required update frequency in seconds</returns>
    <SecuredMethod(True)>
    Public Function GetRuntimeResourceRefreshFrequency() As Integer _
        Implements IServer.GetRuntimeResourceRefreshFrequency

        CheckPermissions()

        Return GetPref(RuntimeRefresh.RefreshFreqenecy, 5)

    End Function

    ''' <summary>
    ''' Get the refresh interval for refreshing runtime resource information
    ''' to the database.
    ''' This is used for both session state and runtime resource state.
    ''' </summary>
    ''' <param name="freqSeconds">The new refresh frequency in seconds</param>
    <SecuredMethod(True)>
    Public Sub SetRuntimeResourceRefreshFrequency(freqSeconds As Integer) _
        Implements IServer.SetRuntimeResourceRefreshFrequency
        CheckPermissions()
        Using con = GetConnection()
            SetPref(Of Integer)(con,
                                RuntimeRefresh.RefreshFreqenecy,
                                Nothing,
                                freqSeconds)
        End Using
    End Sub

    ''' <summary>
    ''' Refresh the current state of the resource PC.
    ''' </summary>
    ''' <param name="resourceName">Name of resource being refreshed</param>
    ''' <param name="status">Current status of resource</param>
    ''' <param name="runningSessions">Number of running sessions</param>
    ''' <param name="activeSessions">Number of running actions</param>
    ''' <remarks></remarks>
    <SecuredMethod(True)>
    Public Sub RefreshResourcePC(ByVal resourceName As String,
                                  ByVal status As ResourceDBStatus,
                                  ByVal runningSessions As Integer,
                                  ByVal activeSessions As Integer) _
                                Implements IServer.RefreshResourcePC
        CheckPermissions()

        If String.IsNullOrEmpty(resourceName) Then _
            Throw New ArgumentException(My.Resources.clsServer_ResourceNameCannotBeEmpty,
NameOf(resourceName))

        Using con = GetConnection()

            ' Check the resource exists.
            Dim exfqdn As String = Nothing
            Dim id As Guid = GetResourceIdAndFQDN(con, resourceName, exfqdn)
            If id = Guid.Empty Then
                Throw New BluePrismException(
                    My.Resources.clsServer_CannotUdpateStatusOfResourcePC0BecauseItDoesnTExist,
                    resourceName)
            End If

            'Now use a SQL command to update the record...
            Using cmd As New SqlCommand(
                 " update bparesource set" &
                 "   lastupdated=@lastupdated," &
                 "   unitsallocated=0," &
                 "   statusid=@statusid, " &
                 "   processesrunning=@running, " &
                 "   actionsrunning=@active " &
                 " where resourceid=@resourceid")

                With cmd.Parameters
                    .AddWithValue("@lastupdated", DateTime.UtcNow)
                    .AddWithValue("@statusid", status)
                    .AddWithValue("@resourceid", id)
                    .AddWithValue("@running", runningSessions)
                    .AddWithValue("@active", activeSessions)
                End With

                con.Execute(cmd)
            End Using
        End Using
    End Sub



    ''' <summary>
    ''' Updates the BPASession table with runtime session info which includes
    ''' the last stage to be run, and the time at which the stage was started
    ''' </summary>
    ''' <param name="sessionNo">The session number to update</param>
    ''' <param name="lastStage">The name of the last stage </param>
    ''' <param name="lastStageUpdated">The date/time (UTC) the stage was updated</param>
    ''' <param name="warningThreshold">The stage warning threshold in seconds</param>
    <SecuredMethod(True)>
    Public Sub RefreshSessionInfo(sessionNo As Integer, lastStage As String,
                                  lastStageUpdated As DateTimeOffset, warningThreshold As Integer) _
      Implements IServer.RefreshSessionInfo
        CheckPermissions()

        Using con = GetConnection()
            UpdateSessionInfo(con, sessionNo, lastStage, lastStageUpdated, warningThreshold)
        End Using

    End Sub

    ''' <summary>
    ''' Updates the BPASession table with runtime session info which includes
    ''' the last stage to be run, and the time at which the stage was started
    ''' </summary>
    ''' <param name="con">The database connection</param>
    ''' <param name="sessionNo">The session number to update</param>
    ''' <param name="lastStage">The name of the last stage </param>
    ''' <param name="lastStageUpdated">The date/time (UTC) the stage was updated</param>
    ''' <param name="warningThreshold">The stage warning threshold in seconds</param>
    Private Sub UpdateSessionInfo(con As IDatabaseConnection, sessionNo As Integer, lastStage As String,
                                  lastStageUpdated As DateTimeOffset, warningThreshold As Integer)
        Using cmd As New SqlCommand(
            " update bpasession set " &
            "   laststage=@laststage, " &
            "   lastupdated=@lastupdated, " &
            "   lastupdatedtimezoneoffset=@lastupdatedtimezoneoffset, " &
            "   warningthreshold=@threshold " &
            " where sessionnumber = @sessno")
            With cmd.Parameters
                .AddWithValue("@laststage", lastStage)
                .AddWithValue("@lastupdated", lastStageUpdated.DateTime)
                .AddWithValue("@lastupdatedtimezoneoffset", lastStageUpdated.Offset.TotalSeconds)
                .AddWithValue("@threshold", warningThreshold)
                .AddWithValue("@sessno", sessionNo)
            End With
            con.Execute(cmd)
        End Using
    End Sub

    ''' <summary>
    ''' Resource PCs call this on startup (after registration) and also periodically,
    ''' to keep track of pool information. If the Resource PC is a member of a pool
    ''' then information about the controller of the pool is returned. Additionally,
    ''' the Resource PC may take over controllership of the pool during this call.
    '''
    ''' In order to reduce unnecessary database load, information about the Resource
    ''' PC's intended diagnostics status is retrieved and returned at the same time,
    ''' as is auto-archiving status.
    ''' </summary>
    ''' <param name="resourceID">The ID of the Resource PC making the call.</param>
    ''' <param name="iscontroller">True if the Resource PC believes itself to be
    ''' the controller of the pool.</param>
    ''' <param name="poolID">On return, contains the ID of the Pool the Resource PC
    ''' is a member of, or Guid.Empty if none.</param>
    ''' <param name="controllerID">On return, contains the ID of the controller of
    ''' the pool. Will be Guid.Empty if the Resource PC is not a member of a pool,
    ''' or if the pool has no controller. The latter should not happen!</param>
    ''' <param name="iscontrollernow">On return, contains True if the Resource PC has
    ''' become the controller of its pool during the call.</param>
    ''' <param name="diags">On return, contains the diagnostics flags for the
    ''' Resource PC.</param>
    ''' <param name="isAutoArchiver">On return, contains True if the Resource is
    ''' designated as the auto-archiving machine.</param>
    <SecuredMethod(True)>
    Public Sub PoolUpdate(ByVal resourceID As Guid, ByVal iscontroller As Boolean, ByRef poolID As Guid, ByRef controllerID As Guid, ByRef iscontrollernow As Boolean, ByRef diags As Integer, ByRef isAutoArchiver As Boolean) Implements IServer.PoolUpdate
        CheckPermissions()

        Dim con = GetConnection()
        Try
            'Determine auto-archive status...
            Dim cmd As New SqlCommand()
            If IsAutoArchiving(con) Then
                cmd.CommandText = "select ArchivingResource from BPASysConfig"
                isAutoArchiver =
                 (IfNull(con.ExecuteReturnScalar(cmd), Guid.Empty) = resourceID)
            Else
                isAutoArchiver = False
            End If

            'Get the pool ID.
            cmd.CommandText =
             "select pool,diagnostics from BPAResource where resourceid = @resid"
            cmd.Parameters.AddWithValue("@resid", resourceID)
            Dim dt As DataTable = con.ExecuteReturnDataTable(cmd)
            If dt.Rows.Count <> 1 Then
                Throw New Exception(My.Resources.clsServer_MissingResourceRecordDuringPoolUpdate)
            End If
            diags = CInt(dt.Rows(0)(1))
            Dim p As Object = dt.Rows(0)(0)
            If TypeOf (p) Is DBNull Then
                'It's a normal Resource PC, not in a pool...
                poolID = Guid.Empty
                controllerID = Guid.Empty
                iscontrollernow = False
                Return
            End If
            poolID = CType(p, Guid)

            cmd = New SqlCommand("select controller,lastupdated from BPAResource (XLOCK) where resourceid = @PoolID")
            With cmd.Parameters
                .AddWithValue("@PoolID", poolID)
            End With
            con.BeginTransaction()
            Dim c As Object = Nothing, lastupdated As DateTime
            Using reader = con.ExecuteReturnDataReader(cmd)
                Do While reader.Read()
                    c = reader("controller")
                    lastupdated = If(reader("lastupdated") Is DBNull.Value, Date.MinValue, CDate(reader("lastupdated")))
                Loop
            End Using
            If TypeOf c Is DBNull Or DateTime.UtcNow - lastupdated > New TimeSpan(0, 4, 0) Then
                'There is no controller, or its record is out of date, so we are taking over...
                cmd = New SqlCommand("update BPAResource set controller=@ResourceID, lastupdated=GETUTCDATE(), statusid=@StatusID where resourceid=@PoolID")
                With cmd.Parameters
                    .AddWithValue("@ResourceID", resourceID)
                    .AddWithValue("@PoolID", poolID)
                    .AddWithValue("@StatusID", ResourceDBStatus.Ready)
                End With
                con.Execute(cmd)
                con.CommitTransaction()
                controllerID = resourceID
                iscontrollernow = True
                Return
            Else
                'There is already a controller, and its record is fresh.
                controllerID = CType(c, Guid)
                iscontrollernow = False
                If iscontroller Then
                    If c.Equals(resourceID) Then
                        'Update the controller record.
                        cmd = New SqlCommand("update BPAResource set lastupdated=GETUTCDATE(), statusid=@StatusID where resourceid=@PoolID")
                        With cmd.Parameters
                            .AddWithValue("@PoolID", poolID)
                            .AddWithValue("@StatusID", ResourceDBStatus.Ready)
                        End With
                        con.Execute(cmd)
                        con.CommitTransaction()
                        Return
                    Else
                        con.RollbackTransaction()
                        Throw New Exception(My.Resources.clsServer_ResourcePCBelievesItIsThePoolControllerButItIsNot)
                    End If
                End If
                Return
            End If

        Catch e As Exception
            Throw e
        Finally
            con.Close()
        End Try

    End Sub


    ''' <summary>
    ''' Deregisters the resource PC with the given name from the database. This
    ''' happens when a resource PC shuts down - the record for the resource PC is not
    ''' physically removed, but the status is set to offline.
    ''' </summary>
    ''' <param name="name">The resource name</param>
    <SecuredMethod(True)>
    Public Sub DeregisterLoginAgent(name As String) Implements IServer.DeregisterLoginAgent
        CheckPermissions()
        Using con = GetConnection()
            Dim id = GetResourceId(con, name)
            SubtractFromResourceAttributes(con, id, ResourceAttribute.LoginAgent)
            DeregisterResourcePC(con, name)
        End Using
    End Sub

    ''' <summary>
    ''' Deregisters the resource PC with the given name from the database. This
    ''' happens when a resource PC shuts down - the record for the resource PC is not
    ''' physically removed, but the status is set to offline.
    ''' </summary>
    ''' <param name="name">The resource name</param>
    <SecuredMethod(True)>
    Public Sub DeregisterResourcePC(name As String) Implements IServer.DeregisterResourcePC
        CheckPermissions()
        Using con = GetConnection()
            DeregisterResourcePC(con, name)
        End Using
    End Sub

    ''' <summary>
    ''' Deregisters the resource PC with the given name from the database. This
    ''' happens when a resource PC shuts down - the record for the resource PC is not
    ''' physically removed, but the status is set to offline.
    ''' </summary>
    ''' <param name="name">The name of the resource to deregister</param>
    Private Sub DeregisterResourcePC(con As IDatabaseConnection, name As String)

        Dim deregisterResourceId = GetResourceId(con, name)

        Dim sErr As String = ""
        Dim poolId As Guid = Guid.Empty
        Dim controllerId As Guid = Guid.Empty
        GetResourcePoolInfo(deregisterResourceId, poolId, controllerId)

        con.BeginTransaction()
        ' Set resource to offline, clear user if the resource was private as it may not be the same user next time
        Using cmd As New SqlCommand("UPDATE BPAResource SET statusid=@statusid, userid=NULL WHERE resourceid = @resourceid")
            With cmd.Parameters
                .AddWithValue("@resourceid", deregisterResourceId)
                .AddWithValue("@statusid", ResourceDBStatus.Offline)
            End With
            con.Execute(cmd)
        End Using

        If poolId <> Guid.Empty AndAlso controllerId = deregisterResourceId Then
            Using poolCmd = New SqlCommand("update BPAResource set controller=@ResourceID, lastupdated=@lastupdated, statusid=@StatusID where resourceid=@PoolID")
                With poolCmd.Parameters
                    .AddWithValue("@ResourceID", DBNull.Value)
                    .AddWithValue("@PoolID", poolId)
                    .AddWithValue("@StatusID", ResourceDBStatus.Offline)
                    .AddWithValue("@lastupdated", DBNull.Value)
                End With
                con.Execute(poolCmd)
            End Using
        End If
        con.CommitTransaction()
    End Sub

    ''' <summary>
    ''' Gets the IDs of the resource pools and their members.
    ''' </summary>
    ''' <returns>A dictionary mapping a set of resource IDs against their owning pool
    ''' IDs. The first entry in the collection representing the members will be the
    ''' currently registered controller of the pool, followed by the other members
    ''' in arbitrary order.</returns>
    <SecuredMethod(True)>
    Public Function GetPoolResourceIds() As IDictionary(Of Guid, ICollection(Of Guid)) Implements IServer.GetPoolResourceIds
        CheckPermissions()
        Using con = GetConnection()
            Return GetPoolResourceIds(con)
        End Using
    End Function

    ''' <summary>
    ''' Gets the IDs of the resource pools and their members.
    ''' </summary>
    ''' <param name="con">The connection to the database to use</param>
    ''' <returns>A dictionary mapping a set of resource IDs against their owning pool
    ''' IDs. The first entry in the collection representing the members will be the
    ''' currently registered controller of the pool, followed by the other members
    ''' in arbitrary order.</returns>
    Private Function GetPoolResourceIds(con As IDatabaseConnection) _
     As IDictionary(Of Guid, ICollection(Of Guid))
        Return GetPoolResourceIds(con, Guid.Empty)
    End Function

    ''' <summary>
    ''' Gets the IDs of the resource pools and their members, or just the members of
    ''' a single pool if a pool ID is given.
    ''' </summary>
    ''' <param name="con">The connection to the database to use</param>
    ''' <param name="poolId">The ID of the pool for which the member resource Ids
    ''' are required; <see cref="Guid.Empty"/> to return all pools.</param>
    ''' <returns>A dictionary mapping a set of resource IDs against their owning pool
    ''' IDs. The first entry in the collection representing the members will be the
    ''' currently registered controller of the pool, followed by the other members
    ''' in arbitrary order.</returns>
    Private Function GetPoolResourceIds(con As IDatabaseConnection, poolId As Guid) _
     As IDictionary(Of Guid, ICollection(Of Guid))
        Dim cmd As New SqlCommand()

        ' Get the pools along with their current controller and all the members
        cmd.CommandText =
         " select" &
         "   pool.resourceid as poolid," &
         "   pool.controller as controller," &
         "   child.resourceid as childid" &
         " from BPAResource pool " &
         "   join BPAResource child on child.pool = pool.resourceid" &
         " where (pool.attributeid & @poolattr) != 0"

        ' Focus in on a single pool if we're given one to focus in on
        If poolId <> Guid.Empty Then
            cmd.CommandText &= " and pool.resourceid = @poolid"
            cmd.Parameters.AddWithValue("@poolid", poolId)
        End If

        cmd.Parameters.AddWithValue("@poolattr", ResourceAttribute.Pool)

        Dim pools As New Dictionary(Of Guid, ICollection(Of Guid))
        Using reader = con.ExecuteReturnDataReader(cmd)
            Dim prov As New ReaderDataProvider(reader)
            While reader.Read()
                Dim resId As Guid = prov.GetGuid("poolid")
                Dim members As ICollection(Of Guid) = Nothing
                If Not pools.TryGetValue(resId, members) Then
                    members = New clsOrderedSet(Of Guid)
                    pools(resId) = members
                    ' We want the first entry in the collection to be the controller
                    ' (if one is present in the pool), so add that immediately
                    Dim controllerId As Guid = prov.GetGuid("controller")
                    If controllerId <> Guid.Empty Then members.Add(controllerId)
                End If
                members.Add(prov.GetGuid("childid"))
            End While
        End Using

        Return pools

    End Function

    ''' <summary>
    ''' Get the name of the Resource that is the controller of given pool.
    ''' </summary>
    ''' <param name="poolid">The ID of the pool.</param>
    ''' <param name="name">On successful return, contains the controller name.</param>
    <SecuredMethod(True)>
    Public Sub GetPoolControllerName(ByVal poolid As Guid, ByRef name As String) Implements IServer.GetPoolControllerName
        CheckPermissions()

        Dim con = GetConnection()
        Try

            Dim cmd As New SqlCommand("select controller from BPAResource where resourceid = @ResourceID")
            With cmd.Parameters
                .AddWithValue("@ResourceID", poolid)
            End With
            Dim cid As Object = con.ExecuteReturnScalar(cmd)
            If TypeOf cid Is DBNull Then
                Throw New Exception(My.Resources.clsServer_PoolNotFound)
            End If
            Dim controllerid As Guid = CType(cid, Guid)

            'Get the name
            cmd = New SqlCommand("select name from BPAResource where resourceid = @ResourceID")
            With cmd.Parameters
                .AddWithValue("@ResourceID", controllerid)
            End With
            name = CStr(con.ExecuteReturnScalar(cmd))
        Catch ex As Exception
            Throw ex
        Finally
            con.Close()
        End Try

    End Sub


    ''' <summary>
    ''' Get information about a Resource's pool status.
    ''' </summary>
    ''' <param name="resid">The ID of the Resource to get information about.</param>
    ''' <param name="poolid">On return, contains the ID of the pool the Resource is
    ''' a member of, or Guid.Empty if the Resource is not a member of one.</param>
    ''' <param name="controllerid">On return, contains the ID of the controller of
    ''' that pool. This will be Guid.Empty if there is no controller, or no pool.
    ''' </param>
    <SecuredMethod(True)>
    Public Sub GetResourcePoolInfo(ByVal resid As Guid, ByRef poolid As Guid, ByRef controllerid As Guid) Implements IServer.GetResourcePoolInfo
        CheckPermissions()

        Dim con = GetConnection()
        'Make sure the resource is not already in a pool.
        Dim cmd As New SqlCommand("select pool from BPAResource where resourceid = @ResourceID")
        cmd.Parameters.AddWithValue("@ResourceID", resid)
        Dim res As Object = con.ExecuteReturnScalar(cmd)
        If TypeOf res Is DBNull Then
            poolid = Guid.Empty
            controllerid = Guid.Empty
        Else
            poolid = CType(res, Guid)
            cmd = New SqlCommand("select controller from BPAResource where resourceid = @PoolID")
            cmd.Parameters.AddWithValue("@PoolID", poolid)
            res = con.ExecuteReturnScalar(cmd)
            If TypeOf res Is DBNull Then
                controllerid = Guid.Empty
            Else
                controllerid = CType(res, Guid)
            End If
        End If
        con.Close()

    End Sub


    ''' <summary>
    ''' Add a Resource PC to a Resource Pool.
    ''' </summary>
    ''' <param name="poolName">The name of the pool to add it to.</param>
    ''' <param name="resourceName">The name of the Resource being added.</param>
    <SecuredMethod(Permission.SystemManager.Resources.Pools)>
    Public Sub AddResourceToPool(poolName As String, resourceName As String) Implements IServer.AddResourceToPool
        CheckPermissions()

        Using con = GetConnection()

            ' Validate passed parameters
            Dim poolId = GetResourceId(con, poolName)
            If poolId = Guid.Empty Then Throw New InvalidArgumentException(
                My.Resources.clsServer_CannotFindPoolToAddTo)

            Dim resourceId = GetResourceId(con, resourceName)
            If resourceId = Guid.Empty Then Throw New InvalidArgumentException(
                My.Resources.clsServer_CannotFindResourceToAdd)

            Dim attributes = GetResourceAttributes(con, poolId)
            If (attributes And ResourceAttribute.Pool) = 0 Then Throw New InvalidArgumentException(
                My.Resources.clsServer_CannotAddTargetIsNotAPool)

            attributes = GetResourceAttributes(con, resourceId)
            If (attributes And ResourceAttribute.Pool) <> 0 Then Throw New InvalidArgumentException(
                My.Resources.clsServer_CannotAddAPoolToAPool)

            'Make sure the resource is not already in a pool.
            Dim cmd As New SqlCommand("select pool from BPAResource where resourceid = @ResourceID")
            With cmd.Parameters
                .AddWithValue("@ResourceID", resourceId)
            End With
            If Not (TypeOf con.ExecuteReturnScalar(cmd) Is DBNull) Then Throw New InvalidStateException(
                    My.Resources.clsServer_TheResourceAlreadyInAPool)

            'Add it to the pool
            cmd = New SqlCommand("update BPAResource set pool = @PoolID where resourceid = @ResourceID")
            With cmd.Parameters
                .AddWithValue("@ResourceID", resourceId)
                .AddWithValue("@PoolID", poolId)
            End With
            con.BeginTransaction()
            con.Execute(cmd)
            AuditRecordResourceEvent(con, ResourceEventCode.AddResourceToPool, resourceName, poolName, "")
            RemoveClonedResources(con, resourceId)
            con.CommitTransaction()
        End Using
        InvalidateCaches()
    End Sub

    ''' <summary>
    ''' Remove a Resource from a Resource Pool.
    ''' </summary>
    ''' <param name="resourceName">The name of the Resource being removed.</param>
    <SecuredMethod(Permission.SystemManager.Resources.Pools)>
    Public Sub RemoveResourceFromPool(resourceName As String) Implements IServer.RemoveResourceFromPool
        CheckPermissions()

        Using con = GetConnection()
            Dim resourceId = GetResourceId(con, resourceName)
            If resourceId = Guid.Empty Then Throw New InvalidArgumentException(
                My.Resources.clsServer_CannotFindResourceToRemove)

            'Make sure the resource is actually in a pool.
            Dim cmd As New SqlCommand("select pool from BPAResource where resourceid = @ResourceID")
            With cmd.Parameters
                .AddWithValue("@ResourceID", resourceId)
            End With
            Dim p As Object = con.ExecuteReturnScalar(cmd)
            If TypeOf p Is DBNull Then Throw New InvalidStateException(
                My.Resources.clsServer_TheResourceIsNotInAPool)
            Dim poolId As Guid = CType(p, Guid)

            'Remove it from the pool
            con.BeginTransaction()
            cmd = New SqlCommand("update BPAResource set pool = NULL where resourceid = @ResourceID")
            With cmd.Parameters
                .AddWithValue("@ResourceID", resourceId)
            End With
            con.Execute(cmd)

            'Check if the resource is the controller of the pool...
            cmd = New SqlCommand("select controller from BPAResource where resourceid = @PoolID")
            cmd.Parameters.AddWithValue("@PoolID", poolId)
            p = con.ExecuteReturnScalar(cmd)
            If (Not TypeOf p Is DBNull) AndAlso CType(p, Guid).Equals(resourceId) Then
                'It is - but it can't be any more!
                cmd = New SqlCommand("update BPAResource set controller=NULL where resourceid = @PoolID")
                cmd.Parameters.AddWithValue("@PoolID", poolId)
                con.Execute(cmd)
            End If
            Dim poolName = GetResourceName(con, poolId)
            AuditRecordResourceEvent(con, ResourceEventCode.RemoveResourceFromPool, resourceName, poolName, "")

            Dim defaultId = GetDefaultGroupId(Groups.GroupTreeType.Resources)

            cmd = New SqlCommand(
            " if not exists (select 1 from BPAGroupResource where groupid = @groupid and memberid = @memberid)
            begin
            insert into BPAGroupResource (groupid, memberid) values (@groupid, @memberid)
            end ")

            With cmd.Parameters
                .AddWithValue("@groupid", defaultId)
                .AddWithValue("@memberid", resourceId)
            End With
            con.Execute(cmd)
            con.CommitTransaction()
        End Using
        InvalidateCaches()
    End Sub

    ''' <summary>
    ''' Create a Resource Pool in from the database with the passed name
    ''' </summary>
    ''' <param name="poolName">The resource pool name</param>
    <SecuredMethod(Permission.SystemManager.Resources.Pools)>
    Public Sub CreateResourcePool(poolName As String) Implements IServer.CreateResourcePool
        CheckPermissions()

        Using con = GetConnection()
            con.BeginTransaction()
            RegisterResourcePC(con, poolName, Nothing, ResourceDBStatus.Offline,
                                           False, ResourceAttribute.Pool, False, Guid.Empty, CultureInfo.CurrentCulture.NativeName)
            AuditRecordResourceEvent(con, ResourceEventCode.CreatePool, "", poolName, "")
            con.CommitTransaction()
        End Using

        InvalidateCaches()
    End Sub

    ''' <summary>
    ''' Delete a Resource Pool from the database.
    ''' </summary>
    ''' <param name="poolName">The resource pool name</param>
    <SecuredMethod(Permission.SystemManager.Resources.Pools)>
    Public Sub DeleteResourcePool(poolName As String) Implements IServer.DeleteResourcePool
        CheckPermissions()

        Using connection = GetConnection()

            Dim poolId = GetResourceId(poolName)
            If poolId = Guid.Empty Then Throw New InvalidArgumentException(
                My.Resources.clsServer_CannotFindPoolToDelete)

            Dim attributes = GetResourceAttributes(poolName)
            If (attributes And ResourceAttribute.Pool) = 0 Then Throw New InvalidArgumentException(
                My.Resources.clsServer_CannotDeleteAPhysicalResourceOnlyAPool)


            connection.BeginTransaction()

            ' Delete all the tables which have an non-cascaded FK to BPAResource first,
            ' then get rid of the BPAResource record.
            Using command As New SqlCommand("DELETE FROM BPACredentialsResources WHERE resourceid = @ResourceID;")
                With command.Parameters
                    .AddWithValue("@ResourceID", poolId)
                End With
                connection.Execute(command)
            End Using

            Dim defaultId = GetDefaultGroupId(Groups.GroupTreeType.Resources)

            Using command = New SqlCommand("
                insert into BPAGroupResource (
                    groupid,
                    memberid)
                select
                    @groupid,
                    resourceid
                from BPAResource
                where pool =@poolId and resourceid not in
                (select  memberid
                from BPAGroupResource where groupid = @groupid)")

                With command.Parameters
                    .AddWithValue("@groupid", defaultId)
                    .AddWithValue("@poolId", poolId)
                End With
                connection.Execute(command)
            End Using

            'Make sure the pool has no members before we try and delete it.
            Using command = New SqlCommand("UPDATE BPAResource SET pool = NULL WHERE pool = @poolId")
                With command.Parameters
                    .AddWithValue("@poolId", poolId)
                End With
                connection.Execute(command)
            End Using

            'Delete the pool
            Using command = New SqlCommand("DELETE FROM BPAResource WHERE resourceid = @ResourceID")
                With command.Parameters
                    .AddWithValue("@ResourceID", poolId)
                End With
                connection.Execute(command)
            End Using

            AuditRecordResourceEvent(connection, ResourceEventCode.DeletePool, "", poolName, "")
            connection.CommitTransaction()
        End Using
        InvalidateCaches()
    End Sub

    ''' <summary>
    ''' Get the ID of a Resource from the database, given its name.
    ''' </summary>
    ''' <param name="sResourceName">The Resource name</param>
    ''' <returns>The Resource ID, or Guid.Empty if the named Resource is not
    ''' registered in the database.</returns>
    <SecuredMethod(True)>
    Public Function GetResourceId(ByVal sResourceName As String) As Guid Implements IServer.GetResourceId
        CheckPermissions()

        Try
            Using con = GetConnection()
                Return GetResourceId(con, sResourceName)
            End Using

        Catch e As Exception
            ' Ignore, against my better judgement... but it's too scattered around
            ' the app to go around changing everything without good cause.
            Return Guid.Empty

        End Try

    End Function

    ''' <summary>
    ''' Internal method to get a resource ID - unlike the public method this one
    ''' will actually throw an exception if any problems occur while getting the
    ''' resource ID.
    ''' </summary>
    ''' <param name="con">The connection over which the resource ID should be
    ''' retrieved.</param>
    ''' <param name="resourceName">The name of the resource for which the ID is
    ''' required. </param>
    ''' <returns>The ID of the resource on the database which has the given name
    ''' as its name, or Guid.Empty if no such resource was found.</returns>
    ''' <exception cref="Exception">An arbitrary exception might be thrown if the
    ''' database code fails for any reason.</exception>
    Private Function GetResourceId(ByVal con As IDatabaseConnection, ByVal resourceName As String) As Guid

        Dim cmd As New SqlCommand("SELECT resourceid FROM BPAResource where name = @name")
        cmd.Parameters.AddWithValue("@name", resourceName)

        Dim id As Guid = Guid.Empty

        Using reader = con.ExecuteReturnDataReader(cmd)
            Do While reader.Read()
                id = DirectCast(reader("resourceid"), Guid)
            Loop
        End Using

        Return id

    End Function


    ''' <summary>
    ''' Check if any of a set of Resources have an FQDN set.
    ''' </summary>
    ''' <param name="resourceIDs">A List of Resource IDs</param>
    ''' <returns>True if one or more of the given Resources have an FQDN set.
    ''' </returns>
    ''' <exception cref="Exception">An arbitrary exception might be thrown if the
    ''' database code fails for any reason.</exception>
    <SecuredMethod(True)>
    Public Function ResourcesHaveFQDN(ByVal resourceIDs As ICollection(Of Guid)) As Boolean Implements IServer.ResourcesHaveFQDN
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand()

            Dim sb As New StringBuilder("SELECT COUNT(*) FROM BPAResource where fqdn is not null and resourceid in (")
            Dim i As Integer = 0
            For Each id As Guid In resourceIDs
                If i > 0 Then sb.Append(","c)
                i += 1
                cmd.Parameters.AddWithValue("@id" & i, id)
                sb.AppendFormat("@id{0}", i)
            Next
            sb.Append(")")
            cmd.CommandText = sb.ToString()
            Return CInt(con.ExecuteReturnScalar(cmd)) > 0
        End Using
    End Function

    ''' <summary>
    ''' Reset the FQDN for the given Resource.
    ''' </summary>
    ''' <param name="resourceId">Resource ID</param>
    ''' <exception cref="Exception">An arbitrary exception might be thrown if the
    ''' database code fails for any reason.</exception>
    <SecuredMethod(Permission.Resources.ConfigureResource)>
    Public Sub ResetResourceFQDN(resourceId As Guid) Implements IServer.ResetResourceFQDN

        ' Check role-based permissions
        CheckPermissions()

        Using con = GetConnection()
            ' Check group-based permissions
            If Not HasPermissionOnResource(con, resourceId, Permission.Resources.ConfigureResource) Then
                Throw New PermissionException(
                    My.Resources.clsServer_TheCurrentUserDoesNotHavePermissionToConfigureThisResource)
            End If

            Dim cmd As New SqlCommand("
                update BPAResource
                set fqdn = null
                where resourceid = @resid")

            cmd.Parameters.AddWithValue("@resid", resourceId)
            con.Execute(cmd)
        End Using

    End Sub

    ''' <summary>
    ''' Internal method to get a resource ID and its FQDN.
    ''' </summary>
    ''' <param name="con">The connection over which the resource ID should be
    ''' retrieved.</param>
    ''' <param name="resourceName">The name of the resource for which the ID is
    ''' required. </param>
    ''' <param name="fqdn">On successful return (ID is not Guid.empty) this
    ''' contains the FQDN. Otherwise, it's undefined.</param>
    ''' <returns>The ID of the resource on the database which has the given name
    ''' as its name, or Guid.Empty if no such resource was found.</returns>
    ''' <exception cref="Exception">An arbitrary exception might be thrown if the
    ''' database code fails for any reason.</exception>
    Private Function GetResourceIdAndFQDN(ByVal con As IDatabaseConnection, ByVal resourceName As String, ByRef fqdn As String) As Guid

        Dim cmd As New SqlCommand("SELECT resourceid, fqdn FROM BPAResource where name = @name")
        cmd.Parameters.AddWithValue("@name", resourceName)

        Dim id As Guid = Guid.Empty

        Using reader = con.ExecuteReturnDataReader(cmd)
            Do While reader.Read()
                id = DirectCast(reader("resourceid"), Guid)
                Dim fqdnval As Object = reader("fqdn")
                If Not Convert.IsDBNull(fqdnval) Then
                    fqdn = CStr(fqdnval)
                Else
                    fqdn = Nothing
                End If
            Loop
        End Using

        Return id

    End Function


    ''' <summary>
    ''' Get the attributes of a resource from the database, given its name.
    ''' </summary>
    ''' <param name="resourceName">The Resource name</param>
    ''' <returns>The attributes of the resource.</returns>
    ''' <remarks>Throws an exception in the event of an error.</remarks>
    <SecuredMethod(True)>
    Public Function GetResourceAttributes(ByVal resourceName As String) As ResourceAttribute Implements IServer.GetResourceAttributes
        CheckPermissions()
        Dim con = GetConnection()

        Try
            Dim cmd As New SqlCommand("SELECT AttributeID FROM BPAResource where name = @ResourceName")
            With cmd.Parameters
                .AddWithValue("@ResourceName", resourceName)
            End With

            Dim attr As ResourceAttribute = CType(con.ExecuteReturnScalar(cmd), ResourceAttribute)
            Return attr

        Finally
            con.Close()
        End Try

    End Function


    ''' <summary>
    ''' Get the attributes of a resource from the database.
    ''' </summary>
    ''' <param name="resourceID">The Resource ID</param>
    ''' <param name="con">The database connection</param>
    ''' <returns>The attributes of the resource.</returns>
    ''' <remarks>Throws an exception in the event of an error.</remarks>
    Private Function GetResourceAttributes(con As IDatabaseConnection, resourceID As Guid) As ResourceAttribute
        Using cmd As New SqlCommand("SELECT AttributeID FROM BPAResource WHERE resourceid = @ResourceID")
            With cmd.Parameters
                .AddWithValue("@ResourceID", resourceID)
            End With

            Return CType(con.ExecuteReturnScalar(cmd), ResourceAttribute)
        End Using
    End Function


    ''' <summary>
    ''' Get the diagnostics flags for the given Resource PC.
    ''' </summary>
    ''' <param name="resid">The ID of the Resource PC.</param>
    ''' <returns>The diagnostics flags.</returns>
    ''' <remarks>Throws an exception in the event of an error.</remarks>
    <SecuredMethod(True)>
    Public Function GetResourceDiagnostics(ByVal resid As Guid) As Integer Implements IServer.GetResourceDiagnostics
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand("select diagnostics from BPAResource where resourceid = @id")
            cmd.Parameters.AddWithValue("@id", resid)
            Return CType(con.ExecuteReturnScalar(cmd), Integer)
        End Using
    End Function

    ''' <summary>
    ''' Get the diagnostics flags and settings for the given Resource PCs.
    ''' </summary>
    ''' <param name="ids">The IDs of the Resource PCs</param>
    ''' <returns>The combined configuration settings for the specified resources.
    ''' </returns>
    ''' <remarks>Throws an exception in the event of an error.</remarks>
    <SecuredMethod(True)>
    Public Function GetResourceDiagnosticsCombined(ByVal ids As ICollection(Of Guid)) _
     As CombinedConfig Implements IServer.GetResourceDiagnosticsCombined
        CheckPermissions()
        Using con = GetConnection()
            Return GetResourceDiagnosticsCombined(con, ids)
        End Using
    End Function

    ''' <summary>
    ''' Get the diagnostics flags and settings for the given Resource PCs.
    ''' </summary>
    ''' <param name="con">The connection over which the resource diagnostics and
    ''' settings should be retrieved.</param>
    ''' <param name="ids">The IDs of the Resource PCs</param>
    ''' <returns>The combined configuration settings for the specified resources.
    ''' </returns>
    ''' <remarks>Throws an exception in the event of an error.</remarks>
    Private Function GetResourceDiagnosticsCombined(ByVal con As IDatabaseConnection,
     ByVal ids As ICollection(Of Guid)) As CombinedConfig

        Dim cmd As New SqlCommand()

        ' If we were passed a null/empty collection, treat that as 'get all resources'
        Dim sb As New StringBuilder("select diagnostics, logtoeventlog from bparesource")

        ' If we do have some resource IDs, add constraints to focus on them
        If Not CollectionUtil.IsNullOrEmpty(ids) Then
            sb.Append(" where resourceid in (")
            Dim i As Integer = 0
            For Each id As Guid In ids
                If i > 0 Then sb.Append(","c)
                i += 1
                cmd.Parameters.AddWithValue("@id" & i, id)
                sb.AppendFormat("@id{0}", i)
            Next
            sb.Append(")")
        End If
        cmd.CommandText = sb.ToString()

        ' Read the data in and combine it into a CombinedConfig object - this contains
        ' the logic to compound all the flags into a clear 'enabled' or 'disabled' state
        ' if all the flags match, or an 'indeterminate' state if they do not.
        Using reader = con.ExecuteReturnDataReader(cmd)
            Dim prov As New ReaderDataProvider(reader)
            Dim cfg As New CombinedConfig()

            Dim first As Boolean = True
            While reader.Read()
                Dim flag = DirectCast(prov.GetValue("diagnostics", 0), clsAPC.Diags)
                If first Then
                    first = False
                    cfg.SetLoggingStates(flag, prov.GetValue("logtoeventlog", True))
                Else
                    cfg.CompoundLoggingStates(flag, prov.GetValue("logtoeventlog", True))
                End If
            End While
            Return cfg
        End Using

    End Function

    ''' <summary>
    ''' Set the diagnostics flags for the given Resource PC.
    ''' </summary>
    ''' <param name="resourceId">The ID of the Resource PC.</param>
    ''' <param name="diags">The new diagnostics flags</param>
    <SecuredMethod(Permission.Resources.ConfigureResource)>
    Public Sub SetResourceDiagnostics(resourceId As Guid, diags As Integer) Implements IServer.SetResourceDiagnostics

        ' Check role-based permissions
        CheckPermissions()

        Using con = GetConnection()
            ' Check group-based permissions
            If Not HasPermissionOnResource(con, resourceId, Permission.Resources.ConfigureResource) Then
                Throw New PermissionException(
                    My.Resources.clsServer_TheCurrentUserDoesNotHavePermissionToConfigureThisResource)
            End If

            Dim cmd As New SqlCommand("
                update BPAResource
                set diagnostics = @diagnostics
                where ResourceID = @resid")

            With cmd.Parameters
                .AddWithValue("@diagnostics", diags)
                .AddWithValue("@resid", resourceId)
            End With
            con.Execute(cmd)
        End Using

    End Sub

    ''' <summary>
    ''' Set the diagnostics flags for the given Resource PC. We provide checked and
    ''' unchecked parameters so that some flags can be left as is.
    ''' </summary>
    ''' <param name="resourceId">The ID of the Resource PC.</param>
    <SecuredMethod(Permission.Resources.ConfigureResource)>
    Public Sub SetResourceDiagnosticsCombined(resourceId As Guid, logLevel As Integer, logMemory As Integer,
        logForceGC As Integer, logWebServices As Integer) Implements IServer.SetResourceDiagnosticsCombined

        ' Check role-based permissions
        CheckPermissions()

        Using con = GetConnection()
            ' Check group-based permissions
            If Not HasPermissionOnResource(con, resourceId, Permission.Resources.ConfigureResource) Then
                Throw New PermissionException(
                    My.Resources.clsServer_TheCurrentUserDoesNotHavePermissionToConfigureThisResource)
            End If

            Dim cmd As New SqlCommand("
                update BPAResource
                set diagnostics = (diagnostics | @mask1) & ~@mask2
                where ResourceID = @resid")

            Dim mask1 As Integer = 0
            Dim mask2 As Integer = 0

            Select Case logLevel
                Case 0
                    mask2 = clsAPC.Diags.LogOverrideKey _
                                Or clsAPC.Diags.LogOverrideAll _
                                Or clsAPC.Diags.LogOverrideErrorsOnly
                Case 1
                    mask1 = clsAPC.Diags.LogOverrideKey
                    mask2 = clsAPC.Diags.LogOverrideAll _
                                Or clsAPC.Diags.LogOverrideErrorsOnly
                Case 2
                    mask1 = clsAPC.Diags.LogOverrideAll
                    mask2 = clsAPC.Diags.LogOverrideKey _
                                Or clsAPC.Diags.LogOverrideErrorsOnly
                Case 3
                    mask1 = clsAPC.Diags.LogOverrideErrorsOnly
                    mask2 = clsAPC.Diags.LogOverrideAll _
                                Or clsAPC.Diags.LogOverrideKey
            End Select

            Select Case logMemory
                Case 0
                    mask2 = clsAPC.Diags.LogMemory Or clsAPC.Diags.ForceGC
                Case 1
                    mask1 = clsAPC.Diags.LogMemory
            End Select

            Select Case logForceGC
                Case 0
                    mask2 = clsAPC.Diags.ForceGC
                Case 1
                    mask1 = clsAPC.Diags.ForceGC
            End Select

            Select Case logWebServices
                Case 0
                    mask2 = clsAPC.Diags.LogWebServices
                Case 1
                    mask1 = clsAPC.Diags.LogWebServices
            End Select

            With cmd.Parameters
                .AddWithValue("@mask1", mask1)
                .AddWithValue("@mask2", mask2)
                .AddWithValue("@resid", resourceId)
            End With

            con.Execute(cmd)
        End Using

    End Sub

    ''' <summary>
    ''' Reads a resource name from the database into a string.
    ''' </summary>
    ''' <param name="gResourceId">The resource id</param>
    ''' <returns>The resource name</returns>
    <SecuredMethod(True)>
    Public Function GetResourceName(ByVal gResourceId As Guid) As String Implements IServer.GetResourceName
        CheckPermissions()

        Using con = GetConnection()
            Return GetResourceName(con, gResourceId)
        End Using
    End Function


    <SecuredMethod(True)>
    Public Function GetResourceNameFromSessionId(sessionId As Guid) As String Implements IServer.GetResourceNameFromSessionId
        CheckPermissions()

        Dim session As clsProcessSession

        Using con = GetConnection()
            Dim sessions = GetActualSessions(con, sessionId, Nothing)

            If Not sessions.Any() Then
                Throw New clsFunctionException(My.Resources.GetResourceNameFunction_MissingSessionInGetResourceName)
            End If

            session = sessions.First()

            Try
                Return GetResourceName(con, session.ResourceID)
            Catch ex As Exception
                Throw New clsFunctionException(String.Format(My.Resources.GetResourceNameFunction_FailedToGetResourceName0, ex.Message))
            End Try
        End Using
    End Function

    ''' <summary>
    ''' Returns the name for a given resource ID.
    ''' </summary>
    ''' <param name="con">The database connection</param>
    ''' <param name="gResourceID">The resource ID</param>
    ''' <returns>Resource name</returns>
    Private Function GetResourceName(ByVal con As IDatabaseConnection, ByVal gResourceID As Guid) As String
        Dim cmd As New SqlCommand("select name from BPAResource where resourceid=@id")
        cmd.Parameters.AddWithValue("@id", gResourceID)
        Dim obj As Object = con.ExecuteReturnScalar(cmd)
        If obj Is Nothing Then Return "" Else Return DirectCast(obj, String)
    End Function

    Private Function HasPermissionOnResource(con As IDatabaseConnection, resourceId As Guid, perm As String) As Boolean
        Return GetEffectiveMemberPermissions(
            con, New Groups.ResourceGroupMember(resourceId)).
            HasPermission(mLoggedInUser, perm)
    End Function

    Private Sub EnsureResourceCapNotExceeded(resName As String)
        Dim connectionManager = GetASCRConnectionManager()
        If connectionManager Is Nothing Then Return
        Dim ascrManager = TryCast(connectionManager, OnDemandConnectionManager)
        ascrManager?.EnsureResourceCapNotExceeded(resName)
    End Sub

    <SecuredMethod(True)>
    Public Function GetResourceReport() As List(Of ResourceSummaryData) Implements IServer.GetResourceReport
        CheckPermissions()

        Using con = GetConnection()
            Return GetResourceSummaryData(con)
        End Using
    End Function

    Protected Overridable Function GetResourceSummaryData(con As IDatabaseConnection) As List(Of ResourceSummaryData)
        Dim resourceLoggingData As New List(Of ResourceSummaryData)
        Using command = mDatabaseCommandFactory("")
            command.CommandText = "
            with #cte_resource_schedules as (
            select distinct ts2.resourcename
                , s2.[name] + ':' + stuff((
                    select ', ' + t1.[name]
                    from BPATaskSession ts1
		            join BPATask t1 on t1.id = ts1.taskid
		            join BPASchedule s1 on s2.id = t1.scheduleid
                    where ts1.resourcename = ts2.resourcename
		            and s1.id = s2.id
                    for xml path('')
                ), 1, 1, '') as Schedules
            from BPATaskSession ts2
            JOIN BPATask t2 on t2.id = ts2.taskid
            JOIN BPASchedule s2 ON s2.id = t2.scheduleid
            ),
            #cte_all_schedules as (
            select distinct c1.resourcename
                , stuff((
                    select '; ' + c2.Schedules
                    from #cte_resource_schedules c2
                    where c1.resourcename = c2.resourcename
                    for xml path('')
                ), 1, 1, '') as Schedules
            from #cte_resource_schedules c1
            )
            select b.[name],
	               b.FQDN,
	               b.resourceid,
	               b.AttributeID,
	               p.[name] as 'Pool',
	               p.controller,
	               b.userID,
	               b.diagnostics,
	               c.Schedules
            from BPAResource b
            left join #cte_all_schedules c on c.resourcename = b.[name]
            left join BPVPools p on p.id = b.[pool]
            "

            Using reader = con.ExecuteReturnDataReader(command)
                Dim prov As New ReaderDataProvider(reader)
                Do While reader.Read()
                    resourceLoggingData.Add(New ResourceSummaryData(prov.GetString("name"),
                                                                    prov.GetString("FQDN"),
                                                                    prov.GetString("resourceid"),
                                                                    CType(prov.GetInt("AttributeID"), ResourceAttribute),
                                                                    prov.GetString("Pool"),
                                                                    prov.GetString("controller"),
                                                                    prov.GetString("userID"),
                                                                    CType(prov.GetInt("diagnostics"), clsAPC.Diags),
                                                                    prov.GetString("Schedules")))
                Loop

            End Using

            Return resourceLoggingData
        End Using
    End Function

    <SecuredMethod(True)>
    Public Function GetResourceCount() As Integer Implements IServer.GetResourceCount
        CheckPermissions()

        Using con = GetConnection()
            Using cmd As New SqlCommand("select resourceid from BPAResource where (AttributeID & @defaultAttrib) = 0 and (AttributeID & @retiredAttrib) = 0")
                With cmd.Parameters
                    .AddWithValue("@defaultAttrib", ResourceAttribute.DefaultInstance)
                    .AddWithValue("@retiredAttrib", ResourceAttribute.Retired)
                End With
                Dim idList = New List(Of Guid)
                Using reader = con.ExecuteReturnDataReader(cmd)
                    Dim prov As New ReaderDataProvider(reader)
                    While reader.Read()
                        idList.Add(prov.GetGuid("resourceid"))
                    End While
                End Using
                Return idList.Where(Function(id As Guid)
                                        Dim member = New Groups.ResourceGroupMember(id)
                                        member.Permissions = GetEffectiveMemberPermissions(con, member)
                                        Return member.HasViewPermission(mLoggedInUser)
                                    End Function).Count
            End Using
        End Using
    End Function
End Class
