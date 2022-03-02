/*
SCRIPT         : 85
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Add tagging infrastructure to work queue items.
*/

-- create the tag table
create table BPAWorkQueueItemTag (
    queueitemident bigint not null 
        constraint FK_BPAWorkQueueItemTag_BPAWorkQueueItem 
            foreign key references BPAWorkQueueItem (ident)
            on delete cascade,
    tag varchar(64) not null,
    primary key (queueitemident, tag)
)

-- index on the tag too
create index INDEX_WorkQueueItemTag_tag on BPAWorkQueueItemTag (tag)

--set DB version
INSERT INTO BPADBVersion VALUES (
  '85',
  GETUTCDATE(),
  'db_upgradeR85.sql UTC',
  'Add tagging infrastructure to work queue items'
)
GO
