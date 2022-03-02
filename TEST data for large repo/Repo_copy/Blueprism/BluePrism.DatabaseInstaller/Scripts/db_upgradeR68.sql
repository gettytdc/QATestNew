/*
SCRIPT         : 68
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : PJW
PURPOSE        : Permission changes for Work Queues development
*/

UPDATE BPAPermission SET [Name]='Read-Only Access to Session Management' WHERE [Name]='Read-Only Access to Control Room'
UPDATE BPAPermission SET [Name]='Full Access to Session Management' WHERE [Name]='Full Access to Control Room'
GO

DECLARE @NewIndex BIGINT 
SET @NewIndex= 1
DECLARE @Count BIGINT
DECLARE @Total BIGINT
SET @Total = 0


--We find out the next available slot for new Readonly Queue Management permission and create it
SET @Count = 1
WHILE @Count > 0
BEGIN
    SELECT @Count = COUNT(PermissionID) FROM BPAPermission WHERE PermissionID = @NewIndex
    IF @Count > 0 SET @NewIndex = 2 * @NewIndex
END
INSERT INTO BPAPermission ([Name],PermissionID) VALUES ('Read-Only Access to Queue Management', @NewIndex)
SET @Total = @Total + @NewIndex

--We find out the next available slot for new Full Queue Management permission and create it
SET @Count = 1
WHILE @Count > 0
BEGIN
    SELECT @Count = COUNT(PermissionID) FROM BPAPermission WHERE PermissionID = @NewIndex
    IF @Count > 0 SET @NewIndex = 2 * @NewIndex
END
INSERT INTO BPAPermission ([Name],PermissionID) VALUES ('Full Access to Queue Management', @NewIndex)
SET @Total = @Total + @NewIndex


--We need to update Control Room group to include all related permissions
SET @Total = @Total + (SELECT PermissionID FROM BPAPermission WHERE [Name]='Read-Only Access to Session Management')
SET @Total = @Total + (SELECT PermissionID FROM BPAPermission WHERE [Name]='Full Access to Session Management')
UPDATE BPAPermission SET PermissionID = @Total WHERE [Name]='Control Room'
UPDATE BPARole SET RolePermissions = @Total WHERE RoleName='Controller'
GO




--set DB version
INSERT INTO BPADBVersion VALUES (
  '68',
  GETUTCDATE(),
  'db_upgradeR68.sql UTC',
  'New permissions for Work Queue Management'
)
GO
