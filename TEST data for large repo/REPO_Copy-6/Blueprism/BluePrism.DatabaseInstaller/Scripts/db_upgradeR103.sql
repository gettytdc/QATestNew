/*
SCRIPT         : 103
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : GB
PURPOSE        : Add Scheduler Permissions and Roles to DB
*/


INSERT INTO BPAPermission (PermissionID,[Name]) VALUES (17179869184,'View Schedule')
INSERT INTO BPAPermission (PermissionID,[Name]) VALUES (34359738368,'Edit Schedule')
INSERT INTO BPAPermission (PermissionID,[Name]) VALUES (68719476736,'Create Schedule')
INSERT INTO BPAPermission (PermissionID,[Name]) VALUES (137438953472,'Delete Schedule')
INSERT INTO BPAPermission (PermissionID,[Name]) VALUES (257698037760,'Scheduler')

DECLARE @NextRole as int
set @NextRole = 2 * (SELECT MAX(RoleID) FROM BPARole)
INSERT INTO BPARole (RoleID, RoleName, RolePermissions,SingleSignonUserGroup) VALUES (@NextRole,'Schedule Manager',257698037760,NULL)

GO
    
-- set DB version
INSERT INTO BPADBVersion VALUES (
  '103',
  GETUTCDATE(),
  'db_upgradeR103.sql UTC',
  'Add Scheduler Permissions and Roles to DB'
);
