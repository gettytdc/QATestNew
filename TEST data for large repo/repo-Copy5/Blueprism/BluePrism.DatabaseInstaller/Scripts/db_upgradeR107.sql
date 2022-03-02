/*
SCRIPT         : 107
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : GB 
PURPOSE        : Adds permission to retire schedules

*/

INSERT INTO BPAPermission (PermissionID,Name) VALUES (274877906944,'Retire Schedule')

UPDATE BPAPermission SET PermissionID=532575944704 WHERE PermissionID=257698037760

UPDATE BPARole Set RolePermissions=532575944704 WHERE RoleID=128
    
-- set DB version
INSERT INTO BPADBVersion VALUES (
  '107',
  GETUTCDATE(),
  'db_upgradeR107.sql UTC',
  'Adds permission to retire schedules' 
);

