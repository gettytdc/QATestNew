/*
SCRIPT         : 108
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : GB 
PURPOSE        : Makes BPASchedule entries hideable, and also makes BPATask names nullable

*/

ALTER TABLE BPASchedule ALTER COLUMN
    name varchar(128) NULL
    
ALTER TABLE BPASchedule ADD
    deletedname varchar(128) NULL
    
ALTER TABLE BPATask ALTER COLUMN
    name varchar(128) NULL
    
-- set DB version
INSERT INTO BPADBVersion VALUES (
  '108',
  GETUTCDATE(),
  'db_upgradeR108.sql UTC',
  'Makes BPASchedule entries hideable, and also makes BPATask names nullable' 
);

