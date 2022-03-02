IF EXISTS(SELECT * FROM sys.indexes WHERE object_id = object_id('BPATaskSession') AND NAME ='IX_BPATaskSession_taskid')
    DROP INDEX [IX_BPATaskSession_taskid] ON [BPATaskSession];
GO
CREATE INDEX [IX_BPATaskSession_taskid] ON [BPATaskSession] (taskid);

IF EXISTS(SELECT * FROM sys.indexes WHERE object_id = object_id('BPAScheduleLogEntry') AND NAME ='IX_BPAScheduleLogEntry_logsess')
    DROP INDEX [IX_BPAScheduleLogEntry_logsess] ON [BPAScheduleLogEntry];
GO
CREATE INDEX [IX_BPAScheduleLogEntry_logsess] ON [BPAScheduleLogEntry] (logsessionnumber);

IF EXISTS(SELECT * FROM sys.indexes WHERE object_id = object_id('BPAScheduleTrigger') AND NAME ='IX_BPAScheduleTrigger_schedule')
    DROP INDEX [IX_BPAScheduleTrigger_schedule] ON [BPAScheduleTrigger];
GO
CREATE INDEX [IX_BPAScheduleTrigger_schedule] ON [BPAScheduleTrigger] (ScheduleId);

-- set DB version
insert into BPADBVersion values (
  '263',
  getutcdate(),
  'db_upgradeR263.sql',
  'Add missing scheduler indexes',
  0 -- UTC
)