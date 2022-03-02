CREATE PROCEDURE #usp_dropShowUiDefault
    @tableName NVARCHAR(100)
AS
BEGIN
    DECLARE @command NVARCHAR(MAX) = (
        SELECT TOP 1 'ALTER TABLE [' + t.name + '] DROP CONSTRAINT [' + d.name + ']' FROM sys.default_constraints d
        JOIN sys.tables t ON t.object_id = d.parent_object_id
        WHERE t.name = @tableName AND d.name LIKE '%showI%'
        ORDER BY t.name)

    EXEC(@command)
END

GO

IF COL_LENGTH('BPAPerm','requiredFeature') IS NULL
BEGIN
    ALTER TABLE [BPAPerm]
    ADD [requiredFeature] NVARCHAR(100) NOT NULL CONSTRAINT BPAPerm_default_requiredFeature DEFAULT ''
END

IF COL_LENGTH('BPAPermGroup','requiredFeature') IS NULL
BEGIN
    ALTER TABLE [BPAPermGroup]
    ADD [requiredFeature] NVARCHAR(100) NOT NULL CONSTRAINT BPAPermGroup_default_requiredFeature DEFAULT ''
END

IF COL_LENGTH('BPAUserRole','requiredFeature') IS NULL
BEGIN
    ALTER TABLE [BPAUserRole]
    ADD [requiredFeature] NVARCHAR(100) NOT NULL CONSTRAINT BPAUserRole_default_requiredFeature DEFAULT ''
END

GO

UPDATE [BPAPerm]
SET [requiredFeature] = 'DocumentProcessing'
WHERE [showInUi] = 0

UPDATE [BPAPermGroup]
SET [requiredFeature] = 'DocumentProcessing'
WHERE [showInUi] = 0

UPDATE [BPAUserRole]
SET [requiredFeature] = 'DocumentProcessing'
WHERE [showInUi] = 0

EXEC #usp_dropShowUiDefault 'BPAPerm'
EXEC #usp_dropShowUiDefault 'BPAPermGroup'
EXEC #usp_dropShowUiDefault 'BPAUserRole'

ALTER TABLE [BPAPerm] 
DROP COLUMN [showInUi]

ALTER TABLE [BPAPermGroup] 
DROP COLUMN [showInUi]

ALTER TABLE [BPAUserRole] 
DROP COLUMN [showInUi]

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '280',
  GETUTCDATE(),
  'db_upgradeR280.sql',
  'Change showInUi field to instead reference a specific feature',
  0 -- UTC
);
