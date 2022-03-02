/*
SCRIPT         : 82
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Add a way of dropping an inline-defined default in SQL Server
               : Add 'session' to the BPAWorkQueueItem table so we can track
               :  an item's actions. Dropped 'resource' from the same, since it
               :  was never used, and we can get it through 'session' anyway

*/
-- Check if the SP is already there - drop it if it is.
-- 
if exists (select 1 from sysobjects 
    where id = object_id('bpa_sp_dropdefault') and type='P')
begin
    drop procedure bpa_sp_dropdefault
end
go

-- There's no easy way to drop a sys-generated default constraint (ie. 
-- a default on a column which was declared inline when creating the table).
-- The name SQL Server gives it is randomly generated, and thus different
-- for each database, and the 'alter table alter column drop default'
-- syntax just doesn't work (though MSDN seems to think it should?)
-- bpa_sp_dropdefault <TABLE>, <COLUMN>
-- is a nice easy way of getting around SQL Server's little idiosyncrasy
-- and is there for future scripts to use if needed
create procedure bpa_sp_dropdefault
    @tableName varchar(256),
    @columnName varchar(256)
as
-- SQL Server 2005 method - doesn't exist in 2000 and before
-- select @defaultName = name from sys.default_constraints 
--  where parent_object_id = object_id(@tableName)
--    and col_name(parent_object_id, parent_column_id) = @columnName

-- SQL Server 2000 method - works in 2005 for back compatibility
-- but don't know how much further down the line it will be supported
    if exists (select 1 from syscolumns 
        where id = object_id(@tableName) and name=@columnName)
    begin
        declare @defaultName varchar(256)
        select @defaultName = object_name(cdefault) from syscolumns 
            where id = object_id(@tableName) and name = @columnName
        exec('alter table [' + @tableName + '] drop constraint ' + @defaultName)
    end
go

-- The resource id was never used, so it's just taking up space and causing
-- more processing than is strictly required.
-- Aside from that, we can now access the resource (as well as a whole lot
-- more) through the session id. I say it should go.
-- Remove the FK to BPAResource
alter table BPAWorkQueueItem drop constraint FK_BPAWorkQueueItem_BPAResource;
-- Use the SP defined above to drop the default constraint on resourceid
exec bpa_sp_dropdefault 'BPAWorkQueueItem', 'resourceid';
-- And now it's good to go.
alter table BPAWorkQueueItem drop column resourceid;

-- Add the new session ID to the table
alter table BPAWorkQueueItem add sessionid uniqueidentifier null;

/* It would be nice to have a foreign key to the BPASession record here
 * but we can't - if the session gets deleted / archived, we really don't
 * want to go through and delete all the work queue items... they can
 * exist without a session, thus they should survive a session's demise.
 * The ideal would be an FK with "ON DELETE SET NULL", but this (SQL-92)
 * clause is not supported by SQL Server 2000.
 */

--set DB version
INSERT INTO BPADBVersion VALUES (
  '82',
  GETUTCDATE(),
  'db_upgradeR82.sql UTC',
  'Updated work queue items to hold the session ID on being processed.'
)
GO
