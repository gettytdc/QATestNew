

delete from BPAGroupProcess 
    where processid in (select processid from BPAProcess where AttributeId & 1 = 1);

delete from BPAGroupResource
    where memberid in (select resourceid from BPAResource where AttributeID & 1 = 1);

GO

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '241',
  GETUTCDATE(),
  'db_upgradeR241.sql',
  'Remove existing retired processes / objects /resources from groups',
  0 -- UTC
);
