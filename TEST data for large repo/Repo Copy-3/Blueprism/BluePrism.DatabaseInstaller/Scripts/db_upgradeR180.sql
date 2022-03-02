/*
SCRIPT         : 180
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Updates groups for easy access and extensibility;
*/

--
-- Insert the new perms for editing proc/object groups
--
insert into BPAPerm (name) values('Edit Process Groups');
insert into BPAPerm (name) values('Edit Object Groups');

--
-- Add them to the 'XXX Studio' groups
--
insert into BPAPermGroupMember (permgroupid, permid)
  select pg.id, p.id
    from BPAPermGroup pg cross join BPAPerm p
    where pg.name = 'Process Studio' and p.name = 'Edit Process Groups';

insert into BPAPermGroupMember (permgroupid, permid)
  select pg.id, p.id
    from BPAPermGroup pg cross join BPAPerm p
    where pg.name = 'Object Studio' and p.name = 'Edit Object Groups';

--
-- And add them to the roles
--
insert into BPAUserRolePerm (userroleid, permid)
  select r.id, p.id
    from BPAUserRole r cross join BPAPerm p
    where r.name in ('Developer', 'Process Administrator', 'System Administrator')
      and p.name in ('Edit Process Groups', 'Edit Object Groups');

--
-- Create a joining table which links groups to the groups they are
-- contained in
--
create table BPAGroupGroup (
    groupid uniqueidentifier not null -- effectively 'parent'
        constraint FK_BPAGroupGroup_BPAGroup_groupid
            foreign key references BPAGroup(id)
            on delete cascade,
    memberid uniqueidentifier not null -- effectively 'child'
        constraint FK_BPAGroupGroup_BPAGroup_memberid
            foreign key references BPAGroup(id)
        -- a child can only have one parent, unlike other group entries
        constraint UNQ_BPAGroupGroup_memberid
            unique, -- maybe PK this?
    constraint PK_BPAGroupGroup
        primary key clustered (groupid, memberid)
);

--
-- Copy the existing group relationships to the new table
--
insert into BPAGroupGroup (groupid, memberid)
    select g.parentid, g.id
    from BPAGroup g
    where g.parentid is not null;

--
-- Remove the FK constraint on BPAGroup.parentid
--
alter table BPAGroup drop constraint FK_BPAGroup_BPAGroup;

--
-- And remove the column
--
alter table BPAGroup drop column parentid;

--
-- Add a table to hold the tree types
-- Note - this is backed by BluePrism.AutomateAppCore.Groups.TreeType
-- in the code. If this changes, that must be changed to reflect it
--
create table BPATree (
    id int not null primary key,
    name nvarchar(255) not null
);
insert into BPATree (id, name)
    select 1, 'Tiles' union all
    select 2, 'Processes' union all
    select 3, 'Objects' union all
    select 4, 'Queues' union all
    select 5, 'Resources';

-- I don't think anything is using it, but just in case 'ProcessExplorer'
-- is 0. Since we need two trees here, we'll assume they are 'Processes'
update BPAGroup
   set treeid = 2 where treeid = 0;

-- 'Tiles' was 1 before and is still 1 so needs no changing

--
-- Link the BPAGroup.treeid column to the BPATree table
--
alter table BPAGroup
    add constraint FK_BPAGroup_BPATree
        foreign key (treeid) references BPATree(id);
GO

--
-- Copy the existing process groups into the new structure
--
-- **********
-- Note that until Release Manager is fixed for these groups, it will not work in any
-- meaningful way due to the proc groups being entirely different now, but the db schema
-- is ready for the relman code to use, and existing packages should be transferred
-- across correctly.
-- **********

declare @treeid int;
select @treeid = id from BPATree where name = 'Processes';

-- Create the groups
insert into BPAGroup (treeid, id, name)
select @treeid, pg.GroupID, pg.GroupName
  from BPAProcessGroup pg;

-- They're all top level (because groups didn't support any more than that)
-- so create the assignments from their membership
insert into BPAGroupProcess (groupid, processid)
select gm.GroupID, gm.ProcessID
  from BPAProcessGroupMembership gm;

-- Now we've got that done, we need to repurpose the BPAPackageProcessGroupMember
-- table to point to BPAGroupProcess instead of BPAProcessGroupMembership
-- First remove the FK to PGMembership
alter table BPAPackageProcessGroupMember
  drop constraint FK_BPAPackageProcessGroup_BPAProcessGroupMembership;

-- The checks should work out - all the groups have been copied with the same IDs,
-- so GroupProcess should be keyed on the same values as PGMembership
alter table BPAPackageProcessGroupMember
  add constraint FK_BPAPackageProcessGroup_BPAGroupProcess
    foreign key (processgroupid, processid)
    references BPAGroupProcess(groupid, processid)
    on delete cascade;

-- With that done, we can now drop the old tables
drop table BPAProcessGroupMembership;
drop table BPAProcessGroup;

--
-- Resource and Queue Groups
-- Set these up so that this one script sets the groups DB schema for the future
-- They'll just lie unused until the development work is done to manipulate them
--
create table BPAGroupResource (
  groupid uniqueidentifier not null
    constraint FK_BPAGroupResource_BPAGroup
      foreign key references BPAGroup(id) on delete cascade,
  memberid uniqueidentifier not null
    constraint FK_BPAGroupResource_BPAResource
      foreign key references BPAResource(resourceid) on delete cascade,
   constraint PK_BPAGroupResource
        primary key clustered (groupid, memberid)
);

create table BPAGroupQueue (
  groupid uniqueidentifier not null
    constraint FK_BPAGroupQueue_BPAGroup
      foreign key references BPAGroup(id) on delete cascade,
  memberid int not null
    constraint FK_BPAGroupQueue_BPAWorkQueue
      foreign key references BPAWorkQueue(ident) on delete cascade,
  constraint PK_BPAGroupQueue
    primary key clustered (groupid, memberid)
);

--
-- VIEWS
--
GO

-- Create some placeholder views if they don't already exist
if not exists(select * from sys.views where name = 'BPVGroupedTiles')
  exec (N'create view BPVGroupedTiles as select 1 as placeholder');

if not exists(select * from sys.views where name = 'BPVGroupedProcessesObjects')
  exec (N'create view BPVGroupedProcessesObjects as select 1 as placeholder');

if not exists(select * from sys.views where name = 'BPVGroupedProcesses')
  exec (N'create view BPVGroupedProcesses as select 1 as placeholder');
    
if not exists(select * from sys.views where name = 'BPVGroupedObjects')
  exec (N'create view BPVGroupedObjects as select 1 as placeholder');
    
if not exists(select * from sys.views where name = 'BPVGroupedResources')
  exec (N'create view BPVGroupedResources as select 1 as placeholder');
  
if not exists(select * from sys.views where name = 'BPVGroupedQueues')
  exec (N'create view BPVGroupedQueues as select 1 as placeholder');
  
if not exists(select * from sys.views where name = 'BPVGroupedGroups')
  exec (N'create view BPVGroupedGroups as select 1 as placeholder');

if not exists(select * from sys.views where name = 'BPVGroupedActiveProcesses')
  exec (N'create view BPVGroupedActiveProcesses as select 1 as placeholder');

if not exists(select * from sys.views where name = 'BPVGroupedActiveObjects')
  exec (N'create view BPVGroupedActiveObjects as select 1 as placeholder');

if not exists(select * from sys.views where name = 'BPVGroupedPublishedProcesses')
  exec (N'create view BPVGroupedPublishedProcesses as select 1 as placeholder');

if not exists(select * from sys.views where name = 'BPVGroupTree')
  exec (N'create view BPVGroupTree as select 1 as placeholder');

GO

-- The tiles and their groups (null treeid, groupid, groupname if not in a group)
alter view BPVGroupedTiles as
select 
    g.treeid as treeid,
    g.id as groupid,
    g.name as groupname,
    t.id as id,
    t.name as name,
    t.tiletype as tiletype,
    t.description as description
  from BPATile t
      left join (
        BPAGroupTile gt
            inner join BPAGroup g on gt.groupid = g.id
      ) on gt.tileid = t.id;
GO

-- All the processes and/or objects and their groups (or null if not in a group)
alter view BPVGroupedProcessesObjects as
select
    g.treeid as treeid,
    g.id as groupid,
    g.name as groupname,
    p.processid as id,
    p.name as name,
    p.ProcessType as processtype,
    p.description as description,
    p.createdate as createddate,
    cu.username as createdby,
    p.lastmodifieddate as lastmodifieddate,
    p.attributeid as attributes,
    mu.username as lastmodifiedby
  from BPAProcess p
    join BPAUser cu on p.createdby = cu.userid
    join BPAUser mu on p.lastmodifiedby = mu.userid
    left join (
        BPAGroupProcess gp
            inner join BPAGroup g on gp.groupid = g.id
    ) on gp.processid = p.processid;
GO

-- Just the processes and their groups
alter view BPVGroupedProcesses as
select * from BPVGroupedProcessesObjects where processtype = 'P';
GO

-- Just the VBOs and their groups
alter view BPVGroupedObjects as
select * from BPVGroupedProcessesObjects where processtype = 'O';
GO

-- All active processes
alter view BPVGroupedActiveProcesses as
select * from BPVGroupedProcesses where (attributes & 1) = 0;
GO

-- All active objects
alter view BPVGroupedActiveObjects as
select * from BPVGroupedObjects where (attributes & 1) = 0;
GO

-- All active published processes; no empty groups
alter view BPVGroupedPublishedProcesses as
select * from BPVGroupedActiveProcesses where (attributes & 2) != 0;
GO

-- All the resources grouped together
alter view BPVGroupedResources as
select
    g.treeid as treeid,
    g.id as groupid,
    g.name as groupname,
    r.resourceid as id,
    r.name as name,
    r.attributeid as attributes,
    r.status as status
    from BPAResource r
      left join (
        BPAGroupResource gr
            inner join BPAGroup g on gr.groupid = g.id
      ) on gr.memberid = r.resourceid;
GO

if not exists (select 1 from BPADBVersion where dbversion='193') exec('
alter view BPVGroupedQueues as
select
    g.treeid as treeid,
    g.id as groupid,
    g.name as groupname,
    q.ident as id,
    q.name as name,
    q.id as guid,
    q.running as running,
    q.encryptname as encryptname
    from BPAWorkQueue q
      left join (
        BPAGroupQueue gq
            inner join BPAGroup g on gq.groupid = g.id
      ) on gq.memberid = q.ident;');
GO

alter view BPVGroupedGroups as
select
    g.treeid as treeid,
    g.id as groupid,
    g.name as groupname,
    sg.id as id,
    sg.name as name
  from BPAGroup g
    join BPAGroupGroup gg on gg.groupid = g.id
    join BPAGroup sg on gg.memberid = sg.id;
GO

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '180',
  GETUTCDATE(),
  'db_upgradeR180.sql UTC',
  'Updates groups for easy access and extensibility;'
);
