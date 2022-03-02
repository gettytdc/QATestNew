/*
SCRIPT         : 183
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : CG
PURPOSE        : Rename reporting permission
*/

update BPAPerm set name = 'System - Reporting' where name = 'View Reports';

update BPAPermGroupMember set permgroupid = (select id from BPAPermGroup where name = 'System Manager') where permid = (select id from BPAPerm where name = 'System - Reporting');
GO

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '183',
  GETUTCDATE(),
  'db_upgradeR183.sql UTC',
  'Rename reporting permission'
);

