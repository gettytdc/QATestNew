/*
SCRIPT         : 136
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : CG
PURPOSE        : Add table for font storage
*/

create table BPAFont (
    name varchar(255) not null
        constraint PK_BPAFont primary key,
    version varchar(255) not null,
    fontdata text not null,
)


--set DB version
INSERT INTO BPADBVersion VALUES (
  '136',
  GETUTCDATE(),
  'db_upgradeR136.sql UTC',
  'Add table for font storage'
)

