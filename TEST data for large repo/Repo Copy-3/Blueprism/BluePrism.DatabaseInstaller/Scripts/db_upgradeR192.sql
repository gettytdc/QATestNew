
-- Add 'ispoolmember' to the grouped resources view
alter view BPVGroupedResources as
select
    g.treeid as treeid,
    g.id as groupid,
    g.name as groupname,
    r.resourceid as id,
    r.name as name,
    r.attributeid as attributes,
    case when r.pool is not null then 1 else 0 end as ispoolmember,
    r.status as status
    from BPAResource r
      left join (
        BPAGroupResource gr
            inner join BPAGroup g on gr.groupid = g.id
      ) on gr.memberid = r.resourceid;
GO


-- set DB version
INSERT INTO BPADBVersion VALUES (
  '192',
  GETUTCDATE(),
  'db_upgradeR192.sql UTC',
  'Adds info about pool membership to resource group view'
);

