/*
SCRIPT         : 224
PURPOSE        : Renames the 'Dashboard' Role Group to be 'Analytics'
*/

update BPAPermGroup set name='Analytics' where name='Dashboard';
GO

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '224',
  GETUTCDATE(),
  'db_upgradeR224.sql',
  'Renames the Dashboard Role Group to be Analytics',
  0 -- UTC
);
