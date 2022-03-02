IF EXISTS(SELECT * FROM sys.indexes WHERE object_id = object_id('BPAWorkQueueItem') AND NAME ='Index_BPAWorkQueueItem_queueident_finished')
DROP INDEX [Index_BPAWorkQueueItem_queueident_finished] ON [BPAWorkQueueItem]
GO

CREATE NONCLUSTERED INDEX [Index_BPAWorkQueueItem_queueident_finished]
ON [BPAWorkQueueItem] ([queueident],[finished])
INCLUDE ([completed],[exception],[deferred],[attemptworktime])
WITH (FILLFACTOR = 90)
GO

-- Set DB version.
INSERT INTO BPADBVersion(dbversion,
                         scriptrundate,
                         scriptname,
                         [description],
                         timezoneoffset)
VALUES('274',
       getutcdate(),
       'db_upgradeR274.sql',
       'BPAWorkQueueItem indexes used to optimise retrieval of queue stats',
       0);