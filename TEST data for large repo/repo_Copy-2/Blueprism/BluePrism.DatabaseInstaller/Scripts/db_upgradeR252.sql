/*
BUG/STORY      : US-2561
PURPOSE        : Add processId column to the internal auth table.
*/

alter table BPAInternalAuth add
ProcessId uniqueidentifier,
IsWebService bit not null default 0;

alter table BPAInternalAuth
add constraint FK_BPAInternalAuth_BPAProcess
foreign key (ProcessId) references BPAProcess(ProcessId);

-- set DB version
insert into BPADBVersion values (
  '252',
  getutcdate(),
  'db_upgradeR252.sql',
  'Add processId column to the internal auth table.',
  0 -- UTC
);