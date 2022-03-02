/*

SCRIPT         : 25
PROJECT NAME   : Automate
DATABASE NAME  : BPA
NAME           : spBPAUpdateCalendar
CREATION DATE  : Dec 2005
AUTHOR         : John Carter
PURPOSE        : To keep year values in BPACalendar upto date.
NOTES          : Works on the assumption that there are three years in 
                 BPACalendar set to Year-1, Year, Year+1. WARNING - this update 
                 will not work correctly if there are more than three years.
USAGE          : Uptime/Downtime Report
RELATED TO     : BPACalendar
                 vwBPACalendar
                 BPAClock
                 vwBPAUptime 
                 spBPAUptimeMonth

*/

--DROP PROCEDURE spBPAUpdateCalendar

CREATE PROCEDURE spBPAUpdateCalendar
AS 

UPDATE BPACalendar SET [year] = [year] + 1
WHERE DATENAME(MONTH, GETDATE()) = 'JANUARY'
AND [year] > 0
AND NOT EXISTS (
    SELECT [year] FROM BPACalendar WHERE [year] > DATEPART(YEAR, GETDATE())
)

GO
    

--set DB version
INSERT INTO BPADBVersion VALUES (
  '25',
  GETUTCDATE(),
  'db_upgradeR25.sql UTC',
  'Database amendments - Create SP for uptime report.'
)
GO
