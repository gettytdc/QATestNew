/*
SCRIPT         : 118
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : GB
PURPOSE        : Update Role from Designer to Developer and remove Report Console Permission
*/

UPDATE BPARole SET RoleName = 'Developer' WHERE RoleName = 'Designer'

DELETE FROM BPAPermission WHERE PermissionID = 8192 --Add Report
DELETE FROM BPAPermission WHERE PermissionID = 16384 --Delete Report
DELETE FROM BPAPermission WHERE PermissionID = 32768 --View Report
DELETE FROM BPAPermission WHERE PermissionID = 57344 --Report Console
DELETE FROM BPAPermission WHERE Name = 'Report Management'


-- set DB version
INSERT INTO BPADBVersion VALUES (
  '118',
  GETUTCDATE(),
  'db_upgradeR118.sql UTC',
  'Update Role from Designer to Developer and remove Report Console Permission'
);

