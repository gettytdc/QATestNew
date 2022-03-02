/*
SCRIPT         : 152
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Adds further indexes to queue / tag tables
*/

-- This script would have been R164 but for the fact that it needed to
-- be ported to v4.1 as well, which was currently up to R151.
-- The original R152 has been merged into R153.
-- This work was done under bug 6391

-- Index missed because I thought that a foreign key on the
-- column would implicitly create an index
if not exists (
    select *
    from sys.indexes
    where name = 'Index_BPAWorkQueueItemTag_queueitemident'
  )
  create index Index_BPAWorkQueueItemTag_queueitemident
    on BPAWorkQueueItemTag(queueitemident)

-- Index making the join which isolates the latest attempt of an item
-- from its prior attempts operate a bit faster
if not exists (
    select *
    from sys.indexes
    where name = 'Index_BPAWorkQueueItem_itemid_attempt'
  )
  create index Index_BPAWorkQueueItem_itemid_attempt
    on BPAWorkQueueItem(id, attempt)

--set DB version
insert into BPADBVersion values (
  '152',
  GETUTCDATE(),
  'db_upgradeR152.sql UTC',
  'Adds further indexes to queue / tag tables'
);
