/*

SCRIPT         : 22
PROJECT NAME   : Automate
DATABASE NAME  : BPA
NAME           : BPACalendar
CREATION DATE  : Dec 2005
AUTHOR         : John Carter
PURPOSE        : A base table for vwBPACalendar.
NOTES          : The calendar size can be increased by changing more -1 values 
                 in the year column, but this will have affect on speed.
                 The procedure spUpdateCalendar is called in spUptimeMonth to 
                 update the year column every January.
USAGE          : Uptime/Downtime Report
RELATED TO     : vwBPACalendar
                 BPAClock
                 vwBPAUptime 
                 spBPAUptimeMonth
                 spBPAUpdateCalendar

*/

--drop TABLE [BPACalendar]

CREATE TABLE BPACalendar (
    [Day] [smallint] NOT NULL ,
    [Month] [smallint] NOT NULL ,
    [Year] [smallint] NOT NULL 

    CONSTRAINT [PK_BPABPACalendar] PRIMARY KEY  CLUSTERED 
    (
        [Day],
        [Month],
        [Year]
    )
)
GO

INSERT INTO BPACalendar VALUES(1,1,2003)
INSERT INTO BPACalendar VALUES(2,2,2004)
INSERT INTO BPACalendar VALUES(3,3,2005)
INSERT INTO BPACalendar VALUES(4,4,-1)
INSERT INTO BPACalendar VALUES(5,5,-1)
INSERT INTO BPACalendar VALUES(6,6,-1)
INSERT INTO BPACalendar VALUES(7,7,-1)
INSERT INTO BPACalendar VALUES(8,8,-1)
INSERT INTO BPACalendar VALUES(9,9,-1)
INSERT INTO BPACalendar VALUES(10,10,-1)
INSERT INTO BPACalendar VALUES(11,11,-1)
INSERT INTO BPACalendar VALUES(12,12,-1)
INSERT INTO BPACalendar VALUES(13,-1,-1)
INSERT INTO BPACalendar VALUES(14,-1,-1)
INSERT INTO BPACalendar VALUES(15,-1,-1)
INSERT INTO BPACalendar VALUES(16,-1,-1)
INSERT INTO BPACalendar VALUES(17,-1,-1)
INSERT INTO BPACalendar VALUES(18,-1,-1)
INSERT INTO BPACalendar VALUES(19,-1,-1)
INSERT INTO BPACalendar VALUES(20,-1,-1)
INSERT INTO BPACalendar VALUES(21,-1,-1)
INSERT INTO BPACalendar VALUES(22,-1,-1)
INSERT INTO BPACalendar VALUES(23,-1,-1)
INSERT INTO BPACalendar VALUES(24,-1,-1)
INSERT INTO BPACalendar VALUES(25,-1,-1)
INSERT INTO BPACalendar VALUES(26,-1,-1)
INSERT INTO BPACalendar VALUES(27,-1,-1)
INSERT INTO BPACalendar VALUES(28,-1,-1)
INSERT INTO BPACalendar VALUES(29,-1,-1)
INSERT INTO BPACalendar VALUES(30,-1,-1)
INSERT INTO BPACalendar VALUES(31,-1,-1)
GO
    

--set DB version
INSERT INTO BPADBVersion VALUES (
  '22',
  GETUTCDATE(),
  'db_upgradeR22.sql UTC',
  'Database amendments - Create and populate base table for uptime report.'
)
GO
