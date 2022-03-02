-- Add EnvironmentId Column to [BPASysConfig]
IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'EnvironmentId'
          AND Object_ID = Object_ID(N'BPASysConfig'))
BEGIN
    ALTER TABLE [BPASysConfig]
    ADD [EnvironmentId] uniqueidentifier NOT NULL
    CONSTRAINT BPASysConfig_EnvironmentId DEFAULT NEWID()
END
GO

DELETE FROM [BPALicense] WHERE LicenseKey = 'MVcwc2sCCh8MZTFONGK0jp0='
GO

-- Add BPALicenseActivationRequest table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[BPALicenseActivationRequest]') AND type in (N'U'))
BEGIN
CREATE TABLE [BPALicenseActivationRequest](
    [RequestId] int IDENTITY(1,1) NOT NULL,
    [LicenseId] int NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [RequestDateTime] datetime NOT NULL CONSTRAINT BPALicenseActivationRequest_RequestDateTime DEFAULT GETUTCDATE(),
    [Reference] uniqueidentifier NOT NULL CONSTRAINT BPALicenseActivationRequest_Reference DEFAULT NEWID(),
    [Request] varchar(max) NOT NULL,
 CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED 
(
    [RequestId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
 CONSTRAINT [IX_User_Guid] UNIQUE NONCLUSTERED 
(
    [Reference] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_BPALicenseActivationRequest_BPALicense]') AND parent_object_id = OBJECT_ID(N'[BPALicenseActivationRequest]'))
ALTER TABLE [BPALicenseActivationRequest]  WITH CHECK ADD  CONSTRAINT [FK_BPALicenseActivationRequest_BPALicense] FOREIGN KEY([LicenseId])
REFERENCES [BPALicense] ([Id])
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_BPALicenseActivationRequest_BPALicense]') AND parent_object_id = OBJECT_ID(N'[BPALicenseActivationRequest]'))
ALTER TABLE [BPALicenseActivationRequest] CHECK CONSTRAINT [FK_BPALicenseActivationRequest_BPALicense]
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_BPALicenseActivationRequest_BPAUser]') AND parent_object_id = OBJECT_ID(N'[BPALicenseActivationRequest]'))
ALTER TABLE [BPALicenseActivationRequest]  WITH CHECK ADD  CONSTRAINT [FK_BPALicenseActivationRequest_BPAUser] FOREIGN KEY([UserId])
REFERENCES [BPAUser] ([UserId])
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[FK_BPALicenseActivationRequest_BPAUser]') AND parent_object_id = OBJECT_ID(N'[BPALicenseActivationRequest]'))
ALTER TABLE [BPALicenseActivationRequest] CHECK CONSTRAINT [FK_BPALicenseActivationRequest_BPAUser]
GO

IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'licenseactivationresponse'
          AND Object_ID = Object_ID(N'BPALicense'))
BEGIN
    ALTER TABLE BPALicense
    ADD licenseactivationresponse varchar(MAX) NULL
END
GO

-- Set DB version.
insert into BPADBVersion(dbversion,
                         scriptrundate,
                         scriptname, 
                         [description], 
                         timezoneoffset)
values('304',
        getutcdate(),
        'db_upgradeR304.sql',
        'Added License Activation Request functionality.',
        0);
