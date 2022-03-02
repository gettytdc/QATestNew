/*
SCRIPT         : 349
PURPOSE        : Create new table BPAMappedActiveDirectoryUser and create stored procedure usp_getmappedadusers
*/
create table BPAMappedActiveDirectoryUser
( bpuserid uniqueidentifier not null constraint FK_BPAMappedActiveDirectoryUser_BPAUser foreign key references BPAUser (userid),
  sid nvarchar(256) not null
);
go 

alter table BPAMappedActiveDirectoryUser
add constraint UNQ_sid unique (sid);
go

alter table BPAMappedActiveDirectoryUser
add constraint UNQ_bpuserid unique (bpuserid);
go

create index IX_BPAMappedActiveDirectoryUser_sid on BPAMappedActiveDirectoryUser(sid)
go

create type ActiveDirectoryUserTableType as table  
    ( securityidentifier nvarchar(256) );
go
​
create procedure usp_getmappedadusers
    (@tvpActiveDirectoryUsers ActiveDirectoryUserTableType READONLY) 
as
​
select 
    adusers.securityidentifier
from 
    @tvpActiveDirectoryUsers adusers
left join 
    BPAMappedActiveDirectoryUser mappedusers 
        on adusers.securityidentifier = mappedusers.sid
where 
    mappedusers.bpuserid is not null;
go

-- Set DB version.
insert into BPADBVersion (
	dbversion
	,scriptrundate
	,scriptname
	,[description]
	,timezoneoffset
	)
values (
	'349'
	,getutcdate()
	,'db_upgradeR349.sql'
	,'Create new table BPAMappedActiveDirectoryUser and create stored procedure usp_getmappedadusers'
	,0
	);