/*
SCRIPT         : 165
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Adds error message for missing data stage referred to from end stage
*/

insert into BPAValCheck (checkid, catid, typeid, description, enabled)
values (14, 0, 0, 'Stage to read value from does not exist{0}', 1)

--set DB version
insert into BPADBVersion values (
  '165',
  GETUTCDATE(),
  'db_upgradeR165.sql UTC',
  'Adds error message for missing data stage referred to from end stage'
);
