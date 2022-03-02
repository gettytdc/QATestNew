/*
SCRIPT         : 52
PROJECT NAME   : Automate
DATABASE NAME  : BPA
CREATION DATE  : 13 April 2007
AUTHOR         : JC
PURPOSE        : Add a password duration column to user table.
NOTES          : 
*/

ALTER TABLE BPAUser ADD PasswordDurationWeeks INTEGER NULL

GO

--set DB version
INSERT INTO BPADBVersion VALUES (
  '52',
  GETUTCDATE(),
  'db_upgradeR52.sql UTC',
  'Database amendments - add a password duration column to user table.'
)
