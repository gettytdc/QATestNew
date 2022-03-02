/*
SCRIPT         : 72
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : GMB
PURPOSE        : Create a new table for storing process MI templates
*/

CREATE TABLE [BPAProcessMITemplate] (
    [templatename] [varchar] (32) NOT NULL ,
    [processid] [uniqueidentifier] NOT NULL ,
    [defaulttemplate] [bit] NOT NULL,
    [templatexml] [text] NULL
)

--set DB version
INSERT INTO BPADBVersion VALUES (
  '72',
  GETUTCDATE(),
  'db_upgradeR72.sql UTC',
  'Create a new table for storing process MI templates'
)
GO
