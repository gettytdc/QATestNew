IF EXISTS(SELECT * FROM sys.indexes WHERE object_id = object_id('BPASession') AND NAME ='Index_statusID')
    DROP INDEX [Index_statusID] ON [BPASession];
GO

IF EXISTS(SELECT * FROM sys.indexes WHERE object_id = object_id('BPASession') AND NAME ='Index_BPASession_statusid')
    DROP INDEX [Index_BPASession_statusid] ON [BPASession];
GO
CREATE INDEX [Index_BPASession_statusid] ON [BPASession] ([statusid])  INCLUDE ([processid], [starterresourceid], [starteruserid], [runningresourceid], [queueid]) WITH (FILLFACTOR=90)

IF EXISTS(SELECT * FROM sys.indexes WHERE object_id = object_id('BPASession') AND NAME ='Index_BPASession_Queueid_Status_Not_6')
    DROP INDEX [Index_BPASession_Queueid_Status_Not_6] ON [BPASession];
GO
CREATE NONCLUSTERED INDEX [Index_BPASession_Queueid_Status_Not_6] ON [BPASession] (Queueid) WHERE StatusId <> 6 WITH (FILLFACTOR=90)

-- Create index to improve performance of Work Queue VBO action 'GetLockedItems'
IF EXISTS(SELECT * FROM sys.indexes WHERE object_id = object_id('BPACaseLock') AND NAME ='Index_BPACaseLock_locktime')
    DROP INDEX [Index_BPACaseLock_locktime] ON [BPACaseLock];
GO
CREATE NONCLUSTERED INDEX [Index_BPACaseLock_locktime]
ON [BPACaseLock] ([locktime])
INCLUDE ([id])
WITH (FILLFACTOR = 90)

-- Set DB version.
INSERT INTO BPADBVersion(dbversion,
                         scriptrundate,
                         scriptname,
                         [description],
                         timezoneoffset)
VALUES('275',
       getutcdate(),
       'db_upgradeR275.sql',
       'Changed Indexing of BPASession and BPACaseLock',
       0);