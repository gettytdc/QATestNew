/*
BUG/STORY      : BG-1635
PURPOSE        : Add execute process/business object to process administrators
*/

insert into BPAUserRolePerm (userroleid, permid)
  select r.id, p.id
    from BPAUserRole r cross join BPAPerm p
    where r.name='Process Administrators'
    and p.name in ('Execute Process','Execute Business Object')
    and not exists (select * from BPAUserRolePerm where r.id = userroleid and p.id = permid);

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '248',
  GETUTCDATE(),
  'db_upgradeR248.sql',
  'Add execute process/business object to process administrators',
  0 -- UTC
);