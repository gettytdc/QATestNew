/*
SCRIPT         : 104
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : GB
PURPOSE        : Add Maximum login attempts
*/


ALTER TABLE BPAUser ADD 
  loginattempts int NOT NULL CONSTRAINT DEF_BPAUser_loginattempt DEFAULT 0
    
ALTER TABLE BPASysConfig ADD 
  maxloginattempts int NULL

GO
    
-- set DB version
INSERT INTO BPADBVersion VALUES (
  '104',
  GETUTCDATE(),
  'db_upgradeR104.sql UTC',
  'Add Maximum login attempts'
);
