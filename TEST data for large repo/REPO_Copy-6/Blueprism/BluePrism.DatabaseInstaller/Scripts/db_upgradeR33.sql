
--SCRIPT PURPOSE: New table containing details outlining each test.
--NUMBER: 33
--AUTHOR: DD
--DATE: 10/10/2005 


CREATE TABLE BPAScenarioDetail
    (
    scenarioid uniqueidentifier NOT NULL,
    testnum numeric(18, 0) NOT NULL,
    detailid numeric(18, 0) NOT NULL,
    testtext varchar(1000) NULL
    )
GO
ALTER TABLE BPAScenarioDetail ADD CONSTRAINT
    PK_BPAScenarioDetail PRIMARY KEY CLUSTERED 
    (
    scenarioid,
    testnum,
    detailid
    )

GO
ALTER TABLE BPAScenarioDetail ADD CONSTRAINT
    FK_BPAScenarioDetail_BPAScenario FOREIGN KEY
    (
    scenarioid,
    testnum
    ) REFERENCES BPAScenario
    (
    scenarioid,
    testnum
    )
GO

--set DB version
INSERT INTO BPADBVersion VALUES (
  '33',
  GETUTCDATE(),
  'db_upgradeR33.sql UTC',
  'Database amendments - Add new table BPAScenarioDetail containing details outlining each test.'
)
