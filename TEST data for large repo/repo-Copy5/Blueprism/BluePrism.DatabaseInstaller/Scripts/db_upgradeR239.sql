/*
PURPOSE: Adds the BPATreeDefaultGroup table which maps default groups to trees.
    Adds default groups for the process, object and resource trees.
*/


create table BPATreeDefaultGroup (
    id int identity(1,1) not null,
    treeid int not null
        constraint FK_BPATreeDefaultGroup_BPATree
            foreign key references BPATree(id)
            on delete cascade,
    groupid uniqueidentifier not null
        constraint FK_BPATreeDefaultGroup_BPAGroup
            foreign key references BPAGroup(id)
            on delete cascade,

    constraint UNIQUE_BPATreeDefaultGroup_TreeID unique(treeid)
    );

declare @processTreeDefaultGroupId uniqueidentifier = NEWID()
declare @objectTreeDefaultGroupId uniqueidentifier = NEWID()
declare @resourceTreeDefaultGroupId uniqueidentifier = NEWID()

declare @newdefaultnameprocesstree nvarchar (12) = 'Default'
declare @newdefaultnameobjecttree nvarchar (12) = 'Default'
declare @newdefaultnameresourcetree nvarchar (12) = 'Default'
declare @defaultcounter int = 1

while exists (select id from BPAGroup where [name] = @newdefaultnameprocesstree and treeid = 2)
begin 
    set @newdefaultnameprocesstree = concat('Default (', @defaultcounter, ')')
    set @defaultcounter = @defaultcounter + 1
end
set @defaultcounter = 1

while exists (select id from BPAGroup where [name] = @newdefaultnameobjecttree and treeid = 3)
begin 
    set @newdefaultnameobjecttree = concat('Default (', @defaultcounter, ')')
    set @defaultcounter = @defaultcounter + 1
end
set @defaultcounter = 1

while exists (select id from BPAGroup where [name] = @newdefaultnameresourcetree and treeid = 5)
begin 
    set @newdefaultnameresourcetree = concat('Default (', @defaultcounter, ')')
    set @defaultcounter = @defaultcounter + 1
end

insert into BPAGroup (id, treeid, [name], isrestricted)
values
(@processTreeDefaultGroupId, 2, @newdefaultnameprocesstree, 0),
(@objectTreeDefaultGroupId, 3, @newdefaultnameobjecttree, 0),
(@resourceTreeDefaultGroupId, 5, @newdefaultnameresourcetree, 0);

insert into BPATreeDefaultGroup (treeid, groupid)
values
(2, @processTreeDefaultGroupId),
(3, @objectTreeDefaultGroupId),
(5, @resourceTreeDefaultGroupId);

-- Move any processes / objects in the root of the trees into the default groups.
insert into BPAGroupProcess(groupid, processid) 
select @processTreeDefaultGroupID, p.processid from BPAProcess p 
left join BPAGroupProcess gp on p.processid = gp.processid
where groupid is null and p.ProcessType = 'P'

insert into BPAGroupProcess(groupid, processid) 
select @objectTreeDefaultGroupID, p.processid from BPAProcess p 
left join BPAGroupProcess gp on p.processid = gp.processid
where groupid is null and p.ProcessType = 'O'

insert into BPAGroupResource(groupid, memberid) 
select @resourceTreeDefaultGroupId, r.resourceid from BPAResource r
left join BPAGroupResource gr on r.resourceid = gr.memberid
where groupid is null

-- set DB version
insert into BPADBVersion values (
  '239',
  GETUTCDATE(),
  'db_upgradeR239.sql',
  'Adds the BPATreeDefaultGroup table which maps default groups to trees. Adds default groups for the process, object and resource trees.',
  0 -- UTC
  );