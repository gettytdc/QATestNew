/*
SCRIPT         : 135
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Gives WebService a primary key - introduces packages and releases
*/

-- For some reason, the web service has no primary key
-- Obviously, this means that a foreign key wouldn't work to it.
IF object_id('PK_BPAWebService') IS NULL
    alter table BPAWebService
        add constraint PK_BPAWebService
        primary key (serviceid)

-- Package - can be superseded by new versions.
create table BPAPackage (
    id int not null identity
        constraint PK_BPAPackage primary key,
    name varchar(255) not null
        constraint UNQ_BPAPackage_name unique,
    description text not null,
    userid uniqueidentifier not null,
    created datetime not null
)

/* Contents of a package:-
 * Process / Object
 * Work Queue
 * Credential
 * Schedule
 * Schedule List
 * <s>External Business Object</s> - not actually registered on DB
 * Web Service
 * Environment Variable
 */
create table BPAPackageProcess (
    packageid int not null
        constraint FK_BPAPackageProcess_BPAPackage
            foreign key references BPAPackage(id)
            on delete cascade,
    processid uniqueidentifier not null
        constraint FK_BPAPackageProcess_BPAProcess
            foreign key references BPAProcess(processid)
            on delete cascade,
    constraint PK_BPAPackageProcess
        primary key (packageid, processid)
)

create table BPAPackageProcessGroupMember (
    packageid int not null
        constraint FK_BPAPackageProcessGroup_BPAPackage
            foreign key references BPAPackage(id)
            on delete cascade,
    processgroupid uniqueidentifier not null,
    processid uniqueidentifier not null,
    -- Odd table this. Needs an FK to the process group member
    -- since it may or may not contain all the members of the
    -- group, but if a member is removed from a group, it must
    -- be removed from the package too.
    constraint FK_BPAPackageProcessGroup_BPAProcessGroupMembership
        foreign key (processgroupid, processid)
            references BPAProcessGroupMembership(GroupID,ProcessID)
            on delete cascade,
    -- We also need a link to the package-process. If a process
    -- is removed from a package, its group assignment(s) must be
    -- removed along with it.
    constraint FK_BPAPackageProcessGroupMember_BPAPackageProcess
        foreign key (packageid, processid)
            references BPAPackageProcess(packageid,processid),
    constraint PK_BPAPackageProcessGroup
        primary key (packageid, processgroupid, processid)
)

create table BPAPackageWorkQueue (
    packageid int not null
        constraint FK_BPAPackageWorkQueue_BPAPackage
            foreign key references BPAPackage(id)
            on delete cascade,
    queueident int not null
        constraint FK_BPAPackageWorkQueue_BPAWorkQueue
            foreign key references BPAWorkQueue(ident)
            on delete cascade,
    constraint PK_BPAPackageWorkQueue
        primary key (packageid, queueident)
)

create table BPAPackageCredential (
    packageid int not null
        constraint FK_BPAPackageCredential_BPAPackage
            foreign key references BPAPackage(id)
            on delete cascade,
    credentialid uniqueidentifier not null
        constraint FK_BPAPackageCredential_BPACredentials
            foreign key references BPACredentials(id)
            on delete cascade,
    constraint PK_BPAPackageCredential
        primary key (packageid, credentialid)
)

create table BPAPackageSchedule (
    packageid int not null
        constraint FK_BPAPackageSchedule_BPAPackage
            foreign key references BPAPackage(id)
            on delete cascade,
    scheduleid int not null
        constraint FK_BPAPackageSchedule_BPASchedule
            foreign key references BPASchedule(id)
            on delete cascade,
    constraint PK_BPAPackageSchedule
        primary key (packageid, scheduleid)
)

create table BPAPackageScheduleList (
    packageid int not null
        constraint FK_BPAPackageScheduleList_BPAPackage
            foreign key references BPAPackage(id)
            on delete cascade,
    schedulelistid int not null
        constraint FK_BPAPackageScheduleList_BPAScheduleList
            foreign key references BPAScheduleList(id)
            on delete cascade,
    constraint PK_BPAPackageScheduleList
        primary key (packageid, schedulelistid)
)

create table BPAPackageWebService (
    packageid int not null
        constraint FK_BPAPackageWebService_BPAPackage
            foreign key references BPAPackage(id)
            on delete cascade,
    webserviceid uniqueidentifier not null
        constraint FK_BPAPackageWebService_BPAWebService
            foreign key references BPAWebService(serviceid)
            on delete cascade,
    constraint PK_BPAPackageWebService
        primary key (packageid, webserviceid)
)

create table BPAPackageEnvironmentVar (
    packageid int not null
        constraint FK_BPAPackageEnvironmentVar_BPAPackage
            foreign key references BPAPackage(id)
            on delete cascade,
    name varchar(64) not null
        constraint FK_BPAPackageEnvironmentVar_BPAEnvironmentVar
            foreign key references BPAEnvironmentVar(name)
            on delete cascade
            on update cascade,
    constraint PK_BPAPackageEnvironmentVar
        primary key (packageid, name)
)

-- Releases, borne from packages
create table BPARelease (
    id int not null identity
        constraint PK_BPARelease primary key,
    packageid int not null /* The package this release was created from */
        constraint FK_BPARelease_BPAPackage
            foreign key references BPAPackage(id)
            on delete cascade,
    name varchar(255) not null,
    created datetime not null,
    userid uniqueidentifier not null
        constraint FK_BPARelease_BPAUser
            foreign key references BPAUser(userid),
    notes text not null,
    compressedxml image null,
    constraint UNQ_BPARelease_packageid_name
        unique (packageid, name)
)

-- Table containing the entries which form a release.
-- Note that these must be disassociated with the actual entities on
-- the database so that those entities can be freely deleted.
create table BPAReleaseEntry (
    id int not null identity
        constraint PK_BPAReleaseEntry primary key,
    releaseid int not null
        constraint FK_BPAReleaseEntry_BPARelease
        foreign key references BPARelease(id)
        on delete cascade,
    typekey varchar(64) not null,
    entityid varchar(255) not null, -- Could be integer, GUID or string
    name varchar(255) not null
)

--set DB version
INSERT INTO BPADBVersion VALUES (
  '135',
  GETUTCDATE(),
  'db_upgradeR135.sql UTC',
  'Gives WebService a primary key - introduces packages and releases'
)
