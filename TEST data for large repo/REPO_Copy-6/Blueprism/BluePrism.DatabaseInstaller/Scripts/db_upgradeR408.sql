/*
SCRIPT         : 408
AUTHOR         : Luke Dawkin
PURPOSE        : Update the UpdateWorkQueueItemAggregate Stored Procedure.
*/

alter procedure usp_UpdateWorkQueueItemAggregate as 
begin 
	declare @timestamp datetime = getutcdate()
	set transaction isolation level snapshot 
	
	insert into BPAWorkQueueItemAggregate
	(
	    queueIdent,
	    completed,
	    pending,
	    exceptioned,
	    deferred,
	    averageWorktime,
	    total,
	    totalWorkTime,
	    dateUpdated
	)
	select
		wq.ident,
	    0,				-- completed - int
	    0,				-- pending - int
	    0,				-- exceptioned - int
	    0,				-- deferred - int
	    0,				-- averageWorktime - int
	    0,				-- total - int
	    0,				-- totalWorkTime - int
	    @timestamp		-- dateUpdated - datetime
	from BPAWorkQueue as wq
	where not exists(
		select 1
		from BPAWorkQueueItemAggregate as wqia
		where wq.ident = wqia.queueIdent
	);
	;WITH lockedWQI AS
        (
			SELECT [bwqi].[queueident], COUNT(1) locked
			FROM  [dbo].[BPAWorkQueueItem] AS [bwqi] WITH(NOLOCK)
			WHERE EXISTS(SELECT 1 FROM [dbo].[BPACaseLock] AS [bcl] WHERE [bwqi].[ident]=[bcl].id)
			GROUP BY [bwqi].[queueident]
		)
		,WQIAgg AS
        (
			SELECT
				wqi.queueident
				,count(wqi.completed) as completed
				,COUNT(1) TotalWQI
				,COUNT(CASE WHEN wqi.finished is NOT NULL THEN 1 END) Totalfinished
				,count( CASE when wqi.deferred > getutcdate()	then 1 end ) as DEFERRED
				,COUNT(wqi.exception) as exceptioned
				,isnull(avg( CASE WHEN wqi.finished is not null	then cast(wqi.attemptworktime as float)	end), 0) as averageworktime
				,isnull(sum(convert(bigint, wqi.attemptworktime)), 0) as totalworktime
			FROM BPAWorkQueueItem wqi WITH(NOLOCK)
			where EXISTS
			(
				select 1 from BPAWorkQueue as wq
				where wqi.queueident = wq.ident
			)
			GROUP BY wqi.queueident
		)
		
		​update wqia
		set
			wqia.completed		=	items.completed,
			wqia.pending		=	items.pending,
			wqia.exceptioned	=	items.exceptioned,
			wqia.deferred		=	items.deferred,
			wqia.averageWorktime=	items.averageworktime,
			wqia.total			=	items.total,
			wqia.totalWorkTime  =	items.totalworktime,
			wqia.dateUpdated	=	getutcdate(),
			wqia.locked			=	items.locked
		from BPAWorkQueueItemAggregate as wqia
		inner join 
		(
			SELECT
			 [qag].[queueident]
			,[qag].completed
			,COALESCE( [qag].TotalWQI- ( [qag].Totalfinished+ISNULL(lwqi.[locked],0)),0) AS Pending
			,[qag].DEFERRED
			,[qag].exceptioned
			,[qag].averageworktime
			,[qag].TotalWQI total
			,[qag].totalworktime
			,COALESCE(lwqi.[locked],0)[locked]
		FROM WQIAgg qag
		left JOIN lockedWQI lwqi ON [qag].[queueident]=lwqi.[queueident]
		) as items on wqia.queueIdent = items.[queueident]
end
go

insert into BPADBVersion
(
    dbversion,
    scriptrundate,
    scriptname,
    [description],
    timezoneoffset
)
values
(
    '408',
    getutcdate(),
    'db_upgradeR408.sql',
    'Performance improvements to stored procedure usp_UpdateWorkQueueItemAggregate',
    0
);
