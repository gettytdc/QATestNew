
/*
SCRIPT         : 202
AUTHOR         : CG
PURPOSE        : Add column for Resource PC SSL status
RELATED BUG    : 9500
*/

ALTER TABLE BPAResource ADD ssl bit NOT NULL DEFAULT 0;
GO
ALTER TABLE BPASysConfig
    ADD RequireSecuredResourceConnections integer NOT NULL DEFAULT 0;

--set DB version
INSERT INTO BPADBVersion VALUES (
  '202',
  GETUTCDATE(),
  'db_upgradeR202.sql UTC',
  'Add column for Resource PC SSL status'
);
