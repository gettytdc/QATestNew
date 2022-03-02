/*
SCRIPT         : 439
AUTHOR         : Tomasz Zelewski, Vinu Thomas
PURPOSE        : Update BPDS_Get_Process_History_Per_Worker_Parameterised_Query stored proc, update BPDS_Get_Process_History_By_Date_Range_Query stored proc, grant permissions for bpa_ExecuteSP_System user
*/

set ansi_nulls on;
go

set quoted_identifier on;
go

if (
	select object_id('BPDS_Get_Process_History_Per_Worker_Parameterised_Query')
	) is not null
drop proc BPDS_Get_Process_History_Per_Worker_Parameterised_Query;

go

create proc BPDS_Get_Process_History_Per_Worker_Parameterised_Query (
	@startdate datetime,					--mandatory
	@resourceid as GuidIdTableType readonly,--optional
	@pagenumber integer = 1,				--optional, default of 1
	@pagesize integer = 10					--optional
	)
as
begin
	declare @startsatefloor datetime
		,@enddateceiling datetime;
	set @startsatefloor = cast(floor(cast(@startdate as float)) as datetime);
	set @enddateceiling = dateadd(ms, - 3, dateadd(dd, 1, cast(floor(cast(@startdate as float)) as datetime)));
	declare @lresourceId as GuidIdTableType;
	-- Each resource should show a full day regardless of params. Check to see if resources have been passed in, if not, get all
	if exists (
			select 1
			from @resourceid
			)
	begin
		insert into @lresourceId (id)
		select RI.id
		from @resourceid RI;
		if @pagesize is null
			set @pagesize = @@ROWCOUNT;
	end;
	else
	begin
		-- Apply paging here, to the number of Resources
		insert into @lresourceId (id)
		select R.resourceid
		from BPAResource R
		where R.[name] not like '%debug%'
		order by R.[name] asc OFFSET((@pagenumber - 1) * @pagesize) rows
		fetch next @pagesize rows only;
	end;
	select distinct ph.sessionnumber
		,rs.resourceid
		,rs.[name] as digitalworkername
		,isnull(cast(convert(char(16), case 
										when ph.startdatetime < @startsatefloor
											then @startsatefloor
										else ph.startdatetime
										end, 126) + ':00' as datetime), @startsatefloor) dt1
		,isnull(cast(convert(char(16), case 
										when ph.enddatetime > @enddateceiling
											then @enddateceiling
										else ph.enddatetime
										end, 126) + ':00' as datetime), @startsatefloor) dt2
	into #temp_dayDS
	from @lresourceId ri
	left join BPAResource as rs on ri.id = rs.resourceid
	left join BPASession ph on ph.runningresourceid = rs.resourceid
		and (
				(
					(ph.startdatetime between @startsatefloor and dateadd(day, 1, @startdate))
					or 
					(ph.[enddatetime] between @startsatefloor and dateadd(day, 1, @startdate))
				)
				or
				(
					(@startdate between startdatetime and ph.[enddatetime]) and (@enddateceiling between  startdatetime and ph.[enddatetime])
				)
			)
	;with Dates_CTE	as 
	(
		select @startsatefloor as UtilizationDate
			,1 Cnt
		union all
		select dateadd(hh, 1, UtilizationDate)
			,Cnt + 1
		from Dates_CTE
		where Cnt < 24
	)
	,ResourceAndDate as 
	(
		select distinct vw.resourceid as ResourceId
			,vw.digitalworkername
			,UtilizationDate.UtilizationDate as UtilizationDate
		from #temp_dayDS vw
		join Dates_CTE UtilizationDate on 1 = 1
	)
	,main_ds as (
		select resourceid
			,[Time] = periodH
			,usage = sum(diff)
		from (
			select distinct resourceid
				,periodH
				,period1
				,period2
				,diff
			from (
				select *
					,diff = datediff(minute, case 
							when dt1 < period1
								then period1
							else dt1
							end, case 
							when dt2 > period2
								then period2
							else dt2
							end)
				from #temp_dayDS as t
				-- hour slice --
				join (
					values (0),(1),(2),(3),(4),(5),(6),(7),(8),(9),(10),(11),(12),(13),(14),(15),(16),(17),(18),(19),(20),(21),(22),(23)
					) hours(hh) on hh between datepart(hour, dt1)
						and datepart(hour, dt2)
				-- 5 minutes slice --
				join (
					values (0),(5),(10),(15),(20),(25),(30),(35),(40),(45),(50),(55)
					) minutes(mm) on t.dt2 between cast(convert(char(15), dt1, 126) + '0:00' as datetime)
						and cast(convert(char(16), dt2, 126) + ':00' as datetime)
				outer apply (
					select periodH = convert(char(11), dt1, 126) + RIGHT('0' + cast(hh as varchar), 2) + ':00:00'
						,--hour
						period1 = cast(convert(char(11), dt1, 126) + RIGHT('0' + cast(hh as varchar), 2) + ':' + RIGHT('0' + cast(mm as varchar), 2) + ':00' as datetime)
						,-- hours +MIn
						period2 = dateadd(minute, 5, cast(convert(char(11), dt1, 126) + RIGHT('0' + cast(hh as varchar), 2) + ':' + RIGHT('0' + cast(mm as varchar), 2) + ':00' as datetime))
						--(hours +MIn)+5 min sice
					) period
				) a
			where diff > 0
			) a
		group by resourceid
			,periodH
		)
	select rd.ResourceId
		,rd.digitalworkername
		,rd.UtilizationDate
		,case when coalesce(md.usage, 0)>=59 then 60 else coalesce(md.usage, 0) end as Usage
	from ResourceAndDate rd
	left join main_ds md on rd.ResourceId = md.resourceid
		and md.[Time] = rd.UtilizationDate
	order by rd.digitalworkername
		,rd.UtilizationDate;
	drop table #temp_dayDS;
end;

go

if (
		select object_id('BPDS_Get_Process_History_By_Date_Range_Query')
		) is not null
	drop proc BPDS_Get_Process_History_By_Date_Range_Query;

go

create proc [BPDS_Get_Process_History_By_Date_Range_Query] (
	@startdate datetime
	,@enddate datetime
	,@resourceid as GuidIdTableType readonly
	,@attributeid as IntIdTableType readonly
	)
as
begin
	declare @startdatefloor datetime
		,@enddateceiling datetime;

	set @startdatefloor = cast(floor(cast(@startdate as float)) as datetime);
	set @enddateceiling = dateadd(ms, - 3, dateadd(dd, 1, cast(floor(cast(@enddate as float)) as datetime)));

	declare @lresourcId as GuidIdTableType;
	declare @high int = (datediff(day, @startdatefloor, @enddateceiling) + 1) - 1;
	declare @low int = 0;

	if exists (
			select 1
			from @resourceid
			)
	begin
		insert into @lresourcId ([id])
		select ri.id
		from @resourceid as ri;
	end;
	else
	begin
		insert into @lresourcId ([id])
		select br.resourceid
		from BPAResource as br
		where br.[name] not like '%debug%';
	end;

	create table #temp_drange 
	(
		 dt1 datetime
		,dt2 datetime
	);

	declare @lstartdate datetime = @startdatefloor;
	declare @lenddateceiling datetime;

	while (@low <= @high)
	begin
		set @lstartdate = dateadd(day, @low, @startdatefloor);
		set @lenddateceiling=dateadd(ms, - 3, dateadd(dd, 1, cast(floor(cast(@lstartdate as float)) as datetime)));
		
		insert into #temp_drange
		select 
			cast(convert(char(16), case when ph.startdatetime < @lstartdate then @lstartdate
											else ph.startdatetime
									end, 126) + ':00' as datetime) dt1
			,cast(convert(char(16), case when ph.enddatetime > @lenddateceiling then @lenddateceiling
											else ph.enddatetime
									end, 126) + ':00' as datetime) dt2
			--,@lstartdate,@lenddateceiling,[ph].[startdatetime],ph.[enddatetime]
		from BPAResource as rs
		inner join BPASession ph on ph.runningresourceid = rs.resourceid
		where 
			(
				(
					(ph.startdatetime between @lstartdate	and dateadd(day, 1, @lstartdate))
					or 
					(ph.[enddatetime] between @lstartdate and dateadd(day, 1, @lstartdate))
				)
				or
				(
					(@lstartdate between startdatetime and ph.[enddatetime]) and (@lenddateceiling between  startdatetime and ph.[enddatetime])
				)
			)
			and exists 
			(
				select 1
				from @lresourcId as ri
				where ri.id = rs.resourceid
			)
			and not exists 
			(
				select 1
				from @attributeid a
				where rs.AttributeID = a.[id]
			)
		

		set @low = @low + 1;
	end;
	set @high = (datediff(day, @startdatefloor, @enddateceiling) + 1) * 24 - 1;
	set @low = 0;

	;with L0 as 
	(
		select c
		from (
				select 1
				union all
				select 1
			) as D(c)
	)
	,L1	as 
	(
		select 1 as c
		from L0 as A
		cross join L0 as B
	)
	,L2	as 
	(
		select 1 as c
		from L1 as A
		cross join L1 as B
	)
	,L3	as 
	(
		select 1 as c
		from L2 as A
		cross join L2 as B
	)
	,L4	as 
	(
		select 1 as c
		from L3 as A
		cross join L3 as B
	)
	,L5	as 
	(
		select 1 as c
		from L4 as A
		cross join L4 as B
	)
	,num as 
	(
		select row_number() over (order by (select null)) as rownum
		from L5
	)
	,Dates_CTE	as 
	(
		select top (@high - @low + 1) dateadd(hh, @low + rownum - 1, @startdatefloor) as Dates
		from num
		order by rownum
	)
	,main_ds as 
	(
		select [Time] = periodH
			,usage = sum(diff)
		from (
			select distinct periodH
				,period1
				,period2
				,diff
			from (
				select *
					,[diff] = datediff(minute, case 
							when dt1 < period1
								then period1
							else dt1
							end, case 
							when dt2 > period2
								then period2
							else dt2
							end)
				from #temp_drange as t
				-- hour slice --
				join (
					values (0),(1),(2),(3),(4),(5),(6),(7),(8),(9),(10),(11),(12),(13),(14),(15),(16),(17),(18),(19),(20),(21),(22),(23)
					) hours(hh) on hh between datepart(hour, dt1)
						and datepart(hour, dt2)
				-- 5 minutes slice --
				join (
					values (0),(5),(10),(15),(20),(25),(30),(35),(40),(45),(50),(55)
					) minutes(mm) on t.dt2 between cast(convert(char(15), dt1, 126) + '0:00' as datetime)
						and cast(convert(char(16), dt2, 126) + ':00' as datetime)
				outer apply (
					select periodH = convert(char(11), dt1, 126) + RIGHT('0' + cast(hh as varchar), 2) + ':00:00',--hour
						period1 = cast(convert(char(11), dt1, 126) + RIGHT('0' + cast(hh as varchar), 2) + ':' + right('0' + cast(mm as varchar), 2) + ':00' as datetime),-- hours + min
						period2 = dateadd(minute, 5, cast(convert(char(11), dt1, 126) + RIGHT('0' + cast(hh as varchar), 2) + ':' + right('0' + cast(mm as varchar), 2) + ':00' as datetime))--(hours + min)+5 min sice
					) periode
				) a
			where diff > 0
			) a
		group by periodH
		)
	select Dt.Dates
		,case 
			when coalesce(md.usage, 0) >= 59
				then 60
			else coalesce(md.usage, 0)
			end usage
	from Dates_CTE Dt
	left join main_ds md on Dt.Dates = md.[Time];

	drop table #temp_drange;
end;

go

if database_principal_id('bpa_ExecuteSP_System') is not null
begin
	grant execute
		on [BPDS_Get_Process_History_Per_Worker_Parameterised_Query]
		to [bpa_ExecuteSP_System]

	grant execute
		on [BPDS_Get_Process_History_By_Date_Range_Query]
		to [bpa_ExecuteSP_System]
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
	('439', 
	GETUTCDATE(), 
	'db_upgradeR439.sql', 
	'update BPDS_Get_Process_History_Per_Worker_Parameterised_Query and BPDS_Get_Process_History_By_Date_Range_Query stored procs',
	0
);
