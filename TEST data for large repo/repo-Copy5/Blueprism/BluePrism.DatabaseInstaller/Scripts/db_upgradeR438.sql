/*
SCRIPT         : 438
PURPOSE        : Add authServerName column
*/

alter table BPAUser
add authServerName nvarchar(max);
go;

---- Set DB version.
insert into BPADBVersion
(dbversion, 
 scriptrundate, 
 scriptname, 
 [description], 
 timezoneoffset
)
values
('438', 
 GETUTCDATE(), 
 'db_upgradeR438.sql', 
 'Add authServerName column', 
 0
);
