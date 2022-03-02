
/*
SCRIPT         : 156
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : CG
PURPOSE        : Update BPASysConfig archiving columns
*/
ALTER TABLE BPASysConfig
        DROP COLUMN ArchivingInterval;
ALTER TABLE BPASysConfig
    DROP COLUMN ArchivingTime;
ALTER TABLE BPASysConfig        
    DROP COLUMN ArchivingDayOfMonth;
ALTER TABLE BPASysConfig
    DROP COLUMN ArchivingMinLogAgeLimit;
ALTER TABLE BPASysConfig
    DROP COLUMN ArchivingMachineName;
ALTER TABLE BPASysConfig
    ADD ArchivingMode integer NOT NULL DEFAULT 0;
ALTER TABLE BPASysConfig
    ADD ArchivingLastAuto datetime NULL;
ALTER TABLE BPASysConfig
    ADD ArchivingFolder text NOT NULL DEFAULT '';
ALTER TABLE BPASysConfig
    ADD ArchivingAge text NOT NULL DEFAULT '6m';
ALTER TABLE BPASysConfig
    Add ArchivingDelete bit NOT NULL DEFAULT 0;
ALTER TABLE BPASysConfig
    ADD ArchivingResource uniqueidentifier,
    CONSTRAINT [FK_BPASysconfig_BPAResource] FOREIGN KEY ( ArchivingResource )
    REFERENCES BPAResource ( ResourceID );

GO

--set DB version
INSERT INTO BPADBVersion VALUES (
  '156',
  GETUTCDATE(),
  'db_upgradeR156.sql UTC',
  'Update BPASysConfig archiving columns'
);
