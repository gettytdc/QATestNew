ALTER TABLE BPASysWebConnectionSettings
ADD connectiontimeout int null

-- Set DB version.
INSERT INTO BPADBVersion(dbversion,
                         scriptrundate,
                         scriptname,
                         [description],
                         timezoneoffset)
VALUES('267',
       getutcdate(),
       'db_upgradeR267.sql',
       'Add connectiontimeout column to BPASysWebConnectionSettings',
       0);
