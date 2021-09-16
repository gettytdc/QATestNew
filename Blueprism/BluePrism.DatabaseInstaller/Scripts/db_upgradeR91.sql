
--SCRIPT PURPOSE: Add column to BPASysConfig for configuring showing user names on login
--NUMBER: 91
--AUTHOR: GMB
--DATE: 09/03/2010

-- new option for system manager
alter table bpasysconfig add showusernamesonlogin Bit default 0
GO

update bpasysconfig set showusernamesonlogin = '0'
GO

--set DB version
INSERT INTO BPADBVersion VALUES (
  '91',
  GETUTCDATE(),
  'db_upgradeR91.sql UTC',
  'Database amendments - Add column to BPASysConfig for configuring showing user names on login'
)
