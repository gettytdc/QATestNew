/*
SCRIPT         : 84
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : GMB
PURPOSE        : Changes to support storing credential key in the DB
*/

ALTER TABLE BPASysConfig ADD credentialkey varchar(50);
GO

--set DB version
INSERT INTO BPADBVersion VALUES (
  '84',
  GETUTCDATE(),
  'db_upgradeR84.sql UTC',
  'Changes to support storing credential key in the DB'
)
GO

