/*
SCRIPT         : 57
PROJECT NAME   : Automate
DATABASE NAME  : BPA
CREATION DATE  : June 2007
AUTHOR         : JC
PURPOSE        : Amend session and log tables to work with session number rather than session id.
NOTES          : 
*/

--Drop session table relationships
ALTER TABLE BPASession DROP
CONSTRAINT FK_BPASession_BPAResource,
CONSTRAINT FK_BPASession_BPAResource1,
CONSTRAINT FK_BPASession_BPAStatus,
CONSTRAINT FK_BPASession_BPAUser,
CONSTRAINT FK_BPASession_BPAProcess
GO

--Rename session table, PK constraint and indices
EXEC sp_rename 'BPASession.Index_processID','Index_processID_OLD'
EXEC sp_rename 'BPASession.Index_statusID','Index_statusID_OLD'
EXEC sp_rename 'PK_BPASession','PK_BPASession_OLD'
EXEC sp_rename 'BPASession','BPASession_OLD'
GO

--Drop log table relationships
ALTER TABLE BPASessionLog DROP 
CONSTRAINT FK_BPASessionLog_BPASession
GO

--Rename log table, PK constraint and indices
EXEC sp_rename 'BPASessionLog.Index_sessionID','Index_sessionID_OLD'
EXEC sp_rename 'PK_BPASessionLog','PK_BPASessionLog_OLD'
EXEC sp_rename 'BPASessionLog','BPASessionLog_OLD'
GO


--Create a new session table
CREATE TABLE BPASession (
    sessionid UNIQUEIDENTIFIER NOT NULL ,
    sessionnumber INT IDENTITY (1, 1) NOT NULL ,
    startdatetime DATETIME NULL ,
    enddatetime DATETIME NULL ,
    processid UNIQUEIDENTIFIER NULL ,
    starterresourceid UNIQUEIDENTIFIER NULL ,
    starteruserid UNIQUEIDENTIFIER NULL ,
    runningresourceid UNIQUEIDENTIFIER NULL ,
    runningosusername VARCHAR (50)  ,
    statusid INT NULL ,
    startparamsxml TEXT  ,
    logginglevelsxml TEXT  ,
    sessionstatexml TEXT  ,
    CONSTRAINT PK_BPASession PRIMARY KEY  CLUSTERED 
    (
        sessionid
    ),
    CONSTRAINT Index_sessionnumber UNIQUE  NONCLUSTERED 
    (
        sessionnumber
    ),
    CONSTRAINT FK_BPASession_BPAProcess FOREIGN KEY 
    (
        processid
    ) REFERENCES BPAProcess (
        processid
    ),
    CONSTRAINT FK_BPASession_BPAResource FOREIGN KEY 
    (
        starterresourceid
    ) REFERENCES BPAResource (
        resourceid
    ),
    CONSTRAINT FK_BPASession_BPAResource1 FOREIGN KEY 
    (
        runningresourceid
    ) REFERENCES BPAResource (
        resourceid
    ),
    CONSTRAINT FK_BPASession_BPAStatus FOREIGN KEY 
    (
        statusid
    ) REFERENCES BPAStatus (
        statusid
    ),
    CONSTRAINT FK_BPASession_BPAUser FOREIGN KEY 
    (
        starteruserid
    ) REFERENCES BPAUser (
        userid
    )
) 
ALTER TABLE BPASession NOCHECK CONSTRAINT FK_BPASession_BPAStatus
CREATE INDEX Index_processID ON BPASession(processid) 
CREATE INDEX Index_statusID ON BPASession(statusid)
GO

--Create the new log table
CREATE TABLE BPASessionLog (
    sessionnumber INT NOT NULL ,
    seqnum INT NOT NULL ,
    stageid UNIQUEIDENTIFIER NULL ,
    stagename VARCHAR (50) NULL ,
    stagetype INT NULL ,
    processname VARCHAR (50) NULL ,
    pagename VARCHAR (50) NULL ,
    objectname VARCHAR (50) NULL ,
    actionname VARCHAR (50) NULL ,
    result TEXT NULL ,
    resulttype INT NULL ,
    startdatetime DATETIME NULL ,
    enddatetime DATETIME NULL ,
    attributexml TEXT NULL ,
    CONSTRAINT PK_BPASessionLog PRIMARY KEY  CLUSTERED 
    (
        sessionnumber,
        seqnum
    ),
    CONSTRAINT FK_BPASessionLog_BPASession FOREIGN KEY 
    (
        sessionnumber
    ) REFERENCES BPASession (
        sessionnumber
    )
)
CREATE INDEX Index_SessionStageType ON BPASessionLog(stagetype, sessionnumber)

GO


--set DB version
INSERT INTO BPADBVersion VALUES (
  '57',
  GETUTCDATE(),
  'db_upgradeR57.sql UTC',
  'Amend session and log tables to work with session number rather than session id.'
)


