/*
SCRIPT         : 186
AUTHOR         : GM
PURPOSE        : Adds Blue Prism database roles and refreshes dashboard stored procedures
*/

-- Create role for executing system procedures
if DATABASE_PRINCIPAL_ID('bpa_ExecuteSP_System') is null
    exec(N'CREATE ROLE bpa_ExecuteSP_System');

GRANT EXECUTE ON OBJECT::usp_RefreshMI TO bpa_ExecuteSP_System;
GRANT EXECUTE ON OBJECT::usp_RefreshProductivityData TO bpa_ExecuteSP_System;
GRANT EXECUTE ON OBJECT::usp_RefreshUtilisationData TO bpa_ExecuteSP_System;

-- Create role for executing published tile data sources
if DATABASE_PRINCIPAL_ID('bpa_ExecuteSP_DataSource_bpSystem') is null
    exec(N'CREATE ROLE bpa_ExecuteSP_DataSource_bpSystem');

GRANT EXECUTE ON OBJECT::BPDS_AverageHandlingTime TO bpa_ExecuteSP_DataSource_bpSystem;
GRANT EXECUTE ON OBJECT::BPDS_AverageRetries TO bpa_ExecuteSP_DataSource_bpSystem;
GRANT EXECUTE ON OBJECT::BPDS_DailyProductivity TO bpa_ExecuteSP_DataSource_bpSystem;
GRANT EXECUTE ON OBJECT::BPDS_DailyUtilisation TO bpa_ExecuteSP_DataSource_bpSystem;
GRANT EXECUTE ON OBJECT::BPDS_Exceptions TO bpa_ExecuteSP_DataSource_bpSystem;
GRANT EXECUTE ON OBJECT::BPDS_FTEProductivityComparison TO bpa_ExecuteSP_DataSource_bpSystem;
GRANT EXECUTE ON OBJECT::BPDS_LargestTables TO bpa_ExecuteSP_DataSource_bpSystem;
GRANT EXECUTE ON OBJECT::BPDS_ProcessUtilisationByHour TO bpa_ExecuteSP_DataSource_bpSystem;
GRANT EXECUTE ON OBJECT::BPDS_ResourceUtilisationByHour TO bpa_ExecuteSP_DataSource_bpSystem;
GRANT EXECUTE ON OBJECT::BPDS_TotalAutomations TO bpa_ExecuteSP_DataSource_bpSystem;
GRANT EXECUTE ON OBJECT::BPDS_WorkforceAvailability TO bpa_ExecuteSP_DataSource_bpSystem;

-- Create role (placeholder) for executing custom tile data sources
if DATABASE_PRINCIPAL_ID('bpa_ExecuteSP_DataSource_custom') is null
    exec(N'CREATE ROLE bpa_ExecuteSP_DataSource_custom');
GO

-- Create helper functions for tile data sources
if OBJECT_ID('ufn_GetReportDays') is null
    exec(N'create function ufn_GetReportDays (@Number int) returns @Days table (TheDate datetime) as begin return; end');

if OBJECT_ID('ufn_GetReportMonths') is null
    exec(N'create function ufn_GetReportMonths (@Number int) returns @Months table (TheYear int, TheMonth int) as begin return; end');
GO

alter function ufn_GetReportDays (
    @Number int)
returns @Days table (
    TheDate datetime)
as
begin
    declare @StartDate datetime;
    set @StartDate = DATEADD(DAY, -1, CAST(FLOOR(CAST(GETDATE() as float)) as datetime));

    with CTE_DatesTable as (
        select @StartDate as TheDate
        union all
        select DATEADD(DAY, -1, TheDate) from CTE_DatesTable
        where DATEADD(DAY, -1, TheDate) > DATEADD(DAY, -@Number, @StartDate))
    insert into @Days(TheDate) select TheDate FROM CTE_DatesTable option (MAXRECURSION 0);
  return;
end
GO

alter function ufn_GetReportMonths (
    @Number int)
returns @Months table (
    TheYear int,
    TheMonth int)
as
begin
    declare @StartDate datetime;
    set @StartDate = DATEADD(DAY, -(DAY(GETDATE())-1), GETDATE());
    
    with CTE_DatesTable as (
        select @StartDate as TheDate
        union all
        select DATEADD(MONTH, -1, TheDate) from CTE_DatesTable
        where DATEADD(MONTH, -1, TheDate) > DATEADD(MONTH, -@Number, @StartDate))
    insert into @Months (TheYear, TheMonth) select YEAR(TheDate), MONTH(TheDate) FROM CTE_DatesTable option (MAXRECURSION 0);
  return;
end
GO

-- Recreate data source stored procedures
alter procedure BPDS_AverageHandlingTime
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
    group by q.name;

return;
GO

alter procedure BPDS_AverageRetries
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
    group by q.name;

return;
GO

alter procedure BPDS_DailyProductivity
    @BPQueueName nvarchar(max) = null,
    @NumberOfDays int = 7
as

if @NumberOfDays < 1 or @NumberOfDays > 31
    raiserror('@NumberOfDays must be between 1 and 31', 11, 1);
else
    select
        DATENAME(day, dys.TheDate) + '-' + DATENAME(month, dys.TheDate) as "Day",
        ISNULL(SUM(d.created), 0) as New,
        ISNULL(SUM(d.deferred), 0) as Deferred,
        ISNULL(SUM(d.completed), 0) as Complete
    from ufn_GetReportDays(@NumberOfDays) dys
        left join BPMIProductivityDaily d on d.reportdate = dys.TheDate
        left join BPAWorkQueue q on d.queueident = q.ident
    where @BPQueueName is null or @BPQueueName = q.name
    group by dys.TheDate;

return;
GO

alter procedure BPDS_DailyUtilisation
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
        ) as u;

return;
GO

alter procedure BPDS_Exceptions
    @BPQueueName nvarchar(max) = null,
    @NumberOfDays int = 3
as

if @NumberOfDays < 1 or @NumberOfDays > 31
    raiserror('@NumberOfDays must be between 1 and 31', 11, 1);
else
begin
    declare @ColumnName nvarchar(max);
    declare @Query nvarchar(max);
    declare @WhereClause nvarchar(max);

    set @WhereClause = ISNULL(' and q.name = ''' + @BPQueueName + '''', '');
    select @ColumnName = ISNULL(@ColumnName + ',', '') + QUOTENAME(DATENAME(day, TheDate) + '-' + DATENAME(month, TheDate))
        from ufn_GetReportDays(@NumberOfDays) order by TheDate;

    set @Query = 
    'select
        name, ' + @ColumnName + '
    from (select
            ISNULL(q.name, ''<unknown>'') as name,
            DATENAME(day, d.reportdate) + ''-'' + DATENAME(month, d.reportdate) as pivotdate,
            d.exceptioned
        from BPMIProductivityDaily d
            left join BPAWorkQueue q on d.queueident = q.ident
        where d.reportdate >= (select MIN(TheDate) from ufn_GetReportDays(' + CAST(@NumberOfDays as nvarchar) + '))
            ' + @WhereClause + ') as src
    pivot (sum(exceptioned) for pivotdate in (' + @ColumnName + ')) as pvt'
    exec(@Query)
end

return;
GO

alter procedure BPDS_FTEProductivityComparison
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
        else completed  end
    from (
        select
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

return;
GO

alter procedure BPDS_LargestTables
    @NumberOfTables int = 5
as

if @NumberOfTables < 1 or @NumberOfTables > 25
    raiserror('@NumberOfTables must be between 1 and 25', 11, 1);
else
    select top(@NumberOfTables)
        t.name as "Table Name",
        CAST(CAST((SUM(a.total_pages)*8) as decimal)/1024 as decimal(12,2)) as "Size (Mb)"
    from sys.tables t
        inner join sys.indexes i on t.object_id = i.object_id
        inner join sys.partitions p on i.object_id = p.object_id and i.index_id = p.index_id
        inner join sys.allocation_units a on p.partition_id = a.container_id
    where t.name like 'BP%' and
        i.object_id > 255 and
        i.index_id <= 1
    group by t.name
    order by SUM(a.total_pages) desc;

return;
GO

alter procedure BPDS_ProcessUtilisationByHour
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
    ) as u;
end

return;
GO

alter procedure BPDS_ResourceUtilisationByHour
    @BPResourceName nvarchar(max) = null,
    @DisplayUnits nvarchar(max) = 'minute'
as

if @DisplayUnits not in ('second', 'minute', 'hour', 'percentage')
    raiserror('@DisplayUnits must be second, minute, hour or percentage', 11, 1);
else
begin
declare @Units decimal;
select @Units = case when @DisplayUnits = 'second' then 1 when @DisplayUnits = 'hour' then 3600 else 60 end;

select 
    'Utilisation',
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
        COUNT(distinct(r.resourceid)) as "Resources"
    from ufn_GetReportDays(1) dates
        left join BPMIUtilisationDaily d on d.reportdate = dates.TheDate
        left join BPAResource r on d.resourceid = r.resourceid
    where @BPResourceName is null or @BPResourceName = r.name
    ) as u;
end

return;
GO

alter procedure BPDS_TotalAutomations
as

select
    case when ProcessType = 'O' then 'Objects' else 'Processes' end as [Type],
    COUNT(*) as Total
from BPAProcess
where ProcessType in ('O', 'P') and (AttributeID & 1) = 0
group by ProcessType;

return;
GO

alter procedure BPDS_WorkforceAvailability
as

select
    'Percentage' as Label,
    CAST((CAST(r.ready as decimal)/r.total)*100 as decimal(6,2)) as "% Available"
from (select
        SUM(case when [status] = 'Ready' then 1 else 0 end) as ready,
        COUNT(*) as total
    from BPAResource
    where (AttributeID & 13) = 0) as r;

return;
GO

-- Recreate MI Refresh procedures
alter procedure usp_RefreshUtilisationData
    @ReportDate datetime,
    @DaysToKeep int,
    @MonthsToKeep int
as

declare @ReportTo datetime;
declare @DailyUtilisation table (
    resourceid uniqueidentifier,
    processid uniqueidentifier,
    h0 int, h1 int, h2 int, h3 int,
    h4 int, h5 int, h6 int, h7 int,
    h8 int, h9 int, h10 int, h11 int,
    h12 int, h13 int, h14 int, h15 int,
    h16 int, h17 int, h18 int, h19 int,
    h20 int, h21 int, h22 int, h23 int);

set @ReportTo = DATEADD(DAY, 1, @ReportDate);

with
hours5 as (
    select 0 as h union all select 0 union all
    select 0 union all select 0 union all select 0),
hours25 as (
    select ROW_NUMBER() over(order by a.h) - 1 as h
    from hours5 a cross join hours5 b)
   
insert into @DailyUtilisation
select
    resourceid, processid,
    [0],[1],[2],[3],[4],[5],
    [6],[7],[8],[9],[10],[11],
    [12],[13],[14],[15],[16],[17],
    [18],[19],[20],[21],[22],[23]
from(
    select
        tp.resourceid,
        tp.processid,
        [Hour],
        case
            when tp.startdatetime < intervals.StartDate then
                case
                    when ISNULL(tp.enddatetime, @ReportTo) > intervals.EndDate
                        then DATEDIFF(SECOND, intervals.StartDate, intervals.EndDate)
                    when ISNULL(tp.enddatetime, @ReportTo) between intervals.StartDate and intervals.EndDate
                        then DATEDIFF(SECOND, intervals.StartDate, ISNULL(tp.enddatetime, @ReportTo))
                    else 0
                end
            when tp.startdatetime between intervals.StartDate and intervals.EndDate then
                case
                    when ISNULL(tp.enddatetime, @ReportTo) > intervals.EndDate
                        then DATEDIFF(SECOND, tp.startdatetime, intervals.EndDate)
                    else DATEDIFF(SECOND, tp.startdatetime, ISNULL(tp.enddatetime, @ReportTo))
                end
            else 0
        end as Duration
        from BPMIUtilisationShadow tp
            inner join hours25 hrs on hrs.h between 0 and 23
            cross apply (
                select hrs.h as [Hour], DATEADD(HOUR, hrs.h, @ReportDate) as StartDate,
                DATEADD(HOUR, hrs.h + 1, @ReportDate) as EndDate) as intervals
) as src
pivot (SUM(Duration) for [Hour] in ([0],[1],[2],[3],[4],[5],[6],[7],[8],[9],[10],[11],[12],[13],[14],[15],[16],[17],[18],[19],[20],[21],[22],[23])) as piv;

--Create daily records
insert into BPMIUtilisationDaily select @ReportDate, * from @DailyUtilisation;

--Update any existing monthly records
update BPMIUtilisationMonthly set
    hr0 = hr0 + d.h0, hr1 = hr1 + d.h1, hr2 = hr2 + d.h2, hr3 = hr3 + d.h3, hr4 = hr4 + d.h4, hr5 = hr5 + d.h5,
    hr6 = hr6 + d.h6, hr7 = hr7 + d.h7, hr8 = hr8 + d.h8, hr9 = hr9 + d.h9, hr10 = hr10 + d.h10, hr11 = hr11 + d.h11,
    hr12 = hr12 + d.h12, hr13 = hr13 + d.h13, hr14 = hr14 + d.h14, hr15 = hr15 + d.h15, hr16 = hr16 + d.h16, hr17 = hr17 + d.h17,
    hr18 = hr18 + d.h18, hr19 = hr19 + d.h19, hr20 = hr20 + d.h20, hr21 = hr21 + d.h21, hr22 = hr22 + d.h22, hr23 = hr23 + d.h23
from BPMIUtilisationMonthly m
    inner join @DailyUtilisation d on m.resourceid = d.resourceid and m.processid = d.processid
where m.reportyear = DATEPART(YEAR, @ReportDate) and m.reportmonth = DATEPART(MONTH, @ReportDate);

--Insert new monthly records where required
insert into BPMIUtilisationMonthly
select DATEPART(YEAR, @ReportDate), DATEPART(MONTH, @ReportDate), d.*
from @DailyUtilisation d
    left join BPMIUtilisationMonthly m on d.resourceid = m.resourceid and d.processid = m.processid
    and m.reportyear = DATEPART(YEAR, @ReportDate) and m.reportmonth = DATEPART(MONTH, @ReportDate)
where m.reportyear is null;

--Age out any old daily records
delete from BPMIUtilisationDaily where reportdate < DATEADD(DAY, -@DaysToKeep, @ReportDate);

--Age out any old monthly records
delete from BPMIUtilisationMonthly
where reportyear <= DATEPART(YEAR, (DATEADD(MONTH, -@MonthsToKeep, @ReportDate))) and 
      reportmonth < DATEPART(MONTH, (DATEADD(MONTH, -@MonthsToKeep, @ReportDate)));

--Purge down shadow table (any sessions that completed before the day just reported on)
delete from BPMIUtilisationShadow where enddatetime is not null and enddatetime < @ReportDate;

return;
GO

alter procedure usp_RefreshProductivityData
    @ReportDate datetime,
    @DaysToKeep int,
    @MonthsToKeep int
as

declare @ReportTo datetime;
declare @DailyProductivity table (
    queueident int,
    created int, deferred int, retried int,
    exceptioned int, completed int,
    minworktime int, avgworktime decimal(12,2), maxworktime int,
    minelapsedtime int, avgelapsedtime decimal(12,2), maxelapsedtime int,
    minretries int, avgretries decimal(12,2), maxretries int);

set @ReportTo = DATEADD(DAY, 1, @ReportDate);

insert into @DailyProductivity
select
    queueident,
    SUM(case when eventid = 1 then 1 else 0 end),
    SUM(case when eventid = 3 then 1 else 0 end),
    SUM(case when eventid = 4 then 1 when eventid = 8 then 1 else 0 end),
    SUM(case when eventid = 6 then 1 else 0 end),
    SUM(case when eventid = 5 then 1 else 0 end),
    MIN(case when eventid = 5 and worktime > 0 then worktime else 0 end),
    AVG(case when eventid = 5 and worktime > 0 then CAST(worktime as float) else 0 end),
    MAX(case when eventid = 5 and worktime > 0 then worktime else 0 end),
    MIN(case when eventid = 5 then elapsedtime else 0 end),
    AVG(case when eventid = 5 then CAST(elapsedtime as float) else 0 end),
    MAX(case when eventid = 5 then elapsedtime else 0 end),
    MIN(case when eventid = 5 then attempt-1 else 0 end),
    AVG(case when eventid = 5 then CAST(attempt-1 as float) else 0 end),
    MAX(case when eventid = 5 then attempt-1 else 0 end)
from BPMIProductivityShadow
where eventdatetime >= @ReportDate and eventdatetime < @ReportTo
group by queueident;

--Create daily records
insert into BPMIProductivityDaily select @ReportDate, * from @DailyProductivity;

--Update any existing monthly records
update BPMIProductivityMonthly set
    created = m.created + d.created,
    deferred = m.deferred + d.deferred,
    retried = m.retried + d.retried,
    exceptioned = m.exceptioned + d.exceptioned,
    completed = m.completed + d.completed,
    minelapsedtime = (case when d.minelapsedtime < m.minelapsedtime then d.minelapsedtime else m.minelapsedtime end),
    avgelapsedtime = (case when m.completed + d.completed > 0 then ((m.completed * m.avgelapsedtime) + (d.completed * d.avgelapsedtime)) / (m.completed + d.completed) else 0 end),
    maxelapsedtime = (case when d.maxelapsedtime > m.maxelapsedtime then d.maxelapsedtime else m.maxelapsedtime end),
    minworktime = (case when d.minworktime < m.minworktime then d.minworktime else m.minworktime end),
    avgworktime = (case when m.completed + d.completed > 0 then ((m.completed * m.avgworktime) + (d.completed * d.avgworktime)) / (m.completed + d.completed) else 0 end),
    maxworktime = (case when d.maxworktime > m.maxworktime then d.maxworktime else m.maxworktime end),
    minretries = (case when d.minretries < m.minretries then d.minretries else m.minretries end),
    avgretries = (case when m.completed + d.retried > 0 then ((m.completed * m.avgretries) + (d.completed * d.avgretries)) / (m.completed + d.retried) else 0 end),
    maxretries = (case when d.maxretries > m.maxretries then d.maxretries else m.maxretries end)
from BPMIProductivityMonthly m
    inner join @DailyProductivity d on m.queueident = d.queueident
where m.reportyear = DATEPART(YEAR, @ReportDate) and m.reportmonth = DATEPART(MONTH, @ReportDate);

--Insert new monthly records where required
insert into BPMIProductivityMonthly
select DATEPART(YEAR, @ReportDate), DATEPART(MONTH, @ReportDate), d.*
from @DailyProductivity d
    left join BPMIProductivityMonthly m on d.queueident = m.queueident
    and m.reportyear = DATEPART(YEAR, @ReportDate) and m.reportmonth = DATEPART(MONTH, @ReportDate)
where m.reportyear is null;

--Age out any old daily records
delete from BPMIProductivityDaily where reportdate < DATEADD(DAY, -@DaysToKeep, @ReportDate);

--Age out any old monthly records
delete from BPMIProductivityMonthly
where reportyear <= DATEPART(YEAR, (DATEADD(MONTH, -@MonthsToKeep, @ReportDate))) and
      reportmonth < DATEPART(MONTH, (DATEADD(MONTH, -@MonthsToKeep, @ReportDate)));

--Purge down shadow table (any events that occured before the day just reported on)
delete from BPMIProductivityShadow where eventdatetime < @ReportDate;

return;
GO

alter procedure usp_RefreshMI as
declare
    @Today datetime,
    @LastRefresh datetime,
    @ReportDate datetime,
    @DaysToKeep int,
    @MonthsToKeep int,
    @MIEnabled bit;

--Get MI config settings
select
    @MIEnabled=mienabled,
    @DaysToKeep=dailyfor,
    @MonthsToKeep=monthlyfor,
    @LastRefresh=lastrefresh
from BPAMIControl where id=1;

--Exit if not enabled
if @MIEnabled=0 return;

--Calculate reporting period start date
set @Today=CAST(FLOOR(CAST(GETDATE() as float)) as datetime);
if @LastRefresh is null
    set @ReportDate=CAST(FLOOR(CAST(DATEADD(DAY, -1, @Today) as float)) as datetime);
else
    set @ReportDate=CAST(FLOOR(CAST(DATEADD(DAY, 1, @LastRefresh) as float)) as datetime);

--Acquire MI refresh lock
update BPAMIControl set refreshinprogress=1 where id=1 and refreshinprogress=0;
if @@ROWCOUNT <> 1 return;

--Execute stored procedures to collect/aggregate MI data
begin try
    while @ReportDate<@Today
    begin
        begin transaction;
        exec usp_RefreshUtilisationData @ReportDate, @DaysToKeep, @MonthsToKeep;
        exec usp_RefreshProductivityData @ReportDate, @DaysToKeep, @MonthsToKeep;
        update BPAMIControl set lastrefresh=@ReportDate where id=1;
        commit;
        
        set @ReportDate=DATEADD(DAY, 1, @ReportDate);
    end
end try
begin catch
    rollback;
    declare @ErrMsg nvarchar(4000), @ErrSeverity int
    select @ErrMsg=ERROR_MESSAGE(), @ErrSeverity=ERROR_SEVERITY()
    raiserror(@ErrMsg, @ErrSeverity, 1)
end catch

--Release MI refresh lock
update BPAMIControl set refreshinprogress=0 where id=1;

return;
GO

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '186',
  GETUTCDATE(),
  'db_upgradeR186.sql UTC',
  'Adds Blue Prism database roles and refreshes dashboard stored procedures'
);
