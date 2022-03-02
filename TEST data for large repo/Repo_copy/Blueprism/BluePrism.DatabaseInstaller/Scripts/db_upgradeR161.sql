/*
SCRIPT         : 161
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : GMB
PURPOSE        : Adds permittedroles bitmask to BPAResource
*/

alter table BPAResource add
  permittedroles bigint not null default -1;

--set DB version
insert into BPADBVersion values (
  '161',
  GETUTCDATE(),
  'db_upgradeR161.sql UTC',
  'Adds permittedroles bitmask to BPAResource'
);
