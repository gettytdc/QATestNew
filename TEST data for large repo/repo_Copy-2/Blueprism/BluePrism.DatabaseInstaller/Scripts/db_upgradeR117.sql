/*
SCRIPT         : 117
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Extend WorkQueueItem.status to 255 chars.
*/

alter table BPAWorkQueueItem
    alter column status varchar(255) null;
go

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '117',
  GETUTCDATE(),
  'db_upgradeR117.sql UTC',
  'Extend WorkQueueItem.status to 255 chars.'
);

