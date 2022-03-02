/*
SCRIPT         : 235
AUTHOR         : GM/GB
PURPOSE        : Add database structure for segregated permissions and add "Manage Access Rights" permissions.
*/

-- Create group level permissions table
create table BPAGroupUserRolePerm (
    groupid uniqueidentifier not null
        constraint FK_BPAGroupUserRolePerm_BPAGroup
            foreign key references BPAGroup(id)
            on delete cascade,
    userroleid int not null
        constraint FK_BPAGroupUserRolePerm_BPAUserRole
            foreign key references BPAUserRole(id),
    permid int not null
        constraint FK_BPAGroupUserRolePerm_BPAPerm
            foreign key references BPAPerm(id),
    constraint PK_BPAGroupUserRolePerm 
        primary key clustered (groupid, userroleid, permid),
    constraint FK_BPAGroupUserRolePerm_BPAUserRolePerm
        foreign key (permid, userroleid) references BPAUserRolePerm(permid, userroleid)
        on delete cascade);

-- Allow groups to have restricted permissions
alter table BPAGroup add isrestricted bit not null default 0;

-- Allow permissions to be linked to group trees
alter table BPAPerm add treeid int null;
GO

-- Link permissions to relevant group trees
update BPAPerm set treeid=(select id from BPATree where name='Objects')
where name in (
    'Create/Clone Business Object',
    'Delete Business Object',
    'Edit Business Object',
    'Export Business Object',
    'Import Business Object',
    'Test Business Object',
    'View Business Object',
    'Edit Object Groups');

update BPAPerm set treeid=(select id from BPATree where name='Processes')
where name in (
    'Create/Clone Process',
    'Delete Process',
    'Edit Process',
    'Export Process',
    'Import Process',
    'Test Process',
    'View Process',
    'Edit Process Groups');

-- Add the "Manage xxx Access Rights" permission
declare @id int;
insert into BPAPerm (name, treeid) select 'Manage Process Access Rights', id from BPATree where name='Processes';
insert into BPAPerm (name, treeid) select 'Manage Business Object Access Rights', id from BPATree where name='Objects';

-- Add the permissions to the relevant Studio permission group
select @id = id from BPAPermGroup where name='Process Studio';
insert into BPAPermGroupMember (permgroupid, permid) select @id, a.id from BPAPerm a
where a.name = 'Manage Process Access Rights';

select @id = id from BPAPermGroup where name='Object Studio';
insert into BPAPermGroupMember (permgroupid, permid) select @id, a.id from BPAPerm a
where a.name = 'Manage Business Object Access Rights';

-- Give the system administrator role the new permissions
select @id = id from BPAUserRole where name='System Administrator';
insert into BPAUserRolePerm (userroleid, permid) select @id, a.id from BPAPerm a
where a.name in ('Manage Process Access Rights', 'Manage Business Object Access Rights');
GO

-- Pluralise the default role names
update BPAUserRole set name = name + 's'
where name in (
    'Alert Subscriber',
    'Developer',
    'Process Administrator',
    'Runtime Resource',
    'Schedule Manager',
    'System Administrator',
    'Tester');
GO

-- Rename Test and Create/Clone Object/Process permissions
update BPAPerm set name = 'Create Business Object' where name = 'Create/Clone Business Object';
update BPAPerm set name = 'Create Process' where name = 'Create/Clone Process';
update BPAPerm set name = 'Execute Business Object' where name = 'Test Business Object';
update BPAPerm set name = 'Execute Process' where name = 'Test Process';
GO

-- Remove the redundant Compare permissions
delete from BPAPermGroupMember where permid in (select id from BPAPerm where name in ('Compare Business Objects', 'Compare Processes'));
delete from BPAUserRolePerm where permid in (select id from BPAPerm where name in ('Compare Business Objects', 'Compare Processes'));
delete from BPAPerm where name = 'Compare Business Objects'
delete from BPAPerm where name = 'Compare Processes'
GO

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '235',
  GETUTCDATE(),
  'db_upgradeR235.sql',
  'Add database structure for segregated permissions and add "Manage Access Rights" permissions.',
  0 -- UTC
);
