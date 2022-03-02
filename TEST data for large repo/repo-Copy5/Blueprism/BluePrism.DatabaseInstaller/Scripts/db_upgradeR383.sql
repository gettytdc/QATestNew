DECLARE @appserverresourceconnectionPreference VARCHAR(50) = 'system.settings.appserverresourceconnection';

IF NOT EXISTS (SELECT 1 
               FROM BPAPref 
               WHERE [name] = @appserverresourceconnectionPreference and userid is null)
BEGIN

insert into [BPAPref] ([name],[userid]) VALUES (@appserverresourceconnectionPreference,null); 
insert into [BPAIntegerPref] ([prefid],[value]) VALUES(SCOPE_IDENTITY(),0);


-- set db version.
insert into bpadbversion (
	dbversion
	,scriptrundate
	,scriptname
	,[description]
	,timezoneoffset
	)
values (
	'383'
	,getutcdate()
	,'db_upgradeR383.sql'
	,'db setting to enable resources to connect to via appserver, on demand connections'
	,0
	);

end
