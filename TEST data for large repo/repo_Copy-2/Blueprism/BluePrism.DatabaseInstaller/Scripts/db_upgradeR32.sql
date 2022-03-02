
--SCRIPT PURPOSE: New table showing who is logged on where. See bug 24.
--NUMBER: 32
--AUTHOR: PJW
--DATE: 29/09/2005 


create table BPAAliveResources (
    ResourceID [uniqueidentifier] NOT NULL ,
    UserID [uniqueidentifier],
    LastUpdated [datetime],
    CONSTRAINT PK_BPAAliveResources PRIMARY KEY CLUSTERED
    (
        [ResourceID]
    )
)
GO


--set DB version
INSERT INTO BPADBVersion VALUES (
  '32',
  GETUTCDATE(),
  'db_upgradeR32.sql UTC',
  'Database amendments - Add new table BPAAliveResources showing who is logged on where to prevent the deletion of logged in users.'
)
