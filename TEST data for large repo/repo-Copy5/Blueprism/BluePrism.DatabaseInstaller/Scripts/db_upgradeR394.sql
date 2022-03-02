/*
SCRIPT         : 394
AUTHOR         : Neal Callaghan
PURPOSE        : Removal of legacy column requiredFeature from BPAWorkQueue
*/

delete from BPAWorkQueueItem
where queueid in (select id from BPAWorkQueue where requiredFeature <> '')

delete from BPAWorkQueue
where requiredFeature <> ''

go

alter table BPAWorkQueue
drop constraint BPAWorkQueue_default_requiredFeature

drop index INDEX_WorkQueueName on BPAWorkQueue

create unique nonclustered index INDEX_WorkQueueName on BPAWorkQueue ([name] asc) include ([id], [keyfield], [running], [maxattempts], [encryptid], [processid], [snapshotconfigurationid], [resourcegroupid])
	with (PAD_INDEX = off, STATISTICS_NORECOMPUTE = off, SORT_IN_TEMPDB = off, IGNORE_DUP_KEY = off, DROP_EXISTING = off, ONLINE = off, ALLOW_ROW_LOCKS = on, ALLOW_PAGE_LOCKS = on) on [PRIMARY]

alter table BPAWorkQueue
drop column requiredFeature

-- Set DB version.
insert into BPADBVersion
(dbversion, 
 scriptrundate, 
 scriptname, 
 [description], 
 timezoneoffset
)
values
('394', 
 GETUTCDATE(), 
 'db_upgradeR394.sql', 
 'Removal of legacy column requiredFeature from BPAWorkQueue', 
 0
);
