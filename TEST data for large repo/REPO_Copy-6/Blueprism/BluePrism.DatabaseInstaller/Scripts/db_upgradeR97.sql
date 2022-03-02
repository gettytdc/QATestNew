/*
SCRIPT         : 97
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Create concept of 'system' user - add a scheduler user.
*/

-- Create a systemusername 
-- : when null, it's a normal user, treated as before
-- : when username is null & systemusername not null, it's a system user.
alter table BPAUser add 
    systemusername varchar(128) null;
go

-- Only one system user for now - the scheduler
insert into BPAUser (userid, systemusername)
    values (newid(), 'Scheduler');

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '97',
  GETUTCDATE(),
  'db_upgradeR97.sql UTC',
  'Created system user using a ''systemusername'' field on BPAUser'
);
