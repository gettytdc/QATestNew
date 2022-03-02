/*
SCRIPT        : 373
PURPOSE       : Add currentculture to BPAResource to note what culture the resource has been started as
AUTHOR		  : Gareth Davidson
*/

alter table [bparesource] add [currentculture] nvarchar(max) null

-- set db version.
insert into bpadbversion (
	dbversion
	,scriptrundate
	,scriptname
	,[description]
	,timezoneoffset
	)
values (
	'373'
	,getutcdate()
	,'db_upgradeR373.sql'
	,'Add currentculture to BPAResource to note what culture the resource has been started as'
	,0
	);