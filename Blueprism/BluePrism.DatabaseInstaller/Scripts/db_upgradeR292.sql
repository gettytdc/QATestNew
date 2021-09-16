-- removed old script

--set DB version
INSERT INTO BPADBVersion VALUES (
  '292',
  GETUTCDATE(),
  'db_upgradeR292.sql UTC',
  'Add columns for Work Queue Analysis setting',
  0)