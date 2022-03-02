/*
SCRIPT         : 81
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : CG
PURPOSE        : Drop unused 'availability' column from BPAResource as per bug #3775
*/

alter table BPAResource drop column availability;

--set DB version
INSERT INTO BPADBVersion VALUES (
  '81',
  GETUTCDATE(),
  'db_upgradeR81.sql UTC',
  'Drop unused availability column from BPAResource as per bug #3775'
)
GO
