/*
SCRIPT         : 205
AUTHOR         : GM
PURPOSE        : Adds last signed in column to user table
*/

alter table BPAUser add lastsignedin datetime null;

--set DB version
INSERT INTO BPADBVersion VALUES (
  '205',
  GETUTCDATE(),
  'db_upgradeR205.sql UTC',
  'Adds last signed in column to user table'
);
