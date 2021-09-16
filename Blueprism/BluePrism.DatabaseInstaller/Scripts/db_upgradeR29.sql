/* THIS IS A DATABASE UPGRADE SCRIPT USED TO MAKE THE NECESSARY CHANGES FOR THE RESTRUCTURED USER ROLES FEATURES OF AUTOMATE.
    IT UPGRADES AN R28 database to R29
    BY PJW.
*/



/* Get rid of old tables no longer needed */
if exists (select * from dbo.sysobjects where id = object_id(N'[BPAUserRole]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAUserRole]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[BPAUserRoleNar]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAUserRoleNar]
GO

/*prepare new tables for a remake */
if exists (select * from dbo.sysobjects where id = object_id(N'[BPAUserRole]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPARole]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[BPAPermission]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [BPAPermission]
GO




/* Create the new tables if needed*/
if not exists (select * from dbo.sysobjects where id = object_id(N'[BPARole]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
    CREATE TABLE [BPARole] (
        [RoleID] [bigint] NOT NULL ,
        [RoleName] [varchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL ,
        [RolePermissions] [bigint] NOT NULL ,
        CONSTRAINT [PK_BPARole] PRIMARY KEY  CLUSTERED 
        (
            [RoleID]
        )
    )
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[BPAPermission]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
    CREATE TABLE [BPAPermission] (
        [PermissionID] [bigint] NOT NULL ,
        [Name] [varchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL 
        CONSTRAINT [PK_BPAPermission] PRIMARY KEY  CLUSTERED 
        (
            [PermissionID]
        )
    )
GO





/* update the user table to have a role and a permission field */
IF NOT EXISTS
    (SELECT * FROM dbo.sysobjects O INNER JOIN SysColumns C ON O.ID=C.ID WHERE ObjectProperty(O.ID,'IsUserTable')=1
        AND O.Name='BPAUser'
            AND C.Name='Roles')
ALTER TABLE BPAUser ADD Roles bigint NOT NULL DEFAULT 0
GO

IF NOT EXISTS
    (SELECT * FROM dbo.sysobjects O INNER JOIN SysColumns C ON O.ID=C.ID WHERE ObjectProperty(O.ID,'IsUserTable')=1
        AND O.Name='BPAUser'
            AND C.Name='Permissions')
ALTER TABLE BPAUser ADD [Permissions] bigint NOT NULL DEFAULT 0
GO




/* populate BPAPermission - NB this table is only ever modified via a script - never at run time*/
if not ((select COUNT(*) from BPAPermission) > '0') begin
    INSERT INTO BPAPermission (PermissionID, [Name]) VALUES (1, 'Create/Clone Process')
    INSERT INTO BPAPermission (PermissionID, [Name]) VALUES (2, 'Import Process')
    INSERT INTO BPAPermission (PermissionID, [Name]) VALUES (4, 'Edit Process')
    INSERT INTO BPAPermission (PermissionID, [Name]) VALUES (16, 'Delete Process')
    INSERT INTO BPAPermission (PermissionID, [Name]) VALUES (32, 'View Process')
    INSERT INTO BPAPermission (PermissionID, [Name]) VALUES (524288, 'Export Process')
    INSERT INTO BPAPermission (PermissionID, [Name]) VALUES (524343, 'Process Studio')
    
    INSERT INTO BPAPermission (PermissionID, [Name]) VALUES (64, 'Create Test Plan')
    INSERT INTO BPAPermission (PermissionID, [Name]) VALUES (128, 'Edit Test Plan')
    INSERT INTO BPAPermission (PermissionID, [Name]) VALUES (256, 'Clone Test Plan')
    INSERT INTO BPAPermission (PermissionID, [Name]) VALUES (512, 'Delete Test Plan')
    INSERT INTO BPAPermission (PermissionID, [Name]) VALUES (1024, 'Implement Test Plan')
    INSERT INTO BPAPermission (PermissionID, [Name]) VALUES (2048, 'View Test Plan Results')
    INSERT INTO BPAPermission (PermissionID, [Name]) VALUES (4096, 'Perform ad hoc Tests')
    INSERT INTO BPAPermission (PermissionID, [Name]) VALUES (8128, 'Test Lab')
    
    INSERT INTO BPAPermission (PermissionID, [Name]) VALUES (8192, 'Add Report')
    INSERT INTO BPAPermission (PermissionID, [Name]) VALUES (16384, 'Delete Report')
    INSERT INTO BPAPermission (PermissionID, [Name]) VALUES (32768, 'View Report')
    INSERT INTO BPAPermission (PermissionID, [Name]) VALUES (57344, 'Report Console')
    
    INSERT INTO BPAPermission (PermissionID, [Name]) VALUES (65536, 'Read-Only Access to Control Room')
    INSERT INTO BPAPermission (PermissionID, [Name]) VALUES (131072, 'Full Access to Control Room')
    INSERT INTO BPAPermission (PermissionID, [Name]) VALUES (196608, 'Control Room')
    
    INSERT INTO BPAPermission (PermissionID, [Name]) VALUES (262144, 'System Manager')
end
GO




/* insert some default roles. The user may delete these later if desired */
if not ((select COUNT(*) from bpaRole) > '0') begin
    INSERT INTO BPARole (RoleID, [RoleName], [RolePermissions]) VALUES ('1', 'System Administrator', '9223372036854775807')         --admin should always be able to do everything so he gets long.maxvalue
    INSERT INTO BPARole (RoleID, [RoleName], [RolePermissions]) VALUES ('2', 'Designer', '524343')
    INSERT INTO BPARole (RoleID, [RoleName], [RolePermissions]) VALUES ('4', 'Tester', '8128')
    INSERT INTO BPARole (RoleID, [RoleName], [RolePermissions]) VALUES ('8', 'Controller', '196608')
    INSERT INTO BPARole (RoleID, [RoleName], [RolePermissions]) VALUES ('16', 'Observer', '57344')
end
GO




/*make sure the admin can get into the system */
UPDATE BPAUser SET [Permissions]='1048567' where username='admin'
GO





/* DB Version */
insert into BPADBVersion values ('29',GETUTCDATE(),'db_upgradeR29.sql UTC','Database amendments - new user roles tables etc.')



