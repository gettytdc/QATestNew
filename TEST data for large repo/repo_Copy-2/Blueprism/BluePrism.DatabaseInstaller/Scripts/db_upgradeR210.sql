/*
SCRIPT         : 210
AUTHOR         : GM
PURPOSE        : Add roles column to auth token table
*/

alter table BPAInternalAuth
    add Roles nvarchar(max) default '';

--set DB version
INSERT INTO BPADBVersion VALUES (
  '210',
  GETUTCDATE(),
  'db_upgradeR210.sql UTC',
  'Add roles column to auth token table'
);
