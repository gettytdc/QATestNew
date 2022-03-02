/*
SCRIPT         : 196
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : CG
PURPOSE        : Add fields for Resource FQDN support
*/


IF NOT EXISTS(SELECT * FROM sys.columns 
    WHERE Name = N'ResourceRegistrationMode' AND Object_ID = Object_ID(N'BPASysConfig'))
    begin
        ALTER TABLE BPASysConfig
            ADD ResourceRegistrationMode integer NOT NULL DEFAULT 0;
    end
IF NOT EXISTS(SELECT * FROM sys.columns 
    WHERE Name = N'FQDN' AND Object_ID = Object_ID(N'BPAResource'))
    begin
        ALTER TABLE BPAResource
            ADD FQDN nvarchar(max) NULL;
    end
ALTER TABLE BPASysConfig
    ADD PreventResourceRegistration integer NOT NULL DEFAULT 0;
GO

--set DB version
INSERT INTO BPADBVersion VALUES (
  '196',
  GETUTCDATE(),
  'db_upgradeR196.sql UTC',
  'Add fields for Resource FQDN support'
);


