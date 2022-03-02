alter view BPVGroupedResources as
select
    g.treeid as treeid,
    case when r.pool is not null then r.pool else g.id end as groupid,
    g.name as groupname,
    r.resourceid as id,
    r.name as name,
    r.attributeid as attributes,
    case when r.pool is not null then 1 else 0 end as ispoolmember,
    r.status as status
    from BPAResource r
      left join (
        BPAGroupResource gr
            inner join BPAGroup g on gr.groupid = g.id
      ) on gr.memberid = r.resourceid;
GO
declare @treeid int;
select @treeid=id from BPATree where name='Resources';

-- Add new/rename existing resource permissions
insert into BPAPerm (name, treeid) values ('Authenticate as Resource', null);
insert into BPAPerm (name, treeid) values ('Manage Resource Access Rights', @treeid);
insert into BPAPerm (name, treeid) values ('Edit Resource Groups', @treeid);
update BPAPerm set name='Configure Resource', treeid=@treeid where name='Resources - Management';
update BPAPerm set name='View Resource', treeid=@treeid where name='Read Access to Session Management';
update BPAPerm set name='Control Resource', treeid=@treeid where name='Full Access to Session Management';
update BPAPerm set name='View Resource Screen Captures', treeid=@treeid where name='View resource screen captures';

-- Add new Resources permissions group, and assign relevant permissions to it
declare @groupid int;
insert into BPAPermGroup (name) values ('Resources');
select @groupid=id from BPAPermGroup where name='Resources';
insert into BPAPermGroupMember (permgroupid, permid) select @groupid, id from BPAPerm where name='Authenticate as Resource';
insert into BPAPermGroupMember (permgroupid, permid) select @groupid, id from BPAPerm where name='Manage Resource Access Rights';
insert into BPAPermGroupMember (permgroupid, permid) select @groupid, id from BPAPerm where name='Edit Resource Groups';
update BPAPermGroupMember set permgroupid=@groupid where permid in (
    select id from BPAPerm where name in ('View Resource', 'Configure Resource', 'Control Resource', 'View Resource Screen Captures'));

-- Associate relevant group level permissions to Resources tree
insert into BPATreePerm (treeid, permid, groupLevelPerm) select @treeid, id, 0 from BPAPerm where name='View Resource';
insert into BPATreePerm (treeid, permid, groupLevelPerm) select @treeid, id, 0 from BPAPerm where name='Configure Resource';
insert into BPATreePerm (treeid, permid, groupLevelPerm) select @treeid, id, 0 from BPAPerm where name='Control Resource';
insert into BPATreePerm (treeid, permid, groupLevelPerm) select @treeid, id, 1 from BPAPerm where name='Manage Resource Access Rights';
insert into BPATreePerm (treeid, permid, groupLevelPerm) select @treeid, id, 0 from BPAPerm where name='View Resource Screen Captures';
insert into BPATreePerm (treeid, permid, groupLevelPerm) select @treeid, id, 1 from BPAPerm where name='Edit Resource Groups';

-- Give the system administrators role the new Resource permissions
declare @id int;
select @id = id from BPAUserRole where name='System Administrators';
insert into BPAUserRolePerm (userroleid, permid) select @id, a.id from BPAPerm a
where a.name in ('Authenticate as Resource', 'Manage Resource Access Rights');

-- Give the Authenticate as Resource permission to the Runtime Resoures role
select @id = id from BPAUserRole where name='Runtime Resources';
insert into BPAUserRolePerm (userroleid, permid) select @id, a.id from BPAPerm a
where a.name = 'Authenticate as Resource';

-- Give 'Edit Resource Groups' to any roles that previously had 'Resources - Management'
-- (which is now called 'Configure Resource')
select @id = id from BPAPerm where name='Edit Resource Groups';
insert into BPAUserRolePerm (userroleid, permid)
select rp.userroleid, @id from BPAUserRolePerm rp inner join BPAPerm p on p.id=rp.permid
where p.name='Configure Resource'

-- Drop the old way of doing permissions table
drop table BPAResourceRole;

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '244',
  GETUTCDATE(),
  'db_upgradeR244.sql',
  'Add resource related group based permissions',
  0 -- UTC
);