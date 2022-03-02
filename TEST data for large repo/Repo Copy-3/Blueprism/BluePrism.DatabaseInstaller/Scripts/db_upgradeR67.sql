/*
SCRIPT         : 67
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : CEG
PURPOSE        : Add tables required for Work Queues development
*/

CREATE TABLE BPAWorkQueue (
    id UNIQUEIDENTIFIER NOT NULL,
    name VARCHAR(30) UNIQUE NOT NULL,
    keyfield VARCHAR(30) NOT NULL,
    running BIT NOT NULL,
    maxattempts INT DEFAULT 0 NOT NULL,
    CONSTRAINT PK_BPAWorkQueue PRIMARY KEY (id),
    CONSTRAINT Index_name UNIQUE (name)
)
GO

CREATE TABLE BPAWorkQueueItem (
    id UNIQUEIDENTIFIER NOT NULL,
    queueid UNIQUEIDENTIFIER NOT NULL,
    resourceid UNIQUEIDENTIFIER DEFAULT NULL,
    keyvalue VARCHAR(30),
    status VARCHAR(30) DEFAULT '',
    attempts INT DEFAULT 0,
    loaded DATETIME NULL,
    locked DATETIME NULL,
    completed DATETIME NULL,
    exception DATETIME NULL,
    exceptionreason TEXT NULL,
    deferred DATETIME,
    worktime INT DEFAULT 0,
    data TEXT NULL,
    CONSTRAINT PK_BPAWorkQueueItem PRIMARY KEY (id),
    CONSTRAINT FK_BPAWorkQueueItem_BPAWorkQueue FOREIGN KEY (queueid)
        REFERENCES BPAWorkQueue (id),
    CONSTRAINT FK_BPAWorkQueueItem_BPAResource FOREIGN KEY (resourceid)
        REFERENCES BPAResource (resourceid),
);
CREATE INDEX Index_queueid ON BPAWorkQueueItem(queueid);
CREATE INDEX Index_BPAWorkQueueItem_key ON BPAWorkQueueItem(keyvalue);
CREATE INDEX Index_BPAWorkQueueItem_loaded ON BPAWorkQueueItem(loaded);
CREATE INDEX Index_BPAWorkQueueItem_locked ON BPAWorkQueueItem(locked);
CREATE INDEX Index_BPAWorkQueueItem_completed ON BPAWorkQueueItem(completed);
CREATE INDEX Index_BPAWorkQueueItem_exception ON BPAWorkQueueItem(exception);
GO


--set DB version
INSERT INTO BPADBVersion VALUES (
  '67',
  GETUTCDATE(),
  'db_upgradeR67.sql UTC',
  'Added tables required for Work Queues development'
)
GO
