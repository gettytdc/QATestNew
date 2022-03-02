/*
STORY:      BP-2601
PURPOSE:    Add flag that indicates that authenticaiton via the Authentication server is turned on for this environment

*/

alter table BPASysConfig
add enableauthenticationserverauth bit not null default 0;

-- Set DB version.
insert into BPADBVersion
(dbversion, 
 scriptrundate, 
 scriptname, 
 [description], 
 timezoneoffset
)
values
('414',
 getutcdate(), 
 'db_upgradeR414.sql', 
 'Add flag that indicates that authenticaiton via the Authentication server is turned on for this environment', 
 0
);
