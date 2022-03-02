/*
SCRIPT         : 133
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : CG
PURPOSE        : Add tables for environment variables
*/

CREATE TABLE BPAEnvironmentVar (
    name varchar(64) NOT NULL,
    datatype varchar(16) NOT NULL,
    value text NOT NULL,
    description text NOT NULL,
    CONSTRAINT PK_BPAEnvironmentVar PRIMARY KEY  CLUSTERED (
        name
    )
)

CREATE TABLE BPAProcessEnvVar (
    processid uniqueidentifier NOT NULL,
    name varchar(64) NOT NULL,
    CONSTRAINT PK_BPAProcessEnvVar PRIMARY KEY  CLUSTERED (
        processid,name
    ),
    CONSTRAINT FK_BPAProcessEnvVar_BPAProcess FOREIGN KEY (
        processid
    ) REFERENCES BPAProcess (
        processid
    )
)

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '133',
  GETUTCDATE(),
  'db_upgradeR133.sql UTC',
  'Add tables for environment variables'
)
