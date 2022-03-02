DECLARE @idTable TABLE([id] INT)

-- Find the existing Document Processing group created in R277
DECLARE @docProcessingPermGroupId INT = (
SELECT [id] FROM
[BPAPermGroup] WHERE [name] = 'Document Processing' )

-- Add new role
INSERT INTO [BPAPerm] ([name], [treeid], [requiredFeature])
OUTPUT inserted.id INTO @idTable
VALUES ('Document Processing - View Document', NULL, 'DocumentProcessing')

DECLARE @viewDocumentPermId INT = 
(SELECT TOP 1 id FROM @idTable)
DELETE FROM @idTable

-- Add new role
INSERT INTO [BPAPerm] ([name], [treeid], [requiredFeature])
OUTPUT inserted.id INTO @idTable
VALUES ('Document Processing - View Document Data', NULL, 'DocumentProcessing')

DECLARE @viewDocumentDataPermId INT = 
(SELECT TOP 1 id FROM @idTable)
DELETE FROM @idTable

-- Associate the two new roles with the doc processing group
INSERT INTO [BPAPermGroupMember] ([permgroupid], [permid])
VALUES (@docProcessingPermGroupId, @viewDocumentPermId)

INSERT INTO [BPAPermGroupMember] ([permgroupid], [permid])
VALUES (@docProcessingPermGroupId, @viewDocumentDataPermId)

DECLARE @systemAdminRoleId INT = (
    SELECT TOP 1 [id]
    FROM [BPAUserRole]
    WHERE [name] = 'System Administrators')

-- Associate the two new roles with the sysadmin role
INSERT INTO [BPAUserRolePerm] ([permid], [userroleid])
VALUES (@viewDocumentPermId, @systemAdminRoleId)

INSERT INTO [BPAUserRolePerm] ([permid], [userroleid])
VALUES (@viewDocumentDataPermId, @systemAdminRoleId)

-- Get the Doc Processing role created in R277
DECLARE @docProcessingUserRoleId INT = (
SELECT Id FROM
[BPAUserRole] WHERE [name] = 'Document Processing User' )

-- Associate the two new roles with the Document Processing User role
INSERT INTO [BPAUserRolePerm] ([permid], [userroleid])
VALUES (@viewDocumentPermId, @docProcessingUserRoleId)

INSERT INTO [BPAUserRolePerm] ([permid], [userroleid])
VALUES (@viewDocumentDataPermId, @docProcessingUserRoleId)

-- Set DB version.
INSERT INTO BPADBVersion(dbversion, 
                         scriptrundate, 
                         scriptname, 
                         [description], 
                         timezoneoffset)
VALUES('286',
       getutcdate(),
       'db_upgradeR286.sql',
       'Add additional document processing permissions.',
       0);