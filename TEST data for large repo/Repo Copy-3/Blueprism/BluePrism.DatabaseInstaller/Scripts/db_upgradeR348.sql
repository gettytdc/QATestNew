/*
SCRIPT         : 348
AUTHOR         : William Forster
PURPOSE        : Include locked items in pending column for QueueVolumesNow tile
*/

alter procedure [BPDS_QueueVolumesNow] @BPQueueName       nvarchar(max) = null, 
                                       @ExcludePending    nvarchar(max) = 'False', 
                                       @ExcludeDeferred   nvarchar(max) = 'False', 
                                       @ExcludeComplete   nvarchar(max) = 'False', 
                                       @ExcludeExceptions nvarchar(max) = 'False'
as
set transaction isolation level snapshot
     if @ExcludePending not in('True', 'False')
         raiserror('@ExcludePending must be either True or False', 11, 1);
         else
         if @ExcludeDeferred not in('True', 'False')
             raiserror('@ExcludeDeferred must be either True or False', 11, 1);
             else
             if @ExcludeComplete not in('True', 'False')
                 raiserror('@ExcludeComplete must be either True or False', 11, 1);
                 else
                 if @ExcludeExceptions not in('True', 'False')
                     raiserror('@ExcludeExceptions must be either True or False', 11, 1);
                     else
                     if @BPQueueName is not null
                        and not exists
                     (
                         select 1
                         from BPAWorkQueue
                         where name = @BPQueueName
                     )
                         begin
                             declare @rtnMessage varchar(500)= concat('@BPQueueName, provided queue (', @BPQueueName, ') name does not exist');
                             raiserror(@rtnMessage, 11, 1);
                     end;
                         else
                         begin
                             declare @ColumnNames nvarchar(max);
                             select @ColumnNames = isnull(@ColumnNames + ',', '') + quotename(ItemStatus)
                             FROM
                             (
                                 select 'Pending' AS ItemStatus
                                 where @ExcludePending = 'False'
                                 union
                                 select 'Deferred' AS ItemStatus
                                 where @ExcludeDeferred = 'False'
                                 union
                                 select 'Complete' AS ItemStatus
                                 where @ExcludeComplete = 'False'
                                 union
                                 select 'Exceptions' AS ItemStatus
                                 where @ExcludeExceptions = 'False'
                             ) AS StatusNarrs;
                             declare @whereClause nvarchar(max);
                             set @whereClause = @BPQueueName;
                             declare @SQLQuery nvarchar(max);
                             declare @Params nvarchar(500);
                             if @BPQueueName IS NOT NULL
                                 begin
                                     set @SQLQuery = 'with results as (
        select
            q.name,
            case
                when i.state in (1,2) then ''Pending''
                when i.state = 3 then ''Deferred''
                when i.state = 4 then ''Complete''
                when i.state = 5 then ''Exceptions''
            end as state,
            count(*) as Number
        from BPAWorkQueue q
            inner join BPVWorkQueueItem i on i.queueident=q.ident
        where i.state in (1,2,3,4,5) and q.name = @whereParam 
        group by q.name, i.state)
        select name, ' + @ColumnNames + ' from results pivot (sum(Number) for state in (' + @ColumnNames + ')) as number';
                                     set @params = N'@whereParam nvarchar(max)';
									 begin transaction
										execute sp_executesql 
										        @SQLQuery, 
										        @Params, 
										        @whereParam = @whereClause;
									 commit;
                             end;
                                 else
                                 begin
                                     set @SQLQuery = 'with results as (
        select
            q.name,
            case
                when i.state in (1,2) then ''Pending''
                when i.state = 3 then ''Deferred''
                when i.state = 4 then ''Complete''
                when i.state = 5 then ''Exceptions''
            end as state,
            count(*) as Number
        from BPAWorkQueue q
            inner join BPVWorkQueueItem i on i.queueident=q.ident
        where i.state in (1,2,3,4,5) 
        group by q.name, i.state)
        , p as (select name, ' + @ColumnNames + ' from results pivot (sum(Number) for state in (' + @ColumnNames + ')) as number)
		select * from p order by name';
									begin transaction
										execute sp_executesql 
										        @SQLQuery;
									commit;
                             end;
                     end;
                     return
go

-- Set DB version.
insert into BPADBVersion
(dbversion, 
 scriptrundate, 
 scriptname, 
 [description], 
 timezoneoffset
)
values
('348', 
 GETUTCDATE(), 
 'db_upgradeR348.sql', 
 'Include locked items in pending column for QueueVolumesNow tile', 
 0
);