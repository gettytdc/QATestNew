/*
SCRIPT         : 130
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Fixes queueitem.lastupdated column; Increases some field lengths
*/

-- Get the version number of the database... used in upgrade scripts passim,
-- see R92 and R114. Indexes on computed columns can only work for SS2005+
-- without changing the default value of a DB setting
declare @sver nvarchar(128)
declare @ver int
set @sver = cast(serverproperty('ProductVersion') AS nvarchar)
set @ver = convert(int, SUBSTRING(@sver, 1, CHARINDEX('.', @sver) - 1))
-- if @ver = 8 : SS2000; 9 : SS2005; 10 : SS2008

-- Drop the index on the column
if @ver > 8
  drop index INDEX_BPAWorkQueueItem_lastupdated on BPAWorkQueueItem 

-- Drop the column
alter table BPAWorkQueueItem drop column lastupdated;

-- Add the new column. Persisted if the version supports it.
if @ver > 8 exec('
  alter table BPAWorkQueueItem add lastupdated as coalesce(completed,exception,locked,loaded) persisted')
else exec('
  alter table BPAWorkQueueItem add lastupdated as coalesce(completed,exception,locked,loaded)')

if @ver > 8
  create index INDEX_BPAWorkQueueItem_lastupdated on BPAWorkQueueItem(lastupdated)

-- Now, some other less contentious non-version-dependent stuff.
alter table BPAWorkQueue alter column name varchar(255) not null;
alter table BPAWorkQueue alter column keyfield varchar(255) not null;

drop index Index_BPAWorkQueueItem_key on BPAWorkQueueItem;
alter table BPAWorkQueueItem alter column keyvalue varchar(255) not null;
create index Index_BPAWorkQueueItem_key on BPAWorkQueueItem(keyvalue);

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '130',
  GETUTCDATE(),
  'db_upgradeR130.sql UTC',
  'Fixes queueitem.lastupdated column; Increases some field lengths'
);
