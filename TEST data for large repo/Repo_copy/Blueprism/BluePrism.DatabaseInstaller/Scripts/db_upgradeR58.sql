/*
SCRIPT         : 58
PROJECT NAME   : Automate
DATABASE NAME  : BPA
CREATION DATE  : July 2007
AUTHOR         : JC
PURPOSE        : Amend session log table column sizes
NOTES          : 
*/

ALTER TABLE BPASessionLog ALTER COLUMN stagename VARCHAR(128) NULL
ALTER TABLE BPASessionLog ALTER COLUMN processname VARCHAR(128) NULL
ALTER TABLE BPASessionLog ALTER COLUMN pagename    VARCHAR(128) NULL
ALTER TABLE BPASessionLog ALTER COLUMN objectname  VARCHAR(128) NULL
ALTER TABLE BPASessionLog ALTER COLUMN actionname  VARCHAR(128) NULL

GO


--set DB version
INSERT INTO BPADBVersion VALUES (
  '58',
  GETUTCDATE(),
  'db_upgradeR58.sql UTC',
  'Amend session log table column sizes'
)


