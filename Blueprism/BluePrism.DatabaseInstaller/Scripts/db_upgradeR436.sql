/*
	SCRIPT         : 436
	AUTHOR         : Ian Guthrie
	PURPOSE        : Extra setting for ASCR
*/

declare @prefstringpingtime varchar(50) = 'resourceconnection.connectionpingtime';


declare @prefcreatesessionsleepinmilliseconds varchar(50) = 'scheduler.createsessionsleepinmilliseconds'
declare @prefenablerunmodecache varchar(50) = 'resourceconnection.listener.enablerunmodecache'


update BPAIntegerPref 
set value = 60 
where prefid = (select Top(1) Id from BPAPref where name = @prefstringpingtime )


if not exists(select 1
from BPAPref
where [name] = @prefcreatesessionsleepinmilliseconds and userid is null)
begin
insert into [BPAPref] ([name],[userid]) values(@prefcreatesessionsleepinmilliseconds, null);
insert into[BPAIntegerPref] ([prefid],[value]) values(SCOPE_IDENTITY(), 50);
end


if not exists(select 1
from BPAPref
where [name] = @prefenablerunmodecache and userid is null)
begin
insert into [BPAPref] ([name],[userid]) values(@prefenablerunmodecache, null);
insert into[BPAIntegerPref] ([prefid],[value]) values(SCOPE_IDENTITY(), 1);
end


insert into BPADBVersion
(dbversion,
scriptrundate,
scriptname,
[description],
timezoneoffset
)
values
('436',
GETUTCDATE(),
'db_upgradeR436.sql',
'Extra setting and updates for ASCR',
0
);
