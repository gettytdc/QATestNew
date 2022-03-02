/*
SCRIPT         : 220
AUTHOR         : GMB/GM
PURPOSE        : Provides additional resource info and updates Workforce Availability tile
*/

alter table BPASession add lastupdated datetime null;
alter table BPASession add laststage nvarchar(max) null;
alter table BPASession add warningthreshold integer null;
alter table BPAResource add [userID] uniqueidentifier null;
GO

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
    s.queueid             as "queueid",
    s.lastupdated         as "lastupdated",
    s.laststage           as "laststage",
    s.warningthreshold    as "warningthreshold"
from BPASession s
    join BPAProcess p on s.processid = p.processid
    join BPAResource sr on s.starterresourceid = sr.resourceid
    join BPAResource rr on s.runningresourceid = rr.resourceid
    join BPAUser su on s.starteruserid = su.userid
where s.statusid <> 6;
GO

-- Add calculated resource display status
alter table BPAResource add DisplayStatus as (
    case
        when (AttributeID & 13) <> 0 then null
        when [status] = 'Offline' then [status]
        when DATEDIFF(second, lastupdated, GETUTCDATE()) >= 60 then 'Missing'
        when (AttributeID & 16) <> 0 then 'Logged Out'
        when (AttributeID & 32) <> 0 then 'Private'
        when actionsrunning = 0 then 'Idle'
        else 'Working'
    end);
GO

-- Add new entries to resource attribute table
insert into BPAResourceAttribute (AttributeID, AttributeName)
values (16, 'Login Agent');
insert into BPAResourceAttribute (AttributeID, AttributeName)
values (32, 'Private');
GO

-- Re-define published data source to use display status
alter procedure BPDS_WorkforceAvailability
as

select DisplayStatus as [Status], COUNT(*) as [Total] from BPAResource
where DisplayStatus is not null
group by DisplayStatus;

return;
GO

-- Change published tile to a pie
update BPATile set
    [description]='Current status of registered resources',
    xmlproperties='<Chart type="6" plotByRow="false"><Procedure name="BPDS_WorkforceAvailability" /></Chart>'
where name='Workforce Availability' and xmlproperties like '%Procedure name="BPDS_WorkforceAvailability"%';

-- Reset last updated stamps since this is now stored as UTC
update BPAResource set lastupdated = null

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '220',
  GETUTCDATE(),
  'db_upgradeR220.sql UTC',
  'Provides additional resource info and updates Worforce Availability tile'
);

