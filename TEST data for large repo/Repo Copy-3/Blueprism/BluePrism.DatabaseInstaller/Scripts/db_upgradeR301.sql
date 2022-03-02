ALTER TABLE BPADashboard ADD sendeveryseconds INT NOT NULL default 3600

ALTER TABLE BPADataPipelineSettings ADD sendpublisheddashboardstodatagateways bit default 0

-- Set DB version.
INSERT INTO BPADBVersion(dbversion, 
                         scriptrundate, 
                         scriptname, 
                         [description], 
                         timezoneoffset)

VALUES('301',
       getutcdate(),
       'db_upgradeR301.sql',
       'Add data pipeline config tables.',
       0);