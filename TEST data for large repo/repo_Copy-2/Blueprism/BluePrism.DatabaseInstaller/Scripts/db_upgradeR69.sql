/*
SCRIPT         : 69
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : GMB
PURPOSE        : New table for exception types.
*/

CREATE TABLE BPAExceptionType (
    id UNIQUEIDENTIFIER NOT NULL,
    type VARCHAR(30) UNIQUE NOT NULL,
    constraint PK_BPAExceptionType primary key (id)
)
GO


--set DB version
INSERT INTO BPADBVersion VALUES (
  '69',
  GETUTCDATE(),
  'db_upgradeR69.sql UTC',
  'New table for exception types'
)
GO
