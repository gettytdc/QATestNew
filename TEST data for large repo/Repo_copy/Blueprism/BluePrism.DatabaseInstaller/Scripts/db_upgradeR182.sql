/*
SCRIPT         : 182
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : CG
PURPOSE        : Switch to full unicode support
*/

-- Replace the existing 'drop default constraint' stored procedure (originally
-- created in R82) with a more up to date one.
if exists (select 1 from sysobjects 
    where id = object_id('bpa_sp_dropdefault') and type='P')
begin
    drop procedure bpa_sp_dropdefault
end
GO
create procedure bpa_sp_dropdefault
    @tableName nvarchar(256),
    @columnName nvarchar(256)
as
    declare @Command nvarchar(max)
    select @Command = 'ALTER TABLE ' + @tableName + ' drop constraint ' + d.name
        from sys.tables t join sys.default_constraints d on d.parent_object_id = t.object_id  
        join sys.columns c on c.object_id = t.object_id and c.column_id = d.parent_column_id
        where t.name = @tableName and c.name = @columnName;
    execute (@Command);
GO

-- Add a stored procedure for dropping unique constraings too
if exists (select 1 from sysobjects 
    where id = object_id('bpa_sp_dropunique') and type='P')
begin
    drop procedure bpa_sp_dropunique
end
GO
create procedure bpa_sp_dropunique
    @tableName nvarchar(256),
    @columnName nvarchar(256)
as
    declare @Command nvarchar(1000)
    select @Command = 'ALTER TABLE ' + @tableName + ' drop constraint ' + d.name
        from sys.tables t join sys.indexes d on d.object_id = t.object_id  
        join sys.columns c on c.object_id = t.object_id
        where t.name = @tableName and d.type=2 and d.is_unique=1 and c.name = @columnName;
    execute (@Command);
GO


-- These are obsolete tables from previous conversions, which we might as well
-- get rid of now.
DROP TABLE BPASession_OLD;
DROP TABLE BPASessionLog_OLD

-- Rename existing session log table - this _v4 table contains the logs created
-- by V4.x. These will remain in this table until such time as they are
-- archived. The software will use both the _v4 and normal tables when looking
-- for log data for a particular session.
EXEC sp_rename 'BPASessionLog','BPASessionLog_v4'
EXEC sp_rename 'PK_BPASessionLog','PK_BPASessionLog_v4'
EXEC sp_rename 'FK_BPASessionLog_BPASession', 'FK_BPASessionLog_BPASession_v4'

GO

-- Create the new table...
CREATE TABLE BPASessionLog (
    sessionnumber int NOT NULL,
    seqnum int NOT NULL,
    stageid uniqueidentifier NULL,
    stagename nvarchar(128) NULL,
    stagetype int NULL,
    processname nvarchar(128) NULL,
    pagename nvarchar(128) NULL,
    objectname nvarchar(128) NULL,
    actionname nvarchar(128) NULL,
    result nvarchar(max) NULL,
    resulttype int NULL,
    startdatetime datetime NULL,
    enddatetime datetime NULL,
    attributexml nvarchar(max) NULL,
    automateworkingset bigint NULL,
    targetappname nvarchar(32) NULL,
    targetappworkingset bigint NULL,
    CONSTRAINT PK_BPASessionLog PRIMARY KEY CLUSTERED 
    (
        sessionnumber ASC,
        seqnum ASC
    ))
GO

ALTER TABLE BPASessionLog WITH CHECK ADD CONSTRAINT FK_BPASessionLog_BPASession FOREIGN KEY(sessionnumber)
    REFERENCES BPASession (sessionnumber);
GO

-- BPASession just gets the fields upgraded. It will contain both old and new
-- sessions.
ALTER TABLE BPASession ALTER COLUMN runningosusername nvarchar(50);
ALTER TABLE BPASession ALTER COLUMN startparamsxml nvarchar(max);
ALTER TABLE BPASession ALTER COLUMN logginglevelsxml nvarchar(max);
ALTER TABLE BPASession ALTER COLUMN sessionstatexml nvarchar(max);


ALTER TABLE BPAUser ALTER COLUMN username nvarchar(128);
ALTER TABLE BPAUser ALTER COLUMN password nvarchar(100);
ALTER TABLE BPAUser ALTER COLUMN useremail nvarchar(60);
ALTER TABLE BPAUser ALTER COLUMN preferredStatisticsInterval nvarchar(60);
ALTER TABLE BPAUser ALTER COLUMN systemusername nvarchar(128);

ALTER TABLE BPAProcess ALTER COLUMN ProcessType nvarchar(1);
ALTER TABLE BPAProcess ALTER COLUMN name nvarchar(128);
ALTER TABLE BPAProcess ALTER COLUMN description nvarchar(1000);
ALTER TABLE BPAProcess ALTER COLUMN version nvarchar(20);
ALTER TABLE BPAProcess ALTER COLUMN processxml nvarchar(max);

CREATE table BPAProcessEnvVar_new (
    processid uniqueidentifier NOT NULL,
    name nvarchar(64) NOT NULL,
    CONSTRAINT PK_BPAProcessEnvVar_new PRIMARY KEY CLUSTERED (processid,name)
);
INSERT into BPAProcessEnvVar_new (processid, name)
    select processid, name from BPAProcessEnvVar;
drop table BPAProcessEnvVar;
exec sp_rename 'BPAProcessEnvVar_new', 'BPAProcessEnvVar';
exec sp_rename 'PK_BPAProcessEnvVar_new', 'PK_BPAProcessEnvVar';

GO

ALTER TABLE BPAProcessBackUp ALTER COLUMN processxml nvarchar(max);

ALTER TABLE BPAStatus ALTER COLUMN type nvarchar(10);
ALTER TABLE BPAStatus ALTER COLUMN description nvarchar(20);

ALTER TABLE BPAResource DROP CONSTRAINT UNQ_BPAResource_name;
ALTER TABLE BPAResource ALTER COLUMN name nvarchar(128);
ALTER TABLE BPAResource ALTER COLUMN status nvarchar(10);
ALTER TABLE BPAResource ADD CONSTRAINT UNQ_BPAResource_name unique (name);

create table BPAEnvLock_new (
    name nvarchar(255) not null
        constraint PK_BPAEnvLock_new primary key,
    token nvarchar(255) null,
    sessionid uniqueidentifier null
        constraint FK_BPAEnvLock_BPASession_new
        foreign key references BPASession(sessionid),
    locktime datetime null,
    comments nvarchar(1024) null
);
insert into BPAEnvLock_new (name, token, sessionid, locktime, comments) 
    select name, token, sessionid, locktime, comments from BPAEnvLock;
drop table BPAEnvLock;
exec sp_rename 'BPAEnvLock_new', 'BPAEnvLock';
exec sp_rename 'PK_BPAEnvLock_new', 'PK_BPAEnvLock';
exec sp_rename 'FK_BPAEnvLock_BPASession_new', 'FK_BPAEnvLock_BPASession';

ALTER TABLE BPAScenarioLink ALTER COLUMN scenarioname nvarchar(50);

GO

CREATE TABLE BPAToolPosition_new (
    UserID uniqueidentifier NULL,
    Name nvarchar(100) NULL,
    Position nchar(1) NULL,
    X int NULL,
    Y int NULL,
    Mode nchar(1) NULL,
    Visible bit NULL
);
create clustered index INDEX_BPAToolPosition_userid_name_new on BPAToolPosition_new(UserID, Name);
insert into BPAToolPosition_new(UserID, Name, Position, X, Y, Mode, Visible)
    select UserID, Name, Position, X, Y, Mode, Visible from BPAToolPosition;
drop table BPAToolPosition;
GO
exec sp_rename 'BPAToolPosition_new', 'BPAToolPosition';
exec sp_rename 'BPAToolPosition.INDEX_BPAToolPosition_userid_name_new', 'INDEX_BPAToolPosition_userid_name';

ALTER TABLE BPAScheduleList DROP CONSTRAINT UNQ_BPAScheduleList_listtype_name;
ALTER TABLE BPAScheduleList ALTER COLUMN name nvarchar(128);
ALTER TABLE BPAScheduleList ALTER COLUMN description nvarchar(max);
ALTER TABLE BPAScheduleList ADD CONSTRAINT UNQ_BPAScheduleList_listtype_name unique (listtype, name);

ALTER TABLE BPAScenario ALTER COLUMN scenariotext nvarchar(1000);
ALTER TABLE BPAScenario ALTER COLUMN scenarionotes nvarchar(1000);

CREATE TABLE BPAResourceConfig_new (
    name nvarchar(128) NOT NULL,
    config nvarchar(max) NULL,
    CONSTRAINT PK_BPAResourceConfig_new PRIMARY KEY CLUSTERED
            (name ASC)
);
insert into BPAResourceConfig_new (name, config) 
    select name, config from BPAResourceConfig;
drop table BPAResourceConfig;
exec sp_rename 'BPAResourceConfig_new', 'BPAResourceConfig';
exec sp_rename 'PK_BPAResourceConfig_new', 'PK_BPAResourceConfig';

ALTER TABLE BPAResourceAttribute ALTER COLUMN AttributeName nvarchar(64);

ALTER TABLE BPAReport ALTER COLUMN name nvarchar(128);
ALTER TABLE BPAReport ALTER COLUMN description nvarchar(1000);

ALTER TABLE BPASchedule ALTER COLUMN name nvarchar(128);
ALTER TABLE BPASchedule ALTER COLUMN description nvarchar(max);
ALTER TABLE BPASchedule ALTER COLUMN deletedname nvarchar(128);

CREATE TABLE BPAAlertsMachines_new (
    MachineName nvarchar(128) NOT NULL,
    CONSTRAINT PK_BPAAlertsMachines_new PRIMARY KEY CLUSTERED (MachineName ASC)
);
insert into BPAAlertsMachines_new (MachineName) 
    select MachineName from BPAAlertsMachines;
drop table BPAAlertsMachines;
exec sp_rename 'BPAAlertsMachines_new', 'BPAAlertsMachines';
exec sp_rename 'PK_BPAAlertsMachines_new', 'PK_BPAAlertsMachines';

ALTER TABLE BPAPublicHolidayGroup DROP CONSTRAINT UNQ_BPAPublicHoliday_name;
ALTER TABLE BPAPublicHolidayGroup ALTER COLUMN name nvarchar(64);
ALTER TABLE BPAPublicHolidayGroup ADD CONSTRAINT UNQ_BPAPublicHoliday_name unique (name);

ALTER TABLE BPAPublicHoliday ALTER COLUMN name nvarchar(64);

CREATE TABLE BPAProcessMITemplate_new (
    templatename nvarchar(32) NOT NULL,
    processid uniqueidentifier NOT NULL,
    defaulttemplate bit NOT NULL,
    templatexml nvarchar(max) NULL
)
alter table BPAProcessMITemplate_new
    add constraint PK_BPAProcessMITemplate_new
    primary key(templatename, processid);
insert into BPAProcessMITemplate_new (templatename, processid, defaulttemplate, templatexml) 
    select templatename, processid, defaulttemplate, templatexml from BPAProcessMITemplate;
drop table BPAProcessMITemplate;
exec sp_rename 'BPAProcessMITemplate_new', 'BPAProcessMITemplate';
exec sp_rename 'PK_BPAProcessMITemplate_new', 'PK_BPAProcessMITemplate';

ALTER TABLE BPAProcessAttribute ALTER COLUMN AttributeName nvarchar(64);

ALTER TABLE BPAPasswordRules ALTER COLUMN additional nvarchar(128);

GO

ALTER TABLE BPAPackageFont DROP CONSTRAINT FK_BPAPackageFont_BPAFont;
CREATE TABLE BPAPackageFont_new (
    packageid int NOT NULL,
    name nvarchar(255) NOT NULL,
    CONSTRAINT PK_BPAPackageFont_new PRIMARY KEY CLUSTERED 
    (packageid,  name)
);
CREATE TABLE BPAFont_new (
    name nvarchar(255) NOT NULL,
    version nvarchar(255) NOT NULL,
    fontdata nvarchar(max) NOT NULL,
    CONSTRAINT PK_BPAFont_new PRIMARY KEY CLUSTERED (name)
);
insert into BPAFont_new (name,version,fontdata) 
    select name,version,fontdata from BPAFont;
insert into BPAPackageFont_new (packageid, name)
    select packageid, name FROM BPAPackageFont;
drop table BPAFont;
exec sp_rename 'BPAFont_new', 'BPAFont';
exec sp_rename 'PK_BPAFont_new', 'PK_BPAFont';
drop table BPAPackageFont;
exec sp_rename 'BPAPackageFont_new', 'BPAPackageFont';
exec sp_rename 'PK_BPAPackageFont_new', 'PK_BPAPackageFont';
ALTER TABLE BPAPackageFont WITH CHECK ADD CONSTRAINT FK_BPAPackageFont_BPAFont FOREIGN KEY(name)
    REFERENCES BPAFont (name)
    ON UPDATE CASCADE
    ON DELETE CASCADE;
ALTER TABLE BPAPackageFont WITH CHECK ADD CONSTRAINT FK_BPAPackageFont_BPAPackage FOREIGN KEY(packageid)
    REFERENCES BPAPackage (id)
    ON DELETE CASCADE;

exec bpa_sp_dropunique 'BPAExceptionType', 'type';
ALTER TABLE BPAExceptionType ALTER COLUMN type nvarchar(30);
ALTER TABLE BPAExceptionType ADD CONSTRAINT UNQ_BPAExceptionType_type unique (type);

ALTER TABLE BPAPackage DROP CONSTRAINT UNQ_BPAPackage_name;
ALTER TABLE BPAPackage ALTER COLUMN name nvarchar(255);
ALTER TABLE BPAPackage ALTER COLUMN description nvarchar(max);
ALTER TABLE BPAPackage ADD CONSTRAINT UNQ_BPAPackage_name unique (name);

GO

if exists (select 1 from BPADBVersion where dbversion='193') exec('
CREATE TABLE BPACredentials_new (
    id uniqueidentifier NOT NULL,
    name nvarchar(64) NOT NULL,
    description nvarchar(max) NOT NULL,
    login nvarchar(64) NOT NULL,
    password nvarchar(max) NOT NULL,
    expirydate datetime NULL,
    invalid bit NULL,
    encryptid int NULL
        constraint FK_BPACredentials_BPAKeyStore_new
        foreign key references BPAKeyStore (id),
    CONSTRAINT PK_BPACredentials_new PRIMARY KEY CLUSTERED (id ASC)
);
insert into BPACredentials_new (id, name, description, login, password, expirydate, invalid, encryptid)
    select id, name, description, login, password, expirydate, invalid, encryptid from BPACredentials;');
else exec('
CREATE TABLE BPACredentials_new (
    id uniqueidentifier NOT NULL,
    name nvarchar(64) NOT NULL,
    description nvarchar(max) NOT NULL,
    login nvarchar(64) NOT NULL,
    password nvarchar(max) NOT NULL,
    expirydate datetime NULL,
    invalid bit NULL,
    CONSTRAINT PK_BPACredentials_new PRIMARY KEY CLUSTERED (id ASC)
);
insert into BPACredentials_new (id, name, description, login, password, expirydate, invalid)
    select id, name, description, login, password, expirydate, invalid from BPACredentials;');

ALTER TABLE BPACredentialRole DROP CONSTRAINT FK_BPACredentialRole_BPACredential;
ALTER TABLE BPAPackageCredential DROP CONSTRAINT FK_BPAPackageCredential_BPACredentials;
ALTER TABLE BPACredentialsProcesses DROP CONSTRAINT FK_BPACredentialsProcesses_cred;
ALTER TABLE BPACredentialsResources DROP CONSTRAINT FK_BPACredentialsResources_cred;
ALTER TABLE BPACredentialsProperties DROP CONSTRAINT FK_BPACredentialsProperties_cred;
drop table BPACredentials;
exec sp_rename 'BPACredentials_new', 'BPACredentials';
exec sp_rename 'PK_BPACredentials_new', 'PK_BPACredentials';

if exists (select 1 from BPADBVersion where dbversion='193')
    exec sp_rename 'FK_BPACredentials_BPAKeyStore_new', 'FK_BPACredentials_BPAKeyStore';

ALTER TABLE BPACredentialsProcesses ADD CONSTRAINT FK_BPACredentials_RoleID FOREIGN KEY
    (credentialid) REFERENCES BPACredentials (id);
ALTER TABLE BPACredentialsResources ADD CONSTRAINT FK_BPACredentialsResources_cred FOREIGN KEY
    (credentialid) REFERENCES BPACredentials (id);
ALTER TABLE BPACredentialsProperties ADD CONSTRAINT FK_BPACredentialsProperties_cred FOREIGN KEY
    (credentialid) REFERENCES BPACredentials (id);
ALTER TABLE BPAPackageCredential ADD CONSTRAINT FK_BPAPackageCredential_BPACredentials FOREIGN KEY
        (credentialid) REFERENCES BPACredentials(id) on delete cascade;
ALTER TABLE BPACredentialRole ADD CONSTRAINT FK_BPACredentialRole_BPACredential FOREIGN KEY
        (credentialid) REFERENCES BPACredentials (id) on delete cascade;

ALTER TABLE BPAAuditEvents ALTER COLUMN sCode nvarchar(10);
ALTER TABLE BPAAuditEvents ALTER COLUMN sNarrative nvarchar(500);
ALTER TABLE BPAAuditEvents ALTER COLUMN comments nvarchar(512);
ALTER TABLE BPAAuditEvents ALTER COLUMN EditSummary nvarchar(max);
ALTER TABLE BPAAuditEvents ALTER COLUMN oldXML nvarchar(max);
ALTER TABLE BPAAuditEvents ALTER COLUMN newXML nvarchar(max);


ALTER TABLE BPAPackageEnvironmentVar DROP CONSTRAINT FK_BPAPackageEnvironmentVar_BPAEnvironmentVar;
CREATE TABLE BPAEnvironmentVar_new (
    name nvarchar(64) NOT NULL,
    datatype nvarchar(16) NOT NULL,
    value nvarchar(max) NOT NULL,
    description nvarchar(max) NOT NULL,
    CONSTRAINT PK_BPAEnvironmentVar_new PRIMARY KEY CLUSTERED (name)
);
insert into BPAEnvironmentVar_new (name, datatype, value, description)
    select name, datatype, value, description from BPAEnvironmentVar;
CREATE TABLE BPAPackageEnvironmentVar_new (
    packageid int NOT NULL,
    name nvarchar(64) NOT NULL,
    CONSTRAINT PK_BPAPackageEnvironmentVar_new PRIMARY KEY CLUSTERED 
    (packageid, name)
);
insert into BPAPackageEnvironmentVar_new (packageid, name)
    select packageid, name from BPAPackageEnvironmentVar;
drop table BPAEnvironmentVar;
exec sp_rename 'BPAEnvironmentVar_new', 'BPAEnvironmentVar';
exec sp_rename 'PK_BPAEnvironmentVar_new', 'PK_BPAEnvironmentVar';
drop table BPAPackageEnvironmentVar;
exec sp_rename 'BPAPackageEnvironmentVar_new', 'BPAPackageEnvironmentVar';
exec sp_rename 'PK_BPAPackageEnvironmentVar_new', 'PK_BPAPackageEnvironmentVar';
ALTER TABLE BPAPackageEnvironmentVar ADD constraint FK_BPAPackageEnvironmentVar_BPAEnvironmentVar
              FOREIGN KEY (name) references BPAEnvironmentVar(name);
GO

CREATE TABLE BPADataTracker_new (
        dataname nvarchar(64) not null
        constraint PK_BPADataTracker_new primary key,
    versionno bigint not null
);
insert into BPADataTracker_new (dataname, versionno)
    select dataname, versionno from BPADataTracker;
drop table BPADataTracker;
exec sp_rename 'BPADataTracker_new', 'BPADataTracker';
exec sp_rename 'PK_BPADataTracker_new', 'PK_BPADataTracker';

CREATE TABLE [BPAStatistics_new] (
    sessionid uniqueidentifier NOT NULL,
    name nvarchar (50) NOT NULL,
    datatype nvarchar (32) NULL,
    value_text nvarchar (255) NULL,
    value_number float NULL,
    value_date datetime NULL,
    value_flag bit NULL,
    CONSTRAINT PK_BPAStatistics_new primary key (sessionid, name)
);
insert into BPAStatistics_new (sessionid, name, datatype, value_text, value_number, value_date, value_flag) 
    select sessionid, name, datatype, value_text, value_number, value_date, value_flag from BPAStatistics;
drop table BPAStatistics;
exec sp_rename 'BPAStatistics_new', 'BPAStatistics';
exec sp_rename 'PK_BPAStatistics_new', 'PK_BPAStatistics';

ALTER TABLE BPATag ALTER COLUMN tag nvarchar(255);

ALTER TABLE BPAValCategory ALTER COLUMN description nvarchar(255);

GO

ALTER TABLE BPAWorkQueueLog ALTER COLUMN keyvalue nvarchar(255);

ALTER TABLE BPAValAction ALTER COLUMN description nvarchar(255);

ALTER TABLE BPAWebService ALTER COLUMN servicename nvarchar(128);
ALTER TABLE BPAWebService ALTER COLUMN url nvarchar(2083);
ALTER TABLE BPAWebService ALTER COLUMN wsdl nvarchar(max);
ALTER TABLE BPAWebService ALTER COLUMN settingsXML nvarchar(max);

ALTER TABLE BPAValType ALTER COLUMN description  nvarchar(255);

exec bpa_sp_dropunique 'BPAWorkQueueFilter', 'FilterName';
ALTER TABLE BPAWorkQueueFilter ALTER COLUMN FilterName nvarchar(32);
ALTER TABLE BPAWorkQueueFilter ALTER COLUMN FilterXML nvarchar(max);
ALTER TABLE BPAWorkQueueFilter ADD CONSTRAINT UNQ_BPAWorkQueueFilter_FilterName unique (FilterName);

GO

ALTER TABLE BPAWorkQueue ALTER COLUMN name nvarchar(255);
ALTER TABLE BPAWorkQueue ALTER COLUMN keyfield nvarchar(255);
if not exists (select 1 from BPADBVersion where dbversion='193') exec('
ALTER TABLE BPAWorkQueue ALTER COLUMN encryptname nvarchar(255);');

ALTER TABLE BPAValCheck ALTER COLUMN description nvarchar(255);

ALTER TABLE BPATask ALTER COLUMN name nvarchar(128);
ALTER TABLE BPATask ALTER COLUMN description nvarchar(max);

ALTER TABLE BPATaskSession ALTER COLUMN processparams nvarchar(max);
ALTER TABLE BPATaskSession ALTER COLUMN resourcename nvarchar(128);

GO

ALTER TABLE BPAAlertEvent ALTER COLUMN Message nvarchar(500);

exec bpa_sp_dropdefault 'BPASysConfig', 'ActiveDirectoryProvider';
exec bpa_sp_dropdefault 'BPASysConfig', 'ArchivingFolder';
exec bpa_sp_dropdefault 'BPASysConfig', 'ArchivingAge';
ALTER TABLE BPASysConfig ALTER COLUMN maxnumconcproc nvarchar(100);
ALTER TABLE BPASysConfig ALTER COLUMN ArchiveInProgress nvarchar(20);
ALTER TABLE BPASysConfig ALTER COLUMN LicenseKey nvarchar(max);
ALTER TABLE BPASysConfig ALTER COLUMN ActiveDirectoryProvider nvarchar(max);
if not exists (select 1 from BPADBVersion where dbversion='193') exec('
ALTER TABLE BPASysConfig ALTER COLUMN credentialkey nvarchar(50);');
ALTER TABLE BPASysConfig ALTER COLUMN ArchivingFolder nvarchar(max);
ALTER TABLE BPASysConfig ALTER COLUMN ArchivingAge nvarchar(max);
ALTER TABLE BPASysConfig ADD DEFAULT ('') FOR ActiveDirectoryProvider;
ALTER TABLE BPASysConfig ADD DEFAULT ('') FOR ArchivingFolder;
ALTER TABLE BPASysConfig ADD DEFAULT ('6m') FOR ArchivingAge;

CREATE TABLE BPAAliveResources_new (
    MachineName nvarchar(16) NOT NULL,
    UserID uniqueidentifier NOT NULL,
    LastUpdated datetime NOT NULL,
    CONSTRAINT PK_BPAAliveResources_new PRIMARY KEY CLUSTERED 
    (MachineName, UserID)
);
INSERT INTO BPAAliveResources_new (MachineName,UserID,LastUpdated)
    SELECT MachineName,UserID,LastUpdated FROM BPAAliveResources;
drop table BPAAliveResources;
exec sp_rename 'BPAAliveResources_new', 'BPAAliveResources';
exec sp_rename 'PK_BPAAliveResources_new', 'PK_BPAAliveResources';

exec bpa_sp_dropunique 'BPACalendar', 'name';
ALTER TABLE BPACalendar ALTER COLUMN name nvarchar(128);
ALTER TABLE BPACalendar ALTER COLUMN description nvarchar(max);
ALTER TABLE BPACalendar ADD CONSTRAINT UNQ_BPACalendar_name unique (name);

GO

ALTER TABLE BPAOldPassword ALTER COLUMN password nvarchar(128);

ALTER TABLE BPAPref DROP CONSTRAINT UNQ_BPAPref_name_userid;
ALTER TABLE BPAPref ALTER COLUMN name nvarchar(255);
ALTER TABLE BPAPref ADD constraint UNQ_BPAPref_name_userid
    unique (name, userid);

ALTER TABLE BPARelease DROP CONSTRAINT UNQ_BPARelease_packageid_name;
ALTER TABLE BPARelease ALTER COLUMN name nvarchar(255);
ALTER TABLE BPARelease ALTER COLUMN notes nvarchar(max);
ALTER TABLE BPARelease ADD  constraint UNQ_BPARelease_packageid_name
    unique (packageid, name);

ALTER TABLE BPARecent ALTER COLUMN name nvarchar(128);

ALTER TABLE BPAScheduleLog ALTER COLUMN servername nvarchar(255);

GO

DROP Index IX_BPAScheduleLogEntry_logid_entrytype ON BPAScheduleLogEntry;
ALTER TABLE BPAScheduleLogEntry ALTER COLUMN terminationreason nvarchar(255);
ALTER TABLE BPAScheduleLogEntry ALTER COLUMN stacktrace nvarchar(max);
-- Note that this index was previously left out when on SQL Server 2000, but
-- as that's no longer supported we don't need to check for it here.
create index IX_BPAScheduleLogEntry_logid_entrytype
    on BPAScheduleLogEntry(schedulelogid, entrytype)
    include (entrytime, terminationreason);

ALTER TABLE BPAScenarioDetail ALTER COLUMN testtext nvarchar(1000);

ALTER TABLE BPAReleaseEntry ALTER COLUMN typekey nvarchar(64);
ALTER TABLE BPAReleaseEntry ALTER COLUMN entityid nvarchar(255);
ALTER TABLE BPAReleaseEntry ALTER COLUMN name nvarchar(255);

GO

ALTER TABLE BPAStringPref ALTER COLUMN value nvarchar(max);

exec bpa_sp_dropdefault 'BPAWorkQueueItem', 'status';
DROP INDEX Index_BPAWorkQueueItem_exceptionreasonvarchar ON BPAWorkQueueItem;
DROP INDEX Index_BPAWorkQueueItem_exceptionreasontag ON BPAWorkQueueItem;
ALTER TABLE BPAWorkQueueItem DROP COLUMN exceptionreasonvarchar;
ALTER TABLE BPAWorkQueueItem DROP COLUMN exceptionreasontag;
ALTER TABLE BPAWorkQueueItem ALTER COLUMN keyvalue nvarchar(255);
ALTER TABLE BPAWorkQueueItem ALTER COLUMN status nvarchar(255);
ALTER TABLE BPAWorkQueueItem ALTER COLUMN exceptionreason nvarchar(max);
ALTER TABLE BPAWorkQueueItem ALTER COLUMN data nvarchar(max);
ALTER TABLE BPAWorkQueueItem ADD  DEFAULT ('') FOR status;
ALTER TABLE BPAWorkQueueItem ADD exceptionreasonvarchar AS
    (CONVERT(nvarchar(400),exceptionreason));
ALTER TABLE BPAWorkQueueItem ADD exceptionreasontag AS
    (CONVERT(nvarchar(415),N'Exception: '+replace(CONVERT(nvarchar(400),exceptionreason),N';',N':'))) PERSISTED;
create index Index_BPAWorkQueueItem_exceptionreasonvarchar
  on BPAWorkQueueItem(exceptionreasonvarchar);
create index Index_BPAWorkQueueItem_exceptionreasontag
  on BPAWorkQueueItem(exceptionreasontag);

GO

CREATE TABLE BPADBVersion_new (
    dbversion nvarchar(50) NOT NULL,
    scriptrundate datetime NULL,
    scriptname nvarchar(50) NULL,
    description nvarchar(200) NULL,
    CONSTRAINT PK_BPADBVersion_new PRIMARY KEY CLUSTERED 
    (dbversion)
);
INSERT INTO BPADBVersion_new (dbversion, scriptrundate, scriptname, description)
    SELECT dbversion, scriptrundate, scriptname, description FROM BPADBVersion;
drop table BPADBVersion;
exec sp_rename 'BPADBVersion_new', 'BPADBVersion';
exec sp_rename 'PK_BPADBVersion_new', 'PK_BPADBVersion';

GO

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '182',
  GETUTCDATE(),
  'db_upgradeR182.sql UTC',
  'Switch to full unicode support'
);

