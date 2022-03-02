-- Check if column already exists.
IF COL_LENGTH('BPATask', 'delayafterend') IS NULL
BEGIN
    ALTER TABLE BPATask
    ADD delayafterend INT NOT NULL DEFAULT 0;
END

-- Set DB version.
INSERT INTO BPADBVersion(dbversion,
                         scriptrundate,
                         scriptname,
                         [description],
                         timezoneoffset)
VALUES('271',
       getutcdate(),
       'db_upgradeR271.sql',
       'Add delayafterend column to BPATask table.',
       0);
