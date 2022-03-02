IF NOT EXISTS (SELECT 1 
               FROM sys.foreign_keys 
               WHERE object_id = OBJECT_ID(N'[FK_BPMIConfiguredSnapshot_BPMISnapshotTrigger]') 
                    AND parent_object_id = OBJECT_ID(N'[BPMISnapshotTrigger]'))
BEGIN
    ALTER TABLE [BPMISnapshotTrigger]  
    WITH CHECK ADD 
    CONSTRAINT [FK_BPMIConfiguredSnapshot_BPMISnapshotTrigger] 
    FOREIGN KEY([snapshotid])
    REFERENCES [BPMIConfiguredSnapshot] ([snapshotid])
    ON DELETE CASCADE;
END

-- Set DB version.
INSERT INTO BPADBVersion(dbversion, 
                         scriptrundate, 
                         scriptname, 
                         [description], 
                         timezoneoffset)
VALUES('318',
       GETUTCDATE(),
       'db_upgradeR318.sql',
       'Add FK restraint to BPMISnapshotTrigger table to link to BPMIConfiguredSnapshot.',
       0);