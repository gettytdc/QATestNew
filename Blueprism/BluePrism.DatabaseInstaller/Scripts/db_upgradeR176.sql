/*
SCRIPT         : 176
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : CEG
PURPOSE        : Add new tables for Dependency Tracking
RELATED BUGS   : 4305
*/

--Add dependency state indicator (defaulting to Valid)
alter table BPASysConfig add
    DependencyState int default 2 not null;
GO
--Where processes exist at time of upgrade, mark dependencies as Invalid
update BPASysConfig set DependencyState=0 where (select COUNT(processid) from BPAProcess) > 0;

--Add denormalised info to process table
alter table BPAProcess add
    runmode int default 0 not null;

--Create process dependency table
create table BPAProcessIDDependency (
    id int identity not null,
    processID uniqueidentifier not null constraint FK_BPAProcessIDDependency_BPAProcess
     foreign key references BPAProcess(processid) on delete cascade,
    refProcessID uniqueidentifier not null,
    constraint PK_BPAProcessIDDependency primary key (id)
);

--Create object dependency table
create table BPAProcessNameDependency (
    id int identity not null,
    processID uniqueidentifier not null constraint FK_BPAProcessNameDependency_BPAProcess
     foreign key references BPAProcess(processid) on delete cascade,
    refProcessName nvarchar(128) not null,
    constraint PK_BPAProcessNameDependency primary key (id)
);

--Create object action dependency table
create table BPAProcessActionDependency (
    id int identity not null,
    processID uniqueidentifier not null constraint FK_BPAProcessActionDependency_BPAProcess
     foreign key references BPAProcess(processid) on delete cascade,
    refProcessName nvarchar(128) not null,
    refActionName nvarchar(max) not null,
    constraint PK_BPAProcessActionDependency primary key (id)
);

--Create model element dependency table
create table BPAProcessElementDependency (
    id int identity not null,
    processID uniqueidentifier not null constraint FK_BPAProcessElementDependency_BPAProcess
     foreign key references BPAProcess(processid) on delete cascade,
    refProcessName nvarchar(128) not null,
    refElementID uniqueidentifier not null,
    constraint PK_BPAProcessElementDependency primary key (id)
);

--Create process web service table
create table BPAProcessWebServiceDependency (
    id int identity not null,
    processID uniqueidentifier not null constraint FK_BPAProcessWebServiceDependency_BPAProcess
     foreign key references BPAProcess(processid) on delete cascade,
    refServiceName nvarchar(128) not null,
    constraint PK_BPAProcessWebServiceDependency primary key (id)
);

--Create work queue dependency table
create table BPAProcessQueueDependency (
    id int identity not null,
    processID uniqueidentifier not null constraint FK_BPAProcessQueueDependency_BPAProcess
     foreign key references BPAProcess(processid) on delete cascade,
    refQueueName nvarchar(255) not null,
    constraint PK_BPAProcessQueueDependency primary key (id)
);

--Create credentials dependency table
create table BPAProcessCredentialsDependency (
    id int identity not null,
    processID uniqueidentifier not null constraint FK_BPAProcessCredentialsDependency_BPAProcess
     foreign key references BPAProcess(processid) on delete cascade,
    refCredentialsName nvarchar(255) not null,
    constraint PK_BPAProcessCredentialsDependency primary key (id)
);

--Create environment variable dependency table
create table BPAProcessEnvironmentVarDependency (
    id int identity not null,
    processID uniqueidentifier not null constraint FK_BPAProcessEnvironmentVarDependency_BPAProcess
     foreign key references BPAProcess(processid) on delete cascade,
    refVariableName nvarchar(64) not null,
    constraint PK_BPAProcessEnvironmentVarDependency primary key (id)
);

--Create calendar dependency table
create table BPAProcessCalendarDependency (
    id int identity not null,
    processID uniqueidentifier not null constraint FK_BPAProcessCalendarDependency_BPAProcess
     foreign key references BPAProcess(processid) on delete cascade,
    refCalendarName nvarchar(128) not null,
    constraint PK_BPAProcessCalendarDependency primary key (id)
);

--Create font dependency table
create table BPAProcessFontDependency (
    id int identity not null,
    processID uniqueidentifier not null constraint FK_BPAProcessFontDependency_BPAProcess
     foreign key references BPAProcess(processid) on delete cascade,
    refFontName nvarchar(255) not null,
    constraint PK_BPAProcessFontDependency primary key (id)
);

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '176',
  GETUTCDATE(),
  'db_upgradeR176.sql UTC',
  'Add new tables for Dependency Tracking'
);
