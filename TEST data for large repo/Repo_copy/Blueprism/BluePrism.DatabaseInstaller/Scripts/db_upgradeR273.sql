
/*
Intentionally blank, was applied as a patch in 5.0.35, 6.2.2 and potentialy 6.3.1
*/

-- Set DB version.
INSERT INTO BPADBVersion(dbversion,
                         scriptrundate,
                         scriptname,
                         [description],
                         timezoneoffset)
VALUES('273',
       getutcdate(),
       'db_upgradeR273.sql',
       'Placeholder for patch script',
       0);