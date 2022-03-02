/*
SCRIPT         : 86
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Reworking of the tagging infrastructure
*/

-- Basically what we're doing here is modelling the current
-- many to many (queueitem* <=> tag*) relationship with a
-- linking table - upgrade 85 meant that to get distinct
-- tags, you'd have to do a full index search on all the 
-- tags on all the queue items. This way, the number of
-- records to search is vastly reduced, at the price of a
-- little more complexity in the insertion / searching
-- processes (effectively inserting / searching on one more
-- table than the previous way of doing it).

-- create the, er, (more generic) tag table
create table BPATag (
    id int not null identity primary key,
    tag varchar(64) not null
);

-- Copy over the tags from the BPAWorkQueueItemTag table
insert into BPATag (tag)
select tag from BPAWorkQueueItemTag order by queueitemident;

-- disable the constraints (briefly) for the item/tag linking table
alter table BPAWorkQueueItemTag nocheck constraint all

-- add the link to BPATag on BPAWorkQueueItemTag (allow NULLs for now)
alter table BPAWorkQueueItemTag
    add tagid int
        constraint FK_BPAWorkQueueItemTag_BPATag
            foreign key references BPATag (id)
            on delete cascade;
GO

-- Get the correct tag IDs for the corresponding tags.
update it set it.tagid = t.id 
    from BPAWorkQueueItemTag as it
        inner join BPATag as t on it.tag = t.tag;

-- And add a NOT NULL constraint onto the column
alter table BPAWorkQueueItemTag
    alter column tagid int not null;

-- re-enable the constraints
alter table BPAWorkQueueItemTag check constraint all;

-- remove the index on the tag column
drop index INDEX_WorkQueueItemTag_tag on BPAWorkQueueItemTag;

-- Find the primary key and drop it (???)
declare @pkname varchar(64);
select @pkname = name
    from sysobjects
    where xtype = 'PK'
        and parent_obj = (object_id('BPAWorkQueueItemTag'));
exec ('alter table BPAWorkQueueItemTag drop constraint ['+@pkname+']');

-- And add in the new primary key (clustered again)
alter table BPAWorkQueueItemTag
    add constraint PK_BPAWorkQueueItemTag 
        primary key clustered (queueitemident, tagid);

-- and finally remove the tag column from the linking table
alter table BPAWorkQueueItemTag
    drop column tag;

--set DB version
INSERT INTO BPADBVersion VALUES (
  '86',
  GETUTCDATE(),
  'db_upgradeR86.sql UTC',
  'Rework of the tagging infrastructure'
)
GO
