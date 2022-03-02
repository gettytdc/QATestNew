/*
SCRIPT         : 441
AUTHOR         : Adam Price
PURPOSE        : Set Session Management enforces permissions to on by default
*/

delete from bpaintegerpref where prefid = (select id from bpapref where name = 'enforce.controlling.permission');
delete from bpapref where name = 'enforce.controlling.permission'
insert into [BPAPref] ([name],[userid]) VALUES ('enforce.controlling.permission',null);
insert into [BPAIntegerPref] ([prefid],[value]) VALUES(SCOPE_IDENTITY(), 1);
          
---- Set DB version.
insert into BPADBVersion 
	(dbversion, 
	scriptrundate,
	scriptname,
	[description],
	timezoneoffset
)
values 
	('441', 
	GETUTCDATE(), 
	'db_upgradeR441.sql', 
	'Set Session Management enforces permissions to on by default',
	0
);
