/*
SCRIPT         : 440
AUTHOR         : Rob Cairns
PURPOSE        : Turn ASCR on by default
*/

declare @preferenceId int = (select id from BPAPref where name = 'system.settings.appserverresourceconnection') 

update BPAIntegerPref
set value = 1
where prefid = @preferenceId

---- Set DB version.
insert into BPADBVersion 
	(dbversion, 
	scriptrundate,
	scriptname,
	[description],
	timezoneoffset
)
values 
	('440', 
	GETUTCDATE(), 
	'db_upgradeR440.sql', 
	'Turn ASCR on by default',
	0
);
