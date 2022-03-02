/*
SCRIPT         : 407
PURPOSE        : Insert a new integer preference to specify the timeout of a user token used by app-server-controlled-robots
*/

declare @prefstring varchar(50) = 'resourceconnection.tokentimeout';

if not exists (select 1 
               from BPAPref 
               where [name] = @prefstring and userid is null)
begin

insert into [BPAPref] ([name],[userid]) values (@prefstring,null); 
insert into [BPAIntegerPref] ([prefid],[value]) values (SCOPE_IDENTITY(),10);

end

-- set db version.
insert into bpadbversion (
	dbversion
	,scriptrundate
	,scriptname
	,[description]
	,timezoneoffset
	)
values (
	'407'
	,getutcdate()
	,'db_upgradeR407.sql'
	,'Insert a new integer preference to specify the timeout of a user token used by app-server-controlled-robots'
	,0
	);
