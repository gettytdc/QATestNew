/*
SCRIPT         : 128
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Adds the BPVSessionInfo view
*/

create view BPVSessionInfo as
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
    s.sessionstatexml     as "sessionstatexml"
from BPASession s
    join BPAProcess p on s.processid = p.processid
    join BPAResource sr on s.starterresourceid = sr.resourceid
    join BPAResource rr on s.runningresourceid = rr.resourceid
    join BPAUser su on s.starteruserid = su.userid
GO


-- set DB version
INSERT INTO BPADBVersion VALUES (
  '128',
  GETUTCDATE(),
  'db_upgradeR128.sql UTC',
  'Adds the BPVSessionInfo view, combining user, resource and process data into a single view'
);
