/*
SCRIPT         : 79
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : CEG
PURPOSE        : Resolve web service configuration entries with no timeout defined, see bug #4348
*/

UPDATE BPAWebService SET timeout=10000 where timeout is NULL

GO

--set DB version
INSERT INTO BPADBVersion VALUES (
  '79',
  GETUTCDATE(),
  'db_upgradeR79.sql UTC',
  'Resolve web service configuration entries with no timeout defined'
)
GO
