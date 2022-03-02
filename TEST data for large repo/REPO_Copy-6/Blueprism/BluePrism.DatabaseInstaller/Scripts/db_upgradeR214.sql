/*
SCRIPT         : 214
AUTHOR         : AMB
PURPOSE        : Add additional columns for password rules number of passwords and 
              number of days so the settings are no longer mutually exclusive.
RELATED BUG    : bg-251
*/

ALTER TABLE BPAPasswordRules
ADD numberofrepeats INT NOT NULL
                        DEFAULT 0;
GO
ALTER TABLE BPAPasswordRules
ADD numberofdays INT NOT NULL
                     DEFAULT 0;
GO

-- Add any existing setting into the appropriate new column
DECLARE @sql AS NVARCHAR(MAX);
SET @sql = '
UPDATE BPAPasswordRules
SET numberofrepeats = (SELECT CASE norepeats WHEN 1 THEN COALESCE((SELECT numberofrepeatsordays),0) ELSE 0 END),
numberofdays = (SELECT CASE norepeatsdays WHEN 1 THEN COALESCE((SELECT numberofrepeatsordays),0) ELSE 0 END)
            ';
EXEC sp_executesql
     @sql;
GO

-- Drop the old column (including its default value)
EXEC bpa_sp_dropdefault
     BPAPasswordRules,
     numberofrepeatsordays;
GO
ALTER TABLE BPAPasswordRules DROP COLUMN numberofrepeatsordays;
GO

--set DB version
INSERT INTO BPADBVersion
VALUES
('214',
 GETUTCDATE(),
 'db_upgradeR214.sql UTC',
 'Add additional columns for password rules number of repeats and number of days, drop column numberofrepeatsordays'
);