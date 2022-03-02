/*
SCRIPT         : 56
PROJECT NAME   : Automate
DATABASE NAME  : BPA
CREATION DATE  : 31 May 2007
AUTHOR         : PJW
PURPOSE        : Amend user roles to allow finer granularity of access to system manager
NOTES          : 
*/


DECLARE @SysMan BIGINT
SELECT  @SysMan = PermissionID FROM BPAPermission WHERE [Name]='System Manager'


DECLARE @NewIndex BIGINT 
SET @NewIndex= 1
DECLARE @Count BIGINT
DECLARE @Total BIGINT
SET @Total = 0

--Insert new permission for accessing User Management
SET @Count = 1
WHILE @Count > 0
BEGIN
    SELECT @Count = COUNT(PermissionID) FROM BPAPermission WHERE PermissionID = @NewIndex
    IF @Count > 0 SET @NewIndex = 2 * @NewIndex
END
INSERT INTO BPAPermission ([Name],PermissionID) VALUES ('User Management', @NewIndex)
SET @Total = @Total + @NewIndex


--Insert new permission for accessing Report Management
SET @Count = 1
WHILE @Count > 0
BEGIN
    SELECT @Count = COUNT(PermissionID) FROM BPAPermission WHERE PermissionID = @NewIndex
    IF @Count > 0 SET @NewIndex = 2 * @NewIndex
END
INSERT INTO BPAPermission ([Name],PermissionID) VALUES ('Report Management', @NewIndex)
SET @Total = @Total + @NewIndex


--Insert new permission for accessing Resource Management
SET @Count = 1
WHILE @Count > 0
BEGIN
    SELECT @Count = COUNT(PermissionID) FROM BPAPermission WHERE PermissionID = @NewIndex
    IF @Count > 0 SET @NewIndex = 2 * @NewIndex
END
INSERT INTO BPAPermission ([Name],PermissionID) VALUES ('Resource Management', @NewIndex)
SET @Total = @Total + @NewIndex



--Insert new permission for accessing Process Management
SET @Count = 1
WHILE @Count > 0
BEGIN
    SELECT @Count = COUNT(PermissionID) FROM BPAPermission WHERE PermissionID = @NewIndex
    IF @Count > 0 SET @NewIndex = 2 * @NewIndex
END
INSERT INTO BPAPermission ([Name],PermissionID) VALUES ('Process Management', @NewIndex)
SET @Total = @Total + @NewIndex


--Insert new permission for accessing Logs
SET @Count = 1
WHILE @Count > 0
BEGIN
    SELECT @Count = COUNT(PermissionID) FROM BPAPermission WHERE PermissionID = @NewIndex
    IF @Count > 0 SET @NewIndex = 2 * @NewIndex
END
INSERT INTO BPAPermission ([Name],PermissionID) VALUES ('System Manager Logs', @NewIndex)
SET @Total = @Total + @NewIndex



--Insert new permission for accessing Web Services
SET @Count = 1
WHILE @Count > 0
BEGIN
    SELECT @Count = COUNT(PermissionID) FROM BPAPermission WHERE PermissionID = @NewIndex
    IF @Count > 0 SET @NewIndex = 2 * @NewIndex
END
INSERT INTO BPAPermission ([Name],PermissionID) VALUES ('Web Service Management', @NewIndex)
SET @Total = @Total + @NewIndex


--Insert new permission for accessing Business Object Management
SET @Count = 1
WHILE @Count > 0
BEGIN
    SELECT @Count = COUNT(PermissionID) FROM BPAPermission WHERE PermissionID = @NewIndex
    IF @Count > 0 SET @NewIndex = 2 * @NewIndex
END
INSERT INTO BPAPermission ([Name],PermissionID) VALUES ('Business Object Management', @NewIndex)
SET @Total = @Total + @NewIndex




--Insert new permission for accessing Database management
SET @Count = 1
WHILE @Count > 0
BEGIN
    SELECT @Count = COUNT(PermissionID) FROM BPAPermission WHERE PermissionID = @NewIndex
    IF @Count > 0 SET @NewIndex = 2 * @NewIndex
END
INSERT INTO BPAPermission ([Name],PermissionID) VALUES ('Database Management', @NewIndex)
SET @Total = @Total + @NewIndex

--Insert new permission for accessing Settings
SET @Count = 1
WHILE @Count > 0
BEGIN
    SELECT @Count = COUNT(PermissionID) FROM BPAPermission WHERE PermissionID = @NewIndex
    IF @Count > 0 SET @NewIndex = 2 * @NewIndex
END
INSERT INTO BPAPermission ([Name],PermissionID) VALUES ('General Settings', @NewIndex)
SET @Total = @Total + @NewIndex


--Insert overall grouping for these system manager permissions
UPDATE BPAPermission SET PermissionID = @Total WHERE [Name] = 'System Manager'


--Update each existing role so that anyone with system manager role retains it
DECLARE @MaxInt BIGINT
SET @MaxInt = 9223372036854775807
UPDATE BPARole SET RolePermissions = ((RolePermissions & (@MaxInt ^ @SysMan)) | @Total) WHERE (RolePermissions & @SysMan) > 0


---Likewise with each user permission
UPDATE BPAUser SET [Permissions] = (([Permissions] & (@MaxInt ^ @SysMan)) | @Total) WHERE ([Permissions] & @SysMan) > 0

---Restore System Administrator to max permissions, in case it was affected
UPDATE BPARole SET RolePermissions = @MaxInt WHERE RoleName='System Administrator'





--set DB version
INSERT INTO BPADBVersion VALUES (
  '56',
  GETUTCDATE(),
  'db_upgradeR56.sql UTC',
  'Licensing changes: Added finer granularity of system manager permissions to allow tighter control over license key changes.'
)
