/*
SCRIPT        : 366
PURPOSE       : Add utcoffset column to BPAScheduleTrigger to ensure trigger timezone can be calculated
AUTHOR		  : Lee Allan

Note : Work for Schedule changes has now been reverted.  (us-8229)

*/


-- Set DB version.
insert into BPADBVersion (
	dbversion
	,scriptrundate
	,scriptname
	,[description]
	,timezoneoffset
	)
values (
	'366'
	,getutcdate()
	,'db_upgradeR366.sql'
	,'Content Reverted'
	,0
	);