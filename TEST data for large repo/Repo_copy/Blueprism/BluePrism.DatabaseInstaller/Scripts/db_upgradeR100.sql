/*
SCRIPT         : 100
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Add a default schedule and task to the initial database;
                 Renamed "Yesterday's Reports" to "Recent Activity"
                 Removed over-restrictive unique constraint from task table 
*/

-- Change the name of the "Yesterday's Reports" report to "Recent Activity"
update BPAScheduleList set name='Recent Activity' where listtype = 1 and name = 'Yesterday''s Reports';
GO

-- Set up a blank schedule with one task assigned to it - only do this if neither
-- a schedule named 'Schedule 1' nor a task named 'Task 1' exists... I could have
-- done some nasty wrangling to work around this, but it's just too messy and
-- only occurs if the user has been creating schedules / tasks already.
if not exists (select id from BPASchedule where name='Schedule 1' union all select id from BPATask where name='Task 1')
begin

    declare @scheduleid int;

    insert into BPASchedule (name, description, initialtaskid, retired, versionno)
        values ('New Schedule', '', NULL, 0, 1);
    set @scheduleid = scope_identity();

    insert into BPATask (scheduleid, name, description)
        values (@scheduleid, 'New Schedule - New Task', '');
    update BPASchedule set initialtaskid=scope_identity() where id = @scheduleid;

end
GO

-- Drop the unique key on task name - it's supposed to be unique *within a schedule*,
-- not in and of itself... that key already exists, so this is too restrictive.
alter table BPATask drop constraint UNQ_BPATask_name;
GO

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '100',
  GETUTCDATE(),
  'db_upgradeR100.sql UTC',
  'Added an example schedule and task; Renamed the example timetable; Removed standalone UNIQUE constraint on BPATask.name'
);
