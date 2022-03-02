alter view BPVGroupedResources as
select
    g.treeid as treeid,
    (case when r.pool is not null then r.pool else g.id end) as groupid,
    g.name as groupname,
    r.resourceid as id,
    r.name as name,
    r.attributeid as attributes,
    case when r.pool is not null then 1 else 0 end as ispoolmember,
    'Idle' as status
    from BPAResource r
      left join (
        BPAGroupResource gr
            inner join BPAGroup g on gr.groupid = g.id
      ) on gr.memberid = r.resourceid
    where attributeId & 8 = 0;
GO

if not exists(select * from sys.views where name = 'BPVPools')
  exec (N'create view BPVPools as select 1 as placeholder');
GO

alter view BPVPools as
select
    g.treeid as treeid,
    g.id as groupid,
    g.name as groupname,
    r.resourceid as id,
    r.name as name,
    r.attributeid as attributes,
    r.status as status
    from BPAResource r
      left join (
        BPAGroupResource gr
            inner join BPAGroup g on gr.groupid = g.id
      ) on gr.memberid = r.resourceid
      where attributeId & 8 = 8;
GO

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '245',
  GETUTCDATE(),
  'db_upgradeR245.sql',
  'Add pools to resource managment screen',
  0 -- UTC
);