
--SCRIPT PURPOSE: Create a process back up table and add a new column to BPASysconfig
--NUMBER: 27
--AUTHOR: JFC
--DATE: 22/06/2005 

--Drop table if necessary
IF EXISTS (
  SELECT * FROM dbo.sysobjects 
  WHERE id = object_id(N'[BPAProcessBackup]') 
  AND OBJECTPROPERTY(id, N'IsUserTable') = 1
)
DROP TABLE [BPAProcessBackup]

--Create table
CREATE TABLE [BPAProcessBackup] (
    [processid] [uniqueidentifier] NOT NULL
        constraint PK_BPAProcessBackup primary key,
    [name] [varchar] (128) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
    [description] [varchar] (1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
    [version] [varchar] (20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
    [statusid] [int] NULL ,
    [createdate] [datetime] NULL ,
    [createdby] [uniqueidentifier] NULL ,
    [lastmodifieddate] [datetime] NULL ,
    [lastmodifiedby] [uniqueidentifier] NULL ,
    [processxml] [text] COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
    [backupdate] [datetime] NULL ,
    [restoredate] [datetime] NULL ,
    [backuplevel] [int] NULL 
)

--Add new column
ALTER TABLE BPASysconfig ADD autosaveinterval INT

--set DB version
INSERT INTO BPADBVersion VALUES (
  '27',
  GETUTCDATE(),
  'db_upgradeR27.sql UTC',
  'Database amendments - CREATE TABLE BPAProcessBackup and ALTER TABLE BPASysConfig'
)



