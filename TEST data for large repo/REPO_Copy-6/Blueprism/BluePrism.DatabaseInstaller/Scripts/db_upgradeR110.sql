/*
SCRIPT         : 110
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Add support for minutes (and seconds) to BPASchedule trigger intervals
*/

alter table BPAScheduleTrigger
    drop constraint CHK_BPAScheduleTrigger;

alter table BPAScheduleTrigger
    add constraint CHK_BPAScheduleTrigger check (unittype < 8);

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '110',
  GETUTCDATE(),
  'db_upgradeR110.sql UTC',
  'Increases the allowed range of the schedule interval type to allow for minutes and seconds'
);

