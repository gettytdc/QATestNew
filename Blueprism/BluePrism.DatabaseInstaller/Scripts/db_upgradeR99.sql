/*
SCRIPT         : 99
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Added a 'user trigger' flag to the trigger to indicate which ones can be
                 configured by a user in the UI and which are system triggers (eg. "Run Now" et al) 
*/

alter table BPAScheduleTrigger
    add usertrigger bit not null
        constraint DEF_BPAScheduleTrigger_usertrigger default 1;
go

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '99',
  GETUTCDATE(),
  'db_upgradeR99.sql UTC',
  'Added a flag to indicate if a trigger can be configured using the UI or not.'
);
