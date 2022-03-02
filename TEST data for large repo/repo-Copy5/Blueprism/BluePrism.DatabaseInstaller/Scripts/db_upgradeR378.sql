/*
SCRIPT		: 378
STORY		: us-8552
PURPOSE		: Add new table BPAAliveAutomateC
AUTHOR		: Rowland Hill
*/


if not exists (select * from sysobjects where name = N'BPAAliveAutomateC')
begin
	create table [BPAAliveAutomateC](
		[MachineName] [nvarchar](128) not null,
		[UserID] [uniqueidentifier] not null,
		[LastUpdated] [datetime] not null,
			 constraint [PK_BPAAliveAutomateC] primary key clustered 
			 (
				[MachineName] asc,
				[UserID] asc
			 )
	)
end
go



-- set db version.
insert into bpadbversion (
	dbversion
	,scriptrundate
	,scriptname
	,[description]
	,timezoneoffset
	)
values (
	'378'
	,getutcdate()
	,'db_upgradeR378.sql'
	,'Add new table BPAAliveAutomateC'
	,0
	);
go
