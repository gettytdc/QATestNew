CREATE TABLE [BPADocumentProcessingQueueOverride](
    [batchid] [uniqueidentifier] NOT NULL,
    [queueid] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_BPADocumentProcessingQueueOverride] PRIMARY KEY CLUSTERED 
(
    [batchid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


DECLARE @idTable TABLE([id] INT)

INSERT INTO [BPAPerm] ([name], [treeid], [showInUi])
OUTPUT inserted.id INTO @idTable
VALUES ('Document Processing - Redirect Output', NULL, 0)

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

-- Set DB version.
INSERT INTO BPADBVersion(dbversion, 
                         scriptrundate, 
                         scriptname, 
                         [description], 
                         timezoneoffset)
VALUES('278',
       getutcdate(),
       'db_upgradeR278.sql',
       'Create document processing queue override table.',
       0);