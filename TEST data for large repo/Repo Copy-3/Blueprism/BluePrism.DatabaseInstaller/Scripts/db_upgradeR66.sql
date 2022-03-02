/*

SCRIPT         : 66
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : PJW
PURPOSE        : Add fields and tables required for single sign-on (Active Directory)
*/


ALTER TABLE BPAUser ALTER COLUMN [UserName] VARCHAR(128)

 
ALTER TABLE BPARole
    ADD SingleSignonUserGroup Varchar(256)
GO

ALTER TABLE BPASysConfig
    ADD ActiveDirectoryProvider VarChar(4096) NOT NULL DEFAULT ''
GO

CREATE TABLE BPAInternalAuth (
    UserID UNIQUEIDENTIFIER NOT NULL,
    Token UNIQUEIDENTIFIER NOT NULL,
    Expiry DATETIME NOT NULL,
    CONSTRAINT PK_BPAInternalAuth PRIMARY KEY CLUSTERED (
        [Token]
    ),
    CONSTRAINT [FK_BPAInternalAuth_BPAUser] FOREIGN KEY (
        [UserID]
    ) REFERENCES [BPAUser] (
        [UserID]
    )
)
GO

--set DB version
INSERT INTO BPADBVersion VALUES (
  '66',
  GETUTCDATE(),
  'db_upgradeR66.sql UTC',
  'Database amendments - Add new fields and table for single sign-on.'
)
GO
