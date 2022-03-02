/*
SCRIPT         : 433
AUTHOR         : Paul Cooke
PURPOSE        : Add BPDS_Get_Process_History_By_Date_Range_Query stored procedure
*/

if not exists (
		select *
		from sys.objects
		where type = 'P' and name = 'BPDS_Get_Process_History_By_Date_Range_Query'
		)
begin
	exec (
			'
create proc BPDS_Get_Process_History_By_Date_Range_Query
(
@StartDate datetime,
@EndDate datetime,
@resourceId as GuidIdTableType readonly,
@AttributeID AS IntIdTableType readonly
)
as
begin
	

	declare
	@StartDateFloor datetime,
	@EndDateCeiling datetime
	set @StartDateFloor = cast(floor(cast(@StartDate as float)) as datetime)
	set @EndDateCeiling = dateadd(ms, -3, dateadd(dd, 1, cast(floor(cast(@EndDate as float)) as datetime)))

	declare @lresourcId as GuidIdTableType

	if exists(select 1 from @resourceId)
	begin	
		insert into @lresourcId([id])
		select id from @resourceId as [ri]
	end
	else
	begin
		insert into @lresourcId([id])
		select br.[resourceid] from [dbo].[BPAResource] as [br]
		where br.[name] not like ''%debug%''
    end

	select distinct rs.[resourceid], rs.name VirtualWorkerName, cast(convert(char(16), startdatetime,126)+'':00'' as datetime)dt1, cast(convert(char(16), enddatetime,126)  +'':00'' as datetime)dt2
	into #temp
	from [dbo].[BPAResource] as [rs] 
			inner join [dbo].[BPASession] ph on ph.runningresourceid = rs.resourceid
	where ph.startdatetime > @StartDateFloor and  ph.[enddatetime] < dateadd(day,1,@EndDateCeiling) 
	and exists(select 1 from @lresourcId as [ri] where ri.id=rs.[resourceid])
	and not exists( select 1 from @AttributeID a where  rs.[AttributeID]=a.[id])
	order by resourceid, cast(convert(char(16), startdatetime,126)+'':00'' as datetime), cast(convert(char(16), enddatetime,126)  +'':00'' as datetime)


	;with Dates_CTE as
	(
		select @StartDateFloor as Dates, 1 Cnt
		union all
		select dateadd(hh, 1, Dates), cnt+1
		from   Dates_CTE
		where  Cnt < (datediff(day,@StartDateFloor,@EndDateCeiling)+1)*24
	),
	main_ds as
	(
	select  [Time] = periodeH, [usage] = SUM(diff)
	from(
			select distinct periodeH,periode1,periode2,diff
			from(
					select *,[diff]=datediff(minute
											,case when dt1 < periode1 then periode1 else dt1 end
											,case when dt2 > periode2 then periode2 else dt2 end
										 )

					from [#temp] AS [t]
						-- hour slice --
					join (values(1),(2),(3),(4),(5),(6),(7),(8),(9),(10),(11),(12),(13),(14),(15),(16),(17),(18),(19),(20),(21),(22),(23)
						 )hours(hh) on hh between datepart(hour,dt1) and  datepart(hour,dt2)
						-- 5 minutes slice --
					join (values(0),(5),(10),(15),(20),(25),(30),(35),(40),(45),(50),(55)
						 )minutes(mm) on  t.dt2 between cast(convert(char(15),dt1,126)+''0:00'' as datetime) and cast(convert(char(16),dt2,126)+'':00'' as datetime)
					outer apply(select 
									 [periodeH] = convert(char(11), dt1 ,126)+right(''0''+cast(hh as varchar),2)+'':00:00'' --Hour
									,[periode1] = cast(convert(char(11), dt1,126)+right(''0''+cast(hh as varchar),2)+'':''+right(''0''+cast(mm as varchar),2)+'':00'' as datetime)-- Hours +MIn
									,[periode2] = dateadd(minute,5,cast(convert(char(11), dt1 ,126)+right(''0''+cast(hh as varchar),2)+'':''+right(''0''+cast(mm as varchar),2)+'':00'' as datetime))--(Hours +MIn)+5 min sice
								)periode
				)a
			where diff>0
		)a group by periodeH
	)


	select dt.[Dates],coalesce(md.[usage],0)usage from
	Dates_CTE Dt 
	left join main_ds md ON [Dt].[Dates]=md.[Time]  option (maxrecursion 2500) 


end
')
end
go



---- Set DB version.
insert into BPADBVersion
(dbversion, 
 scriptrundate, 
 scriptname, 
 [description], 
 timezoneoffset
)
values
('433', 
 GETUTCDATE(), 
 'db_upgradeR433.sql', 
 'Add BPDS_Get_Process_History_By_Date_Range_Query stored procedure', 
 0
);
