/*
SCRIPT         : 191
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : GM
PURPOSE        : Adds Unicode logging option and renames session log tables
*/

-- Add unicode option to system config
-- Default to off, unless entries already exist in new table (i.e. beta testers)
alter table BPASysConfig add unicodeLogging bit not null default 0;
GO
update BPASysConfig set unicodeLogging=1 where (select COUNT(sessionnumber) from BPASessionLog) > 0;

-- Rename 'old' logging table to BPASessionLog_NonUnicode
EXEC sp_rename 'BPASessionLog_v4','BPASessionLog_NonUnicode'
EXEC sp_rename 'PK_BPASessionLog_v4','PK_BPASessionLog_NonUnicode'
EXEC sp_rename 'FK_BPASessionLog_BPASession_v4', 'FK_BPASessionLog_NonUnicode_BPASession'

-- Rename 'new' logging table to BPASessionLog_Unicode
EXEC sp_rename 'BPASessionLog','BPASessionLog_Unicode'
EXEC sp_rename 'PK_BPASessionLog','PK_BPASessionLog_Unicode'
EXEC sp_rename 'FK_BPASessionLog_BPASession', 'FK_BPASessionLog_Unicode_BPASession'

-- Adds index missed from R182
create index Index_SessionStageType on BPASessionLog_Unicode(sessionnumber, stagetype)

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '191',
  GETUTCDATE(),
  'db_upgradeR191.sql UTC',
  'Adds Unicode logging option and renames session log tables'
);

