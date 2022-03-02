
--SCRIPT PURPOSE: Add new AttributeID column to BPAResource, and remove redundant
--                BPAResource.Local field in favour of using the AttributeID column
--
--                Also slip in deletion of BPAResourceUnit (see bug 3566), although unrelated.
--NUMBER: 65
--AUTHOR: PJW
--DATE: 18/08/08


--Create new values in the BPAResourceAttribute table
INSERT INTO BPAResourceAttribute (AttributeID, AttributeName) VALUES (2, 'Local')
INSERT INTO BPAResourceAttribute (AttributeID, AttributeName) VALUES (4, 'Debug')

--Set appropriate attribute value where the 'Local' column is in use
UPDATE BPAResource SET AttributeID=(AttributeID | 2) WHERE [Local]=1

--Do something similar for the new 'Debug' attribute
UPDATE BPAResource SET AttributeID=(AttributeID | 4) WHERE [Name] LIKE '%\_debug' ESCAPE '\'



--Faff about to delete the default constraint on local column
--See http://msdn.microsoft.com/en-us/library/aa175912(SQL.80).aspx
DECLARE @defname VARCHAR(100), @cmd VARCHAR(1000)
SET @defname = 
(
SELECT
    default_constraints.name
FROM 
    sys.all_columns
        INNER JOIN
    sys.tables
        ON all_columns.object_id = tables.object_id
        INNER JOIN
    sys.default_constraints
        ON all_columns.default_object_id = default_constraints.object_id
WHERE 
    tables.name = 'BPAResource'
    AND all_columns.name = 'Local'
)   
SET @cmd = 'ALTER TABLE BPAResource DROP CONSTRAINT ' + @defname
EXEC(@cmd)


--Delete the redundant column, now that the default constraint is gone
ALTER TABLE BPAResource
    DROP COLUMN [Local]
    
    
--Slip in the unrelated change under bug 3566
DROP TABLE BPAResourceUnit
    
    
--set DB version
INSERT INTO BPADBVersion VALUES (
  '65',
  GETUTCDATE(),
  'db_upgradeR65.sql UTC',
  'Database amendments - Add new AttributeID column to BPAResource, and remove redundant BPAResource.Local field in favour of using the AttributeID column. Also deleted BPAResourceUnit.'
)
