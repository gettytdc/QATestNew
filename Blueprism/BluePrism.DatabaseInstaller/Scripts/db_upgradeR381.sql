/*
SCRIPT		: 381
STORY		: bg-7328
PURPOSE		: Update BPDS_Exceptions error message so it can be localised
AUTHOR		: William Forster
*/

alter procedure [BPDS_Exceptions] @BPQueueName  nvarchar(max) = null, 
                                  @NumberOfDays int           = 3
as
     if @BPQueueName is not null
        and not exists
     (
         select 1
         from BPAWorkQueue
         where name = @BPQueueName
     )
         begin
             raiserror('@QueueName does not exist.', 11, 1);
     end;
     if @NumberOfDays < 1
        or @NumberOfDays > 31
         raiserror('@NumberOfDays must be between 1 and 31', 11, 1);
         else
         begin
             declare @ColumnName nvarchar(max);
             declare @Query nvarchar(max);
             declare @WhereClause nvarchar(max);
             declare @Params nvarchar(500);
             set @WhereClause = @BPQueueName;
             select @ColumnName = isnull(@ColumnName + ',', '') + quotename(datename(day, TheDate) + '-' + datename(month, TheDate))
             from ufn_GetReportDays(@NumberOfDays)
             order by TheDate;
             if @BPQueueName is not null
                 begin
                     set @Query = 'select
            name, ''Exceptions'' as [ValueLabel], ' + @ColumnName + '
        from (select
                ISNULL(q.name, ''<unknown>'') as name,
                DATENAME(day, d.reportdate) + ''-'' + DATENAME(month, d.reportdate) as pivotdate,
                d.exceptioned
            from BPMIProductivityDaily d
                left join BPAWorkQueue q on d.queueident = q.ident
            where d.reportdate >= (select MIN(TheDate) from ufn_GetReportDays(@DaysParam))
                and q.name = @WhereParam) as src
        pivot (sum(exceptioned) for pivotdate in (' + @ColumnName + ')) as pvt';
                     set @params = N'@WhereParam nvarchar(max), @DaysParam int';
                     execute sp_executesql 
                             @Query, 
                             @Params, 
                             @WhereParam = @WhereClause, 
                             @DaysParam = @numberOfDays;
             end;
                 else
                 begin
                     set @Query = 'select
            name, ''Exceptions'' as [ValueLabel], ' + @ColumnName + '
        from (select
                ISNULL(q.name, ''<unknown>'') as name,
                DATENAME(day, d.reportdate) + ''-'' + DATENAME(month, d.reportdate) as pivotdate,
                d.exceptioned
            from BPMIProductivityDaily d
                left join BPAWorkQueue q on d.queueident = q.ident
            where d.reportdate >= (select MIN(TheDate) from ufn_GetReportDays(@DaysParam))
        ) as src
        pivot (sum(exceptioned) for pivotdate in (' + @ColumnName + ')) as pvt
		order by name';
                     set @params = N'@DaysParam int';
                     execute sp_executesql 
                             @Query, 
                             @Params, 
                             @DaysParam = @numberOfDays;
             end;
     end;
     return;
go

-- set db version.
insert into bpadbversion (
	dbversion
	,scriptrundate
	,scriptname
	,[description]
	,timezoneoffset
	)
values (
	'381'
	,getutcdate()
	,'db_upgradeR381.sql'
	,'Update BPDS_Exceptions error message so it can be localised'
	,0
	);
go
