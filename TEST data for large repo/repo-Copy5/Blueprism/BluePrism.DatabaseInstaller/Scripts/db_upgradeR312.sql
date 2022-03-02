

alter table BPADataPipelineProcessConfig add iscustom bit NOT NULL default 0;


-- Set DB version.
insert into BPADBVersion(dbversion, 
                         scriptrundate, 
                         scriptname, 
                         [description], 
                         timezoneoffset)
values('312',
       getutcdate(),
       'db_upgradeR312.sql',
       'Add iscustom column to BPADataPipelineProcessConfig table',
       0);
