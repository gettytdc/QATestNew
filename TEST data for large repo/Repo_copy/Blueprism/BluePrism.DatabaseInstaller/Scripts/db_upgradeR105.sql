/*
SCRIPT         : 105
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Add schedule and task IDs to the alert event;
                 Add delete-cascading FKs to process and resource from alertevent.
                 Make alert event and notification types on the user not null, and default 0.
*/

-- Delete any alert events for which the process or resource have been deleted.
-- This is safe since they do not display in the alert history and have limited
-- meaning once their conjoined records have been deleted.
delete BPAAlertEvent
from BPAAlertEvent e
    left join BPAProcess p on e.ProcessID = p.processid
    left join BPAResource r on e.ResourceID = r.resourceid
where p.processid is null or r.resourceid is null;

-- Add schedule and task ID to the alert event.
-- Also add constraints and cascade notes so that alert events are cleaned up
-- if the schedules, processes or resources that they refer to are deleted,
-- rather than just left in an unviewable limbo.
-- Note that if a task is deleted, this record hangs around - we can't add an
-- 'on delete cascade' to task since there's one already on schedule, and it
-- would thus create multiple cascade paths (though why this should be a problem
-- is a question for the Sql Server devs).
alter table BPAAlertEvent add
    -- columns :
    scheduleid int null
        constraint FK_BPAAlertEvent_BPASchedule foreign key references BPASchedule(id)
        on delete cascade,
    taskid int null
        constraint FK_BPAAlertEvent_BPATask foreign key references BPATask(id),
    -- constraints :
    constraint FK_BPAAlertEvent_BPAProcess foreign key (processid) references BPAProcess(processid)
        on delete cascade,
    constraint FK_BPAAlertEvent_BPAResource foreign key (resourceid) references BPAResource(resourceid)
        on delete cascade;

-- Set the alert event types and notification types on the user to use 0 as its
-- 'no types selected' value rather than null - since *any* code in BP is reading
-- this data as "isnull(u.xxx, 0)" anyway, it seems pointless doing it any other way.
-- Same for the alert event itself...
update BPAUser set alerteventtypes = 0 where alerteventtypes is null;
update BPAUser set alertnotificationtypes = 0 where alertnotificationtypes is null;
update BPAAlertEvent set AlertEventType = 0 where AlertEventType is null;
update BPAAlertEvent set AlertNotificationType = 0 where AlertNotificationType is null;

alter table BPAUser alter column alerteventtypes int not null;
alter table BPAUser alter column alertnotificationtypes int not null;
alter table BPAUser add
    constraint DEF_BPAUser_alerteventtypes default 0 for alerteventtypes,
    constraint DEF_BPAUser_alertnotificationtypes default 0 for alertnotificationtypes;

alter table BPAAlertEvent alter column AlertEventType int not null;
alter table BPAAlertEvent alter column AlertNotificationType int not null;

-- Rename BPAAlert to BPAProcessAlert and introduce a similarly functioning BPAScheduleAlert
exec sp_rename 'BPAAlert', 'BPAProcessAlert'

create table BPAScheduleAlert (
    userid uniqueidentifier not null
        constraint FK_BPAScheduleAlert_BPAUser
            foreign key references BPAUser(userid)
            on delete cascade,
    scheduleid int not null
        constraint FK_BPAScheduleAlert_BPASchedule
            foreign key references BPASchedule(id)
            on delete cascade,
    constraint PK_BPAScheduleAlert
        primary key (userid, scheduleid)
);

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '105',
  GETUTCDATE(),
  'db_upgradeR105.sql UTC',
  'Add schedule and task IDs to the alert event; ' + 
  'Ensure alert events are deleted if the process/resource they refer to is deleted; ' +
  'Make the alert types on user more useful defaults'
);

