/*
SCRIPT         : 146
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Adds enabling/disabling of individual validation checks
*/

alter table BPAValCheck
  add enabled bit not null
    constraint DEF_BPAValCheck_enabled default 1;

--set DB version
INSERT INTO BPADBVersion VALUES (
  '146',
  GETUTCDATE(),
  'db_upgradeR146.sql UTC',
  'Adds enabling/disabling of individual validation checks'
)
