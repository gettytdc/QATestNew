/*
SCRIPT         : 154
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : GMB
PURPOSE        : Change data type of password field in BPACredentials
*/

alter table BPACredentials alter column password text not null

--set DB version
INSERT INTO BPADBVersion VALUES (
  '154',
  GETUTCDATE(),
  'db_upgradeR154.sql UTC',
  'Change data type of password field in BPACredentials'
)
