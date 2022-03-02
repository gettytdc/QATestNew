/*
SCRIPT         : 70
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : PJW
PURPOSE        : Create new table for storing work queue filters
*/

--New table to store filtered views of queues in control room
CREATE TABLE BPAWorkQueueFilter (
    FilterID uniqueidentifier NOT NULL,
    FilterName VARCHAR(32) unique,
    FilterXML [TEXT]  NOT NULL,
    CONSTRAINT [PK_BPAWorkQueueFilter] PRIMARY KEY  CLUSTERED 
    (
        [FilterID]
    )
)
GO


--New column to store the default view for a particular queue
ALTER TABLE BPAWorkQueue 
    ADD DefaultFilterID uniqueidentifier NULL
GO
ALTER TABLE BPAWorkQueue 
    ADD CONSTRAINT [FK_BPAWorkQueue_BPAWorkQueueFilter] FOREIGN KEY 
    (
        [DefaultFilterID]
    ) REFERENCES [BPAWorkQueueFilter] (
        [FilterID]
    )


--set DB version
INSERT INTO BPADBVersion VALUES (
  '70',
  GETUTCDATE(),
  'db_upgradeR70.sql UTC',
  'New table to store different filtered views of work queues'
)
GO
