/*
SCRIPT         : 395
AUTHOR         : Daniel Errington
PURPOSE        : Remove unnecessary Decipher items
*/

alter view [BPVGroupedQueues] as
select
    g.treeid as treeid,
    g.id as groupid,
    g.name as groupname,
    q.ident as id,
    q.name as name,
    q.id as guid,
    q.running as running,
    q.encryptid as encryptid,
    q.processid as processid,
    q.resourcegroupid as resourcegroupid,
    case
      when q.processid is not null and q.resourcegroupid is not null then cast(1 as bit)
      else cast(0 as bit)
    end as isactive
    from BPAWorkQueue q
      left join (
        BPAGroupQueue gq
            inner join BPAGroup g on gq.groupid = g.id
      ) on gq.memberid = q.ident;

go

drop table [BPADocumentProcessingQueueOverride]
drop table [BPADocumentTypeDefaultQueue]
drop table [BPADocumentTypeQueues]

-- Set DB version.
insert into BPADBVersion
(dbversion, 
 scriptrundate, 
 scriptname, 
 [description], 
 timezoneoffset
)
values
('395', 
 GETUTCDATE(), 
 'db_upgradeR395.sql', 
 'Remove unnecessary Decipher items', 
 0
);
