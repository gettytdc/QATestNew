/*
SCRIPT         : 201
AUTHOR         : GMB
PURPOSE        : Adds machinename column to BPAProcessLock
*/

-- Add machinename to the processlock table
alter table BPAProcessLock add machinename nvarchar(128) null;
GO

-- Remove the resourceid from the processlock table
alter table BPAProcessLock drop constraint FK_BPAProcessLock_BPAResource;
update BPAProcessLock set machinename = r.name from BPAProcessLock l join BPAResource r on l.resourceid = r.resourceid;
alter table BPAProcessLock drop column resourceid;
GO

-- Remvove the depencency on the resourceid from the groups view
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
    pl.machinename as lockmachinename
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

-- Set DB version
INSERT INTO BPADBVersion VALUES (
  '201',
  GETUTCDATE(),
  'db_upgradeR201.sql UTC',
  'Adds machinename column to BPAProcessLock'
);
