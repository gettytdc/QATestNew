DECLARE @WorkQueueAnalysisPreference VARCHAR(100) = 'EnvironmentLockTimeExpiry.WorkQueueAnalysis.InSeconds';
DECLARE @DataGatewaysPreference VARCHAR(100) = 'EnvironmentLockTimeExpiry.DataGateways.InSeconds';
DECLARE @MIReportingPreference VARCHAR(100) = 'EnvironmentLockTimeExpiry.MIReporting.InSeconds';

DECLARE @DefaultExpiryTimeInSeconds INT = 600;
DECLARE @prefId INT;

-- Insert preference value for Work Queue Analysis.
IF NOT EXISTS (SELECT 1 
               FROM BPAPref 
               WHERE [name] = @WorkQueueAnalysisPreference 
                    AND userid IS NULL)
BEGIN
    INSERT INTO BPAPref ([name]) 
    VALUES (@WorkQueueAnalysisPreference);
END

SET @prefId = (SELECT TOP 1 id 
               FROM BPAPref 
               WHERE [name] = @WorkQueueAnalysisPreference 
                  AND userid IS NULL);

IF NOT EXISTS (SELECT 1 
               FROM BPAIntegerPref 
               WHERE prefid = @prefId)
BEGIN
    INSERT INTO BPAIntegerPref (prefid, 
                                [value])
    VALUES (@prefId, 
            @DefaultExpiryTimeInSeconds);
END

-- Insert preference value for Data Gateways.
IF NOT EXISTS (SELECT 1 
               FROM BPAPref 
               WHERE [name] = @DataGatewaysPreference 
                    AND userid IS NULL)
BEGIN
    INSERT INTO BPAPref ([name]) 
    VALUES (@DataGatewaysPreference);
END

SET @prefId = (SELECT TOP 1 id 
               FROM BPAPref 
               WHERE [name] = @DataGatewaysPreference 
                    AND userid IS NULL);

IF NOT EXISTS (SELECT 1 
               FROM BPAIntegerPref 
               WHERE prefid = @prefId)
BEGIN
    INSERT INTO BPAIntegerPref (prefid, 
                                [value])
    VALUES (@prefId, 
            @DefaultExpiryTimeInSeconds);
END

-- Insert preference value for MI Reporting.
IF NOT EXISTS (SELECT 1 
               FROM BPAPref 
               WHERE [name] = @MIReportingPreference 
                    AND userid IS NULL)
BEGIN
    INSERT INTO BPAPref ([name]) 
    VALUES (@MIReportingPreference);
END

SET @prefId = (SELECT TOP 1 id 
               FROM BPAPref 
               WHERE [name] = @MIReportingPreference 
                    AND userid IS NULL);

IF NOT EXISTS (SELECT 1 
               FROM BPAIntegerPref 
               WHERE prefid = @prefId)
BEGIN
    INSERT INTO BPAIntegerPref (prefid, 
                                [value])
    VALUES (@prefId, 
            @DefaultExpiryTimeInSeconds);
END

-- Set DB version.
INSERT INTO BPADBVersion(dbversion, 
                         scriptrundate, 
                         scriptname, 
                         [description], 
                         timezoneoffset)
VALUES('316',
       getutcdate(),
       'db_upgradeR316.sql',
       'Add environment lock time expiry preference values to BPAPref table.',
       0);
