/*
SCRIPT         : 131
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : CG
PURPOSE        : Adds session log fields for storage of diagnostics information
*/

alter table bpasessionlog add automateworkingset bigint default 0
alter table bpasessionlog add targetappname varchar(32) default NULL
alter table bpasessionlog add targetappworkingset bigint default 0

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '131',
  GETUTCDATE(),
  'db_upgradeR131.sql UTC',
  'Adds session log fields for storage of diagnostics information'
);
