/*
SCRIPT         : 126
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Some support for increasing performance in tags
*/

-- Unique index on BPATag.
-- Largely makes sure that a search for a tag is not occurring on
-- the entire table. Also makes row locks more likely as opposed
-- to table or page locks
create unique index UNQ_BPATag_tag on BPATag(tag)
go

-- Ignore (rather than fail on) duplicate keys in the queue item/tag
-- linking table. This means the code doesn't need to perform the
-- "if not exists" check before inserting a record. It does mean a
-- slight slowdown when the link already exists, but that is the
-- exception case. Usually, the link will be created for the first
-- time by the code rather than overwritten.
alter table BPAWorkQueueItemTag drop constraint PK_BPAWorkQueueItemTag;
go

-- You can't put the 'ignore_dup_key' setting into an ALTER TABLE
-- query in SQL Server 2000, so we'll create our PK this way.
create unique clustered index PK_BPAWorkQueueItemTag
    on BPAWorkQueueItemTag (queueitemident,tagid)
    with (ignore_dup_key = on);
go

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '126',
  GETUTCDATE(),
  'db_upgradeR126.sql UTC',
  'Some support for increasing performance in tags'
);
