

insert into BPAValCheck (checkid, catid, typeid, [description], [enabled])
values
(143, 0, 1, 'Stage has a direct or indirect reference to Processes or Objects which may contain references to items that the user does not have permission to use. Execution may fail for this user: {0}', 1)
GO

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '243',
  GETUTCDATE(),
  'db_upgradeR243.sql',
  'Add new validation checks for processes dependencies the user cannot access.',
  0 -- UTC
);
