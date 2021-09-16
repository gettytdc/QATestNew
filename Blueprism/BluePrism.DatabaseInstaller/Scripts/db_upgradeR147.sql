/*
SCRIPT         : 147
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Adds element name to a validation check
*/

update BPAValCheck set
  description = 'Application element "{0}" has dynamic attributes but no notes'
where checkid = 130

--set DB version
INSERT INTO BPADBVersion VALUES (
  '147',
  GETUTCDATE(),
  'db_upgradeR147.sql UTC',
  'Adds element name to a validation check'
)
