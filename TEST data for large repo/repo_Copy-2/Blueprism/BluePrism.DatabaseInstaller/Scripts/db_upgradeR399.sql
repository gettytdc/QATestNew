/*
SCRIPT         : 399
AUTHOR         : Gary Chadwick
PURPOSE        : Add permission for viewing resource details in control room
*/

declare @treeid int;
declare @permissionName VARCHAR(100) = 'View Resource Details'
select @treeid=id from BPATree where name='Resources';

declare @groupid int;
select @groupid=id from BPAPermGroup where name='Resources';

-- Add new/rename existing resource permissions
insert into BPAPerm (name, treeid) values (@permissionName, @treeid);
insert into BPAPermGroupMember (permgroupid, permid) select @groupid, id from BPAPerm where name = @permissionName;

-- Associate relevant group level permissions to Resources tree
insert into BPATreePerm (treeid, permid, groupLevelPerm) select @treeid, id, 0 from BPAPerm where name = @permissionName;

-- Give the system administrators role the new Resource permissions
declare @id int;
select @id = id from BPAUserRole where name='System Administrators';
insert into BPAUserRolePerm (userroleid, permid) select @id, a.id from BPAPerm a
where a.name = @permissionName;

-- Set DB version.
insert into BPADBVersion
(dbversion, 
 scriptrundate, 
 scriptname, 
 [description], 
 timezoneoffset
)
values
('399', 
 GETUTCDATE(), 
 'db_upgradeR399.sql', 
 'Add permission for viewing resource details in control room', 
 0
);
