/*
SCRIPT         : 151
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Adds index on work queue item table to avoid deadlocks
*/
-- Index suggested by the query analyser
if not exists (
  select *
  from sys.indexes
  where name = 'Index_BPAWorkQueueItem_priorityloadedlockedfinished')
begin

    -- Get which version of Sql Server we're using
    declare @sver nvarchar(128)
    declare @ver int
    set @sver = cast(serverproperty('ProductVersion') as nvarchar)
    set @ver = convert(int, substring(@sver, 1, charindex('.', @sver) - 1))
    -- if @ver = 8 : SS2000; 9 : SS2005; 10 : SS2008

    -- SQL Server 2000 needs ARITHABORT on, which isn't on by default
    -- in our databases. We can't ALTER DATABASE within a transaction
    -- and if we 'SET ARITHABORT ON' then no query can use the index
    -- unless it also sets ARITHABORT on. So... this index is not
    -- supported within SQL Server 2000.
    if @ver > 8
      exec('
      create index Index_BPAWorkQueueItem_priorityloadedlockedfinished
      on BPAWorkQueueItem (
        priority,
        loaded,
        ident,
        locked,
        finished
      )
      include (
        queueident,
        exceptionreasonvarchar
      )
      ')
    end

INSERT INTO BPADBVersion VALUES (
  '151',
  GETUTCDATE(),
  'db_upgradeR151.sql UTC',
  'Adds index on work queue item table to avoid deadlocks'
)
