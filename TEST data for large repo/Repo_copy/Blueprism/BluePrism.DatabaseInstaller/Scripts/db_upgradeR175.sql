/*
SCRIPT         : 175
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Transplants the roles & permissions to a new structure
NOTES          : This uses a few things that were only introduced into SQL Server at
                 SQL Server 2005, so it categorically will not work in SQL 2000
RELATED BUGS   : 5634
*/

------------------------------------------------------------------------------------
--                                 PREPARATION
------------------------------------------------------------------------------------
-- Altered bpa_sp_dropdefault script which handles the case when there is no default
-- defined on the specified column. Also, it swaps out the SQL2000-compatible query
-- for one which is only supported on SQL2005 and beyond
ALTER procedure bpa_sp_dropdefault
    @tableName varchar(256),
    @columnName varchar(256)
as
-- SQL Server 2000 method - works in 2005 for back compatibility
-- but don't know how much further down the line it will be supported
-- select @defaultName = object_name(cdefault) from syscolumns
--     where id = object_id(@tableName) and name = @columnName

-- SQL Server 2005 method - doesn't exist in 2000 and before
    if exists (select 1 from syscolumns
        where id = object_id(@tableName) and name=@columnName)
    begin
        declare @defaultName varchar(256)
        select @defaultName = name
            from sys.default_constraints
            where parent_object_id = object_id(@tableName)
                and col_name(parent_object_id, parent_column_id) = @columnName
        if @defaultName is not null
            exec('alter table [' + @tableName + '] drop constraint ' + @defaultName)
    end
GO
------------------------------------------------------------------------------------
--                               CREATE OBJECTS
------------------------------------------------------------------------------------

-- A single permission
create table BPAPerm (
    id int identity not null,
    name nvarchar(255) not null,
    constraint PK_BPAPerm primary key clustered (id)
);

create unique index UNQ_BPAPerm_name on BPAPerm(name);

-- A group which can contain many permissions (equivalent of the old compound permission)
create table BPAPermGroup (
    id int identity not null,
    name nvarchar(255) not null,
    constraint PK_BPAPermGroup primary key clustered (id)
);

create unique index UNQ_BPAPermGroup_name on BPAPermGroup(name);

-- A member of the perm group
-- No "on delete cascade" for this since both parent tables are only modified
-- by developers in a database upgrade script
create table BPAPermGroupMember (
    permgroupid int not null
        constraint FK_BPAPermGroupMember_BPAPermGroup
            foreign key references BPAPermGroup(id),
    permid int not null
        constraint FK_BPAPermGroupMember_BPAPerm
            foreign key references BPAPerm(id),
    constraint PK_BPAPermGroupMember
        primary key clustered (permgroupid, permid)
);

-- A user role; It contains a set of permissions, and a user can be a member
-- of a number of user roles
create table BPAUserRole (
    id int not null identity,
    name nvarchar(255) not null,
    ssogroup nvarchar(255) null,
    adhocuserid uniqueidentifier null, -- temp column - used to create adhoc roles later in this script
    constraint PK_BPAUserRole primary key clustered (id)
);

create unique index UNQ_BPAUserRole_name on BPAUserRole(name);


-- A permission which is part of a user role
create table BPAUserRolePerm (
    userroleid int not null
        constraint FK_BPAUserRolePerm_BPAUserRole
            foreign key references BPAUserRole(id)
            on delete cascade,
    permid int not null
        constraint FK_BPAUserRolePerm_BPAPerm
            foreign key references BPAPerm(id),
    constraint PK_BPAUserRolePerm
        primary key clustered (permid, userroleid)
);

-- The assignment of a user role to a user
create table BPAUserRoleAssignment (
    userid uniqueidentifier not null
        constraint FK_BPAUserRoleAssignment_BPAUser
            foreign key references BPAUser(userid)
            on delete cascade,
    userroleid int not null
        constraint FK_BPAUserRoleAssignment_BPAUserRole
            foreign key references BPAUserRole(id)
            on delete cascade,
    constraint PK_BPAUserRoleAssignment
        primary key clustered (userid, userroleid)
);

-- A role registered with a credential
create table BPACredentialRole (
    credentialid uniqueidentifier not null
        constraint FK_BPACredentialRole_BPACredential
            foreign key references BPACredentials (id)
            on delete cascade,
    userroleid int null -- a null role indicates available to all roles
        constraint FK_BPACredentialRole_BPAUserRole
            foreign key references BPAUserRole (id)
            on delete cascade,
    constraint UNQ_BPACredentialRole
        unique clustered (credentialid, userroleid)
);

-- A role registered with a resource
create table BPAResourceRole (
    resourceid uniqueidentifier not null
        constraint FK_BPAResourceRole_BPAResource
            foreign key references BPAResource (resourceid)
            on delete cascade,
    userroleid int null -- again, null roleid means 'all roles'
        constraint FK_BPAResourceRole_BPAUserRole
            foreign key references BPAUserRole (id)
            on delete cascade
    constraint UNQ_BPAResourceRole
        unique clustered (resourceid, userroleid)
);

GO

------------------------------------------------------------------------------------
--                               TRANSFER DATA
------------------------------------------------------------------------------------

-- Declare a table to hold the old perms in
declare @groups table (
    gpname varchar(255) not null,
    permname varchar(255) not null
);

-- Load the old perms and their groups into that table
insert into @groups (gpname, permname)
select p.name as "Group Name", p2.name as "Perm Name"
from BPAPermission p
    join BPAPermission p2 on p.PermissionID!=p2.PermissionID
        and (p.PermissionID & p2.PermissionID)!=0
where
    (p.PermissionID & (p.PermissionID-1) != 0) and
    (p2.PermissionID & (p2.PermissionID-1) = 0);

-- Get the perms out
insert into BPAPerm (name)
    select distinct permname from @groups;

-- Get the perm groups out
insert into BPAPermGroup (name)
    select distinct gpname from @groups;

-- Create all the group members picked out from our temp @groups table
-- and joining that data with the new BPAPerm* tables to get the IDs
insert into BPAPermGroupMember (permgroupid, permid)
    select pg.id, p.id
        from BPAPerm p
            cross join BPAPermGroup pg
            join @groups g on p.name = g.permname and pg.name = g.gpname
    order by pg.id, p.id;

-- Get the roles into their new table
insert into BPAUserRole (name, ssogroup)
    select RoleName, SingleSignonUserGroup from BPARole;

-- Add the permissions which are assigned to each role
insert into BPAUserRolePerm (userroleid, permid)
    select ur.id, p.id
        from BPAUserRole ur
            join BPARole r on ur.name = r.RoleName
            join BPAPermission op on (r.RolePermissions & op.PermissionID) != 0
            join BPAPerm p on p.name = op.Name

-- And set up the assignments for all users with a set of roles
insert into BPAUserRoleAssignment (userid, userroleid)
    select u.userid, ur.id
        from BPAUser u
            join BPARole r on (u.Roles & r.RoleID) != 0
            join BPAUserRole ur on ur.name = r.RoleName

-- Now we want to get all those permissions on BPAUser which fall
-- outside the roles that they have assigned to them
declare @adhocs table (
    userid uniqueidentifier not null,
    perm nvarchar(255) not null
);

-- get all permissions assigned to the user where there does not
-- exist a link through the user's assigned roles to the permission
insert into @adhocs (userid, perm)
    select u.userid, p.Name
        from BPAUser u
            join BPAPermission p on (u.Permissions & p.PermissionID)!=0
        where not exists (
            select 1 from BPAUser u2
                join BPARole r2 on (u2.Roles & r2.RoleID) != 0
                join BPAPermission p2 on (r2.RolePermissions & p2.PermissionID) != 0
            where u2.userid = u.userid
                and p.Name = p2.Name
        );

-- Create new adhoc roles, recording the user ID that they are generated for
insert into BPAUserRole (name, adhocuserid)
    select distinct 'Auto-generated Adhoc Role for: ' + u.username, u.userid
        from BPAUser u
            join @adhocs a on u.userid = a.userid;

-- Add the required permissions to the new adhoc roles
insert into BPAUserRolePerm (userroleid, permid)
    select ur.id, p.id
        from @adhocs a
            join BPAUserRole ur on a.userid = ur.adhocuserid
            join BPAPerm p on a.perm = p.name;

-- Finally assign adhoc roles to the corresponding users
insert into BPAUserRoleAssignment (userid, userroleid)
    select adhocuserid, id
        from BPAUserRole
        where adhocuserid is not null;

-- Other tables to update: credentials and resources:

-- Handle the roles assigned to credentials. A single record with a null
-- role ID is the placeholder record to represent 'all roles'
insert into BPACredentialRole (credentialid, userroleid)
    select c.id, ur.id
        from BPACredentials c
            join BPARole r on (c.roleid & r.RoleID) != 0
            join BPAUserRole ur on ur.name = r.RoleName
        where c.roleid != -1
    union all
    select c.id, null
        from BPACredentials c
        where c.roleid = -1;

-- Equally, handle the resources the same way. Again a single record
-- with a null roleid means 'all roles'
insert into BPAResourceRole (resourceid, userroleid)
    select res.resourceid, ur.id
        from BPAResource res
            join BPARole r on (res.permittedroles & r.RoleID) != 0
            join BPAUserRole ur on ur.name = r.RoleName
        where res.permittedroles != -1
    union all
    select res.resourceid, null
        from BPAResource res
        where res.permittedroles = -1;

GO

------------------------------------------------------------------------------------
--                                  CLEAN UP
------------------------------------------------------------------------------------

-- The adhoc user id column in BPAUserRole has done its job; we can lose it now.
alter table BPAUserRole drop column adhocuserid;

-- Drop the default constraints from the two user column and the resource column
exec bpa_sp_dropdefault 'BPAUser', 'Permissions';
exec bpa_sp_dropdefault 'BPAUser', 'Roles';
exec bpa_sp_dropdefault 'BPAResource', 'permittedroles';

-- Drop the obsolete columns
alter table BPAUser drop column Permissions;
alter table BPAUser drop column Roles;
alter table BPACredentials drop column roleid;
alter table BPAResource drop column permittedroles;

-- Finally drop the old role/permission tables
drop table BPARole;
drop table BPAPermission;

------------------------------------------------------------------------------------
--                                  DB Version
------------------------------------------------------------------------------------

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '175',
  GETUTCDATE(),
  'db_upgradeR175.sql UTC',
  'Transplants the roles & permissions to a new structure'
);
