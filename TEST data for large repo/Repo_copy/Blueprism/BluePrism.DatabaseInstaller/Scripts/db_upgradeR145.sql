/*
SCRIPT         : 145
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Adds support for storing imported releases / packages
*/

alter table BPARelease
  add local bit not null
    constraint DEF_BPARelease_local default 1;

--set DB version
INSERT INTO BPADBVersion VALUES (
  '145',
  GETUTCDATE(),
  'db_upgradeR145.sql UTC',
  'Adds support for storing imported releases / packages'
)
