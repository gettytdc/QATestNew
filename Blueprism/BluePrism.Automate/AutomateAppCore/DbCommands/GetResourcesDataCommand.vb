Imports System.Data.SqlClient
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.Resources
Imports BluePrism.AutomateAppCore.Resources.ResourceMachine
Imports BluePrism.AutomateAppCore.Utility
Imports BluePrism.Core.Resources
Imports BluePrism.Data
Imports BluePrism.Server.Domain.Models
Imports BluePrism.Server.Domain.Models.DataFilters
Imports BluePrism.Utilities.Functional

Public Class GetResourcesDataCommand
    Implements IDataAccessCommand
    Private ReadOnly mCommand As IDbCommand
    Private ReadOnly mLoggedInUser As IUser
    Private ReadOnly mResourceParameters As ResourceParameters

    Public Sub New(
        loggedInUser As IUser,
        stageWarningThreshold As Integer,
        resourceParameters As ResourceParameters)
        mLoggedInUser = loggedInUser
        mResourceParameters = resourceParameters
        mCommand = New SqlCommand()
        With mCommand
            .CommandText = CreateCommandText()
            .AddParameter("@itemsperpage", mResourceParameters.ItemsPerPage)
            .AddParameter("@stagewarningthreshold", stageWarningThreshold)
            .AddParameter("@loggedInUserId", mLoggedInUser.Id)
        End With
    End Sub

    Public Function Execute(databaseConnection As IDatabaseConnection) As Object Implements IDataAccessCommand.Execute
        Dim resourcesTable = databaseConnection.ExecuteReturnDataTable(mCommand)
        Dim resources = resourcesTable.Rows.Cast(Of DataRow)().
            Select(Function(x) New ResourceInfo With {
                .ID = CType(x("resourceid"), Guid),
                .Name = If(x("name") Is DBNull.Value, "", CStr(x("name"))),
                .Pool = If(x("pool") Is DBNull.Value, Guid.Empty, CType(x("pool"), Guid)),
                .PoolName = If(x("poolname") Is DBNull.Value, String.Empty, CStr(x("poolName"))),
                .GroupID = If(x("groupId") Is DBNull.Value, Guid.Empty, CType(x("groupId"), Guid)),
                .GroupName = If(x("groupName") Is DBNull.Value, String.Empty, CStr(x("groupName"))),
                .Attributes = If(x("attributeid") Is DBNull.Value, ResourceAttribute.None, CType(x("attributeid"), ResourceAttribute)),
                .ActiveSessions = If(x("actionsrunning") Is DBNull.Value, 0, CInt(x("actionsrunning"))),
                .WarningSessions = If(x("warningsessions") Is DBNull.Value, 0, CInt(x("warningsessions"))),
                .PendingSessions = If(x("pendingsessions") Is DBNull.Value, 0, CInt(x("pendingsessions"))),
                .Status = CType(x("statusid"), ResourceDBStatus),
                .LastUpdated = If(x("lastupdated") Is DBNull.Value, Date.MinValue, CDate(x("lastupdated"))),
                .UserID = If(x("userid") Is DBNull.Value, Guid.Empty, CType(x("userid"), Guid)),
                .DisplayStatus = CType(x("displayStatus"), ResourceStatus)
            }).
            ToArray()

        Return resources
    End Function

    Private Function CreateCommandText() As String
        Dim displayStatusOrderClause = String.Empty
        Dim displayStatusOrderSelect = String.Empty
        Dim isSortedByDisplayStatus =
            mResourceParameters.SortBy = ResourceSortBy.DisplayStatusAscending Or
            mResourceParameters.SortBy = ResourceSortBy.DisplayStatusDescending

        If isSortedByDisplayStatus Then
            displayStatusOrderSelect = $", displayStatusOrder"
            displayStatusOrderClause = $",
            case
			    when r.statusid = 2 then 3
			    when datediff(second, r.lastupdated, getutcdate()) >= 60 then 6
			    when (r.AttributeID & 16) <> 0 then 5
			    when (r.AttributeID & 32) <> 0 and @loggedInUserId <> isnull(r.userID, 0x0) then 7
			    when r.actionsrunning > 0 then
				    case when isnull(s.warningSessions, 0) > 0 then 4 else 1 end
			    else 2
		    end as displayStatusOrder"
        End If

        Dim commandBuilder = New StringBuilder()
        commandBuilder.Append($"
            with sessions (resourceid, warningSessions)
            as
            (
                select
                    runningresourceid,
                    count(*)
                from
		            BPASession
                where
		            @stagewarningthreshold <> 0
                    and statusid = 1
                    and warningthreshold > 0
                    and dateadd(second, lastupdatedtimezoneoffset, getutcdate()) > dateadd(second, warningthreshold, lastupdated)
	            group by runningresourceid
            ),
            statuses (resourceid, warningSessions, displayStatus{displayStatusOrderSelect})
            as
            (
	            select
		            r.resourceid,
		            s.warningSessions,
		            case
			            when r.statusid = 2 then 3
			            when datediff(second, r.lastupdated, getutcdate()) >= 60 then 4
			            when (r.AttributeID & 16) <> 0 then 5
			            when (r.AttributeID & 32) <> 0 and @loggedInUserId <> isnull(r.userID, 0x0) then 7
			            when r.actionsrunning > 0 then
				            case when isnull(s.warningSessions, 0) > 0 then 2 else 0 end
			            else 1
		            end as displayStatus
                    {displayStatusOrderClause}
                    from
		            BPAResource r
	            left join sessions s on s.resourceid = r.resourceid
            )
            select top (@itemsperpage)
                r.resourceid,
                r.name,
                r.pool,
                poolRes.name as poolName,
                g.id as groupId,
                g.name as groupName,
                r.attributeid,
                r.actionsrunning,
                isnull(s.warningSessions, 0) as warningSessions,
                (r.processesrunning - r.actionsrunning) as pendingsessions,
                r.statusid,
                r.lastUpdated,
                r.userid,
                s.displayStatus
                {displayStatusOrderSelect}
            from
	            BPAResource r
                {MteResourceSqlGenerator.MteToken}
	            left join statuses s on s.resourceid = r.resourceid
	            left join BPAResource poolRes on poolRes.resourceid = r.pool
	            left join BPAGroupResource gr on gr.memberid = isnull(poolRes.resourceid, r.resourceid)
	            left join BPAGroup g on g.id = gr.groupid
            where
                (r.attributeid & 12) = 0")

        Dim whereClauses = mResourceParameters.GetSqlWhereClauses(mCommand)

        If whereClauses.Any() Then
            commandBuilder.AppendLine($"{String.Join("", whereClauses.Select(Function(x) $" and ({x.SqlText})").ToArray())}")
        End If

        commandBuilder.AppendLine($"order by {mResourceParameters.GetSqlOrderByClause()}")

        whereClauses.SelectMany(Function(x) x.Parameters).ForEach(AddressOf mCommand.Parameters.Add).Evaluate()

        Dim mteQuery = New MteResourceSqlGenerator(commandBuilder.ToString())
        Dim query = mteQuery.GetResourceQueryAndSetParameters(mLoggedInUser, DirectCast(mCommand, SqlCommand))

        Return query
    End Function

    Public Sub Dispose() Implements IDisposable.Dispose
        mCommand.Dispose()
    End Sub
End Class
