-- This is a placeholder script for database changes made
-- in the 6.4.2 patch.
-- Set DB version.
insert into BPADBVersion(dbversion, 
                         scriptrundate, 
                         scriptname, 
                         [description], 
                         timezoneoffset)
values('294',
       getutcdate(),
       'db_upgradeR294.sql',
       'Placeholder script for patch 6.4.2 changes.',
       0);
