/*
SCRIPT         : 346
AUTHOR         : Alex Wheeler
PURPOSE        : Modify QueueVolumesNow to use snapshot isolation and avoid lockouts
*/

ALTER PROCEDURE [BPDS_QueueVolumesNow] @BPQueueName       NVARCHAR(MAX) = NULL, 
                                       @ExcludePending    NVARCHAR(MAX) = 'False', 
                                       @ExcludeDeferred   NVARCHAR(MAX) = 'False', 
                                       @ExcludeComplete   NVARCHAR(MAX) = 'False', 
                                       @ExcludeExceptions NVARCHAR(MAX) = 'False'
AS
SET TRANSACTION ISOLATION LEVEL SNAPSHOT
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
                             SET @WhereClause = @BPQueueName;
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
									 BEGIN TRANSACTION
										EXECUTE sp_executesql 
										        @SQLQuery, 
										        @Params, 
										        @WhereParam = @WhereClause;
									 COMMIT;
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
        , p as (select name, ' + @ColumnNames + ' from results pivot (SUM(Number) for state in (' + @ColumnNames + ')) as number)
		select * from p order by name';
									BEGIN TRANSACTION
										EXECUTE sp_executesql 
										        @SQLQuery;
									COMMIT;
                             END;
                     END;
                     return
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
('346', 
 GETUTCDATE(), 
 'db_upgradeR346.sql', 
 'Modify QueueVolumesNow to use snapshot isolation', 
 0
);