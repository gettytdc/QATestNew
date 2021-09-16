/*
SCRIPT         : 96
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Add termination details to the schedule log
*/

alter table BPAScheduleLogEntry add 
    terminationreason varchar(255) null,
    stacktrace text null;
go

--set DB version
INSERT INTO BPADBVersion VALUES (
  '96',
  GETUTCDATE(),
  'db_upgradeR96.sql UTC',
  'Add termination reason and stack trace to schedule log entries'
);

