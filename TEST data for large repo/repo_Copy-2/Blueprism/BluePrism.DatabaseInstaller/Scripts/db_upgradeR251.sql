/*
BUG/STORY      : US-1785
PURPOSE        : Amend BPVGroupedResources to return resource pools in groups. 
                 Added to by bg-1803 - return real status or resource
*/

alter view BPVGroupedResources as
    select g.treeid,
        (case when r.pool is not null then r.pool else g.id end) as groupid,
        g.name as groupname, 
        r.resourceid as id,
        r.name,
        r.AttributeID AS attributes,
        case when r.pool is not null then 1 else 0 end as ispoolmember,
        r.status
    from
        BPAResource as r 
        left outer join BPAGroupResource as gr
        inner join BPAGroup as g on gr.groupid = g.id on gr.memberid = r.resourceid
go

-- set DB version
insert into BPADBVersion values (
  '251',
  getutcdate(),
  'db_upgradeR251.sql',
  'Amend BPVGroupedResources to return resource pools in groups',
  0 -- UTC
);
