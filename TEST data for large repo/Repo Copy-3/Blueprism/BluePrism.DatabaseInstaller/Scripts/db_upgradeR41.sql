/*
SCRIPT         : 41
PROJECT NAME   : Automate
DATABASE NAME  : BPA
NAME           : correctcollation
CREATION DATE  : 06 Mar 2006
AUTHOR         : PJW
PURPOSE        : Allows realtimes stats views (layouts) to record preference for tabular data over graphical data.
NOTES          : An RFC document exists for this change. The document records a small amount of discussion and lists a few caveats of this script.
*/

ALTER TABLE BPAREALTIMESTATSVIEW
    ADD InTables Bit DEFAULT 0

GO
--set DB version
INSERT INTO BPADBVersion VALUES (
  '41',
  GETUTCDATE(),
  'db_upgradeR41.sql UTC',
  'Database amendments - Correct collation on columns.'
)