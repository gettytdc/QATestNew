/*
SCRIPT         : 74
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : CEG
PURPOSE        : Change status description for 'Failed' to 'Exception'
*/

UPDATE BPAStatus set description='Exception' where description='Failed';

--set DB version
INSERT INTO BPADBVersion VALUES (
  '74',
  GETUTCDATE(),
  'db_upgradeR74.sql UTC',
  'Change status description for Failed to Exception'
)
GO

