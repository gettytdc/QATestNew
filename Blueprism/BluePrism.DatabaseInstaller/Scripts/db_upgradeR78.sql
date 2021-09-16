/*
SCRIPT         : 78
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : CEG
PURPOSE        : Modify credentials tables to allow an 'any' for the resources and processes
*/

ALTER TABLE BPACredentialsProcesses ALTER COLUMN processid uniqueidentifier NULL
ALTER TABLE BPACredentialsResources ALTER COLUMN resourceid uniqueidentifier NULL


GO

--set DB version
INSERT INTO BPADBVersion VALUES (
  '78',
  GETUTCDATE(),
  'db_upgradeR78.sql UTC',
  'Modify credentials tables to allow an ''any'' for the resources and processes'
)
GO
