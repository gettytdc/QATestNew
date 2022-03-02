/*
SCRIPT         : 164
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Fixes specific collates on BPAProcess/BPAProcessBackup
*/

-- Collate default for the BPAProcess table
alter table BPAProcess
  alter column name varchar(128) collate DATABASE_DEFAULT null
alter table BPAProcess
  alter column processtype varchar(1) collate DATABASE_DEFAULT null
alter table BPAProcess
  alter column description varchar(1000) collate DATABASE_DEFAULT null
alter table BPAProcess
  alter column version varchar(20) collate DATABASE_DEFAULT null

-- Unfortunately processxml isn't so easy in SQL 2000
alter table BPAProcess add [__temp] text
GO
update BPAProcess set [__temp] = processxml
alter table BPAProcess drop column processxml
GO
alter table BPAProcess add processxml text
GO
update BPAProcess set processxml = [__temp]
alter table BPAProcess drop column [__temp]
GO


-- And for the BPAProcessBackup table, again, do it the 2000 way
alter table BPAProcessBackup add [__temp] text
GO
update BPAProcessBackup set [__temp] = processxml
alter table BPAProcessBackup drop column processxml
GO
alter table BPAProcessBackup add processxml text
GO
update BPAProcessBackup set processxml = [__temp]
alter table BPAProcessBackup drop column [__temp]
GO

-- Finally, the ProcessGroup table
-- There is an anonymous unique index on the groupname column, we need to get rid of that first:
declare @droppersql varchar(255)

set @droppersql =
'alter table BPAProcessGroup drop constraint [' + (
    select name
    from sysobjects
    where sysobjects.xtype = 'UQ' AND sysobjects.parent_obj= OBJECT_ID(N'BPAProcessGroup')
) + ']';

exec (@droppersql);
GO

-- Now update the collation on the newly liberated column
alter table BPAProcessGroup
  alter column GroupName varchar(32) COLLATE DATABASE_DEFAULT NOT NULL
GO

-- Now re-add the index, this time give it a name so we don't need to go through all that again...
alter table BPAProcessGroup
  add constraint UQ_BPAProcessGroup_GroupName unique (GroupName);

--set DB version
insert into BPADBVersion values (
  '164',
  GETUTCDATE(),
  'db_upgradeR164.sql UTC',
  'Fixes specific collates on BPAProcess/BPAProcessBackup'
);
