/*
BUG/STORY      : US-2018
PURPOSE        : Remove restriction on audit events comment
*/

alter table BPAAuditEvents alter column comments nvarchar(MAX) null

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '249',
  GETUTCDATE(),
  'db_upgradeR249.sql',
  'Remove restriction on audit events comment',
  0 -- UTC
);
