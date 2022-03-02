IF EXISTS(SELECT * FROM sys.indexes WHERE object_id = object_id('BPAWorkQueueItem') AND NAME ='Index_BPAWorkQueueItem_queuepriorityloaded')
    DROP INDEX [Index_BPAWorkQueueItem_queuepriorityloaded] ON [BPAWorkQueueItem];
GO 
CREATE NONCLUSTERED INDEX [Index_BPAWorkQueueItem_queuepriorityloaded] ON [BPAWorkQueueItem]
( 
 [queueident] ASC, 
 [priority] ASC, 
 [loaded] ASC 
) 
INCLUDE (sessionId, finished, keyvalue, deferred, id) 
WITH (FILLFACTOR = 90)

-- Set DB version.
INSERT INTO BPADBVersion(dbversion,
                         scriptrundate,
                         scriptname,
                         [description],
                         timezoneoffset)
VALUES('269',
       getutcdate(),
       'db_upgradeR269.sql',
       'Change index on BPAWorkQueueItems',
       0);
