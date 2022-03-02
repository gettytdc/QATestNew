/*
SCRIPT         : 222
PURPOSE        : Adds timezoneoffset column to BPADBVersion, and removes tempory UTC flag.
*/

alter table BPADBVersion add timezoneoffset int null;
GO

update BPADBVersion set timezoneoffset=0, scriptname=replace(scriptname,' UTC','') where scriptname like '%UTC';
GO

-- set DB version
INSERT INTO BPADBVersion VALUES (
  '222',
  GETUTCDATE(),
  'db_upgradeR222.sql',
  'Adds timezoneoffset column to BPADBVersion, and removes tempory UTC flag.',
  0 -- UTC
);
