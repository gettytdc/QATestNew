
--SCRIPT PURPOSE: Update tables for Resource Pools support
--
--NUMBER: 87


--Create new values in the BPAResourceAttribute table
INSERT INTO BPAResourceAttribute (AttributeID, AttributeName) VALUES (8, 'Pool')

--Add new fields to BPAResource
ALTER TABLE BPAResource ADD pool uniqueidentifier NULL;
ALTER TABLE BPAResource ADD controller uniqueidentifier NULL;
GO

--Create an index for the new field
create index INDEX_BPAResource_pool on BPAResource(pool)
    
--set DB version
INSERT INTO BPADBVersion VALUES (
  '87',
  GETUTCDATE(),
  'db_upgradeR87.sql UTC',
  'Update tables for Resource Pools support'
)
