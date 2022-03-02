/*
SCRIPT         : 158
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : SW
PURPOSE        : Creates an annotated view of the schedule log
*/
-- Create an index which will help with the view

-- Get which version of Sql Server we're using
declare @sver nvarchar(128)
declare @ver int
set @sver = cast(serverproperty('ProductVersion') as nvarchar)
set @ver = convert(int, substring(@sver, 1, charindex('.', @sver) - 1))
-- if @ver = 8 : SS2000; 9 : SS2005; 10 : SS2008

-- SQL Server 2000 doesn't support the INCLUDE clause of a
-- CREATE INDEX command... and at that point it becomes a bit
-- useless, because it has to do a clustered index scan anyway
-- without those fields there, so there's no point in having a
-- useless index lying around slowing down inserts.
if @ver > 8 exec('
create index IX_BPAScheduleLogEntry_logid_entrytype
    on BPAScheduleLogEntry(schedulelogid, entrytype)
    include (entrytime, terminationreason)
')
GO

-- Create the view - this effectively brings together the
-- start / end time from the log entries into the log itself
create view BPVAnnotatedScheduleLog as
select
  l.id,
  l.scheduleid,
  l.firereason,
  l.instancetime,
  l.servername,
  l.heartbeat,
  estart.entrytime as "starttime",
  efin.entrytime as "endtime",
  efin.entrytype as "endtype",
  efin.terminationreason as "endreason"
from BPAScheduleLog l
  join BPAScheduleLogEntry estart on estart.schedulelogid = l.id and estart.entrytype = 0
  left join BPAScheduleLogEntry efin on efin.schedulelogid = l.id and efin.entrytype in (1,2)
GO

--set DB version
INSERT INTO BPADBVersion VALUES (
  '158',
  GETUTCDATE(),
  'db_upgradeR158.sql UTC',
  'Creates an annotated view of the schedule log'
);

