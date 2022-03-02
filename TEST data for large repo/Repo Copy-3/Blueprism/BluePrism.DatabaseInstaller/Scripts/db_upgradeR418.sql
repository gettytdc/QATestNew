/*
SCRIPT         : 418
AUTHOR         : Dmitriy Karpliuk
PURPOSE        : Added nonclustered index IX_BPATaskSession_processid_taskid_INC to optimize queries
*/

create nonclustered index IX_BPATaskSession_processid_taskid_INC
on BPATaskSession (
                      processid,
                      taskid
                  )
include (resourcename);
go

-- Set DB version.
insert into BPADBVersion
(dbversion, 
 scriptrundate, 
 scriptname, 
 [description], 
 timezoneoffset
)
values
('418', 
 getutcdate(), 
 'db_upgradeR418.sql', 
 'Added nonclustered index IX_BPATaskSession_processid_taskid_INC to optimize queries', 
 0
);
