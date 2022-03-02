/*
SCRIPT         : 76
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : CEG
PURPOSE        : Create tables required for credentials management
*/

CREATE TABLE BPACredentials (
    id uniqueidentifier NOT NULL,
    name varchar(64) NOT NULL,
    description text NOT NULL,
    login varchar(64) NOT NULL,
    password varchar(64) NOT NULL,
    roleid bigint NULL,
    
    CONSTRAINT FK_BPACredentials_RoleID FOREIGN KEY
    (
        roleid
    )
    REFERENCES BPARole
    (
        roleid
    ),
    CONSTRAINT PK_BPACredentials PRIMARY KEY CLUSTERED
    (
        id
    ),
    CONSTRAINT Index_BPACredentials_name UNIQUE
    (
        name
    )
)
GO

CREATE TABLE BPACredentialsProcesses (
    credentialid uniqueidentifier NOT NULL,
    processid uniqueidentifier NOT NULL
    
    CONSTRAINT FK_BPACredentialsProcesses_cred FOREIGN KEY
    (
        credentialid
    )
    REFERENCES BPACredentials
    (
        id
    ),  
    CONSTRAINT FK_BPACredentialsProcesses_proc FOREIGN KEY
    (
        processid
    )
    REFERENCES BPAProcess
    (
        processid
    )   
)
GO

CREATE TABLE BPACredentialsResources (
    credentialid uniqueidentifier NOT NULL,
    resourceid uniqueidentifier NOT NULL
    
    CONSTRAINT FK_BPACredentialsResources_cred FOREIGN KEY
    (
        credentialid
    )
    REFERENCES BPACredentials
    (
        id
    ),  
    CONSTRAINT FK_BPACredentialsResources_res FOREIGN KEY
    (
        resourceid
    )
    REFERENCES BPAResource
    (
        resourceid
    )   
)
GO

--set DB version
INSERT INTO BPADBVersion VALUES (
  '76',
  GETUTCDATE(),
  'db_upgradeR76.sql UTC',
  'Create tables required for credentials management'
)
GO
