/*
SCRIPT         : 442
PURPOSE        : Delete redundant Decipher Roles and Permissions
*/

delete BPAPermGroupMember
from BPAPermGroupMember pgm
inner join BPAPermGroup pg on pgm.permgroupid = pg.id
where pg.requiredFeature = 'DocumentProcessing'
go

delete BPAUserRolePerm
from BPAUserRolePerm urp
inner join BPAPerm p on urp.permid = p.id
where p.requiredFeature = 'DocumentProcessing'
go

delete from BPAUserRole where requiredFeature = 'DocumentProcessing'
go

delete from BPAPerm where requiredFeature = 'DocumentProcessing'
go

delete from BPAPermGroup where requiredFeature = 'DocumentProcessing'
go

---- Set DB version.
insert into BPADBVersion
(dbversion, 
 scriptrundate, 
 scriptname, 
 [description], 
 timezoneoffset
)
values
('442', 
 GETUTCDATE(), 
 'db_upgradeR442.sql', 
 'Delete redundant Decipher Roles and Permissions', 
 0
);
