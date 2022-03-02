/*
SCRIPT         : 109
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Removed UNIQUE constraint from BPASchedule.name and BPATask.name
*/

alter table BPASchedule drop constraint UNQ_BPASchedule_name;

alter table BPATask drop constraint UNQ_BPATask_scheduleid_name;

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '109',
  GETUTCDATE(),
  'db_upgradeR109.sql UTC',
  'Removed UNIQUE constraint from BPASchedule.name and BPATask.name' 
);

