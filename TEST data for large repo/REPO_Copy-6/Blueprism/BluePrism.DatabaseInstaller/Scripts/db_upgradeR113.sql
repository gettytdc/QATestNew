/*
SCRIPT         : 113
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Add commonly used / useful (largely calculated) columns to work queue items
*/

-- First used in script R92
-- Find out which version of SQL Server we're running.
-- SS2K doesn't support "persisted" calculated columns, so we need
-- to code around it. This means that there will be a performance hit reading work queue
-- items in SS2000, but that's unavoidable. Previously, the hit was there for all versions
-- because the values were calculated in the query rather than the table.
DECLARE @sver nvarchar(128)
declare @ver int
SET @sver = CAST(serverproperty('ProductVersion') AS nvarchar)
SET @ver = convert(int, SUBSTRING(@sver, 1, CHARINDEX('.', @sver) - 1))
-- if @ver = 8 : SS2000; 9 : SS2005; 10 : SS2008

-- The new columns for BPAWorkQueueItem (hidden in the exec() text) are as follows :-

-- lastupdated: 
-- when was the item last changed - ie. loaded or finished - deterministic (and thus
-- persisted) - only changes when loaded, completed or exception changes

-- queuepositiondate: 
-- the date which governs this items position in the queue. non-deterministic due to
-- the reliance on GetUTCDate(), and thus cannot be persisted.

-- exceptionreasonvarchar:
-- Initial 1024 chars of exception reason allowing sorting and filtering on it.

-- prevworktime:
-- work time for previous attempt of work queue item. will need to be set whenever
-- a queue item retry instance is created.

-- attemptworktime:
-- the work time of *this attempt* only, ie. not including previous attempts of this
-- item. Deterministic and can thus be persisted.

-- state
-- integer indicating the current state of the item :
-- 1 = pending
-- 2 = locked
-- 3 = deferred (to any date, future or past)
-- 4 = completed
-- 5 = exceptioned

if @ver > 8
    exec('
alter table BPAWorkQueueItem add
    lastupdated as
        case
            when exception > completed then exception
            when exception < completed then completed
            else loaded
        end persisted
    ,queuepositiondate as
        case
            when exception is null and completed is null and locked is null and
                (deferred is null or deferred < GetUTCDate()) then loaded
            else cast(''99991231'' as datetime)
        end
    ,exceptionreasonvarchar as
        cast(exceptionreason as varchar(1024))
    ,prevworktime int not null default 0
    ,attemptworktime as worktime - prevworktime persisted
    ,state as 
        case
            when exception is not null then 5
            when completed is not null then 4
            when deferred is not null then 3
            when locked is not null then 2
            else 1
        end persisted
    ,finished as isnull(exception,completed) persisted')
else -- exactly as above without the 'persisted' keyword
    exec('
alter table BPAWorkQueueItem add
    lastupdated as
        case
            when exception > completed then exception
            when exception < completed then completed
            else loaded
        end
    ,queuepositiondate as
        case
            when exception is null and completed is null and locked is null and
                (deferred is null or deferred < GetUTCDate()) then loaded
            else cast(''99991231'' as datetime)
        end
    ,exceptionreasonvarchar as
        cast(exceptionreason as varchar(1024))
    ,prevworktime int not null default 0
    ,attemptworktime as worktime - prevworktime
    ,state as 
        case
            when exception is not null then 5
            when completed is not null then 4
            when deferred is not null then 3
            when locked is not null then 2
            else 1
        end
    ,finished as isnull(exception,completed)')
GO

-- Set the initial previous work time for all existing work queue items.
update i set 
    i.prevworktime = isnull(iprev.worktime,0)
from BPAWorkQueueItem i 
    left join BPAWorkQueueItem iprev on i.id = iprev.id and i.attempt = iprev.attempt+1;

    
-- set DB version
INSERT INTO BPADBVersion VALUES (
  '113',
  GETUTCDATE(),
  'db_upgradeR113.sql UTC',
  'Add commonly used / useful (largely calculated) columns to work queue items'
);

