/*
SCRIPT         : 190
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : GM
PURPOSE        : Rename single sign-on permission
*/

update BPAPerm set name = 'System - Single Sign On' where name = 'Workflow - Single Sign On';
GO

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '190',
  GETUTCDATE(),
  'db_upgradeR190.sql UTC',
  'Rename single sign-on permission'
);

