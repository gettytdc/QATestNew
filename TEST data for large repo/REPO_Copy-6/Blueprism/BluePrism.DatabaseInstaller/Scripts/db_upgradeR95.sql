/*
SCRIPT         : 95
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : PW
PURPOSE        : Change session status description from 'Exception' to 'Terminated'. See also script 74.
*/

UPDATE BPAStatus set description='Terminated' where description='Exception';

--set DB version
INSERT INTO BPADBVersion VALUES (
  '95',
  GETUTCDATE(),
  'db_upgradeR95.sql UTC',
  'Change session status description from Exception to Terminated. See also script 74'
)
GO

