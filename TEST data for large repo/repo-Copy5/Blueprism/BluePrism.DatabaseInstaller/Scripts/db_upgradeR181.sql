/*
SCRIPT         : 181
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : GM
PURPOSE        : Adds additional process validation checks
*/

insert into BPAValCheck (checkid, catid, typeid, description, enabled)
    values (138, 0, 2, 'Data item ''{0}'' is not referenced', 1);

insert into BPAValCheck (checkid, catid, typeid, description, enabled)
    values (139, 0, 2, 'Collection ''{0}'' is not referenced', 1);

insert into BPAValCheck (checkid, catid, typeid, description, enabled)
    values (140, 0, 2, 'Application element ''{0}'' is not referenced', 1);

update BPAPermGroup set name='Dashboard' where name='Review';

--Create parent object dependency table
create table BPAProcessParentDependency (
    id int identity not null,
    processID uniqueidentifier not null constraint FK_BPAProcessParentDependency_BPAProcess
     foreign key references BPAProcess(processid) on delete cascade,
    refParentName nvarchar(128) not null,
    constraint PK_BPAProcessParentDependency primary key (id)
);

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '181',
  GETUTCDATE(),
  'db_upgradeR181.sql UTC',
  'Adds additional process validation checks'
);
