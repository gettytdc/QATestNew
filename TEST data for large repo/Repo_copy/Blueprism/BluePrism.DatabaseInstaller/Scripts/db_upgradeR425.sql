/*
SCRIPT         : 425
AUTHOR         : Rob Cairns
PURPOSE        : Remove the resourceconnection.resourcecallbackprotocol field fom BPAPref and BPAIntegerPref and turn on Ascr GRPC by default
*/

declare @prefstring varchar(50) = 'resourceconnection.resourcecallbackprotocol';
declare @prefid int;

select @prefid = id
from BPAPref
where @prefstring = name;

begin tran
delete from BPAIntegerPref where prefid = @prefid
delete from BPAPref where name = @prefstring
commit tran

-- Set DB version.
insert into BPADBVersion
(dbversion, 
 scriptrundate, 
 scriptname, 
 [description], 
 timezoneoffset
)
values
('425', 
 GETUTCDATE(), 
 'db_upgradeR425.sql', 
 'Remove the resourceconnection.resourcecallbackprotocol field fom BPAPref and BPAIntegerPref and turn on Ascr GRPC by default', 
 0
);
