/*
SCRIPT         : 336
AUTHOR         : Euan Jones
PURPOSE        : Fix sort orders and issue bg-4176 where adding a queue name broke the sql
*/

ALTER PROCEDURE [BPDS_QueueVolumesNow] @BPQueueName       NVARCHAR(MAX) = NULL, 
                                       @ExcludePending    NVARCHAR(MAX) = 'False', 
                                       @ExcludeDeferred   NVARCHAR(MAX) = 'False', 
                                       @ExcludeComplete   NVARCHAR(MAX) = 'False', 
                                       @ExcludeExceptions NVARCHAR(MAX) = 'False'
AS
     IF @ExcludePending NOT IN('True', 'False')
         RAISERROR('@ExcludePending must be either True or False', 11, 1);
         ELSE
         IF @ExcludeDeferred NOT IN('True', 'False')
             RAISERROR('@ExcludeDeferred must be either True or False', 11, 1);
             ELSE
             IF @ExcludeComplete NOT IN('True', 'False')
                 RAISERROR('@ExcludeComplete must be either True or False', 11, 1);
                 ELSE
                 IF @ExcludeExceptions NOT IN('True', 'False')
                     RAISERROR('@ExcludeExceptions must be either True or False', 11, 1);
                     ELSE
                     IF @BPQueueName IS NOT NULL
                        AND NOT EXISTS
                     (
                         SELECT 1
                         FROM BPAWorkQueue
                         WHERE name = @BPQueueName
                     )
                         BEGIN
                             DECLARE @rtnMessage VARCHAR(500)= CONCAT('@BPQueueName, provided queue (', @BPQueueName, ') name does not exist');
                             RAISERROR(@rtnMessage, 11, 1);
                     END;
                         ELSE
                         BEGIN
                             DECLARE @ColumnNames NVARCHAR(MAX);
                             SELECT @ColumnNames = ISNULL(@ColumnNames + ',', '') + QUOTENAME(ItemStatus)
                             FROM
                             (
                                 SELECT 'Pending' AS ItemStatus
                                 WHERE @ExcludePending = 'False'
                                 UNION
                                 SELECT 'Deferred' AS ItemStatus
                                 WHERE @ExcludeDeferred = 'False'
                                 UNION
                                 SELECT 'Complete' AS ItemStatus
                                 WHERE @ExcludeComplete = 'False'
                                 UNION
                                 SELECT 'Exceptions' AS ItemStatus
                                 WHERE @ExcludeExceptions = 'False'
                             ) AS StatusNarrs;
                             DECLARE @WhereClause NVARCHAR(MAX);
                             SET @WhereClause = @BPQueueName;
                             DECLARE @SQLQuery NVARCHAR(MAX);
                             DECLARE @Params NVARCHAR(500);
                             IF @BPQueueName IS NOT NULL
                                 BEGIN
                                     SET @SQLQuery = 'with results as (
        select
            q.name,
            case
                when i.state = 1 then ''Pending''
                when i.state = 3 then ''Deferred''
                when i.state = 4 then ''Complete''
                when i.state = 5 then ''Exceptions''
            end as state,
            COUNT(*) as Number
        from BPAWorkQueue q
            inner join BPVWorkQueueItem i on i.queueident=q.ident
        where i.state in (1,3,4,5) AND q.name = @WhereParam 
        group by q.name, i.state)
        select name, ''Volume'' as [ValueLabel],' + @ColumnNames + ' from results pivot (SUM(Number) for state in (' + @ColumnNames + ')) as number';
                                     SET @params = N'@WhereParam nvarchar(max)';
                                     EXECUTE sp_executesql 
                                             @SQLQuery, 
                                             @Params, 
                                             @WhereParam = @WhereClause;
                             END;
                                 ELSE
                                 BEGIN
                                     SET @SQLQuery = 'with results as (
        select
            q.name,
            case
                when i.state = 1 then ''Pending''
                when i.state = 3 then ''Deferred''
                when i.state = 4 then ''Complete''
                when i.state = 5 then ''Exceptions''
            end as state,
            COUNT(*) as Number
        from BPAWorkQueue q
            inner join BPVWorkQueueItem i on i.queueident=q.ident
        where i.state in (1,3,4,5) 
        group by q.name, i.state)
        , p as (select name, ''Volume'' as [ValueLabel],' + @ColumnNames + ' from results pivot (SUM(Number) for state in (' + @ColumnNames + ')) as number)
		select * from p order by name';
                                     EXECUTE sp_executesql 
                                             @SQLQuery;
                             END;
                     END;
                     return
GO

ALTER PROCEDURE [BPDS_Exceptions] @BPQueueName  NVARCHAR(MAX) = NULL, 
                                  @NumberOfDays INT           = 3
AS
     IF @BPQueueName IS NOT NULL
        AND NOT EXISTS
     (
         SELECT 1
         FROM BPAWorkQueue
         WHERE name = @BPQueueName
     )
         BEGIN
             DECLARE @rtnMessage VARCHAR(500)= CONCAT('@BPQueueName, provided queue (', @BPQueueName, ') name does not exist');
             RAISERROR(@rtnMessage, 11, 1);
     END;
     IF @NumberOfDays < 1
        OR @NumberOfDays > 31
         RAISERROR('@NumberOfDays must be between 1 and 31', 11, 1);
         ELSE
         BEGIN
             DECLARE @ColumnName NVARCHAR(MAX);
             DECLARE @Query NVARCHAR(MAX);
             DECLARE @WhereClause NVARCHAR(MAX);
             DECLARE @Params NVARCHAR(500);
             SET @WhereClause = @BPQueueName;
             SELECT @ColumnName = ISNULL(@ColumnName + ',', '') + QUOTENAME(DATENAME(day, TheDate) + '-' + DATENAME(month, TheDate))
             FROM ufn_GetReportDays(@NumberOfDays)
             ORDER BY TheDate;
             IF @BPQueueName IS NOT NULL
                 BEGIN
                     SET @Query = 'select
            name, ''Exceptions'' as [ValueLabel], ' + @ColumnName + '
        from (select
                ISNULL(q.name, ''<unknown>'') as name,
                DATENAME(day, d.reportdate) + ''-'' + DATENAME(month, d.reportdate) as pivotdate,
                d.exceptioned
            from BPMIProductivityDaily d
                left join BPAWorkQueue q on d.queueident = q.ident
            where d.reportdate >= (select MIN(TheDate) from ufn_GetReportDays(@DaysParam))
                and q.name = @WhereParam) as src
        pivot (sum(exceptioned) for pivotdate in (' + @ColumnName + ')) as pvt';
                     SET @params = N'@WhereParam nvarchar(max), @DaysParam int';
                     EXECUTE sp_executesql 
                             @Query, 
                             @Params, 
                             @WhereParam = @WhereClause, 
                             @DaysParam = @numberOfDays;
             END;
                 ELSE
                 BEGIN
                     SET @Query = 'select
            name, ''Exceptions'' as [ValueLabel], ' + @ColumnName + '
        from (select
                ISNULL(q.name, ''<unknown>'') as name,
                DATENAME(day, d.reportdate) + ''-'' + DATENAME(month, d.reportdate) as pivotdate,
                d.exceptioned
            from BPMIProductivityDaily d
                left join BPAWorkQueue q on d.queueident = q.ident
            where d.reportdate >= (select MIN(TheDate) from ufn_GetReportDays(@DaysParam))
        ) as src
        pivot (sum(exceptioned) for pivotdate in (' + @ColumnName + ')) as pvt
		order by name';
                     SET @params = N'@DaysParam int';
                     EXECUTE sp_executesql 
                             @Query, 
                             @Params, 
                             @DaysParam = @numberOfDays;
             END;
     END;
     return;
GO

ALTER procedure [BPDS_WorkforceAvailability]
as

select DisplayStatus as [Status], COUNT(*) as [Total] from BPAResource
where DisplayStatus is not null
group by DisplayStatus
order by DisplayStatus;

return;
GO

ALTER procedure [BPDS_TotalAutomations]
as

select
    case when ProcessType = 'O' then 'Objects' else 'Processes' end as [Type],
    COUNT(*) as Total
from BPAProcess
where ProcessType in ('O', 'P') and (AttributeID & 1) = 0
group by ProcessType
order by 1;

return;
GO


ALTER procedure [BPDS_DailyUtilisation]
    @BPResourceName nvarchar(max) = null,
    @NumberOfDays int = 7,
    @DisplayUnits nvarchar(max) = 'minute',
    @MaxResourceHours int = 24
as

if @NumberOfDays < 1 or @NumberOfDays > 31
    raiserror('@NumberOfDays must be between 1 and 31', 11, 1);
else if @DisplayUnits not in ('second', 'minute', 'hour', 'percentage')
    raiserror('@DisplayUnits must be second, minute, hour or percentage', 11, 1);
else if @MaxResourceHours < 1 or @MaxResourceHours > 24
    raiserror('@MaxResourceHours must be between 1 and 24', 11, 1);
else
    select
        DATENAME(day, u.TheDate) + '-' + DATENAME(month, u.TheDate) as "Day",
		@DisplayUnits as [ValueLabel],
        case when @DisplayUnits = 'second' then CAST(Total as decimal(12,2))
             when @DisplayUnits = 'hour' then CAST(Total/3600 as decimal(12,2))
             when @DisplayUnits = 'percentage' and Resources > 0 then CAST(Total/(36*Resources*@MaxResourceHours) as decimal(12,2))
             else CAST(Total/60 as decimal(12,2)) end as "Total"
    from (
        select
            dates.TheDate,
            CAST(ISNULL(SUM(d.hr0 + d.hr1 + d.hr2 + d.hr3 + d.hr4 + d.hr5 + d.hr6 + d.hr7 + 
                    d.hr8 + d.hr9 + d.hr10 + d.hr11 + d.hr12 + d.hr13 + d.hr14 + d.hr15 + 
                    d.hr16 + d.hr17 + d.hr18 + d.hr19 + d.hr20 + d.hr21 + d.hr22 + d.hr23), 0) as float)
                    as "Total",
            COUNT(distinct(d.resourceid)) as "Resources"
        from ufn_GetReportDays(@NumberOfDays) dates
            left join BPMIUtilisationDaily d on d.reportdate = dates.TheDate
            left join BPAResource r on d.resourceid = r.resourceid
        where @BPResourceName is null or @BPResourceName = r.name
        group by dates.TheDate
        ) as u
		order by u.TheDate;
return;
GO

ALTER procedure [BPDS_ProcessUtilisationByHour]
    @BPProcessName nvarchar(max) = null,
    @DisplayUnits nvarchar(max) = 'minute'
as

if @DisplayUnits not in ('second', 'minute', 'hour', 'percentage')
    raiserror('@DisplayUnits must be second, minute, hour or percentage', 11, 1);
else
begin
declare @Units int;
select @Units = case when @DisplayUnits = 'second' then 1 when @DisplayUnits = 'hour' then 3600 else 60 end;

select
    ProcessName,
	@DisplayUnits as [ValueLabel],
    case when @DisplayUnits = 'percentage' and Resources > 0 then CAST(Interval1/(Resources*7200/100) as decimal(12,2))
         else CAST(Interval1/@Units as decimal(12,2)) end as "00:00",
    case when @DisplayUnits = 'percentage' and Resources > 0 then CAST(Interval2/(Resources*7200/100) as decimal(12,2))
         else CAST(Interval2/@Units as decimal(12,2)) end as "02:00",
    case when @DisplayUnits = 'percentage' and Resources > 0 then CAST(Interval3/(Resources*7200/100) as decimal(12,2))
        else CAST(Interval3/@Units as decimal(12,2)) end as "04:00",
    case when @DisplayUnits = 'percentage' and Resources > 0 then CAST(Interval4/(Resources*7200/100) as decimal(12,2))
        else CAST(Interval4/@Units as decimal(12,2)) end as "06:00",
    case when @DisplayUnits = 'percentage' and Resources > 0 then CAST(Interval5/(Resources*7200/100) as decimal(12,2))
        else CAST(Interval5/@Units as decimal(12,2)) end as "08:00",
    case when @DisplayUnits = 'percentage' and Resources > 0 then CAST(Interval6/(Resources*7200/100) as decimal(12,2))
        else CAST(Interval6/@Units as decimal(12,2)) end as "10:00",
    case when @DisplayUnits = 'percentage' and Resources > 0 then CAST(Interval7/(Resources*7200/100) as decimal(12,2))
        else CAST(Interval7/@Units as decimal(12,2)) end as "12:00",
    case when @DisplayUnits = 'percentage' and Resources > 0 then CAST(Interval8/(Resources*7200/100) as decimal(12,2))
        else CAST(Interval8/@Units as decimal(12,2)) end as "14:00",
    case when @DisplayUnits = 'percentage' and Resources > 0 then CAST(Interval9/(Resources*7200/100) as decimal(12,2))
        else CAST(Interval9/@Units as decimal(12,2)) end as "16:00",
    case when @DisplayUnits = 'percentage' and Resources > 0 then CAST(Interval10/(Resources*7200/100) as decimal(12,2))
        else CAST(Interval10/@Units as decimal(12,2)) end as "18:00",
    case when @DisplayUnits = 'percentage' and Resources > 0 then CAST(Interval11/(Resources*7200/100) as decimal(12,2))
        else CAST(Interval11/@Units as decimal(12,2)) end as "20:00",
    case when @DisplayUnits = 'percentage' and Resources > 0 then CAST(Interval12/(Resources*7200/100) as decimal(12,2))
        else CAST(Interval12/@Units as decimal(12,2)) end as "22:00"
from (
    select
        p.name as ProcessName,
        CAST(CAST(ISNULL(SUM(d.hr0 + d.hr1), 0) as decimal) as decimal(12,2)) as "Interval1",
        CAST(CAST(ISNULL(SUM(d.hr2 + d.hr3), 0) as decimal) as decimal(12,2)) as "Interval2",
        CAST(CAST(ISNULL(SUM(d.hr4 + d.hr5), 0) as decimal) as decimal(12,2)) as "Interval3",
        CAST(CAST(ISNULL(SUM(d.hr6 + d.hr7), 0) as decimal) as decimal(12,2)) as "Interval4",
        CAST(CAST(ISNULL(SUM(d.hr8 + d.hr9), 0) as decimal) as decimal(12,2)) as "Interval5",
        CAST(CAST(ISNULL(SUM(d.hr10 + d.hr11), 0) as decimal) as decimal(12,2)) as "Interval6",
        CAST(CAST(ISNULL(SUM(d.hr12 + d.hr13), 0) as decimal) as decimal(12,2)) as "Interval7",
        CAST(CAST(ISNULL(SUM(d.hr14 + d.hr15), 0) as decimal) as decimal(12,2)) as "Interval8",
        CAST(CAST(ISNULL(SUM(d.hr16 + d.hr17), 0) as decimal) as decimal(12,2)) as "Interval9",
        CAST(CAST(ISNULL(SUM(d.hr18 + d.hr19), 0) as decimal) as decimal(12,2)) as "Interval10",
        CAST(CAST(ISNULL(SUM(d.hr20 + d.hr21), 0) as decimal) as decimal(12,2)) as "Interval11",
        CAST(CAST(ISNULL(SUM(d.hr22 + d.hr23), 0) as decimal) as decimal(12,2)) as "Interval12",
        COUNT(distinct(d.resourceid)) as "Resources"
    from ufn_GetReportDays(1) dates
        left join BPMIUtilisationDaily d on d.reportdate = dates.TheDate
        left join BPAProcess p on d.processid = p.processid
    where @BPProcessName is null or @BPProcessName = p.name
    group by p.name
    ) as u order by ProcessName;
end

return;
GO


ALTER procedure [BPDS_AverageHandlingTime]
    @BPQueueName nvarchar(max) = null,
    @NumberOfDays int = 7
as

if @NumberOfDays < 1 or @NumberOfDays > 90
    raiserror('@NumberOfDays must be between 1 and 90', 11, 1);
else
    select
        ISNULL(q.name, '<unknown>'),
        CAST(ISNULL(AVG(d.avgworktime), 0) as decimal(12,2)) as "Average Time"
    from BPMIProductivityDaily d
        left join BPAWorkQueue q on d.queueident = q.ident
    where d.reportdate >= (select MIN(TheDate) from ufn_GetReportDays(@NumberOfDays))
        and (@BPQueueName is null or @BPQueueName = q.name)
    group by q.name
	order by q.name;

return;
GO



ALTER procedure [BPDS_DailyProductivity]
    @BPQueueName nvarchar(max) = null,
    @NumberOfDays int = 7
as

if @NumberOfDays < 1 or @NumberOfDays > 31
    raiserror('@NumberOfDays must be between 1 and 31', 11, 1);
else
    select
        DATENAME(day, dys.TheDate) + '-' + DATENAME(month, dys.TheDate) as "Day",
		'Complete' as [ValueLabel], 
        ISNULL(SUM(d.created), 0) as New,
        ISNULL(SUM(d.deferred), 0) as Deferred,
        ISNULL(SUM(d.completed), 0) as Complete
    from ufn_GetReportDays(@NumberOfDays) dys
        left join BPMIProductivityDaily d on d.reportdate = dys.TheDate
        left join BPAWorkQueue q on d.queueident = q.ident
    where @BPQueueName is null or @BPQueueName = q.name
    group by dys.TheDate
	order by dys.TheDate;

return;
GO



ALTER procedure [BPDS_AverageRetries]
    @BPQueueName nvarchar(max) = null,
    @NumberOfDays int = 7
as

if @NumberOfDays < 1 or @NumberOfDays > 90
    raiserror('@NumberOfDays must be between 1 and 90', 11, 1);
else
    select
        ISNULL(q.name, '<unknown>'),
        CAST(ISNULL(AVG(d.avgretries), 0) as decimal(12,2)) as "Retries"
    from BPMIProductivityDaily d
        left join BPAWorkQueue q on d.queueident = q.ident
    where d.reportdate >= (select MIN(TheDate) from ufn_GetReportDays(@NumberOfDays))
        and (@BPQueueName is null or @BPQueueName = q.name)
    group by q.name
	order by q.name;

return;
GO


ALTER procedure [BPDS_FTEProductivityComparison]
    @BPQueueName nvarchar(max) = null,
    @NumberOfMonths int = 6,
    @FTEProductivity decimal(12,2) = 0,
    @FTECost decimal(12,2) = 0,
    @DisplayAs nvarchar(max) = 'percentage'
as

if @NumberOfMonths < 1 or @NumberOfMonths > 12
    raiserror('@NumberOfMonths must be between 1 and 12', 11, 1);
else if @DisplayAs not in ('percentage', 'number', 'cost')
    raiserror('@DisplayAs must be percentage, number or cost', 11, 1);
else
    select
        TheDate,
        case when @FTEProductivity <> 0 then
                case when @DisplayAs = 'cost' then CAST((completed/(@FTEProductivity*DaysInMonth))*@FTECost as decimal(12,2))
                when @DisplayAs = 'number' then CAST(completed/(@FTEProductivity*DaysInMonth) as decimal(12,2))
                else CAST((completed/(@FTEProductivity*DaysInMonth))*100 as decimal(12,2)) end
        else completed  end as completed
    from (
        select
			mths.TheYear as xYear,
			mths.TheMonth as xMonth,
            DATENAME(month, DATEADD(month, mths.TheMonth, -1)) + ' ' + CAST(mths.TheYear as nvarchar(4)) as TheDate,
            case when mths.TheYear = DATEPART(year, getdate()) and mths.TheMonth = DATEPART(month, getutcdate())
            then DAY(getdate())
            else DAY(DATEADD(day, -1, DATEADD(month, 1, CAST(CAST(mths.TheYear as nvarchar) + '-' + CAST(mths.TheMonth as nvarchar) + '-1' as datetime))))
        end as DaysInMonth,
            ISNULL(SUM(completed), 0) as Completed
        from ufn_GetReportMonths(@NumberOfMonths) mths
            left join BPMIProductivityMonthly m on m.reportyear = mths.TheYear and m.reportmonth = mths.TheMonth
            left join BPAWorkQueue q on m.queueident = q.ident
        where @BPQueueName is null or @BPQueueName = q.name
        group by mths.TheMonth, mths.TheYear
    ) as p
	order by p.xYear, p.XMonth

return;
GO








-- Set DB version.
insert into BPADBVersion
(dbversion, 
 scriptrundate, 
 scriptname, 
 [description], 
 timezoneoffset
)
values
('336', 
 GETUTCDATE(), 
 'db_upgradeR336.sql', 
 'Fix sort orders and issue bg-4176 where adding a queue name broke the sql', 
 0
);