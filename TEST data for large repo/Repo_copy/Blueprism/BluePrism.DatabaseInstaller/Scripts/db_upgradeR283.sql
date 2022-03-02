-- This script has been removed as part of reverting the GetNextItem changes in
-- branch 'us3504-revert_getnextitem_changes'

-- Set DB version.
insert into BPADBVersion(dbversion, 
                         scriptrundate, 
                         scriptname, 
                         [description], 
                         timezoneoffset)
values('283',
       getutcdate(),
       'db_upgradeR283.sql',
       'Make changes to usp_getnextcase to fix duplicate work queue items being retrieved. ',
       0);
