/*
SCRIPT         : 162
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Swaps order of sessionnumber index on BPASessionLog
*/

drop index Index_SessionStageType on BPASessionLog

create index Index_SessionStageType on BPASessionLog(sessionnumber, stagetype)

--set DB version
insert into BPADBVersion values (
  '162',
  GETUTCDATE(),
  'db_upgradeR162.sql UTC',
  'Swaps order of sessionnumber index on BPASessionLog'
);
