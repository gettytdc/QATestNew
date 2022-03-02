/*
SCRIPT         : 171
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Adds ability to set a BPASession record to 'archived'
*/

-- Delete 'ENV' status values which aren't used anywhere
delete from BPAStatus where type='ENV';

-- Add new status value to hold that a session is archived.
insert into BPAStatus(statusid, type, description)
    values (6, 'RUN', 'Archived');
GO

-- Update the session view to hide 'archived' session logs
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
    s.sessionstatexml     as "sessionstatexml"
from BPASession s
    join BPAProcess p on s.processid = p.processid
    join BPAResource sr on s.starterresourceid = sr.resourceid
    join BPAResource rr on s.runningresourceid = rr.resourceid
    join BPAUser su on s.starteruserid = su.userid
where s.statusid <> 6;
GO

-- Add a 'live session' view for use in most of the rest of the code
-- (so that it can ignore 'archived' views without other changes)
create view BPVSession as
select * from BPASession where statusid <> 6;
GO

--set DB version
INSERT INTO BPADBVersion VALUES (
  '171',
  GETUTCDATE(),
  'db_upgradeR171.sql UTC',
  'Adds BPASession flag to indicate if it''s been archived'
)
