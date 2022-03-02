/*

SCRIPT         : 26
PROJECT NAME   : Automate
DATABASE NAME  : BPA
NAME           : spBPAUptimeMonth
CREATION DATE  : Dec 2005
AUTHOR         : John Carter
USAGE          : Uptime/Downtime Report
PURPOSE        : Shows the working hours in each working day in the calendar 
                 broken down into segments of 10 minutes. Every Resource and 
                 whether it was running a Session during each segment is 
                 applied to these time segments to give a view of uptime and 
                 downtime.
NOTES          : Assumes that the working week is Monday to Friday and
                 includes all days from the start week and the end week
                 even if they are from the previous or next months.
                 Sessions that appear to have run overnight or for many days
                 are assumed to have actually crashed sometime on the start
                 day. In such cases a default session time of 1 hour is used.
                 Changing the Segment size affects the granularity, accuracy 
                 and speed of this procedure.
RELATED TO     : BPACalendar
                 vwBPACalendar
                 BPAClock
*/

CREATE PROCEDURE spBPAUptimeMonth
    @Year INT = -1 , 
    @Month INT = -1
AS 

DECLARE @CurrentDate DATETIME
DECLARE @Date DATETIME
DECLARE @FirstOfMonth DATETIME
DECLARE @LastOfMonth DATETIME
DECLARE @StartWeek INT
DECLARE @EndWeek INT
DECLARE @StartYear INT
DECLARE @EndYear INT
DECLARE @MonthDiff INT
DECLARE @CurrentYear INT
DECLARE @CurrentMonth INT
DECLARE @StartHour INT
DECLARE @EndHour INT

SET @StartHour = 9 --Start of working day
SET @EndHour = 17 --End of working day

-- Use the current date to get a DATETIME variable. This is easier
-- than trying to cope with potential CAST and Crystal problems.
SET @CurrentDate = GETUTCDATE()
SET @CurrentYear = DATEPART(YEAR, @CurrentDate)
SET @CurrentMonth = DATEPART(MONTH, @CurrentDate) 

IF (@Year < 0) OR (@Month < 0) BEGIN
    -- Paramters have default values so use the current date.
    SET @Date = @CurrentDate
    SET @Year = @CurrentYear 
    SET @Month = @CurrentMonth 
END
ELSE BEGIN
    -- Modify the year and month of the current date using the given parameters.
    -- NB Had real trouble getting Crystal to accept DATEADD(YEAR,,) - just does
    -- not seem to work, even thoughth the SQL is valid. Hence the 12 * month stuff.  
    IF @Year < @CurrentYear
        SET @MonthDiff = 12 * (@CurrentYear - @Year - 1) - @CurrentMonth - (12 - @Month)
    ELSE IF @Year > @CurrentYear
        SET @MonthDiff = 12 * (@Year - @CurrentYear - 1) + @Month + (12 - @CurrentMonth)
    ELSE 
        SET @MonthDiff = @Month - @CurrentMonth

    SET @Date = DATEADD(MONTH, @MonthDiff, @CurrentDate)
END

-- Get the start and end dates of the required month
SET @FirstOfMonth = DATEADD(DAY, 1 - DATEPART(DAY, @Date), @Date)
SET @LastOfMonth = DATEADD(DAY, -1, DATEADD(MONTH, 1, @FirstOfMonth))

-- If the month is January and starts midweek, the days from earlier 
-- in the week will be from the previous year.
IF (@Month = 1 AND DATENAME(WEEKDAY, @FirstOfMonth) IN 
    ('TUESDAY', 'WEDNESDAY', 'THURSDAY', 'FRIDAY')) BEGIN -- WORKING WEEK
    SET @StartYear = @Year - 1
    SET @StartWeek = DATEPART(WEEK, DATEADD(DAY, -1, @FirstOfMonth))
END
ELSE BEGIN
    SET @StartYear = @Year
    SET @StartWeek = DATEPART(WEEK, @FirstOfMonth)
END

-- If the month is December and ends midweek, the days from later 
-- in the week will be from the next year.
IF (@Month = 12 AND DATENAME(WEEKDAY, @LastOfMonth) IN 
    ('MONDAY', 'TUESDAY', 'WEDNESDAY', 'THURSDAY')) BEGIN -- WORKING WEEK
    SET @EndYear = @Year + 1
    SET @EndWeek = 1
END
ELSE BEGIN
    SET @EndYear = @Year
    SET @EndWeek = DATEPART(WEEK, @LastOfMonth) 
END

-- Make sure the year column in BPACalendar table is correct.
-- NB The IF statement is a bodge to suppress an error in Crystal.
-- @Month will always be > 1 but for some reason there is an RTE
-- without it. Suspect it is something to do with the way Crystal
-- interprets SQL.
IF (@Month > 0) BEGIN
    UPDATE BPACalendar SET [year] = @Year - 1 WHERE [Day] = 1 AND [Month] = 1
    UPDATE BPACalendar SET [year] = @Year WHERE [Day] = 2 AND [Month] = 2
    UPDATE BPACalendar SET [year] = @Year + 1 WHERE [Day] = 3 AND [Month] = 3
END

-- Make a temporary table to hold session details
CREATE TABLE #BPASessionTemp (
    [SessionID] [uniqueidentifier] NOT NULL ,
    [StartDateTime] [datetime] NULL ,
    [EndDateTime] [datetime] NULL,
    StarterResourceId [uniqueidentifier]
) 

-- Again, the IF here is only to get around some Crystal parsing problem.
IF (@Month > 0) BEGIN
    -- Copy session details from the required period into the temp table
    INSERT INTO #BPASessionTemp
    SELECT [SessionID], [StartDateTime], 
    CASE StatusId 
    WHEN 2 THEN --Failed
        CASE DATEDIFF(DAY, [StartDateTime], ISNULL([EndDateTime], GETDATE())) 
        WHEN 0 THEN 
            --Failed on the start day
            ISNULL([EndDateTime], GETDATE())
        ELSE 
            --Failed after the start day, so assume a duration of 1 hour.
            --NB A more accurate end date could be found in the log
            --table but fetching it out would slow things down too much
            DATEADD(HOUR, 1, [StartDateTime])
        END
    ELSE 
        --Use the current time if the session is in progress
        ISNULL([EndDateTime], GETDATE())
    END AS [EndDateTime], 
    StarterResourceId 
    FROM BPASession
    WHERE 
    StatusId IN (1, 2, 3, 4) --running, failed, stopped, completed
    AND ((DATEPART(YEAR, StartDateTime)= @Year AND DATEPART(MONTH, StartDateTime)=@Month)
        OR (DATEPART(YEAR, StartDateTime)= @StartYear AND DATEPART(WEEK, StartDateTime) = @StartWeek) 
        OR (DATEPART(YEAR, EndDateTime)= @EndYear AND DATEPART(WEEK, EndDateTime) = @EndWeek))

END

-- Combine the session details with the calendar and clock to generate uptime data.
SELECT DISTINCT 
    c.[year],
    c.[quarter],
    c.[month],
    c.[week],
    c.[day],
    c.[date],
    h.[hour],
    m.[minute],
    DATEADD(MINUTE, 60 * h.[hour] + m.[minute], c.[date]) AS [time], 

    CASE ISNULL(CAST(SessionID AS VARCHAR(36)), '') 
        WHEN '' THEN 0 
        ELSE 10  -- SEGMENT
    END AS uptime,
    CASE ISNULL(CAST(SessionID AS VARCHAR(36)), '') 
        WHEN '' THEN 10 -- SEGMENT
        ELSE 0 
    END AS downtime,

    r.[name] AS resource,
    DATEADD(HOUR, @StartHour, c.[date]) AS [start], -- WORKING HOURS
    DATEADD(HOUR, @EndHour, c.[date]) AS [end], -- WORKING HOURS
    10 AS segment  -- SEGMENT

FROM vwBPACalendar c 
CROSS JOIN BPAClock h
CROSS JOIN BPAClock m
CROSS JOIN BPAResource r
LEFT OUTER JOIN #BPASessionTemp s ON (
    s.StartDateTime <= (DATEADD(MINUTE, 60 * h.[hour] + m.[minute] + 10, c.[date])) -- SEGMENT
    AND s.EndDateTime >= (DATEADD(MINUTE, 60 * h.[hour] + m.[minute], c.[date]))
    AND r.ResourceId = s.StarterResourceId
)

WHERE h.[hour] >= @StartHour AND h.[hour] < @EndHour -- WORKING HOURS
AND m.[minute] % 10 = 0  -- SEGMENT
AND DATENAME(WEEKDAY, c.[date]) NOT IN ('SATURDAY', 'SUNDAY') -- WORKING DAYS
AND(
    (c.[year]= @StartYear AND c.[week] = @StartWeek) 
    OR (c.[year]= @EndYear AND c.[week] = @EndWeek) 
    OR (c.[year]= @Year AND c.[month] = @Month )
)



GO

--set DB version
INSERT INTO BPADBVersion VALUES (
  '26',
  GETUTCDATE(),
  'db_upgradeR26.sql UTC',
  'Database amendments - Create SP for uptime report.'
)
