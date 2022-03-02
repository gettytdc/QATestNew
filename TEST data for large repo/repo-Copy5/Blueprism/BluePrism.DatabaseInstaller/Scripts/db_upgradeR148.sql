/*
SCRIPT         : 148
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Adds option for a resource to log to event log
*/

alter table BPAResource
  add logtoeventlog bit not null
    constraint DEF_BPAResource_logtoeventlog default 1

--set DB version
INSERT INTO BPADBVersion VALUES (
  '148',
  GETUTCDATE(),
  'db_upgradeR148.sql UTC',
  'Adds option for a resource to log to event log'
)
