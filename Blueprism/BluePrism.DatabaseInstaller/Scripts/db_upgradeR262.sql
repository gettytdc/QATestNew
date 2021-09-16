
insert into BPAPerm ([name]) values ('View Skill'),('Manage Skill'),('Import Skill');
insert into BPAPermGroup ([name]) values('Skills');

declare @skillGroupId as int
select @skillGroupId = id from BPAPermGroup where [name] = 'Skills'

insert into BPAPermGroupMember (permgroupid, permid)
    select @skillGroupId, id from BPAPerm
    where [name] in ('View Skill', 'Manage Skill', 'Import Skill');

insert into BPAUserRolePerm (userroleid, permid)
    select r.id, m.permid from BPAUserRole r
        cross join BPAPermGroupMember m
    where r.name = 'System Administrators' and m.permgroupid = @skillGroupId
go

-- set DB version
insert into BPADBVersion values (
  '262',
  getutcdate(),
  'db_upgradeR262.sql',
  'Add new View, Manage and Import Skill permissions.',
  0 -- UTC
)