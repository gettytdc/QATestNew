DECLARE @idTable TABLE([id] INT)

-- Find the existing Document Processing group created in R277
DECLARE @docProcessingPermGroupId INT = (
SELECT [id] FROM
[BPAPermGroup] WHERE [name] = 'Document Processing' )

-- Add new role
INSERT INTO [BPAPerm] ([name], [treeid], [requiredFeature])
OUTPUT inserted.id INTO @idTable
VALUES ('ViewBatchType', NULL, 'DocumentProcessing')

DECLARE @viewBatchTypePermId INT = 
(SELECT TOP 1 id FROM @idTable)
DELETE FROM @idTable

-- Associate the new role with the doc processing group
INSERT INTO [BPAPermGroupMember] ([permgroupid], [permid])
VALUES (@docProcessingPermGroupId, @viewBatchTypePermId)

DECLARE @systemAdminRoleId INT = (
    SELECT TOP 1 [id]
    FROM [BPAUserRole]
    WHERE [name] = 'System Administrators')

-- Associate the new role with the sysadmin role
INSERT INTO [BPAUserRolePerm] ([permid], [userroleid])
VALUES (@viewBatchTypePermId, @systemAdminRoleId)

-- Get the Doc Processing role created in R277
DECLARE @docProcessingUserRoleId INT = (
SELECT Id FROM
[BPAUserRole] WHERE [name] = 'Document Processing User' )

-- Associate the new role with the Document Processing User role
INSERT INTO [BPAUserRolePerm] ([permid], [userroleid])
VALUES (@viewBatchTypePermId, @docProcessingUserRoleId)

-- Set DB version.
INSERT INTO BPADBVersion(dbversion, 
                         scriptrundate, 
                         scriptname, 
                         [description], 
                         timezoneoffset)
VALUES('290',
       getutcdate(),
       'db_upgradeR290.sql',
       'Add additional document processing permissions.',
       0);
