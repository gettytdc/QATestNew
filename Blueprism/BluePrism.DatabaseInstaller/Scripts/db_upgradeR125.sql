/*
SCRIPT         : 125
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Fixes permission errors introduced in 124
*/

/* Fix BPAPermission.
 * Current largest *single* entry is 'System - Calendars' which
 * has a value: 0x20000000000000. Double that. */
update BPAPermission 
    set PermissionID = cast(0x40000000000000 as bigint) 
    where name = 'System - Scheduler';

/* 2 other permission entries affected */
declare @sysman bigint, @schedman bigint;

/* The 'System Manager' permission covers all permissions which
 * deal with system manager. */
set @sysman = (
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
        'System - Settings',
        'System - Scheduler'
    )
);

/* The 'Scheduler' permission convers all permissions which deal
 * deal with the scheduler. */
set @schedman = (
    select sum(PermissionID) from BPAPermission where Name in (
        'Retire Schedule',
        'Delete Schedule',
        'Create Schedule',
        'Edit Schedule',
        'View Schedule',
        'System - Scheduler'
    )
);

update BPAPermission set PermissionID = @sysman where Name = 'System Manager';
update BPAPermission set PermissionID = @schedman where Name = 'Scheduler';

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '125',
  GETUTCDATE(),
  'db_upgradeR125.sql UTC',
  'Fixes permission errors introduced in 124'
);
