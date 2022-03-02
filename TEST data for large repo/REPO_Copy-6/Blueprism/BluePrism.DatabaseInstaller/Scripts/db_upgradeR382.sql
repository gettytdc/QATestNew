/*
SCRIPT		: 382
STORY		: us-9878
PURPOSE		: Rename Authentication Server columns to Authentication Gateway
AUTHOR		: Brett Hewitt
*/

exec sp_rename '[BPASysConfig].[authenticationserverurl]', 'authenticationgatewayurl', 'COLUMN';
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
	'382'
	,getutcdate()
	,'db_upgradeR382.sql'
	,'Rename authenticationserverurl column to uthenticationgatewayurl'
	,0
	);
go
