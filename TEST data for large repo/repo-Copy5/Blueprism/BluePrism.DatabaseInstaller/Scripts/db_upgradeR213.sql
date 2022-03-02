/*
SCRIPT         : 213
AUTHOR         : DM
PURPOSE        : Rename permissions (bg-221)
*/

update BPAPerm set name = 'Read Access to Queue Management' where name = 'Read-Only Access to Queue Management'
update BPAPerm set name = 'Read Access to Session Management' where name = 'Read-Only Access to Session Management'

--set DB version
INSERT INTO BPADBVersion VALUES (
  '213',
  GETUTCDATE(),
  'db_upgradeR213.sql UTC',
  'Rename permissions (bg-221)'
);
