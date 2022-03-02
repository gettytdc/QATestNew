/*
SCRIPT         : 80
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Replace the GUID key with an IDENTITY key on WorkQueue and WorkQueueItem
*/

-- drop the FK from Item -> Queue
alter table BPAWorkQueueItem drop constraint FK_BPAWorkQueueItem_BPAWorkQueue;

-- drop the PK on Queue
alter table BPAWorkQueue drop constraint PK_BPAWorkQueue;

-- add the new id column
-- Just an int will do for work queues - they're not that high turnover
alter table BPAWorkQueue add ident int identity(1,1) not null;

-- and make it the primary key
alter table BPAWorkQueue add constraint PK_BPAWorkQueue primary key (ident);

-- add a corresponding field in Item for its foreign key
alter table BPAWorkQueueItem add queueident int not null default 0;

-- Apparently, the alter table happens too fast, and the subsequent update fails 
-- because the 'ident' fields don't exist.
-- A 'go' in the middle shouldn't be necessary (the semi-colons act as batch 
-- separators), but it looks like it's the only way to get this to work.
go

-- get the data in there
update i set i.queueident = q.ident
    from BPAWorkQueueItem as i
        inner join BPAWorkQueue as q on i.queueid = q.id;

-- and apply it as a foreign key constraint
alter table BPAWorkQueueItem 
    add constraint FK_BPAWorkQueueItem_BPAWorkQueue
        foreign key (queueident)
        references BPAWorkQueue (ident);

-- That's WorkQueue sorted, now we need to do the same to Item
-- whole lot simpler since there are no FK dependencies on Item
-- First, drop the PK on the GUID
alter table BPAWorkQueueItem drop constraint PK_BPAWorkQueueItem;
-- bigint the work queue item ID - they can build up quite fast.
alter table BPAWorkQueueItem add ident bigint identity(1,1) not null;
-- Set the ID as the primary key
alter table BPAWorkQueueItem add constraint PK_BPAWorkQueueItem primary key (ident); 

GO

-- Now some indexes - the GUIDs are still used throughout to access
-- these tables, so index those.
create nonclustered index INDEX_WorkQueueGuid on BPAWorkQueue(id);
create nonclustered index INDEX_WorkQueueItemGuid on BPAWorkQueueItem(id)

--set DB version
INSERT INTO BPADBVersion VALUES (
  '80',
  GETUTCDATE(),
  'db_upgradeR80.sql UTC',
  'Updated WorkQueue and WorkQueueItem tables to use an IDENTITY PK as well as a GUID'
)
GO
