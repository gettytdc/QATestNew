/*
SCRIPT         : 144
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : GMB
PURPOSE        : Correct permissions for environment variables
*/

UPDATE BPAPermission set Name='Processes - Configure Environment Variables' WHERE Name='Modify Environment Variables'

-- I used the script defined in bug 5634 to determine available
-- permission IDs and just used the first three found.

insert into BPAPermission (permissionid, name)
  select 0x0800000000000000, 'Processes - View Environment Variables' union all
  select 0x1000000000000000, 'Business Objects - Configure Environment Variables' union all
  select 0x2000000000000000, 'Business Objects - View Environment Variables'

update BPAPermission set
  PermissionID = PermissionID | 0x3800000000000000
where Name = 'System Manager'

update BPARole set
  RolePermissions = RolePermissions | 0x3800000000000000
where RoleName = 'System Administrator'

-- Finally, since we're creating brand new permissions here, ensure
-- that no users have them enabled (which is potentially possible
-- since it looks like we're reusing permission IDs)
update BPAUser set
  permissions = permissions & ~cast(0x3800000000000000 as bigint)

--set DB version
INSERT INTO BPADBVersion VALUES (
  '144',
  GETUTCDATE(),
  'db_upgradeR144.sql UTC',
  'Correct permissions for environment variables'
)
