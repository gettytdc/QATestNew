/*
SCRIPT         : 124
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Adds a permission for configuring the scheduler
*/

-- Generate the new permission ID.
declare @newpermid bigint;
set @newpermid = (select 2 * max(PermissionID) from BPAPermission);

-- Add to the permission table
insert into BPAPermission (PermissionID, Name) 
    values (@newpermid, 'System - Scheduler');

-- Add the new permission to the 'System Manager' compound permission
update BPAPermission 
    set PermissionID = (PermissionID | @newpermid)
    where Name = 'System Manager';

-- update the roles 'Schedule Manager' and 'System Administrator'
-- (if they exist)
update BPARole 
    set RolePermissions = (RolePermissions | @newpermid)
    where RoleName in ('Schedule Manager','System Administrator');

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '124',
  GETUTCDATE(),
  'db_upgradeR124.sql UTC',
  'Adds a permission for configuring the scheduler'
);
