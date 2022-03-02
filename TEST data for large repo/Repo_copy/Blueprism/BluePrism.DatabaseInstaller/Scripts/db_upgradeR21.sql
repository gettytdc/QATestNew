/*

SCRIPT         : 21
PROJECT NAME   : Automate
DATABASE NAME  : BPA
NAME           : BPAClock
CREATION DATE  : Dec 2005
AUTHOR         : John Carter
PURPOSE        : A base table for vwBPACalendar.
NOTES          : The calendar size can be increased by changing more -1 values 
                 in the year column, but this will have affect on speed.
                 The procedure spUpdateCalendar is used to update the year 
                 column every January.
USAGE          : Uptime/Downtime Report
RELATED TO     : BPACalendar
                 vwBPACalendar
                 vwBPAUptime 
                 spBPAUptimeMonth
                 spBPAUpdateCalendar
*/

--drop TABLE [BPAClock]

CREATE TABLE BPAClock (
    [Hour] [smallint] NOT NULL ,
    [Minute] [smallint] NOT NULL

    CONSTRAINT [PK_BPAClock] PRIMARY KEY  CLUSTERED 
    (
        [Hour],
        [Minute]
    )
)
GO

INSERT INTO BPAClock VALUES(0, 0)
INSERT INTO BPAClock VALUES(1, 1)
INSERT INTO BPAClock VALUES(2, 2)
INSERT INTO BPAClock VALUES(3, 3)
INSERT INTO BPAClock VALUES(4, 4)
INSERT INTO BPAClock VALUES(5, 5)
INSERT INTO BPAClock VALUES(6, 6)
INSERT INTO BPAClock VALUES(7, 7)
INSERT INTO BPAClock VALUES(8, 8)
INSERT INTO BPAClock VALUES(9, 9)

INSERT INTO BPAClock VALUES(10, 10)
INSERT INTO BPAClock VALUES(11, 11)
INSERT INTO BPAClock VALUES(12, 12)
INSERT INTO BPAClock VALUES(13, 13)
INSERT INTO BPAClock VALUES(14, 14)
INSERT INTO BPAClock VALUES(15, 15)
INSERT INTO BPAClock VALUES(16, 16)
INSERT INTO BPAClock VALUES(17, 17)
INSERT INTO BPAClock VALUES(18, 18)
INSERT INTO BPAClock VALUES(19, 19)

INSERT INTO BPAClock VALUES(20, 20)
INSERT INTO BPAClock VALUES(21, 21)
INSERT INTO BPAClock VALUES(22, 22)
INSERT INTO BPAClock VALUES(23, 23)
INSERT INTO BPAClock VALUES(-1, 24)
INSERT INTO BPAClock VALUES(-1, 25)
INSERT INTO BPAClock VALUES(-1, 26)
INSERT INTO BPAClock VALUES(-1, 27)
INSERT INTO BPAClock VALUES(-1, 28)
INSERT INTO BPAClock VALUES(-1, 29)

INSERT INTO BPAClock VALUES(-1, 30)
INSERT INTO BPAClock VALUES(-1, 31)
INSERT INTO BPAClock VALUES(-1, 32)
INSERT INTO BPAClock VALUES(-1, 33)
INSERT INTO BPAClock VALUES(-1, 34)
INSERT INTO BPAClock VALUES(-1, 35)
INSERT INTO BPAClock VALUES(-1, 36)
INSERT INTO BPAClock VALUES(-1, 37)
INSERT INTO BPAClock VALUES(-1, 38)
INSERT INTO BPAClock VALUES(-1, 39)

INSERT INTO BPAClock VALUES(-1, 40)
INSERT INTO BPAClock VALUES(-1, 41)
INSERT INTO BPAClock VALUES(-1, 42)
INSERT INTO BPAClock VALUES(-1, 43)
INSERT INTO BPAClock VALUES(-1, 44)
INSERT INTO BPAClock VALUES(-1, 45)
INSERT INTO BPAClock VALUES(-1, 46)
INSERT INTO BPAClock VALUES(-1, 47)
INSERT INTO BPAClock VALUES(-1, 48)
INSERT INTO BPAClock VALUES(-1, 49)

INSERT INTO BPAClock VALUES(-1, 50)
INSERT INTO BPAClock VALUES(-1, 51)
INSERT INTO BPAClock VALUES(-1, 52)
INSERT INTO BPAClock VALUES(-1, 53)
INSERT INTO BPAClock VALUES(-1, 54)
INSERT INTO BPAClock VALUES(-1, 55)
INSERT INTO BPAClock VALUES(-1, 56)
INSERT INTO BPAClock VALUES(-1, 57)
INSERT INTO BPAClock VALUES(-1, 58)
INSERT INTO BPAClock VALUES(-1, 59)

GO
    

--set DB version
INSERT INTO BPADBVersion VALUES (
  '21',
  GETUTCDATE(),
  'db_upgradeR21.sql UTC',
  'Database amendments - Create and populate base table for uptime report.'
)
GO
