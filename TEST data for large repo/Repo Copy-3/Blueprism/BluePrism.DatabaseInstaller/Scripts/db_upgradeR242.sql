

delete from BPAGroupUser
    where memberid in (select userid from BPAUser where isdeleted = 1)

GO

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '242',
  GETUTCDATE(),
  'db_upgradeR242.sql',
  'Remove existing deleted users from groups',
  0 -- UTC
);
