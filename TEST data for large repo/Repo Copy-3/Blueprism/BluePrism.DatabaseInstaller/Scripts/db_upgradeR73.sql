/*
SCRIPT         : 73
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : CEG
PURPOSE        : Add timeout field for web service definitions - see bug #4221
*/

ALTER TABLE BPAWebService ADD timeout int default 10000;

--set DB version
INSERT INTO BPADBVersion VALUES (
  '73',
  GETUTCDATE(),
  'db_upgradeR73.sql UTC',
  'Add timeout field to BPAWebService'
)
GO
