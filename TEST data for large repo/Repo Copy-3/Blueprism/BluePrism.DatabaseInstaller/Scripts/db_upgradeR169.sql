/*
SCRIPT         : 169
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Many performance improvements aimed at easing long queries
*/

-- ===================================================
-- = BPPTag
-- ===================================================

-- Drop the UNIQUE index on the tag
if exists (
    select * from sys.indexes
    where object_id = object_id('BPATag')
        and name = N'UNQ_BPATag_tag'
    )
    drop index UNQ_BPATag_tag on BPATag;

-- and make it nvarchar
alter table BPATag
 alter column tag nvarchar(255) not null

-- and recreate the index
create unique index UNQ_BPATag_tag on BPATag(tag);

-- ===================================================
-- = BPAWorkQueue
-- ===================================================

-- Drop the unique index on BPAWorkQueue
if exists (
    select * from sys.indexes
    where object_id = object_id('BPAWorkQueue')
        and name = N'Index_name'
    )
alter table BPAWorkQueue
    drop constraint Index_name;

-- We need to get the name of the other awkward anonymously created UNIQUE
-- index from BPAWorkQueue (see bug 8466) so that we can drop it
declare @unq nvarchar(255);
set @unq = (
  select i.name
    from sys.indexes i
    where i.is_primary_key = 0 and i.is_unique = 1
      and object_name(i.object_id) = 'BPAWorkQueue'
);

-- If we found the name, drop the constraint
if @unq is not null begin
  set @unq = 'alter table BPAWorkQueue drop constraint [' + @unq + ']';
  exec(@unq);
end;

-- And change the work queue name datatype
alter table BPAWorkQueue
    alter column name nvarchar(255);

-- re-add... well, just one of the constraints. The other is pointless
alter table BPAWorkQueue
    add constraint Index_name unique nonclustered (name);

-- ===================================================
-- = BPAWorkQueueItem
-- ===================================================

-- Remove the indexes, on exceptionreasonvarchar
if exists (
    select * from sys.indexes
    where object_id = object_id('BPAWorkQueueItem')
        and name = N'INDEX_BPAWorkQueueItem_exceptionreasonvarchar'
    )
    drop index INDEX_BPAWorkQueueItem_exceptionreasonvarchar on BPAWorkQueueItem;

-- and on keyvalue
if exists (
    select * from sys.indexes
    where object_id = object_id('BPAWorkQueueItem')
        and name = N'Index_BPAWorkQueueItem_key'
    )
    drop index Index_BPAWorkQueueItem_key on BPAWorkQueueItem;

-- Drop the big compound key
if exists (
  select * from sys.indexes
    where
      object_id = object_id(N'BPAWorkQueueItem')
      and name = N'Index_BPAWorkQueueItem_priorityloadedlockedfinished'
  )
  drop index Index_BPAWorkQueueItem_priorityloadedlockedfinished on BPAWorkQueueItem;

-- Make the change; for computed columns we must drop and re-add rather than altering
alter table BPAWorkQueueItem
  drop column exceptionreasonvarchar;

alter table BPAWorkQueueItem
  add exceptionreasonvarchar as cast(exceptionreason as nvarchar(400));

-- The other column can be altered in place
alter table BPAWorkQueueItem
  alter column keyvalue nvarchar(255) not null

-- Re-create the indexes that we're sure of
create index Index_BPAWorkQueueItem_exceptionreasonvarchar
  on BPAWorkQueueItem(exceptionreasonvarchar)

create index Index_BPAWorkQueueItem_key
  on BPAWorkQueueItem(keyvalue)

-- Big compound index attempt number 3:
create index Index_BPAWorkQueueItem_queuepriorityloaded
  on BPAWorkQueueItem (queueident, priority,loaded);

-- Add a new computed column to work queue item which represents the virtual tag
-- meaning that the computation (casting/replacing) is done at write time, not
-- when the tags are being searched later
alter table BPAWorkQueueItem
  add exceptionreasontag as cast(N'Exception: ' + replace(convert(nvarchar(400),exceptionreason), N';', N':') as nvarchar(415)) persisted

-- Add an index so that it can be searched timeously
create index Index_BPAWorkQueueItem_exceptionreasontag
  on BPAWorkQueueItem(exceptionreasontag)

-- ===================================================
-- = BPAWorkQueueItemTag / BPVWorkQueueItemTag
-- ===================================================

-- Drop the index that might exist if the user has added it (bug 8512)
if exists (
    select * from sys.indexes
    where object_id = object_id('BPAWorkQueueItemTag')
        and name = N'Index_Temp_BPAWorkQueueItemTag_tagid'
    )
    drop index Index_Temp_BPAWorkQueueItemTag_tagid on BPAWorkQueueItemTag;

-- There's an index which starts with queueitemident, and an index on queueitemident,
-- but no way to get straight to the records matching a specific tag. Since that
-- is the usual way that this table will be approached, it makes more sense to
-- swap the order of the index around (leaving the queueitemident index in place)
drop index PK_BPAWorkQueueItemTag on BPAWorkQueueItemTag;

-- It wasn't a PK before, but a unique clustered index, so... a PK with a slightly
-- different syntax requirement. Might as well make it a PK so the name is not misleading.
alter table BPAWorkQueueItemTag
  add constraint PK_BPAWorkQueueItemTag
    primary key clustered (tagid, queueitemident)
    with (ignore_dup_key = on);

-- Need to end this batch here, in order to make the ALTER VIEW the first statement in a batch
GO

-- Alter the tag view to use the new column - no need for item-centric modifications
-- within the view now, that's all there in the computed column
alter view BPViewWorkQueueItemTag (queueitemident, tag)
as
    select it.queueitemident, t.tag
    from BPAWorkQueueItemTag it
        join BPATag t on it.tagid = t.id
union
    select i.ident, i.exceptionreasontag
    from BPAWorkQueueItem i
    where i.exception is not null;
GO

-- A 'bare' tag view which provides the same join as BPViewWorkQueueItemTag but
-- without including the the virtual tags - ie. not including the exception reason tag
create view BPViewWorkQueueItemTagBare (queueitemident, tag)
as
    select it.queueitemident, t.tag
    from BPAWorkQueueItemTag it
        join BPATag t on it.tagid = t.id;
GO

-- ===================================================
-- = BPAEnvLock
-- ===================================================

-- Change the env lock name to nvarchar; we need to drop move
-- the table because Azure won't let us drop the PK, which is
-- based on the column we're altering.
create table BPAEnvLock_new (
    name nvarchar(255) not null
        constraint PK_BPAEnvLock_new primary key,
    token varchar(255) null,
    sessionid uniqueidentifier null
        constraint FK_BPAEnvLock_BPASession_new
        foreign key references BPASession(sessionid),
    locktime datetime null,
    comments varchar(1024) null
);
insert into BPAEnvLock_new (name, token, sessionid, locktime, comments) 
    select name, token, sessionid, locktime, comments from BPAEnvLock;
drop table BPAEnvLock;
exec sp_rename 'BPAEnvLock_new', 'BPAEnvLock';
exec sp_rename 'PK_BPAEnvLock_new', 'PK_BPAEnvLock';
exec sp_rename 'FK_BPAEnvLock_BPASession_new', 'FK_BPAEnvLock_BPASession';

-- ===================================================
-- = BPAAlertEvent
-- ===================================================

-- Add a compound index on userid and date for alerts
create index Index_BPAAlertEvent_subscriberuserid_subscriberdate
    on BPAAlertEvent (subscriberuserid, subscriberdate);

--set DB version
INSERT INTO BPADBVersion VALUES (
  '169',
  GETUTCDATE(),
  'db_upgradeR169.sql UTC',
  'Various changes aimed at improving performance of long queries'
)
