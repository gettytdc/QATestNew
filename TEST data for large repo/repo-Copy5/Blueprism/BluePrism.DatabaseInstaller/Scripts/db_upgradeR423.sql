/*
SCRIPT         : 423
AUTHOR         : Ian Guthrie
PURPOSE        : Adds 2 new configuration parameters to control access to resources in control room.
*/

declare @prefenvironmentrobotslowthreshold varchar(50) = 'resourceconnection.environmentrobotslowthreshold'
declare @prefenvironmentrobotshighthreshold varchar(50) = 'resourceconnection.environmentrobotshighthreshold'



if not exists (select 1 
               from BPAPref 
               where [name] = @prefenvironmentrobotslowthreshold and userid is null)
begin
	insert into [BPAPref] ([name],[userid]) values (@prefenvironmentrobotslowthreshold,null); 
	insert into [BPAIntegerPref] ([prefid],[value]) values (SCOPE_IDENTITY(),200);
end


if not exists (select 1 
               from BPAPref 
               where [name] = @prefenvironmentrobotshighthreshold and userid is null)
begin
	insert into [BPAPref] ([name],[userid]) values (@prefenvironmentrobotshighthreshold,null); 
	insert into [BPAIntegerPref] ([prefid],[value]) values (SCOPE_IDENTITY(),800);
end


-- Set DB version.
insert into BPADBVersion
(dbversion, 
 scriptrundate, 
 scriptname, 
 [description], 
 timezoneoffset
)
values
('423', 
 GETUTCDATE(), 
 'db_upgradeR423.sql', 
 'Adds 2 new configuration parameters to control access to resources in control room', 
 0
);