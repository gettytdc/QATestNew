/*
SCRIPT         : 363
AUTHOR         : Ian Guthrie
PURPOSE        : Add further data gateways config.
*/

alter table BPADataPipelineSettings add databaseusercredentialname nvarchar(64) null;
alter table BPADataPipelineSettings add useIntegratedSecurity bit not null default(0);
alter table BPADataPipelineOutputConfig add selectedsessionlogfields [nvarchar](max) NULL;

-- Set DB version.
insert into BPADBVersion(dbversion, 
                         scriptrundate, 
                         scriptname, 
                         [description], 
                         timezoneoffset)
values('363',
       getutcdate(),
       'db_upgradeR363.sql',
       'Add further data gateways config.',
       0);

