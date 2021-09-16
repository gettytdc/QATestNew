/*
SCRIPT         : 225
PURPOSE        : Adds the 'Design Published Dashboards' permission and assigns
             it to existing users with the 'Design Global Dashboard' permision
*/

declare @id int;
insert into BPAPerm (name) values('Design Published Dashboards');
select @id = id from BPAPerm where name = 'Design Published Dashboards';

-- Add it the System Manager group
insert into BPAPermGroupMember (permgroupid, permid)
select g.id, @id from BPAPermGroup g where g.name = 'Analytics';

-- Add it any roles that currently have 'Design Global Dashboards' permission
insert into BPAUserRolePerm (userroleid, permid)
select r.userroleid, @id from BPAUserRolePerm r inner join BPAPerm p on p.id = r.permid
where p.name = 'Design Global Dashboards';

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '225',
  GETUTCDATE(),
  'db_upgradeR225.sql',
  'Adds Design Published Dashboards Permission',
  0 -- UTC
);
