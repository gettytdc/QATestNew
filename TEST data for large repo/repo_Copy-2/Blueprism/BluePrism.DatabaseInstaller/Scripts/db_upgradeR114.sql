/*
SCRIPT         : 114
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Added indexes to calc columns in BPAWorkQueueItem 
*/

-- Make exceptionreasonvarchar 500 chars rather than 1024, so it can be
-- indexed cleanly
alter table BPAWorkQueueItem drop column exceptionreasonvarchar;
GO
alter table BPAWorkQueueItem add 
    exceptionreasonvarchar as cast(exceptionreason as varchar(500))
GO

-- First used in script R92
-- Find out which version of SQL Server we're running.
-- In order to manipulate indexes on calculated columns, SS2K requires the session
-- variable AUTHABORT to be ON. It's set OFF by default in the client.
-- One option is to set AUTHABORT ON when the connection is opened
-- One is set change it in the database using an ALTER DATABASE command
-- Final one is just to skip indexes on calculated columns for SS2K.
-- I decided on option 3.
DECLARE @sver nvarchar(128)
declare @ver int
SET @sver = CAST(serverproperty('ProductVersion') AS nvarchar)
SET @ver = convert(int, SUBSTRING(@sver, 1, CHARINDEX('.', @sver) - 1))
-- if @ver = 8 : SS2000; 9 : SS2005; 10 : SS2008

if @ver > 8 begin

-- Some indexes to speed up the GUI - lastupdated is the default sort order.
create index INDEX_BPAWorkQueueItem_lastupdated on BPAWorkQueueItem(lastupdated)

-- finished is primarily for the stats gathering on the queues.
create index INDEX_BPAWorkQueueItem_finished on BPAWorkQueueItem(finished)

-- exceptionreasonvarchar is for searching on exceptions... or virtual tags. See below
create index INDEX_BPAWorkQueueItem_exceptionreasonvarchar on BPAWorkQueueItem(exceptionreasonvarchar)

end
GO

-- Change the tag view to use the calculated field, in the hope that it will use the index.
-- I suspect it can't because of that call to REPLACE(), but I see no way around that right now.
alter view BPViewWorkQueueItemTag (queueitemident, tag)
as
    select it.queueitemident, t.tag
    from BPAWorkQueueItemTag it
        join BPATag t on it.tagid = t.id
union
    select i.ident, 'Exception: ' + REPLACE(i.exceptionreasonvarchar,';',':')
    from BPAWorkQueueItem i
    where i.exceptionreasonvarchar is not null
GO

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '114',
  GETUTCDATE(),
  'db_upgradeR114.sql UTC',
  'Added indexes to BPAWorkQueueItem for calculated columns'
);

