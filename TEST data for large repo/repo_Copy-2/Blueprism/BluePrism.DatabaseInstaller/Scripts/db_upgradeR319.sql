if not exists (SELECT 1 FROM BPAStatus WHERE statusid = 8)
BEGIN
    INSERT INTO BPAStatus(statusid, [type], [description])
    VALUES (8, 'RUN', 'Warning');
END

-- Set DB version.
INSERT INTO BPADBVersion(dbversion, 
                         scriptrundate, 
                         scriptname, 
                         [description], 
                         timezoneoffset)
VALUES('319',
       GETUTCDATE(),
       'db_upgradeR319sql',
       'Add missing Warning status to BPAStatus table.',
       0);