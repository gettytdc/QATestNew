/*
SCRIPT         : 178
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : GMB
PURPOSE        : Adds new permissions for system manager
*/

insert into BPAPerm (name) values('Processes - Exposure');
insert into BPAPerm (name) values('Business Objects - Exposure');
insert into BPAPerm (name) values('Audit - Alerts');
insert into BPAPerm (name) values('Workflow - Single Sign On');

declare @id int;
select @id = id from BPAPermGroup where name='System Manager';
insert into BPAPermGroupMember select @id, a.id from BPAPerm a
where a.name in ('Processes - Exposure', 'Business Objects - Exposure', 'Audit - Alerts', 'Workflow - Single Sign On');

select @id = id from BPAUserRole where name='System Administrator';
insert into BPAUserRolePerm select @id, a.id from BPAPerm a
where a.name in ('Processes - Exposure', 'Business Objects - Exposure', 'Audit - Alerts', 'Workflow - Single Sign On');

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '178',
  GETUTCDATE(),
  'db_upgradeR178.sql UTC',
  'Adds new permissions for system manager'
);
