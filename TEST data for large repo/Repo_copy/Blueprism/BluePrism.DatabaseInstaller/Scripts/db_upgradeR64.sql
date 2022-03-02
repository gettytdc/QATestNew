/*

SCRIPT         : 64
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : PJW
PURPOSE        : Remove relationship between BPASysconfig and BPAResource
                    - resource PCs are for running processes and do not (necessarily)
                    correspond to physical machines used by a user.

*/

--Remember the current archiving settings
IF object_id('BPAUpgradeTempMachine') IS NOT NULL
    DROP TABLE BPAUpgradeTempMachine
CREATE TABLE BPAUpgradeTempMachine (
    MachineName Varchar(16) primary key
)
INSERT INTO BPAUpgradeTempMachine (MachineName) 
    (SELECT TOP 1 R.[Name] AS MachineName FROM BPAResource R
    LEFT JOIN BPASysconfig C ON (C.ArchivingMachineID = R.ResourceID))
GO


--Remove relationship with BPAResource, and rename column
ALTER TABLE BPASysconfig
    DROP FK_BPASysconfig_BPAResource
GO
ALTER TABLE BPASysconfig
    ALTER COLUMN ArchivingMachineID VarChar(16)
GO
EXEC sp_rename 'BPASysConfig.ArchivingMachineID', 'ArchivingMachineName', 'COLUMN'
GO


--Restore whatever data was previously there, and clean up
UPDATE BPASysConfig SET ArchivingMachineName=(SELECT MachineName FROM BPAUpgradeTempMachine)
GO
DROP TABLE BPAUpgradeTempMachine


--set DB version
INSERT INTO BPADBVersion VALUES (
  '64',
  GETUTCDATE(),
  'db_upgradeR64.sql UTC',
  'Database amendments - Remove relationship between BPASysConfig and BPAResource'
)
GO
