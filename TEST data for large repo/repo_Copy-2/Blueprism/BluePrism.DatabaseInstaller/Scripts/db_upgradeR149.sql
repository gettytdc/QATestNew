/*
SCRIPT         : 149
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Adds permission to use the Fonts UI
*/

declare @perm bigint

-- Last positive permission value - see bug 5717
set @perm = 0x4000000000000000

-- Add the permission
insert into BPAPermission (permissionid, name)
    values (@perm, 'System - Fonts')

-- Include it in the "System Manager" compound permission
update BPAPermission set
    permissionid = permissionid | @perm
where name = 'System Manager'

-- Also add it to the 'System Administrator' role
update BPARole set
    rolepermissions = rolepermissions | @perm
where rolename = 'System Administrator'

--set DB version
INSERT INTO BPADBVersion VALUES (
  '149',
  GETUTCDATE(),
  'db_upgradeR149.sql UTC',
  'Adds permission to use the Fonts UI'
)
