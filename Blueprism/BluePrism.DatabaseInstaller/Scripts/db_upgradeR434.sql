/*
STORY:     BP-3472
PURPOSE:   Add updatedLastSynchronizationDate column to BPAUser table
*/

alter table BPAUser
add updatedLastSynchronizationDate datetimeoffset null

-- Set DB version.
insert into BPADBVersion
(dbversion, 
 scriptrundate, 
 scriptname, 
 [description], 
 timezoneoffset
)
values
('434',
 getutcdate(), 
 'db_upgradeR434.sql', 
 'Add updatedLastSynchronizationDate column to BPAUser table', 
 0
);
