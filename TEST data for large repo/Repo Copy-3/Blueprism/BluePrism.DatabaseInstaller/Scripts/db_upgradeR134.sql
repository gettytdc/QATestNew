/*
SCRIPT         : 134
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : CG
PURPOSE        : New permission for environment variables
*/

DECLARE @NewIndex BIGINT 
SET @NewIndex= 1
DECLARE @Count BIGINT

--We find out the next available slot for the permission and create it
SET @Count = 1
WHILE @Count > 0
BEGIN
    SELECT @Count = COUNT(PermissionID) FROM BPAPermission WHERE PermissionID = @NewIndex
    IF @Count > 0 SET @NewIndex = 2 * @NewIndex
END
INSERT INTO BPAPermission (Name,PermissionID) VALUES ('Modify Environment Variables', @NewIndex)

--No users can possibly have the new permission, because it's new. But we'll make
--sure because we can re-use old deleted ones here.
UPDATE BPAUser SET Permissions = Permissions & (~@NewIndex)

--Add to 'compound permission' for System Manager...
UPDATE BPAPermission set PermissionID = PermissionID | @NewIndex WHERE Name='System Manager'

--Add to the System Administrator role...
UPDATE BPARole set RolePermissions = RolePermissions | @NewIndex WHERE RoleName='System Administrator'

--set DB version
INSERT INTO BPADBVersion VALUES (
  '134',
  GETUTCDATE(),
  'db_upgradeR134.sql UTC',
  'New permission for Environment Variables'
)

