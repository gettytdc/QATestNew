/*
SCRIPT         : 115
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : GB
PURPOSE        : Ammended Default Roles to match delivery methodology. 
*/

DELETE FROM BPARole WHERE RoleID = 2 --This is the Process Designer Role
DELETE FROM BPARole WHERE RoleName = 'Business Object Designer' --We have to use rolename because the roleid for this role is not hard coded see script db_upgradeR48.sql
DELETE FROM BPARole WHERE RoleID = 16 --This is the Observer Role

DECLARE @NextRole as int

set @NextRole = 2 * (SELECT MAX(RoleID) FROM BPARole)
INSERT INTO BPARole (RoleID, RoleName, RolePermissions) VALUES (@NextRole, 'Designer', 267911295)

set @NextRole = 2 * (SELECT MAX(RoleID) FROM BPARole)
INSERT INTO BPARole (RoleID, RoleName, RolePermissions) VALUES (@NextRole, 'Tester', 8594587712)

UPDATE BPARole SET RoleName = 'Process Administrator' WHERE RoleID = 8 --Controller Role
GO

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '115',
  GETUTCDATE(),
  'db_upgradeR115.sql UTC',
  'Ammended Default Roles to match delivery methodology'
);

