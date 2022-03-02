
--SCRIPT PURPOSE: Add user_informed column to BPAProcessBackup table
--NUMBER: 37
--AUTHOR: DD
--DATE: 07/12/2005 

ALTER TABLE [BPAProcessBackup] ADD 
    [user_informed] [bit] NOT NULL CONSTRAINT [DF_BPAProcessBackup_user_informed] DEFAULT (0)
GO
    
--set DB version
INSERT INTO BPADBVersion VALUES (
  '37',
  GETUTCDATE(),
  'db_upgradeR37.sql UTC',
  'Database amendments - Alter BPAProcessBackup to store user_informed flag.'
)
