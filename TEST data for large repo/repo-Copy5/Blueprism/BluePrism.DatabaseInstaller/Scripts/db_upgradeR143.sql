/*
SCRIPT         : 143
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Adds permissions for release manager
*/

-- I used the script defined in bug 5634 to determine available
-- permission IDs and just used the first four found.

insert into BPAPermission (permissionid, name)
  select 0x0000000000008000, 'View Release Manager' union all
  select 0x0100000000000000, 'Create/Edit Package'  union all
  select 0x0200000000000000, 'Create Release'       union all
  select 0x0400000000000000, 'Delete Package'       union all
  select 0x0700000000008000, 'Release Manager' -- compound permission

 
update BPARole set
  RolePermissions = RolePermissions | 0x0700000000008000
where RoleName = 'System Administrator'

-- Finally, since we're creating brand new permissions here, ensure
-- that no users have them enabled (which is potentially possible
-- since it looks like we're reusing permission IDs)
update BPAUser set
  permissions = permissions & ~cast(0x0700000000008000 as bigint)

--set DB version
INSERT INTO BPADBVersion VALUES (
  '143',
  GETUTCDATE(),
  'db_upgradeR143.sql UTC',
  'Adds fonts to packages / releases'
)
