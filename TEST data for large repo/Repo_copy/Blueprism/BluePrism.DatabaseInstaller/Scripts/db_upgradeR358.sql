/*
SCRIPT         : 358
AUTHOR         : Kevin Benson-White
PURPOSE        : Update BPDS Dashboard sources with specified column names
*/

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
    'Utilisation' AS [Queue Name],
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


ALTER procedure [BPDS_AverageHandlingTime]
    @BPQueueName nvarchar(max) = null,
    @NumberOfDays int = 7
as

if @NumberOfDays < 1 or @NumberOfDays > 90
    raiserror('@NumberOfDays must be between 1 and 90', 11, 1);
else
    select
        ISNULL(q.name, '<unknown>') AS [Queue Name],
        CAST(ISNULL(AVG(d.avgworktime), 0) as decimal(12,2)) as "Average Time"
    from BPMIProductivityDaily d
        left join BPAWorkQueue q on d.queueident = q.ident
    where d.reportdate >= (select MIN(TheDate) from ufn_GetReportDays(@NumberOfDays))
        and (@BPQueueName is null or @BPQueueName = q.name)
    group by q.name
	order by q.name;

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
('358', 
 GETUTCDATE(), 
 'db_upgradeR358.sql', 
 'BPDS procs with specified column names', 
 0
);