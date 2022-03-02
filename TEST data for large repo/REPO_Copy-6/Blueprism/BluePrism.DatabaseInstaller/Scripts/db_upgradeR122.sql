/*
SCRIPT         : 122
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Fixes BPAWorkQueueItem.state to correctly represent deferred / pending states.
*/


alter table BPAWorkQueueItem drop column state;
go

alter table BPAWorkQueueItem
    add state as (
        case 
            when exception is not null then 5
            when completed is not null then 4
            when (deferred is not null and deferred > getutcdate()) then 3
            when locked is not null then 2
            else 1 
        end);
go

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '122',
  GETUTCDATE(),
  'db_upgradeR122.sql UTC',
  'Fixes BPAWorkQueueItem.state to correctly represent deferred / pending states'
);
