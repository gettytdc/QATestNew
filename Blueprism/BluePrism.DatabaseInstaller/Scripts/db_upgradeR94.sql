/*
SCRIPT         : 94
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Gave BPASchedule a version number such that if it or
                 any of its dependents are changed the version number is
                 updated. This way the in-memory schedule can quickly
                 check if it has been modified or not without dragging
                 all of its data out and across the network.
*/

-- allow null until we set a value for existing records
alter table BPASchedule
    add versionno int null;
GO

-- set version number on existing records to 1  
update BPASchedule set versionno=1;

-- Ensure that future records contain a non-null version no.
alter table BPASchedule
    alter column versionno int not null;
GO

--set DB version
INSERT INTO BPADBVersion VALUES (
  '94',
  GETUTCDATE(),
  'db_upgradeR94.sql UTC',
  'Added a version number field to the scheduler' + 
  'process to run on the same resource within the same scheduler task'
);
