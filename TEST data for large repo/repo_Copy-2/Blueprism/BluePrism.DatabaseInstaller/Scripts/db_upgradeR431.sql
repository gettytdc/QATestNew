/*
SCRIPT         : 431
AUTHOR         : Paul Cooke
PURPOSE        : Add BPDS_Get_Process_History_Per_Worker_Parameterised_Query stored procedure
*/

if not exists (
		select *
		from sys.objects
		where type = 'P' and name = 'BPDS_Get_Process_History_Per_Worker_Parameterised_Query'
		)
begin
	exec (
			'create PROC BPDS_Get_Process_History_Per_Worker_Parameterised_Query
(
	@StartDate DATETIME,
	@resourceId as GuidIdTableType READONLY	
)
AS
BEGIN
	DECLARE
	@StartDateFloor datetime,
	@EndDateCeiling datetime
	set @StartDateFloor = CAST(floor(cast(@StartDate as float)) as datetime)
	set @EndDateCeiling = DATEADD(ms, -3, DATEADD(dd, 1, CAST(floor(cast(@StartDate as float)) as datetime)))


	DECLARE @lresourcId as GuidIdTableType

	IF EXISTS(SELECT 1 FROM @resourceId)
	BEGIN	
		INSERT INTO @lresourcId([id])
		SELECT id FROM @resourceId AS [ri]
	END
	ELSE
	BEGIN
		INSERT INTO @lresourcId([id])
		SELECT br.[resourceid] FROM [dbo].[BPAResource] AS [br]
		WHERE br.[name] NOT LIKE ''%debug%''
    end
		

	select distinct rs.[resourceid], rs.name VirtualWorkerName, cast(convert(char(16), startdatetime,126)+'':00'' as datetime)dt1, cast(convert(char(16), enddatetime,126)  +'':00'' as datetime)dt2
	INTO #temp
	FROM [dbo].[BPAResource] AS [rs] 
			inner JOIN [dbo].[BPASession] ph 	ON ph.runningresourceid = rs.resourceid
	where ph.startdatetime > @StartDateFloor AND  ph.[enddatetime] <DATEADD(DAY,1,@StartDate) 
	AND EXISTS(SELECT 1 FROM @lresourcId AS [ri] WHERE ri.id=rs.[resourceid])
	order by resourceid, cast(convert(char(16), startdatetime,126)+'':00'' as datetime), cast(convert(char(16), enddatetime,126)  +'':00'' as datetime)


	;WITH Dates_CTE AS 
	(
		SELECT @StartDateFloor AS Dates, 1 Cnt
		UNION ALL
		SELECT Dateadd(hh, 1, Dates), cnt+1
		FROM   Dates_CTE
		WHERE  Cnt < 24
	),
	ResourceAndDate AS
	(
		SELECT DISTINCT
		vw.resourceid ResourceId,
		VirtualWorkerName ,
		dates.Dates Dates
		FROM
		#temp vw
		JOIN
		Dates_CTE dates
		ON 1 = 1
	),
	main_ds as
	(
	select resourceid, [Time] = periodeH, [usage] = SUM(diff)
	from(
			select distinct resourceid,periodeH,periode1,periode2,diff
			from(
					select *,[diff]=DATEDIFF(MINUTE
											,case when dt1 < periode1 then periode1 else dt1 end
											,case when dt2 > periode2 then periode2 else dt2 end
										 )

					from [#temp] AS [t]
						-- hour slice --
					join (values(1),(2),(3),(4),(5),(6),(7),(8),(9),(10),(11),(12),(13),(14),(15),(16),(17),(18),(19),(20),(21),(22),(23)
						 )hours(hh) on hh between datepart(HOUR,dt1) and  datepart(HOUR,dt2)
						-- 5 minutes slice --
					join (values(0),(5),(10),(15),(20),(25),(30),(35),(40),(45),(50),(55)
						 )minutes(mm) on  t.dt2 between cast(convert(char(15),dt1,126)+''0:00'' as datetime) and cast(convert(char(16),dt2,126)+'':00'' as datetime)
					outer apply(select 
									 [periodeH] = convert(char(11), dt1 ,126)+RIGHT(''0''+cast(hh as varchar),2)+'':00:00'' --Hour
									,[periode1] = cast(convert(char(11), dt1,126)+RIGHT(''0''+cast(hh as varchar),2)+'':''+RIGHT(''0''+cast(mm as varchar),2)+'':00'' as datetime)-- Hours +MIn
									,[periode2] = dateadd(minute,5,cast(convert(char(11), dt1 ,126)+RIGHT(''0''+cast(hh as varchar),2)+'':''+RIGHT(''0''+cast(mm as varchar),2)+'':00'' as datetime))--(Hours +MIn)+5 min sice
								)periode
				)a
			where diff>0
		)a group by resourceid,periodeH
	)
	SELECT  rd.ResourceId,rd.VirtualWorkerName, rd.Dates,COALESCE( md.[usage],0) Usage
	FROM ResourceAndDate rd
		LEFT JOIN main_ds md ON rd.resourceid= md.resourceid AND md.[Time]=rd.Dates

END'
			)
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
('431', 
 GETUTCDATE(), 
 'db_upgradeR431.sql', 
 'Add BPDS_Get_Process_History_Per_Worker_Parameterised_Query stored procedure', 
 0
);
