ALTER TABLE BPADashboard ADD lastsent datetime

-- Set DB version.
INSERT INTO BPADBVersion(dbversion, 
                         scriptrundate, 
                         scriptname, 
                         [description], 
                         timezoneoffset)

VALUES('302',
       getutcdate(),
       'db_upgradeR302.sql',
       'Add lastSent column to BPADashboard',
       0);