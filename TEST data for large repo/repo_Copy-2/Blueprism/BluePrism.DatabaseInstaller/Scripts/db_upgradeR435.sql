/*
SCRIPT         : 435
AUTHOR         : Mikhov Daniil
PURPOSE        : Amend BPDS_Get_Process_History_Per_Worker_Parameterised_Query stored procedure
*/

if exists (
		select *
		from sys.objects
		where type = 'P' and name = 'BPDS_Get_Process_History_Per_Worker_Parameterised_Query'
		)
begin
	exec (
			'ALTER PROC BPDS_Get_Process_History_Per_Worker_Parameterised_Query (
	@StartDate datetime --mandatory
	,@resourceId as GuidIdTableType READONLY --optional
	,@pageNumber integer = 1 --optional, default of 1
	,@pageSize integer = 10 --optional
	)
AS
BEGIN
	declare @StartDateFloor datetime
		,@EndDateCeiling datetime
	set @StartDateFloor = cast(floor(cast(@StartDate as float)) as datetime)
	set @EndDateCeiling = dateadd(ms, - 3, dateadd(dd, 1, cast(floor(cast(@StartDate as float)) as datetime)))
	declare @lresourceId as GuidIdTableType
	-- Each resource should show a full day regardless of params
	-- Check to see if resources have been passed in, if not, get all
	if exists (
			select 1
			from @resourceId
			)
	begin
		insert into @lresourceId (id)
		select RI.id
		from @resourceId RI
		-- assign a value if nothing is passed through
		if @pageSize is null
			set @pageSize = @@ROWCOUNT
	end
	else
	begin
		-- Apply paging here, to the number of Resources
		insert into @lresourceId (id)
		select R.resourceid
		from BPAResource R
		where R.[name] not like ''%debug%''
		order by R.[name] asc offset(@pageNumber - 1) rows
		fetch next @pageSize rows only;
	end
	select distinct rs.[resourceid]
		,rs.name DigitalWorkerName
		,cast(convert(char(16), startdatetime, 126) + '':00'' as datetime) dt1
		,cast(convert(char(16), enddatetime, 126) + '':00'' as datetime) dt2
	into #temp
	from BPAResource as rs
	inner join BPASession ph on ph.runningresourceid = rs.resourceid
	where ph.startdatetime > @StartDateFloor
		and ph.enddatetime < dateadd(day, 1, @StartDate)
		and exists (
			select 1
			from @lresourceId as ri
			where ri.id = rs.resourceid
			)
	order by resourceid
		,cast(convert(char(16), startdatetime, 126) + '':00'' as datetime)
		,cast(convert(char(16), enddatetime, 126) + '':00'' as datetime);
	with Dates_CTE
	as (
		select @StartDateFloor as UtilizationDate
			,1 Cnt
		union all
		select dateadd(hh, 1, UtilizationDate)
			,cnt + 1
		from Dates_CTE
		where Cnt < 24
		)
		,ResourceAndDate
	as (
		select distinct vw.resourceid ResourceId
			,DigitalWorkerName
			,UtilizationDate.UtilizationDate UtilizationDate
		from #temp vw
		join Dates_CTE UtilizationDate on 1 = 1
		)
		,main_ds
	as (
		select resourceid
			,[Time] = periodH
			,usage = SUM(diff)
		from (
			select distinct resourceid
				,periodH
				,period1
				,period2
				,diff
			from (
				select *
					,[diff] = DATEDIFF(MINUTE, case 
							when dt1 < period1
								then period1
							else dt1
							end, case 
							when dt2 > period2
								then period2
							else dt2
							end)
				from #temp as t
				-- hour slice --
				join (
					values (1),(2),(3),(4),(5),(6),(7),(8),(9),(10),(11),(12),
					(13),(14),(15),(16),(17),(18),(19),(20),(21),(22),(23)
					) hours(hh) on hh between datepart(HOUR, dt1)
						and datepart(HOUR, dt2)
				-- 5 minutes slice --
				join (
					values (0),(5),(10),(15),(20),(25),(30),(35),(40),(45),(50),(55)
					) minutes(mm) on t.dt2 between cast(convert(char(15), dt1, 126) + ''0:00'' as datetime)
						and cast(convert(char(16), dt2, 126) + '':00'' as datetime)
				outer apply (
					select [periodH] = convert(char(11), dt1, 126) + RIGHT(''0'' + cast(hh as varchar), 2) + '':00:00'' --Hour
						,[period1] = cast(convert(char(11), dt1, 126) + RIGHT(''0'' + cast(hh as varchar), 2) + '':'' + RIGHT(''0'' + cast(mm as varchar), 2) + '':00'' as datetime) -- Hours +MIn
						,[period2] = dateadd(minute, 5, cast(convert(char(11), dt1, 126) + RIGHT(''0'' + cast(hh as varchar), 2) + '':'' + RIGHT(''0'' + cast(mm as varchar), 2) + '':00'' as datetime)) --(Hours +MIn)+5 min sice
					) period
				) a
			where diff > 0
			) a
		group by resourceid
			,periodH
		)
	select rd.ResourceId
		,rd.DigitalWorkerName
		,rd.UtilizationDate
		,COALESCE(md.usage, 0) Usage
	from ResourceAndDate rd
	left join main_ds md on rd.resourceid = md.resourceid
		and md.[Time] = rd.UtilizationDate
	order by rd.DigitalWorkerName
		,rd.UtilizationDate
	drop table #temp
END
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
('435', 
 GETUTCDATE(), 
 'db_upgradeR435.sql', 
 'Amend BPDS_Get_Process_History_Per_Worker_Parameterised_Query stored procedure', 
 0
);
