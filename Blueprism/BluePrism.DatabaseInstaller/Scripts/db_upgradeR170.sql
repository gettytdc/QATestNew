/*
SCRIPT         : 170
PROJECT NAME   : Automate
DATABASE NAME  : BPA
CREATION DATE  : 28 Jan 2015
AUTHOR         : CEG
PURPOSE        : Updates for retrospective changes made under bug #8443
*/

-- The first bunch are all created earlier in their relevant scripts, but are
-- added here if they don't already exist (for upgrades from versions after
-- the aforementioned scripts)

IF object_id('PK_BPAExceptionType') IS NULL
    ALTER TABLE BPAExceptionType ADD
       constraint PK_BPAExceptionType primary key (id);

IF object_id('PK_BPAStatistics') IS NULL
   BEGIN
    ALTER TABLE BPAStatistics ALTER COLUMN sessionid uniqueidentifier NOT NULL;
    ALTER TABLE BPAStatistics ALTER COLUMN name varchar(50) NOT NULL;
   END
GO
IF object_id('PK_BPAStatistics') IS NULL
   BEGIN
    ALTER TABLE BPAStatistics ADD constraint PK_BPAStatistics primary key (sessionid, name);
   END
GO

IF object_id('PK_BPAPasswordRules') IS NULL
    alter table BPAPasswordRules ADD
    id int NOT NUll default 1
    constraint PK_BPAPasswordRules primary key (id)

IF object_id('PK_BPAValActionMap') IS NULL
    alter table BPAValActionMap ADD
    constraint PK_BPAValActionMap primary key (catid, typeid, actionid)

IF not exists (SELECT * FROM sys.indexes WHERE name='INDEX_BPAAuditEvents_eventdatetime' 
        AND object_id = OBJECT_ID('BPAAuditEvents'))
    create clustered index INDEX_BPAAuditEvents_eventdatetime on BPAAuditEvents(eventdatetime);

-- The following didn't need to be created in advance because the table is
-- not written to until runtime.
create clustered index INDEX_BPAIntegerPref_prefid on BPAIntegerPref(prefid);
create clustered index INDEX_BPAStringPref_prefid on BPAStringPref(prefid);
create clustered index INDEX_BPAToolPosition_userid_name on BPAToolPosition(UserID, Name);

--set DB version
INSERT INTO BPADBVersion VALUES (
  '170',
  GETUTCDATE(),
  'db_upgradeR170.sql UTC',
  'Updates for retrospective changes made under bug #8443'
)

