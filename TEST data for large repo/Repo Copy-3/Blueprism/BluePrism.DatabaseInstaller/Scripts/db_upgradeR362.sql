/*
SCRIPT         : 362
PURPOSE        : Add controller column to BPVPools View
AUTHOR         : Will Forster
*/

create index [IX_BPATaskSession_taskid_resourcename] on [BPATaskSession] ([taskid] asc, [resourcename] asc)
go

create index [IX_BPATask_scheduleid_INC] on [BPATask] ([scheduleid] asc) include ([name])
go

drop index [IX_BPATaskSession_taskid] on bpatasksession
go
 
if not exists(select 1 from sys.views where name = 'BPVPools')
  exec (N'create view BPVPools as select 1 as placeholder');
go

alter view BPVPools as
select
    g.treeid as treeid,
    g.id as groupid,
    g.[name] as groupname,
    r.resourceid as id,
    r.[name] as name,
    r.attributeid as attributes,
    r.statusid as statusid,
	r.controller as controller
from BPAResource r
      left join (
        BPAGroupResource gr
            inner join BPAGroup g on gr.groupid = g.id
      ) on gr.memberid = r.resourceid
where attributeId & 8 = 8;
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
('362', 
 GETUTCDATE(), 
 'db_upgradeR360.sql', 
 'Add new BPADataPipelineSettings serverport column', 
 0
);