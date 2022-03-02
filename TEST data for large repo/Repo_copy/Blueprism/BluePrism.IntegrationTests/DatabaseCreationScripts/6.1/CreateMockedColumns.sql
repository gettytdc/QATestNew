ALTER TABLE [BPAPerm] 
ADD [requiredFeature] NVARCHAR(100) NOT NULL CONSTRAINT BPAPerm_default_requiredFeature DEFAULT ''

ALTER TABLE [BPAPermGroup] 
ADD [requiredFeature] NVARCHAR(100) NOT NULL CONSTRAINT BPAPermGroup_default_requiredFeature DEFAULT ''

ALTER TABLE [BPAUserRole] 
ADD [requiredFeature] NVARCHAR(100) NOT NULL CONSTRAINT BPAUserRole_default_requiredFeature DEFAULT ''
