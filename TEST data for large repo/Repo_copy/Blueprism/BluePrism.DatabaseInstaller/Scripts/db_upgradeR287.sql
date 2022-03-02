--Truncate the BPAResourceAttribute table and repopulate so it matches the ResourceAttribute enum
IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_NAME = 'BPAResourceAttribute')
BEGIN
     TRUNCATE TABLE [BPAResourceAttribute]
     INSERT INTO [BPAResourceAttribute] (AttributeID, AttributeName) VALUES(0,'None')
     INSERT INTO [BPAResourceAttribute] (AttributeID, AttributeName) VALUES(1,'Retired')
     INSERT INTO [BPAResourceAttribute] (AttributeID, AttributeName) VALUES(2,'Local')
     INSERT INTO [BPAResourceAttribute] (AttributeID, AttributeName) VALUES(4,'Debug')
     INSERT INTO [BPAResourceAttribute] (AttributeID, AttributeName) VALUES(8,'Pool')
     INSERT INTO [BPAResourceAttribute] (AttributeID, AttributeName) VALUES(16,'LoginAgent')
     INSERT INTO [BPAResourceAttribute] (AttributeID, AttributeName) VALUES(32,'Private')
END

--Truncate the BPAProcessAttribute table and repopulate so it matches the ProcessAttribute enum
IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_NAME = 'BPAProcessAttribute')
BEGIN
     TRUNCATE TABLE [BPAProcessAttribute]
     INSERT INTO [BPAProcessAttribute] (AttributeID, AttributeName) VALUES(0,'None')
     INSERT INTO [BPAProcessAttribute] (AttributeID, AttributeName) VALUES(1,'Retired')
     INSERT INTO [BPAProcessAttribute] (AttributeID, AttributeName) VALUES(2,'Published')
     INSERT INTO [BPAProcessAttribute] (AttributeID, AttributeName) VALUES(4,'PublishedWS')
END


-- Set DB version.
INSERT INTO BPADBVersion(dbversion, 
                         scriptrundate, 
                         scriptname, 
                         [description], 
                         timezoneoffset)
VALUES('287',
       getutcdate(),
       'db_upgradeR287.sql',
       'Synchronise the BPAResourceAttribute Table with the ResourceAttribute Enum, and the BPAProcessAttribute Table with the ProcessAttribute Enum.',
       0);
