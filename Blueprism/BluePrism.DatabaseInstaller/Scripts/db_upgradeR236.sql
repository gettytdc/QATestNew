/*
SCRIPT         : 236
PURPOSE        : Renames the 'View Process' Permission to 'View Process Definition' and 
                    'View Business Object' to 'View Business Object Definition'
*/


update BPAPerm set [name] = 'View Process Definition' where [name] = 'View Process';
update BPAPerm set [name] = 'View Business Object Definition' where [name] = 'View Business Object'

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '236',
  GETUTCDATE(),
  'db_upgradeR236.sql',
  'Renames view process permissions',
  0 -- UTC
);
