/*
SCRIPT         : 345
AUTHOR         : Gary Chadwick
PURPOSE        : Rename index as not clear which column it is related to
*/

EXEC sp_rename N'BPAWorkQueue.Index_name','INDEX_WorkQueueName', N'INDEX'

-- Set DB version.
insert into BPADBVersion (
	dbversion
	,scriptrundate
	,scriptname
	,[description]
	,timezoneoffset
	)
values (
	'345'
	,getutcdate()
	,'db_upgradeR345.sql'
	,'Rename index as not clear which column it is related to'
	,0
	);