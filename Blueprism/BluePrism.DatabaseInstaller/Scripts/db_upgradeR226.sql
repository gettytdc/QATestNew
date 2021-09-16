/*
SCRIPT         : 226
PURPOSE        : Sql inject protection on tile data source import.
*/

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Check if the SP is already there - drop it if it is.
-- 
if exists (select 1 from sysobjects 
    where id = object_id('usp_setupDataSource') and type='P')
begin
    drop procedure [usp_setupDataSource]
end
go

CREATE procedure usp_setupDataSource
     @spName nvarchar(128),
     @grant bit
    as
begin
    DECLARE @sql nvarchar(200)

    if not exists(select 1 from sys.objects where type='P' and name=@spName)
    begin
        SET @sql = 'create procedure ' +  quotename(@spName) + ' as begin set nocount on; end';
        EXEC sp_executesql @sql

        if (@grant =1)
        begin
            SET @sql = 'grant execute on OBJECT::' + quotename(@spName) + ' to bpa_ExecuteSP_DataSource_custom';
            EXEC sp_executesql @sql
       end
    end
end
GO

grant execute on OBJECT::usp_setupDataSource to bpa_ExecuteSP_System;
GO

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '226',
  GETUTCDATE(),
  'db_upgradeR226.sql',
  'Sql inject protection on tile data source import.',
  0 -- UTC
);
