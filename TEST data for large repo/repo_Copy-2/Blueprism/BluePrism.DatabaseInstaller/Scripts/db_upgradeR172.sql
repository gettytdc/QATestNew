/*
SCRIPT         : 172
PROJECT NAME   : Automate
DATABASE NAME  : BPA
CREATION DATE  : 28 Aug 2015
AUTHOR         : CEG
PURPOSE        : Ensure correct index on BPAStatistics
*/

CREATE TABLE [BPAStatistics_new] (
    sessionid uniqueidentifier NOT NULL,
    name varchar (50) NOT NULL,
    datatype varchar (32) NULL,
    value_text varchar (255) NULL,
    value_number float NULL,
    value_date datetime NULL,
    value_flag bit NULL,
    CONSTRAINT PK_BPAStatistics_new primary key (sessionid, name)
);
insert into BPAStatistics_new (sessionid, name, datatype, value_text, value_number, value_date, value_flag) 
    select sessionid, name, datatype, value_text, value_number, value_date, value_flag from BPAStatistics;
drop table BPAStatistics;
exec sp_rename 'BPAStatistics_new', 'BPAStatistics';
exec sp_rename 'PK_BPAStatistics_new', 'PK_BPAStatistics';

--set DB version
INSERT INTO BPADBVersion VALUES (
  '172',
  GETUTCDATE(),
  'db_upgradeR172.sql UTC',
  'Ensure correct index on BPAStatistics'
)


