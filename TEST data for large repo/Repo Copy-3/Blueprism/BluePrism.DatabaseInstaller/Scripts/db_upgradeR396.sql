/*
SCRIPT         : 396
AUTHOR         : Ian Guthrie
PURPOSE        : Removed unused table (BPARecent) and view (BPVGroupTree)
*/

drop table BPARecent
drop view BPVGroupTree

-- Set DB version.
insert into BPADBVersion
(dbversion, 
 scriptrundate, 
 scriptname, 
 [description], 
 timezoneoffset
)
values
('396', 
 GETUTCDATE(), 
 'db_upgradeR396.sql', 
 'Removed unused table (BPARecent) and view (BPVGroupTree)', 
 0
);
