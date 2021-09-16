-- Set DB version.
insert into BPADBVersion(dbversion, 
                         scriptrundate, 
                         scriptname, 
                         [description], 
                         timezoneoffset)
values('326',
       getutcdate(),
       'db_upgradeR326.sql',
       'removed',
       0);
go
