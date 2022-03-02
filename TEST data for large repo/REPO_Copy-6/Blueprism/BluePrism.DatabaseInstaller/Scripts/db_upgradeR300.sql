CREATE TABLE BPADataPipelineSettings
(
    id int not null,
    writesessionlogstodatabase bit not null,
    emitsessionlogstodatagateways bit not null,
    monitoringfrequency int not null,
 constraint PK_BPADataPipelineSettings primary key clustered (id)
 )

insert into BPADataPipelineSettings ([id], [writesessionlogstodatabase], [emitsessionlogstodatagateways], [monitoringfrequency])
    values (1,1,0,5);

-- Set DB version.
INSERT INTO BPADBVersion(dbversion, 
                         scriptrundate, 
                         scriptname, 
                         [description], 
                         timezoneoffset)
VALUES('300',
       getutcdate(),
       'db_upgradeR300.sql',
       'Add data pipeline config tables.',
       0);