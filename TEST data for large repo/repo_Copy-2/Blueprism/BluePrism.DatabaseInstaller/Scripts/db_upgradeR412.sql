/*
SCRIPT         : 407
PURPOSE        : Inserts integer preferences to control app-server-controlled-robots behaviour
*/

declare @prefstringpingtime varchar(50) = 'resourceconnection.connectionpingtime';
declare @prefstringsleeptime varchar(50) = 'resourceconnection.processresourceinputsleeptime'

if not exists (select 1 
               from BPAPref 
               where [name] = @prefstringpingtime and userid is null)
begin
insert into [BPAPref] ([name],[userid]) values (@prefstringpingtime,null); 
insert into [BPAIntegerPref] ([prefid],[value]) values (SCOPE_IDENTITY(),5);
end


if not exists (select 1 
               from BPAPref 
               where [name] = @prefstringsleeptime and userid is null)
begin
insert into [BPAPref] ([name],[userid]) values (@prefstringsleeptime,null); 
insert into [BPAIntegerPref] ([prefid],[value]) values (SCOPE_IDENTITY(),100);
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
	'412'
	,getutcdate()
	,'db_upgradeR412.sql'
	,'Insert integer preferences to control app-server-controlled-robots behaviour'
	,0
	);
