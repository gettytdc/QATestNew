/*
SCRIPT         : 150
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Adds missing validation checks
*/

-- See bugs 5755 and 5771

-- Adds :-
-- a warning to say that an output param does not map to a stage, -and-
-- a warning to indicate that an input param was left empty -and-
-- a comment indicating that a action has no description -and-
insert into BPAValCheck (checkid, catid, typeid, description)
  select 135, 0, 1, 'No ''Store In'' mapping set{0}'
    union all
  select 136, 0, 1, 'Blank value supplied{0}'
    union all
  select 137, 2, 2, 'Published action does not contain a description{0}'

-- And set the "missing description on action stage" type to 'advice'  
update BPAValCheck set typeid=2 where checkid=129
  
--set DB version
INSERT INTO BPADBVersion VALUES (
  '150',
  GETUTCDATE(),
  'db_upgradeR150.sql UTC',
  'Adds missing validation checks'
)
