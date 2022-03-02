IF NOT EXISTS (SELECT * FROM BPAPref WHERE [name] = 'ConnectionCheckRetrySeconds' AND userid IS NULL)
BEGIN
    INSERT INTO BPAPref ([name]) VALUES ('ConnectionCheckRetrySeconds')
END

DECLARE @prefId INT = (SELECT TOP 1 id from BPAPref WHERE [name] = 'ConnectionCheckRetrySeconds' AND userid IS NULL);

IF NOT EXISTS (SELECT * FROM BPAIntegerPref WHERE prefid = @prefId)
BEGIN
    INSERT INTO BPAIntegerPref (prefid, [value])
    VALUES (@prefId, 5)
END

IF NOT EXISTS (SELECT * FROM BPADataTracker WHERE dataname = 'Preferences')
BEGIN
    INSERT INTO BPADataTracker (dataname, versionno)
    VALUES ('Preferences', 1)
END

-- Set DB version.
INSERT INTO BPADBVersion(dbversion, 
                         scriptrundate, 
                         scriptname, 
                         [description], 
                         timezoneoffset)
VALUES('307',
       getutcdate(),
       'db_upgradeR307.sql',
       'Add ConnectionCheckRetrySeconds to BPA Pref.',
       0);
