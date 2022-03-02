/*
SCRIPT         : 129
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Adds permissions for the locking UI, adds Scheduler permission to SysAdmin
*/

-- The current top value for a permission is "System - Scheduler",
-- set in db_upgradeR125 at 0x40000000000000. Double that.
declare @perm bigint
set @perm = cast(0x80000000000000 as bigint)

-- Insert this into the permissions table.
insert into BPAPermission (PermissionID, name)
    values (@perm, 'Workflow - Environment Locking')

-- And add to the System Manager compound permission.
update BPAPermission
    set PermissionID = PermissionID | @perm
where name = 'System Manager'

-- Finally, update the System Administrator role
update BPARole
    set RolePermissions = RolePermissions | @perm
where RoleName = 'System Administrator'

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '129',
  GETUTCDATE(),
  'db_upgradeR129.sql UTC',
  'Adds permissions for the locking UI, adds Scheduler permission to SysAdmin'
);
