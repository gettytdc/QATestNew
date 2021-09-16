/*
SCRIPT		: 379
STORY		: bg-7061
PURPOSE		: Add a new row to BPAValCheck
AUTHOR		: Euan Jones
*/

insert into BPAValCheck ([checkid], [catid], [typeid], [description], [enabled])
values
(148,1, 1, '''Batch Size'' input value is - {1}. {2}', 1)
GO

-- set db version.
insert into bpadbversion (
	dbversion
	,scriptrundate
	,scriptname
	,[description]
	,timezoneoffset
	)
values (
	'379'
	,getutcdate()
	,'db_upgradeR379.sql'
	,'Add new parameter to BPAValCheck Add To Queue batch size'
	,0
	);
go
