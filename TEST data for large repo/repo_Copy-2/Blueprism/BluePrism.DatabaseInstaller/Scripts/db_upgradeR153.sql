/*
SCRIPT         : 152-Pre/153
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW/GMB
PURPOSE        : Adds table supporting standalone calendars in packages / 
        Adds table supporting password constraints on previous passwords
*/

-- Support calendars to be added to a package independently of schedules
-- See bug 5947

-- Note that this was initially R152 - it was bundled up into this
-- script because 152 was needed for a 4.1 release too.
-- This work was done under bug 6391

create table BPAPackageCalendar (
    packageid int not null
        constraint FK_BPAPackageCalendar_BPAPackage
            foreign key references BPAPackage(id)
            on delete cascade,
    calendarid int not null
        constraint FK_BPAPackageCalendar_BPACalendar
            foreign key references BPACalendar(id)
            on delete cascade,
    constraint PK_BPAPackageCalendar
        primary key (packageid, calendarid)
)

-- The original R153 script begins here
create table BPAOldPassword(
    id int identity not null primary key,
    userid uniqueidentifier not null
    constraint fk_bpaoldpassword_bpauser
      foreign key references bpauser(userid)
      on delete cascade,
    password varchar(128) null,
    lastuseddate datetime not null
)

alter table BPAPasswordRules add
    norepeats bit not null default 0,
    norepeatsdays bit not null default 0,
    numberofrepeatsordays int not null default 0

INSERT INTO BPADBVersion VALUES (
  '153',
  GETUTCDATE(),
  'db_upgradeR153.sql UTC',
  'Adds tables supporting standalone calendars in packages and constraints on previous passwords'
)
