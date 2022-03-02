/*
SCRIPT         : 46
PROJECT NAME   : Automate
DATABASE NAME  : BPA
NAME           : Remove test lab permissions from database
CREATION DATE  : 04 Jul 2006
AUTHOR         : PJW
PURPOSE        : Adjusts user roles/permissions after removing test lab from Automate
NOTES          : 
*/


--Remove all test lab permissions and roles
DELETE FROM BPAPermission WHERE [Name] in  ('Create Test Plan',
                                            'Edit Test Plan',
                                            'Clone Test Plan',
                                            'Delete Test Plan',
                                            'Implement Test Plan',
                                            'Perform ad hoc Tests',
                                            'View Test Plan Results',
                                            'Test Lab')
DELETE FROM BPARole WHERE RoleName = 'Tester'


--Create new test process permission under process studio                                           
INSERT INTO BPAPermission (PermissionID, [Name]) VALUES (64, 'Test Process')
UPDATE BPAPermission SET PermissionID = (PermissionID + 64) WHERE [Name] = 'Process Studio'
UPDATE BPARole SET RolePermissions = (SELECT PermissionID FROM BPAPermission WHERE [Name] = 'Process Studio') WHERE RoleName = 'Designer'


--set DB version
INSERT INTO BPADBVersion VALUES (
  '46',
  GETUTCDATE(),
  'db_upgradeR46.sql UTC',
  'Adjusts roles/permissions settings to account removal of test lab'
)
GO

