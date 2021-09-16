/*
STORY      : us-4106
PURPOSE    : Create new column to store send to work queue analysis to data gateways bool 
*/

-- Modify column
if not exists(select 1 from sys.columns 
          where name = N'sendworkqueueanalysistodatagateways'
          and object_id = object_id(N'BPADataPipelineSettings'))
begin
    alter table [BPADataPipelineSettings]
    add [sendworkqueueanalysistodatagateways] bit not null default 0
end
go

-- Set DB version.
insert into BPADBVersion(dbversion, 
                         scriptrundate, 
                         scriptname, 
                         [description], 
                         timezoneoffset)
values('324',
       getutcdate(),
       'db_upgradeR324sql',
       'Add extra column to BPADataPipelineSettings.',
       0);
go
