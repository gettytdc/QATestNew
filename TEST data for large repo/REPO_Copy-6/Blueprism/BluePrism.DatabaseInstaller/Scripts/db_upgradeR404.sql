alter table BPASysConfig add
    authenticationserverurl nvarchar(2083) null;

go    
-- Set DB version.
insert into BPADBVersion(dbversion,
                         scriptrundate,
                         scriptname,
                         [description],
                         timezoneoffset)
values('404',
       getutcdate(),
       'db_upgradeR404.sql',
       'Add authenticationserverurl column to BPASysConfig',
       0);
