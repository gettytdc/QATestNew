ALTER TABLE [BPADataPipelineOutputConfig] 
ADD selecteddashboards nvarchar(max)
GO

-- Set DB version.
INSERT INTO BPADBVersion(dbversion, 
                         scriptrundate, 
                         scriptname, 
                         [description], 
                         timezoneoffset)
VALUES('323',
       GETUTCDATE(),
       'db_upgradeR323.sql',
       'Add extra column to BPADataPipelineOutputConfig',
       0);