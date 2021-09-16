create table BPASysWebConnectionSettings(
    maxidletime int,
    connectionlimit int)
go

insert into BPASysWebConnectionSettings 
    values (5,2)
go 

create table BPASysWebUrlSettings(
    baseuri varchar(max) not null,
    connectionlimit int not null,
    connectiontimeout int null,
    maxidletime int not null)
go

insert into BPAPerm ([name]) values ('System - Web Connection Settings')
go

declare @permId as int
select @permId = id from bpaperm where [name] = 'System - Web Connection Settings'

insert into BPAPermGroupMember (permgroupid, permid) values (7, @permId)
insert into BPAUserRolePerm (userroleid, permid) values (1, @permId)
go

-- set DB version
insert into BPADBVersion values (
  '259',
  getutcdate(),
  'db_upgradeR259.sql',
  'Add new table for web connection settings. Add new associated permission and add to system group.',
  0 -- UTC
)