
--SCRIPT PURPOSE: Add column to BPASysConfig, BPAAuditEvents, BPAUser for Process Edit Summaries.
--NUMBER: 31
--AUTHOR: PJW
--DATE: 15/09/2005 


alter table BPAAuditEvents add EditSummary TEXT
GO

-- new option for system manager
alter table bpasysconfig add EnforceEditSummaries Bit default 1
GO

update bpasysconfig set EnforceEditSummaries = '1'
GO

-- new user preference
alter table bpauser add UseEditSummaries Bit default 1
GO

update bpauser set UseEditSummaries = '1'
GO

--set DB version
INSERT INTO BPADBVersion VALUES (
  '31',
  GETUTCDATE(),
  'db_upgradeR31.sql UTC',
  'Database amendments - Add new EnforceEditSummaries column to BPASysConfig; new column to BPAUser'
)
