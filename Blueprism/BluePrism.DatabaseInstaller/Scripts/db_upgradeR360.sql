/*
SCRIPT         : 360
PURPOSE        : Add new datapipeline setting for server port.
AUTHOR         : Ian Guthrie
*/
 
 if not exists ( select 1
                from INFORMATION_SCHEMA.columns
                where table_name ='BPADataPipelineSettings' and column_name ='serverPort')
begin
alter table BPADataPipelineSettings add serverPort Int NOT NULL default(1433)
end
go



-- Set DB version.
insert into BPADBVersion
(dbversion, 
 scriptrundate, 
 scriptname, 
 [description], 
 timezoneoffset
)
values
('360', 
 GETUTCDATE(), 
 'db_upgradeR360.sql', 
 'Add new BPADataPipelineSettings serverport column', 
 0
);