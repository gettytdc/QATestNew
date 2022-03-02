IF COL_LENGTH('BPASysConfig', 'DatabaseInstallerOptions') IS NULL
    ALTER TABLE BPASysConfig ADD DatabaseInstallerOptions INT
GO

EXEC sp_rename 'BPASessionLog_Unicode','BPASessionLog_Unicode_pre65'
EXEC sp_rename 'PK_BPASessionLog_Unicode','PK_BPASessionLog_Unicode_pre65'
EXEC sp_rename 'FK_BPASessionLog_Unicode_BPASession','FK_BPASessionLog_Unicode_BPASession_pre65'
GO

-- Create the new table...
CREATE TABLE BPASessionLog_Unicode (
    logid BIGINT NOT NULL IDENTITY(1,1),
    sessionnumber INT NOT NULL,
    stageid uniqueidentifier NULL,
    stagename NVARCHAR(128) NULL,
    stagetype INT NULL,
    processname NVARCHAR(128) NULL,
    pagename NVARCHAR(128) NULL,
    objectname NVARCHAR(128) NULL,
    actionname NVARCHAR(128) NULL,
    result NVARCHAR(max) NULL,
    resulttype INT NULL,
    startdatetime DATETIME NULL,
    enddatetime DATETIME NULL,
    attributexml NVARCHAR(max) NULL,
    automateworkingset BIGINT NULL,
    targetappname NVARCHAR(32) NULL,
    targetappworkingset BIGINT NULL,
    starttimezoneoffset INT NULL,
    endtimezoneoffset INT NULL,
    attributesize as ISNULL(DATALENGTH(attributexml),0) PERSISTED,
    CONSTRAINT PK_BPASessionLog_Unicode PRIMARY KEY (logid)
)
GO

--- Do the data migration if requested...
IF EXISTS (SELECT 1 FROM BPASysConfig WHERE DatabaseInstallerOptions & 1 = 1)  -- (flags) 0x1 = MigrateSessionPre65
BEGIN
    DECLARE @BatchSize INT = 50000;
    WHILE EXISTS(SELECT 1 FROM BPASessionLog_Unicode_pre65)
    BEGIN
        INSERT INTO BPASessionLog_Unicode 
        SELECT TOP (@BatchSize)
            sessionnumber,
            stageid,
            stagename,
            stagetype,
            processname,
            pagename,
            objectname,
            actionname,
            result,
            resulttype,
            startdatetime,
            enddatetime,
            attributexml,
            automateworkingset,
            targetappname,
            targetappworkingset,
            starttimezoneoffset,
            endtimezoneoffset
        FROM BPASessionLog_Unicode_pre65
        ORDER BY sessionnumber, seqnum;

        WITH CTE AS
        (
            SELECT TOP (@BatchSize) *
            FROM BPASessionLog_Unicode_pre65
            ORDER BY sessionnumber, seqnum
        )
        DELETE FROM CTE
    END
END
GO

ALTER TABLE BPASessionLog_Unicode ADD CONSTRAINT FK_BPASessionLog_Unicode_BPASession FOREIGN KEY (sessionnumber) REFERENCES BPASession(sessionnumber)
CREATE INDEX Index_BPASessionLog_Unicode_sessionnumber ON BPASessionLog_Unicode (sessionnumber) WITH (FILLFACTOR = 90)
GO

IF EXISTS (SELECT 1 FROM BPASysConfig WHERE DatabaseInstallerOptions & 1 = 1)  -- (flags) 0x1 = MigrateSessionPre65
BEGIN
    ALTER TABLE BPASessionLog_Unicode_pre65 DROP CONSTRAINT FK_BPASessionLog_Unicode_BPASession_pre65;
    ALTER TABLE BPASessionLog_Unicode_pre65 DROP CONSTRAINT PK_BPASessionLog_Unicode_pre65;
    DROP TABLE BPASessionLog_Unicode_pre65;
END
GO

EXEC sp_rename 'BPASessionLog_NonUnicode','BPASessionLog_NonUnicode_pre65'
EXEC sp_rename 'PK_BPASessionLog_NonUnicode','PK_BPASessionLog_NonUnicode_pre65'
EXEC sp_rename 'FK_BPASessionLog_NonUnicode_BPASession','FK_BPASessionLog_NonUnicode_BPASession_pre65'
GO

CREATE TABLE BPASessionLog_NonUnicode (
    logid BIGINT NOT NULL IDENTITY(1,1),
    sessionnumber INT NOT NULL,
    stageid uniqueidentifier NULL,
    stagename VARCHAR(128) NULL,
    stagetype INT NULL,
    processname VARCHAR(128) NULL,
    pagename VARCHAR(128) NULL,
    objectname VARCHAR(128) NULL,
    actionname VARCHAR(128) NULL,
    result VARCHAR(max) NULL,
    resulttype INT NULL,
    startdatetime DATETIME NULL,
    enddatetime DATETIME NULL,
    attributexml VARCHAR(max) NULL,
    automateworkingset BIGINT NULL,
    targetappname VARCHAR(32) NULL,
    targetappworkingset BIGINT NULL,
    starttimezoneoffset INT NULL,
    endtimezoneoffset INT NULL,
    attributesize as ISNULL(DATALENGTH(attributexml),0) PERSISTED,
    CONSTRAINT PK_BPASessionLog_NonUnicode PRIMARY KEY (logid)
)
GO

IF EXISTS (SELECT 1 FROM BPASysConfig WHERE DatabaseInstallerOptions & 1 = 1)  -- (flags) 0x1 = MigrateSessionPre65
BEGIN
    DECLARE @BatchSize INT = 50000;
    WHILE EXISTS(SELECT 1 FROM BPASessionLog_NonUnicode_pre65)
    BEGIN
        INSERT INTO BPASessionLog_NonUnicode 
        SELECT TOP (@BatchSize)
            sessionnumber,
            stageid,
            stagename,
            stagetype,
            processname,
            pagename,
            objectname,
            actionname,
            result,
            resulttype,
            startdatetime,
            enddatetime,
            attributexml,
            automateworkingset,
            targetappname,
            targetappworkingset,
            starttimezoneoffset,
            endtimezoneoffset
        FROM BPASessionLog_NonUnicode_pre65
        ORDER BY sessionnumber, seqnum;

        WITH CTE AS
        (
            SELECT TOP (@BatchSize) *
            FROM BPASessionLog_NonUnicode_pre65
            ORDER BY sessionnumber, seqnum
        )
        DELETE FROM CTE
    END
END
GO

ALTER TABLE BPASessionLog_NonUnicode ADD CONSTRAINT FK_BPASessionLog_NonUnicode_BPASession FOREIGN KEY (sessionnumber) REFERENCES BPASession(sessionnumber)
CREATE INDEX Index_BPASessionLog_NonUnicode_sessionnumber ON BPASessionLog_NonUnicode (sessionnumber) WITH (FILLFACTOR = 90)
GO

IF EXISTS (SELECT 1 FROM BPASysConfig WHERE DatabaseInstallerOptions & 1 = 1)  -- (flags) 0x1 = MigrateSessionPre65
BEGIN
    ALTER TABLE BPASessionLog_NonUnicode_pre65 DROP CONSTRAINT FK_BPASessionLog_NonUnicode_BPASession_pre65;
    ALTER TABLE BPASessionLog_NonUnicode_pre65 DROP CONSTRAINT PK_BPASessionLog_NonUnicode_pre65;
    DROP TABLE BPASessionLog_NonUnicode_pre65;
END
GO

-- Set DB version.
INSERT INTO BPADBVersion(dbversion, 
                         scriptrundate, 
                         scriptname, 
                         [description], 
                         timezoneoffset)
VALUES('295',
       GETUTCDATE(),
       'db_upgradeR295.sql',
       'Improve performance of BPASessionLogs',
       0);
