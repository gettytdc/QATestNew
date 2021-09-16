/*
SCRIPT         : 45
PROJECT NAME   : Automate
DATABASE NAME  : BPA
NAME           : Introduce Process Comparision
CREATION DATE  : 27 Jun 2006
AUTHOR         : PJW
PURPOSE        : Adjusts user roles/permissions after adding Process Comparision to process studio menu
NOTES          : 
*/


--Insert new permission and adjust process studio header 
INSERT INTO BPAPermission (PermissionID, [Name]) VALUES (8, 'Compare Processes')
UPDATE BPAPermission SET PermissionID = 524351 WHERE [Name] = 'Process Studio'
--We do not update the role of Designer to include this new action because
--that may not be desired by Automate Administrators. It is up to them to add
--users manually using the Role management feature in System Manager


--The following addresses a mistake made in previous scripts, in which the 
--admin role should be made all powerful. Documented in bug number 1460
UPDATE BPAUser SET Roles = '1' WHERE UserName = 'admin'


--set DB version
INSERT INTO BPADBVersion VALUES (
  '45',
  GETUTCDATE(),
  'db_upgradeR45.sql UTC',
  'Adjusts roles/permissions settings to account for new process comparision feature'
)
GO

