/*

SCRIPT         : 60
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : PJW
PURPOSE        : Simplify BPAProcessBackup table
*/

DROP TABLE BPAProcessBackup
GO

CREATE TABLE [BPAProcessBackup] (
    [processid] [uniqueidentifier] NOT NULL,
    [UserID] [uniqueidentifier] NOT NULL ,
    [backupdate] [datetime] NULL ,
    [processxml] [text] COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
    CONSTRAINT [PK_BPAProcessBackup] PRIMARY KEY  CLUSTERED 
    (
        [processid]
    ),
    CONSTRAINT [FK_BPAProcessBackup_BPAProcess] FOREIGN KEY 
    (
        [processid]
    ) REFERENCES [BPAProcess] (
        [processid]
    ),
)
GO



--set DB version
INSERT INTO BPADBVersion VALUES (
  '61',
  GETUTCDATE(),
  'db_upgradeR61.sql UTC',
  'Database amendments - Simplify BPAProcessBackup table, and create the missing key constraints (bug 1529).'
)
GO
