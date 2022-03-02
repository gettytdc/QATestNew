
--SCRIPT PURPOSE: Add system preference allowing administrators to remind users of forthcoming password expiries.
--NUMBER: 39
--AUTHOR: PJW
--DATE: 03/01/2005 

alter table bpasysconfig add
    PassWordExpiryWarningInterval tinyint
GO

update bpasysconfig set PassWordExpiryWarningInterval = '3'
    
--set DB version
INSERT INTO BPADBVersion VALUES (
  '39',
  GETUTCDATE(),
  'db_upgradeR39.sql UTC',
  'Database amendments - Add system preference allowing administrators to remind users of forthcoming password expiries.'
)
