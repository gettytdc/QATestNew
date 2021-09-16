/*
SCRIPT         : 98
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Added a 'data tracker' table to allow a cheap check to be made to track changes in data
                 without reloading the entire data structure 'just in case'
*/

create table BPADataTracker (
    dataname varchar(64) not null
        constraint PK_BPADataTracker primary key,
    versionno bigint not null
);
go

insert into BPADataTracker (dataname, versionno) values ('Scheduler', 1);

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '98',
  GETUTCDATE(),
  'db_upgradeR98.sql UTC',
  'Created the BPADataTracker table to hold simplistic version numbers for arbitrary data'
);
