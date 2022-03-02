/*
SCRIPT         : 55
PROJECT NAME   : Automate
DATABASE NAME  : BPA
CREATION DATE  : 29 May 2007
AUTHOR         : PJW
PURPOSE        : Add new table for keeping track of the number of machines using process alerts.
NOTES          : 
*/

CREATE TABLE BPAAlertsMachines (
    [MachineName]   VARCHAR(128)        NOT NULL UNIQUE,
    CONSTRAINT [PK_BPAAlertsMachines] PRIMARY KEY  CLUSTERED 
    (
        [MachineName]
    )
)
GO

--set DB version
INSERT INTO BPADBVersion VALUES (
  '55',
  GETUTCDATE(),
  'db_upgradeR55.sql UTC',
  'Licensing changes: Added new table for keeping track of the number of machines using process alerts.'
)
