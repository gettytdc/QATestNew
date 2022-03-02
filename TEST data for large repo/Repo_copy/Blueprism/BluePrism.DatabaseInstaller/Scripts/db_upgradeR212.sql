/*
SCRIPT         : 212
AUTHOR         : GM + AMB
PURPOSE        : Permissions amendments
*/

-- Add new permission for user roles administration
declare @id int;
insert into BPAPerm (name) values('Security - User Roles');
select @id = id from BPAPerm where name = 'Security - User Roles';

-- Add it the System Manager group
insert into BPAPermGroupMember (permgroupid, permid)
select g.id, @id from BPAPermGroup g where g.name = 'System Manager';

-- Add it any roles that currently have 'Security - Users' permission
insert into BPAUserRolePerm (userroleid, permid)
select r.userroleid, @id from BPAUserRolePerm r inner join BPAPerm p on p.id = r.permid
where p.name = 'Security - Users';

-- Rename 'Password Options' permission to 'Sign-on Settings'
select @id = id from BPAPerm where name = 'Security - Password Options'
update BPAPerm set name = 'Security - Sign-on Settings' where id = @id;

-- Ensure it is assigned to any roles that have 'System - Single Sign On' permission
insert into BPAUserRolePerm (userroleid, permid)
select r.userroleid, @id from BPAUserRolePerm r inner join BPAPerm p on p.id = r.permid
where p.name = 'System - Single Sign On' and not exists
    (select 1 from BPAUserRolePerm e where e.userroleid = r.userroleid and e.permid = @id);

-- Now we can remove the 'System - Single Sign On' permission
select @id = id from BPAPerm where name = 'System - Single Sign On';
delete from BPAUserRolePerm where permid = @id;
delete from BPAPermGroupMember where permid = @id;
delete from BPAPerm where id = @id;

-- Ensure to backup the role should for any reason it already exist.
update BPAUserRole set name = 'Runtime Resource - Backup' where name = 'Runtime Resource'

-- Create a resource pc role
insert into BPAUserRole values('Runtime Resource',null);
select @id = id from BPAUserRole where name = 'Runtime Resource';


insert into BPAUserRolePerm (userroleid, permid)
select @id, p.id from BPAPerm p
where p.name in (
    'Audit - Business Object Logs',
    'Audit - Process Logs',
    'View Business Object',
    'View Process',
    'Read-Only Access to Queue Management',
    'Full Access to Session Management');

insert into BPAUserRoleAssignment (userid, userroleid)
select u.userid, @id from BPAUser u where u.systemusername = 'Anonymous Resource'

-- Add the logged in mode to the interal auth table
alter table BPAInternalAuth
    add LoggedInMode int default 0;

-- Rename 'Security - Credentials' permission to 'Security - Manage Credentials'
select @id = id from BPAPerm where name = 'Security - Credentials'
update BPAPerm set name = 'Security - Manage Credentials' where id = @id; 

-- Rename 'Security - View Encryption Schemes' permission to 'Security - View Encryption Scheme Configuration'
select @id = id from BPAPerm where name = 'Security - View Encryption Schemes'
update BPAPerm set name = 'Security - View Encryption Scheme Configuration' where id = @id; 

--set DB version
INSERT INTO BPADBVersion VALUES (
  '212',
  GETUTCDATE(),
  'db_upgradeR212.sql UTC',
  'Permissions amendments'
);
