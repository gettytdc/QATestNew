/*
SCRIPT		: 392
STORY		: bp-1745
PURPOSE		: Add a new integer preference
AUTHOR		: Euan Jones
*/

declare @preferenceString as varchar(44) = 'system.settings.maxappserverrobotconnections';

if not exists (
    select 1
    from BPAPref
    where [name] = @preferenceString and userid is null
)
begin

insert into BPAPref (name, userid) values (@preferenceString, null);
insert into BPAIntegerPref (prefid, value) values (SCOPE_IDENTITY(), 2000);

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
	'392'
	,getutcdate()
	,'db_upgradeR392.sql'
	,'Add a new integer preference'
	,0
	);
go
