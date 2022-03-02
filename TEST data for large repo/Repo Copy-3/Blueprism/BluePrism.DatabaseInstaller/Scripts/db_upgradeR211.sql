/*
SCRIPT         : 211
AUTHOR         : GMB
PURPOSE        : Add anonymous resource to BPAUser
*/

-- Add the anonymous resource system user
insert into BPAUser (userid, systemusername)
    values (newid(), 'Anonymous Resource');

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '211',
  GETUTCDATE(),
  'db_upgradeR211.sql UTC',
  'Add anonymous resource to BPAUser'
);
