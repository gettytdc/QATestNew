/*
SCRIPT         : 90
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : The scheduler and support data tables
*/

-- Public Holidays and their associated groups.

-- The public holiday table
create table BPAPublicHoliday (
    id int not null 
        constraint PK_BPAPublicHoliday primary key,
    name varchar(64) not null,
    dd int null,
    mm int null,
    dayofweek tinyint null
        constraint CHK_BPAPublicHoliday_dayofweek check (dayofweek < 7),
    nthofmonth int null
        constraint CHK_BPAPublicHoliday_nth check (nthofmonth > -2 and nthofmonth < 6),
    relativetoholiday int null
        constraint FK_BPAPublicHoliday_BPAPublicHoliday
            foreign key references BPAPublicHoliday(id)
            on delete no action, -- bizarrely, this is required for self-referential keys - I would prefer on delete set null
    relativedaydiff int null,
    eastersunday bit null
);
    
-- public holiday group
create table BPAPublicHolidayGroup (
    id int not null identity 
        constraint PK_BPAPublicHolidayGroup primary key,
    name varchar(64) not null
        constraint UNQ_BPAPublicHoliday_name unique
);

-- member
create table BPAPublicHolidayGroupMember (
    publicholidaygroupid int not null
        constraint FK_BPAPublicHolidayGroupMember_BPAPublicHolidayGroup
            foreign key references BPAPublicHolidayGroup(id)
            on delete cascade,
    publicholidayid int not null
        constraint FK_BPAPublicHolidayGroupMember_BPAPublicHoliday
            foreign key references BPAPublicHoliday(id)
            on delete cascade,
    constraint PK_BPAPublicHolidayGroupMember 
        primary key (publicholidaygroupid, publicholidayid)
);

-- Initial Public Holiday data 
-- Our aim with this is to cover the initially supported countries, namely:
-- England & Wales, Scotland, Northern Ireland and Republic of Ireland.
-- now build up the data
insert into BPAPublicHoliday
    (id, name, dd, mm, dayofweek, nthofmonth, relativetoholiday, relativedaydiff, eastersunday)
    select 1, 'Easter Sunday', null, null, null, null, null, null, 1
        union all
    select 2, 'Christmas Day', 25, 12, null, null, null, null, null
        union all
    select 3, 'New Years'' Day', 1, 1, null, null, null, null, null
        union all
    select 4, 'Second of January', null, null, null, null, 3, 1, null
        union all
    select 5, 'St Patrick''s Day', 17, 3, null, null, null, null, null
        union all
    select 6, 'Good Friday', null, null, null, null, 1, -2, null
        union all
    select 7, 'Easter Monday', null, null, null, null, 1, 1, null
        union all
    select 8, 'May Day', null, 5, 1, 1, null, null, null
        union all
    select 9, 'May Bank Holiday', null, 5, 1, 1, null, null, null
        union all
    select 10, 'Spring Bank Holiday', null, 5, 1, -1, null, null, null
        union all
    select 11, 'June Bank Holiday', null, 6, 1, 1, null, null, null
        union all
    select 12, 'Orangemen''s Day', 12, 7, null, null, null, null, null
        union all
    select 13, 'August Bank Holiday', null, 8, 1, 1, null, null, null
        union all
    -- Scotland use 1st Monday in August for their summer bank holiday
    select 14, 'Summer Bank Holiday', null, 8, 1, 1, null, null, null
        union all
    -- England & Wales + N. Ireland use last Monday in August for theirs
    select 15, 'Summer Bank Holiday', null, 8, 1, -1, null, null, null
        union all
    select 16, 'October Bank Holiday', null, 10, 1, -1, null, null, null
        union all
    select 17, 'St Andrew''s Day', 30, 11, null, null, null, null, null
        union all
    select 18, 'Boxing Day', null, null, null, null, 2, 1, null
;

-- Put the names into 'constants' so there's no ropey mis-spellings
declare @engName varchar(64), @scotName varchar(64), @norName varchar(64), @ireName varchar(64)
set @engName='England and Wales'
set @scotName='Scotland'
set @norName='Northern Ireland'
set @ireName='Republic of Ireland'

-- Insert the tables
insert into BPAPublicHolidayGroup (name)
    select @engName
    union select @scotName
    union select @norName
    union select @ireName;

-- Get the IDs for each country to use in the group member table
declare @england int, @scotland int, @northernIreland int, @eire int
set @england = (select id from BPAPublicHolidayGroup where name=@engName);
set @scotland = (select id from BPAPublicHolidayGroup where name=@scotName);
set @northernIreland = (select id from BPAPublicHolidayGroup where name=@norName);
set @eire = (select id from BPAPublicHolidayGroup where name=@ireName);
    
-- Finally, put them into the group member table    
insert into BPAPublicHolidayGroupMember (publicholidaygroupid, publicholidayid)
    -- England & Wales
    select @england, 3
        union all
    select @england, 6
        union all
    select @england, 7
        union all
    select @england, 8
        union all
    select @england, 10
        union all
    select @england, 15
        union all
    select @england, 2
        union all
    select @england, 18
        union all
    -- Scotland
    select @scotland, 3
        union all
    select @scotland, 4
        union all
    select @scotland, 6
        union all
    select @scotland, 8
        union all
    select @scotland, 10
        union all
    select @scotland, 14
        union all
    select @scotland, 17
        union all
    select @scotland, 2
        union all
    select @scotland, 18
        union all
    -- Northern Ireland
    select @northernIreland, 3
        union all
    select @northernIreland, 5
        union all
    select @northernIreland, 6
        union all
    select @northernIreland, 7
        union all
    select @northernIreland, 8
        union all
    select @northernIreland, 10
        union all
    select @northernIreland, 12
        union all
    select @northernIreland, 15
        union all
    select @northernIreland, 2
        union all
    select @northernIreland, 18
        union all
    -- Republic of Ireland
    select @eire, 3
        union all
    select @eire, 5
        union all
    select @eire, 7
        union all
    select @eire, 9
        union all
    select @eire, 11
        union all
    select @eire, 13
        union all
    select @eire, 16
        union all
    select @eire, 2
        union all
    select @eire, 18
;

-- The calendar - the named calendar which brings together the working week,
-- public holidays and specific holidays.
create table BPACalendar (
    id int not null identity
        constraint PK_BPACalendar primary key,
    name varchar(64) not null
        constraint UNQ_BPACalendar_name unique,
    description text not null,
    publicholidaygroupid int null
        constraint FK_BPACalendar_BPAPublicHolidayGroup
            foreign key references BPAPublicHolidayGroup(id),
    workingweek tinyint not null
        constraint CHK_BPACalendar_workingweek check (workingweek < 128)    
);

-- The specific non-working days which form part of a calendar.
create table BPANonWorkingDay (
    calendarid int not null
        constraint FK_BPANonWorkingDay_BPACalendar
            foreign key references BPACalendar(id),
    nonworkingday datetime not null,
    constraint PK_BPANonWorkingDay 
        primary key (calendarid, nonworkingday)
);

-- public holiday overrides for a calendar
-- If a calendar has a public holiday group assigned, entries in this table
-- will make a public holiday in that calendar into a working day for that
-- calendar - ie. overriding it.
create table BPAPublicHolidayWorkingDay (
    calendarid int not null
        constraint FK_BPAPublicHolidayWorkingDay_BPACalendar
            foreign key references BPACalendar(id),
    publicholidayid int not null
        constraint FK_BPAPublicHolidayWorkingDay_BPAPublicHoliday
            foreign key references BPAPublicHoliday(id),
    constraint PK_BPAPublicHolidayWorkingDay
        primary key (calendarid, publicholidayid)
);

-- The default calendar, installed along with BP.
insert into BPACalendar (name,description,workingweek)
    values ('Working Week / No Holidays', 'Five day working week with no public holidays or other holidays', ( 2 | 4 | 8 | 16 | 32 ));

-- The schedule itself - note that initialtaskid is, to all intents
-- and purposes, a FK to the BPATask table, but since BPATask has
-- a foreign key to the BPASchedule table we can't actually mark it
-- as such.
create table BPASchedule(
    id int not null identity
        constraint PK_BPASchedule primary key,
    name varchar(64) not null
        constraint UNQ_BPASchedule_name unique,
    description text not null,
    initialtaskid int null,
    retired bit not null
        constraint DEF_BPASchedule_retired
            default 0
);

-- The task - the individual executable component of the schedule.
-- Note that the onsuccess and onfailure fields are set to perform
-- no action if the task they point to is deleted... This would
-- preferable 'set null', but SQL Server doesn't allow that for
-- circular referential relations... not quite sure why.
create table BPATask(
    id int not null identity
        constraint PK_BPATask primary key,
    scheduleid int not null
        constraint FK_BPATask_BPASchedule
            foreign key references BPASchedule(id)
            on delete cascade,
    name varchar(64) not null
        constraint UNQ_BPATask_name unique,
    description text not null,
    onsuccess int null
        constraint FK_BPATask_BPATask_success
            foreign key references BPATask(id)
            on delete no action,
    onfailure int null
        constraint FK_BPATask_BPATask_failure
            foreign key references BPATask(id)
            on delete no action,
    constraint UNQ_BPATask_scheduleid_name UNIQUE (scheduleid, name)
);

-- The 'session' to run as part of a task.
-- This structure implies that you can't run the same process on the
-- same resource more than once in the same task.
create table BPATaskProcess(
    taskid int not null
        constraint FK_BPATaskProcess_BPATask
            foreign key references BPATask(id)
            on delete cascade,
    processid uniqueidentifier not null
        constraint FK_BPATaskProcess_BPAProcess
            foreign key references BPAProcess(processid)
            on delete cascade,
    resourceid uniqueidentifier not null
        constraint FK_BPATaskProcess_BPAResource
            foreign key references BPAResource(resourceid)
            on delete cascade,
    failonerror bit not null
        constraint DEF_BPATaskProcess_failonerror
        default 1,
    processparams text null,
    constraint PK_BPATaskProcess
        primary key (taskid, processid, resourceid)
);

-- The trigger which activates at its defined periods, either firing
-- the schedule, or suppressing another trigger assigned to the
-- schedule. Note that a schedule can have many triggers, but a
-- trigger is assigned to only one schedule.
-- The Scheduler tech design informs further about this table.
create table BPAScheduleTrigger(
    id int not null identity
        constraint PK_BPAScheduleTrigger primary key,
    scheduleid int not null
        constraint FK_BPAScheduleTrigger_BPASchedule
            foreign key references BPASchedule(id)
            on delete cascade,
    priority int not null,
    mode tinyint not null,
    unittype tinyint not null
        constraint CHK_BPAScheduleTrigger check (unittype < 6),
    period int not null,
    startdate datetime not null,
    enddate datetime null,  
    startpoint int null,
    endpoint int null,
    dayset int null,
    calendarid int null
        constraint FK_BPAScheduleTrigger_BPACalendar
            foreign key references BPACalendar(id),
    nthofmonth int null
        constraint CHK_BPAScheduleTrigger_nthofmonth check (nthofmonth > -2 and nthofmonth < 6),
    missingdatepolicy tinyint null
        constraint CHK_BPAScheduleTrigger_missingdatepolicy check (missingdatepolicy < 3)
);

-- The log for a schedule. This provides the basis for grouping together
-- the logging for a particular schedule.
create table BPAScheduleLog (
    id int not null identity
        constraint PK_BPAScheduleLog primary key,
    scheduleid int not null
        constraint FK_BPAScheduleLog_BPASchedule
            foreign key references BPASchedule (id)
            on delete cascade,
    instancetime datetime not null,
    firereason tinyint not null
        constraint CHK_BPAScheduleLog_firereason check (firereason < 5)
);

-- A log entry for a schedule. This is 'owned' by the schedule log record
-- that it references.
create table BPAScheduleLogEntry (
    id bigint not null identity
        constraint PK_BPAScheduleLogEntry primary key,
    schedulelogid int not null
        constraint FK_BPAScheduleLogEntry_BPAScheduleLog
            foreign key references BPAScheduleLog(id)
            on delete cascade,
    entrytype tinyint not null
        constraint CHK_BPAScheduleLogEntry_entrytype check (entrytype < 10),
    entrytime datetime not null,
    taskid int null
        constraint FK_BPAScheduleLogEntry_BPATask
            foreign key references BPATask(id)
            on delete no action,
    logsessionnumber int null
        constraint FK_BPAScheduleLogEntry_BPASession
            foreign key references BPASession(sessionnumber)
);

-- Reports and Timetables
-- The schedule list provides the core data for defining a list of
-- schedule entries - be they backward looking (reports) or forward
-- looking (timetables).
create table BPAScheduleList (
    id int not null identity
        constraint PK_BPAScheduleList primary key,
    listtype tinyint not null
        constraint CHK_BPAScheduleList_listtype check (listtype > 0 and listtype < 3),
    name varchar(64) not null,
    description text not null,
    relativedate tinyint not null
        constraint CHK_BPAScheduleList_relativedate check (relativedate < 4),
    absolutedate datetime null,
    daysdistance int not null,
    constraint UNQ_BPAScheduleList_listtype_name unique (listtype, name)
);

-- A schedule which should be included in the referenced list.
-- Note that the complete absence of a schedule for a list implies
-- that the list should contain *all* schedules.
create table BPAScheduleListSchedule (
    schedulelistid int not null
        constraint FK_BPAScheduleListSchedule_BPAScheduleList
            foreign key references BPAScheduleList(id)
            on delete cascade,
    scheduleid int not null
        constraint FK_BPAScheduleListSchedule_BPASchedule
            foreign key references BPASchedule(id)
            on delete cascade,
    constraint PK_BPAScheduleListSchedule
        primary key (schedulelistid, scheduleid)
);

-- Some default schedule lists 
insert into BPAScheduleList (listtype, name, description, relativedate, absolutedate, daysdistance)
    select 1, 'Yesterday''s Reports', 'Reports for all schedules which ran yesterday and today', 2, NULL, 0
        union all
    select 2, 'Today & Tomorrow', 'Timetable for all schedules running today and tomorrow', 1, NULL, 1;

--set DB version
INSERT INTO BPADBVersion VALUES (
  '90',
  GETUTCDATE(),
  'db_upgradeR90.sql UTC',
  'Creation of all the public holiday, calendar, scheduler & related tables and any initial data'
)
