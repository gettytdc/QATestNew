/*

SCRIPT         : 62
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : Ciaran
PURPOSE        : Add Local field to BPAResource table
*/


ALTER TABLE [BPAResource]
    ADD Local BIT NOT NULL DEFAULT 0
GO


--set DB version
INSERT INTO BPADBVersion VALUES (
  '62',
  GETUTCDATE(),
  'db_upgradeR62.sql UTC',
  'Database amendments - added Local field to BPAResource (bug 3249).'
)
GO
