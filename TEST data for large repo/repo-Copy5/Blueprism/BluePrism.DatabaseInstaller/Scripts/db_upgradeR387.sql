/*
SCRIPT		: 387
STORY		: bp-1445
PURPOSE		: Add a new locked column to BPAWorkQueueItemAggregate
AUTHOR		: Gareth Davidson
*/

alter table BPAWorkQueueItemAggregate
  add locked int not null default(0)
go


alter procedure [usp_UpdateWorkQueueItemAggregate] as begin declare @numOfActiveQueues int = (
    select
        count(id)
    from
        BPAWorkQueue
) declare @timestamp datetime = getutcdate()
set
    transaction isolation level snapshot if @numOfActiveQueues > 0 merge BPAWorkQueueItemAggregate wqia using (
        select
            queueitemstats.ident,
            queueitemstats.completed,
            queueitemstats.pending,
            queueitemstats.deferred,
            queueitemstats.exceptioned,
            isnull(queueavgstats.averageworkedtime, 0) as averageworkedtime,
            queueitemstats.total,
            queueitemstats.totalworktime,
            queueitemstats.locked
        from
            (
                select
                    wq.Ident as ident,
                    count(wqi.Completed) as completed,
                    count(
                        case
                            when wqi.ident is not null 
							and wqi.finished is null
                            and lock.id is null then 1
                            else null
                        end
                    ) as pending,
                    count(
                        case
                            when wqi.deferred is null
                            or wqi.deferred < getutcdate() then null
                            else 1
                        end
                    ) as deferred,
                    count(wqi.exception) as exceptioned,
                    count(wqi.ident) as total,
                    isnull(sum(convert(bigint, wqi.attemptworktime)), 0) as totalworktime,
                    count(lock.id) as locked
                from
                    BPAWorkQueue wq
                    left join BPAWorkQueueItem wqi on wq.Ident = wqi.queueident
                    left join BPACaseLock lock on wqi.Ident = lock.id
                group by
                    wq.Ident
            ) as queueitemstats
            left join (
                select
                    q.ident,
                    isnull(avg(cast(i.attemptworktime as float)), 0) as averageworkedtime
                from
                    BPAWorkQueue q
                    inner join BPAWorkQueueItem i on i.queueident = q.ident
                where
                    i.finished is not null
                group by
                    q.ident
            ) AS queueavgstats on queueitemstats.ident = queueavgstats.ident
    ) as items on (wqia.queueIdent = items.Ident)
    when matched then
update
set
    wqia.completed = items.completed,
    wqia.pending = items.pending,
    wqia.exceptioned = items.exceptioned,
    wqia.deferred = items.deferred,
    wqia.averageWorktime = items.averageworkedtime,
    wqia.total = items.total,
    wqia.totalWorkTime = items.totalworktime,
    wqia.dateUpdated = @timestamp,
	wqia.locked = items.locked
    when not matched by target then
insert
    (
        queueIdent,
        completed,
        pending,
        exceptioned,
        deferred,
        averageWorktime,
        total,
        totalWorkTime,
        dateUpdated,
		locked
    )
values
    (
        items.Ident,
        items.completed,
        items.pending,
        items.exceptioned,
        items.deferred,
        items.averageworkedtime,
        items.total,
        items.totalworktime,
        @timestamp,
		items.locked
    );

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
	'387'
	,getutcdate()
	,'db_upgradeR387.sql'
	,'Add new lock count column to BPAWorkQueueItemAggregate'
	,0
	);
go
