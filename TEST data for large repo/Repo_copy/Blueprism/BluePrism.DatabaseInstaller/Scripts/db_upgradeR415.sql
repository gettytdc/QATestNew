/*
STORY:      BP-2577
PURPOSE:    Adds new permissions for Authentication Server user mapping
*/

insert into BPAPerm (name, requiredFeature) values('Authentication Server - Map Users', '');

declare @id int;
select @id = id from BPAPermGroup where name='System Manager';
insert into BPAPermGroupMember select @id, a.id from BPAPerm a
where a.name = 'Authentication Server - Map Users';

select @id = id from BPAUserRole where name='System Administrators';
insert into BPAUserRolePerm select @id, a.id from BPAPerm a
where a.name = 'Authentication Server - Map Users';

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '415',
  GETUTCDATE(),
  'db_upgradeR415.sql UTC',
  'Adds new permissions for Authentication Server user mapping',
  0
);
