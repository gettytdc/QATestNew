/*
SCRIPT         : 160
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Adds failfastonerror field to BPATask
*/

alter table BPATask add
  failfastonerror bit not null
    constraint DEF_BPATask_failfastonerror default 1

--set DB version
INSERT INTO BPADBVersion VALUES (
  '160',
  GETUTCDATE(),
  'db_upgradeR160.sql UTC',
  'Adds failfastonerror field to BPATask'
);

