
--SCRIPT PURPOSE: Make sure foreign key constraint FK_BPAPROCESSLOCK_BPAPROCESS is enforced
--NUMBER: 38
--AUTHOR: PJW
--DATE: 20/12/2005 

alter table [BPAProcessLock] check constraint [FK_BPAProcessLock_BPAProcess]
GO
    
--set DB version
INSERT INTO BPADBVersion VALUES (
  '38',
  GETUTCDATE(),
  'db_upgradeR37.sql UTC',
  'Database amendments - Make sure foreign key constraint FK_BPAPROCESSLOCK_BPAPROCESS is enforced.'
)
