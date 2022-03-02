/*
SCRIPT         : 51
PROJECT NAME   : Automate
DATABASE NAME  : BPA
CREATION DATE  : 09 Feb 2007
AUTHOR         : JC
PURPOSE        : Create 'tool position' table and add a 'save positions' flag to user table.
NOTES          : 
*/

CREATE TABLE [BPAToolPosition] (
    [UserID] UNIQUEIDENTIFIER,
    [Name] [varchar] (100),
    [Position] [char] (1),
    [X] [int] NULL ,
    [Y] [int] NULL ,
    [Mode] [char] (1),
    [Visible] BIT NULL
)

GO

ALTER TABLE BPAUser ADD SaveToolStripPositions BIT NULL

GO

--set DB version
INSERT INTO BPADBVersion VALUES (
  '51',
  GETUTCDATE(),
  'db_upgradeR51.sql UTC',
  'Database amendments - create tool position table and add a save positions flag to user table.'
)
