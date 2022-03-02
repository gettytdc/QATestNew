
/*
SCRIPT         : 157
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Updates BPAScheduleLog to allow multiple active schedulers
*/

-- Ensure that only one server can run a specific schedule instance
-- at any one time.
alter table BPAScheduleLog
  add constraint UNQ_BPAScheduleLog_scheduleid_time
    unique nonclustered (scheduleid, instancetime);

-- Add servername and heartbeat so that other servers can see if
-- a schedule is still running
alter table BPAScheduleLog add 
  servername varchar(255) null,
  heartbeat datetime null;

--set DB version
INSERT INTO BPADBVersion VALUES (
  '157',
  GETUTCDATE(),
  'db_upgradeR157.sql UTC',
  'Updates BPAScheduleLog to allow multiple active schedulers'
);
