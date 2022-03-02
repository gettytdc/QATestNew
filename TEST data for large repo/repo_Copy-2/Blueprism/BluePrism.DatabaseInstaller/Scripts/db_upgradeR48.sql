/*
SCRIPT         : 48
PROJECT NAME   : Automate
DATABASE NAME  : BPA
CREATION DATE  : 01 Nov 2006
AUTHOR         : PJW
PURPOSE        : Adds necessary user roles for new object studio features
NOTES          : 
*/

--INSERT the individual actions available in Object Studio
INSERT INTO BPAPermission (PermissionID, [Name]) VALUES (1048576, 'Create/Clone Business Object')
INSERT INTO BPAPermission (PermissionID, [Name]) VALUES (2097152, 'Edit Business Object')
INSERT INTO BPAPermission (PermissionID, [Name]) VALUES (4194304, 'Test Business Object')
INSERT INTO BPAPermission (PermissionID, [Name]) VALUES (8388608, 'Import Business Object')
INSERT INTO BPAPermission (PermissionID, [Name]) VALUES (16777216, 'Export Business Object')
INSERT INTO BPAPermission (PermissionID, [Name]) VALUES (33554432, 'Delete Business Object')
INSERT INTO BPAPermission (PermissionID, [Name]) VALUES (67108864, 'View Business Object')
INSERT INTO BPAPermission (PermissionID, [Name]) VALUES (134217728, 'Compare Business Objects')

--INSERT the banner header of 'object studio', the sum of the above permissions
INSERT INTO BPAPermission (PermissionID, [Name]) VALUES (267386880, 'Object Studio')

--Rename the 'designer' role to 'Process Designer'
UPDATE BPARole SET RoleName = 'Process Designer' WHERE RoleID = 2
GO


--Create a new role for business object designers called 'Business Object Designer'
--We just have to hope that the user has not created too many roles already!!!!
DECLARE @NextRole as int
set @NextRole = 2 * (SELECT MAX(RoleID) FROM BPARole)
INSERT INTO BPARole (RoleID, RoleName, RolePermissions) VALUES (@NextRole, 'Business Object Designer', 267386880)


--set DB version
INSERT INTO BPADBVersion VALUES (
  '48',
  GETUTCDATE(),
  'db_upgradeR48.sql UTC',
  'Add necessary user roles for new object studio features'
)
GO

