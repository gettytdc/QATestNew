/*
SCRIPT         : 206
AUTHOR         : SW
PURPOSE        : Add appropriate EXEC permission to usp_rethrow
*/

-- Add execute rights to our new role for the rethrow SP
if DATABASE_PRINCIPAL_ID('bpa_ExecuteSP_System') is not null
    grant execute on object::usp_rethrow to bpa_ExecuteSP_System;
GO

--set DB version
INSERT INTO BPADBVersion VALUES (
  '206',
  GETUTCDATE(),
  'db_upgradeR206.sql UTC',
  'Add appropriate EXEC permission to usp_rethrow'
);
