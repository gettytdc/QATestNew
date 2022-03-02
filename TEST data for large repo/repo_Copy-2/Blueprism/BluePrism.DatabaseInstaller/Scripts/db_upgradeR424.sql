/*
SCRIPT         : 424
AUTHOR         : Adrian Vaughan
PURPOSE        : Make way for v7 licences by clearing out old ones.
*/

delete from BPALicenseActivationRequest
delete from BPAlicense
go


-- Set DB version.
insert into BPADBVersion
(dbversion, 
 scriptrundate, 
 scriptname, 
 [description], 
 timezoneoffset
)
values
('424', 
 GETUTCDATE(), 
 'db_upgradeR424.sql', 
 'Make way for v7 licences by clearing out old ones.', 
 0
);
