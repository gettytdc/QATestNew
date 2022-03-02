/*
SCRIPT         : 77
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : CEG
PURPOSE        : Modify credentials table to allow assignment of multiple roles
*/

ALTER TABLE BPACredentials DROP CONSTRAINT FK_BPACredentials_RoleID

GO

--set DB version
INSERT INTO BPADBVersion VALUES (
  '77',
  GETUTCDATE(),
  'db_upgradeR77.sql UTC',
  'Modify credentials table to allow assignment of multiple roles'
)
GO
