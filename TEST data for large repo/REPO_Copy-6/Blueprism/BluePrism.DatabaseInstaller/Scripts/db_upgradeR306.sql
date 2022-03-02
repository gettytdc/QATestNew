UPDATE [BPAPerm]
SET [name] = REPLACE([name], 'Document Processing', 'Decipher')
WHERE [requiredFeature] = 'DocumentProcessing'

UPDATE [BPAPermGroup]
SET [name] = REPLACE([name], 'Document Processing', 'Decipher')
WHERE [requiredFeature] = 'DocumentProcessing'

UPDATE [BPAUserRole]
SET [name] = REPLACE([name], 'Document Processing', 'Decipher')
WHERE [requiredFeature] = 'DocumentProcessing'

UPDATE [BPAWorkQueue]
SET [name] = 'Decipher Queue'
WHERE [name] = 'Document Processing Queue'

-- Set DB version.
insert into BPADBVersion(dbversion, 
                         scriptrundate, 
                         scriptname, 
                         [description], 
                         timezoneoffset)
values('306',
       getutcdate(),
       'db_upgradeR306.sql',
       'Add additional document processing permissions.',
       0);


select * from BPADBVersion order by scriptrundate