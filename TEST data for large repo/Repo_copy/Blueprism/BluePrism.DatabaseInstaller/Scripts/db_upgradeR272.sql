IF EXISTS(SELECT * FROM sys.indexes WHERE object_id = object_id('BPAWorkQueueItem') AND NAME ='Index_BPAWorkQueueItem_queueident_exception')
    DROP INDEX [Index_BPAWorkQueueItem_queueident_exception] ON [BPAWorkQueueItem];
GO
CREATE NONCLUSTERED INDEX [Index_BPAWorkQueueItem_queueident_exception]
ON [BPAWorkQueueItem] ([queueident], [exception])
INCLUDE ([id],[attempt],[loaded])

IF EXISTS(SELECT * FROM sys.indexes WHERE object_id = object_id('BPAWorkQueueItem') AND NAME ='Index_BPAWorkQueueItem_queueident_completed')
    DROP INDEX [Index_BPAWorkQueueItem_queueident_completed] ON [BPAWorkQueueItem];
GO
CREATE NONCLUSTERED INDEX [Index_BPAWorkQueueItem_queueident_completed] 
ON [BPAWorkQueueItem] ([queueident] ASC, [completed] ASC)
INCLUDE ([id],[attempt],[loaded],[exception],[exceptionreason])

-- Set DB version.
INSERT INTO BPADBVersion(dbversion,
                         scriptrundate,
                         scriptname,
                         [description],
                         timezoneoffset)
VALUES('272',
       getutcdate(),
       'db_upgradeR272.sql',
       'BPAWorkQueueItem indexes used to optimise retrieval of actioned items',
       0);