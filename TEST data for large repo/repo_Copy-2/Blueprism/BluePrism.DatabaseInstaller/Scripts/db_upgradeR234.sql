/*
SCRIPT         : 234
PURPOSE        : Change schema of usp_setupDataSource procedure from [dbo] to current if different.
*/

declare @currentSchema nvarchar(128)
select @currentSchema = SCHEMA_NAME()

if @currentSchema <> 'dbo'
begin 
    if exists (select 1 from sysobjects 
        where id = OBJECT_ID('dbo.usp_setupDataSource')
        and 'dbo' = OBJECT_SCHEMA_NAME(id)
        and type='P')
    begin
        declare @sql nvarchar(200)
        set @sql = 'ALTER SCHEMA ' + @currentSchema + ' TRANSFER dbo.usp_setupDataSource'
        exec sp_executesql @sql
    end
end

-- set DB version
insert into BPADBVersion values (
  '234',
  GETUTCDATE(),
  'db_upgradeR234.sql',
  'Change schema of usp_setupDataSource procedure from dbo to current if different.',
  0 -- UTC
);