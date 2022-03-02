/*
SCRIPT         : 83
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : CG
PURPOSE        : Add priority field to BPAWOrkQueueItem

*/

-- Add the field
alter table BPAWorkQueueItem add priority int not null default 0;
go

-- Set default on any existing items
update BPAWorkQueueItem set priority=0
go

-- Create index because we will sort by priorty
create nonclustered index INDEX_WorkQueueItemPriority on BPAWorkQueueItem(priority)
go

--set DB version
INSERT INTO BPADBVersion VALUES (
  '83',
  GETUTCDATE(),
  'db_upgradeR83.sql UTC',
  'Add priority field to BPAWOrkQueueItem'
)
GO
