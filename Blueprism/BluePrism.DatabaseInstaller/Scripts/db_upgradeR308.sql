-- Create new data source for Queue Snapshot Comparison Data
IF not exists (select * from sys.objects where type = 'P' and name = 'BPDS_QueueSnapshotComparison')
   EXEC(N'create procedure BPDS_QueueSnapshotComparison as begin set nocount on; end');
GO

-- Stored procedure to return today's snapshot data against snapshot data specified in the parameters
ALTER procedure BPDS_QueueSnapshotComparison
    @QueueName NVARCHAR(255) = NULL, 
    @NumberOfSnapshottedDaysPrevious INT = 1,
    @ColumnIdentifier INT = 1,
    @TimeRangeStartTime VARCHAR(8) = NULL,
    @TimeRangeEndTime VARCHAR(8) = NULL
AS

IF @QueueName IS NULL
    RAISERROR('@QueueName must be specified.', 11, 1);
IF @ColumnIdentifier < 1 or @ColumnIdentifier > 15
    RAISERROR('@ColumnIdentifier must be between 1 and 15.', 11, 1);

DECLARE @Today DATETIME;
DECLARE @ColumnName NVARCHAR(255) = NULL;
DECLARE @QueueIdent INT;
DECLARE @PreviousSnapshotDate DATE;
DECLARE @Sql NVARCHAR(MAX);
DECLARE @TimeRangeStartTimeConverted TIME(7);
DECLARE @TimeRangeEndTimeConverted TIME(7);
DECLARE @DontUseTimeRange BIT = 1;
DECLARE @TimezoneOffset INT = 0

IF @TimeRangeStartTime IS NOT NULL AND @TimeRangeEndTime IS NOT NULL
BEGIN
    SET @TimeRangeStartTimeConverted = CAST(@TimeRangeStartTime AS TIME(7))
    SET @TimeRangeEndTimeConverted = CAST(@TimeRangeEndTime AS TIME(7))
    SET @DontUseTimeRange = 0;
END

SET @ColumnName = CASE @ColumnIdentifier 
    WHEN 1 THEN 'totalitems'
    WHEN 2 THEN 'itemspending'
    WHEN 3 THEN 'itemscompleted'
    WHEN 4 THEN 'itemsreferred'
    WHEN 5 THEN 'newitemsdelta'
    WHEN 6 THEN 'completeditemsdelta'
    WHEN 7 THEN 'referreditemsdelta'
    WHEN 8 THEN 'totalworktimecompleted'
    WHEN 9 THEN 'totalworktimereferred'
    WHEN 10 THEN 'totalidletime'
    WHEN 11 THEN 'totalnewsincemidnight'
    WHEN 12 THEN 'totalnewlast24hours'
    WHEN 13 THEN 'averagecompletedworktime'
    WHEN 14 THEN 'averagereferredworktime'
    WHEN 15 THEN 'averageidletime'
    ELSE 'totalitems' 
END;

-- Get the work queue ident
SELECT TOP 1 @QueueIdent = ident
FROM BPAWorkQueue
WHERE [name] = @QueueName

-- Calculate "today" according to the queue timezone - not the server time
SELECT top 1 @TimezoneOffset = DATEPART(TZoffset, snapshotdate)
  FROM BPMIQueueSnapshot
  WHERE queueident = @QueueIdent
  ORDER BY snapshotdate DESC;

SET @Today = CAST(DATEADD(MI, @TimezoneOffset, GETUTCDATE()) AS DATE);

-- Get date of previous snapshot (taking into account any gaps)
SELECT TOP 1 @PreviousSnapshotDate = CAST(snapshotdate AS DATE)
FROM BPMIQueueSnapshot 
WHERE queueident = @QueueIdent AND CAST(snapshotdate AS DATE) <= DATEADD(DAY, -@NumberOfSnapshottedDaysPrevious, @Today)
ORDER BY BPMIQueueSnapshot.snapshotdate DESC;

IF @PreviousSnapshotDate IS NULL
BEGIN
    RAISERROR('Snapshot data not found.', 11, 1);
    RETURN;
END;

-- Build query to compare current metric with 
SET @Sql = 'SELECT [Time],
                   [Previous Metric],
                   [Current Metric]
            FROM (SELECT CONVERT(VARCHAR(5), previous.snapshotdate, 108) as [Time],
                         previous.' + @ColumnName + ' as [Previous Metric],
                         today.' + @ColumnName + ' as [Current Metric] 
                  FROM BPMIQueueSnapshot previous
                  LEFT JOIN BPMIQueueSnapshot today 
                      ON today.queueident = previous.queueident
                          AND CAST(previous.snapshotdate as time) = CAST(today.snapshotdate as time) 
                          AND CAST(today.snapshotdate as date) = @Today
                  WHERE previous.queueident = @QueueIdent 
                          AND CAST(previous.snapshotdate as date) = @PreviousSnapshotDate) Results
            WHERE @DontUseTimeRange = 1
                      OR (@TimeRangeStartTimeConverted <= Results.Time 
                          AND @TimeRangeEndTimeConverted >= Results.Time);'

EXEC sp_executesql @Sql, 
                   N'@PreviousSnapshotDate DATE, 
                     @Today DATETIME,
                     @QueueIdent INT, 
                     @DontUseTimeRange BIT, 
                     @TimeRangeStartTimeConverted TIME(7), 
                     @TimeRangeEndTimeConverted TIME(7)', 
                   @PreviousSnapshotDate, 
                   @Today, 
                   @QueueIdent,
                   @DontUseTimeRange,
                   @TimeRangeStartTimeConverted,
                   @TimeRangeEndTimeConverted
RETURN;
GO

GRANT EXECUTE ON OBJECT::BPDS_QueueSnapshotComparison TO bpa_ExecuteSP_DataSource_bpSystem;
GO

INSERT INTO BPATileDataSources(spname, tiletype, helppage)
VALUES('BPDS_QueueSnapshotComparison', 1, 'QueueSnapshotComparison.htm');

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '308',
  GETUTCDATE(),
  'db_upgradeR308.sql UTC',
  'Create data source for comparison of snapshot data',
   0);