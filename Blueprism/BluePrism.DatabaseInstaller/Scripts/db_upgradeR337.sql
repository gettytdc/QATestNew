/*
SCRIPT         : 337
AUTHOR         : Giles Bathgate
PURPOSE        : Adding usp_getitembyid stored procedure
*/

drop index INDEX_WorkQueueItemGuid on BPAWorkQueueItem;

create nonclustered index INDEX_WorkQueueItemGuid on BPAWorkQueueItem([id])
include([finished],[deferred],[ident],[queueident])
with (pad_index = off,
      statistics_norecompute = off,
      sort_in_tempdb = off,
      online = off,
      allow_row_locks = on,
      allow_page_locks = on);
GO

create procedure [usp_getitembyid]
    @queueName nvarchar(255),
    @sessionId uniqueidentifier,
    @workQueueItemId uniqueidentifier
as
begin
    declare @lockId uniqueidentifier = newid();
    declare @lock table (Id int);

    set transaction isolation level read uncommitted;
    insert into BPACaseLock ([id],[locktime],[sessionid],[lockid])
    output inserted.[id] into @lock
    select
        i.[ident],
        getutcdate(),
        @sessionId,
        @lockId
        from BPAWorkQueueItem i
            join BPAWorkQueue q on i.[queueident] = q.[ident]
            left join BPACaseLock l on l.[id] = i.[ident]
        where
            i.[id] = @workQueueItemId
            and q.[name] = @queueName
            and q.[running] = 1
            and i.[finished] is null and (i.[deferred] is null or i.[deferred] < getutcdate()) /* ie. pending */
            and l.[id] is null; /* ie. and not locked... */

    if exists(select 1 from @lock)
    begin
        select i.[encryptid], i.[id], i.[ident], i.[keyvalue], i.[data], i.[status], i.[attempt]
        from BPAWorkQueueItem i
            join BPACaseLock l on i.[ident] = l.[id]
        where l.[lockid] = @lockId;
    end
end
GO

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '337',
  GETUTCDATE(),
  'db_upgradeR337.sql',
  'Adding usp_getitembyid stored procedure',0
);