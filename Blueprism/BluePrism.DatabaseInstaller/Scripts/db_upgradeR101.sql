/*
SCRIPT         : 101
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Final (hopefully) scheduler changes : 
                 Moved weekly trigger to daily when it's a weekly one with a calendar.
                 Increase max size of various name columns to 128 chars from 64
*/

-- set 'weekly in calendar' to 'daily on calendar' - reset period to 1 (ie. every day), since a
-- period of 2 weeks does not translate to a daily trigger
update BPAScheduleTrigger set period = 1, unittype = 2 where unittype = 3 and calendarid is not null;

-- Change the 'name' column in each of the schedule, task, calendar and list tables
-- to be max 128 chars rather than 64
alter table BPASchedule
    alter column name varchar(128) not null;

alter table BPATask
    alter column name varchar(128) not null;
    
alter table BPACalendar
    alter column name varchar(128) not null;

alter table BPAScheduleList
    alter column name varchar(128) not null;

-- add an 'All Schedules' flag to the list - having the absence of specified schedules
-- semantically meaning 'All Schedules' is fine until someone has a report for a single
-- schedule, then deletes that schedule and suddenly the report is for all schedules.
alter table BPAScheduleList
    add allschedules bit not null
        constraint DEF_BPAScheduleList_allschedules default 0;
GO

-- Ensure that the allschedules flag is set if there are no schedules in a list
-- (which was the previous way of defining 'all schedules')
update l set l.allschedules = 1 
    from BPAScheduleList l
        left join BPAScheduleListSchedule s on l.id = s.schedulelistid 
    where s.schedulelistid is null;
    
-- set DB version
INSERT INTO BPADBVersion VALUES (
  '101',
  GETUTCDATE(),
  'db_upgradeR101.sql UTC',
  'Moved predefined weekly triggers with calendar to daily; Increased max name lengths'
);
