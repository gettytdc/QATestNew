/*
SCRIPT         : 401
PURPOSE        : Add Exception details to BPVSessionInfo
*/

alter view [BPVSessionInfo]
as
    select s.sessionid,
           s.sessionnumber,
           s.startdatetime,
           s.starttimezoneoffset,
           s.enddatetime,
           s.endtimezoneoffset,
           s.processid,
           p.name as processname,
           s.starterresourceid,
           sr.name as starterresourcename,
           s.starteruserid,
           isnull(su.username, '[' + su.systemusername + ']') as starterusername,
           s.runningresourceid, rr.name as runningresourcename,
           s.runningosusername,
           s.statusid,
           s.startparamsxml,
           s.logginglevelsxml,
           s.sessionstatexml,
           s.queueid,
           s.lastupdated,
           s.lastupdatedtimezoneoffset,
           s.laststage,
           s.warningthreshold,
           s.exceptionmessage,
           s.exceptiontype,
           s.terminationreason
    from BPASession as s inner join
                 BPAProcess as p on s.processid = p.processid inner join
                 BPAResource as sr on s.starterresourceid = sr.resourceid inner join
                 BPAResource as rr on s.runningresourceid = rr.resourceid inner join
                 BPAUser as su on s.starteruserid = su.userid
    where s.statusid <> 6
go

insert into BPADBVersion (dbversion,
 scriptrundate,
 scriptname,
 [description],
 timezoneoffset
)
values
('401',
 GETUTCDATE(),
 'db_upgradeR401.sql',
 'Add Exception details to BPVSessionInfo',
 0
);
