/*
STORY:      BP-3544
PURPOSE:   Add authenticationServerClientId column to BPAUser table

*/

alter table BPAUser
add authenticationServerClientId nvarchar(max) null

-- Set DB version.
insert into BPADBVersion
(dbversion, 
 scriptrundate, 
 scriptname, 
 [description], 
 timezoneoffset
)
values
('419',
 getutcdate(), 
 'db_upgradeR419.sql', 
 'Add authenticationServerClientId column to BPAUser table', 
 0
);
