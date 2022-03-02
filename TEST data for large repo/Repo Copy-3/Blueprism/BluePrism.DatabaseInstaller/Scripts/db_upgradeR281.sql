CREATE TABLE [BPADocumentTypeQueues](
    [documentType] [uniqueidentifier] NOT NULL,
    [queue] [uniqueidentifier] NULL
) ON [PRIMARY]

CREATE TABLE [BPADocumentTypeDefaultQueue](
    [queue] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_BPADocumentTypeDefaultQueue] PRIMARY KEY CLUSTERED ([queue] ASC)
) ON [PRIMARY]

GO

IF NOT EXISTS(SELECT [name] FROM [BPAWorkQueue] WHERE [name] like 'Document Processing Queue')
    BEGIN
        DECLARE @idTable TABLE (id UNIQUEIDENTIFIER NOT NULL)

        INSERT INTO [BPAWorkQueue] ([id], [name], [keyfield], [running], [maxattempts], [targetsessions])
        OUTPUT inserted.id INTO @idTable
        VALUES (NEWID(), 'Document Processing Queue', 'Document ID', 1, 1, 0)

        INSERT INTO [BPADocumentTypeDefaultQueue]
        SELECT TOP 1 id FROM @idTable
    END

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '281',
  GETUTCDATE(),
  'db_upgradeR281.sql',
  'Add default document processing queue',
  0 -- UTC
);
