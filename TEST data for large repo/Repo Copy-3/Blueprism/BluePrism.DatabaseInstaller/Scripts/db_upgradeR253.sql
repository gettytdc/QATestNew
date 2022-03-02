
/*
BUG/STORY      : US-2619
PURPOSE        : Add caching tables
*/

CREATE TABLE [BPACacheETags](
    [key] [nvarchar](50) NOT NULL,
    [tag] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_BPACacheETags] PRIMARY KEY CLUSTERED 
(
    [key] ASC
)
)
GO

CREATE PROCEDURE [usp_GetCacheETag]
    @cacheKey NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1 T.[tag] FROM (
        SELECT TOP 1 [tag], 1 AS [priority]
        FROM [BPACacheETags] (NOWAIT)
        WHERE [key] = @cacheKey
        UNION ALL (SELECT '00000000-0000-0000-0000-000000000000' as [tag], 0 AS [priority])) T
        ORDER BY T.[priority] DESC
END
GO

CREATE PROCEDURE [usp_SetCacheETag]
    @cacheKey NVARCHAR(50),
    @tag UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT * FROM BPACacheETags WHERE [key] = @cacheKey)
        INSERT INTO BPACacheETags ([key], [tag]) VALUES (@cacheKey, @tag)
    ELSE
        UPDATE BPACacheETags SET [tag] = @tag WHERE [key] = @cacheKey
END
GO

-- set DB version
insert into BPADBVersion values (
  '253',
  getutcdate(),
  'db_upgradeR253.sql',
  'Add caching tables',
  0 -- UTC
);
