/*
SCRIPT         : 402
PURPOSE        : Insert a new integer preference dictating the type of protocol for app-server-controlled-robots
*/

declare @prefstring varchar(50) = 'resourceconnection.resourcecallbackprotocol';

if not exists (select 1 
               from BPAPref 
               where [name] = @prefstring and userid is null)
begin

insert into [BPAPref] ([name],[userid]) values (@prefstring,null); 
insert into [BPAIntegerPref] ([prefid],[value]) values (SCOPE_IDENTITY(),1);

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
	'402'
	,getutcdate()
	,'db_upgradeR402.sql'
	,'Insert a new integer preference dictating the type of protocol for app-server-controlled-robots'
	,0
	);
