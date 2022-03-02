/*
SCRIPT         : 334
AUTHOR         : Brett Hewitt
PURPOSE        : Add index on BPASession for Warning Threshold and modify index for Queueid_Status_Not_6
*/

drop index [BPASession].[Index_BPASession_Queueid_Status_Not_6];
create nonclustered index [Index_BPASession_Queueid_Status_Not_6] on [BPASession] ([queueid] asc) include (statusId) where ([statusId]<>(6)) with (fillfactor=90);

create nonclustered index [IX_statusid_warningthreshold] on [BPASession] ([statusid] asc, [warningthreshold] asc) include ([runningresourceid], [lastupdated], [lastupdatedtimezoneoffset]) with (fillfactor=90);

-- Set DB version.
insert into BPADBVersion
(dbversion, 
 scriptrundate, 
 scriptname, 
 [description], 
 timezoneoffset
)
values
('334', 
 GETUTCDATE(), 
 'db_upgradeR334.sql', 
 'Add index on BPASession for Warning Threshold and modify index for Queueid_Status_Not_6', 
 0
);