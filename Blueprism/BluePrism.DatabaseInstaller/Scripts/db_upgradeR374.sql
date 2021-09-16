/*
SCRIPT        : 374
PURPOSE       : Add active queue stats MI table and associated stored procedure for population
AUTHOR		  : Gary Chadwick
*/

if object_id('BPAWorkQueueItemAggregate') is null
begin
create table BPAWorkQueueItemAggregate
(
    queueIdent int primary key constraint FK_BPAWorkQueueItemAggregate_BPAWorkQueue foreign key references BPAWorkQueue(Ident) ON DELETE CASCADE,
    completed int not null default(0),
    pending int not null default(0),
    exceptioned int not null default(0),
    deferred int not null default(0),
    averageWorktime int not null default(0),
    total int not null default(0),
    totalWorkTime int not null default(0),
    dateUpdated datetime not null default(getutcdate())
);
end
go

if exists (select * from sys.indexes where name='index_BPAWorkQueueItem_finished')
begin
DROP INDEX BPAWorkQueueItem.index_BPAWorkQueueItem_finished
end
go

create nonclustered index index_BPAWorkQueueItem_finished on BPAWorkQueueItem
(
    [finished] asc
)
include([queueident],[attemptworktime])
with (pad_index = off,
      statistics_norecompute = off,
      sort_in_tempdb = off,
      allow_row_locks = on,
      allow_page_locks = on)
      on [primary]

if exists (select 1 from sysobjects 
    where id = object_id('usp_UpdateWorkQueueItemAggregate') and type='P')
begin
    drop procedure usp_UpdateWorkQueueItemAggregate
end
go

create procedure [usp_UpdateWorkQueueItemAggregate] 
as
begin

declare @numOfActiveQueues INT = (select count(id) from BPAWorkQueue)
declare @timestamp DATETIME = getutcdate()

set transaction isolation level snapshot

if @numOfActiveQueues > 0
        
    merge BPAWorkQueueItemAggregate wqia
	using (select queueitemstats.ident, queueitemstats.completed, queueitemstats.pending, queueitemstats.deferred, queueitemstats.exceptioned, isnull(queueavgstats.averageworkedtime, 0) as averageworkedtime, queueitemstats.total, queueitemstats.totalworktime from (select wq.Ident as ident, 
			    count(wqi.Completed) as completed, 
			    count(wqi.ident)-count(wqi.finished) as pending,
                count(case when wqi.deferred is null or wqi.deferred < getutcdate() then null else 1 end) as deferred,
                count(wqi.exception) as exceptioned,
                count(wqi.ident) as total,
				isnull(sum(convert(bigint,wqi.attemptworktime)),0) as totalworktime
	    from BPAWorkQueue wq
	    left join BPAWorkQueueItem wqi ON wq.Ident = wqi.queueident
	    group by wq.Ident) as queueitemstats
		left join (
					select
					 q.ident,
					 isnull(avg(cast(i.attemptworktime as float)), 0) as averageworkedtime
					 from BPAWorkQueue q
					 inner join BPAWorkQueueItem i on i.queueident = q.ident
				 where i.finished is not null
				 group by q.ident)
		 AS queueavgstats ON queueitemstats.ident = queueavgstats.ident) as items
		on (wqia.queueIdent = items.Ident)
	    when MATCHED
		    then update set
		    wqia.completed = items.completed,
		    wqia.pending = items.pending,
		    wqia.exceptioned = items.exceptioned,
		    wqia.deferred = items.deferred,
		    wqia.averageWorktime = items.averageworkedtime,            
			wqia.total = items.total,
			wqia.totalWorkTime = items.totalworktime,
		    wqia.dateUpdated = @timestamp
	    when not matched by target
		    then insert (queueIdent, completed, pending, exceptioned, deferred, averageWorktime, total, totalWorkTime, dateUpdated)
		    values (items.Ident, items.completed, items.pending, items.exceptioned, items.deferred, items.averageworkedtime, items.total, items.totalworktime, @timestamp);
end
go
-- set db version.
insert into bpadbversion (
	dbversion
	,scriptrundate
	,scriptname
	,[description]
	,timezoneoffset
	)
values (
	'374'
	,getutcdate()
	,'db_upgradeR374.sql'
	,'Add active queue stats MI table and associated stored procedure for population'
	,0
	);
