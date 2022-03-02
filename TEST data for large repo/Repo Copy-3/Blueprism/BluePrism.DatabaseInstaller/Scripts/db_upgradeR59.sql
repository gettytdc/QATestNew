/*
SCRIPT         : 59
PROJECT NAME   : Automate
DATABASE NAME  : BPA
CREATION DATE  : July 2007
AUTHOR         : JC
PURPOSE        : Add new user table column to control log viewer column visibility
NOTES          : 
*/

ALTER TABLE BPAUser ADD LogViewerHiddenColumns INT

GO

--set DB version
INSERT INTO BPADBVersion VALUES (
  '59',
  GETUTCDATE(),
  'db_upgradeR59.sql UTC',
  'Add new user table column to control log viewer column visibility'
)


