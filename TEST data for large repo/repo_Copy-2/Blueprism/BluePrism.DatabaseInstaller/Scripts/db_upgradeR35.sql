
--SCRIPT PURPOSE: New tables and updates to cover View Preferences in realtime stats
--NUMBER: 35
--AUTHOR: PJW
--DATE: 09/11/2005 

ALTER TABLE BPAUser ADD
    preferredStatisticsInterval VARCHAR(64)

--set DB version
INSERT INTO BPADBVersion VALUES (
  '35',
  GETUTCDATE(),
  'db_upgradeR35.sql UTC',
  'Database amendments - Update BPAUser to allow preferred statistics interval..'
)
