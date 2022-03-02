/*
SCRIPT         : 50
PROJECT NAME   : Automate
DATABASE NAME  : BPA
CREATION DATE  : 03 Jan 2007
AUTHOR         : JC
PURPOSE        : Add new process type field to process back up table
NOTES          : 
*/

ALTER TABLE BPAProcessBackup ADD processtype VARCHAR(1) NULL

GO

--set DB version
INSERT INTO BPADBVersion VALUES (
  '50',
  GETUTCDATE(),
  'db_upgradeR50.sql UTC',
  'Database amendments - add new process type field to process back up table.'
)
