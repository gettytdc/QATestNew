/*
SCRIPT         : 54
PROJECT NAME   : Automate
DATABASE NAME  : BPA
CREATION DATE  : 23 May 2007
AUTHOR         : PJW
PURPOSE        : Add new license key field to BPASysconfig
NOTES          : 
*/

ALTER TABLE BPASysConfig
    ADD LicenseKey  VARCHAR(64)
GO

UPDATE BPASysConfig SET LicenseKey = 'MVcwc2sCCh8MZTFONGK0jp0='
GO

--set DB version
INSERT INTO BPADBVersion VALUES (
  '54',
  GETUTCDATE(),
  'db_upgradeR54.sql UTC',
  'Licensing changes: Added new LicenseKey field to BPASysConfig'
)
