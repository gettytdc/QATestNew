/*
SCRIPT         : 119
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Add Queue auditing for the new Fidelity pricing model
*/

create table BPAWorkQueueLog (
    logid bigint identity not null primary key nonclustered, -- we'll cluster on eventtime; this is just to uniqueify the record.
    eventtime datetime not null,
    queueident int not null,
    queueop tinyint not null,
    itemid uniqueidentifier null,
    keyvalue varchar(255) not null
);
GO
-- Some index on the common search fields
-- Cluster on eventtime - it's *always* going to be used in a query, and we'll always
-- want a range (between m and n) and usually want to GROUP BY it...
create clustered index INDEX_BPAWorkQueueLog_eventtime on BPAWorkQueueLog(eventtime);
create index INDEX_BPAWorkQueueLog_queueident on BPAWorkQueueLog(queueident);
create index INDEX_BPAWorkQueueLog_queueop on BPAWorkQueueLog(queueop);

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '119',
  GETUTCDATE(),
  'db_upgradeR119.sql UTC',
  'Add Queue auditing for the new pricing model'
);
