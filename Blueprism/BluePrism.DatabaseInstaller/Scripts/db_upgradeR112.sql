/*
SCRIPT         : 112
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Increased the permitted length of work queue item tags
*/


alter table BPATag
    alter column tag varchar(255) not null
GO

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '112',
  GETUTCDATE(),
  'db_upgradeR112.sql UTC',
  'Increased the permitted length of work queue item tags'
);

