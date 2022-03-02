/*
SCRIPT         : 71
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : PJW
PURPOSE        : Create new tables for organising processes into groups
*/

CREATE TABLE [BPAProcessGroup] (
    [GroupID] [uniqueidentifier] NOT NULL ,
    [GroupName] [varchar] (32) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL ,
    CONSTRAINT [PK_BPAProcessGroup] PRIMARY KEY  CLUSTERED 
    (
        [GroupID]
    ),
     UNIQUE  NONCLUSTERED 
    (
        [GroupName]
    )
)



CREATE TABLE [BPAProcessGroupMembership] (
    [GroupID] [uniqueidentifier] NOT NULL ,
    [ProcessID] [uniqueidentifier] NOT NULL ,
    CONSTRAINT [PK_BPAProcessGroupMembership] PRIMARY KEY  CLUSTERED 
    (
        [GroupID], [ProcessID]
    ),
    CONSTRAINT [FK_BPAProcessGroupMembership_BPAProcessGroup] FOREIGN KEY 
    (
        [GroupID]
    ) REFERENCES [BPAProcessGroup] (
        [GroupID]
    ),
    CONSTRAINT [FK_BPAProcessGroupMembership_BPAProcess] FOREIGN KEY 
    (
        [ProcessID]
    ) REFERENCES [BPAProcess] (
        [ProcessID]
    ),
)






--set DB version
INSERT INTO BPADBVersion VALUES (
  '71',
  GETUTCDATE(),
  'db_upgradeR71.sql UTC',
  'Create new tables for organising processes into groups'
)
GO
