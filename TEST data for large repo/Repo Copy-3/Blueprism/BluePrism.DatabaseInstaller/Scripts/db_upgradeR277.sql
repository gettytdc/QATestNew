ALTER TABLE [BPAPermGroup]
ADD [showInUi] BIT NOT NULL
DEFAULT (1)

ALTER TABLE [BPAPerm]
ADD [showInUi] BIT NOT NULL
DEFAULT (1)

ALTER TABLE [BPAUserRole]
ADD [showInUi] BIT NOT NULL
DEFAULT (1)

GO

DECLARE @showInUi BIT = 0

DECLARE @idTable TABLE([id] INT)

INSERT INTO [BPAPermGroup] ([name], [showInUi])
OUTPUT inserted.id INTO @idTable
VALUES ('Document Processing', @showInUi)

DECLARE @permGroupId INT = 
(SELECT TOP 1 id FROM @idTable)
DELETE FROM @idTable

INSERT INTO [BPAPerm] ([name], [treeid], [showInUi])
OUTPUT inserted.id INTO @idTable
VALUES ('Document Processing - Create Batch', NULL, @showInUi)

DECLARE @permId INT =
(SELECT TOP 1 id FROM @idTable)
DELETE FROM @idTable

INSERT INTO [BPAPermGroupMember] ([permgroupid], [permid])
VALUES (@permGroupId, @permId)

DECLARE @systemAdminRoleId INT = (
    SELECT TOP 1 [id]
    FROM [BPAUserRole]
    WHERE [name] = 'System Administrators')

INSERT INTO [BPAUserRolePerm] ([permid], [userroleid])
VALUES (@permId, @systemAdminRoleId)

INSERT INTO [BPAUserRole] ([name], [showInUi])
OUTPUT inserted.id INTO @idTable
VALUES ('Document Processing User', @showInUi)

DECLARE @roleId INT =
(SELECT TOP 1 id FROM @idTable)
DELETE FROM @idTable

INSERT INTO [BPAUserRolePerm] ([permid], [userroleid])
VALUES (@permId, @roleId)

-- Set DB version.
INSERT INTO BPADBVersion(dbversion, 
                         scriptrundate, 
                         scriptname, 
                         [description], 
                         timezoneoffset)
VALUES('277',
       getutcdate(),
       'db_upgradeR277.sql',
       'Add document processing permissions.',
       0);