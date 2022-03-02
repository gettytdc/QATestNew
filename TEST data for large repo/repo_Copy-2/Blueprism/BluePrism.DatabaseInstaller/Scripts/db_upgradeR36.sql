
--SCRIPT PURPOSE: Update BPASysconfig to store archiving settings
--NUMBER: 36
--AUTHOR: PJW
--DATE: 26/11/2005 


alter table BPASysconfig add 
    ArchivingInterval VARCHAR(32),
    ArchivingTime smalldatetime,
    ArchivingDayOfMonth VARCHAR(32),
    ArchivingMinLogAgeLimit VARCHAR(32),
    ArchivingMachineID uniqueidentifier,
    ArchiveInProgress VARCHAR(20),
    CONSTRAINT [FK_BPASysconfig_BPAResource] FOREIGN KEY
    (
        ArchivingMachineID
    ) REFERENCES BPAResource (
        ResourceID
    )   
GO
    
--set DB version
INSERT INTO BPADBVersion VALUES (
  '36',
  GETUTCDATE(),
  'db_upgradeR36.sql UTC',
  'Database amendments - Update BPASysconfig to store archiving settings.'
)
