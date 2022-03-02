/*
SCRIPT         : 221
PURPOSE        : Adds warning when "Pause After Step" is not set in nav stage
*/

insert into BPAValCheck (checkid, catid, typeid, description, enabled)
select 141, c.catid, t.typeid, '"Pause After Step" not specified in navigate stage', 1
  from BPAValType t cross join BPAValCategory c
  where t.description = 'Warning'
    and c.description = 'Stage Validation';

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '221',
  GETUTCDATE(),
  'db_upgradeR221.sql UTC',
  'Adds warning when "Pause After Step" is not set in nav stage'
);
