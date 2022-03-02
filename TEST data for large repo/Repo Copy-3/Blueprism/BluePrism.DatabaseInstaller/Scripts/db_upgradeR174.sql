/*
SCRIPT         : 174
PROJECT NAME   : Automate
DATABASE NAME  : BPA
CREATION DATE  : 29 Oct 2014
AUTHOR         : GM
PURPOSE        : Adds expiry date & invalid flag to Credentials and new table for Credential properties.
*/

ALTER TABLE BPACredentials ADD
    expirydate datetime NULL,
    invalid bit
GO

UPDATE BPACredentials SET invalid=0
GO

CREATE TABLE BPACredentialsProperties (
    id uniqueidentifier NOT NULL,
    credentialid uniqueidentifier NOT NULL,
    name nvarchar(255) NOT NULL,
    value nvarchar(max) NULL,

    CONSTRAINT PK_BPACredentialsProperties PRIMARY KEY CLUSTERED
    (
        id
    ),
    CONSTRAINT Index_BPACredentialsProperties_credentialidname UNIQUE
    (
        credentialid, name
    ),
    CONSTRAINT FK_BPACredentialsProperties_cred FOREIGN KEY
    (
        credentialid
    )
    REFERENCES BPACredentials
    (
        id
    )
)
GO

--set DB version
INSERT INTO BPADBVersion VALUES (
  '174',
  GETUTCDATE(),
  'db_upgradeR174.sql UTC',
  'Adds expiry date & invalid flag to Credentials and new table for Credential properties.'
)
