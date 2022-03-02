/*
STORY:     BP-2912
PURPOSE:   Add deletedLastSynchronizationDate column to BPAUser table
*/

alter table BPAUser
add deletedLastSynchronizationDate datetimeoffset null

-- Set DB version.
insert into BPADBVersion
(dbversion, 
 scriptrundate, 
 scriptname, 
 [description], 
 timezoneoffset
)
values
('427',
 getutcdate(), 
 'db_upgradeR427.sql', 
 'Add deletedLastSynchronizationDate column to BPAUser table', 
 0
);
