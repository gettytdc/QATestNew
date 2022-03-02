/*
SCRIPT         : 199
AUTHOR         : SW
PURPOSE        : Registers work queue scripts with BPA SQL security roles
RELATED BUG    : 9370
*/

-- Add execute rights to our new role for this purpose (introduced in R186)
if DATABASE_PRINCIPAL_ID('bpa_ExecuteSP_System') is not null
    grant execute on object::usp_gettagids to bpa_ExecuteSP_System;
GO

-- Add execute rights to our new role for this purpose (introduced in R186)
if DATABASE_PRINCIPAL_ID('bpa_ExecuteSP_System') is not null
    grant execute on object::usp_getnextcase to bpa_ExecuteSP_System;
GO

alter procedure BPDS_QueueVolumesNow
    @BPQueueName nvarchar(max) = null,
    @ExcludePending nvarchar(max) = 'False',
    @ExcludeDeferred nvarchar(max) = 'False',
    @ExcludeComplete nvarchar(max) = 'False',
    @ExcludeExceptions nvarchar(max) = 'False' as

if @ExcludePending not in ('True', 'False')
    raiserror('@ExcludePending must be either True or False', 11, 1);
else if @ExcludeDeferred not in ('True', 'False')
    raiserror('@ExcludeDeferred must be either True or False', 11, 1);
else if @ExcludeComplete not in ('True', 'False')
    raiserror('@ExcludeComplete must be either True or False', 11, 1);
else if @ExcludeExceptions not in ('True', 'False')
    raiserror('@ExcludeExceptions must be either True or False', 11, 1);
else
begin
    declare @ColumnNames nvarchar(max);
    select @ColumnNames = ISNULL(@ColumnNames + ',', '') + QUOTENAME(ItemStatus)
    from (
    select 'Pending' as ItemStatus where @ExcludePending='False' union
    select 'Deferred' as ItemStatus where @ExcludeDeferred='False' union
    select 'Complete' as ItemStatus where @ExcludeComplete='False' union
    select 'Exceptions' as ItemStatus where @ExcludeExceptions='False') as StatusNarrs;

    declare @WhereClause nvarchar(max);
    set @WhereClause = ISNULL(' and q.name = ''' + @BPQueueName + '''', '');

    declare @SQLQuery nvarchar(max);
    set @SQLQuery = 'with results as (
    select
        q.name,
        case
            when i.state = 1 then ''Pending''
            when i.state = 3 then ''Deferred''
            when i.state = 4 then ''Complete''
            when i.state = 5 then ''Exceptions''
        end as state,
        COUNT(*) as Number
    from BPAWorkQueue q
        inner join BPVWorkQueueItem i on i.queueident=q.ident
    where i.state in (1,3,4,5)' + @WhereClause + '
    group by q.name, i.state)
    select name, ' + @ColumnNames + ' from results pivot (SUM(Number) for state in (' + @ColumnNames + ')) as number';

    exec(@SQLQuery);
end
return;
GO

--set DB version
INSERT INTO BPADBVersion VALUES (
  '199',
  GETUTCDATE(),
  'db_upgradeR199.sql UTC',
  'Registers work queue scripts with BPA SQL security roles'
);
