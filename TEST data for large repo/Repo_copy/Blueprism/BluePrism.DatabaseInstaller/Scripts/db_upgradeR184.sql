/*
SCRIPT         : 184
AUTHOR         : SW
PURPOSE        : Adds db structure required for active work queue creation and configuration
*/

alter table BPAWorkQueue add
  processid uniqueidentifier null        -- The process which is used to work this queue
    constraint FK_BPAWorkQueue_BPAProcess
      foreign key references BPAProcess(processid),
  resourcegroupid uniqueidentifier null  -- The resource group assigned to this queue
    constraint FK_BPAWorkQueue_BPAGroup
      foreign key references BPAGroup(id),
  targetsessions int not null
    constraint DEF_BPAWorkQueue_targetsessions default 0;

alter table BPASession add
  queueid int null -- The ID of the queue which this resource is assigned to
    constraint FK_BPASession_BPAWorkQueue
      foreign key references BPAWorkQueue(ident),
  stoprequested datetime null,   -- utc: when the stop was requested
  stoprequestack datetime null;  -- utc: when the stop request was acknowledged (with a call to IsStopRequested())

-- New status value to indicate a stop request has been made
insert into BPAStatus (statusid, type, description)
  values (7, 'RUN', 'Stopping');

GO

-- VIEW HANDLING --

-- The BPVSession view needs to know about the new columns
exec sp_refreshview 'BPVSession';
GO

-- Include the queue id into the BPVSessionInfo view
alter view BPVSessionInfo as
select
    s.sessionid           as "sessionid",
    s.sessionnumber       as "sessionnumber",
    s.startdatetime       as "startdatetime",
    s.enddatetime         as "enddatetime",
    s.processid           as "processid",
    p.name                as "processname",
    s.starterresourceid   as "starterresourceid",
    sr.name               as "starterresourcename",
    s.starteruserid       as "starteruserid",
    isnull(su.username, '[' + su.systemusername + ']')
                          as "starterusername",
    s.runningresourceid   as "runningresourceid",
    rr.name               as "runningresourcename",
    s.runningosusername   as "runningosusername",
    s.statusid            as "statusid",
    s.startparamsxml      as "startparamsxml",
    s.logginglevelsxml    as "logginglevelsxml",
    s.sessionstatexml     as "sessionstatexml",
    s.queueid             as "queueid"
from BPASession s
    join BPAProcess p on s.processid = p.processid
    join BPAResource sr on s.starterresourceid = sr.resourceid
    join BPAResource rr on s.runningresourceid = rr.resourceid
    join BPAUser su on s.starteruserid = su.userid
where s.statusid <> 6;
GO

-- Update the queue groups view to include active data
if not exists (select 1 from BPADBVersion where dbversion='193') exec('
alter view BPVGroupedQueues as
select
    g.treeid as treeid,
    g.id as groupid,
    g.name as groupname,
    q.ident as id,
    q.name as name,
    q.id as guid,
    q.running as running,
    q.encryptname as encryptname,
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
      ) on gq.memberid = q.ident;');
GO

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '184',
  GETUTCDATE(),
  'db_upgradeR184.sql UTC',
  'Adds db structure required for active work queue creation and configuration'
);
