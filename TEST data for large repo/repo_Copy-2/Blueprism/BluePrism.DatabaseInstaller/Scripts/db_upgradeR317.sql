-- Create new data source for Queue Snapshot Against Trend
IF not exists (select * from sys.objects where type = 'P' and name = 'BPDS_QueueSnapshotAgainstTrend')
   EXEC(N'create procedure BPDS_QueueSnapshotAgainstTrend as begin set nocount on; end');
GO

-- Stored procedure to return today's snapshot data against snapshot data specified in the parameters
ALTER procedure [BPDS_QueueSnapshotAgainstTrend]
    @QueueName NVARCHAR(255) = NULL, 
    @TrendId INT = 1,
    @ColumnIdentifier INT = 1,
    @TimeRangeStartTime VARCHAR(8) = NULL,
    @TimeRangeEndTime VARCHAR(8) = NULL
AS

IF @QueueName IS NULL
    RAISERROR('@QueueName must be specified.', 11, 1);
IF @TrendId < 1 or @TrendId > 3
    RAISERROR('@Trend must be between 1 and 3.', 11, 1);
IF @ColumnIdentifier < 1 or @ColumnIdentifier > 15
    RAISERROR('@ColumnIdentifier must be between 1 and 15.', 11, 1);

DECLARE @ColumnName NVARCHAR(255) = NULL;
DECLARE @QueueIdent INT = -1;
DECLARE @Sql NVARCHAR(2000);
DECLARE @TrendDataNotAvailable BIT
DECLARE @DontUseTimeRange BIT = 1;
DECLARE @TimeRangeStartTimeConverted TIME(7);
DECLARE @TimeRangeEndTimeConverted TIME(7);
DECLARE @OffsetMinutes INT = 0
DECLARE @TodaysStartDateTime DATETIME;
DECLARE @DayOfWeek INT;

IF @TimeRangeStartTime IS NOT NULL AND @TimeRangeEndTime IS NOT NULL
BEGIN
    SET @TimeRangeStartTimeConverted = CAST(@TimeRangeStartTime AS TIME(7))
    SET @TimeRangeEndTimeConverted = CAST(@TimeRangeEndTime AS TIME(7))
    SET @DontUseTimeRange = 0;
END

-- Get queue ident and find out if trend data exists
SELECT TOP 1
    @QueueIdent = BPAWorkQueue.ident, 
    @TrendDataNotAvailable = CASE WHEN BPMIQueueTrend.id IS NULL THEN 1 ELSE 0 END
FROM BPAWorkQueue
LEFT JOIN BPMIQueueTrend
    ON BPAWorkQueue.ident = BPMIQueueTrend.queueident
    AND BPMIQueueTrend.trendid = @TrendId
WHERE BPAWorkQueue.[name] = @QueueName;

IF @QueueIdent = -1 
BEGIN
    RAISERROR('@QueueName does not exist.', 11, 1);
    RETURN;
END;

IF @TrendDataNotAvailable = 1
BEGIN
    RAISERROR('Trend data not available.', 11, 1);
    RETURN;
END;

-- Get offset minutes 
SELECT top 1 @OffsetMinutes = datepart(TZoffset, snapshotdate)
  FROM BPMIQueueSnapshot
  WHERE queueident = @QueueIdent
  ORDER BY snapshotdate DESC;

-- Work out the current day of the week (where Monday = 1 and Sunday = 7) based on the offset 
SET @TodaysStartDateTime = DATEADD(MI, @OffsetMinutes, GETUTCDATE());
SET @DayOfWeek = DATEPART(WEEKDAY, @TodaysStartDateTime) - 1
IF @DayOfWeek = 0 SET @DayOfWeek = 7 

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

-- Build query to compare current metric with trend data
SET @Sql = 'SELECT [Time],
                   [Trend Metric],
                   [Current Metric]
            FROM (SELECT CONVERT(VARCHAR(5), DATEADD(SS, configuredsnapshot.timeofdaysecs, 0), 8) as [Time],
                         trend.average' + @ColumnName + ' as [Trend Metric],
                         COALESCE(today.' + @ColumnName + ', 0) as [Current Metric] 
                  FROM BPMIConfiguredSnapshot configuredsnapshot
                  INNER JOIN BPMIQueueTrend trend 
                      ON trend.queueident = @QueueIdent 
                          AND trend.trendid = @TrendId
                          AND trend.snapshottimeofdaysecs = configuredsnapshot.timeofdaysecs 
                  LEFT JOIN BPMIQueueSnapshot today 
                      ON today.snapshotid = configuredsnapshot.snapshotid 
                      AND CAST(today.snapshotdate as date) = CAST(@TodaysStartDateTime as date)
                  WHERE configuredsnapshot.dayofweek = @DayOfWeek
                  ) Results
            WHERE @DontUseTimeRange = 1
                      OR (@TimeRangeStartTimeConverted <= Results.Time 
                          AND @TimeRangeEndTimeConverted >= Results.Time)';

EXEC sp_executesql @Sql, 
                   N'@QueueIdent INT,  
                     @TrendId INT,
                     @DontUseTimeRange BIT, 
                     @TimeRangeStartTimeConverted TIME(7), 
                     @TimeRangeEndTimeConverted TIME(7), 
                     @TodaysStartDateTime DATETIME,
                     @DayOfWeek INT',
                   @QueueIdent,
                   @TrendId,
                   @DontUseTimeRange,
                   @TimeRangeStartTimeConverted,
                   @TimeRangeEndTimeConverted,
                   @TodaysStartDateTime,
                   @DayOfWeek;
RETURN;
GO

GRANT EXECUTE ON OBJECT::BPDS_QueueSnapshotAgainstTrend TO bpa_ExecuteSP_DataSource_bpSystem;
GO

INSERT INTO BPATileDataSources(spname, tiletype, helppage)
VALUES('BPDS_QueueSnapshotAgainstTrend', 1, 'QueueSnapshotAgainstTrend.htm');

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '317',
  GETUTCDATE(), 
  'db_upgradeR317.sql UTC',
  'Create data source for comparison of todays queue against a trend',
   0);
