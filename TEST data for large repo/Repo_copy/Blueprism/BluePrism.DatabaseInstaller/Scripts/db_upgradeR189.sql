/*
SCRIPT         : 189
AUTHOR         : SW
PURPOSE        : Adds lock information to process/objects group view
*/

-- Alter the grouped processes/objects view to include lock information
alter view BPVGroupedProcessesObjects as
select
    g.treeid as treeid,
    g.id as groupid,
    g.name as groupname,
    p.processid as id,
    p.name as name,
    p.ProcessType as processtype,
    p.description as description,
    p.createdate as createddate,
    cu.username as createdby,
    p.lastmodifieddate as lastmodifieddate,
    mu.username as lastmodifiedby,
    p.attributeid as attributes,
    pl.lockdatetime as lockdatetime,
    pl.userid as lockuser,
    pl.resourceid as lockresource
  from BPAProcess p
    join BPAUser cu on p.createdby = cu.userid
    join BPAUser mu on p.lastmodifiedby = mu.userid
    left join (
        BPAGroupProcess gp
            inner join BPAGroup g on gp.groupid = g.id
    ) on gp.processid = p.processid
    left join BPAProcessLock pl on pl.processid = p.processid;
GO

exec sp_refreshview 'BPVGroupedProcesses';
exec sp_refreshview 'BPVGroupedObjects';

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '189',
  GETUTCDATE(),
  'db_upgradeR189.sql UTC',
  'Adds lock information to process/objects group view'
);
