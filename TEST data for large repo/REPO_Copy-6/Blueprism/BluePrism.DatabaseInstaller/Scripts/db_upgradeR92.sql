/*
SCRIPT         : 92
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Some foreign key refinements to some of the Scheduler tables
*/

-- Get which version of Sql Server we're using
DECLARE @sver nvarchar(128)
declare @ver int
SET @sver = CAST(serverproperty('ProductVersion') AS nvarchar)
SET @ver = convert(int, SUBSTRING(@sver, 1, CHARINDEX('.', @sver) - 1))
-- if @ver = 8 : SS2000; 9 : SS2005; 10 : SS2008

-- If a public holiday group is deleted, blank the entry on the calendar.
alter table BPACalendar drop constraint FK_BPACalendar_BPAPublicHolidayGroup;
-- Note that SS2k doesn't support 'on delete set null' - also, it checks
-- syntax inside an IF statement that it would never call, thus we need
-- to wrap it into a string and exec() it.
if (@ver > 8)
    exec('alter table BPACalendar
            add constraint FK_BPACalendar_BPAPublicHolidayGroup
            foreign key (publicholidaygroupid) references BPAPublicHolidayGroup(id)
            on delete set null;')

-- if a public holiday is deleted, delete the corresponding public holiday working day records
alter table BPAPublicHolidayWorkingDay drop constraint FK_BPAPublicHolidayWorkingDay_BPAPublicHoliday;
alter table BPAPublicHolidayWorkingDay 
    add constraint FK_BPAPublicHolidayWorkingDay_BPAPublicHoliday
        foreign key (publicholidayid) references BPAPublicHoliday(id)
        on delete cascade;

-- If a calendar is deleted, delete any related BPANonWorkingDay records, and any BPAPublicHolidayWorkingDay records
alter table BPANonWorkingDay drop constraint FK_BPANonWorkingDay_BPACalendar;
alter table BPANonWorkingDay 
    add constraint FK_BPANonWorkingDay_BPACalendar
        foreign key (calendarid) references BPACalendar(id)
        on delete cascade;

alter table BPAPublicHolidayWorkingDay drop constraint FK_BPAPublicHolidayWorkingDay_BPACalendar;
alter table BPAPublicHolidayWorkingDay
    add constraint FK_BPAPublicHolidayWorkingDay_BPACalendar
        foreign key (calendarid) references BPACalendar(id)
        on delete cascade;

-- If a session log is deleted, any schedule log entries which pointed to it should be set to null
alter table BPAScheduleLogEntry drop constraint FK_BPAScheduleLogEntry_BPASession;
if (@ver > 8) -- SS2000 doesn't support "on delete set null" - see above
    exec('alter table BPAScheduleLogEntry 
            add constraint FK_BPAScheduleLogEntry_BPASession
            foreign key (logsessionnumber) references BPASession(sessionnumber)
            on delete set null;')

-- The remaining FK without an ON DELETE clause is :-
-- : BPAScheduleTrigger (FK_BPAScheduleTrigger_BPACalendar)
-- which I'll leave as is - a calendar should not be allowed to be deleted if a trigger relies on it.

--set DB version
INSERT INTO BPADBVersion VALUES (
  '92',
  GETUTCDATE(),
  'db_upgradeR92.sql UTC',
  'Foreign key refinements for the scheduler tables'
);
