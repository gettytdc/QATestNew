/*
SCRIPT         : 44
PROJECT NAME   : Automate
DATABASE NAME  : BPA
NAME           : addwebservices
CREATION DATE  : 22 May 2006
AUTHOR         : PJW
PURPOSE        : Adds database support for process/resource retirement and development/live status
NOTES          : 
*/


--make new Attribute column and drop redundant status column
ALTER TABLE BPAProcess
    DROP CONSTRAINT FK_BPAProcess_BPAStatus
ALTER TABLE BPAProcess
    DROP COLUMN StatusID
ALTER TABLE BPAProcess
    ADD AttributeID int NOT NULL DEFAULT 0
GO

--Make existing Processes Live and non-retired
UPDATE BPAProcess SET AttributeID = 2
GO

--New table gives us friendly names for the statuses
CREATE TABLE BPAProcessAttribute (
    AttributeID int NOT NULL,
    AttributeName   VARCHAR(64) NOT NULL,
    CONSTRAINT [PK_BPAProcessStatus] PRIMARY KEY CLUSTERED
    (
        [AttributeID]
    )
)
GO

--Insert new attributes
INSERT INTO BPAProcessAttribute (AttributeID, AttributeName) VALUES (1, 'Retired')
INSERT INTO BPAProcessAttribute (AttributeID, AttributeName) VALUES (2, 'Live')
GO





--Add new column to resource table
ALTER TABLE BPAResource
    ADD AttributeID int NOT NULL DEFAULT 0
GO


--New table gives us friendly names for the statuses
CREATE TABLE BPAResourceAttribute (
    AttributeID int NOT NULL,
    AttributeName   VARCHAR(64) NOT NULL,
    CONSTRAINT [PK_BPAResourceStatus] PRIMARY KEY CLUSTERED
    (
        [AttributeID]
    )
)
GO

--create new attributes
INSERT INTO BPAResourceAttribute (AttributeID, AttributeName) VALUES (1, 'Retired')
GO





--set DB version
INSERT INTO BPADBVersion VALUES (
  '44',
  GETUTCDATE(),
  'db_upgradeR44.sql UTC',
  'Adds database support for process/resource retirement etc'
)
GO

