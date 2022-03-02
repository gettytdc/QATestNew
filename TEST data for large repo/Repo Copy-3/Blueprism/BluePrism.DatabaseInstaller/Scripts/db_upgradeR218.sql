/*
SCRIPT         : 218
AUTHOR         : GM
PURPOSE        : Rename Credentials Key to Default Encryption Scheme for new installs
*/

-- Create view to allow distiguishing between installs and upgrades
if not exists(select * from sys.views where name = 'BPVScriptEnvironment')
  exec (N'create view BPVScriptEnvironment as select 1 as placeholder');
GO

alter view BPVScriptEnvironment as
select isnull(col_length('BPASysConfig', 'InstallInProgress'), 0) as InstallInProgress;
GO

-- Rename the Credentials Key scheme but ONLY if new install
if (select InstallInProgress from BPVScriptEnvironment) =  1
    update BPAKeyStore set name = 'Default Encryption Scheme' where name = 'Credentials Key';

--set DB version
INSERT INTO BPADBVersion VALUES (
  '218',
  GETUTCDATE(),
  'db_upgradeR218.sql UTC',
  'Rename Credentials Key to Default Encryption Scheme for new installs'
);
