/*
SCRIPT         : 75
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : CEG
PURPOSE        : Changes to support Process XML compression
*/

ALTER TABLE BPAProcess ADD compressedxml image NULL;
ALTER TABLE BPAProcessBackup ADD compressedxml image NULL;
ALTER TABLE BPASysConfig ADD CompressProcessXML bit DEFAULT 1 NOT NULL;
GO
UPDATE BPASysConfig SET CompressProcessXML=1;
GO

--set DB version
INSERT INTO BPADBVersion VALUES (
  '75',
  GETUTCDATE(),
  'db_upgradeR75.sql UTC',
  'Changes to support Process XML compression'
)
GO

