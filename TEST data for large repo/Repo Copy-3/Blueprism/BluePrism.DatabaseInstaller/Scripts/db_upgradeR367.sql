/*
SCRIPT         : 367
PURPOSE        : Change usp_getitembyid to return error codes if the item cannot be found
*/

alter procedure [usp_getitembyid]
    @queueName nvarchar(255),
    @sessionId uniqueidentifier,
    @workQueueItemId uniqueidentifier
as
begin
    declare @lockId uniqueidentifier = newid();
    declare @lock table (Id int);

    set transaction isolation level read uncommitted;

	if object_id ('#tmpgetitem') IS NOT NULL
	drop table #tmpgetitem

	create table #tmpgetitem (ident bigint, currentdate datetime, sessionid uniqueidentifier, lockid uniqueidentifier, finished datetime, deferred datetime, id bigint);

	insert into #tmpgetitem
    select
        i.[ident],
        getutcdate(),
        @sessionId,
        @lockId,
		finished,
		deferred,
		l.id
        from BPAWorkQueueItem i
            join BPAWorkQueue q on i.[queueident] = q.[ident]
            left join BPACaseLock l on l.[id] = i.[ident]
        where
            i.[id] = @workQueueItemId
            and q.[name] = @queueName
            and q.[running] = 1;

	insert into BPACaseLock ([id],[locktime],[sessionid],[lockid])
	output inserted.[id] into @lock
	select ident, currentdate, sessionid, lockid from #tmpgetitem where
			[finished] is null and ([deferred] is null or [deferred] < getutcdate()) /* ie. pending */
            and [id] is null; /* ie. and not locked... */

    if exists(select 1 from @lock)
    begin
        select i.[encryptid], i.[id], i.[ident], i.[keyvalue], i.[data], i.[status], i.[attempt]
        from BPAWorkQueueItem i
            join BPACaseLock l on i.[ident] = l.[id]
        where l.[lockid] = @lockId;
    end
	else
	begin
		
		if not exists (select 1 from #tmpgetitem)
		begin
			;throw 51001, 'Work queue item cannot be found',1
		end

		if exists (select 1 from #tmpgetitem where [id] is not null)
		begin
			;throw 51002, 'Work queue item is locked',1
		end
		
		if exists (select 1 from #tmpgetitem where ([deferred] is not null and [deferred] >= getutcdate()))
		begin
			;throw 51003, 'Work queue item is deferred',1
		end

		if exists (select 1 from #tmpgetitem where [finished] is not null)
		begin
			;throw 51004, 'Work queue item is not in an active state',1
		end
	end
end
go


-- Set DB version.
insert into BPADBVersion (
	dbversion
	,scriptrundate
	,scriptname
	,[description]
	,timezoneoffset
	)
values (
	'367'
	,getutcdate()
	,'db_upgradeR367.sql'
	,'Change usp_getitembyid to return error codes if the item cannot be found'
	,0
	);