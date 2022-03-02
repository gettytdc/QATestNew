/*
SCRIPT         : 116
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : GB
PURPOSE        : Ammended permission names to better reflect system manager areas.
*/
UPDATE BPAPermission SET Name = 'Processes - Management' WHERE Name = 'Process Management'
INSERT INTO BPAPermission (PermissionID,Name) VALUES (0x8000000000,'Processes - Grouping')
INSERT INTO BPAPermission (PermissionID,Name) VALUES (0x10000000000,'Processes - History')
INSERT INTO BPAPermission (PermissionID,Name) VALUES (0x20000000000,'Processes - Exception Types')

UPDATE BPAPermission SET Name = 'Business Objects - Management' WHERE Name = 'Business Object Management'
INSERT INTO BPAPermission (PermissionID,Name) VALUES (0x40000000000,'Business Objects - History')
INSERT INTO BPAPermission (PermissionID,Name) VALUES (0x80000000000,'Business Objects - External')
INSERT INTO BPAPermission (PermissionID,Name) VALUES (0x100000000000,'Business Objects - Exception Types')
UPDATE BPAPermission SET Name = 'Business Objects - Web Services' WHERE Name = 'Web Service Management'

UPDATE BPAPermission SET Name = 'Resources - Management' WHERE Name = 'Resource Management'
INSERT INTO BPAPermission (PermissionID,Name) VALUES (0x200000000000,'Resources - Pools')

INSERT INTO BPAPermission (PermissionID,Name) VALUES (0x400000000000,'Workflow - Work Queue Configuration')

UPDATE BPAPermission SET Name = 'Audit - Audit Logs' WHERE Name = 'System Manager Logs'
INSERT INTO BPAPermission (PermissionID,Name) VALUES (0x800000000000,'Audit - Process Logs')
INSERT INTO BPAPermission (PermissionID,Name) VALUES (0x1000000000000,'Audit - Business Object Logs')
INSERT INTO BPAPermission (PermissionID,Name) VALUES (0x2000000000000,'Audit - Statistics')

UPDATE BPAPermission SET Name = 'Security - Users' WHERE Name = 'User Management'
INSERT INTO BPAPermission (PermissionID,Name) VALUES (0x4000000000000,'Security - Credentials')
INSERT INTO BPAPermission (PermissionID,Name) VALUES (0x8000000000000,'Security - Password Options')

UPDATE BPAPermission SET Name = 'System - Settings' WHERE Name = 'General Settings'
INSERT INTO BPAPermission (PermissionID,Name) VALUES (0x10000000000000,'System - License')
UPDATE BPAPermission SET Name = 'System - Archiving' WHERE Name = 'Database Management'
INSERT INTO BPAPermission (PermissionID,Name) VALUES (0x20000000000000,'System - Calendars')

declare @tot bigint;
set @tot = (
    select sum(PermissionID) from BPAPermission where Name in (
        'Processes - Management',
        'Processes - Grouping',
        'Processes - History',
        'Processes - Exception Types',
        'Business Objects - Management',
        'Business Objects - History',
        'Business Objects - External',
        'Business Objects - Exception Types',
        'Business Objects - Web Services',
        'Resources - Management',
        'Resources - Pools',
        'Workflow - Work Queue Configuration',
        'Audit - Audit Logs',
        'Audit - Process Logs',
        'Audit - Business Object Logs',
        'Audit - Statistics',
        'Security - Users',
        'Security - Credentials',
        'Security - Password Options',
        'System - License',
        'System - Calendars',
        'System - Archiving',
        'System - Settings'
    )
)

UPDATE BPAPermission SET PermissionID = @tot WHERE Name = 'System Manager'
GO

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '116',
  GETUTCDATE(),
  'db_upgradeR116.sql UTC',
  'Ammended permission names to better reflect system manager areas.'
);

