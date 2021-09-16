/*

SCRIPT         : 23
PROJECT NAME   : Automate
DATABASE NAME  : BPA
NAME           : vwBPACalendar
CREATION DATE  : Dec 2005
AUTHOR         : John Carter
PURPOSE        : Shows the working hours in each working day in the calendar 
                 broken down into segments of 10 minutes. Every Resource and 
                 whether it was running a Session during each segment is 
                 applied to these time segments to give a view of uptime and 
                 downtime.
NOTES          : The calendar size can be increased by changing more -1 values 
                 in the year column, but this will have affect on speed.
                 This view depends on the date format yyyy-mm-dd.
USAGE          : Uptime/Downtime Report
RELATED TO     : BPACalendar
                 BPAClock
                 vwBPAUptime 
                 spBPAUptimeMonth
                 spBPAUpdateCalendar

*/

--DROP VIEW vwBPACalendar

CREATE  VIEW vwBPACalendar AS
SELECT
    c.[Year], 
    DATEPART(QUARTER,c.[Date]) AS [Quarter], 
    c.[Month], 
    DATEPART(WEEK,c.[Date]) AS [Week], 
    c.[Day], 
    c.[Date], 
    DATENAME(MONTH, c.[Date]) AS MonthName, 
    DATENAME(WEEKDAY, c.[Date]) AS DayName
FROM (
    SELECT dd.[Day], mm.[Month], yy.[Year], 
    DATEADD(DAY, dd.[Day] - 1, 
    DATEADD(MONTH, mm.[Month] - 1, 
    DATEADD(YEAR, yy.[Year] - 1900, CAST(0 AS DATETIME)))) AS [Date]
    
    FROM BPACalendar dd CROSS JOIN
    BPACalendar mm CROSS JOIN
    BPACalendar yy
    
    WHERE yy.[Year] >= 0
    AND mm.[Month] >= 0
    AND DATEPART(MONTH,
        DATEADD(DAY, dd.[Day] - 1, 
        DATEADD(MONTH, mm.[Month] - 1, 
        DATEADD(YEAR, yy.[Year] - 1900, CAST(0 AS DATETIME))))
    )=mm.[Month]
) c



GO
    

--set DB version
INSERT INTO BPADBVersion VALUES (
  '23',
  GETUTCDATE(),
  'db_upgradeR23.sql UTC',
  'Database amendments - Create view for uptime report.'
)
GO
