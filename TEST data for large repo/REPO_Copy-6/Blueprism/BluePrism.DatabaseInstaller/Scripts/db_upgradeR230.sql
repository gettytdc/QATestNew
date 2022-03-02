/*
SCRIPT         : 230
PURPOSE        : Use Legacy Namespace Flag
AUTHOR      : John Brown
*/

ALTER TABLE BPAProcess
ADD useLegacyNamespace Bit NOT NULL DEFAULT(1);

GO

-- Re-define view with additional web service name column
ALTER VIEW BPVGroupedProcessesObjects as
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
    pl.machinename as lockmachinename,
    p.wspublishname as webservicename,
    p.forceLiteralForm as forceDocumentLiteral,
    p.useLegacyNamespace as useLegacyNamespace
 from BPAProcess p
    join BPAUser cu on p.createdby = cu.userid
    join BPAUser mu on p.lastmodifiedby = mu.userid
    left join (
        BPAGroupProcess gp
            inner join BPAGroup g on gp.groupid = g.id
    ) on gp.processid = p.processid
    left join BPAProcessLock pl on pl.processid = p.processid;

GO
-- Refresh any dependent view
exec sp_refreshview 'BPVGroupedProcesses';
exec sp_refreshview 'BPVGroupedObjects';

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '230',
  GETUTCDATE(),
  'db_upgradeR230.sql',
  'Added field to state if a legacy namespace should be used.',
  0 -- UTC
);
