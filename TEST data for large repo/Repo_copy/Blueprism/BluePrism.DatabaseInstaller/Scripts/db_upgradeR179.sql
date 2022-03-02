/*
SCRIPT         : 179
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : GM
PURPOSE        : Adds new tables and stored procedures for MI reporting
*/

------------------------------------------------------------------------------------
-- MI Related Tables
------------------------------------------------------------------------------------

--MI control table
create table BPAMIControl (
    id int not null default 1,
    mienabled bit default 0,
    autorefresh bit default 0,
    refreshat datetime,
    lastrefresh datetime,
    refreshinprogress bit default 0,
    dailyfor int default 30,
    monthlyfor int default 6,
    constraint PK_BPAMIControl primary key clustered (id)
)

--Utilisation MI shadow table
create table BPMIUtilisationShadow (
    sessionid uniqueidentifier not null,
    resourceid uniqueidentifier not null,
    processid uniqueidentifier not null,
    startdatetime datetime not null,
    enddatetime datetime null,
    constraint PK_BPMIUtilisationShadow primary key clustered (sessionid)
);

--Utilisation MI daily statistics table
create table BPMIUtilisationDaily (
    reportdate smalldatetime not null,
    resourceid uniqueidentifier not null,
    processid uniqueidentifier not null,
    hr0 int null, hr1 int null, hr2 int null, hr3 int null,
    hr4 int null, hr5 int null, hr6 int null, hr7 int null,
    hr8 int null, hr9 int null, hr10 int null, hr11 int null,
    hr12 int null, hr13 int null, hr14 int null, hr15 int null,
    hr16 int null, hr17 int null, hr18 int null, hr19 int null,
    hr20 int null, hr21 int null, hr22 int null, hr23 int null,
    constraint PK_BPMIUtilisationDaily primary key clustered (reportdate,resourceid,processid)
);

--Utilisation MI monthly statistics table
create table BPMIUtilisationMonthly (
    reportyear int not null,
    reportmonth int not null,
    resourceid uniqueidentifier not null,
    processid uniqueidentifier not null,
    hr0 int null, hr1 int null, hr2 int null, hr3 int null,
    hr4 int null, hr5 int null, hr6 int null, hr7 int null,
    hr8 int null, hr9 int null, hr10 int null, hr11 int null,
    hr12 int null, hr13 int null, hr14 int null, hr15 int null,
    hr16 int null, hr17 int null, hr18 int null, hr19 int null,
    hr20 int null, hr21 int null, hr22 int null, hr23 int null,
    constraint PK_BPMIUtilisationMonthly primary key clustered (reportyear,reportmonth,resourceid,processid)
);

--Productivity MI shadow table
create table BPMIProductivityShadow (
    ident bigint identity(1,1) not null,
    eventdatetime datetime not null,
    queueident int not null,
    itemid uniqueidentifier not null,
    eventid int not null,
    worktime int null,
    elapsedtime int null,
    attempt int null,
    constraint PK_BPMIProductivityShadow primary key clustered (ident)
)

--Productivity MI daily statistics table
create table BPMIProductivityDaily (
    reportdate smalldatetime not null,
    queueident int not null,
    created int null,
    deferred int null,
    retried int null,
    exceptioned int null,
    completed int null,
    minworktime int null,
    avgworktime decimal(12,2) null,
    maxworktime int null,
    minelapsedtime int null,
    avgelapsedtime decimal(12,2) null,
    maxelapsedtime int null,
    minretries int null,
    avgretries decimal(12,2) null,
    maxretries int null,
    constraint PK_BPMIProductivityDaily primary key clustered (reportdate, queueident)
)

--Productivity MI monthly statistics table
create table BPMIProductivityMonthly (
    reportyear int not null,
    reportmonth int not null,
    queueident int not null,
    created int null,
    deferred int null,
    retried int null,
    exceptioned int null,
    completed int null,
    minworktime int null,
    avgworktime decimal(12,2) null,
    maxworktime int null,
    minelapsedtime int null,
    avgelapsedtime decimal(12,2) null,
    maxelapsedtime int null,
    minretries int null,
    avgretries decimal(12,2) null,
    maxretries int null,
    constraint PK_BPMIProductivityMonthly primary key clustered (reportyear, reportmonth, queueident)
)

--Insert default MI control settings
insert into BPAMIControl (id) values (1);
GO

------------------------------------------------------------------------------------
-- MI Refresh Stored Procedures
------------------------------------------------------------------------------------

-- Create a stub for them first, then alter them later
-- It means it doesn't stall the upgrade if any one of these already exists
if not exists (select * from sys.objects where type = 'P' and name = 'usp_RefreshProductivityData')
   exec(N'create procedure usp_RefreshProductivityData as begin set nocount on; end');

if not exists (select * from sys.objects where type = 'P' and name = 'usp_RefreshUtilisationData')
   exec(N'create procedure usp_RefreshUtilisationData as begin set nocount on; end');

if not exists (select * from sys.objects where type = 'P' and name = 'usp_RefreshMI')
   exec(N'create procedure usp_RefreshMI as begin set nocount on; end');

if not exists (select * from sys.objects where type = 'P' and name = 'BPDS_WorkforceAvailability')
   exec(N'create procedure BPDS_WorkforceAvailability as begin set nocount on; end');

if not exists (select * from sys.objects where type = 'P' and name = 'BPDS_TotalAutomations')
   exec(N'create procedure BPDS_TotalAutomations as begin set nocount on; end');

if not exists (select * from sys.objects where type = 'P' and name = 'BPDS_LargestTables')
   exec(N'create procedure BPDS_LargestTables as begin set nocount on; end');

if not exists (select * from sys.objects where type = 'P' and name = 'BPDS_DailyUtilisation')
   exec(N'create procedure BPDS_DailyUtilisation as begin set nocount on; end');

if not exists (select * from sys.objects where type = 'P' and name = 'BPDS_ProcessUtilisationByHour')
   exec(N'create procedure BPDS_ProcessUtilisationByHour as begin set nocount on; end');

if not exists (select * from sys.objects where type = 'P' and name = 'BPDS_ResourceUtilisationByHour')
   exec(N'create procedure BPDS_ResourceUtilisationByHour as begin set nocount on; end');

if not exists (select * from sys.objects where type = 'P' and name = 'BPDS_AverageHandlingTime')
   exec(N'create procedure BPDS_AverageHandlingTime as begin set nocount on; end');

if not exists (select * from sys.objects where type = 'P' and name = 'BPDS_DailyProductivity')
   exec(N'create procedure BPDS_DailyProductivity as begin set nocount on; end');

if not exists (select * from sys.objects where type = 'P' and name = 'BPDS_FTEProductivityComparison')
   exec(N'create procedure BPDS_FTEProductivityComparison as begin set nocount on; end');

if not exists (select * from sys.objects where type = 'P' and name = 'BPDS_AverageRetries')
   exec(N'create procedure BPDS_AverageRetries as begin set nocount on; end');

if not exists (select * from sys.objects where type = 'P' and name = 'BPDS_Exceptions')
   exec(N'create procedure BPDS_Exceptions as begin set nocount on; end');

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
    MIN(case when eventid = 5 then worktime end),
    AVG(case when eventid = 5 then CAST(worktime as float) end),
    MAX(case when eventid = 5 then worktime end),
    MIN(case when eventid = 5 then elapsedtime end),
    AVG(case when eventid = 5 then CAST(elapsedtime as float) end),
    MAX(case when eventid = 5 then elapsedtime end),
    MIN(case when eventid = 5 then attempt end),
    AVG(case when eventid = 5 then CAST(attempt as float) end),
    MAX(case when eventid = 5 then attempt end)
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
    avgelapsedtime = ((m.completed * m.avgelapsedtime) + (d.completed * d.avgelapsedtime)) / (m.completed + d.completed),
    maxelapsedtime = (case when d.maxelapsedtime > m.maxelapsedtime then d.maxelapsedtime else m.maxelapsedtime end),
    minworktime = (case when d.minworktime < m.minworktime then d.minworktime else m.minworktime end),
    avgworktime = ((m.completed * m.avgworktime) + (d.completed * d.avgworktime)) / (m.completed + d.completed),
    maxworktime = (case when d.maxworktime > m.maxworktime then d.maxworktime else m.maxworktime end),
    minretries = (case when d.minretries < m.minretries then d.minretries else m.minretries end),
    avgretries = ((m.completed * m.avgretries) + (d.completed * d.avgretries)) / (m.completed + d.retried),
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
                        then DATEDIFF(SECOND, intervals.StartDate, intervals.EndDate)
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
    hr12 = hr12 + d.h12, hr13 = hr13 + d.h13, hr14 = hr14 + d.h14, hr15 = hr15 + d.h15, hr16 = hr16 + d.h16, hr17 = hr17+d.h17,
    hr18 = hr18+d.h18, hr19 = hr19 + d.h19, hr20 = hr20 + d.h20, hr21 = hr21 + d.h21, hr22 = hr22 + d.h22, hr23 = hr23 + d.h23
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
set @Today=CAST(FLOOR(CAST(GETUTCDATE() as float)) as datetime);
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

------------------------------------------------------------------------------------
-- Chart Data Sources
------------------------------------------------------------------------------------
-- Status: Workforce Availability
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

-- Status: Total Automations
alter procedure BPDS_TotalAutomations
as

select
    case when ProcessType = 'O' then 'Objects' else 'Processes' end as [Type],
    COUNT(*) as Total
from BPAProcess
where ProcessType in ('O', 'P')
group by ProcessType;

return;
GO

-- Status: Largest Tables
alter procedure BPDS_LargestTables
    @NumberOfTables int = 5
as

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

-- Utilisation: Daily Utilisation
alter procedure BPDS_DailyUtilisation
    @BPResourceName nvarchar(max) = null,
    @NumberOfDays int = 7,
    @DisplayUnits nvarchar(max) = 'minute'
as

declare @Today datetime;
set @Today = CAST(FLOOR(CAST(GETUTCDATE() as float)) as datetime);

select
    DATENAME(day, u.reportdate) + '-' + DATENAME(month, u.reportdate) as "Day",
    case when @DisplayUnits = 'second' then CAST(Total as decimal(12,2))
         when @DisplayUnits = 'hour' then CAST(Total/3600 as decimal(12,2))
         when @DisplayUnits = 'percentage' then CAST(Total/(Resources*86400/100) as decimal(12,2))
         else CAST(Total/60 as decimal(12,2)) end as "Total"
from (
    select
        d.reportdate,
        CAST(SUM(d.hr0 + d.hr1 + d.hr2 + d.hr3 + d.hr4 + d.hr5 + d.hr6 + d.hr7 + 
                d.hr8 + d.hr9 + d.hr10 + d.hr11 + d.hr12 + d.hr13 + d.hr14 + d.hr15 + 
                d.hr16 + d.hr17 + d.hr18 + d.hr19 + d.hr20 + d.hr21 + d.hr22 + d.hr23) as float)
                as "Total",
        COUNT(distinct(r.resourceid)) as "Resources"
    from
        BPMIUtilisationDaily d
        inner join BPAResource r on d.resourceid = r.resourceid
    where
        d.reportdate >= DATEADD(day, -@NumberOfDays, @Today) and
        (@BPResourceName is null or @BPResourceName = r.name)
    group by
        d.reportdate
    ) as u;

return;
GO

-- Utilisation: Process Utilisation
alter procedure BPDS_ProcessUtilisationByHour
    @BPProcessName nvarchar(max) = null,
    @DisplayUnits nvarchar(max) = 'minute'
as

declare @Today datetime;
set @Today = CAST(FLOOR(CAST(GETUTCDATE() as float)) as datetime);

declare @Units int;
select @Units = case when @DisplayUnits = 'second' then 1 when @DisplayUnits = 'hour' then 3600 else 60 end;

select
    ProcessName,
    case when @DisplayUnits = 'percentage' then CAST(Interval1/(Resources*7200/100) as decimal(12,2))
         else CAST(Interval1/@Units as decimal(12,2)) end as "00:00",
    case when @DisplayUnits = 'percentage' then CAST(Interval2/(Resources*7200/100) as decimal(12,2))
         else CAST(Interval2/@Units as decimal(12,2)) end as "02:00",
    case when @DisplayUnits = 'percentage' then CAST(Interval3/(Resources*7200/100) as decimal(12,2))
        else CAST(Interval3/@Units as decimal(12,2)) end as "04:00",
    case when @DisplayUnits = 'percentage' then CAST(Interval4/(Resources*7200/100) as decimal(12,2))
        else CAST(Interval4/@Units as decimal(12,2)) end as "06:00",
    case when @DisplayUnits = 'percentage' then CAST(Interval5/(Resources*7200/100) as decimal(12,2))
        else CAST(Interval5/@Units as decimal(12,2)) end as "08:00",
    case when @DisplayUnits = 'percentage' then CAST(Interval6/(Resources*7200/100) as decimal(12,2))
        else CAST(Interval6/@Units as decimal(12,2)) end as "10:00",
    case when @DisplayUnits = 'percentage' then CAST(Interval7/(Resources*7200/100) as decimal(12,2))
        else CAST(Interval7/@Units as decimal(12,2)) end as "12:00",
    case when @DisplayUnits = 'percentage' then CAST(Interval8/(Resources*7200/100) as decimal(12,2))
        else CAST(Interval8/@Units as decimal(12,2)) end as "14:00",
    case when @DisplayUnits = 'percentage' then CAST(Interval9/(Resources*7200/100) as decimal(12,2))
        else CAST(Interval9/@Units as decimal(12,2)) end as "16:00",
    case when @DisplayUnits = 'percentage' then CAST(Interval10/(Resources*7200/100) as decimal(12,2))
        else CAST(Interval10/@Units as decimal(12,2)) end as "18:00",
    case when @DisplayUnits = 'percentage' then CAST(Interval11/(Resources*7200/100) as decimal(12,2))
        else CAST(Interval11/@Units as decimal(12,2)) end as "20:00",
    case when @DisplayUnits = 'percentage' then CAST(Interval12/(Resources*7200/100) as decimal(12,2))
        else CAST(Interval12/@Units as decimal(12,2)) end as "22:00"
from (
    select
        p.name as ProcessName,
        CAST(CAST(SUM(d.hr0 + d.hr1) as decimal) as decimal(12,2)) as "Interval1",
        CAST(CAST(SUM(d.hr2 + d.hr3) as decimal) as decimal(12,2)) as "Interval2",
        CAST(CAST(SUM(d.hr4 + d.hr5) as decimal) as decimal(12,2)) as "Interval3",
        CAST(CAST(SUM(d.hr6 + d.hr7) as decimal) as decimal(12,2)) as "Interval4",
        CAST(CAST(SUM(d.hr8 + d.hr9) as decimal) as decimal(12,2)) as "Interval5",
        CAST(CAST(SUM(d.hr10 + d.hr11) as decimal) as decimal(12,2)) as "Interval6",
        CAST(CAST(SUM(d.hr12 + d.hr13) as decimal) as decimal(12,2)) as "Interval7",
        CAST(CAST(SUM(d.hr14 + d.hr15) as decimal) as decimal(12,2)) as "Interval8",
        CAST(CAST(SUM(d.hr16 + d.hr17) as decimal) as decimal(12,2)) as "Interval9",
        CAST(CAST(SUM(d.hr18 + d.hr19) as decimal) as decimal(12,2)) as "Interval10",
        CAST(CAST(SUM(d.hr20 + d.hr21) as decimal) as decimal(12,2)) as "Interval11",
        CAST(CAST(SUM(d.hr22 + d.hr23) as decimal) as decimal(12,2)) as "Interval12",
        COUNT(distinct(d.resourceid)) as "Resources"
    from
        BPMIUtilisationDaily d
        inner join BPAProcess p on d.processid = p.processid
    where
        d.reportdate = DATEADD(day, -1, @Today) and
        (@BPProcessName is null or @BPProcessName = p.name)
    group by
        p.name
    ) as u;

return;
GO

-- Utilisation: Resource Utilisation
alter procedure BPDS_ResourceUtilisationByHour
    @BPResourceName nvarchar(max) = null,
    @DisplayUnits nvarchar(max) = 'minute'
as

declare @Today datetime;
set @Today = CAST(FLOOR(CAST(GETUTCDATE() as float)) as datetime);

declare @Units decimal;
select @Units = case when @DisplayUnits = 'second' then 1 when @DisplayUnits = 'hour' then 3600 else 60 end;

select 
    'Utilisation',
    case when @DisplayUnits = 'percentage' then CAST(Interval1/(Resources*7200/100) as decimal(12,2))
         else CAST(Interval1/@Units as decimal(12,2)) end as "00:00",
    case when @DisplayUnits = 'percentage' then CAST(Interval2/(Resources*7200/100) as decimal(12,2))
         else CAST(Interval2/@Units as decimal(12,2)) end as "02:00",
    case when @DisplayUnits = 'percentage' then CAST(Interval3/(Resources*7200/100) as decimal(12,2))
        else CAST(Interval3/@Units as decimal(12,2)) end as "04:00",
    case when @DisplayUnits = 'percentage' then CAST(Interval4/(Resources*7200/100) as decimal(12,2))
        else CAST(Interval4/@Units as decimal(12,2)) end as "06:00",
    case when @DisplayUnits = 'percentage' then CAST(Interval5/(Resources*7200/100) as decimal(12,2))
        else CAST(Interval5/@Units as decimal(12,2)) end as "08:00",
    case when @DisplayUnits = 'percentage' then CAST(Interval6/(Resources*7200/100) as decimal(12,2))
        else CAST(Interval6/@Units as decimal(12,2)) end as "10:00",
    case when @DisplayUnits = 'percentage' then CAST(Interval7/(Resources*7200/100) as decimal(12,2))
        else CAST(Interval7/@Units as decimal(12,2)) end as "12:00",
    case when @DisplayUnits = 'percentage' then CAST(Interval8/(Resources*7200/100) as decimal(12,2))
        else CAST(Interval8/@Units as decimal(12,2)) end as "14:00",
    case when @DisplayUnits = 'percentage' then CAST(Interval9/(Resources*7200/100) as decimal(12,2))
        else CAST(Interval9/@Units as decimal(12,2)) end as "16:00",
    case when @DisplayUnits = 'percentage' then CAST(Interval10/(Resources*7200/100) as decimal(12,2))
        else CAST(Interval10/@Units as decimal(12,2)) end as "18:00",
    case when @DisplayUnits = 'percentage' then CAST(Interval11/(Resources*7200/100) as decimal(12,2))
        else CAST(Interval11/@Units as decimal(12,2)) end as "20:00",
    case when @DisplayUnits = 'percentage' then CAST(Interval12/(Resources*7200/100) as decimal(12,2))
        else CAST(Interval12/@Units as decimal(12,2)) end as "22:00"
from (
    select
        CAST(CAST(SUM(d.hr0 + d.hr1) as decimal) as decimal(12,2)) as "Interval1",
        CAST(CAST(SUM(d.hr2 + d.hr3) as decimal) as decimal(12,2)) as "Interval2",
        CAST(CAST(SUM(d.hr4 + d.hr5) as decimal) as decimal(12,2)) as "Interval3",
        CAST(CAST(SUM(d.hr6 + d.hr7) as decimal) as decimal(12,2)) as "Interval4",
        CAST(CAST(SUM(d.hr8 + d.hr9) as decimal) as decimal(12,2)) as "Interval5",
        CAST(CAST(SUM(d.hr10 + d.hr11) as decimal) as decimal(12,2)) as "Interval6",
        CAST(CAST(SUM(d.hr12 + d.hr13) as decimal) as decimal(12,2)) as "Interval7",
        CAST(CAST(SUM(d.hr14 + d.hr15) as decimal) as decimal(12,2)) as "Interval8",
        CAST(CAST(SUM(d.hr16 + d.hr17) as decimal) as decimal(12,2)) as "Interval9",
        CAST(CAST(SUM(d.hr18 + d.hr19) as decimal) as decimal(12,2)) as "Interval10",
        CAST(CAST(SUM(d.hr20 + d.hr21) as decimal) as decimal(12,2)) as "Interval11",
        CAST(CAST(SUM(d.hr22 + d.hr23) as decimal) as decimal(12,2)) as "Interval12",
        COUNT(distinct(r.resourceid)) as "Resources"
    from
        BPMIUtilisationDaily d
        inner join BPAResource r on d.resourceid = r.resourceid
    where
        d.reportdate = DATEADD(day, -1, @Today) and
        (@BPResourceName is null or @BPResourceName = r.name)
    ) as u;

return;
GO

-- Productivity: Average Handling Time
alter procedure BPDS_AverageHandlingTime
    @BPQueueName nvarchar(max) = null,
    @NumberOfDays int = 7
as

declare @Today datetime;
set @Today = CAST(FLOOR(CAST(GETUTCDATE() as float)) as datetime);

select
    q.name,
    CAST(AVG(d.avgworktime) as decimal(12,2)) as "Average Time"
from BPMIProductivityDaily d
    inner join BPAWorkQueue q on d.queueident = q.ident
where d.reportdate > DATEADD(day, -@NumberOfDays, @Today)
group by q.name;

return;
GO

-- Productivity: Daily Productivity
alter procedure BPDS_DailyProductivity
    @BPQueueName nvarchar(max) = null,
    @NumberOfDays int = 7
as

declare @Today datetime;
set @Today = CAST(FLOOR(CAST(GETUTCDATE() as float)) as datetime);

select
    d.reportdate,
    SUM(d.created) as New,
    SUM(d.deferred) as Deferred,
    SUM(d.completed) as Complete,
    SUM(d.retried) as Retries,
    SUM(d.exceptioned) as Exceptions
from BPMIProductivityDaily d
    inner join BPAWorkQueue q on d.queueident = q.ident
where d.reportdate > DATEADD(day, -@NumberOfDays, @Today) and
    (@BPQueueName is null or @BPQueueName = q.name)
group by d.reportdate;

return;
GO

-- Productivity: FTE Comparison
alter procedure BPDS_FTEProductivityComparison
    @BPQueueName nvarchar(max) = null,
    @NumberOfMonths int = 6,
    @FTEProductivity decimal(12,2) = 0,
    @FTECost decimal(12,2) = 0,
    @DisplayAs nvarchar(max) = 'percentage'
as

declare @FromYear int, @FromMonth int;
set @FromYear = DATEPART(YEAR, (DATEADD(MONTH, -@NumberOfMonths, GETUTCDATE())));
set @FromMonth = DATEPART(MONTH, (DATEADD(MONTH, -@NumberOfMonths, GETUTCDATE())));
      
declare @Today datetime;
set @Today = CAST(FLOOR(CAST(GETUTCDATE() as float)) as datetime);

select
    TheDate,
    case when @FTEProductivity <> 0 then
            case when @DisplayAs = 'cost' then CAST((completed/(@FTEProductivity*DaysInMonth))*@FTECost as decimal(12,2))
            when @DisplayAs = 'number' then CAST(completed/(@FTEProductivity*DaysInMonth) as decimal(12,2))
            else CAST((completed/(@FTEProductivity*DaysInMonth))*100 as decimal(12,2)) end
    else completed  end
from (
    select
        DATENAME(month, DATEADD(month, reportmonth, -1)) + ' ' + CAST(reportyear as nvarchar(4)) as TheDate,
        DAY(DATEADD(day, -1, DATEADD(month, 1, CAST(CAST(reportyear as nvarchar) + '-' + CAST(reportmonth as nvarchar) + '-1' as datetime)))) as DaysInMonth,
        SUM(completed) as Completed
    from BPMIProductivityMonthly m
        inner join BPAWorkQueue q on m.queueident = q.ident
    where m.reportyear >= @FromYear and m.reportmonth > @FromMonth and
        (@BPQueueName is null or @BPQueueName = q.name)
    group by reportmonth, reportyear
) as p

return;
GO

-- Accuracy: Average Retries
alter procedure BPDS_AverageRetries
    @BPQueueName nvarchar(max) = null,
    @NumberOfDays int = 7
as

declare @Today datetime;
set @Today = CAST(FLOOR(CAST(GETUTCDATE() as float)) as datetime);

select
    q.name,
    CAST(AVG(d.avgretries) as decimal(12,2)) as "Retries"
from BPMIProductivityDaily d
    inner join BPAWorkQueue q on d.queueident = q.ident
where d.reportdate > DATEADD(day, -@NumberOfDays, @Today)
group by q.name;

return;
GO

-- Accuracy: Exceptions
alter procedure BPDS_Exceptions
    @BPQueueName nvarchar(max) = null
as

declare @Today datetime;
set @Today = CAST(FLOOR(CAST(GETUTCDATE() as float)) as datetime);

declare @ColumnName nvarchar(max);
declare @Query nvarchar(max);
declare @WhereClause nvarchar(max);
if @BPQueueName is not null
    set @WhereClause = ' and q.name = ''' + @BPQueueName + '''';

with dates as (
select DATEADD(day, -1, @Today) as TheDate union
select DATEADD(day, -2, @Today) as TheDate union
select DATEADD(day, -3, @Today) as TheDate)

select @ColumnName = ISNULL(@ColumnName + ',', '') + QUOTENAME(CONVERT(nvarchar(10), TheDate, 1)) from dates;

set @Query = 
'select
    name, ' + @ColumnName + '
from (select
        q.name,
        convert(nvarchar(10), d.reportdate, 1) as pivotdate,
        d.exceptioned
    from BPMIProductivityDaily d
        inner join BPAWorkQueue q on d.queueident = q.ident
    where d.reportdate > DATEADD(day, -3, ' + CONVERT(nvarchar(10), @Today, 101) + ')
        ' + ISNULL(@WhereClause, '') + ') as src
pivot (sum(exceptioned) for pivotdate in (' + @ColumnName + ')) as pvt'
exec(@Query)

return;
GO

------------------------------------------------------------------------------------
-- Create Tiles and Default Global Dashboard
------------------------------------------------------------------------------------
delete from BPATile;

insert into BPATile (id, name, tiletype, description, autorefresh, xmlproperties)
 values(newid(),'Workforce Availability',0,'Percentage of registered resources available for work',0,'<Chart type="8" plotByRow="false"><Procedure name="BPDS_WorkforceAvailability" /></Chart>');

insert into BPATile (id, name, tiletype, description, autorefresh, xmlproperties)
 values(newid(),'Largest Database Tables',0,'The largest five tables in the database (Mb)',0,'<Chart type="6" plotByRow="false"><Procedure name="BPDS_LargestTables" /></Chart>');

insert into BPATile (id, name, tiletype, description, autorefresh, xmlproperties)
 values(newid(),'Total Automations',0,'Number of Objects and Processes in the database',0,'<Chart type="6" plotByRow="false"><Procedure name="BPDS_TotalAutomations" /></Chart>');

insert into BPATile (id, name, tiletype, description, autorefresh, xmlproperties)
 values(newid(),'Daily Utilisation Summary',0,'Overall resource utilisation over last 7 days',0,'<Chart type="3" plotByRow="false"><Procedure name="BPDS_DailyUtilisation" /></Chart>');

insert into BPATile (id, name, tiletype, description, autorefresh, xmlproperties)
 values(newid(),'Process Utilisation Yesterday',0,'Overall time spent running each process',0,'<Chart type="4" plotByRow="true"><Procedure name="BPDS_ProcessUtilisationByHour" /></Chart>');

insert into BPATile (id, name, tiletype, description, autorefresh, xmlproperties)
 values(newid(),'Resource Utilisation Yesterday',0,'Overall time spent running sessions',0,'<Chart type="3" plotByRow="true"><Procedure name="BPDS_ResourceUtilisationByHour" /></Chart>');

insert into BPATile (id, name, tiletype, description, autorefresh, xmlproperties)
 values(newid(),'Daily Productivity',0,'Number of new/deferred/completed cases in last 7 days',0,'<Chart type="3" plotByRow="false"><Procedure name="BPDS_DailyProductivity" /></Chart>');

insert into BPATile (id, name, tiletype, description, autorefresh, xmlproperties)
 values(newid(),'Average Handling Time',0,'Average work time for completed cases by queue',0,'<Chart type="0" plotByRow="false"><Procedure name="BPDS_AverageHandlingTime" /></Chart>');

insert into BPATile (id, name, tiletype, description, autorefresh, xmlproperties)
 values(newid(),'FTE Comparison',0,'Productivity in comparison to a human FTE',0,'<Chart type="3" plotByRow="false"><Procedure name="BPDS_FTEProductivityComparison" /></Chart>');

insert into BPATile (id, name, tiletype, description, autorefresh, xmlproperties)
 values(newid(),'Average Retries',0,'Average number of retries for completed cases by queue',0,'<Chart type="6" plotByRow="false"><Procedure name="BPDS_AverageRetries" /></Chart>');

insert into BPATile (id, name, tiletype, description, autorefresh, xmlproperties)
 values(newid(),'Exceptions',0,'Number of exceptions by queue',0,'<Chart type="3" plotByRow="false"><Procedure name="BPDS_Exceptions" /></Chart>');
 
--Create default dashboard
delete from BPADashboard;

insert into BPADashboard (id, name, dashtype, userid)
values('00000000-0000-0000-0000-000000000000', 'Default dashboard', 0, null);

insert into BPADashboardTile (dashid, tileid, displayorder, width, height)
select '00000000-0000-0000-0000-000000000000', t.id, ROW_NUMBER() over (order by t.id), 1, 1 from BPATile t
where t.name in ('Workforce Availability');

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '179',
  GETUTCDATE(),
  'db_upgradeR179.sql UTC',
  'Adds new tables and stored procedures for MI reporting'
);
