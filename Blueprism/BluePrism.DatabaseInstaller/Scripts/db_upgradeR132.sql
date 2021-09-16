/*
SCRIPT         : 132
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : CG
PURPOSE        : Add Resource PC diagnostics configuration
*/

alter table bparesource add diagnostics int default 0 not NULL

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '132',
  GETUTCDATE(),
  'db_upgradeR132.sql UTC',
  'Add Resource PC diagnostics configuration'
);
