/*
SCRIPT         : 365
PURPOSE        : Add indexes to optimise query which gets all logged in users
*/

create index IX_BPAUser_useridUsername ON bpauser([userid],[username]) where [username] is not null 
go

create index IX_BPAAliveResouces_userid on BPAaliveresources([userid])
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
	'365'
	,getutcdate()
	,'db_upgradeR365.sql'
	,'Add indexes IX_BPAUser_useridUsername and IX_BPAAliveResouces_userid to optimise query to return all logged in users'
	,0
	);