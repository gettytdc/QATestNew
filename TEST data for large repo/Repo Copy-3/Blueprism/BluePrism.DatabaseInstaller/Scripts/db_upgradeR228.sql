/*
SCRIPT         : 228
PURPOSE        : Adds the 'Import Published Dashboards' permission and assigns
             it to existing users with the 'Import Global Dashboard' permision
*/

declare @id int;
insert into BPAPerm (name) values('Import Published Dashboard');
select @id = id from BPAPerm where name = 'Import Published Dashboard';

-- Add it the System Manager group
insert into BPAPermGroupMember (permgroupid, permid)
select g.id, @id from BPAPermGroup g where g.name = 'Analytics';

-- Add it any roles that currently have 'Import Global Dashboard' permission
insert into BPAUserRolePerm (userroleid, permid)
select r.userroleid, @id from BPAUserRolePerm r inner join BPAPerm p on p.id = r.permid
where p.name = 'Import Global Dashboard';

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '228',
  GETUTCDATE(),
  'db_upgradeR228.sql',
  'Adds Import Published Dashboard Permission',
  0 -- UTC
);
