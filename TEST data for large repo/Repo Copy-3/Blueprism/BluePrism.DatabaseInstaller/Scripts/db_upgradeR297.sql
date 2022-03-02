DECLARE @ImportReleaseId INT;
DECLARE @NewDatabaseInstallation INT = (SELECT InstallInProgress
                                        FROM BPVScriptEnvironment);
DECLARE @ReleaseManagerGroupId INT = (SELECT TOP 1 id
                                      FROM BPAPermGroup
                                      WHERE [name] = 'Release Manager');

-- Add the 'Import Release' permission to the database.
INSERT INTO BPAPerm([name])
VALUES('Import Release');

-- Retrieve the newly generated ID from the previous INSERT statement and store it for easier use.
SET @ImportReleaseId = SCOPE_IDENTITY();

-- Add the new permission to the 'Release Manager' group.
INSERT INTO BPAPermGroupMember(permgroupid,
                               permid)
VALUES(@ReleaseManagerGroupId,
       @ImportReleaseId);

-- If creating a new database, give the 'System Administrator' & 'Release Manager' roles the new permission.
IF (@NewDatabaseInstallation = 1)
BEGIN
    DECLARE @SystemAdminUserRoleId INT = (SELECT TOP 1 id
                                          FROM BPAUserRole
                                          WHERE [name] = 'System Administrators');
    DECLARE @ReleaseManagersUserRoleId INT = (SELECT TOP 1 id
                                              FROM BPAUserRole
                                              WHERE [name] = 'Release Managers');
    
    INSERT INTO BPAUserRolePerm(userroleid,
                                permid)
    SELECT BPAUserRole.id,
           @ImportReleaseId
    FROM BPAUserRole
    WHERE BPAUserRole.id = @SystemAdminUserRoleId
        OR BPAUserRole.id = @ReleaseManagersUserRoleId;
END
-- If upgrading the database, give any role that currently has 'View Release Manager' the new permission.
ELSE
BEGIN
    INSERT INTO BPAUserRolePerm(userroleid,
                                permid)
    SELECT BPAUserRolePerm.userroleid,
           @ImportReleaseId
    FROM BPAUserRolePerm
    INNER JOIN BPAPerm
        ON BPAUserRolePerm.permid = BPAPerm.id
    WHERE BPAPerm.[name] = 'View Release Manager';
END

--set DB version
INSERT INTO BPADBVersion VALUES (
  '297',
  GETUTCDATE(),
  'db_upgradeR297.sql',
  'Add ImportRelease permission and automatically assign it based on current roles/permissions.',
  0)
