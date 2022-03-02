/*
SCRIPT         : 120
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Update Process Administrator role to allow viewing of logs.
*/

update BPARole 
    set rolepermissions = rolepermissions | (
        select sum(permissionid) 
        from BPAPermission 
        where name in ('Audit - Process Logs', 'Audit - Business Object Logs')
    )
    where rolename = 'Process Administrator';


-- set DB version
INSERT INTO BPADBVersion VALUES (
  '120',
  GETUTCDATE(),
  'db_upgradeR120.sql UTC',
  'Update Process Administrator role to allow viewing of logs'
);
