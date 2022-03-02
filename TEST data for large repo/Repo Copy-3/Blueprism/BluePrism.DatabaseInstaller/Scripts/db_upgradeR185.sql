/*
SCRIPT         : 185
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : GM
PURPOSE        : Integrates tile help and adds tiles & dashboards to release manager
*/

-- Add tile data source table
create table BPATileDataSources (
  spname nvarchar(255) not null,
  tiletype int not null,
  helppage nvarchar(255) null,
  constraint PK_BPATileDataSources primary key clustered (spName)
);
GO

-- Change chart tile type to 1 and add existing data sources
update BPATile set tiletype=1;
insert into BPATileDataSources (spname, tiletype, helppage)
 values ('BPDS_AverageHandlingTime', 1, 'AverageHandlingTime.htm');
insert into BPATileDataSources (spname, tiletype, helppage)
 values ('BPDS_AverageRetries', 1, 'AverageRetries.htm');
insert into BPATileDataSources (spname, tiletype, helppage)
 values ('BPDS_DailyProductivity', 1, 'DailyProductivity.htm');
insert into BPATileDataSources (spname, tiletype, helppage)
 values ('BPDS_DailyUtilisation', 1, 'DailyUtilisation.htm');
insert into BPATileDataSources (spname, tiletype, helppage)
 values ('BPDS_Exceptions', 1, 'Exceptions.htm');
insert into BPATileDataSources (spname, tiletype, helppage)
 values ('BPDS_FTEProductivityComparison', 1, 'FTEProductivityComparison.htm');
insert into BPATileDataSources (spname, tiletype, helppage)
 values ('BPDS_LargestTables', 1, 'LargestTables.htm');
insert into BPATileDataSources (spname, tiletype, helppage)
 values ('BPDS_ProcessUtilisationByHour', 1, 'ProcessUtilisationByHour.htm');
insert into BPATileDataSources (spname, tiletype, helppage)
 values ('BPDS_ResourceUtilisationByHour', 1, 'ResourceUtilisationByHour.htm');
insert into BPATileDataSources (spname, tiletype, helppage)
 values ('BPDS_TotalAutomations', 1, 'TotalAutomations.htm');
insert into BPATileDataSources (spname, tiletype, helppage)
 values ('BPDS_WorkforceAvailability', 1, 'WorkforceAvailability.htm');

-- Add release manager/tile link table
create table BPAPackageTile (
    packageid int not null constraint FK_BPAPackageTile_BPAPackage
        foreign key references BPAPackage(id) on delete cascade,
    tileid uniqueidentifier not null constraint FK_BPAPackageTile_BPATile
        foreign key references BPATile(id) on delete cascade,
    constraint PK_BPAPackageTile primary key (packageid, tileid)
);

-- Add release manager/dashboard link table
create table BPAPackageDashboard (
    packageid int not null constraint FK_BPAPackageDashboard_BPAPackage
        foreign key references BPAPackage(id) on delete cascade,
    dashid uniqueidentifier not null constraint FK_BPAPackageDashboard_BPADashboard
        foreign key references BPADashboard(id) on delete cascade,
    constraint PK_BPAPackageDashboard primary key (packageid, dashid)
);

-- Remove old process group table
if object_id('BPAPackageProcessGroupMember') is not null
  drop table BPAPackageProcessGroupMember;

  -- Add new import permissions
declare @id int;
insert into BPAPerm (name) values('Import Tile');
insert into BPAPerm (name) values('Import Global Dashboard');

-- Add them to the Dashboard group
select @id = id from BPAPermGroup where name='Dashboard';
insert into BPAPermGroupMember select @id, a.id from BPAPerm a
where a.name in ('Import Tile', 'Import Global Dashboard');

-- Add them to the System Manager role
select @id = id from BPAUserRole where name='System Administrator';
insert into BPAUserRolePerm select @id, a.id from BPAPerm a
where a.name in ('Import Tile', 'Import Global Dashboard');

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '185',
  GETUTCDATE(),
  'db_upgradeR185.sql UTC',
  'Integrates tile help and adds tiles & dashboards to release manager'
);
