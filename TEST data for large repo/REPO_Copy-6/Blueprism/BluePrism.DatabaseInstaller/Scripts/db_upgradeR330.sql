/*
SCRIPT         : 330
AUTHOR         : Craig Mitchell
PURPOSE        : Modify reporting stored procedures that use dynamic strings for SQL to use parameterised SQL with sp_executesql
*/

ALTER PROCEDURE [BPDS_QueueVolumesNow] @BPQueueName       NVARCHAR(MAX) = NULL, 
                                       @ExcludePending    NVARCHAR(MAX) = 'False', 
                                       @ExcludeDeferred   NVARCHAR(MAX) = 'False', 
                                       @ExcludeComplete   NVARCHAR(MAX) = 'False', 
                                       @ExcludeExceptions NVARCHAR(MAX) = 'False'
AS
     IF @ExcludePending NOT IN('True', 'False')
         RAISERROR('@ExcludePending must be either True or False', 11, 1);
         ELSE
         IF @ExcludeDeferred NOT IN('True', 'False')
             RAISERROR('@ExcludeDeferred must be either True or False', 11, 1);
             ELSE
             IF @ExcludeComplete NOT IN('True', 'False')
                 RAISERROR('@ExcludeComplete must be either True or False', 11, 1);
                 ELSE
                 IF @ExcludeExceptions NOT IN('True', 'False')
                     RAISERROR('@ExcludeExceptions must be either True or False', 11, 1);
                     ELSE
                     IF @BPQueueName IS NOT NULL
                        AND NOT EXISTS
                     (
                         SELECT 1
                         FROM BPAWorkQueue
                         WHERE name = @BPQueueName
                     )
                         BEGIN
                             DECLARE @rtnMessage VARCHAR(500)= CONCAT('@BPQueueName, provided queue (', @BPQueueName, ') name does not exist');
                             RAISERROR(@rtnMessage, 11, 1);
                     END;
                         ELSE
                         BEGIN
                             DECLARE @ColumnNames NVARCHAR(MAX);
                             SELECT @ColumnNames = ISNULL(@ColumnNames + ',', '') + QUOTENAME(ItemStatus)
                             FROM
                             (
                                 SELECT 'Pending' AS ItemStatus
                                 WHERE @ExcludePending = 'False'
                                 UNION
                                 SELECT 'Deferred' AS ItemStatus
                                 WHERE @ExcludeDeferred = 'False'
                                 UNION
                                 SELECT 'Complete' AS ItemStatus
                                 WHERE @ExcludeComplete = 'False'
                                 UNION
                                 SELECT 'Exceptions' AS ItemStatus
                                 WHERE @ExcludeExceptions = 'False'
                             ) AS StatusNarrs;
                             DECLARE @WhereClause NVARCHAR(MAX);
                             SET @WhereClause = ISNULL(' and q.name = ''' + @BPQueueName + '''', '');
                             DECLARE @SQLQuery NVARCHAR(MAX);
                             DECLARE @Params NVARCHAR(500);
                             IF @BPQueueName IS NOT NULL
                                 BEGIN
                                     SET @SQLQuery = 'with results as (
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
        where i.state in (1,3,4,5) AND q.name = @WhereParam 
        group by q.name, i.state)
        select name, ' + @ColumnNames + ' from results pivot (SUM(Number) for state in (' + @ColumnNames + ')) as number';
                                     SET @params = N'@WhereParam nvarchar(max)';
                                     EXECUTE sp_executesql 
                                             @SQLQuery, 
                                             @Params, 
                                             @WhereParam = @WhereClause;
                             END;
                                 ELSE
                                 BEGIN
                                     SET @SQLQuery = 'with results as (
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
        where i.state in (1,3,4,5) 
        group by q.name, i.state)
        select name, ' + @ColumnNames + ' from results pivot (SUM(Number) for state in (' + @ColumnNames + ')) as number';
                                     EXECUTE sp_executesql 
                                             @SQLQuery;
                             END;
                     END;
                     return
GO

--Update this one
ALTER PROCEDURE [BPDS_Exceptions] @BPQueueName  NVARCHAR(MAX) = NULL, 
                                  @NumberOfDays INT           = 3
AS
     IF @BPQueueName IS NOT NULL
        AND NOT EXISTS
     (
         SELECT 1
         FROM BPAWorkQueue
         WHERE name = @BPQueueName
     )
         BEGIN
             DECLARE @rtnMessage VARCHAR(500)= CONCAT('@BPQueueName, provided queue (', @BPQueueName, ') name does not exist');
             RAISERROR(@rtnMessage, 11, 1);
     END;
     IF @NumberOfDays < 1
        OR @NumberOfDays > 31
         RAISERROR('@NumberOfDays must be between 1 and 31', 11, 1);
         ELSE
         BEGIN
             DECLARE @ColumnName NVARCHAR(MAX);
             DECLARE @Query NVARCHAR(MAX);
             DECLARE @WhereClause NVARCHAR(MAX);
             DECLARE @Params NVARCHAR(500);
             DECLARE @dayVarchar NVARCHAR= CAST(@numberOfDays AS NVARCHAR);
             SET @WhereClause = ISNULL(' and q.name = ''' + @BPQueueName + '''', '');
             SELECT @ColumnName = ISNULL(@ColumnName + ',', '') + QUOTENAME(DATENAME(day, TheDate) + '-' + DATENAME(month, TheDate))
             FROM ufn_GetReportDays(@NumberOfDays)
             ORDER BY TheDate;
             IF @BPQueueName IS NOT NULL
                 BEGIN
                     SET @Query = 'select
            name, ' + @ColumnName + '
        from (select
                ISNULL(q.name, ''<unknown>'') as name,
                DATENAME(day, d.reportdate) + ''-'' + DATENAME(month, d.reportdate) as pivotdate,
                d.exceptioned
            from BPMIProductivityDaily d
                left join BPAWorkQueue q on d.queueident = q.ident
            where d.reportdate >= (select MIN(TheDate) from ufn_GetReportDays(@DaysParam))
                and q.name = @WhereParam) as src
        pivot (sum(exceptioned) for pivotdate in (' + @ColumnName + ')) as pvt';
                     SET @params = N'@WhereParam nvarchar(max), @DaysParam nvarchar';
                     EXECUTE sp_executesql 
                             @Query, 
                             @Params, 
                             @WhereParam = @WhereClause, 
                             @DaysParam = @dayVarchar;
             END;
                 ELSE
                 BEGIN
                     SET @Query = 'select
            name, ' + @ColumnName + '
        from (select
                ISNULL(q.name, ''<unknown>'') as name,
                DATENAME(day, d.reportdate) + ''-'' + DATENAME(month, d.reportdate) as pivotdate,
                d.exceptioned
            from BPMIProductivityDaily d
                left join BPAWorkQueue q on d.queueident = q.ident
            where d.reportdate >= (select MIN(TheDate) from ufn_GetReportDays(@DaysParam))
        ) as src
        pivot (sum(exceptioned) for pivotdate in (' + @ColumnName + ')) as pvt';
                     SET @params = N'@DaysParam nvarchar';
                     EXECUTE sp_executesql 
                             @Query, 
                             @Params, 
                             @DaysParam = @dayVarchar;
             END;
     END;
     return;
GO

-- Set DB version.
INSERT INTO BPADBVersion
(dbversion, 
 scriptrundate, 
 scriptname, 
 [description], 
 timezoneoffset
)
VALUES
('330', 
 GETUTCDATE(), 
 'db_upgradeR330.sql', 
 'Modify dashboard stored procedures to use sp_executesql.', 
 0
);