/*
BUG/STORY      : US-5969
PURPOSE        : Add EnableMappedActiveDirectoryAuth and EnableExternalAuth columns
*/


alter table BPASysConfig
add EnableMappedActiveDirectoryAuth bit default 0 not null;

alter table BPASysConfig
add EnableExternalAuth bit default 0 not null;
go

update BPASysConfig 
set EnableExternalAuth = case when AuthenticationServerUrl is not null and 
                                    len(ltrim(AuthenticationServerUrl)) > 0 
                              then 1 
                              else 0 
                         end;


-- Set DB version.
insert into BPADBVersion (
	dbversion
	,scriptrundate
	,scriptname
	,[description]
	,timezoneoffset
	)
values (
	'347'
	,getutcdate()
	,'db_upgradeR347.sql'
	,'Add EnableMappedActiveDirectoryAuth and EnableExternalAuth columns'
	,0
	);