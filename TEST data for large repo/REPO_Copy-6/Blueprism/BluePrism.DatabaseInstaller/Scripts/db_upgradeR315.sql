IF DATABASE_PRINCIPAL_ID('BPA_DataGatewaysEngine') IS NULL
    EXEC(N'CREATE ROLE BPA_DataGatewaysEngine');
GO

GRANT DELETE ON [BPADataPipelineInput] TO BPA_DataGatewaysEngine
GO

GRANT SELECT ON [BPADataPipelineInput] TO BPA_DataGatewaysEngine
GO


-- Set DB version.
insert into BPADBVersion(dbversion, 
                         scriptrundate, 
                         scriptname, 
                         [description], 
                         timezoneoffset)
values('315',
       getutcdate(),
       'db_upgradeR315.sql',
       'Create BPA_DataGatewaysEngine Role',
       0);
