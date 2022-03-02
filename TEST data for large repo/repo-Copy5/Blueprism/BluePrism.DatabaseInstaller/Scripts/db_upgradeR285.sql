-- NOT NULL for BPAWorkQueue columns
ALTER TABLE BPAWorkQueue DROP CONSTRAINT Index_name
GO
UPDATE BPAWorkQueue SET [name]='' WHERE [name] IS NULL
ALTER TABLE BPAWorkQueue ALTER COLUMN [name] NVARCHAR(255) NOT NULL
ALTER TABLE BPAWorkQueue ADD CONSTRAINT Index_name UNIQUE NONCLUSTERED([name])
GO

-- NOT NULL for BPAProcess columns
DECLARE @userid UNIQUEIDENTIFIER
SET @userid = (SELECT TOP 1 userid FROM BPAUser)

UPDATE BPAProcess SET [name]='' WHERE [name] IS NULL
ALTER TABLE BPAProcess ALTER COLUMN [name] NVARCHAR(128) NOT NULL

UPDATE BPAProcess SET ProcessType='P' WHERE ProcessType IS NULL
ALTER TABLE BPAProcess ALTER COLUMN ProcessType NVARCHAR(1) NOT NULL

UPDATE BPAProcess SET createdate=CAST('1753-1-1' AS DATETIME) WHERE createdate IS NULL
ALTER TABLE BPAProcess ALTER COLUMN createdate DATETIME NOT NULL

UPDATE BPAProcess SET createdby=@userid WHERE createdby IS NULL
ALTER TABLE BPAProcess ALTER COLUMN createdby UNIQUEIDENTIFIER NOT NULL

UPDATE BPAProcess SET lastmodifieddate=GETUTCDATE() WHERE lastmodifieddate IS NULL
ALTER TABLE BPAProcess ALTER COLUMN lastmodifieddate DATETIME NOT NULL

UPDATE BPAProcess SET lastmodifiedby=@userid WHERE lastmodifiedby IS NULL
ALTER TABLE BPAProcess ALTER COLUMN lastmodifiedby UNIQUEIDENTIFIER NOT NULL

-- NOT NULL for BPASession columns
IF EXISTS(SELECT * FROM sys.indexes WHERE object_id = object_id('BPASession') AND NAME ='Index_BPASession_statusid')
    DROP INDEX [Index_BPASession_statusid] ON [BPASession];
GO
IF EXISTS(SELECT * FROM sys.indexes WHERE object_id = object_id('BPASession') AND NAME ='Index_BPASession_Queueid_Status_Not_6')
    DROP INDEX [Index_BPASession_Queueid_Status_Not_6] ON [BPASession];
GO
UPDATE BPASession SET statusid=0 WHERE statusid IS NULL
ALTER TABLE BPASession ALTER COLUMN statusid INT NOT NULL
CREATE INDEX Index_BPASession_statusid ON BPASession (statusid) INCLUDE (processid,starterresourceid,starteruserid,runningresourceid,queueid) WITH (FILLFACTOR = 90)
CREATE NONCLUSTERED INDEX Index_BPASession_Queueid_Status_Not_6 ON BPASession (Queueid) WHERE StatusId <> 6 WITH (FILLFACTOR = 90)
GO

-- NOT NULL for BPAResource columns
ALTER TABLE BPAResource DROP CONSTRAINT UNQ_BPAResource_name
GO
UPDATE BPAResource SET [name]='' WHERE [name] IS NULL
ALTER TABLE BPAResource ALTER COLUMN [name] NVARCHAR(128) NOT NULL
GO
ALTER TABLE BPAResource ADD CONSTRAINT UNQ_BPAResource_name unique (name)

-- Create additional indexes for BPAResource
IF EXISTS(SELECT * FROM sys.indexes WHERE object_id = object_id('BPAResource') AND NAME ='IX_UNQ_BPAResource_Name')
    DROP INDEX [IX_UNQ_BPAResource_Name] ON [BPAResource];
GO
CREATE UNIQUE INDEX IX_UNQ_BPAResource_Name ON BPAResource([name]) INCLUDE (resourceid, fqdn) WITH (FILLFACTOR = 90)
IF EXISTS(SELECT * FROM sys.indexes WHERE object_id = object_id('BPAResource') AND NAME ='INDEX_BPAResource_SSL')
    DROP INDEX [INDEX_BPAResource_SSL] ON [BPAResource];
GO
CREATE NONCLUSTERED INDEX INDEX_BPAResource_SSL ON BPAResource([name]) INCLUDE ([SSL]) WITH (FILLFACTOR = 90)

-- StatusID as INT NOT NULL for BPAResource
ALTER TABLE BPAResource ADD statusid INT NOT NULL DEFAULT 0
GO
UPDATE BPAResource SET statusid=0 where [status]='Unknown'
UPDATE BPAResource SET statusid=1 where [status]='Ready'
UPDATE BPAResource SET statusid=2 where [status]='Offline'
UPDATE BPAResource SET statusid=3 where [status]='Pending'
GO

ALTER VIEW BPVGroupedResources AS
SELECT
    g.treeid AS treeid,
    (CASE WHEN r.[pool] IS NOT NULL THEN r.[pool] ELSE g.id END) AS groupid,
    g.name AS groupname,
    r.resourceid AS id,
    r.name AS name,
    r.attributeid AS attributes,
    CASE WHEN r.[pool] IS NOT NULL THEN 1 ELSE 0 END AS ispoolmember,
    1 AS statusid
FROM BPAResource r
      LEFT JOIN (
        BPAGroupResource gr
            INNER JOIN BPAGroup g ON gr.groupid = g.id
      ) ON gr.memberid = r.resourceid
WHERE attributeId & 8 = 0;
GO

ALTER VIEW BPVPools AS
SELECT
    g.treeid AS treeid,
    g.id AS groupid,
    g.name AS groupname,
    r.resourceid AS id,
    r.name AS name,
    r.attributeid AS attributes,
    r.statusid AS statusid
FROM BPAResource r
      LEFT JOIN (
        BPAGroupResource gr
            INNER JOIN BPAGroup g ON gr.groupid = g.id
      ) ON gr.memberid = r.resourceid
WHERE attributeId & 8 = 8;
GO

ALTER TABLE BPAResource DROP COLUMN DisplayStatus
GO
-- Add calculated resource display status
ALTER TABLE BPAResource ADD DisplayStatus AS (
    CASE
        WHEN (AttributeID & 13) <> 0 THEN null
        WHEN statusid = 2 THEN 'Offline'
        WHEN DATEDIFF(second, lastupdated, GETUTCDATE()) >= 60 THEN 'Missing'
        WHEN (AttributeID & 16) <> 0 THEN 'Logged Out'
        WHEN (AttributeID & 32) <> 0 THEN 'Private'
        WHEN actionsrunning = 0 THEN 'Idle'
        ELSE 'Working'
    END);
GO
ALTER TABLE BPAResource DROP COLUMN [status]

-- NOT NULL for BPAMIControl columns
UPDATE BPAMIControl SET mienabled = 0 WHERE mienabled IS NULL
ALTER TABLE BPAMIControl ALTER COLUMN mienabled BIT NOT NULL

UPDATE BPAMIControl SET autorefresh = 0 WHERE autorefresh IS NULL
ALTER TABLE BPAMIControl ALTER COLUMN autorefresh BIT NOT NULL

UPDATE BPAMIControl SET refreshinprogress = 0 WHERE refreshinprogress IS NULL
ALTER TABLE BPAMIControl ALTER COLUMN refreshinprogress BIT NOT NULL

UPDATE BPAMIControl SET dailyfor = 30 WHERE dailyfor IS NULL
ALTER TABLE BPAMIControl ALTER COLUMN dailyfor INT NOT NULL

UPDATE BPAMIControl SET monthlyfor = 6 WHERE monthlyfor IS NULL
ALTER TABLE BPAMIControl ALTER COLUMN monthlyfor INT NOT NULL

-- NOT NULL for BPATag columns
IF EXISTS(SELECT * FROM sys.indexes WHERE object_id = object_id('BPATag') AND NAME ='UNQ_BPATag_tag')
    DROP INDEX [UNQ_BPATag_tag] ON [BPATag];
GO
UPDATE BPATag SET tag = '' WHERE tag IS NULL
ALTER TABLE BPATag ALTER COLUMN tag NVARCHAR(255) NOT NULL
GO
CREATE UNIQUE INDEX UNQ_BPATag_tag ON BPATag(tag) WITH (FILLFACTOR = 90)

GO
-- Set DB version.
INSERT INTO BPADBVersion(dbversion,
                         scriptrundate,
                         scriptname,
                         [description],
                         timezoneoffset)
VALUES('285',
       GETUTCDATE(),
       'db_upgradeR285.sql',
       'Performance enhancements',
       0);