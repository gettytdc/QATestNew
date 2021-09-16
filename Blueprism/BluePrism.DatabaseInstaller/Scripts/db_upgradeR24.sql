/*

SCRIPT         : 24
PROJECT NAME   : Automate
DATABASE NAME  : BPA
NAME           : vwBPAUptime
CREATION DATE  : Dec 2005
AUTHOR         : John Carter
PURPOSE        : Shows the working hours in each working day in the calendar 
                 broken down into segments of 10 minutes. Every Resource and 
                 whether it was running a Session during each segment is 
                 applied to these time segments to give a view of uptime and 
                 downtime.
NOTES          : Segment size is set in 4 places. Changing this value affects 
                 the granularity, accuracy and speed of this view.
                 Working days and hours are set in the WHERE clause.
USAGE          : Uptime/Downtime Report
RELATED TO     : BPACalendar
                 vwBPACalendar
                 BPAClock
                 spBPAUptimeMonth
                 spBPAUpdateCalendar

*/

--DROP VIEW vwBPAUptime

CREATE VIEW vwBPAUptime AS

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
    CASE ISNULL(CAST(SessionId AS VARCHAR(36)), '') 
        WHEN '' THEN 0 
        ELSE 10  -- SEGMENT
    END AS uptime,
    CASE ISNULL(CAST(SessionId AS VARCHAR(36)), '') 
        WHEN '' THEN 10 -- SEGMENT
        ELSE 0 
    END AS downtime,
    r.[name] AS resource,
    DATEADD(HOUR, 8, c.[date]) AS [start],
    DATEADD(HOUR, 18, c.[date]) AS [end],
    10 AS segment

FROM vwBPACalendar c 
CROSS JOIN BPAClock h
CROSS JOIN BPAClock m
CROSS JOIN BPAResource r
LEFT OUTER JOIN BPASession s ON (
    s.StartDateTime <= (DATEADD(MINUTE, 60 * h.[hour] + m.[minute] + 10, c.[date])) -- SEGMENT
    AND s.EndDateTime >= (DATEADD(MINUTE, 60 * h.[hour] + m.[minute], c.[date]))
    AND r.ResourceId = s.StarterResourceId
)

WHERE h.[hour] >= 8 AND h.[hour] < 18 -- WORKING HOURS
AND m.[minute] % 10 = 0  -- SEGMENT
AND DATENAME(WEEKDAY, c.[date]) NOT IN ('SATURDAY', 'SUNDAY') -- WORKING DAYS



GO
    

--set DB version
INSERT INTO BPADBVersion VALUES (
  '24',
  GETUTCDATE(),
  'db_upgradeR24.sql UTC',
  'Database amendments - Create view for uptime report.'
)
GO
