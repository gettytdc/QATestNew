DECLARE @idTable TABLE([id] INT)

INSERT INTO [BPAPerm] ([name], [treeid], [showInUi])
OUTPUT inserted.id INTO @idTable
VALUES ('Document Processing - Configuration', NULL, 0)

DECLARE @permId INT =
(SELECT TOP 1 id FROM @idTable)
DELETE FROM @idTable

DECLARE @permGroupId INT = (
    SELECT TOP 1 [id] 
    FROM [BPAPermGroup] 
    WHERE [name] = 'Document Processing')

DECLARE @systemAdminRoleId INT = (
    SELECT TOP 1 [id]
    FROM [BPAUserRole]
    WHERE [name] = 'System Administrators')

DECLARE @documentProcessingRoleId INT = (
    SELECT TOP 1 [id]
    FROM [BPAUserRole]
    WHERE [name] = 'Document Processing User')

INSERT INTO [BPAPermGroupMember] ([permgroupid], [permid])
VALUES (@permGroupId, @permId)

INSERT INTO [BPAUserRolePerm] ([permid], [userroleid])
VALUES (@permId, @systemAdminRoleId)

INSERT INTO [BPAUserRolePerm] ([permid], [userroleid])
VALUES (@permId, @documentProcessingRoleId)

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '279',
  GETUTCDATE(),
  'db_upgradeR279.sql',
  'Add permission for document processing queues system menu',
  0 -- UTC
);
