/*
SCRIPT         : 223
PURPOSE        : Adds start/end timezoneoffset columns to BPASession/BPASessionLog tables.
*/
alter table BPASession add
 starttimezoneoffset int null,
 endtimezoneoffset int null,
 lastupdatedtimezoneoffset int null;

exec sp_refreshview 'BPVSession';
GO

alter view BPVSessionInfo as
select
    s.sessionid           as "sessionid",
    s.sessionnumber       as "sessionnumber",
    s.startdatetime       as "startdatetime",
    s.starttimezoneoffset as "starttimezoneoffset",
    s.enddatetime         as "enddatetime",
    s.endtimezoneoffset   as "endtimezoneoffset",
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
    s.lastupdatedtimezoneoffset as "lastupdatedtimezoneoffset",
    s.laststage           as "laststage",
    s.warningthreshold    as "warningthreshold"
from BPASession s
    join BPAProcess p on s.processid = p.processid
    join BPAResource sr on s.starterresourceid = sr.resourceid
    join BPAResource rr on s.runningresourceid = rr.resourceid
    join BPAUser su on s.starteruserid = su.userid
where s.statusid <> 6;
GO

alter table BPASessionLog_NonUnicode add
 starttimezoneoffset int null,
 endtimezoneoffset int null;

alter table BPASessionLog_Unicode add
 starttimezoneoffset int null,
 endtimezoneoffset int null;
GO

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '223',
  GETUTCDATE(),
  'db_upgradeR223.sql',
  'Adds start/end timezoneoffset columns to BPASession/BPASessionLog tables.',
  0 -- UTC
);
