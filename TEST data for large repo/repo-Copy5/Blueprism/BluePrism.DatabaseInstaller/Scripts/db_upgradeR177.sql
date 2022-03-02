/*
SCRIPT         : 177
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : GM
PURPOSE        : Adds new table structure for generic groups and MI dashboards
*/

--Generic groups table
create table BPAGroup (
    id uniqueidentifier not null,
    treeid int not null,
    name nvarchar(255) not null,
    parentid uniqueidentifier null constraint FK_BPAGroup_BPAGroup
     foreign key references BPAGroup(id),
    constraint PK_BPAGroup primary key clustered (id)
);

--Process/object group items
create table BPAGroupProcess (
    groupid uniqueidentifier not null constraint FK_BPAGroupProcess_BPAGroup
     foreign key references BPAGroup(id) on delete cascade,
    processid uniqueidentifier not null constraint FK_BPAGroupProcess_BPAProcess
     foreign key references BPAProcess(processid) on delete cascade,
    constraint PK_BPAGroupProcess primary key clustered (groupid, processid)
);

--Tile definitions
create table BPATile (
    id uniqueidentifier not null,
    name nvarchar(255) not null,
    tiletype int not null,
    description nvarchar(255) null,
    autorefresh int not null,
    xmlproperties nvarchar(max) null,
    constraint PK_BPATile primary key clustered (id),
    constraint Index_BPATile_name unique (name)
);

--MI Dashboard definitions
create table BPADashboard (
    id uniqueidentifier not null,
    name nvarchar(255) not null,
    dashtype int not null,
    userid uniqueidentifier null constraint FK_BPADashboard_BPAUser
     foreign key references BPAUser(userid),
    constraint PK_BPADashboard primary key clustered (id)
);

--Link tiles to dashboard
create table BPADashboardTile (
    dashid uniqueidentifier not null constraint FK_BPADashboardTile_BPADashboard
     foreign key references BPADashboard(id) on delete cascade,
    tileid uniqueidentifier not null constraint FK_BPADashboardTile_BPATile
     foreign key references BPATile(id) on delete cascade,
    displayorder int not null,
    width int not null,
    height int not null,
    constraint PK_BPADashboardTile primary key clustered (dashid, tileid)
);

--Tile group items
create table BPAGroupTile (
    groupid uniqueidentifier not null constraint FK_BPAGroupTile_BPAGroup
     foreign key references BPAGroup(id) on delete cascade,
    tileid uniqueidentifier not null constraint FK_BPAGroupTile_BPATile
     foreign key references BPATile(id) on delete cascade,
    constraint PK_GroupTile primary key clustered (groupid, tileid)
);

--Create additional permissions group & permissions
declare @id int;

insert into BPAPerm values('Design Personal Dashboards');
insert into BPAPerm values('Design Global Dashboards');
insert into BPAPerm values('Create/Edit/Delete Tiles');
insert into BPAPerm values('View Dashboards');
insert into BPAPerm values('View Reports');
insert into BPAPermGroup values('Review');

select @id = id from BPAPermGroup where name='Review';
insert into BPAPermGroupMember select @id, a.id from BPAPerm a
where a.name in ('Design Personal Dashboards', 'Design Global Dashboards', 'Create/Edit/Delete Tiles', 'View Dashboards', 'View Reports');

select @id = id from BPAUserRole where name='System Administrator';
insert into BPAUserRolePerm select @id, a.id from BPAPerm a
where a.name in ('Design Personal Dashboards', 'Design Global Dashboards', 'Create/Edit/Delete Tiles', 'View Dashboards', 'View Reports');

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '177',
  GETUTCDATE(),
  'db_upgradeR177.sql UTC',
  'Adds new table structure for generic groups and MI dashboards'
);
