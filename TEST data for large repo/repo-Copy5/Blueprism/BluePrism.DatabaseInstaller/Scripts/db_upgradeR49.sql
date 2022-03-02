/*
SCRIPT         : 49
PROJECT NAME   : Automate
DATABASE NAME  : BPA
CREATION DATE  : 02 Nov 2006
AUTHOR         : PJW
PURPOSE        : Recreate process table with new process type field for business objects
NOTES          : 
*/


--Drop foreign key constraints against process table
ALTER TABLE BPASESSION
    DROP CONSTRAINT FK_BPASESSION_BPAPROCESS
GO

ALTER TABLE BPAPROCESSLOCK
    DROP CONSTRAINT FK_BPAProcessLock_BPAProcess
GO

ALTER TABLE BPAREALTIMESTATSVIEW
    DROP CONSTRAINT FK_BPAPROCESS
GO

ALTER TABLE BPASCENARIOLINK
    DROP CONSTRAINT FK_BPAScenarioLink_BPAProcess
GO



--make a temp table to hold all existing processes
CREATE TABLE #PROCESSES_TEMP (
    [processid] [uniqueidentifier] NOT NULL ,
    [name] [varchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
    [description] [varchar] (1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
    [version] [varchar] (20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
    [createdate] [datetime] NULL ,
    [createdby] [uniqueidentifier] NULL ,
    [lastmodifieddate] [datetime] NULL ,
    [lastmodifiedby] [uniqueidentifier] NULL ,
    [DefaultRealTimeStatsView] uniqueidentifier,
    [processxml] [text] COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
    [AttributeID] int NOT NULL DEFAULT 0,
)

--Copy over the existing processes to temp table, and drop existing table
INSERT INTO #PROCESSES_TEMP SELECT * FROM BPAPROCESS
DROP TABLE BPAPROCESS

--Recreate the process table with the new "processs type" field.
--We are careful to recreate all foreign key constraints which existed as 
--part of the table
CREATE TABLE [BPAProcess] (
    [processid] [uniqueidentifier] NOT NULL ,
    [ProcessType] [VarChar] (1) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
    [name] [varchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
    [description] [varchar] (1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
    [version] [varchar] (20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
    [createdate] [datetime] NULL ,
    [createdby] [uniqueidentifier] NULL ,
    [lastmodifieddate] [datetime] NULL ,
    [lastmodifiedby] [uniqueidentifier] NULL ,
    [processxml] [text] COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
    [DefaultRealTimeStatsView] uniqueidentifier,
    [AttributeID] int NOT NULL DEFAULT 0,
    CONSTRAINT [PK_BPAProcess] PRIMARY KEY  CLUSTERED 
    (
        [processid]
    ),
    CONSTRAINT [FK_BPAProcess_BPAUser] FOREIGN KEY 
    (
        [createdby]
    ) REFERENCES [BPAUser] (
        [userid]
    ),
    CONSTRAINT [FK_BPAProcess_BPAUser1] FOREIGN KEY 
    (
        [lastmodifiedby]
    ) REFERENCES [BPAUser] (
        [userid]
    ),
    CONSTRAINT [FK_BPAProcess_DefaultRealTimeStatsView] FOREIGN KEY 
    (
        [DefaultRealTimeStatsView]
    ) REFERENCES [BPARealTimeStatsView] (
        [ViewID]
    )
)



--Copy contents of temp table into new table, and get rid of the temp table
INSERT INTO BPAPROCESS (ProcessID, ProcessType, [Name], Description, Version, CreateDate, CreatedBy, LastModifiedDate, LastModifiedBy, ProcessXMl, DefaultRealtimeStatsView, AttributeID)
                 SELECT ProcessID, 'P', [Name], Description, Version, CreateDate, CreatedBy, LastModifiedDate, LastModifiedBy, ProcessXMl, DefaultRealtimeStatsView, AttributeID FROM #PROCESSES_TEMP

DROP TABLE #PROCESSES_TEMP
GO

--Add all the foreign key constraints against the table back in
ALTER TABLE BPASESSION
    ADD CONSTRAINT [FK_BPASession_BPAProcess] FOREIGN KEY 
    (
        [processid]
    ) REFERENCES [BPAProcess] (
        [processid]
    )
GO
ALTER TABLE BPAPROCESSLOCK
    ADD CONSTRAINT [FK_BPAProcessLock_BPAProcess] FOREIGN KEY 
    (
        [processid]
    ) REFERENCES [BPAProcess] (
        [processid]
    )
GO
ALTER TABLE BPAREALTIMESTATSVIEW
    ADD CONSTRAINT [FK_BPAProcess] FOREIGN KEY 
    (
        [ProcessID]
    ) REFERENCES [BPAProcess] (
        [processid]
    )
GO
ALTER TABLE BPASCENARIOLINK
    ADD CONSTRAINT [FK_BPAScenarioLink_BPAProcess] FOREIGN KEY 
    (
        [processid]
    ) REFERENCES [BPAProcess] (
        [processid]
    )
GO



--set DB version
INSERT INTO BPADBVersion VALUES (
  '49',
  GETUTCDATE(),
  'db_upgradeR49.sql UTC',
  'Database amendments - Recreated process table with new process type field for business objects.'
)

