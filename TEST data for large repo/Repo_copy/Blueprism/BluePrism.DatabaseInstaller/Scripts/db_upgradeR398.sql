/*
SCRIPT        : 398
STORY		  : BP-669
PURPOSE       : Added timezoneId to BPAScheduleTrigger
AUTHOR		  : Gary Crosbie
*/

if not exists ( select 1
                from INFORMATION_SCHEMA.columns
                where table_name = 'BPAScheduleTrigger' and column_name = 'timezoneId')
begin
    alter table BPAScheduleTrigger add timezoneId nvarchar(255) null;
end
go
if not exists ( select 1
                from INFORMATION_SCHEMA.columns
                where table_name = 'BPAScheduleTrigger' and column_name = 'utcoffset')
begin
    alter table BPAScheduleTrigger add utcoffset int null;
end
go

-- Set DB version.
insert into BPADBVersion (
	dbversion
	,scriptrundate
	,scriptname
	,description
	,timezoneoffset
	)
values (
	'398'
	,getutcdate()
	,'db_upgradeR398.sql'
	,'Added timezoneId to BPAScheduleTrigger'
	,0
    );
