/*

SCRIPT         : 21
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : PJW
PURPOSE        : Drop unused tables. See bug 2301
*/

DROP TABLE BPAUserPreference
DROP TABLE BPAUserPrefNar
GO

--set DB version
INSERT INTO BPADBVersion VALUES (
  '60',
  GETUTCDATE(),
  'db_upgradeR60.sql UTC',
  'Database amendments - Drop unused tables BPAUserPreference and BPAUserPrefNar.'
)
GO
