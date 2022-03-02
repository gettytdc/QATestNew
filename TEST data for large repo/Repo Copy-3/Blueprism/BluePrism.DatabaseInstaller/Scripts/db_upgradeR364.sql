/*
SCRIPT         : 364
PURPOSE        : Remove index and add composite primary key to BPAMappedActiveDirectoryUser table
*/

drop index IX_BPAMappedActiveDirectoryUser_sid on BPAMappedActiveDirectoryUser
go

alter table BPAMappedActiveDirectoryUser
add constraint PK_BPAMappedActiveDirectoryUser PRIMARY KEY CLUSTERED (bpuserid, sid)
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
	'364'
	,getutcdate()
	,'db_upgradeR364.sql'
	,'Remove index and add composite primary key to BPAMappedActiveDirectoryUser table'
	,0
	);