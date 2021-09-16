-- Set DB version.
insert into BPADBVersion(dbversion, 
                         scriptrundate, 
                         scriptname, 
                         [description], 
                         timezoneoffset)
values('327',
       getutcdate(),
       'db_upgradeR327.sql',
       'removed',
       0);
go
