/*
SCRIPT         : 89
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Precursor to the scheduler udpate - this removes the BPACalendar / BPAClock
                 tables and related views - these were part of the Crystal Reports
                 implementation that has been part of BP for the last few years.
*/

if  exists (select * from sysobjects where type='V' and id = object_id(N'[vwBPACalendar]'))
    drop view [vwBPACalendar];

if  exists (select * from sysobjects where type='V' and id = object_id(N'[vwBPAUptime]'))
    drop view [vwBPAUptime];

if  exists (select * from sysobjects where id = object_id(N'[spBPAUpdateCalendar]') and type in (N'P', N'PC'))
    drop procedure [spBPAUpdateCalendar];

if  exists (select * from sysobjects where id = object_id(N'[spBPAUptimeMonth]') and type in (N'P', N'PC'))
    drop procedure [spBPAUptimeMonth];

if exists (select * from sysobjects where id = object_id(N'[BPACalendar]') and objectproperty(id, N'IsUserTable') = 1)
    drop table [BPACalendar];

if exists (select * from sysobjects where id = object_id(N'[BPAClock]') and objectproperty(id, N'IsUserTable') = 1)
    drop table [BPAClock];

--set DB version
INSERT INTO BPADBVersion VALUES (
  '89',
  GETUTCDATE(),
  'db_upgradeR89.sql UTC',
  'Remove old Calendar / Clock tables and views'
)
