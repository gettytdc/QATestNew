/*
SCRIPT         : 166
PROJECT NAME   : Automate
DATABASE NAME  : BPA
CREATION DATE  : 30 Oct 2013
AUTHOR         : CEG
PURPOSE        : Expand field in BPASysConfig
NOTES          : 
*/

ALTER TABLE BPASysConfig
    ALTER COLUMN LicenseKey text null

--set DB version
INSERT INTO BPADBVersion VALUES (
  '166',
  GETUTCDATE(),
  'db_upgradeR166.sql UTC',
  'Expand field in BPASysConfig'
)

