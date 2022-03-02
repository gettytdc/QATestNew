/* SCRIPT: 237
PURPOSE: Adds a new BPATreePerm table which maps permissions to tree types
*/


create table BPATreePerm (
    id int identity(1,1) not null,
    treeid int not null
        constraint FK_BPATreePerm_BPATree
            foreign key references BPATree(id)
            on delete cascade,
    permid int not null
        constraint FK_BPATreePerm_BPAPerm
            foreign key references BPAPerm(id),
    groupLevelPerm tinyint not null);

-- Process item level permissions
insert into BPATreePerm (treeid, permid, groupLevelPerm)
select 2, id, 0 from BPAPerm where name in (
'Delete Process',
'Edit Process',
'Export Process',
'Execute Process',
'View Process Definition');

-- Process group level permissions
insert into BPATreePerm (treeid, permid, groupLevelPerm)
select 2, id, 1 from BPAPerm where name in (
'Create Process',
'Import Process',
'Edit Process Groups',
'Manage Process Access Rights');

-- Object item level permissions
insert into BPATreePerm (treeid, permid, groupLevelPerm)
select 3, id, 0 from BPAPerm where name in (
'Delete Business Object',
'Edit Business Object',
'Export Business Object',
'Execute Business Object',
'View Business Object Definition');

-- Object group level permissions
insert into BPATreePerm (treeid, permid, groupLevelPerm)
select 3, id, 1 from BPAPerm where name in (
'Create Business Object',
'Import Business Object',
'Edit Object Groups',
'Manage Business Object Access Rights');

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '237',
  GETUTCDATE(),
  'db_upgradeR237.sql',
  'Adds a new BPATreePerm table which maps permissions to tree types',
  0 -- UTC
  );
