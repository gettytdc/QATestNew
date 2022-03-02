/*
SCRIPT         : 356
PURPOSE        : Add new column for digital worker session id
*/

alter table BPAEnvLock add
    digitalworkersessionid uniqueidentifier null
GO

-- Set DB version.
insert into BPADBVersion
(dbversion, 
 scriptrundate, 
 scriptname, 
 [description], 
 timezoneoffset
)
values
('356', 
 GETUTCDATE(), 
 'db_upgradeR356.sql', 
 'Add new column for digital worker session id to BPAEnvLock', 
 0
);